# Roadmap & Status

> What Oceana does today, what's planned, and the reasoning behind the key decisions made so far. This is the honest gap between the current prototype and the wider vision.

## Where things stand

**Implemented (working, tested):**

- **OCAP protocol** — fixed 16-byte handshake header + raw-PCM streaming ([protocol.md](./protocol.md)).
- **Windows agent** — TCP listener that de-interleaves an N-channel stream and plays channel subsets across **one or more output devices** (WASAPI), with a pre-roll jitter buffer and backpressure ([agent.md](./agent.md)).
- **Server** — FastEndpoints Web API with an in-memory agent registry, SignalR status broadcasting, OpenAPI, and CORS; streams a **generated sine test tone** to agents with real-time pacing ([server.md](./server.md), [api.md](./api.md)).
- **Verified end-to-end**: register an agent → start a stream → the agent plays it with a healthy buffer → stop returns to idle.
- **21 unit tests** on a modern xUnit v3 / MTP stack.

**Not yet built:**

- The **React front end** (the server's REST + SignalR surface is ready for it).
- Any **real audio source** — only the test tone exists.
- **Authentication**, **persistence**, and streaming one source to **multiple agents** (fan-out).

## Planned work (with rationale)

| Area | Plan | Why / notes |
|------|------|-------------|
| **Front end** | React app consuming the REST API + SignalR hub. | The contract in [api.md](./api.md) was built for this. |
| **Real audio sources** | File playback (WAV/MP3), system/WASAPI **loopback** capture, line-in/mic. | Replaces the test tone; the OCAP header already carries format so the pipeline is source-agnostic. |
| **Compression (Opus)** | Optional Opus encoding via **Concentus** (pure-managed). | Raw PCM is ~1.5 Mbps/stream; Opus cuts bandwidth ~10–20×. Would be a new OCAP encoding + a decode step on the agent. |
| **Authentication** | Real auth on the server; drop the blanket `AllowAnonymous()`. | FastEndpoints is secure-by-default; every endpoint currently opts out. See [server.md](./server.md#fastendpoints-repr). |
| **Concurrency / fan-out** | Stream one source to **many agents**; per-agent volume/grouping. | Multi-*device* fan-out within one agent is done; streaming one source to multiple agents, and per-agent volume/grouping, are not. |
| **Clock-drift handling** | Detect/compensate when a source's production rate drifts from the agent's playback rate. | A buffer that slowly drains despite good pacing is a *rate deficit*, which no fixed jitter buffer can fix; needs resampling or adaptive pacing. See [agent.md](./agent.md#playback-pipeline). |
| **Shared kernel** | Lift `AgentInfo`/`AgentStatus`/`IAgentRegistry` out of the Agents slice when a 2nd slice appears. | Removes the temporary `Infrastructure → slice` dependency ([architecture.md](./architecture.md#server-side-structure-vertical-slices--shared-infrastructure)). |
| **Code coverage** | Wire up MTP's `Microsoft.Testing.Extensions.CodeCoverage` (`dotnet test --coverage`). | The old coverlet collector doesn't apply under MTP ([development.md](./development.md#code-coverage-not-yet-wired)). |
| **Persistence** | Persist the agent registry (it's in-memory today). | Registry is empty on restart. |

## Decisions already made (and why)

- **Agent listens, server dials out.** Keeps the agent simple and firewall-friendly; the server owns when audio flows. Trade-off (server must know agent addresses) is handled by the API-managed registry. ([architecture.md](./architecture.md#the-connection-model-important))
- **Raw PCM for v1** (no codec). Trivially correct; compression is a later, additive OCAP version.
- **`WaveOut` (WinMM) over WASAPI for v1.** Simplest reliable playback path; WASAPI/exclusive-mode is a later option if lower latency is needed.
- **Pre-roll jitter buffer (400 ms)** + real-time stopwatch pacing on the server. Chosen after observing that starting on an empty buffer, or pacing with a fixed `Task.Delay`, produced audible stutter. ([agent.md](./agent.md#playback-pipeline), [server.md](./server.md#tone-streaming))
- **Multi-device output is agent-configured, WASAPI, drift-tolerant.** The agent maps its local devices to channel ranges in `appsettings.json` (server just sends N channels); independent device clocks are accepted (separate zones), not phase-locked. ([agent.md](./agent.md#multiple-output-devices))
- **Vertical slices + FastEndpoints (REPR).** Feature-first structure; one class per endpoint.
- **xUnit v3 + MTP and AwesomeAssertions.** Modern runner; AwesomeAssertions avoids FluentAssertions v8's commercial licence. ([development.md](./development.md#notable-packagetooling-decisions-and-why))

## Documentation vs. the root README

The root [`README.md`](../README.md) still describes the **long-term vision** — a fully-configurable whole-home audio broadcasting product with many input sources, audio-manipulation components (merging/splitting, ducking, EQ, fades), playlists, and a web dashboard. That remains the direction, but the current code is the much smaller prototype documented here. Its badges also reference a removed `Oceana.Core.csproj` and should be refreshed. Reconciling the README with reality (and updating badges) is a small pending cleanup.
