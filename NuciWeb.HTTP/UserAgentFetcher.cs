using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    /// <summary>
    /// Retrieves user-agent strings for Linux Firefox by scraping a web source and caching the result.
    /// </summary>
    public sealed class UserAgentFetcher : IUserAgentFetcher
    {
        private static string FallbackUserAgent =>
            "Mozilla/5.0 (X11; Linux x86_64; rv:148.0) Gecko/20100101 Firefox/148.0";

        private static string UserAgentSourceUrl =>
            "https://www.whatismybrowser.com/guides/the-latest-user-agent/firefox";

        private static Func<Task<string>> FetchHtmlAsync = RetrieveLatestUserAgentHtmlAsync;

        private string cachedValue = null!;

        /// <summary>
        /// Retrieves a user-agent string, using a cached value if available.
        /// </summary>
        /// <returns>A user-agent string for Linux Firefox.</returns>
        public async Task<string> GetUserAgent()
        {
            if (!string.IsNullOrWhiteSpace(cachedValue))
            {
                return cachedValue;
            }

            string html = await FetchHtmlAsync().ConfigureAwait(false);

            Match match = Regex.Match(html, @"Mozilla\/[1-9]\.[0-9] \(.*; Linux.*x86_64.*?Firefox\/[\d.]+");

            if (match.Success)
            {
                cachedValue = match.Value;

                return match.Value;
            }

            return FallbackUserAgent;
        }

        private static async Task<string> RetrieveLatestUserAgentHtmlAsync()
        {
            using HttpClient client = new();

            return await client.GetStringAsync(UserAgentSourceUrl).ConfigureAwait(false);
        }
    }
}