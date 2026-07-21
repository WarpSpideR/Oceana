# Server — `Oceana.Server`

> An ASP.NET Core Web API (`net10.0`) that manages a registry of agents and streams audio out to them. Built with **FastEndpoints** (REPR) and **SignalR**, organised as **vertical slices**. For the exact HTTP/SignalR contract, see [api.md](./api.md).

## Structure: vertical slices + shared infrastructure

```
src/Oceana.Server/
├── Program.cs                     # composition root
├── Features/
│   └── Agents/                    # the "Agents" vertical slice
│       ├── AgentInfo.cs           # domain model + status
│       ├── AgentStatus.cs
│       ├── IAgentRegistry.cs      # registry abstraction + in-memory impl
│       ├── AgentRegistry.cs
│       ├── ListAgents/            # one folder per endpoint (REPR)
│       ├── GetAgent/
│       ├── SetRouting/            # PUT /agents/{id}/routing (endpoint + request + validator)
│       ├── RemoveAgent/
│       ├── StartStream/           # endpoint + request + response + validator
│       └── StopStream/
└── Infrastructure/                # shared technical services (not a feature)
    ├── Streaming/                 # tone generation + TCP streaming
    └── Realtime/                  # status hub + agent-control hub + notifier/commander
```

- **`Features/Agents/`** is the only slice today. Each use-case is a folder holding its FastEndpoints endpoint, its request/response DTOs, and its FluentValidation validator — everything for that operation in one place.
- **`Infrastructure/`** holds cross-cutting services shared across (future) slices: audio streaming and the SignalR realtime layer.

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
- **Agent-driven, not API-managed:** agents self-register over the control connection (see [Agent control plane](#agent-control-plane)). `RegisterOrUpdate` upserts by the agent's own id (preserving routing/status); `MarkOffline` flips `Connected` on disconnect, ignoring a stale drop after the agent has already reconnected. Nothing is persisted — empty on restart.
- Tracks per agent: `Connected` (control-connection state), reported `Devices`, desired `Routing`, and [`AgentStatus`](../src/Oceana.Server/Features/Agents/AgentStatus.cs) (`Idle → Connecting → Streaming`, or `Faulted`, back to `Idle`).

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

## Realtime (SignalR)

[`AgentStatusHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentStatusHub.cs) is a strongly-typed hub (`Hub<IAgentStatusClient>`) mapped at **`/hubs/agents`**. The server pushes changes through [`IStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/IStatusNotifier.cs) → [`SignalRStatusNotifier`](../src/Oceana.Server/Infrastructure/Realtime/SignalRStatusNotifier.cs) whenever an agent is registered, changes status, or is removed. Client methods: `AgentChanged(AgentInfo)` and `AgentRemoved(Guid)`. Enums are serialised **as strings**. Contract detail in [api.md](./api.md).

> The status hub isn't yet exercised by a browser client (a React app will consume it); the agent-control hub below *is* exercised by the live agent.

## Agent control plane

Agents connect to [`AgentControlHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentControlHub.cs) (`Hub<IAgentControlClient>`) at **`/hubs/agents-control`**:

- **`Register(AgentRegistration)`** — the agent calls this on connect (and reconnect). The hub joins the agent to a **group keyed by its id** (robust across reconnects), upserts the registry (host from the connection's remote IP, port + devices from the agent), notifies the front end, and returns the agent's stored `AgentRouting`.
- **`OnDisconnectedAsync`** — marks the agent offline (via `MarkOffline`) and notifies the front end.
- **Pushing routing:** [`IAgentRoutingCommander`](../src/Oceana.Server/Infrastructure/Realtime/IAgentRoutingCommander.cs) → [`SignalRRoutingCommander`](../src/Oceana.Server/Infrastructure/Realtime/SignalRRoutingCommander.cs) sends `SetRouting` to `Clients.Group(agentId)` — a no-op if the agent is offline. `PUT /api/agents/{id}/routing` stores the routing then pushes it; the agent applies it on its **next stream**.

Shared DTOs (`AgentRegistration`, `AgentRouting`, `AudioDevice`, `IAgentControlClient`) live in [`Oceana.Contracts`](../src/Oceana.Contracts). The agent side is in [agent.md](./agent.md#server-control-connection).

## Cross-cutting setup ([`Program.cs`](../src/Oceana.Server/Program.cs))

- **OpenAPI** via `FastEndpoints.OpenApi` (Microsoft.AspNetCore.OpenApi under the hood). Document name `v1`; served at `/openapi/v1.json` **in Development only**.
- **CORS** policy `frontend`: origins from config `Cors:AllowedOrigins` (default `http://localhost:5173`, `http://localhost:3000`), `AllowAnyHeader` + `AllowAnyMethod` + `AllowCredentials` (the last is needed for SignalR).
- **Logging:** Serilog (console + request logging).
- DI singletons: `IAgentRegistry`, `IAgentConnectionFactory`, `IStatusNotifier`, `IAudioStreamManager`.

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
