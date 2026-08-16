[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/nuciweb.http)](https://github.com/hmlendea/nuciweb.http/releases/latest)
[![Build Status](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NuciWeb.HTTP)](https://nuget.org/packages/NuciWeb.HTTP)
[![License](https://img.shields.io/github/license/hmlendea/nuciweb.http)](https://github.com/hmlendea/nuciweb.http/blob/master/LICENSE)

# NuciWeb.HTTP

NuciWeb.HTTP is a .NET 10 library for creating `HttpClient` instances with configurable User-Agent headers and for connectivity, public IPv4 address, and reverse DNS operations.

## 📑 Table of Contents

- [Table of Contents](#table-of-contents)
- [Capabilities](#capabilities)
- [Usage](#usage)
  - [Examples](#examples)
    - [Wait for Connectivity](#wait-for-connectivity)
    - [Resolve Hostnames](#resolve-hostnames)
    - [Supply a Custom User-Agent](#supply-a-custom-user-agent)
- [Known Limitations](#known-limitations)
- [Installation](#installation)
  - [Package Manager Installation](#package-manager-installation)
  - [Manual Installation](#manual-installation)
- [Compatibility](#compatibility)
- [Integrations](#integrations)
- [Extensibility](#extensibility)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Test](#test)
  - [Continuous Integration](#continuous-integration)
  - [Dependencies](#dependencies)
- [Project Structure](#project-structure)
  - [Projects and Packages](#projects-and-packages)
- [Architecture](#architecture)
- [Contributing](#contributing)
- [Security](#security)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Create `HttpClient` instances with a dynamically retrieved, directly supplied, or provider-supplied User-Agent header
- Detect internet access through concurrent ICMP, TCP, and HTTPS probes with multiple fallback endpoints
- Retrieve and temporarily cache the first valid public IPv4 address returned by multiple providers
- Resolve and temporarily cache reverse DNS hostnames for an IP address
- Wait synchronously for connectivity with a default or caller-specified timeout

## 🚀 Usage

Create an `HttpClient` with a dynamically retrieved Linux Firefox User-Agent:

```csharp
using System;
using System.Net.Http;

using NuciWeb.HTTP;

using HttpClient httpClient = await HttpClientCreator.CreateAsync();
string responseBody = await httpClient.GetStringAsync("https://example.com");

Console.WriteLine(responseBody);
```

Use the network utilities independently:

```csharp
using System;

using NuciWeb.HTTP;

if (await NetworkUtils.HasInternetAccessAsync())
{
  string publicIpAddress = NetworkUtils.GetPublicIpAddress();
  Console.WriteLine($"Public IPv4 address: {publicIpAddress}");
}
```

### Examples

#### Wait for Connectivity

```csharp
using System;

using NuciWeb.HTTP;

NetworkUtils.WaitForInternetAccess(TimeSpan.FromSeconds(30));
```

`WaitForInternetAccess` polls once per second and throws `TimeoutException` when connectivity is not detected within the specified interval.

#### Resolve Hostnames

```csharp
using System;

using NuciWeb.HTTP;

foreach (string hostname in NetworkUtils.GetHostnames("1.1.1.1"))
{
  Console.WriteLine(hostname);
}
```

The result contains distinct primary and alias hostnames. An unavailable reverse DNS record produces an empty list, while malformed text produces `ArgumentException`.

#### Supply a Custom User-Agent

```csharp
using System.Net.Http;
using System.Threading.Tasks;

using NuciWeb.HTTP;

using HttpClient httpClient = await HttpClientCreator.CreateAsync(new StaticUserAgentFetcher());

public sealed class StaticUserAgentFetcher : IUserAgentFetcher
{
  public Task<string> GetUserAgent() => Task.FromResult("ExampleClient/1.0");
}
```

For a fixed value without a provider, call `HttpClientCreator.Create("ExampleClient/1.0")`.

## ⚠️ Known Limitations

- `GetPublicIpAddress()` accepts IPv4 responses only
- Connectivity endpoints, provider lists, timeouts, and cache intervals are not publicly configurable
- `UserAgentFetcher` propagates download failures; its fallback is used only when retrieved HTML contains no matching User-Agent
- Public IP retrieval, reverse DNS resolution, and connectivity waiting expose synchronous APIs and may block the calling thread

## 📦 Installation

[![Obtain it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciWeb.HTTP)
[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/nuciweb.http/releases)

### Package Manager Installation

```bash
dotnet add package NuciWeb.HTTP
```

Or, via the `Package Manager Console`:

```powershell
Install-Package NuciWeb.HTTP
```

### Manual Installation

Release assets also contain the `.nupkg` package. Download it from the [latest GitHub release](https://github.com/hmlendea/nuciweb.http/releases/latest), place it in a local package-source directory, and reference that directory from the consuming project:

```bash
dotnet add package NuciWeb.HTTP --source /path/to/package-source
```

## 🧩 Compatibility

| Component | Supported Versions | Notes |
|-----------|--------------------|-------|
| .NET | 10.0 or later | The package targets `net10.0`. |

## 🔌 Integrations

| Integration | Compatibility | Purpose | Required |
|-------------|---------------|---------|----------|
| whatismybrowser.com | HTTPS | Retrieves a current Linux Firefox User-Agent | Only for the default dynamic User-Agent provider |
| Connectivity probe endpoints | ICMP, TCP port 443, and HTTPS | Detects internet access through independent strategies | Only for connectivity checks and waiting |
| Public IP providers | HTTPS with plain-text IPv4 responses | Retrieves the caller's public IPv4 address | Only for public IP retrieval |
| Operating-system DNS resolver | .NET DNS APIs | Resolves hostnames from IP addresses | Only for reverse DNS operations |

## 🧱 Extensibility

Implement `IUserAgentFetcher` and pass it to `HttpClientCreator.CreateAsync(IUserAgentFetcher)` to control User-Agent acquisition without modifying the library.

| Extension Point | Contract | Purpose |
|-----------------|----------|---------|
| `IUserAgentFetcher` | `Task<string> GetUserAgent()` | Supplies the User-Agent assigned to a newly created `HttpClient` |

## 🛡️ Privacy and Data

| Data | Purpose | Storage | Retention | Optional |
|------|---------|---------|-----------|----------|
| Public IPv4 address | Returns the caller's public address | In-memory cache within the consuming process | Up to two minutes or until process termination | Yes; only when `GetPublicIpAddress()` is invoked |
| Outbound request metadata | Facilitates User-Agent retrieval, connectivity probes, and public IP retrieval | Not persisted by the library | The library retains none; external service policies apply | Yes; only when network-dependent APIs are invoked |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/)

### Setup

```bash
git clone https://github.com/hmlendea/nuciweb.http.git
cd nuciweb.http
dotnet restore NuciWeb.HTTP.sln
```

### Build

```bash
dotnet build NuciWeb.HTTP.sln --no-restore
```

### Test

```bash
dotnet test NuciWeb.HTTP.sln
```

### Continuous Integration

The `.NET` workflow restores dependencies, compiles the solution, and executes all tests for pushes and pull requests targeting `master`. The setup, compilation, and test commands above reproduce those checks locally.

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `NuciExtensions` | 5.3.1 | Runtime | Randomises connectivity endpoint and public IP provider order |

## 🗂️ Project Structure

The solution separates the distributable library from its NUnit unit tests.

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `NuciWeb.HTTP/NuciWeb.HTTP.csproj` | .NET library | Implements the public HTTP and network utility APIs |
| `NuciWeb.HTTP.UnitTests/NuciWeb.HTTP.UnitTests.csproj` | NUnit test project | Verifies public contracts, error handling, caching, and probe coordination |

## 🏗️ Architecture

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Add tests for additional or modified functionality

## 🔒 Security

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/nuciweb.http/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License version 3` or later.
See [LICENSE](./LICENSE) for further information.
