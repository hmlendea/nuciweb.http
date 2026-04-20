using System.Net.Http;
using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    public static class HttpClientCreator
    {
        public static async Task<HttpClient> CreateAsync()
            => Create(await new UserAgentFetcher().GetUserAgent());

        public static async Task<HttpClient> CreateAsync(IUserAgentFetcher uaFetcher)
            => Create(await uaFetcher.GetUserAgent());

        public static HttpClient Create()
            => CreateAsync().Result;

        public static HttpClient Create(string userAgent)
        {
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            return httpClient;
        }
    }
}
