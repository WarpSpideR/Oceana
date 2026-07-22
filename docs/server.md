# Server — `Oceana.Server`

> An ASP.NET Core Web API (`net10.0`) that manages a registry of agents and streams audio out to them. Built with **FastEndpoints** (REPR) and **SignalR**, organised as **vertical slices**. For the exact HTTP/SignalR contract, see [api.md](./api.md).

## Structure: vertical slices + shared infrastructure

```
src/Oceana.Server/
├── Program.cs                     # composition root
├── Features/
│   ├── Agents/                    # the "Agents" vertical slice
│   │   ├── AgentInfo.cs           # domain model + status
│   │   ├── AgentStatus.cs
│   │   ├── IAgentRegistry.cs      # registry abstraction + in-memory impl
│   │   ├── AgentRegistry.cs
│   │   ├── ListAgents/            # one folder per endpoint (REPR)
│   │   ├── GetAgent/
│   │   ├── SetRouting/            # PUT /agents/{id}/routing (endpoint + request + validator)
│   │   ├── RemoveAgent/
│   │   ├── StartStream/           # endpoint + request + response + validator
│   │   └── StopStream/
│   └── Zones/                     # the "Zones" vertical slice
│       ├── ZoneInfo.cs            # domain model
│       ├── ZoneDevice.cs          # (agentId, deviceId) assignment
│       ├── ZoneUpdateResult.cs    # Updated / NotFound / NameConflict
│       ├── IZoneRegistry.cs       # registry abstraction + in-memory impl
│       ├── ZoneRegistry.cs
│       ├── ListZones/  GetZone/   # one folder per endpoint (REPR)
│       ├── CreateZone/  UpdateZone/
│       └── RemoveZone/
└── Infrastructure/                # shared technical services (not a feature)
    ├── Streaming/                 # tone generation + TCP streaming
    └── Realtime/                  # agent status hub + zone status hub + agent-control hub + notifiers/commander
```

- **`Features/Agents/`** and **`Features/Zones/`** are the slices today. Each use-case is a folder holding its FastEndpoints endpoint, its request/response DTOs, and its FluentValidation validator — everything for that operation in one place.
- **`Infrastructure/`** holds cross-cutting services shared across slices: audio streaming and the SignalR realtime layer.

> See the [architecture](./architecture.md#server-side-structure-vertical-slices--shared-infrastructure) note about the intentional (temporary) dependency from `Infrastructure` onto the Agents slice's domain types.

## FastEndpoints (REPR)

Each endpoint is its own class deriving from `Endpoint<TRequest[,TResponse]>` (or `EndpointWithoutRequest<TResponse>`), with `Configure()` declaring the route/verb and `HandleAsync()` doing the work. Responses use the current `Send.*` API (`Send.OkAsync`, `Send.CreatedAtAsync<T>`, `Send.NoContentAsync`, `Send.NotFoundAsync`, `Send.ErrorsAsync`, `Send.ResponseAsync`).

- **Global route prefix `api`** — set in `UseFastEndpoints` (so `Get("/agents")` → `/api/agents`).
- **Secure by default:** FastEndpoints requires authorization unless an endpoint calls `AllowAnonymous()`. There is **no auth yet**, so every endpoint currently calls `AllowAnonymous()`. (See [roadmap.md](./roadmap.md).)
- **Validation:** FluentValidation `Validator<TRequest>` classes are auto-discovered; failures return `400` with a structured error body — no handler wiring. Example: [`StartStreamValidator`](../src/Oceana.Server/Features/Agents/StartStream/StartStreamValidator.cs), [`SetRoutingValidator`](../src/Oceana.Server/Features/Agents/SetRouting/SetRoutingValidator.cs).

The endpoints and their status codes are tabulated in [api.md](./api.md).

## Agent registry

[`IAgentRegistry`](../src/Oceana.Server/Features/Agents/IAgentRegistry.cs) / [`AgentRegistry`](../src/Oceana.Server/Features/Agents/AgentRegistry.cs):

- In-memory, thread-safe (`ConcurrentDictionary<Guid, AgentInfo>`), registered as a **singleton**.
- **Agent-driven, not API-managed:** agents self-register over the control connection (see [Agent control plane](#agent-control-plane)). `RegisterOrUpdate` upserts by the agent's own id (preserving routing/status); `MarkOffline` flips `Connected` on disconnect, ignoring a stale drop after the agent has already reconnected. Agent **configuration** (identity, last-seen devices, routing) is persisted (see [Persistence](#persistence)); live state is re-derived on reconnect.
- Tracks per agent: `Connected` (control-connection state), reported `Devices`, desired `Routing`, and [`AgentStatus`](../src/Oceana.Server/Features/Agents/AgentStatus.cs) (`Idle → Connecting → Streaming`, or `Faulted`, back to `Idle`).

## Zone registry

[`IZoneRegistry`](../src/Oceana.Server/Features/Zones/IZoneRegistry.cs) / [`ZoneRegistry`](../src/Oceana.Server/Features/Zones/ZoneRegistry.cs):

- In-memory, registered as a **singleton**, write-through persisted (see [Persistence](#persistence)) so zones survive a restart.
- **API-managed** (unlike agents): `Create` generates the zone id; `Update` replaces name + devices; `Remove` deletes. Endpoints notify over `/hubs/zones` on success.
- **Unique names.** Because uniqueness spans keys, writes are serialised by a single `lock` guarding a `Dictionary<Guid, ZoneInfo>` plus a case-insensitive name→id index (a deliberate departure from `AgentRegistry`'s per-key CAS loops). `Create` returns null and `Update` returns `NameConflict` when a name is taken (→ `409`); a rename may keep the zone's own name.
- A zone holds a name and a list of [`ZoneDevice`](../src/Oceana.Server/Features/Zones/ZoneDevice.cs) `(agentId, deviceId)` pairs. Membership is **not** validated against the agent registry, so a zone may reference an offline or since-removed device; the front end resolves names/availability against `GET /api/agents`. Playing audio to a zone is the [zone broadcast](#zone-broadcast) below.

## Tone streaming

The current audio source is a **generated sine test tone** (proves the full pipeline without external inputs).

- [`ToneGenerator`](../src/Oceana.Server/Infrastructure/Streaming/ToneGenerator.cs) — interleaved **16-bit PCM** sine at **48 000 Hz** (`SampleRate=48000`, `BitsPerSample=16`, `Amplitude=0.25`), with a configurable **channel count** (1–8) and a **distinct frequency per channel** (channel n at `frequency × (n+1)`) so each channel is identifiable. Default base frequency **440 Hz**, **2** channels.
- [`AudioStreamManager`](../src/Oceana.Server/Infrastructure/Streaming/AudioStreamManager.cs) — orchestrates a stream to one agent:
  1. Resolve the agent, open a connection via [`IAgentConnectionFactory`](../src/Oceana.Server/Infrastructure/Streaming/IAgentConnectionFactory.cs) (`status → Connecting`).
  2. Write the OCAP header (PCM, 48 kHz, requested channel count, 16-bit), then `status → Streaming`.
  3. Pump **~20 ms chunks** (`ChunkMilliseconds=20` → 960 frames / 3840 bytes) until cancelled or the optional duration elapses; `status → Idle` (or `Faulted`).
- **Real-time pacing:** a `Stopwatch`-based schedule sleeps only while the wall clock is *behind* the audio timeline. This avoids the drift of a fixed `Task.Delay` per chunk (which, due to Windows' ~15 ms timer granularity, under-produces and slowly starves the agent's buffer).
- **One stream per agent:** tracked in a `ConcurrentDictionary` keyed by agent id; a second start returns `409 Conflict`.

The connection is abstracted behind [`IAgentConnection`](../src/Oceana.Server/Infrastructure/Streaming/IAgentConnection.cs) (real impl: [`TcpAgentConnection`](../src/Oceana.Server/Infrastructure/Streaming/TcpAgentConnection.cs), which sets `NoDelay`), so the stream manager is unit-testable without real sockets — see [`AudioStreamManagerTests`](../tests/Oceana.Server.Tests/Infrastructure/Streaming/AudioStreamManagerTests.cs).

The transport half of `AudioStreamManager` is source-agnostic: `RunStreamAsync(header, pump)` writes any OCAP header and runs any pump, so besides the tone it also streams a finite PCM buffer via `TryStreamPcmAsync(agentId, pcm, StreamFormat, ct)` (awaitable, same one-per-agent guard). `PumpBufferAsync` reuses the 20 ms real-time pacing, then holds the socket open for `TailHoldMilliseconds` (~750 ms) so the agent's buffered tail plays out (the agent does not drain on EOF).

## Zone broadcast

[`IZoneBroadcaster`](../src/Oceana.Server/Infrastructure/Streaming/IZoneBroadcaster.cs) / [`ZoneBroadcaster`](../src/Oceana.Server/Infrastructure/Streaming/ZoneBroadcaster.cs) plays a recorded PCM message (decoded from an uploaded WAV by [`WavReader`](../src/Oceana.Server/Infrastructure/Audio/WavReader.cs) — mono or stereo, 48 kHz, 16-bit) on every reachable device in a zone. `POST /api/zones/{id}/broadcast` reads the raw `audio/wav` body, then `PlanAndStart`:

- **Classifies** the zone's agents via [`ZoneTargeting.Classify`](../src/Oceana.Server/Infrastructure/Streaming/ZoneTargeting.cs) (shared with playback): `Offline` (not connected / unknown), `Busy` (already streaming), `NoActiveDevices` (none of the zone's device ids are currently reported), else **targeted**. Returns the targeted/skipped summary immediately as the `202` body — best-effort.
- For each targeted agent, a background task pushes an `AgentRouting` mapping the source's channels to those devices ([`IAgentRoutingCommander`](../src/Oceana.Server/Infrastructure/Realtime/IAgentRoutingCommander.cs)), waits a short **settle** delay (the agent snapshots routing at connect time), streams the buffer, then **restores** the agent's stored routing. Status flows through the normal `AudioStreamManager` path, so targeted agents show `Streaming` on `/hubs/agents`.

## Zone playback

[`IZonePlaybackService`](../src/Oceana.Server/Infrastructure/Streaming/IZonePlaybackService.cs) / [`ZonePlaybackService`](../src/Oceana.Server/Infrastructure/Streaming/ZonePlaybackService.cs) plays an uploaded audio **file** (stereo) to a zone with **start/stop and now-playing** — the first sustained, controllable source. The browser decodes + resamples to 48 kHz 16-bit and uploads PCM (so the server stays codec-free); `POST /api/zones/{id}/play` streams it play-once.

- Reuses `ZoneTargeting` and the same push-routing (all source channels → devices) → settle → `TryStreamPcmAsync` → restore envelope as broadcast, but is **stateful**: it keeps a `ConcurrentDictionary<zoneId, {CTS, target agents, source name, started-at}>`. `Start` replaces any current playback, returns the [`ZonePlaybackState`](../src/Oceana.Server/Infrastructure/Streaming/ZonePlaybackState.cs) immediately, and a coordinator (`Task.WhenAll` of the per-agent tasks) clears the entry and notifies **stopped** when the track ends naturally. `Stop(zoneId)` cancels the zone's linked CTS (each agent stream unwinds and restores routing).
- Playback start/stop/end are pushed over `/hubs/zones` as `ZonePlaybackChanged` (see Realtime); `GET /api/zones/{id}/playback` returns the current state for initial load. The uploaded PCM is held in memory for the playback's lifetime (bounded by the 256 MB body cap; a temp file is a future optimisation).

## Per-zone volume

Each zone has a persisted `Volume` (0.0–1.0, attenuation only) on [`ZoneInfo`](../src/Oceana.Server/Features/Zones/ZoneInfo.cs), stored/reloaded by the [zone registry](#zone-registry) like the rest of the config (`IZoneRegistry.SetVolume` write-through; `PUT /api/zones/{id}/volume` notifies `ZoneChanged`). The gain is applied **server-side in the stream pump**: `AudioStreamManager.TryStreamPcmAsync` takes a `Func<double> volume` read once per 20 ms chunk (unity is zero-copy; otherwise [`PcmGain.ApplyInt16`](../src/Oceana.Server/Infrastructure/Streaming/PcmGain.cs) scales the 16-bit samples). Playback passes a **live** provider that re-reads the zone's volume from the registry each chunk (so a slider change applies to audio already playing); broadcast captures the zone's volume at start.

## Realtime (SignalR)

[`AgentStatusHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentStatusHub.cs) is a strongly-typed hub (`Hub<IAgentStatusClient>`) mapped at **`/hubs/agents`**. The server pushes changes through [`IStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/IStatusNotifier.cs) → [`SignalRStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/SignalRStatusNotifier.cs) whenever an agent is registered, changes status, or is removed. Client methods: `AgentChanged(AgentInfo)` and `AgentRemoved(Guid)`. Enums are serialised **as strings**. Contract detail in [api.md](./api.md).

[`ZoneStatusHub`](../src/Oceana.Server/Infrastructure/Realtime/ZoneStatusHub.cs) (`Hub<IZoneStatusClient>`) is the zones analogue, mapped at **`/hubs/zones`**. The zone endpoints and playback service push through [`IZoneStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/IZoneStatusNotifier.cs) → [`SignalRZoneStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/SignalRZoneStatusNotifier.cs). Client methods: `ZoneChanged(ZoneInfo)`, `ZoneRemoved(Guid)`, and `ZonePlaybackChanged(ZonePlaybackState)`.

> The front end consumes both status hubs; the agent-control hub below is exercised by the live agent.

## Agent control plane

Agents connect to [`AgentControlHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentControlHub.cs) (`Hub<IAgentControlClient>`) at **`/hubs/agents-control`**:

- **`Register(AgentRegistration)`** — the agent calls this on connect (and reconnect). The hub joins the agent to a **group keyed by its id** (robust across reconnects), upserts the registry (host from the connection's remote IP, port + devices from the agent), notifies the front end, and returns the agent's stored `AgentRouting`.
- **`OnDisconnectedAsync`** — marks the agent offline (via `MarkOffline`) and notifies the front end.
- **Pushing routing:** [`IAgentRoutingCommander`](../src/Oceana.Server/Infrastructure/Realtime/IAgentRoutingCommander.cs) → [`SignalRRoutingCommander`](../src/Oceana.Server/Infrastructure/Realtime/SignalRRoutingCommander.cs) sends `SetRouting` to `Clients.Group(agentId)` — a no-op if the agent is offline. `PUT /api/agents/{id}/routing` stores the routing then pushes it; the agent applies it on its **next stream**.

Shared DTOs (`AgentRegistration`, `AgentRouting`, `AudioDevice`, `IAgentControlClient`) live in [`Oceana.Contracts`](../src/Oceana.Contracts). The agent side is in [agent.md](./agent.md#server-control-connection).

## Cross-cutting setup ([`Program.cs`](../src/Oceana.Server/Program.cs))

- **OpenAPI** via `FastEndpoints.OpenApi` (Microsoft.AspNetCore.OpenApi under the hood). Document name `v1`; served at `/openapi/v1.json` **in Development only**.
- **CORS** policy `frontend`: reflects **any** origin (`SetIsOriginAllowed(_ => true)`) with `AllowAnyHeader` + `AllowAnyMethod` + `AllowCredentials` (credentials are needed for SignalR, and can't be combined with `AllowAnyOrigin`, hence origin reflection). Permissive by design while there's no auth; tighten to an allow-list alongside authentication ([roadmap.md](./roadmap.md)).
- **Logging:** Serilog (console + request logging).
- DI singletons: `IAgentRegistry`, `IAgentConnectionFactory`, `IStatusNotifier`, `IAudioStreamManager`, `IZoneBroadcaster`, `IZonePlaybackService`, `IZoneRegistry`, `IZoneStatusNotifier`, and two `IStateStore<>` (see below).
- **Hubs mapped:** `/hubs/agents`, `/hubs/agents-control`, `/hubs/zones`.

## Persistence

Configuration is persisted as JSON so it survives a restart; **live state is not** (it is re-derived when agents reconnect and stream). Behind the existing registry interfaces:

- [`IStateStore<T>`](../src/Oceana.Server/Infrastructure/Persistence/IStateStore.cs) / [`JsonStateStore<T>`](../src/Oceana.Server/Infrastructure/Persistence/JsonStateStore.cs) — loads/saves a state snapshot as an indented JSON file. Writes are **atomic** (temp file then `File.Move` replace) and lock-serialised; a missing or corrupt file loads as `null` (start fresh).
- **Zones** → `data/zones.json` ([`ZonesState`](../src/Oceana.Server/Features/Zones/ZonesState.cs)): `ZoneRegistry` loads on startup (ids preserved) and writes through on every `Create`/`Update`/`Remove`.
- **Agents** → `data/agents.json` ([`AgentConfig`](../src/Oceana.Server/Features/Agents/AgentConfig.cs)/[`AgentsState`](../src/Oceana.Server/Features/Agents/AgentsState.cs)): only **configuration** — id, last-seen name/devices, desired `Routing`. Loaded on startup as **offline** known agents; when the agent reconnects, `RegisterOrUpdate` refreshes host/port/devices/connection and preserves the persisted routing (which `Register` returns to the agent). Written through on register, `SetDesiredRouting`, and remove — **not** on `MarkOffline`/`UpdateStatus` (those are live-only).
- **Location:** `Storage:Path` in config (absolute, or relative to the content root; default `data`). The directory is git-ignored. The registries take the store as an **optional** dependency, so unit tests construct them with no persistence (or an in-memory fake).

## Ports (note the discrepancy)

| Source | URL(s) |
|--------|--------|
| [`appsettings.json`](../src/Oceana.Server/appsettings.json) Kestrel | `http://localhost:5069` |
| [`Properties/launchSettings.json`](../src/Oceana.Server/Properties/launchSettings.json) profile | `https://localhost:50857;http://localhost:50858` |

> These don't match. `dotnet run` uses the **launchSettings** profile (50857/50858); running the built DLL directly with no launch profile picks up the **Kestrel** config (5069) — but only if the content root can find `appsettings.json` (i.e. run from the project's output directory). Worth reconciling; tracked in [roadmap.md](./roadmap.md).

## Running

```bash
# from the repo root
dotnet run --project src/Oceana.Server            # Development profile (launchSettings)
# OpenAPI (Development): http://localhost:<port>/openapi/v1.json
```

To see it drive a real agent end to end, start the [agent](./agent.md), then register it and start a stream via the [API](./api.md).

## Key files

| Area | Files |
|------|-------|
| Composition | [`Program.cs`](../src/Oceana.Server/Program.cs) |
| Registry & domain | [`Features/Agents/AgentRegistry.cs`](../src/Oceana.Server/Features/Agents/AgentRegistry.cs), [`AgentInfo.cs`](../src/Oceana.Server/Features/Agents/AgentInfo.cs), [`AgentStatus.cs`](../src/Oceana.Server/Features/Agents/AgentStatus.cs) |
| Endpoints | [`Features/Agents/*/`](../src/Oceana.Server/Features/Agents) (ListAgents, GetAgent, RegisterAgent, RemoveAgent, StartStream, StopStream) |
| Streaming | [`Infrastructure/Streaming/AudioStreamManager.cs`](../src/Oceana.Server/Infrastructure/Streaming/AudioStreamManager.cs), [`ToneGenerator.cs`](../src/Oceana.Server/Infrastructure/Streaming/ToneGenerator.cs) |
| Realtime | [`Infrastructure/Realtime/AgentStatusHub.cs`](../src/Oceana.Server/Infrastructure/Realtime/AgentStatusHub.cs), [`SignalRStatusNotifier.cs`](../src/Oceana.Server/Infrastructure/Realtime/SignalRStatusNotifier.cs) |
