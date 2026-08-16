using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class HttpClientCreatorTests
{
    private Func<Task<string>> originalFetchHtmlAsync = null!;

    private static string DefaultUserAgent =>
        "Mozilla/5.0 (X11; Linux x86_64; rv:148.0) Gecko/20100101 Firefox/148.0";

    [SetUp]
    public void SetUp()
        => originalFetchHtmlAsync = GetFetchHtmlAsync();

    [TearDown]
    public void TearDown()
        => SetFetchHtmlAsync(originalFetchHtmlAsync);

    [Test]
    public void GivenUserAgent_WhenCreate_ThenClientContainsUserAgentHeader()
    {
        string userAgent = DefaultUserAgent;

        HttpClient client = HttpClientCreator.Create(userAgent);

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(userAgent));
    }

    [Test]
    public void GivenMissingUserAgentValues_WhenCreatingClient_ThenThrowsFormatExceptionForEachValue()
    {
        string?[] missingUserAgentValues = [null, string.Empty, "   ", "\t\r\n"];

        foreach (string? userAgent in missingUserAgentValues)
        {
            Assert.Throws<FormatException>(() => HttpClientCreator.Create(userAgent!));
        }
    }

    [Test]
    public void GivenInvalidUserAgentValues_WhenCreatingClient_ThenThrowsFormatExceptionForEachValue()
    {
        string[] invalidUserAgentValues = ["(", "UserAgent\r\nInjected: value", "Mozilla/5.0\u0000"];

        foreach (string userAgent in invalidUserAgentValues)
        {
            Assert.Throws<FormatException>(() => HttpClientCreator.Create(userAgent));
        }
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

    [Test]
    public void GivenNullUserAgentFetcher_WhenCreateAsync_ThenThrowsNullReferenceException()
        => Assert.ThrowsAsync<System.NullReferenceException>(
            async () => await HttpClientCreator.CreateAsync((IUserAgentFetcher)null!));

    [Test]
    public async Task GivenDefaultCreateAsync_WhenFetcherReturnsValidUserAgent_ThenCreatesClient()
    {
        string expectedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:152.0) Gecko/20100101 Firefox/152.0";
        SetFetchHtmlAsync(() => Task.FromResult($"<html><body>{expectedUserAgent}</body></html>"));

        HttpClient client = await HttpClientCreator.CreateAsync();

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(expectedUserAgent));
    }

    [Test]
    public void GivenDefaultCreateAsync_WhenFetcherThrows_ThenPropagatesException()
    {
        SetFetchHtmlAsync(() => throw new InvalidOperationException("forced async failure"));

        InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await HttpClientCreator.CreateAsync())!;

        Assert.That(exception.Message, Is.EqualTo("forced async failure"));
    }

    [Test]
    public void GivenDefaultCreate_WhenFetcherReturnsValidUserAgent_ThenCreatesClient()
    {
        string expectedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:153.0) Gecko/20100101 Firefox/153.0";
        SetFetchHtmlAsync(() => Task.FromResult($"<html><body>{expectedUserAgent}</body></html>"));

        HttpClient client = HttpClientCreator.CreateAsync().Result;

        Assert.That(client.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo(expectedUserAgent));
    }

    [Test]
    public void GivenDefaultCreate_WhenFetcherThrows_ThenThrowsException()
    {
        SetFetchHtmlAsync(() => throw new InvalidOperationException("forced sync failure"));

        Exception thrown = Assert.Throws<AggregateException>(() =>
        {
            _ = HttpClientCreator.CreateAsync().Result;
        })!;

        Assert.That(thrown.InnerException, Is.TypeOf<InvalidOperationException>());
    }

    private static Func<Task<string>> GetFetchHtmlAsync()
    {
        FieldInfo fieldInfo = typeof(UserAgentFetcher).GetField("FetchHtmlAsync", BindingFlags.NonPublic | BindingFlags.Static)!;

        return (Func<Task<string>>)fieldInfo.GetValue(null)!;
    }

    private static void SetFetchHtmlAsync(Func<Task<string>> fetchHtmlAsync)
    {
        FieldInfo fieldInfo = typeof(UserAgentFetcher).GetField("FetchHtmlAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
        fieldInfo.SetValue(null, fetchHtmlAsync);
    }
}
