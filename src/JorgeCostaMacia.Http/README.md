# JorgeCostaMacia.Http

**HTTP request/response base contracts** — traceable `abstract record`s and marker interfaces for messages that cross the HTTP layer, carrying a stable id, a correlation id and a UTC timestamp for consistent tracing across distributed systems.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http
```

## Types

| Type | For |
| --- | --- |
| `IRequestHttp` | marker for incoming HTTP request contracts |
| `IResponseHttp` | marker for outgoing HTTP response contracts |
| `RequestHttp` | `abstract record` request base with `AggregateId` / `AggregateCorrelationId` / `AggregateOccurredAt` |
| `ResponseHttp` | `abstract record` response base with the same traceability metadata |

## Usage

```csharp
using JorgeCostaMacia.Http.Domain;

public sealed record CreateCustomerRequest(string Name)
    : RequestHttp(aggregateId: null, aggregateCorrelationId: null, aggregateOccurredAt: null);

// metadata is auto-filled: AggregateId (via GuidFactory — UUIDv7 on .NET 9+, v4 on .NET 8),
// AggregateCorrelationId (defaults to AggregateId), AggregateOccurredAt (UTC).
```

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

Depends on [JorgeCostaMacia.GuidFactory](https://www.nuget.org/packages/JorgeCostaMacia.GuidFactory/).

## About

`JorgeCostaMacia.Http` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
