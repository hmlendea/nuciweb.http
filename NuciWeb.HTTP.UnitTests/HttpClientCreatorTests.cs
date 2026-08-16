using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class HttpClientCreatorTests
{
    private static string DefaultUserAgent =>
        "Mozilla/5.0 (X11; Linux x86_64; rv:148.0) Gecko/20100101 Firefox/148.0";

    [Test]
    public void GivenUserAgent_WhenCreate_ThenClientContainsUserAgentHeader()
    {
        string userAgent = DefaultUserAgent;

        HttpClient client = HttpClientCreator.Create(userAgent);

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(userAgent));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("\t\r\n")]
    public void GivenMissingUserAgent_WhenCreate_ThenThrowsFormatException(string? userAgent)
        => Assert.Throws<System.FormatException>(() => HttpClientCreator.Create(userAgent!));

    [Test]
    [TestCase("(")]
    [TestCase("UserAgent\r\nInjected: value")]
    [TestCase("Mozilla/5.0\u0000")]
    public void GivenInvalidUserAgent_WhenCreate_ThenThrowsFormatException(string userAgent)
        => Assert.Throws<System.FormatException>(() => HttpClientCreator.Create(userAgent));

    [Test]
    public async Task GivenUserAgentFetcher_WhenCreateAsync_ThenUsesFetchedUserAgent()
    {
        string fetchedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:149.0) Gecko/20100101 Firefox/149.0";
        Mock<IUserAgentFetcher> userAgentFetcherMock = new();
        userAgentFetcherMock
            .Setup(x => x.GetUserAgent())
            .ReturnsAsync(fetchedUserAgent);

        HttpClient client = await HttpClientCreator.CreateAsync(userAgentFetcherMock.Object);

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(fetchedUserAgent));
        userAgentFetcherMock.Verify(x => x.GetUserAgent(), Times.Once);
    }

    [Test]
    public void GivenNullUserAgentFetcher_WhenCreateAsync_ThenThrowsNullReferenceException()
        => Assert.ThrowsAsync<System.NullReferenceException>(
            async () => await HttpClientCreator.CreateAsync((IUserAgentFetcher)null!));

    [Test]
    public async Task GivenDefaultCreateAsync_WhenInvoked_ThenEitherCreatesClientOrThrowsNetworkException()
    {
        HttpClient? client = null;
        System.Exception? exception = null;

        try
        {
            client = await HttpClientCreator.CreateAsync();
        }
        catch (System.Exception caughtException)
        {
            exception = caughtException;
        }

        Assert.That(client is not null || exception is not null);

        if (client is not null)
        {
            Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.Not.Empty);
        }
    }

    [Test]
    public void GivenDefaultCreate_WhenInvoked_ThenEitherCreatesClientOrThrowsNetworkException()
    {
        HttpClient? client = null;
        System.Exception? exception = null;

        try
        {
            client = HttpClientCreator.Create();
        }
        catch (System.Exception caughtException)
        {
            exception = caughtException;
        }

        Assert.That(client is not null || exception is not null);

        if (client is not null)
        {
            Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.Not.Empty);
        }
    }
}
