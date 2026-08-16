using System;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class UserAgentFetcherTests
{
    private Func<Task<string>> originalFetchHtmlAsync = null!;

    [SetUp]
    public void SetUp()
        => originalFetchHtmlAsync = GetFetchHtmlAsync();

    [TearDown]
    public void TearDown()
        => SetFetchHtmlAsync(originalFetchHtmlAsync);

    [Test]
    public void GivenUserAgentFetcher_WhenInspectingType_ThenImplementsIUserAgentFetcher()
        => Assert.That(typeof(IUserAgentFetcher).IsAssignableFrom(typeof(UserAgentFetcher)), Is.True);

    [Test]
    public async Task GivenCachedUserAgent_WhenGetUserAgent_ThenReturnsCachedValue()
    {
        string cachedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:150.0) Gecko/20100101 Firefox/150.0";
        UserAgentFetcher fetcher = new();
        FieldInfo cachedValueField = typeof(UserAgentFetcher).GetField("cachedValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        cachedValueField.SetValue(fetcher, cachedUserAgent);

        string userAgent = await fetcher.GetUserAgent();

        Assert.That(userAgent, Is.EqualTo(cachedUserAgent));
    }

    [Test]
    [TestCase("Mozilla/5.0 (X11; Linux x86_64; rv:151.0) Gecko/20100101 Firefox/151.0")]
    [TestCase("CustomBrowser/1.0")]
    [TestCase("edge-case/with;symbols(1)")]
    public async Task GivenDifferentCachedValues_WhenGettingUserAgent_ThenReturnsExactlyCachedValue(string cachedUserAgent)
    {
        UserAgentFetcher fetcher = new();
        FieldInfo cachedValueField = typeof(UserAgentFetcher).GetField("cachedValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        cachedValueField.SetValue(fetcher, cachedUserAgent);

        string userAgent = await fetcher.GetUserAgent();

        Assert.That(userAgent, Is.EqualTo(cachedUserAgent));
    }

    [Test]
    public async Task GivenWhitespaceCachedValue_WhenGettingUserAgent_ThenFetchesAndParsesReturnedHtml()
    {
        string expectedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:151.0) Gecko/20100101 Firefox/151.0";

        SetFetchHtmlAsync(() =>
            Task.FromResult($"<html><body>{expectedUserAgent}</body></html>"));

        UserAgentFetcher fetcher = new();
        FieldInfo cachedValueField = typeof(UserAgentFetcher).GetField("cachedValue", BindingFlags.Instance | BindingFlags.NonPublic)!;
        cachedValueField.SetValue(fetcher, "   ");

        string userAgent = await fetcher.GetUserAgent();

        Assert.That(userAgent, Is.EqualTo(expectedUserAgent));
    }

    [Test]
    public async Task GivenUncachedFetcher_WhenGettingUserAgentTwice_ThenSecondCallUsesCachedValue()
    {
        string expectedUserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:152.0) Gecko/20100101 Firefox/152.0";
        int invocationCount = 0;

        SetFetchHtmlAsync(() =>
        {
            invocationCount += 1;
            return Task.FromResult($"<html><body>{expectedUserAgent}</body></html>");
        });

        UserAgentFetcher fetcher = new();

        string firstUserAgent = await fetcher.GetUserAgent();
        string secondUserAgent = await fetcher.GetUserAgent();

        Assert.Multiple(() =>
        {
            Assert.That(secondUserAgent, Is.EqualTo(firstUserAgent));
            Assert.That(invocationCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GivenFetchResultWithoutMatchingUserAgent_WhenGettingUserAgent_ThenReturnsFallbackValue()
    {
        SetFetchHtmlAsync(() => Task.FromResult("<html><body>no valid firefox user agent here</body></html>"));

        UserAgentFetcher fetcher = new();

        string userAgent = await fetcher.GetUserAgent();

        Assert.That(
            userAgent,
            Is.EqualTo("Mozilla/5.0 (X11; Linux x86_64; rv:148.0) Gecko/20100101 Firefox/148.0"));
    }

    private static Func<Task<string>> GetFetchHtmlAsync()
    {
        FieldInfo fieldInfo = typeof(UserAgentFetcher).GetField("fetchHtmlAsync", BindingFlags.NonPublic | BindingFlags.Static)!;

        return (Func<Task<string>>)fieldInfo.GetValue(null)!;
    }

    private static void SetFetchHtmlAsync(Func<Task<string>> fetchHtmlAsync)
    {
        FieldInfo fieldInfo = typeof(UserAgentFetcher).GetField("fetchHtmlAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
        fieldInfo.SetValue(null, fetchHtmlAsync);
    }
}
