# JorgeCostaMacia.Http.ProblemDetails

**RFC 7807 ProblemDetails, enriched** — consistent error responses for ASP.NET Core that add `requestId`, `traceId`, `nodeId` and domain aggregate metadata (`aggregateId` / `aggregateCode` / `aggregateType`), plus per-field `errors` for validation and bad-request failures, all following your JSON naming policy.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.ProblemDetails.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.ProblemDetails/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.ProblemDetails.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.ProblemDetails/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.ProblemDetails
```

## Usage

```csharp
builder.Services.AddProblemDetailsContext();
```

Every error response gets `requestId`, `traceId` and `nodeId`. A `DomainException` contributes its aggregate metadata (and, for a `ValidationException`, a per-field `errors` dictionary); a `BadHttpRequestException` contributes `errors` describing missing/invalid JSON fields. All keys follow the app's configured `JsonNamingPolicy`.

The `ProblemDetails` record (implementing `IResponseHttp`) is also provided as the canonical response shape for documentation and typed clients.

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

Depends on [JorgeCostaMacia.Http](https://www.nuget.org/packages/JorgeCostaMacia.Http/), [JorgeCostaMacia.Exception](https://www.nuget.org/packages/JorgeCostaMacia.Exception/) and the ASP.NET Core shared framework.

## About

`JorgeCostaMacia.Http.ProblemDetails` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
