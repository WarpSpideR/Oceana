# CLAUDE.md

Guidance for AI agents working in this repository. Keep it accurate — update it when the rules below change. Full documentation lives in [`docs/`](docs/README.md); this file is the short operating guide.

## Project

Oceana is a networked audio broadcasting system: a **server** streams audio to one or more **agents**, each of which plays it through its machine's speakers. The code today is an early **prototype** — a shared protocol library, a Windows playback agent, a server that streams a generated sine **test tone**, and a React + MUI front end that drives the server.

| Project | TFM | Docs |
|---------|-----|------|
| `src/Oceana.Protocol` | `net10.0` | [protocol.md](docs/protocol.md) |
| `src/Oceana.Agent.Windows` | `net10.0-windows10.0.19041.0` | [agent.md](docs/agent.md) |
| `src/Oceana.Server` | `net10.0` | [server.md](docs/server.md) |
| `src/Oceana.Web` | React SPA — Vite + TS, npm (**not** MSBuild/`.sln`) | [web.md](docs/web.md) |
| `tests/*` | matches SUT | [development.md](docs/development.md) |

See [architecture.md](docs/architecture.md) for the big picture and [roadmap.md](docs/roadmap.md) for what's planned vs. done.

## Commands

```bash
dotnet build Oceana.sln -c Debug
dotnet test --solution Oceana.sln              # NOT `dotnet test Oceana.sln` — see Tests below
dotnet run --project src/Oceana.Server         # API server
dotnet run --project src/Oceana.Agent.Windows  # playback agent (Windows only), listens 0.0.0.0:8090
```

Front end (`src/Oceana.Web`, needs Node + npm; **not** built by `dotnet`/`Oceana.sln`):

```bash
cd src/Oceana.Web
npm install
npm run dev     # Vite dev server on http://localhost:5173 (set VITE_API_BASE_URL to the server's URL)
npm run build   # tsc -b + vite build
npm run lint    # oxlint
npm test        # vitest
```

## Build rules (non-negotiable)

- **Zero warnings.** `src` projects set `TreatWarningsAsErrors`. Fix the cause; do not suppress or `#pragma` around it.
- **XML docs required on all public *and* internal members** (StyleCop `documentInternalElements`). Culture is **en-GB**.
- **StyleCop.Analyzers `1.2.0-beta.556`** is enforced on `src`; some rules are disabled in [`.globalconfig`](.globalconfig). Match the existing file style (file-scoped namespaces, `using`s outside the namespace, System first). Test projects are outside `src/` and aren't StyleCop-governed.
- **Do not downgrade `Microsoft.OpenApi` below `2.11.0`** in `Oceana.Server` — the transitively-pulled `2.0.0` has a known vulnerability (`GHSA-v5pm-xwqc-g5wc`); the pin is deliberate.

## Tests

- **xUnit v3 on Microsoft.Testing.Platform (MTP).** Test projects are `OutputType=Exe`; [`global.json`](global.json) opts `dotnet test` into MTP mode, so use `dotnet test --solution <sln>` / `--project <csproj>` (a positional path fails). You can also run a test project's built `.exe` directly.
- **Assertions: AwesomeAssertions** — namespace is **`AwesomeAssertions`**, not `FluentAssertions`. Do **not** add FluentAssertions (v8+ is a paid/commercial library); AwesomeAssertions is the free fork with the same `.Should()` API.
- **Mocking: NSubstitute.**
- Each test project sets its `RootNamespace` to the project under test and **mirrors that project's folder structure** (a test sits in the same namespace/path shape as the code it tests). Follow this when adding tests.

## Architecture must-knows

- **Connection direction is inverted:** the **agent is the TCP listener** (`0.0.0.0:8090`); the **server is the client that dials out** to it. The wire format is **OCAP** — a fixed 16-byte handshake header, then raw PCM ([protocol.md](docs/protocol.md)).
- **Server** = FastEndpoints (REPR) + **vertical slices** (`Features/Agents/`, `Features/Zones/` — one folder per endpoint) with shared `Infrastructure/` (`Streaming/`, `Realtime/` SignalR, `Audio/` WAV decode). **Zones** are named groups of `(agentId, deviceId)` devices — in-memory, unique names (409 on clash); live over the `/hubs/zones` status hub. A **zone broadcast** (`POST /api/zones/{id}/broadcast`, raw `audio/wav` body) plays a recorded mic message on the zone's devices: `ZoneBroadcaster` pushes per-agent routing → streams the mono PCM (via `AudioStreamManager.TryStreamPcmAsync`) → restores routing (best-effort, skips offline/busy agents). FastEndpoints is **secure-by-default**, and there is no auth yet, so **every endpoint must call `AllowAnonymous()`**. CORS is likewise wide open (any origin, `SetIsOriginAllowed(_ => true)` + credentials) for the front end — tighten both when auth lands.
- **Agent** uses **NAudio 3.0-preview**: use `WaveOut` (the old `WaveOutEvent` is `[Obsolete]`); `BufferedWaveProvider`'s buffer size is a **constructor arg**, not a settable property. **Keep the `net10.0-windows10.0.19041.0` TFM** — NAudio 3.0's Windows audio stack requires the `10.0.19041` floor; a plain `net10.0-windows` silently loses `WaveOut`/WASAPI.
- **Front end** (`Oceana.Web`) is a **standalone Vite/npm project**, deliberately **outside** `Oceana.sln` and `dotnet` (it must not inherit `src/Directory.Build.props`' StyleCop/warnings-as-errors). Vertical slices under `src/`, TanStack Query + a SignalR hook keep state live; **MIT MUI only** (no MUI X Pro/Premium). See [web.md](docs/web.md).

## Working conventions

- Prefer **free/OSS** and modern **.NET 10** packages; call out licensing traps.
- Keep [`docs/`](docs/README.md) in sync when behaviour or structure changes.
- Don't commit or push unless explicitly asked. When unsure about an architectural fork, ask rather than assume.
