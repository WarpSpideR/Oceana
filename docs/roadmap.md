# Roadmap & Status

> What Oceana does today, what's planned, and the reasoning behind the key decisions made so far. This is the honest gap between the current prototype and the wider vision.

## Where things stand

**Implemented (working, tested):**

- **OCAP protocol** — fixed 16-byte handshake header + raw-PCM streaming ([protocol.md](./protocol.md)).
- **Windows agent** — TCP listener that de-interleaves an N-channel stream and plays channel subsets across **one or more output devices** (WASAPI), with a pre-roll jitter buffer and backpressure ([agent.md](./agent.md)).
- **Server** — FastEndpoints Web API with an in-memory agent registry, SignalR status broadcasting, OpenAPI, and CORS; streams a **generated sine test tone** to agents with real-time pacing ([server.md](./server.md), [api.md](./api.md)).
- **Server control plane** — agents **self-register** over a persistent SignalR connection (`/hubs/agents-control`) reporting their devices; the server pushes **channel→device routing**, applied on the agent's next stream ([server.md](./server.md#agent-control-plane), [agent.md](./agent.md#server-control-connection)).
- **React front end** (`Oceana.Web`) — a Vite + TypeScript + MUI SPA consuming the REST API and the `/hubs/agents` + `/hubs/zones` status hubs: a live agent dashboard, per-agent channel→device routing editor, and test-tone stream start/stop ([web.md](./web.md)).
- **Zones (management)** — a `Features/Zones` slice + UI to create/rename/delete named zones and assign devices from any agent(s), kept live over `/hubs/zones`. In-memory, unique names ([server.md](./server.md#zone-registry), [api.md](./api.md#zones)).
- **Configuration persistence** — zones and agent configuration (identity, last-seen devices, desired routing) persist to JSON (`data/*.json`) and survive a restart; live state is re-derived on reconnect ([server.md](./server.md#persistence)).
- **Per-zone volume** — a persisted volume (0–100%, attenuation) on each zone, applied server-side in the stream pump and adjustable **live** during playback ([server.md](./server.md#per-zone-volume), [api.md](./api.md#zones)).
- **Zone playback** — play an audio **file** to a zone (stereo) with **start/stop and now-playing**, kept live over `/hubs/zones`. The browser decodes/resamples to 48 kHz (any common format) and uploads PCM; the server streams it play-once to every reachable device, best-effort ([server.md](./server.md#zone-playback), [api.md](./api.md#play-audio-to-a-zone), [web.md](./web.md#playing-a-file-to-a-zone)).
- **Zone broadcast** — record a message from the browser microphone and play it on every reachable device in a zone. The **first real audio source** and the first **fan-out**: the server decodes the uploaded WAV, and for each agent pushes routing to the zone's devices, streams the mono PCM (best-effort, skips offline/busy agents), then restores routing ([server.md](./server.md#zone-broadcast), [api.md](./api.md#broadcast-a-recorded-message), [web.md](./web.md#broadcasting-a-message)).
- **Verified end-to-end**: agent self-registers with devices → push routing → 4-channel stream splits across two devices per the server config → disconnect flips `connected`.
- **35 unit tests** on a modern xUnit v3 / MTP stack.

**Not yet built:**

- Broader **real audio sources** — a recorded mic message now works (zone broadcast); file playback, loopback capture and line-in are still to come.
- **Authentication**, **persistence**, and streaming one source to **multiple agents** (fan-out).

## Planned work (with rationale)

| Area | Plan | Why / notes |
|------|------|-------------|
| **Front end** | Grow the SPA ([web.md](./web.md)) as new server features land; add auth once the server has it. | The base dashboard, routing editor and stream controls are built against the [api.md](./api.md) contract. |
| **Real audio sources** | System/WASAPI **loopback** capture, line-in, and a reusable server-side **media library**. | Recorded **mic** (broadcast) and **file playback** to a zone both work; the browser decodes/resamples any common format, so remaining sources (live capture) and a library are the next steps. |
| **Compression (Opus)** | Optional Opus encoding via **Concentus** (pure-managed). | Raw PCM is ~1.5 Mbps/stream; Opus cuts bandwidth ~10–20×. Would be a new OCAP encoding + a decode step on the agent. |
| **Authentication** | Real auth on the server; drop the blanket `AllowAnonymous()`; tighten the allow-any-origin CORS policy to an explicit allow-list. | FastEndpoints is secure-by-default; every endpoint currently opts out, and CORS currently reflects any origin for the front end. See [server.md](./server.md#fastendpoints-repr). |
| **Concurrency / fan-out** | Continuous streaming to **many agents** at once; per-device volume/trim and grouping. | Zone **broadcast** + **file playback** fan out to every reachable agent in a zone, and **per-zone volume** is done ([server.md](./server.md#per-zone-volume)); a continuous *live* source to many agents, and per-device volume, are not. |
| **Clock-drift handling** | Detect/compensate when a source's production rate drifts from the agent's playback rate. | A buffer that slowly drains despite good pacing is a *rate deficit*, which no fixed jitter buffer can fix; needs resampling or adaptive pacing. See [agent.md](./agent.md#playback-pipeline). |
| **Shared kernel** | Lift `AgentInfo`/`AgentStatus`/`IAgentRegistry` out of the Agents slice when a 2nd slice appears. | Removes the temporary `Infrastructure → slice` dependency ([architecture.md](./architecture.md#server-side-structure-vertical-slices--shared-infrastructure)). |
| **Code coverage** | Wire up MTP's `Microsoft.Testing.Extensions.CodeCoverage` (`dotnet test --coverage`). | The old coverlet collector doesn't apply under MTP ([development.md](./development.md#code-coverage-not-yet-wired)). |
| **Persistence** | *Configuration* (zones + agent config) now persists to JSON ([server.md](./server.md#persistence)). Remaining: a real datastore (e.g. SQLite) if the data model grows, and persisting more than config. | Done for config; live state is intentionally not persisted. |

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
