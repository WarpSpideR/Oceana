# Oceana Documentation

> The single entry point for Oceana's documentation. Start here, then follow the links below.

## What Oceana is (today)

Oceana is an **audio broadcasting system**: a central **server** streams audio out over the network to one or more **agents**, and each agent plays that audio through the speakers of the machine it runs on.

The current codebase is an early, working **prototype** of that pipeline:

- **`Oceana.Protocol`** — a tiny shared library defining the "OCAP" wire format used between server and agent.
- **`Oceana.Agent.Windows`** — a Windows console app that listens for a connection and plays the incoming audio to the default output device (via NAudio).
- **`Oceana.Server`** — an ASP.NET Core Web API (FastEndpoints + SignalR) that manages a registry of agents and streams a **generated sine test tone** to them on demand.
- **`Oceana.Web`** — a **React + MUI single-page app** (Vite + TypeScript) that consumes the server's REST + SignalR surface: a live agent dashboard with per-agent routing and test-tone stream controls.

> **Current state vs. the vision.** The root [`README.md`](../README.md) describes a broad, aspirational whole-home-audio product (many input sources, playlists, EQ, dashboards). That is the long-term goal, **not** what the code does today. This `docs/` folder documents the software **as it actually exists**. See [roadmap.md](./roadmap.md) for the gap between the two.

## Table of contents

| Document | What it covers |
|----------|----------------|
| [architecture.md](./architecture.md) | The big picture: components, how server and agent connect, end-to-end data flow, diagrams, glossary. |
| [protocol.md](./protocol.md) | The OCAP wire format — the 16-byte handshake header and the raw-PCM stream that follows. |
| [agent.md](./agent.md) | The Windows playback agent: listener, playback pipeline, jitter buffering, configuration. |
| [server.md](./server.md) | The Web API server: vertical-slice architecture, agent registry, tone streaming, SignalR. |
| [api.md](./api.md) | The consumer-facing contract: REST endpoints and the SignalR hub (what the front end consumes). |
| [web.md](./web.md) | The React + MUI front end: stack, vertical-slice structure, dev/build scripts, server connectivity. |
| [development.md](./development.md) | Build, run, and test the solution; code-style rules; notable package/tooling decisions. |
| [roadmap.md](./roadmap.md) | What's implemented vs. planned, and the rationale behind the key decisions made so far. |

## Solution at a glance

| Project | Type | Target framework |
|---------|------|------------------|
| [`Oceana.Protocol`](../src/Oceana.Protocol) | Class library | `net10.0` |
| [`Oceana.Agent.Windows`](../src/Oceana.Agent.Windows) | Console app | `net10.0-windows10.0.19041.0` |
| [`Oceana.Server`](../src/Oceana.Server) | ASP.NET Core Web API | `net10.0` |
| [`Oceana.Web`](../src/Oceana.Web) | React SPA (Vite + TypeScript) | Node / npm (not MSBuild) |

The .NET projects target **.NET 10**; the front end is a standalone npm/Vite project (deliberately kept out of the solution — see [web.md](./web.md)). Build and test instructions are in [development.md](./development.md).
