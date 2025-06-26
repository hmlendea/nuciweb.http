using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace NuciWeb.HTTP
{
    public static class NetworkUtils
    {
        /// <summary>
        /// Checks if the system has internet access.
        /// </summary>
        /// <returns>Returns true if internet access is available, otherwise false.</returns>
        public static bool HasInternetAccess()
        {
            try
            {
                using Ping ping = new();

                return ping.Send("mozilla.org", 3000).Status.Equals(IPStatus.Success);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for internet access to be available, with a default timeout of 30 seconds.
        /// </summary>
        /// <exception cref="TimeoutException">Thrown if internet access is not available within the specified timeout.</exception>
        public static void WaitForInternetAccess()
            => WaitForInternetAccess(TimeSpan.FromSeconds(30));

        /// <summary>
        /// Waits for internet access to be available.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for internet access.</param>
        /// <exception cref="TimeoutException">Thrown if internet access is not available within the specified timeout.</exception>
        public static void WaitForInternetAccess(TimeSpan timeout)
        {
            DateTime beginningDT = DateTime.Now;

            while (DateTime.Now < beginningDT + timeout)
            {
                if (HasInternetAccess())
                {
                    return;
                }

                Thread.Sleep(1000);
            }

            throw new TimeoutException("No internet access after the specified timeout.");
        }
    }
}
