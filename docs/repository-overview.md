# Repository Overview

**Last Reviewed:** 2026-10-13

## Purpose

NuciWeb.HTTP is a .NET 10 library providing utilities for HTTP communication and network operations. It is distributed as a NuGet package and consumed by other .NET applications.

## Scope

### In Scope

- `HttpClient` creation with configurable User-Agent headers
- Internet connectivity detection (ICMP, TCP, HTTPS probes)
- Public IPv4 address retrieval from multiple providers
- Reverse DNS hostname resolution
- In-memory caching for User-Agent, public IP, and hostnames
- Unit tests covering all public APIs

### Out of Scope

- Persistent storage or database integration
- Logging, metrics, or tracing infrastructure
- Configuration files or environment variable support
- Background services or daemon processes
- IPv6 address handling
- HTTP request/response middleware or interception
- Authentication or authorization logic
- Web server or hosting capabilities

## Entry Points

### Public API (Static Facades)

| Class | Namespace | Purpose |
|-------|-----------|---------|
| `HttpClientCreator` | `NuciWeb.HTTP` | Creates `HttpClient` instances with User-Agent headers |
| `NetworkUtils` | `NuciWeb.HTTP` | Connectivity checks, IP retrieval, reverse DNS |

### Abstraction

| Interface | Namespace | Purpose |
|-----------|-----------|---------|
| `IUserAgentFetcher` | `NuciWeb.HTTP` | Contract for User-Agent string retrieval |

### Implementation

| Class | Namespace | Purpose |
|-------|-----------|---------|
| `UserAgentFetcher` | `NuciWeb.HTTP` | Web scraping implementation with caching |

## Project Structure

```
nuciweb.http/
├── NuciWeb.HTTP/                    # Main library project
│   ├── HttpClientCreator.cs         # Static facade for HttpClient creation
│   ├── IUserAgentFetcher.cs         # User-Agent fetcher interface
│   ├── NetworkUtils.cs              # Static facade for network utilities
│   ├── UserAgentFetcher.cs          # User-Agent fetching implementation
│   └── NuciWeb.HTTP.csproj          # Project file (net10.0)
├── NuciWeb.HTTP.UnitTests/          # Unit test project
│   ├── HttpClientCreatorTests.cs    # HttpClientCreator tests
│   ├── NetworkUtilsTests.cs         # NetworkUtils tests
│   ├── UserAgentFetcherTests.cs     # UserAgentFetcher tests
│   └── NuciWeb.HTTP.UnitTests.csproj # Test project file
├── NuciWeb.HTTP.slnx                # SDK-style solution file
├── README.md                        # Project overview and usage
├── ARCHITECTURE.md                  # High-level architecture
├── SECURITY.md                      # Security policy
├── PRIVACY.md                       # Privacy policy
├── CONTRIBUTING.md                  # Contribution guidelines
├── LICENSE                          # GPL-3.0-or-later license
└── docs/                            # Detailed documentation
```

## Target Framework

- **Primary:** .NET 10.0 (`net10.0`)
- **Nullable:** Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings:** Disabled (`<ImplicitUsings>disable</ImplicitUsings>`)

## Distribution

- **NuGet Package:** `NuciWeb.HTTP` (version 1.7.2)
- **Repository:** https://github.com/hmlendea/nuciweb.http
- **License:** GPL-3.0-or-later

## Key Dependencies

| Dependency | Version | Purpose |
|------------|---------|---------|
| `NuciExtensions` | 5.3.1 | Collection shuffling utility |
| `Microsoft.NET.Test.Sdk` | 18.3.0 | Test runner (test project) |
| `Moq` | 4.20.72 | Mocking framework (test project) |
| `NUnit` | 4.5.1 | Test framework (test project) |
| `NUnit3TestAdapter` | 6.2.0 | Test adapter (test project) |
| `Microsoft.AspNetCore.App` | Framework | HTTP probe server (test project) |

## Build & Test Commands

```bash
# Restore dependencies
dotnet restore NuciWeb.HTTP.slnx

# Build solution
dotnet build NuciWeb.HTTP.slnx

# Run all tests
dotnet test NuciWeb.HTTP.slnx

# Run with coverage
dotnet test NuciWeb.HTTP.slnx --collect:"XPlat Code Coverage"
```

## Related Documentation

- [Architecture](./architecture.md) — Detailed architecture
- [Repository Structure](./repository-structure.md) — Source tree layout
- [Root ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture
- [Root README.md](../README.md) — Project overview and usage