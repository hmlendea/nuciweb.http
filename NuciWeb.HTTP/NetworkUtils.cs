using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private static readonly List<string> PingHosts =
        [
            "1.1.1.1",
            "1.0.0.1",
            "9.9.9.9",
            "149.112.112.112",
            "94.140.14.14",
            "94.140.15.15",
            "cloudflare.com",
            "disroot.org",
            "debian.org",
            "duckduckgo.com",
            "ecloud.global",
            "eff.org",
            "fsf.org",
            "mullvad.net",
            "openstreetmap.org",
            "ping.archlinux.org",
            "privacyguides.org",
            "quad9.net",
            "riseup.net",
            "torproject.org",
            "wikipedia.org",
        ];

        private static readonly List<string> TcpHosts =
        [
            "1.1.1.1",
            "1.0.0.1",
            "9.9.9.9",
            "149.112.112.112",
            "94.140.14.14",
            "94.140.15.15",
            "checkonline.home-assistant.io",
            "cloudflare.com",
            "disroot.org",
            "debian.org",
            "duckduckgo.com",
            "ecloud.global",
            "fsf.org",
            "mullvad.net",
            "openstreetmap.org",
            "ping.archlinux.org",
            "privacyguides.org",
            "quad9.net",
            "riseup.net",
            "torproject.org",
        ];

        private static readonly List<string> HttpUrls =
        [
            "https://checkonline.home-assistant.io",
            "https://codeberg.org",
            "https://cloudflare.com",
            "https://disroot.org",
            "https://debian.org",
            "https://duckduckgo.com",
            "https://eff.org",
            "https://fsf.org",
            "https://mullvad.net",
            "https://openstreetmap.org",
            "https://ping.archlinux.org",
            "https://privacyguides.org",
            "https://riseup.net",
            "https://torproject.org",
            "https://wikipedia.org",
        ];

        private static readonly List<string> PublicIpSources =
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
            using CancellationTokenSource cts = new();

            List<Task<bool>> checks =
            [
                TryTcpAsync(cts.Token),
                TryHttpAsync(cts.Token),
                TryPingAsync(cts.Token),
            ];

            while (checks.Count > 0)
            {
                Task<bool> completedCheck = await Task.WhenAny(checks).ConfigureAwait(false);

                if (await completedCheck.ConfigureAwait(false))
                {
                    cts.Cancel();
                    return true;
                }

                checks.Remove(completedCheck);
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

            foreach (string source in PublicIpSources.Shuffle())
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
        /// Gets the known hostnames associated with the specified IP address using reverse DNS lookup.
        /// </summary>
        /// <param name="ipAddress">The IP address to resolve.</param>
        /// <returns>A list containing the primary hostname and aliases, if available.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ipAddress"/> is null.</exception>
        public static List<string> GetHostnames(IPAddress ipAddress)
        {
            ArgumentNullException.ThrowIfNull(ipAddress);

            IPHostEntry hostEntry;

            try
            {
                hostEntry = Dns.GetHostEntry(ipAddress);
            }
            catch (SocketException)
            {
                return [];
            }

            return [
                .. new[] { hostEntry.HostName }
                    .Concat(hostEntry.Aliases)
                    .Where(hostname => !string.IsNullOrWhiteSpace(hostname))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            ];
        }

        /// <summary>
        /// Gets the known hostnames associated with the specified IP address using reverse DNS lookup.
        /// </summary>
        /// <param name="ipAddress">The IP address to resolve.</param>
        /// <returns>A list containing the primary hostname and aliases, if available.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ipAddress"/> is not a valid IP address.</exception>
        public static List<string> GetHostnames(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress parsedIpAddress))
            {
                throw new ArgumentException("The provided value is not a valid IP address.", nameof(ipAddress));
            }

            return GetHostnames(parsedIpAddress);
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


        private static async Task<bool> TryPingAsync(CancellationToken cancellationToken)
        {
            try
            {
                using Ping ping = new();

                foreach (string host in PingHosts.Shuffle())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    try
                    {
                        PingReply reply = await ping
                            .SendPingAsync(host, 2000)
                            .WaitAsync(cancellationToken)
                            .ConfigureAwait(false);

                        if (reply.Status.Equals(IPStatus.Success))
                        {
                            return true;
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            catch
            {
                // Ignore
            }

            return false;
        }

        private static async Task<bool> TryTcpAsync(CancellationToken cancellationToken)
        {
            foreach (string host in TcpHosts.Shuffle())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                try
                {
                    using TcpClient client = new();
                    using CancellationTokenSource cts =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    cts.CancelAfter(TimeSpan.FromMilliseconds(2000));

                    await client.ConnectAsync(host, 443, cts.Token).ConfigureAwait(false);

                    if (client.Connected)
                    {
                        return true;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                catch
                {
                    // Ignore
                }
            }

            return false;
        }

        private static async Task<bool> TryHttpAsync(CancellationToken cancellationToken)
        {
            foreach (string url in HttpUrls.Shuffle())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                try
                {
                    using HttpRequestMessage request = new(HttpMethod.Head, url);
                    using CancellationTokenSource cts =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    cts.CancelAfter(TimeSpan.FromMilliseconds(3000));

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
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return false;
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
