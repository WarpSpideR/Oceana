# Development

> How to build, run, and test Oceana, plus the code-style rules and the notable package/tooling decisions behind the current setup.

## Prerequisites

- **.NET 10 SDK** (developed against `10.0.300`).
- Windows is required to **run** `Oceana.Agent.Windows` and its tests (they target `net10.0-windows10.0.19041.0` and use NAudio). `Oceana.Protocol` and `Oceana.Server` are OS-agnostic (`net10.0`).

## Solution layout

| Project | TFM | Purpose |
|---------|-----|---------|
| [`src/Oceana.Protocol`](../src/Oceana.Protocol) | `net10.0` | Shared OCAP wire format ([protocol.md](./protocol.md)). |
| [`src/Oceana.Agent.Windows`](../src/Oceana.Agent.Windows) | `net10.0-windows10.0.19041.0` | Playback agent ([agent.md](./agent.md)). |
| [`src/Oceana.Server`](../src/Oceana.Server) | `net10.0` | Web API server ([server.md](./server.md)). |
| [`tests/Oceana.Protocol.Tests`](../tests/Oceana.Protocol.Tests) | `net10.0` | Protocol tests. |
| [`tests/Oceana.Agent.Windows.Test`](../tests/Oceana.Agent.Windows.Test) | `net10.0-windows10.0.19041.0` | Agent tests. |
| [`tests/Oceana.Server.Tests`](../tests/Oceana.Server.Tests) | `net10.0` | Server tests. |

## Build

```bash
dotnet build Oceana.sln -c Debug
```

The build must be **0 warnings / 0 errors**: `TreatWarningsAsErrors` and StyleCop are enforced on the `src` projects (see below).

## Run

```bash
dotnet run --project src/Oceana.Server            # the API server
dotnet run --project src/Oceana.Agent.Windows     # a playback agent (0.0.0.0:8090)
```

End-to-end: start both, then register the agent and start a tone via the [API](./api.md).

## Test

The test projects use **xUnit v3** on **Microsoft.Testing.Platform (MTP)** — each test project is an executable. The repo's [`global.json`](../global.json) opts `dotnet test` into MTP mode:

```json
{ "test": { "runner": "Microsoft.Testing.Platform" } }
```

Because of MTP mode, the invocation uses `--solution` / `--project` (not a positional path):

```bash
dotnet test --solution Oceana.sln          # run all tests
dotnet test --project tests/Oceana.Server.Tests/Oceana.Server.Tests.csproj
```

You can also run a test project's **executable directly** (handy for CI without the `dotnet test` wrapper):

```bash
./tests/Oceana.Server.Tests/bin/Debug/net10.0/Oceana.Server.Tests.exe
```

There are **21 tests** today (Protocol 4, Agent 4, Server 13), all green.

### Test stack

| Package | Version | Role |
|---------|---------|------|
| `xunit.v3.mtp-v2` | 3.2.2 | xUnit v3, wired for MTP **v2** (matches the .NET 10 SDK). |
| `AwesomeAssertions` | 9.5.0 | Fluent assertions (`.Should()…`). Note: its namespace is `AwesomeAssertions`. |
| `AwesomeAssertions.Analyzers` | 9.0.8 | Assertion analyzers. |
| `NSubstitute` (+ `.Analyzers.CSharp`) | 6.0.0 / 1.0.17 | Mocking (server & agent tests). |

**Convention:** each test project sets its `RootNamespace` to the project under test and **mirrors that project's folder structure**, so a test lives in the same namespace/path shape as the code it exercises (e.g. `Infrastructure/Streaming/AudioStreamManagerTests.cs` ↔ `Infrastructure/Streaming/AudioStreamManager.cs`).

## Code style & quality

Enforced on `src` projects via [`src/Directory.Build.props`](../src/Directory.Build.props):

- **`TreatWarningsAsErrors = True`** (Debug & Release) — warnings fail the build.
- **`GenerateDocumentationFile = True`** — public **and internal** members require XML doc comments (StyleCop `documentInternalElements`).
- **`EnforceCodeStyleInBuild = True`**.
- **StyleCop.Analyzers `1.2.0-beta.556`** — a preview that understands modern C# (collection expressions, `required`, target-typed `new`, primary constructors).
- Docs culture is **en-GB**; file-scoped namespaces; `using`s outside the namespace, System first.

Rule tweaks live in [`.globalconfig`](../.globalconfig): a handful of StyleCop rules are disabled (`SA1101`, `SA1200`, `SA1201`, `SA1202`, `SA1204`, `SA1313`, `SA1633`, `SA1642`; `SA1202/1200/1313` are marked to re-enable once StyleCop stabilises). Test projects are **not** under `src/`, so they don't inherit these rules.

## Notable package/tooling decisions (and why)

- **NAudio 3.0.0-preview.18** for the agent — chosen for the modern Windows audio stack; forces the `net10.0-windows10.0.19041.0` TFM (see [agent.md](./agent.md#target-framework--and-why-its-so-specific)).
- **FastEndpoints 8.2.0 + FastEndpoints.OpenApi 8.2.0** for the server — REPR endpoints + vertical slices, with Microsoft.AspNetCore.OpenApi-based OpenAPI.
- **`Microsoft.OpenApi` pinned to 2.11.0** — an explicit override because the transitively-pulled 2.0.0 has a known vulnerability (advisory `GHSA-v5pm-xwqc-g5wc`).
- **AwesomeAssertions instead of FluentAssertions** — FluentAssertions v8+ became a paid/commercial library; AwesomeAssertions is the free Apache-2.0 fork of the last open 7.x line. Same `.Should()` API, different namespace.
- **xUnit v3 + MTP** over xUnit v2/VSTest — the modern .NET 10 test runner; `Microsoft.NET.Test.Sdk` and the VSTest adapter are no longer referenced.

## Code coverage (not yet wired)

Coverage isn't configured. The old `coverlet.collector` (a VSTest data-collector) doesn't apply under MTP; the modern path is the `Microsoft.Testing.Extensions.CodeCoverage` extension with `dotnet test --coverage`. Tracked in [roadmap.md](./roadmap.md).
