# Front end — `Oceana.Web`

> A React single-page app that drives the [server](./server.md)'s REST API and subscribes to its SignalR status hubs for live updates. It's the browser-facing control plane: a live **agent** dashboard (per-agent channel→device routing editor and test-tone stream controls) and a **zone** manager (create/rename/delete zones and assign devices from any agents). For the wire contract it consumes, see [api.md](./api.md).

## Stack

- **Vite + React + TypeScript** — fast dev server and build; strict TS (`verbatimModuleSyntax`, `erasableSyntaxOnly`, so `import type` for types and no `enum`s).
- **MUI (Material UI)** — component library and theming (`@mui/material`, `@mui/icons-material`). MIT packages only; **no MUI X Pro/Premium**.
- **TanStack Query** — server-state cache for all REST calls; the SignalR hub writes live updates straight into its cache.
- **`@microsoft/signalr`** — client for the `/hubs/agents` status hub.
- **react-router-dom** — routing (agent list, agent detail).
- **Vitest + React Testing Library** — unit/component tests; **oxlint** for linting.

All dependencies are free/OSS, in line with the repo's [conventions](../CLAUDE.md).

## Structure: vertical slices

Mirrors the server's feature-first layout ([architecture.md](./architecture.md#server-side-structure-vertical-slices--shared-infrastructure)). Each slice owns its api, types, hooks, components and pages; `shared/` is slice-agnostic; `app/` is the composition root.

```
src/Oceana.Web/src/
├── app/                    # composition root
│   ├── main.tsx            # entry: Providers + RouterProvider
│   ├── Providers.tsx       # QueryClient + MUI theme + live-updates hub
│   ├── router.tsx          # routes
│   ├── App.tsx             # app-bar layout + connection banner
│   ├── theme.ts, queryClient.ts, hubStatusContext.ts
├── shared/                 # cross-cutting, slice-agnostic
│   ├── api/                # http.ts (fetch wrapper + ApiError), config.ts (base URL)
│   ├── realtime/           # createHubConnection.ts (SignalR factory)
│   └── components/         # ConnectionBanner, ConfirmDialog
└── features/
    ├── agents/             # the Agents slice
    │   ├── types.ts        # AgentInfo, AgentRouting, … (mirror the server DTOs)
    │   ├── api/            # agentsApi.ts (REST calls), agentKeys.ts (query keys)
    │   ├── hooks/          # useAgents, useAgent, useAgentMutations, useAgentHub, agentCache
    │   ├── components/     # AgentStatusChip, AgentsTable, RoutingEditor, StartStreamDialog
    │   ├── routingModel.ts # pure routing parse/validate/build helpers
    │   └── pages/          # AgentsListPage, AgentDetailPage
    └── zones/              # the Zones slice
        ├── types.ts        # ZoneInfo, ZoneDevice, BroadcastResult
        ├── api/            # zonesApi.ts (CRUD + broadcastToZone), zoneKeys.ts
        ├── hooks/          # useZones, useZone, useZoneMutations (+ useBroadcastToZone), useZoneHub, zoneCache
        ├── audio/          # wav.ts (WAV encoder), useAudioRecorder (mic capture)
        ├── components/     # ZonesTable, CreateZoneDialog, DevicePicker, BroadcastDialog
        ├── zoneModel.ts    # pure device-selection + resolve-against-agents helpers
        └── pages/          # ZonesListPage, ZoneDetailPage
```

The app bar switches between the **Agents** and **Zones** sections; `app/Providers.tsx` runs both the agent and zone hub hooks and feeds a combined status to the connection banner.

### Broadcasting a message

The zone detail page has a **Broadcast a message** card (enabled only when a zone device's agent is connected). `BroadcastDialog` uses `useAudioRecorder` — `getUserMedia` + `MediaRecorder` to capture (≤ 60 s), an `<audio>` element for preview, and re-record — then, on broadcast, `getWavBlob()` decodes the recording, down-mixes to mono and resamples to **48 kHz** via `OfflineAudioContext`, and `wav.ts` encodes a 16-bit PCM WAV. That blob is `POST`ed as `audio/wav` to `/api/zones/{id}/broadcast` (the explicit `Content-Type` is why binary upload needs no change to the shared `request` helper). The dialog shows the returned summary (agents played to / skipped). All browser media APIs live in the recorder hook so the rest of the UI stays testable; `wav.ts` is unit-tested directly.

### Data flow

Each slice seeds its TanStack Query cache from REST (`GET /api/agents`, `GET /api/zones`) and then treats its SignalR hook (`useAgentHub` / `useZoneHub`) as the live source of truth — `*Changed` upserts and `*Removed` deletes are applied directly to the cache (`agentCache.ts` / `zoneCache.ts`). Because the server broadcasts a `*Changed` event after every mutation, mutations don't need optimistic updates; they invalidate as a safety net. Zone device assignments store only `(agentId, deviceId)`; the UI resolves display names and availability against the agents cache (`zoneModel.resolveZoneDevices`).

## Scripts

Run from `src/Oceana.Web/` (requires Node + npm; **not** part of `Oceana.sln` or `dotnet` — see below):

```bash
npm install          # restore dependencies
npm run dev          # Vite dev server on http://localhost:5173
npm run build        # type-check (tsc -b) + production build to dist/
npm run lint         # oxlint
npm test             # Vitest (run once); npm run test:watch to watch
```

## Server connectivity

The SPA calls the server **directly** and relies on the server's permissive CORS policy (any origin) — there's no dev proxy. The base URL comes from `VITE_API_BASE_URL` (see [`.env.example`](../src/Oceana.Web/.env.example)); copy it to `.env.local` to override.

`appsettings.json` pins Kestrel to `http://localhost:5069`, and that takes precedence over the launch profile's `applicationUrl` — so the server listens on **5069 whether it's started via `dotnet run` or as the built DLL**, and the default `VITE_API_BASE_URL` works out of the box. (The launch profile also exposes HTTPS on `https://localhost:50857`; point `VITE_API_BASE_URL` there if you prefer HTTPS. See the server's [ports note](./server.md#ports-note-the-discrepancy).)

## Why it's not in the solution

`src/Directory.Build.props` applies StyleCop, `TreatWarningsAsErrors` and `GenerateDocumentationFile` to **every MSBuild project under `src/`** — which would break a JS build. So `Oceana.Web` is deliberately a standalone npm/Vite project and is **not** added to `Oceana.sln` as a buildable project. It has its own `.gitignore` (covers `node_modules`, `dist`, `coverage`); `src/.editorconfig` has a scoped block giving its files 2-space indent and plain UTF-8.

## Running end to end

1. Start the [server](./server.md): `dotnet run --project src/Oceana.Server` (listens on `http://localhost:5069`).
2. `npm run dev` — the default `VITE_API_BASE_URL` (5069) already matches, so no override is needed.
3. Open `http://localhost:5173`. Agents [self-register](./api.md#agents-self-register); start an [agent](./agent.md) and it appears live, ready for routing and streaming. Audio playback itself needs a Windows agent with real output devices.
