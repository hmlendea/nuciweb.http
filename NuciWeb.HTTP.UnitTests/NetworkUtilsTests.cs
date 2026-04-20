using System;
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
        Task<bool> task = (Task<bool>)method.Invoke(null, new object[] { cancellationToken })!;

        return await task;
    }
}
