using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class HttpClientCreatorTests
{
    [Test]
    public void GivenUserAgent_WhenCreate_ThenClientContainsUserAgentHeader()
    {
        string userAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:148.0) Gecko/20100101 Firefox/148.0";

        HttpClient client = HttpClientCreator.Create(userAgent);

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(userAgent));
    }

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
}
