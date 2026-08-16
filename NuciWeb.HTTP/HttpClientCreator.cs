using System.Net.Http;
using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    /// <summary>
    /// Creates <see cref="HttpClient"/> instances configured with a user-agent header.
    /// </summary>
    public static class HttpClientCreator
    {
        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance using a dynamically retrieved user-agent value.
        /// </summary>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        public static async Task<HttpClient> CreateAsync()
            => Create(await new UserAgentFetcher().GetUserAgent());

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance using a user-agent value retrieved by the specified fetcher.
        /// </summary>
        /// <param name="uaFetcher">The user-agent fetcher used to retrieve the header value.</param>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        public static async Task<HttpClient> CreateAsync(IUserAgentFetcher uaFetcher)
            => Create(await uaFetcher.GetUserAgent());

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance using a dynamically retrieved user-agent value.
        /// </summary>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        public static HttpClient Create()
            => CreateAsync().Result;

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance using the provided user-agent value.
        /// </summary>
        /// <param name="userAgent">The user-agent value to assign to the default request headers.</param>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        public static HttpClient Create(string userAgent)
        {
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            return httpClient;
        }
    }
}
