using System.Diagnostics;
using NAudio.Wave;
using Oceana.Agent.Windows.Playback;
using Oceana.Agent.Windows.Protocol;
using Serilog;

namespace Oceana.Agent.Windows.Networking;

/// <summary>
/// Handles a single connected server: reads the handshake header, then streams the incoming raw
/// audio to the local output device until the connection closes or playback is cancelled.
/// </summary>
public sealed class AudioPlaybackSession
{
    private const int ReadBufferSize = 8192;
    private const int PlaybackBufferSeconds = 2;
    private const long LowWaterMilliseconds = 50;

    private static readonly TimeSpan BackpressureDelay = TimeSpan.FromMilliseconds(10);
    private static readonly TimeSpan PreRollDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan BufferLogInterval = TimeSpan.FromSeconds(1);

    private readonly IAudioPlayerFactory playerFactory;
    private readonly ILogger logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioPlaybackSession"/> class.
    /// </summary>
    /// <param name="playerFactory">The factory used to create the output player for this session.</param>
    /// <param name="logger">The logger used to report session progress.</param>
    public AudioPlaybackSession(IAudioPlayerFactory playerFactory, ILogger logger)
    {
        this.playerFactory = playerFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Reads the handshake header from the stream and plays the audio that follows until the stream ends.
    /// </summary>
    /// <param name="stream">The connected stream carrying the header followed by raw audio.</param>
    /// <param name="cancellationToken">A token used to stop playback.</param>
    /// <returns>A task that completes when the stream ends or playback is cancelled.</returns>
    public async Task RunAsync(Stream stream, CancellationToken cancellationToken)
    {
        var header = await AudioStreamHeader.ReadAsync(stream, cancellationToken);
        var waveFormat = header.ToWaveFormat();
        logger.Information(
            "Negotiated audio format: {Encoding}, {SampleRate} Hz, {Channels} channel(s), {BitsPerSample}-bit.",
            header.Encoding,
            header.SampleRate,
            header.Channels,
            header.BitsPerSample);

        var buffer = new BufferedWaveProvider(waveFormat, TimeSpan.FromSeconds(PlaybackBufferSeconds));

        using var player = playerFactory.Create();
        player.Init(buffer);

        try
        {
            await PumpAsync(stream, buffer, player, cancellationToken);
        }
        finally
        {
            player.Stop();
        }
    }

    private static async Task WaitForCapacityAsync(BufferedWaveProvider buffer, int required, CancellationToken cancellationToken)
    {
        while ((buffer.BufferLength - buffer.BufferedBytes) < required)
        {
            await Task.Delay(BackpressureDelay, cancellationToken);
        }
    }

    private async Task PumpAsync(Stream stream, BufferedWaveProvider buffer, IAudioPlayer player, CancellationToken cancellationToken)
    {
        var readBuffer = new byte[ReadBufferSize];
        var logStopwatch = Stopwatch.StartNew();
        var playing = false;

        while (true)
        {
            var read = await stream.ReadAsync(readBuffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            await WaitForCapacityAsync(buffer, read, cancellationToken);
            buffer.AddSamples(readBuffer, 0, read);

            if (!playing && buffer.BufferedDuration >= PreRollDuration)
            {
                StartPlayback(player, buffer, "Pre-roll complete");
                playing = true;
            }

            if (playing && logStopwatch.Elapsed >= BufferLogInterval)
            {
                LogBufferLevel(buffer);
                logStopwatch.Restart();
            }
        }

        if (!playing)
        {
            StartPlayback(player, buffer, "Stream ended before the pre-roll target was reached");
        }
    }

    private void StartPlayback(IAudioPlayer player, BufferedWaveProvider buffer, string reason)
    {
        player.Play();
        logger.Information("{Reason}; starting playback with {BufferedMilliseconds} ms buffered.", reason, (long)buffer.BufferedDuration.TotalMilliseconds);
    }

    private void LogBufferLevel(BufferedWaveProvider buffer)
    {
        var bufferedMilliseconds = (long)buffer.BufferedDuration.TotalMilliseconds;
        if (bufferedMilliseconds <= LowWaterMilliseconds)
        {
            logger.Warning("Playback buffer running low: {BufferedMilliseconds} ms buffered (risk of underrun).", bufferedMilliseconds);
        }
        else
        {
            logger.Information("Playback buffer level: {BufferedMilliseconds} ms buffered.", bufferedMilliseconds);
        }
    }
}
