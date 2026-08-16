using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
[NonParallelizable]
public class NetworkUtilsTests
{
    private const BindingFlags PrivateStaticBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

    private List<string> originalPingHosts = [];
    private List<string> originalTcpHosts = [];
    private List<string> originalHttpUrls = [];
    private List<string> originalPublicIpSources = [];
    private Func<string, int, CancellationToken, Task<IPStatus>> originalPingProbeAsync = null!;
    private Func<string, int, CancellationToken, Task<bool>> originalTcpProbeAsync = null!;
    private Func<string, CancellationToken, Task<HttpStatusCode>> originalHttpProbeAsync = null!;

    [SetUp]
    public void SetUp()
    {
        originalPingHosts = [.. GetMutableStringList("PingHosts")];
        originalTcpHosts = [.. GetMutableStringList("TcpHosts")];
        originalHttpUrls = [.. GetMutableStringList("HttpUrls")];
        originalPublicIpSources = [.. GetMutableStringList("PublicIpSources")];
        originalPingProbeAsync = GetProbeDelegate<Func<string, int, CancellationToken, Task<IPStatus>>>("pingProbeAsync");
        originalTcpProbeAsync = GetProbeDelegate<Func<string, int, CancellationToken, Task<bool>>>("tcpProbeAsync");
        originalHttpProbeAsync = GetProbeDelegate<Func<string, CancellationToken, Task<HttpStatusCode>>>("httpProbeAsync");

        ClearNetworkUtilsCache();
    }

    [TearDown]
    public void TearDown()
    {
        RestoreMutableStringList("PingHosts", originalPingHosts);
        RestoreMutableStringList("TcpHosts", originalTcpHosts);
        RestoreMutableStringList("HttpUrls", originalHttpUrls);
        RestoreMutableStringList("PublicIpSources", originalPublicIpSources);
        SetProbeDelegate("pingProbeAsync", originalPingProbeAsync);
        SetProbeDelegate("tcpProbeAsync", originalTcpProbeAsync);
        SetProbeDelegate("httpProbeAsync", originalHttpProbeAsync);

        ClearNetworkUtilsCache();
    }

    [Test]
    public void GivenNullIpAddress_WhenGettingHostnames_ThenThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => NetworkUtils.GetHostnames((IPAddress)null!))!;

        Assert.That(exception.ParamName, Is.EqualTo("ipAddress"));
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("not-an-ip")]
    [TestCase("example.com")]
    [TestCase("256.1.1.1")]
    [TestCase("203.0.113.5 extra")]
    [TestCase("<ip>203.0.113.5</ip>")]
    [TestCase("1.2.3.4:443")]
    public void GivenInvalidIpAddressString_WhenGettingHostnames_ThenThrowsArgumentException(string ipAddress)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => NetworkUtils.GetHostnames(ipAddress))!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.ParamName, Is.EqualTo("ipAddress"));
            Assert.That(exception.Message, Does.Contain("not a valid IP address"));
        });
    }

    [Test]
    [TestCase("192.0.2.123")]
    [TestCase("203.0.113.255")]
    [TestCase("198.51.100.17")]
    public void GivenNonResolvableIpAddress_WhenGettingHostnames_ThenReturnsEmptyCollection(string ipAddress)
    {
        List<string> hostnames = NetworkUtils.GetHostnames(IPAddress.Parse(ipAddress));

        Assert.That(hostnames, Is.Empty);
    }

    [Test]
    [TestCase("127.0.0.1")]
    [TestCase("8.8.8.8")]
    [TestCase("1.1.1.1")]
    public void GivenEquivalentIpInputs_WhenGettingHostnames_ThenReturnsEquivalentResults(string ipAddress)
    {
        IPAddress parsedIpAddress = IPAddress.Parse(ipAddress);

        List<string> hostnamesFromIp = NetworkUtils.GetHostnames(parsedIpAddress);
        List<string> hostnamesFromString = NetworkUtils.GetHostnames(ipAddress);

        Assert.That(hostnamesFromString, Is.EquivalentTo(hostnamesFromIp));
    }

    [Test]
    public void GivenLoopbackIpAddress_WhenGettingHostnames_ThenMatchesDnsTransformation()
    {
        IPAddress loopbackIpAddress = IPAddress.Loopback;

        IPHostEntry hostEntry = Dns.GetHostEntry(loopbackIpAddress);
        List<string> expectedHostnames =
        [
            .. new[] { hostEntry.HostName }
                .Concat(hostEntry.Aliases)
                .Where(hostname => !string.IsNullOrWhiteSpace(hostname))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];

        List<string> actualHostnames = NetworkUtils.GetHostnames(loopbackIpAddress);

        Assert.That(actualHostnames, Is.EqualTo(expectedHostnames));
    }

    [Test]
    public void GivenMutableReturnedCollection_WhenMutatingIt_ThenCacheIsNotMutated()
    {
        IPAddress loopbackIpAddress = IPAddress.Loopback;
        List<string> firstHostnames = NetworkUtils.GetHostnames(loopbackIpAddress);

        firstHostnames.Add("injected-hostname");

        List<string> secondHostnames = NetworkUtils.GetHostnames(loopbackIpAddress);

        Assert.That(secondHostnames, Is.Not.Contain("injected-hostname"));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-10)]
    public void GivenNonPositiveTimeout_WhenWaitingForInternetAccess_ThenThrowsTimeoutException(int timeoutMilliseconds)
        => Assert.Throws<TimeoutException>(
            () => NetworkUtils.WaitForInternetAccess(TimeSpan.FromMilliseconds(timeoutMilliseconds)));

    [Test]
    public void GivenUnavailableConnectivity_WhenWaitingForInternetAccess_ThenSleepsAndTimesOut()
    {
        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", []);

        Assert.Throws<TimeoutException>(() => NetworkUtils.WaitForInternetAccess(TimeSpan.FromMilliseconds(1200)));
    }

    [Test]
    public void GivenLocalHttpAvailability_WhenWaitingForInternetAccess_ThenReturnsBeforeTimeout()
    {
        using HttpProbeServer probeServer = new(
            request => new ProbeResponse(HttpStatusCode.OK, request.HttpMethod));

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        Assert.DoesNotThrow(() => NetworkUtils.WaitForInternetAccess(TimeSpan.FromSeconds(2)));
    }

    [Test]
    public void GivenLocalHttpAvailability_WhenWaitingForInternetAccessWithDefaultTimeout_ThenReturns()
    {
        using HttpProbeServer probeServer = new(
            request => new ProbeResponse(HttpStatusCode.OK, request.HttpMethod));

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        Assert.DoesNotThrow(() => NetworkUtils.WaitForInternetAccess());
    }

    [Test]
    public async Task GivenCancelledToken_WhenTryingPing_ThenReturnsFalse()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenCancelledToken_WhenTryingTcp_ThenReturnsFalse()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenCancelledToken_WhenTryingHttp_ThenReturnsFalse()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenLoopbackPingHost_WhenTryingPing_ThenReturnsBooleanResult()
    {
        RestoreMutableStringList("PingHosts", ["127.0.0.1"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", CancellationToken.None);

        Assert.That(wasSuccessful || !wasSuccessful);
    }

    [Test]
    public async Task GivenFailingHttpEndpoint_WhenTryingHttp_ThenReturnsFalse()
    {
        using HttpProbeServer probeServer = new(
            _ => new ProbeResponse(HttpStatusCode.ServiceUnavailable, string.Empty));

        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenInformationalHttpEndpoint_WhenTryingHttp_ThenReturnsFalse()
    {
        RestoreMutableStringList("HttpUrls", ["http://unused-host"]);
        SetProbeDelegate(
            "httpProbeAsync",
            (Func<string, CancellationToken, Task<HttpStatusCode>>)((url, cancellationToken) =>
                Task.FromResult(HttpStatusCode.Continue)));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenSuccessfulHttpEndpoint_WhenTryingHttp_ThenReturnsTrue()
    {
        using HttpProbeServer probeServer = new(
            _ => new ProbeResponse(HttpStatusCode.NoContent, string.Empty));

        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", CancellationToken.None);

        Assert.That(wasSuccessful);
    }

    [Test]
    public async Task GivenNoAvailableStrategies_WhenCheckingInternetAccessAsync_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", []);

        bool hasInternetAccess = await NetworkUtils.HasInternetAccessAsync();

        Assert.That(hasInternetAccess, Is.False);
    }

    [Test]
    public async Task GivenLocalHttpStrategyAvailable_WhenCheckingInternetAccessAsync_ThenReturnsTrue()
    {
        using HttpProbeServer probeServer = new(
            _ => new ProbeResponse(HttpStatusCode.OK, string.Empty));

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        bool hasInternetAccess = await NetworkUtils.HasInternetAccessAsync();

        Assert.That(hasInternetAccess);
    }

    [Test]
    public void GivenNoConnectivityStrategies_WhenCheckingInternetAccess_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", []);

        bool hasInternetAccess = NetworkUtils.HasInternetAccess();

        Assert.That(hasInternetAccess, Is.False);
    }

    [Test]
    public void GivenCreateHttpClient_WhenInvoked_ThenClientHasExpectedDefaults()
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("CreateHttpClient", PrivateStaticBindingFlags)!;

        HttpClient client = (HttpClient)method.Invoke(null, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(client.Timeout, Is.EqualTo(Timeout.InfiniteTimeSpan));
            Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo("InternetAccessCheck/1.0"));
        });
    }

    [Test]
    [TestCase("203.0.113.7", "203.0.113.7")]
    [TestCase(" 203.0.113.7 ", "203.0.113.7")]
    [TestCase("203.0.113.7\r\n", "203.0.113.7")]
    public void GivenValidPublicIpResponse_WhenNormalising_ThenReturnsTrue(string response, string expectedIpAddress)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("TryNormalizePublicIpAddress", PrivateStaticBindingFlags)!;
        object?[] parameters = [response, null];

        bool isValid = (bool)method.Invoke(null, parameters)!;

        Assert.Multiple(() =>
        {
            Assert.That(isValid);
            Assert.That(parameters[1], Is.EqualTo(expectedIpAddress));
        });
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not-an-ip")]
    [TestCase("2001:db8::1")]
    [TestCase("203.0.113.5 extra")]
    [TestCase("<html>203.0.113.5</html>")]
    [TestCase("127.0.0.1:443")]
    public void GivenInvalidPublicIpResponse_WhenNormalising_ThenReturnsFalse(string response)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("TryNormalizePublicIpAddress", PrivateStaticBindingFlags)!;
        object?[] parameters = [response, null];

        bool isValid = (bool)method.Invoke(null, parameters)!;

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(parameters[1], Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void GivenUnavailableConnectivity_WhenGettingPublicIpAddress_ThenThrowsInvalidOperationException()
    {
        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", []);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => NetworkUtils.GetPublicIpAddress())!;

        Assert.That(exception.Message, Does.Contain("No internet access"));
    }

    [Test]
    public void GivenPublicIpSourceResponses_WhenGettingPublicIpAddress_ThenSkipsInvalidAndReturnsFirstValidValue()
    {
        using HttpProbeServer probeServer = new(
            request => request.Url?.AbsolutePath switch
            {
                "/invalid" => new ProbeResponse(HttpStatusCode.OK, "not-an-ip"),
                "/valid" => new ProbeResponse(HttpStatusCode.OK, "198.51.100.77\n"),
                _ => new ProbeResponse(HttpStatusCode.NotFound, string.Empty),
            });

        string invalidSource = $"{probeServer.RootUrl}invalid";
        string validSource = $"{probeServer.RootUrl}valid";

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);
        RestoreMutableStringList("PublicIpSources", [invalidSource, validSource]);

        string publicIpAddress = NetworkUtils.GetPublicIpAddress();

        Assert.That(publicIpAddress, Is.EqualTo("198.51.100.77"));
    }

    [Test]
    public void GivenCachedPublicIpAddress_WhenSourceBecomesInvalid_ThenReturnsCachedValue()
    {
        using HttpProbeServer probeServer = new(
            request => request.Url?.AbsolutePath switch
            {
                "/valid" => new ProbeResponse(HttpStatusCode.OK, "203.0.113.17\n"),
                "/invalid" => new ProbeResponse(HttpStatusCode.OK, "<invalid>"),
                _ => new ProbeResponse(HttpStatusCode.NotFound, string.Empty),
            });

        string validSource = $"{probeServer.RootUrl}valid";
        string invalidSource = $"{probeServer.RootUrl}invalid";

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        RestoreMutableStringList("PublicIpSources", [validSource]);
        string firstPublicIpAddress = NetworkUtils.GetPublicIpAddress();

        RestoreMutableStringList("PublicIpSources", [invalidSource]);
        string secondPublicIpAddress = NetworkUtils.GetPublicIpAddress();

        Assert.Multiple(() =>
        {
            Assert.That(firstPublicIpAddress, Is.EqualTo("203.0.113.17"));
            Assert.That(secondPublicIpAddress, Is.EqualTo("203.0.113.17"));
        });
    }

    [Test]
    public void GivenOnlyInvalidPublicIpResponses_WhenGettingPublicIpAddress_ThenThrowsAggregateInvalidOperationException()
    {
        using HttpProbeServer probeServer = new(
            _ => new ProbeResponse(HttpStatusCode.OK, "not-an-ip"));

        string invalidSource = $"{probeServer.RootUrl}invalid";

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);
        RestoreMutableStringList("PublicIpSources", [invalidSource]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => NetworkUtils.GetPublicIpAddress())!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Does.Contain("Unable to retrieve the public IP address"));
            Assert.That(exception.InnerException, Is.TypeOf<AggregateException>());
        });
    }

    [Test]
    public void GivenExceptionPublicIpSource_WhenGettingPublicIpAddress_ThenCollectsErrorsAndThrows()
    {
        using HttpProbeServer probeServer = new(
            _ => new ProbeResponse(HttpStatusCode.OK, string.Empty));

        RestoreMutableStringList("PingHosts", []);
        RestoreMutableStringList("TcpHosts", []);
        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);
        RestoreMutableStringList("PublicIpSources", ["http://127.0.0.1:1", "http://127.0.0.1:2"]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => NetworkUtils.GetPublicIpAddress())!;
        AggregateException aggregateException = (AggregateException)exception.InnerException!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Does.Contain("Unable to retrieve the public IP address"));
            Assert.That(aggregateException.InnerExceptions.Count, Is.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task GivenInvalidPingHost_WhenTryingPing_ThenReturnsFalseAfterCatchingProbeException()
    {
        RestoreMutableStringList("PingHosts", ["invalid host with spaces"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenCancellationDuringPingWait_WhenTryingPing_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", ["203.0.113.200"]);

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(10));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenUnreachablePingIpAddress_WhenTryingPing_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", ["203.0.113.254"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenProbeThrowsOperationCancelled_WhenTryingPing_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", ["unused-host"]);
        SetProbeDelegate(
            "pingProbeAsync",
            async (string host, int timeoutMilliseconds, CancellationToken cancellationToken) =>
            {
                await Task.Delay(1, cancellationToken);
                return IPStatus.Unknown;
            });

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(1));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenProbeThrowsGenericException_WhenTryingPing_ThenReturnsFalse()
    {
        RestoreMutableStringList("PingHosts", ["unused-host"]);
        SetProbeDelegate(
            "pingProbeAsync",
            (Func<string, int, CancellationToken, Task<IPStatus>>)((host, timeoutMilliseconds, cancellationToken) =>
                throw new SocketException()));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryPingAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenTcpHostRefusingConnections_WhenTryingTcp_ThenReturnsFalse()
    {
        RestoreMutableStringList("TcpHosts", ["127.0.0.1"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenKnownInternetTcpHost_WhenTryingTcp_ThenReturnsBooleanResult()
    {
        RestoreMutableStringList("TcpHosts", ["cloudflare.com", "github.com", "google.com"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", CancellationToken.None);

        Assert.That(wasSuccessful);
    }

    [Test]
    public async Task GivenCancellationDuringTcpConnect_WhenTryingTcp_ThenReturnsFalse()
    {
        RestoreMutableStringList("TcpHosts", ["203.0.113.201"]);

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(20));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenTcpProbeReturningFalse_WhenTryingTcp_ThenReturnsFalse()
    {
        RestoreMutableStringList("TcpHosts", ["unused-host"]);
        SetProbeDelegate(
            "tcpProbeAsync",
            (Func<string, int, CancellationToken, Task<bool>>)((host, port, cancellationToken) =>
                Task.FromResult(false)));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenTcpProbeThrowingOperationCancelled_WhenTryingTcp_ThenReturnsFalse()
    {
        RestoreMutableStringList("TcpHosts", ["unused-host"]);
        SetProbeDelegate(
            "tcpProbeAsync",
            (Func<string, int, CancellationToken, Task<bool>>)((host, port, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw new OperationCanceledException(cancellationToken);
            }));

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryTcpAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenHttpEndpointRefusingConnection_WhenTryingHttp_ThenReturnsFalseAfterCatch()
    {
        RestoreMutableStringList("HttpUrls", ["http://127.0.0.1:1"]);

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", CancellationToken.None);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public async Task GivenCancellationDuringHttpRequest_WhenTryingHttp_ThenReturnsFalse()
    {
        using HttpProbeServer probeServer = new(
            request =>
            {
                Thread.Sleep(250);
                return new ProbeResponse(HttpStatusCode.OK, request.HttpMethod);
            });

        RestoreMutableStringList("HttpUrls", [probeServer.RootUrl]);

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(20));

        bool wasSuccessful = await InvokePrivateNetworkCheckAsync("TryHttpAsync", cancellationTokenSource.Token);

        Assert.That(wasSuccessful, Is.False);
    }

    [Test]
    public void GivenExpiredCacheEntry_WhenCreatingCachedValue_ThenFactoryIsInvokedAgain()
    {
        MethodInfo method = typeof(NetworkUtils)
            .GetMethod("GetOrCreateCachedValue", PrivateStaticBindingFlags)!
            .MakeGenericMethod(typeof(string));

        string cacheKey = $"test-cache-key-{Guid.NewGuid():N}";
        int invocationCount = 0;

        Func<string> firstFactory = () =>
        {
            invocationCount += 1;
            return "first";
        };

        Func<string> secondFactory = () =>
        {
            invocationCount += 1;
            return "second";
        };

        string firstValue = (string)method.Invoke(
            null,
            [cacheKey, TimeSpan.FromMilliseconds(1), firstFactory])!;

        Thread.Sleep(15);

        string secondValue = (string)method.Invoke(
            null,
            [cacheKey, TimeSpan.FromMilliseconds(1), secondFactory])!;

        Assert.Multiple(() =>
        {
            Assert.That(firstValue, Is.EqualTo("first"));
            Assert.That(secondValue, Is.EqualTo("second"));
            Assert.That(invocationCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void GivenNullFactoryResult_WhenCreatingCachedValue_ThenResultIsNotCached()
    {
        MethodInfo method = typeof(NetworkUtils)
            .GetMethod("GetOrCreateCachedValue", PrivateStaticBindingFlags)!
            .MakeGenericMethod(typeof(object));

        string cacheKey = $"null-cache-key-{Guid.NewGuid():N}";
        int invocationCount = 0;

        Func<object?> valueFactory = () =>
        {
            invocationCount += 1;
            return null;
        };

        object? firstValue = method.Invoke(null, [cacheKey, TimeSpan.FromMinutes(1), valueFactory]);
        object? secondValue = method.Invoke(null, [cacheKey, TimeSpan.FromMinutes(1), valueFactory]);

        Assert.Multiple(() =>
        {
            Assert.That(firstValue, Is.Null);
            Assert.That(secondValue, Is.Null);
            Assert.That(invocationCount, Is.EqualTo(2));
        });
    }

    private static async Task<bool> InvokePrivateNetworkCheckAsync(string methodName, CancellationToken cancellationToken)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod(methodName, PrivateStaticBindingFlags)!;
        Task<bool> task = (Task<bool>)method.Invoke(null, [cancellationToken])!;

        return await task;
    }

    private static List<string> GetMutableStringList(string fieldName)
    {
        FieldInfo field = typeof(NetworkUtils).GetField(fieldName, PrivateStaticBindingFlags)!;

        return (List<string>)field.GetValue(null)!;
    }

    private static void RestoreMutableStringList(string fieldName, IEnumerable<string> values)
    {
        List<string> list = GetMutableStringList(fieldName);
        list.Clear();

        foreach (string value in values)
        {
            list.Add(value);
        }
    }

    private static void ClearNetworkUtilsCache()
    {
        FieldInfo field = typeof(NetworkUtils).GetField("Cache", PrivateStaticBindingFlags)!;
        object cacheInstance = field.GetValue(null)!;
        MethodInfo clearMethod = cacheInstance.GetType().GetMethod("Clear")!;
        clearMethod.Invoke(cacheInstance, null);
    }

    private static TDelegate GetProbeDelegate<TDelegate>(string fieldName)
        where TDelegate : Delegate
    {
        FieldInfo fieldInfo = typeof(NetworkUtils).GetField(fieldName, PrivateStaticBindingFlags)!;

        return (TDelegate)fieldInfo.GetValue(null)!;
    }

    private static void SetProbeDelegate(string fieldName, Delegate probeDelegate)
    {
        FieldInfo fieldInfo = typeof(NetworkUtils).GetField(fieldName, PrivateStaticBindingFlags)!;
        fieldInfo.SetValue(null, probeDelegate);
    }

    private sealed class HttpProbeServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Func<HttpListenerRequest, ProbeResponse> responseFactory;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task processingTask;

        public string RootUrl { get; }

        public HttpProbeServer(Func<HttpListenerRequest, ProbeResponse> responseFactory)
        {
            this.responseFactory = responseFactory;
            listener = new HttpListener();

            int port = GetAvailablePort();
            RootUrl = $"http://127.0.0.1:{port}/";

            listener.Prefixes.Add(RootUrl);
            listener.Start();

            cancellationTokenSource = new CancellationTokenSource();
            processingTask = Task.Run(() => ProcessRequestsAsync(cancellationTokenSource.Token));
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();

            try
            {
                listener.Stop();
                listener.Close();
                processingTask.GetAwaiter().GetResult();
            }
            catch
            {
                // Intentionally ignored in test infrastructure cleanup.
            }

            cancellationTokenSource.Dispose();
        }

        private async Task ProcessRequestsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext listenerContext;

                try
                {
                    listenerContext = await listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                ProbeResponse response = responseFactory(listenerContext.Request);

                listenerContext.Response.StatusCode = (int)response.StatusCode;

                if (!string.IsNullOrEmpty(response.Body))
                {
                    byte[] payload = System.Text.Encoding.UTF8.GetBytes(response.Body);
                    listenerContext.Response.ContentLength64 = payload.Length;

                    await listenerContext.Response.OutputStream.WriteAsync(payload, cancellationToken);
                }
                else
                {
                    listenerContext.Response.ContentLength64 = 0;
                }

                listenerContext.Response.OutputStream.Close();
                listenerContext.Response.Close();
            }
        }

        private static int GetAvailablePort()
        {
            TcpListener tcpListener = new(IPAddress.Loopback, 0);
            tcpListener.Start();

            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            return port;
        }
    }

    private sealed record ProbeResponse(HttpStatusCode StatusCode, string Body);
}
