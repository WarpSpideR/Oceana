using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Infrastructure.Audio;

/// <summary>
/// The decoded contents of a WAV file: its format and the raw interleaved PCM samples.
/// </summary>
/// <param name="Format">The audio format described by the WAV header.</param>
/// <param name="Pcm">The raw PCM sample bytes from the data chunk.</param>
public sealed record WavAudio(StreamFormat Format, byte[] Pcm);
