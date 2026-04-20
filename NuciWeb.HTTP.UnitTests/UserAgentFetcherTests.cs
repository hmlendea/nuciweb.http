using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NuciWeb.HTTP.UnitTests;

[TestFixture]
public class UserAgentFetcherTests
{
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
}
