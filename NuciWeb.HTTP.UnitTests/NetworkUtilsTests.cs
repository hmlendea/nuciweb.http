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

    private static async Task<bool> InvokePrivateNetworkCheckAsync(string methodName, CancellationToken cancellationToken)
    {
        MethodInfo method = typeof(NetworkUtils).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
        Task<bool> task = (Task<bool>)method.Invoke(null, [cancellationToken])!;

        return await task;
    }
}
