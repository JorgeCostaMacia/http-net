# JorgeCostaMacia.Http.Exception.Serilog

**Serilog exception logging** for ASP.NET Core — an `IExceptionHandler` that logs every unhandled exception (enriched with domain aggregate metadata and the authenticated user), while leaving the response itself to your status-code mapping.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.Exception.Serilog.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception.Serilog/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.Exception.Serilog.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception.Serilog/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.Exception.Serilog
```

## Usage

```csharp
using JorgeCostaMacia.Http.Exception.Serilog.Infrastructure;

builder.Services.AddExceptionHandler<ExceptionHandler>();
```

The handler logs known exceptions (`DomainException`, `BadHttpRequestException`, `FluentValidation.ValidationException`) at **Warning** and everything else at **Error**, pushing `ExceptionAggregateId` / `ExceptionAggregateCode` / `ExceptionAggregateType` / `UserName` to the Serilog `LogContext`. It always returns `false`, so the response is still produced by your status-code mapping (e.g. [JorgeCostaMacia.Http.Exception](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception/)).

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

Depends on [JorgeCostaMacia.Exception](https://www.nuget.org/packages/JorgeCostaMacia.Exception/), [Serilog](https://www.nuget.org/packages/Serilog/) and the ASP.NET Core shared framework.

## About

`JorgeCostaMacia.Http.Exception.Serilog` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
