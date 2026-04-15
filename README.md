[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/donate) [![Build Status](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/nuciweb.http/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/nuciweb.http)](https://github.com/hmlendea/nuciweb.http/releases/latest)

# About

NuGet package for common HTTP operations.

# Installation

[![Get it from NuGet](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/nuget.png)](https://nuget.org/packages/NuciWeb.HTTP)

**.NET CLI**:
```bash
dotnet add package NuciWeb.HTTP
```

**Package Manager**:
```powershell
Install-Package NuciWeb.HTTP
```

# Usage

## Public IP address

`NetworkUtils.GetPublicIpAddress()` uses multiple public IP providers.

On each call, it:
- Gets the full list of configured providers
- Randomizes the order
- Tries each provider one by one
- Returns the first successful non-empty response

If all providers fail, an `InvalidOperationException` is thrown.
