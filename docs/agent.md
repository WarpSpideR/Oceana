# Agent — `Oceana.Agent.Windows`

> A Windows console app that waits for a connection from the server and plays the incoming audio stream to the machine's default output device. Built on [NAudio](https://github.com/naudio/NAudio) 3.0.

## Responsibilities

1. Listen on TCP for an incoming connection from the server.
2. Read the [OCAP header](./protocol.md), then continuously read raw PCM.
3. Play that audio to the **default output device**, smoothly (buffering to absorb network jitter).
4. Handle disconnects and graceful shutdown; then wait for the next connection.

## Target framework — and why it's so specific

`net10.0-windows10.0.19041.0` (see [`Oceana.Agent.Windows.csproj`](../src/Oceana.Agent.Windows/Oceana.Agent.Windows.csproj)).

> **Why the `10.0.19041` floor?** NAudio 3.0 splits its Windows audio backends (WinMM/`WaveOut`, WASAPI, ASIO) into a dependency group that targets `net*-windows10.0.19041`. A plain `net10.0-windows` (which defaults to a Windows 7 baseline) would silently resolve to the *stripped* group — you'd lose `WaveOut`/WASAPI entirely and only get the core DSP types. The `.19041` floor is what pulls in the actual playback stack.

## Networking — the listener

[`AudioAgentListener`](../src/Oceana.Agent.Windows/Networking/AudioAgentListener.cs):

- Binds **`IPAddress.Any`** on the configured port (all interfaces, so a server on another machine can connect).
- Accepts and handles **one connection at a time** (a sequential accept → play → loop). A second server connecting while one is active simply waits in the TCP backlog.
- Per-connection socket tuning: `NoDelay = true` (disables Nagle, so the server's writes aren't coalesced into laggy bursts) and `ReceiveBufferSize = 262144` (256 KB, giving the kernel room to smooth bursty delivery).
- Resilient: a failure on one connection is logged and the listener keeps accepting; cancellation stops it cleanly.

## Playback pipeline

[`AudioPlaybackSession`](../src/Oceana.Agent.Windows/Networking/AudioPlaybackSession.cs) handles a single connection:

1. **Read the header** → map to a `WaveFormat`.
2. Create a `BufferedWaveProvider` with **2 seconds** of capacity (`PlaybackBufferSeconds = 2`).
3. **Pre-roll:** pump the network into the buffer until **400 ms** is buffered (`PreRollDuration`), *then* start playback. (If the stream ends before 400 ms, it starts anyway.)
4. **Pump loop:** read ~8 KB at a time (`ReadBufferSize = 8192`) and append to the buffer.
5. **Backpressure:** if the buffer is near full, wait (`BackpressureDelay = 10 ms`) before reading more — TCP flow control then throttles the server, bounding latency without dropping audio.
6. **Observability:** every second (`BufferLogInterval`) it logs the buffer level; below **50 ms** (`LowWaterMilliseconds`) it warns of a possible underrun.

> **Why pre-roll + a jitter buffer?** Without it, playback starts on an empty buffer and runs permanently on the edge of starvation — any network hiccup or GC pause becomes an audible gap (a stutter), because NAudio's `BufferedWaveProvider` fills underruns with silence rather than stopping. The 400 ms cushion rides out that variance. A buffer that stays healthy with a well-paced producer but slowly drains with an under-feeding one indicates a *producer rate deficit* (clock drift), not a jitter problem — see [roadmap.md](./roadmap.md).

### Output device

[`WaveOutAudioPlayer`](../src/Oceana.Agent.Windows/Playback/WaveOutAudioPlayer.cs) wraps NAudio's `WaveOut`:

- `DeviceNumber = -1` — the WAVE_MAPPER, i.e. the current **system default** output device.
- `BufferMilliseconds = 100`, `NumberOfBuffers = 3` (~300 ms of device-side buffering) — extra slack for the render callback against scheduling/GC pauses.

## The `IAudioPlayer` seam

Playback is abstracted behind [`IAudioPlayer`](../src/Oceana.Agent.Windows/Playback/IAudioPlayer.cs) + [`IAudioPlayerFactory`](../src/Oceana.Agent.Windows/Playback/IAudioPlayerFactory.cs), with `WaveOut` behind [`WaveOutAudioPlayer`](../src/Oceana.Agent.Windows/Playback/WaveOutAudioPlayer.cs).

> **Why:** it lets the network + parsing + buffering logic be unit-tested with a substitute player — **no sound card required** in tests. See [`AudioPlaybackSessionTests`](../tests/Oceana.Agent.Windows.Test/Networking/AudioPlaybackSessionTests.cs).

## NAudio 3.0-preview notes

The project uses `NAudio` **3.0.0-preview.18**, which differs from 2.x in ways that bit us:

- `WaveOutEvent` is `[Obsolete]` and renamed to **`WaveOut`** (the event-driven model).
- `BufferedWaveProvider.BufferLength`/`BufferDuration` are **read-only**; buffer size is now a **constructor argument**: `new BufferedWaveProvider(waveFormat, TimeSpan)`.

## Configuration & running

- **Port:** `--port <n>` (1–65535); defaults to **8090** (`DefaultPort`). Invalid values log a warning and fall back to the default. See [`Program.cs`](../src/Oceana.Agent.Windows/Program.cs).
- **Shutdown:** Ctrl+C (`Console.CancelKeyPress`) triggers cooperative cancellation and a clean stop.
- **Logging:** Serilog to the console.

```bash
# from the repo root
dotnet run --project src/Oceana.Agent.Windows                 # listens on 0.0.0.0:8090
dotnet run --project src/Oceana.Agent.Windows -- --port 9000  # custom port
```

See [development.md](./development.md) for building and testing.

## Key files

| File | Purpose |
|------|---------|
| [`Program.cs`](../src/Oceana.Agent.Windows/Program.cs) | Entry point: Serilog, `--port`, Ctrl+C, starts the listener. |
| [`Networking/AudioAgentListener.cs`](../src/Oceana.Agent.Windows/Networking/AudioAgentListener.cs) | TCP accept loop, socket tuning. |
| [`Networking/AudioPlaybackSession.cs`](../src/Oceana.Agent.Windows/Networking/AudioPlaybackSession.cs) | Header read, buffering, pre-roll, pump, logging. |
| [`Playback/IAudioPlayer.cs`](../src/Oceana.Agent.Windows/Playback/IAudioPlayer.cs) / [`WaveOutAudioPlayer.cs`](../src/Oceana.Agent.Windows/Playback/WaveOutAudioPlayer.cs) | Output-device abstraction + NAudio implementation. |
| [`Playback/AudioStreamHeaderExtensions.cs`](../src/Oceana.Agent.Windows/Playback/AudioStreamHeaderExtensions.cs) | OCAP header → NAudio `WaveFormat`. |
