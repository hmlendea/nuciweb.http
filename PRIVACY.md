# Privacy Policy

**Effective Date:** 2026-10-13
**Last Updated:** 2026-10-13
**Version:** 1.0

## 1. Overview

NuciWeb.HTTP is a .NET library for creating `HttpClient` instances with configurable User-Agent headers and for connectivity, public IPv4 address, and reverse DNS operations. This privacy policy describes how the library handles data when used in your applications.

## 2. Data We Process

### 2.1 User-Agent Strings
- **What:** User-Agent header values sent with HTTP requests
- **Source:** Dynamically retrieved from a remote provider, directly supplied by the caller, or provided via a custom `IUserAgentFetcher` implementation
- **Purpose:** To identify the client making HTTP requests
- **Retention:** Not persisted by the library; used only for the duration of the `HttpClient` instance

### 2.2 Public IPv4 Addresses
- **What:** The first valid public IPv4 address returned by external providers
- **Source:** Queried from multiple public IP detection endpoints (e.g., `ifconfig.me`, `ipinfo.io`, `icanhazip.com`)
- **Purpose:** To determine the application's public IP address for connectivity verification
- **Retention:** Temporarily cached in memory for the lifetime of the `PublicIpv4AddressResolver` instance; not written to disk

### 2.3 Reverse DNS Hostnames
- **What:** Hostnames resolved via reverse DNS lookup for a given IP address
- **Source:** System DNS resolver (PTR records)
- **Purpose:** To provide human-readable hostnames for IP addresses
- **Retention:** Temporarily cached in memory for the lifetime of the `ReverseDnsResolver` instance; not written to disk

### 2.4 Connectivity Probe Data
- **What:** Results of ICMP ping, TCP connection, and HTTPS requests to probe endpoints
- **Source:** Multiple fallback endpoints (e.g., `google.com`, `cloudflare.com`, `github.com`)
- **Purpose:** To detect internet connectivity
- **Retention:** Not persisted; evaluated in-memory during connectivity checks

## 3. Data We Do Not Collect

- **No personal data:** The library does not collect names, email addresses, account credentials, or any personally identifiable information
- **No telemetry:** No usage analytics, crash reports, or performance metrics are sent to any third party
- **No persistent storage:** No data is written to disk, databases, or local files by the library itself
- **No tracking:** No cookies, local storage, or fingerprinting mechanisms are used

## 4. Third-Party Providers

The library queries the following types of external services:

| Purpose | Example Providers | Data Sent |
|---------|-------------------|-----------|
| Public IP detection | `ifconfig.me`, `ipinfo.io`, `icanhazip.com` | HTTP request with default headers |
| Connectivity probes | `google.com`, `cloudflare.com`, `github.com` | ICMP/TCP/HTTPS probe requests |
| User-Agent fetching | Configurable remote endpoint | HTTP GET request |

**Your responsibility:** When using this library, you should review the privacy policies of any third-party endpoints you configure or that are used by default. The library maintainers do not control these external services.

## 5. Data Flow Summary

```
Application Code
      │
      ▼
NuciWeb.HTTP Library
      │
      ├──▶ HttpClient (User-Agent header) ──▶ Your HTTP Target
      │
      ├──▶ PublicIpv4AddressResolver ──▶ IP Detection Providers
      │         │
      │         └──▶ In-memory cache (instance lifetime)
      │
      ├──▶ ReverseDnsResolver ──▶ System DNS (PTR lookup)
      │         │
      │         └──▶ In-memory cache (instance lifetime)
      │
      └──▶ ConnectivityChecker ──▶ Probe Endpoints (ICMP/TCP/HTTPS)
                │
                └──▶ In-memory result (method call scope)
```

## 6. Your Obligations

As the application developer integrating NuciWeb.HTTP:

1. **Disclose** to your users if your application transmits their IP address or User-Agent to external services
2. **Configure** custom endpoints only with providers whose privacy practices you accept
3. **Implement** `IUserAgentFetcher` responsibly if supplying custom User-Agent logic
4. **Comply** with applicable data protection laws (GDPR, CCPA, etc.) for your application's overall data handling

## 7. Security

- All external communications use HTTPS where supported by the provider
- No sensitive data (tokens, passwords, keys) is ever transmitted by the library
- The library does not implement encryption; it relies on the underlying .NET `HttpClient` and system DNS

## 8. Changes to This Policy

Updates will be published in the repository's `PRIVACY.md` and noted in release changelogs. Continued use of the library after changes constitutes acceptance.

## 9. Contact

For privacy questions related to this library, open an issue at:
https://github.com/hmlendea/nuciweb.http/issues

---

*This policy covers the NuciWeb.HTTP library only. Your application's overall privacy practices remain your responsibility.*