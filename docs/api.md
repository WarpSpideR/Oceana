# API Reference

> The HTTP + SignalR contract exposed by [`Oceana.Server`](./server.md) — what the (planned) React front end, or any client, consumes. REST routes are under the global prefix **`/api`**; there are two SignalR hubs — **`/hubs/agents`** (front-end status) and **`/hubs/agents-control`** (agents connect here; not for the front end).

## Base URLs & CORS

- Dev server (launch profile): `http://localhost:50858` (HTTP) / `https://localhost:50857` (HTTPS). Running the DLL directly (or with `ASPNETCORE_URLS`) uses `http://localhost:5069`. See the [ports note](./server.md#ports-note-the-discrepancy).
- **CORS** allows origins `http://localhost:5173` and `http://localhost:3000` by default (config key `Cors:AllowedOrigins`), with credentials — for the front-end REST + `/hubs/agents`. (The agent control hub isn't browser-facing, so CORS doesn't apply to it.)
- **Auth:** none currently (all endpoints and hubs are anonymous).
- **Enums** are serialised as **strings** (e.g. `"Idle"`, not `0`) in both REST and SignalR payloads.

## Agents self-register

There is **no "create agent" endpoint**. Agents open a persistent SignalR connection to `/hubs/agents-control` on startup and register themselves; they then appear in `GET /api/agents` with `connected: true` and their reported devices. The REST surface below manages and drives already-registered agents.

## REST endpoints

| # | Verb & route | Body | Success | Errors |
|--:|--------------|------|---------|--------|
| 1 | `GET /api/agents` | — | `200` `AgentInfo[]` | — |
| 2 | `GET /api/agents/{id}` | — | `200` `AgentInfo` | `404` unknown id |
| 3 | `PUT /api/agents/{id}/routing` | `AgentRouting` | `200` `AgentInfo` (stores + pushes to the agent) | `404` unknown id · `400` validation |
| 4 | `DELETE /api/agents/{id}` | — | `204` (stops any stream first) | `404` unknown id |
| 5 | `POST /api/agents/{id}/stream` | `StartStreamRequest` | `202` `StartStreamResponse` | `404` unknown id · `409` already streaming · `400` validation |
| 6 | `DELETE /api/agents/{id}/stream` | — | `204` | `404` no active stream |

On register/status-change/routing-change/remove, the server broadcasts over `/hubs/agents` (see below).

## Data shapes

### `AgentInfo`
```json
{
  "id": "44e23759-1a28-4dc3-99eb-8cda11c08693",
  "name": "living-room",
  "host": "127.0.0.1",
  "port": 8090,
  "connected": true,
  "status": "Idle",
  "lastError": null,
  "devices": [ { "id": "{0.0.0.00000000}.{01202ecf-…}", "name": "Speakers (High Definition Audio Device)" } ],
  "routing": { "outputs": [ { "device": "Speakers (High Definition Audio Device)", "channels": [0, 1] } ] }
}
```
`connected` reflects the control connection. `status` ∈ `"Idle" | "Connecting" | "Streaming" | "Faulted"`; `lastError` is set only when `Faulted`. `host` is derived from the control connection; `port` is the audio port the agent reported. `devices` and `routing` are populated from the agent.

### `AgentRouting` (request body of endpoint 3)
```json
{ "outputs": [
  { "device": "Speakers (High Definition Audio Device)", "channels": [0, 1] },
  { "device": "5 - LG HDR 4K (AMD High Definition Audio Device)", "channels": [2, 3] }
] }
```
Each `device` is a WASAPI friendly name or id (from `AgentInfo.devices`); `null`/omitted selects the agent's default device. `channels` is the ordered list of source channels for that device. An **empty `outputs`** means "play all channels to the default device". Validation: every output must list at least one non-negative channel. The change applies to the agent's **next stream**.

### `StartStreamRequest`
```json
{ "frequency": 440, "channels": 4, "durationSeconds": 5 }
```
`frequency` (Hz) in `20..20000` (default `440`); `channels` in `1..8` (default `2`) — each channel is a distinct multiple of `frequency`; `durationSeconds` optional — omit/`null` to play until stopped, otherwise `0.1..3600`. `id` comes from the route.

### `StartStreamResponse`
```json
{ "agentId": "44e23759-1a28-4dc3-99eb-8cda11c08693", "frequency": 440 }
```

## Example flow

```bash
# 1. The agent self-registers on startup — it just appears:
curl http://localhost:5069/api/agents            # → [ { "id":"…","connected":true,"devices":[…] } ]

# 2. Route channels 0,1 to one device and 2,3 to another:
curl -X PUT http://localhost:5069/api/agents/{id}/routing -H "Content-Type: application/json" \
  -d '{"outputs":[{"device":"Speakers (…)","channels":[0,1]},{"device":"5 - LG HDR 4K (…)","channels":[2,3]}]}'
# → 200 (routing stored and pushed to the agent)

# 3. Stream a 6-second, 4-channel tone (applies the routing above):
curl -X POST http://localhost:5069/api/agents/{id}/stream -H "Content-Type: application/json" \
  -d '{"frequency":220,"channels":4,"durationSeconds":6}'
# → 202

# 4. Stop / clean up:
curl -X DELETE http://localhost:5069/api/agents/{id}/stream   # → 204 (or 404 if none active)
curl -X DELETE http://localhost:5069/api/agents/{id}          # → 204
```

## SignalR — front-end status hub `/hubs/agents`

Strongly-typed hub ([`AgentStatusHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentStatusHub.cs)) that **pushes** agent changes to connected front ends:

| Method | Payload | Fired when |
|--------|---------|-----------|
| `AgentChanged` | `AgentInfo` | An agent registers, connects/disconnects, changes streaming status, or its routing changes. |
| `AgentRemoved` | `agentId` (`Guid` string) | An agent is removed from the registry. |

```ts
const conn = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5069/hubs/agents").withAutomaticReconnect().build();
conn.on("AgentChanged", (agent) => { /* upsert */ });
conn.on("AgentRemoved", (agentId) => { /* remove */ });
await conn.start();
```

A front end loads the initial list via `GET /api/agents`, then keeps it live via these events. The hub is push-only (no client→server methods).

## SignalR — agent control hub `/hubs/agents-control`

Agents (not the front end) connect here. Agent → server: `Register(AgentRegistration)` returns the agent's stored `AgentRouting`. Server → agent: `SetRouting(AgentRouting)`. See [server.md](./server.md#agent-control-plane) and [agent.md](./agent.md#server-control-connection) for details.

## OpenAPI

In Development the OpenAPI document is served at **`/openapi/v1.json`** (document name `v1`, title "Oceana Server API") — useful for generating a typed front-end client.
