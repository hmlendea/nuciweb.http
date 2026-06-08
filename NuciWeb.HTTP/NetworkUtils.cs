using System;
using System.Collections.Concurrent;
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
        private static readonly TimeSpan ReverseLookupCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan PublicIpAddressCacheDuration = TimeSpan.FromMinutes(2);
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();

        private static readonly List<string> PingHosts =
        [
            "1.1.1.1",
            "1.0.0.1",
            "9.9.9.9",
            "149.112.112.112",
            "94.140.14.14",
            "94.140.15.15",
            "89.233.43.71",
            "91.239.100.100",
            "185.95.218.42",
            "185.95.218.43",
            "185.228.168.9",
            "185.228.169.9",
            "194.242.2.2",
            "194.242.2.3",
            "193.110.81.0",
            "185.253.5.0",
            "76.76.2.0",
            "76.76.10.0",
            "149.154.159.92",
            "cloudflare.com",
            "bitwarden.com",
            "discourse.privacyguides.net",
            "disroot.org",
            "debian.org",
            "duckduckgo.com",
            "ecloud.global",
            "epic.org",
            "eff.org",
            "fdroid.org",
            "fsf.org",
            "fsfe.org",
            "gnu.org",
            "infomaniak.com",
            "ivpn.net",
            "libreoffice.org",
            "mailbox.org",
            "matrix.org",
            "mozilla.org",
            "mullvad.net",
            "nixos.org",
            "noyb.eu",
            "privacyinternational.org",
            "posteo.de",
            "proton.me",
            "openstreetmap.org",
            "ping.archlinux.org",
            "privacyguides.org",
            "quad9.net",
            "riseup.net",
            "signal.org",
            "torproject.org",
            "tuta.com",
            "wikimedia.org",
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
            "89.233.43.71",
            "91.239.100.100",
            "185.95.218.42",
            "185.95.218.43",
            "185.228.168.9",
            "185.228.169.9",
            "194.242.2.2",
            "194.242.2.3",
            "193.110.81.0",
            "185.253.5.0",
            "76.76.2.0",
            "76.76.10.0",
            "149.154.159.92",
            "checkonline.home-assistant.io",
            "cloudflare.com",
            "bitwarden.com",
            "discourse.privacyguides.net",
            "disroot.org",
            "debian.org",
            "duckduckgo.com",
            "ecloud.global",
            "epic.org",
            "fsf.org",
            "fdroid.org",
            "fsfe.org",
            "gnu.org",
            "infomaniak.com",
            "ivpn.net",
            "libreoffice.org",
            "mailbox.org",
            "matrix.org",
            "mozilla.org",
            "mullvad.net",
            "nixos.org",
            "noyb.eu",
            "privacyinternational.org",
            "posteo.de",
            "proton.me",
            "openstreetmap.org",
            "ping.archlinux.org",
            "privacyguides.org",
            "quad9.net",
            "riseup.net",
            "signal.org",
            "torproject.org",
            "tuta.com",
            "wikimedia.org",
        ];

        private static readonly List<string> HttpUrls =
        [
            "https://checkonline.home-assistant.io",
            "https://bitwarden.com",
            "https://codeberg.org",
            "https://cloudflare.com",
            "https://discourse.privacyguides.net",
            "https://disroot.org",
            "https://debian.org",
            "https://duckduckgo.com",
            "https://epic.org",
            "https://eff.org",
            "https://fdroid.org",
            "https://fsf.org",
            "https://fsfe.org",
            "https://gnu.org",
            "https://infomaniak.com",
            "https://ivpn.net",
            "https://libreoffice.org",
            "https://mailbox.org",
            "https://matrix.org",
            "https://mozilla.org",
            "https://mullvad.net",
            "https://nixos.org",
            "https://noyb.eu",
            "https://privacyinternational.org",
            "https://posteo.de",
            "https://proton.me",
            "https://openstreetmap.org",
            "https://ping.archlinux.org",
            "https://privacyguides.org",
            "https://riseup.net",
            "https://signal.org",
            "https://torproject.org",
            "https://tuta.com",
            "https://wikimedia.org",
            "https://wikipedia.org",
        ];

        private static readonly List<string> PublicIpSources =
        [
            "https://4.ident.me",
            "https://am.i.mullvad.net/ip",
            "https://api-ipv4.ip.sb/ip",
            "https://api.ip.sb/ip",
            "https://api.ipify.org",
            "https://api.my-ip.io/ip",
            "https://api4.ipify.org",
            "https://bot.whatismyipaddress.com",
            "https://checkip.amazonaws.com",
            "https://icanhazip.com",
            "https://ident.me",
            "https://ifconfig.co/ip",
            "https://ifconfig.eu",
            "https://ifconfig.io/ip",
            "https://ifconfig.me/ip",
            "https://ip.sb",
            "https://ip.seeip.org",
            "https://ip.tyk.nu",
            "https://ipecho.io/plain",
            "https://ipecho.net/plain",
            "https://ipinfo.io/ip",
            "https://ipv4.icanhazip.com",
            "https://ipv4.seeip.org",
            "https://ipv4bot.whatismyipaddress.com",
            "https://l2.io/ip",
            "https://myexternalip.com/raw",
            "https://myip.dnsomatic.com",
            "https://myip.wtf/text",
            "https://v4.ident.me",
            "https://v4.ifconfig.co/ip",
            "https://whatismyip.akamai.com",
            "https://wtfismyip.com/text",
        ];

        private sealed record CacheEntry(object Value, DateTimeOffset ExpiresAt);

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
            => GetOrCreateCachedValue(
                "public-ip-address",
                PublicIpAddressCacheDuration,
                RetrievePublicIpAddress);

        /// <summary>
        /// Gets the known hostnames associated with the specified IP address using reverse DNS lookup.
        /// </summary>
        /// <param name="ipAddress">The IP address to resolve.</param>
        /// <returns>A list containing the primary hostname and aliases, if available.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ipAddress"/> is null.</exception>
        public static List<string> GetHostnames(IPAddress ipAddress)
        {
            ArgumentNullException.ThrowIfNull(ipAddress);

            string[] cachedHostnames = GetOrCreateCachedValue(
                $"hostnames:{ipAddress}",
                ReverseLookupCacheDuration,
                () => ResolveHostnames(ipAddress).ToArray());

            return [.. cachedHostnames];
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

        private static string RetrievePublicIpAddress()
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
                        .GetResult();

                    if (TryNormalizePublicIpAddress(response, out string publicIpAddress))
                    {
                        return publicIpAddress;
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

        private static bool TryNormalizePublicIpAddress(string response, out string publicIpAddress)
        {
            publicIpAddress = string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            string candidate = response.Trim();

            if (!IPAddress.TryParse(candidate, out IPAddress parsedIpAddress))
            {
                return false;
            }

            if (parsedIpAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            publicIpAddress = parsedIpAddress.ToString();

            return true;
        }

        private static List<string> ResolveHostnames(IPAddress ipAddress)
        {
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

        private static T GetOrCreateCachedValue<T>(
            string cacheKey,
            TimeSpan cacheDuration,
            Func<T> valueFactory)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (Cache.TryGetValue(cacheKey, out CacheEntry cachedEntry)
                && cachedEntry.ExpiresAt > now)
            {
                return (T)cachedEntry.Value;
            }

            T value = valueFactory();

            if (value is not null)
            {
                Cache[cacheKey] = new CacheEntry(value, now.Add(cacheDuration));
            }

            return value;
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
