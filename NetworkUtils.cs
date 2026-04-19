using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NuciExtensions;

namespace NuciWeb.HTTP
{
    public static class NetworkUtils
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static readonly string[] PingHosts =
        [
            "1.1.1.1",
            "9.9.9.9",
            "cloudflare.com",
            "quad9.net",
            "wikipedia.org",
            "eff.org",
            "torproject.org",
            "ping.archlinux.org",
            "ecloud.global"
        ];

        private static readonly string[] TcpHosts =
        [
            "1.1.1.1",
            "9.9.9.9",
            "cloudflare.com",
            "quad9.net",
            "ping.archlinux.org",
            "checkonline.home-assistant.io",
            "ecloud.global"
        ];

        private static readonly string[] HttpUrls =
        [
            "https://cloudflare.com",
            "https://www.wikipedia.org",
            "https://www.eff.org",
            "https://checkonline.home-assistant.io",
            "https://ping.archlinux.org"
        ];

        private static readonly string[] PublicIpSources =
        [
            "https://api.ipify.org",
            "https://checkip.amazonaws.com",
            "https://icanhazip.com",
            "https://ifconfig.me/ip",
        ];

        /// <summary>
        /// Checks if the system has internet access.
        /// </summary>
        /// <returns>Returns true if internet access is available, otherwise false.</returns>
        public static bool HasInternetAccess()
            => HasInternetAccessAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Checks if the system has internet access asynchronously.
        /// </summary>
        /// <returns>Returns true if internet access is available, otherwise false.</returns>
        public static async Task<bool> HasInternetAccessAsync()
        {
            if (await TryPingAsync().ConfigureAwait(false))
            {
                return true;
            }

            if (await TryTcpAsync().ConfigureAwait(false))
            {
                return true;
            }

            if (await TryHttpAsync().ConfigureAwait(false))
            {
                return true;
            }

            return false;
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


        private static async Task<bool> TryPingAsync()
        {
            try
            {
                using Ping ping = new();

                foreach (string host in PingHosts)
                {
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(host, 2000).ConfigureAwait(false);

                        if (reply.Status.Equals(IPStatus.Success))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return false;
        }

        private static async Task<bool> TryTcpAsync()
        {
            foreach (string host in TcpHosts)
            {
                try
                {
                    using TcpClient client = new();
                    using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(2000));

                    await client.ConnectAsync(host, 443, cts.Token).ConfigureAwait(false);

                    if (client.Connected)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            return false;
        }

        private static async Task<bool> TryHttpAsync()
        {
            foreach (string url in HttpUrls)
            {
                try
                {
                    using HttpRequestMessage request = new(HttpMethod.Head, url);
                    using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(3000));

                    using HttpResponseMessage response =
                        await HttpClient.SendAsync(
                            request,
                            HttpCompletionOption.ResponseHeadersRead,
                            cts.Token).ConfigureAwait(false);

                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 500)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            return false;
        }

        private static HttpClient CreateHttpClient()
        {
            SocketsHttpHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            HttpClient client = new(handler)
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("InternetAccessCheck/1.0");

            return client;
        }
    }
}
