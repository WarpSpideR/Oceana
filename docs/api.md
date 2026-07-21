# API Reference

> The HTTP + SignalR contract exposed by [`Oceana.Server`](./server.md) — what the (planned) React front end, or any client, consumes. All routes are under the global prefix **`/api`**; the SignalR hub is at **`/hubs/agents`**.

## Base URLs & CORS

- Dev server (launch profile): `http://localhost:50858` (HTTP) / `https://localhost:50857` (HTTPS). Running the DLL directly uses `http://localhost:5069`. See the [ports note](./server.md#ports-note-the-discrepancy).
- **CORS** allows origins `http://localhost:5173` and `http://localhost:3000` by default (config key `Cors:AllowedOrigins`), with credentials — sufficient for a Vite/CRA dev front end and SignalR.
- **Auth:** none currently (all endpoints are anonymous).
- **Enums** are serialised as **strings** (e.g. `"Idle"`, not `0`) in both REST and SignalR payloads.

## REST endpoints

| # | Verb & route | Body | Success | Errors |
|--:|--------------|------|---------|--------|
| 1 | `GET /api/agents` | — | `200` `AgentInfo[]` | — |
| 2 | `GET /api/agents/{id}` | — | `200` `AgentInfo` | `404` unknown id |
| 3 | `POST /api/agents` | `RegisterAgentRequest` | `201` `AgentInfo` (+ `Location` → endpoint 2) | `400` validation |
| 4 | `DELETE /api/agents/{id}` | — | `204` (stops any stream first) | `404` unknown id |
| 5 | `POST /api/agents/{id}/stream` | `StartStreamRequest` | `202` `StartStreamResponse` | `404` unknown id · `409` already streaming · `400` validation |
| 6 | `DELETE /api/agents/{id}/stream` | — | `204` | `404` no active stream |

On register/status-change/remove, the server also broadcasts over SignalR (see below).

## Data shapes

### `AgentInfo`
```json
{
  "id": "81279755-3981-40cd-add2-23fa2bfde58b",
  "name": "living-room",
  "host": "127.0.0.1",
  "port": 8090,
  "status": "Idle",
  "lastError": null
}
```
`status` ∈ `"Idle" | "Connecting" | "Streaming" | "Faulted"`. `lastError` is a string when `status` is `"Faulted"`, otherwise `null`.

### `RegisterAgentRequest`
```json
{ "name": "living-room", "host": "127.0.0.1", "port": 8090 }
```
Validation: `name` and `host` non-empty; `port` in `1..65535` (defaults to `8090`).

### `StartStreamRequest`
```json
{ "frequency": 440, "durationSeconds": 5 }
```
`frequency` (Hz) in `20..20000` (default `440`); `durationSeconds` optional — omit/`null` to play until stopped, otherwise `0.1..3600`. `id` comes from the route.

### `StartStreamResponse`
```json
{ "agentId": "81279755-3981-40cd-add2-23fa2bfde58b", "frequency": 440 }
```

## Example flow

```bash
# 1. register an agent
curl -X POST http://localhost:5069/api/agents \
  -H "Content-Type: application/json" \
  -d '{"name":"local-agent","host":"127.0.0.1","port":8090}'
# → 201 { "id":"…","status":"Idle", … }

# 2. start a 5-second 440 Hz tone
curl -X POST http://localhost:5069/api/agents/{id}/stream \
  -H "Content-Type: application/json" \
  -d '{"frequency":440,"durationSeconds":5}'
# → 202 { "agentId":"…","frequency":440 }   (status → Streaming, then Idle after 5s)

# 3. stop early / clean up
curl -X DELETE http://localhost:5069/api/agents/{id}/stream   # → 204 (or 404 if none active)
curl -X DELETE http://localhost:5069/api/agents/{id}          # → 204
```

## SignalR hub — `/hubs/agents`

Strongly-typed hub ([`AgentStatusHub`](../src/Oceana.Server/Infrastructure/Realtime/AgentStatusHub.cs)) that **pushes** agent changes to connected clients. The client implements two methods:

| Method | Payload | Fired when |
|--------|---------|-----------|
| `AgentChanged` | `AgentInfo` | An agent is registered or its status changes (Connecting/Streaming/Idle/Faulted). |
| `AgentRemoved` | `agentId` (`Guid` string) | An agent is removed from the registry. |

Example client (TypeScript, `@microsoft/signalr`):

```ts
const conn = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5069/hubs/agents")
  .withAutomaticReconnect()
  .build();

conn.on("AgentChanged", (agent: AgentInfo) => { /* upsert into UI state */ });
conn.on("AgentRemoved", (agentId: string) => { /* remove from UI state */ });

await conn.start();
```

> There is no client→server hub method today; the hub is push-only. A typical front end loads the initial list via `GET /api/agents`, then keeps it live via these two events.

## OpenAPI

In the Development environment the OpenAPI document is served at **`/openapi/v1.json`** (document name `v1`, title "Oceana Server API"). Useful for generating a typed client for the front end.
