using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    public interface IUserAgentFetcher
    {
        public Task<string> GetUserAgent();
    }
}