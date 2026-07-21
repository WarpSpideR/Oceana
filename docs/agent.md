# Agent — `Oceana.Agent.Windows`

> A Windows console app that waits for a connection from the server, then plays the incoming audio — routing channels to one or more output devices — through the machine's speakers. Built on [NAudio](https://github.com/naudio/NAudio) 3.0 (WASAPI).

## Responsibilities

1. Listen on TCP for an incoming connection from the server.
2. Read the [OCAP header](./protocol.md), then continuously read raw PCM.
3. De-interleave the channels and play each configured subset to its **output device**, smoothly (buffering to absorb network jitter). With no configuration, the whole stream plays to the **default** device.
4. Handle disconnects and graceful shutdown; then wait for the next connection.

## Target framework — and why it's so specific

`net10.0-windows10.0.19041.0` (see [`Oceana.Agent.Windows.csproj`](../src/Oceana.Agent.Windows/Oceana.Agent.Windows.csproj)).

> **Why the `10.0.19041` floor?** NAudio 3.0 splits its Windows audio backends (WinMM, WASAPI, ASIO) into a dependency group that targets `net*-windows10.0.19041`. A plain `net10.0-windows` (which defaults to a Windows 7 baseline) would silently resolve to the *stripped* group — you'd lose WASAPI/WinMM entirely and only get the core DSP types. The `.19041` floor is what pulls in the actual playback stack.

## Networking — the listener

[`AudioAgentListener`](../src/Oceana.Agent.Windows/Networking/AudioAgentListener.cs):

- Binds **`IPAddress.Any`** on the configured port (all interfaces, so a server on another machine can connect).
- Accepts and handles **one connection at a time** (a sequential accept → play → loop). A second server connecting while one is active simply waits in the TCP backlog.
- Per-connection socket tuning: `NoDelay = true` (disables Nagle, so the server's writes aren't coalesced into laggy bursts) and `ReceiveBufferSize = 262144` (256 KB, giving the kernel room to smooth bursty delivery).
- Resilient: a failure on one connection is logged and the listener keeps accepting; cancellation stops it cleanly.

## Playback pipeline

[`AudioPlaybackSession`](../src/Oceana.Agent.Windows/Networking/AudioPlaybackSession.cs) handles a single connection:

1. **Read the header** → the source `WaveFormat` (N channels, sample rate, bit depth).
2. **Build the output plan** from configuration (see [Multiple output devices](#multiple-output-devices)). With no config, one output plays all channels to the default device.
3. For each output, create a `BufferedWaveProvider` (in that output's channel-subset format) with **2 seconds** capacity (`PlaybackBufferSeconds = 2`) and an [`IAudioPlayer`](../src/Oceana.Agent.Windows/Playback/IAudioPlayer.cs) bound to its device.
4. **Pre-roll:** pump the network into the buffers until **400 ms** is buffered (`PreRollDuration`), *then* start all players together. (If the stream ends before 400 ms, they start anyway.)
5. **Pump loop:** read ~8 KB at a time (`ReadBufferSize = 8192`); the [`ChannelRouter`](../src/Oceana.Agent.Windows/Playback/ChannelRouter.cs) de-interleaves each frame and appends every output's channels to its buffer.
6. **Backpressure:** if any buffer is near full, wait (`BackpressureDelay = 10 ms`) before reading more — TCP flow control then throttles the server.
7. **Observability:** every second (`BufferLogInterval`) it logs each output's buffer level; below **50 ms** (`LowWaterMilliseconds`) it warns of a possible underrun.

> **Why pre-roll + a jitter buffer?** Without it, playback starts on an empty buffer and runs permanently on the edge of starvation — any network hiccup or GC pause becomes an audible gap (a stutter), because NAudio's `BufferedWaveProvider` fills underruns with silence rather than stopping. The 400 ms cushion rides out that variance. A buffer that stays healthy with a well-paced producer but slowly drains with an under-feeding one indicates a *producer rate deficit* (clock drift), not a jitter problem — see [roadmap.md](./roadmap.md).

### Channel routing & de-interleaving

The incoming stream is one interleaved N-channel PCM stream, but NAudio has **no "tee"** — two devices cannot read one stream (reads are destructive). So [`ChannelRouter`](../src/Oceana.Agent.Windows/Playback/ChannelRouter.cs) reads the stream once, de-interleaves each frame, and copies each output's **ordered channel list** into that output's `BufferedWaveProvider` (carrying over a partial trailing frame between reads). Because the channel list is ordered, channels can be reordered or duplicated per output.

### Output devices (WASAPI)

[`WasapiAudioPlayer`](../src/Oceana.Agent.Windows/Playback/WasapiAudioPlayer.cs) plays one output to a specific device, built via NAudio's `WasapiPlayerBuilder` (shared mode, event-sync, 150 ms latency). Devices are resolved by friendly name or id through [`WasapiAudioDeviceResolver`](../src/Oceana.Agent.Windows/Playback/WasapiAudioDeviceResolver.cs) (`MMDeviceEnumerator`); a null/empty device selects the system default render endpoint. The WASAPI player adapts bit depth and channel count to the device's mix format automatically — no resampling, since the sample rate is unchanged.

## Multiple output devices

An agent can split one N-channel stream across several devices — e.g. receive 4 channels and play channels 0–1 on one device and 2–3 on another. **Routing is set from the server**, not locally: the front end / API calls `PUT /api/agents/{id}/routing`, the server stores it and pushes it to the agent over the [control connection](#server-control-connection), and the agent applies it on its **next stream**. Each output maps a device (WASAPI friendly name or id; omit for the default) to an ordered list of source channels; with no routing set, the whole stream plays to the default device. Run `--list-devices` to see the device names/ids the server offers.

> **Synchronisation caveat.** Each device runs on its **own clock**, so independent devices drift over time and start with a small skew — fine for separate *zones*, not for phase-locked same-room surround. With one shared source and global backpressure, the slowest device paces the read loop, so a much faster one can eventually underrun (silence-padded). True cross-device sync is out of scope (see [roadmap.md](./roadmap.md)).

## Server control connection

A server is **required**. On startup the agent opens a persistent SignalR connection ([`ServerControlConnection`](../src/Oceana.Agent.Windows/Networking/ServerControlConnection.cs)) to `{ServerUrl}/hubs/agents-control` and:

- **Self-registers** — sends an `AgentRegistration` (a stable, locally-persisted agent id in an `agent-id` file, the machine name, the audio port, and its render devices) and stores the routing the server returns.
- **Receives routing** — `On<AgentRouting>("SetRouting", …)` updates the thread-safe [`RoutingStore`](../src/Oceana.Agent.Windows/Playback/RoutingStore.cs); the listener reads `routingStore.Current` at the start of each connection (next-stream apply).
- **Reconnects** — a jittered initial-connect retry loop plus `WithAutomaticReconnect`, and it **re-registers on every reconnect** (rejoining its server-side group). Until the first registration completes, routing defaults to all-channels-to-default-device.

`ServerUrl` comes from `appsettings.json`; if it is missing the agent logs a fatal error and exits.

## The `IAudioPlayer` seam

Playback is abstracted behind [`IAudioPlayer`](../src/Oceana.Agent.Windows/Playback/IAudioPlayer.cs) + [`IAudioPlayerFactory`](../src/Oceana.Agent.Windows/Playback/IAudioPlayerFactory.cs) (`Create(deviceId)`), with WASAPI behind [`WasapiAudioPlayer`](../src/Oceana.Agent.Windows/Playback/WasapiAudioPlayer.cs) / [`WasapiAudioPlayerFactory`](../src/Oceana.Agent.Windows/Playback/WasapiAudioPlayerFactory.cs).

> **Why:** it lets the network + parsing + de-interleave + buffering logic be unit-tested with substitute players — **no sound card required** in tests. See [`AudioPlaybackSessionTests`](../tests/Oceana.Agent.Windows.Test/Networking/AudioPlaybackSessionTests.cs) and [`ChannelRouterTests`](../tests/Oceana.Agent.Windows.Test/Playback/ChannelRouterTests.cs).

## NAudio 3.0-preview notes

The project uses `NAudio` **3.0.0-preview.18**, which differs from 2.x in ways that bit us:

- `WaveOutEvent` is `[Obsolete]` (renamed `WaveOut`), and **`WaveOut`/`WasapiOut` are themselves `[Obsolete]`** in favour of the fluent `WasapiPlayerBuilder` → `WasapiPlayer` (zero-copy buffers, MMCSS priority, IAudioClient3 low-latency). This agent uses the builder.
- `BufferedWaveProvider` buffer size is a **constructor argument** (`new BufferedWaveProvider(waveFormat, TimeSpan)`); `BufferLength`/`BufferDuration` are read-only. `Read` takes a `Span<byte>`.

## Configuration & running

- **Server (required):** `appsettings.json` `ServerUrl` — the base server URL; the agent appends `/hubs/agents-control`.
- **Output routing:** managed from the server (see [above](#multiple-output-devices)), not in `appsettings.json`.
- **List devices:** `--list-devices` prints render devices and exits.
- **Port:** `--port <n>` (1–65535); defaults to **8090** (`DefaultPort`). Invalid values log a warning and fall back to the default. See [`Program.cs`](../src/Oceana.Agent.Windows/Program.cs).
- **Shutdown:** Ctrl+C (`Console.CancelKeyPress`) triggers cooperative cancellation and a clean stop.
- **Logging:** Serilog to the console.

```bash
# from the repo root
dotnet run --project src/Oceana.Agent.Windows -- --list-devices   # discover device names/ids
dotnet run --project src/Oceana.Agent.Windows                     # listens on 0.0.0.0:8090
dotnet run --project src/Oceana.Agent.Windows -- --port 9000      # custom port
```

See [development.md](./development.md) for building and testing.

## Key files

| File | Purpose |
|------|---------|
| [`Program.cs`](../src/Oceana.Agent.Windows/Program.cs) | Entry point: Serilog, `ServerUrl` config, agent-id, `--list-devices`, `--port`, Ctrl+C, starts the control connection + listener. |
| [`Configuration/ServerOptions.cs`](../src/Oceana.Agent.Windows/Configuration/ServerOptions.cs) | Bound `ServerUrl` (required). |
| [`Networking/ServerControlConnection.cs`](../src/Oceana.Agent.Windows/Networking/ServerControlConnection.cs) | SignalR control client: self-register, receive routing, reconnect/re-register. |
| [`Playback/RoutingStore.cs`](../src/Oceana.Agent.Windows/Playback/RoutingStore.cs) | Thread-safe current routing (server-pushed), read per connection. |
| [`Networking/AudioAgentListener.cs`](../src/Oceana.Agent.Windows/Networking/AudioAgentListener.cs) | TCP accept loop, socket tuning. |
| [`Networking/AudioPlaybackSession.cs`](../src/Oceana.Agent.Windows/Networking/AudioPlaybackSession.cs) | Header read, output plan, per-output buffering, pre-roll, pump, logging. |
| [`Playback/ChannelRouter.cs`](../src/Oceana.Agent.Windows/Playback/ChannelRouter.cs) | De-interleave + fan-out to per-output buffers. |
| [`Playback/WasapiAudioPlayer.cs`](../src/Oceana.Agent.Windows/Playback/WasapiAudioPlayer.cs) / [`WasapiAudioDeviceResolver.cs`](../src/Oceana.Agent.Windows/Playback/WasapiAudioDeviceResolver.cs) | WASAPI playback + device resolution. |
| [`Playback/AudioStreamHeaderExtensions.cs`](../src/Oceana.Agent.Windows/Playback/AudioStreamHeaderExtensions.cs) | OCAP header → NAudio `WaveFormat`. |
