# Repository Structure

**Last Reviewed:** 2026-10-13

## Source Tree Layout

```
nuciweb.http/
├── .github/
│   └── workflows/
│       └── dotnet.yml              # CI/CD pipeline
├── docs/                           # Detailed documentation
│   ├── api-reference/
│   │   ├── INDEX.md                # API reference index
│   │   ├── httpclientcreator.md    # HttpClientCreator API
│   │   ├── networkutils.md         # NetworkUtils API
│   │   └── iuseragentfetcher.md    # IUserAgentFetcher API
│   ├── components/
│   │   ├── http-client-creator.md
│   │   ├── user-agent-fetcher.md
│   │   ├── network-utils.md
│   │   └── abstractions.md
│   ├── flows/
│   │   ├── http-client-creation.md
│   │   ├── connectivity-detection.md
│   │   ├── public-ip-retrieval.md
│   │   └── reverse-dns-lookup.md
│   ├── INDEX.md                    # Master documentation index
│   ├── architecture.md             # Detailed architecture
│   ├── repository-overview.md      # This file
│   ├── repository-structure.md     # Source tree layout
│   ├── design-decisions.md
│   ├── configuration.md
│   ├── concurrency-and-scheduling.md
│   ├── error-handling.md
│   ├── logging.md
│   ├── security.md
│   ├── state-and-persistence.md
│   ├── dependencies.md
│   ├── integrations.md
│   ├── build-and-deployment.md
│   ├── testing.md
│   ├── change-guide.md
│   ├── troubleshooting.md
│   ├── documentation-maintenance.md
│   ├── api-usage-examples.md
│   ├── data-model.md
│   ├── invariants.md
│   ├── ambiguities-and-open-questions.md
│   └── faq.md
├── NuciWeb.HTTP/                   # Main library project
│   ├── HttpClientCreator.cs        # Static facade for HttpClient creation
│   ├── IUserAgentFetcher.cs        # User-Agent fetcher interface
│   ├── NetworkUtils.cs             # Static facade for network utilities
│   ├── UserAgentFetcher.cs         # User-Agent fetching implementation
│   └── NuciWeb.HTTP.csproj         # Project file (net10.0)
├── NuciWeb.HTTP.UnitTests/         # Unit test project
│   ├── HttpClientCreatorTests.cs   # HttpClientCreator tests
│   ├── NetworkUtilsTests.cs        # NetworkUtils tests
│   ├── UserAgentFetcherTests.cs    # UserAgentFetcher tests
│   └── NuciWeb.HTTP.UnitTests.csproj # Test project file
├── NuciWeb.HTTP.slnx               # SDK-style solution file
├── README.md                       # Project overview and usage
├── ARCHITECTURE.md                 # High-level architecture
├── SECURITY.md                     # Security policy
├── PRIVACY.md                      # Privacy policy
├── CONTRIBUTING.md                 # Contribution guidelines
├── LICENSE                         # GPL-3.0-or-later license
└── .gitignore                      # Git ignore rules
```

## Module Organisation

### NuciWeb.HTTP (Main Library)

| File | Type | Responsibility |
|------|------|----------------|
| `HttpClientCreator.cs` | Static class | Public API: Creates `HttpClient` with User-Agent |
| `IUserAgentFetcher.cs` | Interface | Abstraction: User-Agent retrieval contract |
| `UserAgentFetcher.cs` | Sealed class | Implementation: Web scraping + caching |
| `NetworkUtils.cs` | Static class | Public API: Connectivity, IP, DNS utilities |

**Namespace:** `NuciWeb.HTTP` for all types

### NuciWeb.HTTP.UnitTests (Test Project)

| File | Type | Responsibility |
|------|------|----------------|
| `HttpClientCreatorTests.cs` | Test class | Tests for HttpClientCreator |
| `NetworkUtilsTests.cs` | Test class | Tests for NetworkUtils |
| `UserAgentFetcherTests.cs` | Test class | Tests for UserAgentFetcher |

**Namespace:** `NuciWeb.HTTP.UnitTests` for all test types

## Project Files

### NuciWeb.HTTP.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>NuciWeb.HTTP</RootNamespace>
    <Version>1.7.2</Version>
    <Description>HttpClient wrapper</Description>
    <Authors>Horațiu Mlendea</Authors>
    <Copyright>Copyright 2026 © Horațiu Mlendea</Copyright>
    <RepositoryUrl>https://github.com/hmlendea/nuciweb.http</RepositoryUrl>
    <PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression>
    <PackageTags>Web HTTP</PackageTags>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NuciExtensions" Version="5.3.1" />
  </ItemGroup>
</Project>
```

### NuciWeb.HTTP.UnitTests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="NUnit" Version="4.5.1" />
    <PackageReference Include="NUnit3TestAdapter" Version="6.2.0" />
  </ItemGroup>
</Project>
```

### NuciWeb.HTTP.slnx

```xml
<?xml version="1.0" encoding="utf-8"?>
<Solution Sdk="Microsoft.NET.Sdk.Solution">
  <Project Path="NuciWeb.HTTP/NuciWeb.HTTP.csproj" />
  <Project Path="NuciWeb.HTTP.UnitTests/NuciWeb.HTTP.UnitTests.csproj" />
</Solution>
```

## Documentation Structure

The `docs/` directory follows a structured layout:

| Directory | Purpose |
|-----------|---------|
| `api-reference/` | One file per public API surface |
| `components/` | Per-component deep dives |
| `flows/` | End-to-end execution flows |
| Root `docs/` files | Cross-cutting concerns, reference, operations |

## Naming Conventions

| Element | Convention |
|---------|------------|
| Project directories | PascalCase (`NuciWeb.HTTP`) |
| Source files | PascalCase matching type name (`HttpClientCreator.cs`) |
| Test files | `{TypeName}Tests.cs` |
| Documentation files | kebab-case (`http-client-creator.md`) |
| Documentation directories | kebab-case, plural (`components/`, `flows/`) |
| Namespaces | PascalCase matching project (`NuciWeb.HTTP`) |

## Build Artifacts (Ignored)

```
bin/
obj/
*.nupkg
*.snupkg
```

## Related Documentation

- [Repository Overview](./repository-overview.md) — Purpose, scope, entry points
- [Architecture](./architecture.md) — Detailed architecture
- [Root ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture