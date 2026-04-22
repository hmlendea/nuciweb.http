using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class NetworkUtilsTests
{
    [Test]
    public void GivenNullIpAddress_WhenGetHostnames_ThenThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => NetworkUtils.GetHostnames((IPAddress)null!))!;

        Assert.That(exception.ParamName, Is.EqualTo("ipAddress"));
    }

    [Test]
    [TestCase("192.0.2.123")]
    public void GivenNonResolvableIpAddress_WhenGetHostnames_ThenReturnsEmptyCollection(string ipAddress)
    {
        List<string> hostnames = NetworkUtils.GetHostnames(IPAddress.Parse(ipAddress));

        Assert.That(hostnames, Is.Empty);
    }

    [Test]
    public void GivenInvalidIpAddressString_WhenGetHostnames_ThenThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => NetworkUtils.GetHostnames("not-an-ip"))!;

        Assert.Multiple(() =>
        {
            Assert.That(exception.ParamName, Is.EqualTo("ipAddress"));
            Assert.That(exception.Message, Does.Contain("not a valid IP address"));
        });
    }

    [Test]
    public void GivenEquivalentIpInputs_WhenGetHostnames_ThenReturnsSameResults()
    {
        IPAddress loopback = IPAddress.Loopback;

        List<string> hostnamesFromIp = NetworkUtils.GetHostnames(loopback);
        List<string> hostnamesFromString = NetworkUtils.GetHostnames(loopback.ToString());

        Assert.That(hostnamesFromString, Is.EquivalentTo(hostnamesFromIp));
    }

    [Test]
    public void GivenLoopbackIp_WhenGetHostnames_ThenMatchesDnsTransformation()
    {
        IPAddress loopback = IPAddress.Loopback;

        IPHostEntry hostEntry = Dns.GetHostEntry(loopback);
        List<string> expected =
        [
            .. new[] { hostEntry.HostName }
                .Concat(hostEntry.Aliases)
                .Where(hostname => !string.IsNullOrWhiteSpace(hostname))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        ];

        List<string> actual = NetworkUtils.GetHostnames(loopback);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GivenZeroTimeout_WhenWaitForInternetAccess_ThenThrowsTimeoutException()
        => Assert.Throws<TimeoutException>(() => NetworkUtils.WaitForInternetAccess(TimeSpan.Zero));

    [Test]
    public async Task GivenCancelledToken_WhenTryPingAsync_ThenReturnsFalse()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        bool result = await InvokePrivateNetworkCheckAsync("TryPingAsync", cts.Token);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GivenCancelledToken_WhenTryTcpAsync_ThenReturnsFalse()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        bool result = await InvokePrivateNetworkCheckAsync("TryTcpAsync", cts.Token);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GivenCancelledToken_WhenTryHttpAsync_ThenReturnsFalse()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        bool result = await InvokePrivateNetworkCheckAsync("TryHttpAsync", cts.Token);

        Assert.That(result, Is.False);
    }

    [Test]
    public void GivenCreateHttpClient_WhenInvoked_ThenClientHasExpectedDefaults()
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("CreateHttpClient", BindingFlags.NonPublic | BindingFlags.Static)!;

        HttpClient client = (HttpClient)method.Invoke(null, null)!;

        Assert.Multiple(() =>
        {
            Assert.That(client.Timeout, Is.EqualTo(Timeout.InfiniteTimeSpan));
            Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo("InternetAccessCheck/1.0"));
        });
    }

    [Test]
    [TestCase("203.0.113.7", "203.0.113.7")]
    public void GivenValidIpResponse_WhenTryNormalizePublicIpAddress_ThenReturnsTrue(string response, string expectedIp)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("TryNormalizePublicIpAddress", BindingFlags.NonPublic | BindingFlags.Static)!;
        object?[] parameters = [response, null];

        bool isValid = (bool)method.Invoke(null, parameters)!;

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.True);
            Assert.That(parameters[1], Is.EqualTo(expectedIp));
        });
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not-an-ip")]
    [TestCase("2001:db8::1")]
    [TestCase("203.0.113.5 extra")]
    [TestCase("<html>203.0.113.5</html>")]
    public void GivenInvalidIpResponse_WhenTryNormalizePublicIpAddress_ThenReturnsFalse(string response)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod("TryNormalizePublicIpAddress", BindingFlags.NonPublic | BindingFlags.Static)!;
        object?[] parameters = [response, null];

        bool isValid = (bool)method.Invoke(null, parameters)!;

        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(parameters[1], Is.EqualTo(string.Empty));
        });
    }

    private static async Task<bool> InvokePrivateNetworkCheckAsync(string methodName, CancellationToken cancellationToken)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
        Task<bool> task = (Task<bool>)method.Invoke(null, [cancellationToken])!;

        return await task;
    }
}
