# http-net — working in this repo

ASP.NET Core building blocks — request/response abstractions, exception handling, ProblemDetails, OpenAPI, API versioning and Serilog request logging — each scoped to a single concern and shipped independently on NuGet under `JorgeCostaMacia.Http.*`. Part of the `JorgeCostaMacia.*` family, on top of the **shared-net** foundation (consumed as published NuGet packages).

## Layout

- `src/<Package>/` — one package per folder. `test/<Package>.Tests/` — its tests. `assets/` — icons + social preview.
- **3-tier `Directory.Build.props`**: **root** (repo identity — Authors / Company / Copyright / Repository — + the single lockstep `VersionPrefix`; TFMs `net8.0;net9.0;net10.0`; ImplicitUsings, Nullable, AnalysisLevel, EnforceCodeStyleInBuild) → **`src/`** (package-output: icon / readme / license, SourceLink, symbols, `GenerateDocumentationFile`, pack of LICENSE/COPYRIGHT/icon/README) → **`test/`** (test settings). Each `src` csproj declares **only** `Description` / `PackageTags`; everything else is inherited (don't restate it).

## Targets & stack

- TFMs: **`net8.0;net9.0;net10.0`** (net6/7 dropped — EOL). Per-package override where a dependency forces it — e.g. **`Http.OpenApi` is `net10.0`-only** (`Microsoft.AspNetCore.OpenApi`).
- ASP.NET Core deps via **`<FrameworkReference Include="Microsoft.AspNetCore.App" />`** (not the metapackage), so packages stay framework-aligned without pinning a runtime version.
- Tests: **xUnit v4 (the `xunit.v3` package, 4.x) on Microsoft.Testing.Platform v2 (MTP)** — test projects are `OutputType=Exe`, and `dotnet test` runs MTP because the root **`global.json`** opts in (`"test": { "runner": "Microsoft.Testing.Platform" }`). MTP v2 dropped the VSTest bridge, so `TestingPlatformDotnetTestSupport` is gone and running the tests needs the **.NET 10 SDK**. Not MSTest, not VSTest.
- Source is **UTF-8 without BOM** (`.editorconfig` `charset = utf-8`). camelCase locals, PascalCase types, I-prefixed interfaces. Copyright year stays **2023** (deliberate — don't bump).
- **Explicit types everywhere — spell the type out.** Never `var`, never target-typed `new()`, never collection expressions `[]`: write `new Foo(...)`, `new byte[] { ... }`, `new List<T> { ... }`, `Array.Empty<T>()`. The `.editorconfig` sets all three to explicit, but only `var` is analyzer-enforced — `new()`/`[]` can't be flagged (the analyzer never reports the implicit form), so they are **convention, kept explicit by hand and by the IDE generating explicit**. Do not introduce `new()`/`[]` when editing. Full ruleset in `.editorconfig` (shared verbatim with the other JorgeCostaMacia.* repos).

## Dependencies — two kinds

- **Cross-repo, on shared-net** (e.g. `Http.Exception` → `JorgeCostaMacia.Exception`, `Http` → `GuidFactory`): these are **`PackageReference` to the published shared-net packages**, version-pinned centrally in `Directory.Packages.props`. shared-net is a separate repo — never `ProjectReference` across repos. (The logging packages depend on the public `Serilog` / `Serilog.AspNetCore`, not on `JorgeCostaMacia.Serilog`.)
- **Intra-repo, between `Http.*` packages** (e.g. `Http.ProblemDetails` → `Http`): **`ProjectReference`**. `dotnet pack` turns each `ProjectReference` into a NuGet `<dependency>` at the sibling's version, so the graph still ships in the nuspec — but you build against local source and **release everything together** (no phased, tier-by-tier publishing). Don't reintroduce `PackageReference` between same-repo packages.

## Dependencies — Central Package Management

Third-party **and** the cross-repo shared-net package versions are centralized in **`Directory.Packages.props`** (`ManagePackageVersionsCentrally=true`): add or bump them **there** as `<PackageVersion>`, and reference packages in csproj **without** a `Version`. (Intra-repo `Http.*` deps are `ProjectReference`, not packages — so CPM doesn't manage them.)

## Versioning — lockstep

A single **`<VersionPrefix>`** lives in the **root `Directory.Build.props`** — bump it once and all packages + their intra-repo cross-deps move together. Never put `VersionPrefix` back in individual csproj.

## CI / publishing

- `.github/workflows/main.yml`: push to `main` → build → test → `dotnet pack http-net.slnx` → `dotnet nuget push *.nupkg --skip-duplicate` (nuget.org via Trusted Publishing / OIDC, no API-key secret). **The (central) `VersionPrefix` is the publish gate** — only new versions publish.
- `develop.yml`: builds/tests on develop + PRs (no publish).
- `release.yml`: on a pushed `v*` tag → creates the **GitHub Release** with auto-generated notes.
- All three declare **top-level** `permissions:` (single job each).

## Branching & releases — GitFlow

Use the **`gitflow` skill** for any branch/release work — never improvise.

- Feature/bugfix → `feature/`|`bugfix/<name>-<ts>` from develop → finish `--no-ff` into develop.
- Release → `release/<version>` from develop → bump the **single** `VersionPrefix` in the **root** `Directory.Build.props` → Release Finish (merge develop+main, annotated tag `v<version>`, atomic push). One bump versions everything.
- Use git's **default merge message** (`--no-ff --no-edit`, never `-m`).
- Branch prefixes only: `feature` / `bugfix` / `release` / `hotfix`.

## Git etiquette

- Commit under **your own identity** — don't hardcode anyone's name/email.
- Keep history clean — **no** `Co-Authored-By` / AI-assistant trailers in commits or messages.
- Merges use git's **default** message (see *Branching & releases* above).

## Relevant skills

Skills that apply to this repo — let them trigger, or invoke explicitly. `gitflow`, `solid`, `clean-architecture`, `ddd`, `testing`, `logging-net` and `validation-net` are from `jorgecostamacia-agent-skills`; the rest from `dotnet-agent-skills` (the `dotnet/skills` marketplace).

- **`gitflow`** — all branch/release work (see *Branching & releases* above).
- **`solid`** — SOLID-principles design review; apply when shaping or reviewing the public surface of these packages (middleware, `Use`/`Add` extensions, handlers).
- **`clean-architecture`** — layers and the inward dependency rule; these packages ARE the Presentation-side toolkit (thin delivery, transport mapping at the boundary) consumed by the bounded contexts.
- **`ddd`** — tactical DDD; here mainly domain errors as typed exceptions with fixed codes, which this repo translates to HTTP.
- **`testing`** — testing principles: done-means-tested, one test file per unit, names as specification, classicist doubles (the TestServer suites), rule coverage.
- **`logging-net`** — the logging style for every log statement: fixed low-cardinality messages as grouping keys, all variable data via the log context, correlation ids in every scope (the `Http.Serilog` and `Http.Exception.Serilog` packages follow it — "Request End", "Request Fail" + enrichment).
- **`validation-net`** — the boundary side of the validation spec: request validators via standard FluentValidation DI, and the family `ValidationException` failure list that **`Http.ProblemDetails` renders as per-field errors** — read it before touching exception handling or ProblemDetails.
- **`dotnet-aspnetcore`** — the core domain here (Minimal APIs, middleware, ProblemDetails, OpenAPI, exception handling, API versioning).
- **`dotnet-webapi`** — HTTP endpoint semantics, OpenAPI metadata, and error-handling shape for the Minimal-API surface these packages support.
- **`dotnet`** — C# language server + general .NET development.
- **`dotnet-msbuild`** — `Directory.Build.props`, project-file quality/review, Central Package Management, build perf.
- **`dotnet-nuget`** — dependency management and package modernization.
- **`dotnet-test`** / **`dotnet-test-migration`** — running/generating tests; the xUnit.v3 / MTP setup.
- **`dotnet-upgrade`** — migrating across target-framework versions.

Not relevant to this repo (skip): `dotnet-ai`, `dotnet-maui`, `dotnet-blazor`, `dotnet-data`, `dotnet-template-engine`, `dotnet11`, `dotnet-diag`, `dotnet-advanced`.

## Build & test

```
dotnet format http-net.slnx                  # apply .editorconfig (using order, whitespace) — run before committing
dotnet build  http-net.slnx -c Release
dotnet test   http-net.slnx -c Release       # MTP v2 via global.json (needs the .NET 10 SDK); --logger is VSTest-only (MTP0001)
dotnet pack   http-net.slnx -c Release        # packs all packable; tests are IsPackable=false
```

Run **`dotnet format` before committing** — it applies the `.editorconfig` (using ordering, whitespace), the CLI equivalent of Visual Studio's *Code Cleanup*, so generated code doesn't drift from what the IDE would produce.
