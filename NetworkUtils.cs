using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;

using NuciExtensions;

namespace NuciWeb.HTTP
{
    public static class NetworkUtils
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly string[] PublicIpSources =
        {
            "https://api.ipify.org",
            "https://ifconfig.me/ip",
            "https://icanhazip.com",
            "https://checkip.amazonaws.com"
        };

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
        /// Gets the public IP address of the system by making a request to an external service.
        /// </summary>
        /// <returns>The public IP address as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no internet access is available.</exception>
        public static string GetPublicIpAddress()
        {
            if (!HasInternetAccess())
            {
                throw new InvalidOperationException("No internet access available.");
            }

            List<Exception> errors = [];
            string[] sources = (string[])PublicIpSources.Clone();
            sources.Shuffle();

            foreach (string source in sources)
            {
                try
                {
                    string response = HttpClient
                        .GetStringAsync(source)
                        .GetAwaiter()
                        .GetResult()
                        .Trim();

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            throw new InvalidOperationException(
                "Unable to retrieve the public IP address from any source.",
                new AggregateException(errors));
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
