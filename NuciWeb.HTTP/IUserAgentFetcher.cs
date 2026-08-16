using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    /// <summary>
    /// Defines a contract for retrieving a user-agent string.
    /// </summary>
    public interface IUserAgentFetcher
    {
        /// <summary>
        /// Retrieves a user-agent string.
        /// </summary>
        /// <returns>A user-agent string.</returns>
        public Task<string> GetUserAgent();
    }
}