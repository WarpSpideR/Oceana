# Roadmap & Status

> What Oceana does today, what's planned, and the reasoning behind the key decisions made so far. This is the honest gap between the current prototype and the wider vision.

## Where things stand

**Implemented (working, tested):**

- **OCAP protocol** — fixed 16-byte handshake header + raw-PCM streaming ([protocol.md](./protocol.md)).
- **Windows agent** — TCP listener that de-interleaves an N-channel stream and plays channel subsets across **one or more output devices** (WASAPI), with a pre-roll jitter buffer and backpressure ([agent.md](./agent.md)).
- **Server** — FastEndpoints Web API with an in-memory agent registry, SignalR status broadcasting, OpenAPI, and CORS; streams a **generated sine test tone** to agents with real-time pacing ([server.md](./server.md), [api.md](./api.md)).
- **Server control plane** — agents **self-register** over a persistent SignalR connection (`/hubs/agents-control`) reporting their devices; the server pushes **channel→device routing**, applied on the agent's next stream ([server.md](./server.md#agent-control-plane), [agent.md](./agent.md#server-control-connection)).
- **React front end** (`Oceana.Web`) — a Vite + TypeScript + MUI SPA consuming the REST API and the `/hubs/agents` + `/hubs/zones` status hubs: a live agent dashboard, per-agent channel→device routing editor, and test-tone stream start/stop ([web.md](./web.md)).
- **Zones (management)** — a `Features/Zones` slice + UI to create/rename/delete named zones and assign devices from any agent(s), kept live over `/hubs/zones`. In-memory, unique names. Streaming *to* a zone is not yet built ([server.md](./server.md#zone-registry), [api.md](./api.md#zones)).
- **Verified end-to-end**: agent self-registers with devices → push routing → 4-channel stream splits across two devices per the server config → disconnect flips `connected`.
- **35 unit tests** on a modern xUnit v3 / MTP stack.

**Not yet built:**

- Any **real audio source** — only the test tone exists.
- **Authentication**, **persistence**, and streaming one source to **multiple agents** (fan-out).

## Planned work (with rationale)

| Area | Plan | Why / notes |
|------|------|-------------|
| **Front end** | Grow the SPA ([web.md](./web.md)) as new server features land; add auth once the server has it. | The base dashboard, routing editor and stream controls are built against the [api.md](./api.md) contract. |
| **Real audio sources** | File playback (WAV/MP3), system/WASAPI **loopback** capture, line-in/mic. | Replaces the test tone; the OCAP header already carries format so the pipeline is source-agnostic. |
| **Compression (Opus)** | Optional Opus encoding via **Concentus** (pure-managed). | Raw PCM is ~1.5 Mbps/stream; Opus cuts bandwidth ~10–20×. Would be a new OCAP encoding + a decode step on the agent. |
| **Authentication** | Real auth on the server; drop the blanket `AllowAnonymous()`; tighten the allow-any-origin CORS policy to an explicit allow-list. | FastEndpoints is secure-by-default; every endpoint currently opts out, and CORS currently reflects any origin for the front end. See [server.md](./server.md#fastendpoints-repr). |
| **Concurrency / fan-out** | Stream one source to **many agents**, incl. **to every device in a zone** at once; per-agent/zone volume/grouping. | Multi-*device* fan-out within one agent is done, and **zone management** exists ([server.md](./server.md#zone-registry)); streaming to multiple agents / to a zone, and per-agent/zone volume, are not. |
| **Clock-drift handling** | Detect/compensate when a source's production rate drifts from the agent's playback rate. | A buffer that slowly drains despite good pacing is a *rate deficit*, which no fixed jitter buffer can fix; needs resampling or adaptive pacing. See [agent.md](./agent.md#playback-pipeline). |
| **Shared kernel** | Lift `AgentInfo`/`AgentStatus`/`IAgentRegistry` out of the Agents slice when a 2nd slice appears. | Removes the temporary `Infrastructure → slice` dependency ([architecture.md](./architecture.md#server-side-structure-vertical-slices--shared-infrastructure)). |
| **Code coverage** | Wire up MTP's `Microsoft.Testing.Extensions.CodeCoverage` (`dotnet test --coverage`). | The old coverlet collector doesn't apply under MTP ([development.md](./development.md#code-coverage-not-yet-wired)). |
| **Persistence** | Persist the agent registry (it's in-memory today). | Registry is empty on restart. |

## Decisions already made (and why)

- **Agent listens for audio; agent dials the server for control.** The server dials the agent to stream audio, while the agent opens an outbound SignalR *control* connection to self-register and receive routing — so the server learns the agent's audio host from that connection (no manual registration). ([architecture.md](./architecture.md#the-connection-model-important))
- **Server-driven routing, applied next stream, in-memory.** The server is the source of truth for each agent's channel→device routing and pushes it over the control connection; it takes effect on the agent's next stream. Desired routing (and the registry) are in-memory and reset on server restart. ([server.md](./server.md#agent-control-plane))
- **Raw PCM for v1** (no codec). Trivially correct; compression is a later, additive OCAP version.
- **`WaveOut` (WinMM) over WASAPI for v1.** Simplest reliable playback path; WASAPI/exclusive-mode is a later option if lower latency is needed.
- **Pre-roll jitter buffer (400 ms)** + real-time stopwatch pacing on the server. Chosen after observing that starting on an empty buffer, or pacing with a fixed `Task.Delay`, produced audible stutter. ([agent.md](./agent.md#playback-pipeline), [server.md](./server.md#tone-streaming))
- **Multi-device output is WASAPI, drift-tolerant, server-routed.** The channel→device map is set from the server (see above); independent device clocks are accepted (separate zones), not phase-locked. ([agent.md](./agent.md#multiple-output-devices))
- **Vertical slices + FastEndpoints (REPR).** Feature-first structure; one class per endpoint.
- **xUnit v3 + MTP and AwesomeAssertions.** Modern runner; AwesomeAssertions avoids FluentAssertions v8's commercial licence. ([development.md](./development.md#notable-packagetooling-decisions-and-why))

## Documentation vs. the root README

The root [`README.md`](../README.md) still describes the **long-term vision** — a fully-configurable whole-home audio broadcasting product with many input sources, audio-manipulation components (merging/splitting, ducking, EQ, fades), playlists, and a web dashboard. That remains the direction, but the current code is the much smaller prototype documented here. Its badges also reference a removed `Oceana.Core.csproj` and should be refreshed. Reconciling the README with reality (and updating badges) is a small pending cleanup.
