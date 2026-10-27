# http-net — working in this repo

ASP.NET Core building blocks — request/response abstractions, exception handling, ProblemDetails, OpenAPI, API versioning, forwarded headers and Serilog request logging — each scoped to a single concern and shipped independently on NuGet under `JorgeCostaMacia.Http.*`. Part of the `JorgeCostaMacia.*` family, on top of the **shared-net** foundation (consumed as published NuGet packages).

## Layout

- `src/<Package>/` — one package per folder. `test/<Package>.Tests/` — its tests. `assets/` — icons + social preview.
- **3-tier `Directory.Build.props`**: **root** (repo identity — Authors / Company / Copyright / Repository — + the single lockstep `VersionPrefix`; TFM `net10.0`; ImplicitUsings, Nullable, AnalysisLevel, EnforceCodeStyleInBuild, TreatWarningsAsErrors, the NuGet audit; `IsPackable` false by default) → **`src/`** (`IsPackable` true, package-output: icon / readme / license, SourceLink, symbols, `GenerateDocumentationFile`, pack of LICENSE/COPYRIGHT/icon/README) → **`test/`** (test settings). Each `src` csproj declares **only** `Description` / `PackageTags`; everything else is inherited (don't restate it).

## Targets & stack

- TFM: **`net10.0`**, single-target, declared once in the root props. net6/7/8/9 were dropped as they reached EOL — net9 in May 2026, net8 in November 2026. **No per-package override**: `Http.Exception` carried `net9.0;net10.0` for `ExceptionHandlerOptions.StatusCodeSelector` and `Http.OpenApi` was already `net10.0`-only for the native OpenAPI pipeline, and both restrictions stop being exceptions once the floor is net10. Being single-target is also what collapsed the per-TFM `PackageVersion Update` blocks in `Directory.Packages.props`, so there is now **one** version of every package — do not reintroduce either.
- ASP.NET Core deps via **`<FrameworkReference Include="Microsoft.AspNetCore.App" />`** (not the metapackage), so packages stay framework-aligned without pinning a runtime version.
- Tests: **xUnit v4 (the `xunit.v3` package, 4.x) on Microsoft.Testing.Platform v2 (MTP)** — test projects are `OutputType=Exe`, and `dotnet test` runs MTP because the root **`global.json`** opts in (`"test": { "runner": "Microsoft.Testing.Platform" }`). MTP v2 dropped the VSTest bridge, so `TestingPlatformDotnetTestSupport` is gone and running the tests needs the **.NET 10 SDK**. Not MSTest, not VSTest.
- Source is **UTF-8 without BOM** (`.editorconfig` `charset = utf-8`). camelCase locals, PascalCase types, I-prefixed interfaces. Copyright year stays **2023** (deliberate — don't bump).
- **Explicit types everywhere — spell the type out.** Never `var`, never target-typed `new()`, never collection expressions `[]`: write `new Foo(...)`, `new byte[] { ... }`, `new List<T> { ... }`, `Array.Empty<T>()`. The `.editorconfig` sets all three to explicit, but only `var` is analyzer-enforced — `new()`/`[]` can't be flagged (the analyzer never reports the implicit form), so `develop.yml` guards target-typed `new(` and implicit `new[]` with a **grep step** that fails the build, and collection expressions `[…]` with a second one (`code-style-net`'s pattern: attributes, indexers, array types and list patterns never match it). Do not introduce any of them. Lists follow the 120-column rule of `code-style-net` — declarations and constructions vertical, long calls and comments wrapped. No primary constructors on classes; a positional record stays where its generated properties are the point (a JSON-bound request body). Full ruleset in `.editorconfig` (shared verbatim with the other JorgeCostaMacia.* repos).

## Dependencies — two kinds

- **Cross-repo, on shared-net** (e.g. `Http.Exception` → `JorgeCostaMacia.Exception`): these are **`PackageReference` to the published shared-net packages**, version-pinned centrally in `Directory.Packages.props`. shared-net is a separate repo — never `ProjectReference` across repos. (The logging packages depend on the public `Serilog` / `Serilog.AspNetCore`, not on `JorgeCostaMacia.Serilog`.)
- **Intra-repo, between `Http.*` packages** (e.g. `Http.ProblemDetails` → `Http`): **`ProjectReference`**. `dotnet pack` turns each `ProjectReference` into a NuGet `<dependency>` at the sibling's version, so the graph still ships in the nuspec — but you build against local source and **release everything together** (no phased, tier-by-tier publishing). Don't reintroduce `PackageReference` between same-repo packages.

## Dependencies — Central Package Management

Third-party **and** the cross-repo shared-net package versions are centralized in **`Directory.Packages.props`** (`ManagePackageVersionsCentrally=true`): add or bump them **there** as `<PackageVersion>`, and reference packages in csproj **without** a `Version`. (Intra-repo `Http.*` deps are `ProjectReference`, not packages — so CPM doesn't manage them.)

**The strict gates are on**: `TreatWarningsAsErrors` and the NuGet vulnerability audit on every restore (direct and transitive, every severity), so any warning — an advisory included — fails the build. The fix is a version bump or a transitive override in CPM, never `NoWarn`. A third-party version lands once it is three days old, a security fix the day it ships (see `build-net`). The root `nuget.config` clears every inherited source and keeps nuget.org alone: this library consumes nothing else, and an inherited second source would raise `NU1507`. It is **deny-read** for agents — hand changes to it to the user.

## Versioning — lockstep

A single **`<VersionPrefix>`** lives in the **root `Directory.Build.props`** — bump it once and all packages + their intra-repo cross-deps move together. Never put `VersionPrefix` back in individual csproj.

## CI / publishing

- `.github/workflows/main.yml`: push to `main` → build → test → `dotnet pack http-net.slnx` → `dotnet nuget push *.nupkg --skip-duplicate` (nuget.org via Trusted Publishing / OIDC, no API-key secret). **The (central) `VersionPrefix` is the publish gate** — only new versions publish.
- `develop.yml`: on develop + PRs (no publish): the explicit-types grep, build, the `dotnet format` check, test, and pack as a packaging check. `.gitattributes` checks `*.cs` out as CRLF, so the Linux runner sees what the `.editorconfig` requires.
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

Skills that apply to this repo — let them trigger, or invoke explicitly. The first group is from `jorgecostamacia-agent-skills` (each `-net` skill brings the generic one it realizes); the rest from `dotnet-agent-skills` (the `dotnet/skills` marketplace).

A skill listed here but missing on this machine is one command away: `claude plugin marketplace add JorgeCostaMacia/skills` (or `dotnet/skills`) once, then `claude plugin install <skill>@<marketplace>`.

- **`validation-net`** — the boundary side of the validation spec: request validators via standard FluentValidation DI, and the family `ValidationException` failure list that **`Http.ProblemDetails` renders as per-field errors** — read it before touching exception handling or ProblemDetails.
- **`ddd-net`** (with **`ddd`**) — the exception family this repo translates to HTTP: typed `NotFound` / `Exist` / `Validation` exceptions with their fixed UUIDv7 codes.
- **`logging-net`** (with **`logging`**) — the logging style the `Http.Serilog` and `Http.Exception.Serilog` packages implement: fixed messages ("Request End", "Request Fail"), every variable value through the log context, correlation in every scope.
- **`code-style-net`** — how the C# is written: explicit types, the 120-column width with declarations and constructions vertical, comments wrapped too, the gates.
- **`build-net`** — the three-tier `Directory.Build.props`, the single `VersionPrefix`, CPM, the three-day package age, the NuGet audit as a build gate, the library's packing metadata, and the `nuget.config` base.
- **`testing-net`** (with **`testing`**) — xUnit v4 (the `xunit.v3` package) on Microsoft.Testing.Platform, the test project layout, the `TestServer` host suites with hand-written fakes.
- **`clean-architecture-net`** (with **`clean-architecture`**) / **`solid`** — these packages are the Presentation-side toolkit (thin delivery, transport mapping at the boundary) a host composes; design review of the public surface (middleware, `Use`/`Add` extensions, handlers).
- **`gitflow`** — all branch/release work (see *Branching & releases* above).
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

Run **`dotnet format` before committing** — it applies the `.editorconfig` (using ordering, whitespace, line endings), the CLI equivalent of Visual Studio's *Code Cleanup*, and it is the normalizer for a `.cs` a shell tool wrote with LF. Develop checks it after the build with `--verify-no-changes`: the solution is clean, so the check only ever fails on what the push itself brought. `.gitattributes` checks `*.cs` out as CRLF (`eol=crlf`), so the Linux runner sees what the `.editorconfig` requires.
