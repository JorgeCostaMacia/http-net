# JorgeCostaMacia.Http.Exception

**Global HTTP exception-handling middleware** for ASP.NET Core — maps known exception types to the right HTTP status code so unhandled exceptions produce consistent responses.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.Exception.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.Exception.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.Exception
```

## Usage

```csharp
using JorgeCostaMacia.Http.Exception.Infrastructure;
using JorgeCostaMacia.Http.ProblemDetails.Infrastructure;

builder.Services.AddProblemDetails(options => options.WithDefaults());   // or plain AddProblemDetails() — REQUIRED:
                                                                        // without an IProblemDetailsService the pipeline
                                                                        // throws InvalidOperationException at startup

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions().WithDefaultStatusCodes());
```

Pair it with [`JorgeCostaMacia.Http.ProblemDetails`](https://www.nuget.org/packages/JorgeCostaMacia.Http.ProblemDetails/) (`AddProblemDetails(o => o.WithDefaults())`) so the mapped status codes ship with the enriched RFC 7807 body.

`DefaultStatusCodeSelector` is public, so you can compose it — map a third-party exception explicitly and delegate the rest to the default.

## Status code mapping

| Exception | HTTP status |
| --- | --- |
| `DomainException` (and derived) | its own `AggregateHttpCode` |
| `BadHttpRequestException` | its own `StatusCode` |
| `FluentValidation.ValidationException` | 400 |
| `UnauthorizedAccessException` | 403 |
| `OperationCanceledException` / `TaskCanceledException` | 499 |
| `NotImplementedException` | 501 |
| anything else | 500 |

Programming-error exceptions (`ArgumentException`, `InvalidOperationException`…) and upstream-dependency failures fall through to 500 on purpose, so a bug never masquerades as a client error and the response never leaks — via a gateway code — that a downstream exists.

## Requirements

One of the following SDKs: **.NET 9 / 10** *(.NET 10 recommended)* — `StatusCodeSelector` requires ASP.NET Core 9+.

Depends on [JorgeCostaMacia.Exception](https://www.nuget.org/packages/JorgeCostaMacia.Exception/) and the ASP.NET Core shared framework.

## About

`JorgeCostaMacia.Http.Exception` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
