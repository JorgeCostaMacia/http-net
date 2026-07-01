# JorgeCostaMacia.Http.OpenApi

**Native OpenAPI setup with RFC 7807 enrichment** — one call configures .NET's built-in OpenAPI generation and augments the ProblemDetails schemas with `errors`, `requestId`, `traceId`, `nodeId` and domain aggregate metadata, so your API docs match the error shape your services actually return.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.OpenApi.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.OpenApi/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.OpenApi.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.OpenApi/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.OpenApi
```

## Usage

```csharp
builder.Services.AddOpenApiContext();
```

Registers a schema transformer that adds `errors`, `requestId`, `traceId`, `nodeId`, `aggregateId`, `aggregateCode` and `aggregateType` to the `ProblemDetails` / `ValidationProblemDetails` / `HttpValidationProblemDetails` OpenAPI schemas, using the native Microsoft OpenAPI engine.

## Requirements

**.NET 10** — uses the native `Microsoft.AspNetCore.OpenApi` (10.x) generation pipeline.

## About

`JorgeCostaMacia.Http.OpenApi` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
