<p align="center">
  <img src="https://raw.githubusercontent.com/JorgeCostaMacia/http-net/main/assets/social-preview.png" width="100%" alt="http-net" />
</p>

# http-net

> HTTP building blocks for ASP.NET Core — request/response abstractions, exception handling, ProblemDetails, OpenAPI, API versioning and Serilog request logging — each scoped to a single concern and shipped independently under `JorgeCostaMacia.Http.*`.

[![License](https://img.shields.io/github/license/JorgeCostaMacia/http-net.svg)](LICENSE.txt)
[![Main](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/main.yml)
[![Develop](https://github.com/JorgeCostaMacia/http-net/actions/workflows/develop.yml/badge.svg?branch=develop)](https://github.com/JorgeCostaMacia/http-net/actions/workflows/develop.yml)

Part of the `JorgeCostaMacia.*` family, on top of the [shared-net](https://github.com/JorgeCostaMacia/shared-net) DDD foundation (consumed as published NuGet packages), alongside bus-net (messaging).

## Requirements

One of the following SDKs: **.NET 8 / 9 / 10** *(.NET 10 recommended)*.

## Packages

| Package | What it does |
| --- | --- |
| [JorgeCostaMacia.Http](https://www.nuget.org/packages/JorgeCostaMacia.Http/) | HTTP request/response base contracts — traceable abstract records. |
| [JorgeCostaMacia.Http.Exception](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception/) | Maps domain / validation / bad-request exceptions to HTTP status codes. |
| [JorgeCostaMacia.Http.Exception.Serilog](https://www.nuget.org/packages/JorgeCostaMacia.Http.Exception.Serilog/) | Serilog logging of unhandled exceptions, enriched with aggregate metadata. |
| [JorgeCostaMacia.Http.MinimalApi.Versioning](https://www.nuget.org/packages/JorgeCostaMacia.Http.MinimalApi.Versioning/) | URL-segment API versioning for Minimal APIs. |
| [JorgeCostaMacia.Http.OpenApi](https://www.nuget.org/packages/JorgeCostaMacia.Http.OpenApi/) | Native OpenAPI setup with RFC 7807 ProblemDetails schema enrichment. |
| [JorgeCostaMacia.Http.ProblemDetails](https://www.nuget.org/packages/JorgeCostaMacia.Http.ProblemDetails/) | Enriched RFC 7807 ProblemDetails responses with domain metadata and per-field errors. |
| [JorgeCostaMacia.Http.Serilog](https://www.nuget.org/packages/JorgeCostaMacia.Http.Serilog/) | Serilog request-logging middleware: body buffering, enrichment and a per-request summary. |

## Contact

- [LinkedIn](https://www.linkedin.com/in/jorge-costa-macia-842817164/)
- [GitHub](https://github.com/JorgeCostaMacia/)
- [Bitbucket](https://bitbucket.org/jorgecostamacia/)
