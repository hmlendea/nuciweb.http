using System.Net.Http;
using System.Threading.Tasks;

namespace NuciWeb.HTTP
{
    public static class NetworkUtils
    {
        public static bool HasInternetAccess()
        {
            try
            {
                using (Ping ping = new())
                {
                    return ping.Send("mozilla.org", 3000).Status.Equals(IPStatus.Success);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
