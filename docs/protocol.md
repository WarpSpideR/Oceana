# OCAP — the Oceana Audio Protocol

> The wire format between server and agent, defined in [`Oceana.Protocol`](../src/Oceana.Protocol). Deliberately minimal: one fixed-size handshake header, then a continuous stream of raw audio bytes.

## Overview

When the server connects to an agent it sends, **once**, a fixed **16-byte header** describing the audio that will follow. Everything after those 16 bytes is **raw, interleaved PCM samples** in the format the header advertised, streamed until the connection closes. There is no per-frame framing — the stream *is* the audio.

```
┌──────────────────────────┐
│  16-byte OCAP header      │  ← sent once, immediately on connect
├──────────────────────────┤
│  raw PCM samples …        │  ← continuous, until the connection closes
│  …                        │
└──────────────────────────┘
```

## Header layout

Fixed size: **16 bytes**, little-endian. Defined in [`AudioStreamHeader.cs`](../src/Oceana.Protocol/AudioStreamHeader.cs) (`AudioStreamHeader.Size = 16`).

| Offset | Size | Field | Type | Notes |
|-------:|-----:|-------|------|-------|
| 0 | 4 | Magic | ASCII | `"OCAP"` (`0x4F 0x43 0x41 0x50`) |
| 4 | 1 | Version | `byte` | Currently `1` (`AudioStreamHeader.CurrentVersion`) |
| 5 | 1 | Encoding | `byte` | `AudioEncoding` — `0` = PCM, `1` = IEEE float |
| 6 | 2 | Channels | `UInt16` | e.g. `2` for stereo |
| 8 | 4 | SampleRate | `Int32` | Hertz, e.g. `48000` |
| 12 | 2 | BitsPerSample | `UInt16` | e.g. `16` |
| 14 | 2 | Reserved | `UInt16` | Written as `0`; reserved for future use |

### Encoding values

Defined in [`AudioEncoding.cs`](../src/Oceana.Protocol/AudioEncoding.cs):

| Value | Name | Meaning |
|------:|------|---------|
| `0` | `Pcm` | Integer PCM samples; width given by `BitsPerSample`. |
| `1` | `IeeeFloat` | 32-bit IEEE floating-point samples. |

> **What the server sends today:** `Pcm`, 2 channels, `48000` Hz, `16`-bit — see the tone streamer in [server.md](./server.md). The `IeeeFloat` path exists in the header/mapping but isn't exercised by the current server.

## Why a magic + version prefix?

- **Magic bytes (`OCAP`)** let the agent reject anything that isn't an Oceana stream *immediately* and with a clear error, rather than mis-reading arbitrary bytes (a port scanner, a stray HTTP request) as a sample rate/channel count and failing confusingly deep in the audio stack. It's a fail-fast self-identifying signature (like `PK` in a zip or `RIFF` in a wav). It is **not** security — it only guards against accidental/corrupt connections.
- **Version byte** lets the format evolve: a v1 agent cleanly rejects a stream it can't understand instead of misinterpreting it. Magic answers *"is this OCAP at all?"*; version answers *"is it a dialect I know?"*.

## API surface

`AudioStreamHeader` is a `readonly struct` with:

- `static AudioStreamHeader Parse(ReadOnlySpan<byte> buffer)` — parse from an already-filled 16-byte buffer.
- `static ValueTask<AudioStreamHeader> ReadAsync(Stream, CancellationToken)` — read a full header from a stream, reassembling across partial reads (uses `Stream.ReadExactlyAsync`).
- `void Write(Span<byte>)` and `ValueTask WriteToAsync(Stream, CancellationToken)` — serialise.
- Properties: `Encoding`, `Channels`, `SampleRate`, `BitsPerSample`.

The **agent** maps a parsed header to an NAudio `WaveFormat` via an extension kept on the agent side (so the protocol library stays free of NAudio): [`AudioStreamHeaderExtensions.ToWaveFormat()`](../src/Oceana.Agent.Windows/Playback/AudioStreamHeaderExtensions.cs).

### Error behaviour

`Parse` throws:

| Condition | Exception |
|-----------|-----------|
| Buffer shorter than 16 bytes | `ArgumentException` |
| Magic bytes ≠ `OCAP` | `InvalidDataException` |
| Version ≠ `1` | `InvalidDataException` |

`ReadAsync` additionally surfaces `EndOfStreamException` if the stream ends before 16 bytes arrive.

## Design notes

- **No per-frame framing (v1).** Because the format is fixed at handshake and the transport is a reliable TCP byte stream, the agent just appends incoming bytes to its playback buffer. This keeps v1 trivial. A future framed/control-message layer could be added under a new version if needed.
- **Little-endian** throughout, via `System.Buffers.Binary.BinaryPrimitives`.

## Key files

- [`src/Oceana.Protocol/AudioStreamHeader.cs`](../src/Oceana.Protocol/AudioStreamHeader.cs) — header struct, parse/read/write.
- [`src/Oceana.Protocol/AudioEncoding.cs`](../src/Oceana.Protocol/AudioEncoding.cs) — encoding enum.
- [`src/Oceana.Agent.Windows/Playback/AudioStreamHeaderExtensions.cs`](../src/Oceana.Agent.Windows/Playback/AudioStreamHeaderExtensions.cs) — header → NAudio `WaveFormat` (agent-side).
