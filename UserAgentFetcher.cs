using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    public sealed class UserAgentFetcher : IUserAgentFetcher
    {
        string cachedValue = null;

        public async Task<string> GetUserAgent()
        {
            if (!string.IsNullOrWhiteSpace(cachedValue))
            {
                return cachedValue;
            }

            using var client = new HttpClient();
            var html = await client.GetStringAsync("https://www.whatismybrowser.com/guides/the-latest-user-agent/firefox");

            var match = Regex.Match(html, @"Mozilla\/[1-9]\.[0-9] \(.*; Linux.*x86_64.*?Firefox\/[\d.]+");

            if (match.Success)
            {
                cachedValue = match.Value;

                return match.Value;
            }

            return "Mozilla/5.0 (X11; Linux x86_64; rv:139.0) Gecko/20100101 Firefox/139.0";
        }
    }
}