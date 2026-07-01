# JorgeCostaMacia.Http.Serilog

**Serilog request-logging middleware** for ASP.NET Core — request body buffering, request- and authentication-level log-context enrichment, and Serilog's per-request summary log event with a status-based log level.

[![NuGet](https://img.shields.io/nuget/v/JorgeCostaMacia.Http.Serilog.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Serilog/)
[![Downloads](https://img.shields.io/nuget/dt/JorgeCostaMacia.Http.Serilog.svg)](https://www.nuget.org/packages/JorgeCostaMacia.Http.Serilog/)
[![Build](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](https://github.com/JorgeCostaMacia/http-net/blob/main/LICENSE.txt)

---

## Install

```bash
dotnet add package JorgeCostaMacia.Http.Serilog
```

## Usage

Register in pipeline order:

```csharp
app.UseSerilogBodyBufferContext();          // enable body re-reading
app.UseSerilogEnrichRequestContext();       // scheme, host, ip, body, user-agent, X-Request-ID
app.UseAuthentication();
app.UseSerilogEnrichAuthenticationContext(); // UserName (after auth)
app.UseSerilogRequestLoggingContext();       // one "Request End" event per request
```

The request-completion event is logged at **Error** for exceptions / 5xx, **Warning** for 4xx, and **Information** otherwise.

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

Depends on [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore/) and the ASP.NET Core shared framework.

## About

`JorgeCostaMacia.Http.Serilog` is part of **[http-net](https://github.com/JorgeCostaMacia/http-net)** — ASP.NET Core building blocks, each scoped to a single concern and reusable across your services.

- **Repository:** [github.com/JorgeCostaMacia/http-net](https://github.com/JorgeCostaMacia/http-net)
- **Issues & requests:** [open an issue](https://github.com/JorgeCostaMacia/http-net/issues)
- **Contributing:** [CONTRIBUTING.md](https://github.com/JorgeCostaMacia/http-net/blob/main/CONTRIBUTING.md)
- **Security:** [report a vulnerability](https://github.com/JorgeCostaMacia/http-net/security/advisories/new)

**Author:** Jorge Costa Maciá

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
