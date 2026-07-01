# JorgeCostaMacia.Http.MinimalApi.Versioning

**URL-segment API versioning for Minimal APIs** — one call wires [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) from an `ApiVersion` configuration value, with ApiExplorer integration for OpenAPI.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.MinimalApi.Versioning.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.MinimalApi.Versioning/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.MinimalApi.Versioning.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.MinimalApi.Versioning/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.MinimalApi.Versioning
```

## Usage

```jsonc
// appsettings.json
{ "ApiVersion": 1 }
```

```csharp
builder.Services.AddVersioningContext(builder.Configuration);
```

Configures URL-segment versioning (e.g. `/v1/resource`): sets the default version, reports `api-supported-versions` headers, assumes the default when unspecified, and substitutes the `{version:apiVersion}` route token. Throws if `ApiVersion` is missing from configuration.

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

Depends on [Asp.Versioning.Http](https://www.nuget.org/packages/Asp.Versioning.Http/) and [Asp.Versioning.Mvc.ApiExplorer](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer/).

## About

`JorgeCostaMacia.Http.MinimalApi.Versioning` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
