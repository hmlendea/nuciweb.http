[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/donate) [![Build Status](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/nuciweb.http)](https://github.com/hmlendea/nuciweb.http/releases/latest) [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# NuciWeb.HTTP

A lightweight .NET library with utilities for common HTTP and network-related tasks:

- Creating `HttpClient` instances with a realistic User-Agent
- Checking internet connectivity with multiple probing strategies
- Getting the public IP address through fallback providers
- Performing reverse DNS lookups for hostnames

## Features

- `HttpClientCreator` for easy `HttpClient` construction
- `UserAgentFetcher` for obtaining a modern Linux Firefox User-Agent
- `NetworkUtils` helpers for connectivity checks, reverse DNS, and public IP discovery
- Synchronous and asynchronous connectivity APIs

## Requirements

- .NET (target framework: `net10.0`)

## Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciWeb.HTTP)

### .NET CLI

```bash
dotnet add package NuciWeb.HTTP
```

### Package Manager Console

```powershell
Install-Package NuciWeb.HTTP
```

## Quick Start

```csharp
using NuciWeb.HTTP;

HttpClient client = await HttpClientCreator.CreateAsync();
bool online = await NetworkUtils.HasInternetAccessAsync();

if (online)
{
	string publicIp = NetworkUtils.GetPublicIpAddress();
}
```

## API Overview

### HttpClientCreator

Creates `HttpClient` instances with a configured User-Agent.

```csharp
HttpClient a = await HttpClientCreator.CreateAsync();
HttpClient b = await HttpClientCreator.CreateAsync(customFetcher);
HttpClient c = HttpClientCreator.Create("MyApp/1.0");
```

Methods:

- `CreateAsync()`
- `CreateAsync(IUserAgentFetcher uaFetcher)`
- `Create()`
- `Create(string userAgent)`

### IUserAgentFetcher / UserAgentFetcher

`IUserAgentFetcher` allows custom User-Agent provider implementations.

`UserAgentFetcher` implementation:

- Downloads the latest Firefox User-Agent page
- Extracts a Linux x86_64 Firefox signature
- Caches the value in-memory
- Falls back to a hardcoded modern Firefox User-Agent if extraction fails

### NetworkUtils

#### Connectivity

- `HasInternetAccess()`
- `HasInternetAccessAsync()`

Connectivity detection combines three probing strategies in parallel:

- TCP connect probes
- HTTP HEAD probes
- ICMP ping probes

The first successful strategy short-circuits the result to `true`.

#### Public IP

- `GetPublicIpAddress()`

Behavior:

- Requires internet connectivity (`HasInternetAccess`)
- Randomizes provider order on each call
- Returns the first non-empty response
- Throws `InvalidOperationException` if internet is unavailable or all providers fail

#### Reverse DNS

- `GetHostnames(IPAddress ipAddress)`
- `GetHostnames(string ipAddress)`

Behavior:

- Returns a de-duplicated list containing primary hostname and aliases
- Returns an empty list when reverse DNS is unavailable for the IP
- Throws:
  - `ArgumentNullException` for null `IPAddress`
  - `ArgumentException` for invalid IP string input

#### Wait for connectivity

- `WaitForInternetAccess()`
- `WaitForInternetAccess(TimeSpan timeout)`

Behavior:

- Polls connectivity once per second
- Throws `TimeoutException` when the timeout is exceeded

## Examples

### Check connectivity and wait until online

```csharp
using NuciWeb.HTTP;

if (!NetworkUtils.HasInternetAccess())
{
	NetworkUtils.WaitForInternetAccess(TimeSpan.FromSeconds(30));
}
```

### Resolve hostnames for an IP

```csharp
using System.Net;
using NuciWeb.HTTP;

List<string> hostnames = NetworkUtils.GetHostnames(IPAddress.Parse("1.1.1.1"));

if (hostnames.Count == 0)
{
	// No reverse DNS records were available
}
```

### Use a custom User-Agent fetcher

```csharp
using System.Threading.Tasks;
using NuciWeb.HTTP;

public sealed class StaticUaFetcher : IUserAgentFetcher
{
	public Task<string> GetUserAgent()
		=> Task.FromResult("MyApp/1.0");
}

HttpClient client = await HttpClientCreator.CreateAsync(new StaticUaFetcher());
```

## Development

### Prerequisites

- .NET SDK compatible with the target framework

### Build

```bash
dotnet build NuciWeb.HTTP.csproj
```

### Run

```bash
dotnet run --project NuciWeb.HTTP.csproj
```

### Test

```bash
dotnet test
```

## Contributing

Contributions are welcome.

When contributing:

- keep the project cross-platform
- preserve the existing public API unless a breaking change is intentional
- keep the changes focused and consistent with the current coding style
- update the documentation when behavior changes
- include tests for any new behavior

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
