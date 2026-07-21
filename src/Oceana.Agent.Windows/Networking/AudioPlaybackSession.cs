using System.Diagnostics;
using NAudio.Wave;
using Oceana.Agent.Windows.Playback;
using Oceana.Contracts;
using Oceana.Protocol;
using Serilog;

namespace Oceana.Agent.Windows.Networking;

/// <summary>
/// Handles a single connected server: reads the handshake header, then de-interleaves the incoming
/// audio and streams each configured channel subset to its output device until the connection closes
/// or playback is cancelled.
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
    private readonly IReadOnlyList<AudioOutput> outputs;
    private readonly ILogger logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioPlaybackSession"/> class.
    /// </summary>
    /// <param name="playerFactory">The factory used to create output players for this session.</param>
    /// <param name="outputs">The routing for this session; when empty, the whole stream plays to the default device.</param>
    /// <param name="logger">The logger used to report session progress.</param>
    public AudioPlaybackSession(IAudioPlayerFactory playerFactory, IReadOnlyList<AudioOutput> outputs, ILogger logger)
    {
        this.playerFactory = playerFactory;
        this.outputs = outputs;
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

        var plans = BuildPlans(header.Channels);
        var activeOutputs = new List<ActiveOutput>(plans.Count);

        try
        {
            foreach (var plan in plans)
            {
                var format = CreateFormat(header, plan.SourceChannels.Count);
                var buffer = new BufferedWaveProvider(format, TimeSpan.FromSeconds(PlaybackBufferSeconds));
                var player = playerFactory.Create(plan.DeviceId);
                player.Init(buffer);
                activeOutputs.Add(new ActiveOutput(plan, buffer, player));
                logger.Information(
                    "Output {Device} plays source channels [{Channels}].",
                    plan.DeviceId ?? "(default device)",
                    string.Join(", ", plan.SourceChannels));
            }

            var router = new ChannelRouter(
                waveFormat.Channels,
                waveFormat.BitsPerSample / 8,
                activeOutputs.Select(o => new ChannelRoute(o.Plan.SourceChannels, o.Buffer)).ToList());

            await PumpAsync(stream, router, activeOutputs, cancellationToken);
        }
        finally
        {
            foreach (var output in activeOutputs)
            {
                output.Player.Stop();
                output.Player.Dispose();
            }
        }
    }

    private static async Task WaitForCapacityAsync(IReadOnlyList<ActiveOutput> outputs, CancellationToken cancellationToken)
    {
        while (HasFullBuffer(outputs))
        {
            await Task.Delay(BackpressureDelay, cancellationToken);
        }
    }

    private static bool HasFullBuffer(IReadOnlyList<ActiveOutput> outputs)
    {
        foreach (var output in outputs)
        {
            if ((output.Buffer.BufferLength - output.Buffer.BufferedBytes) < ReadBufferSize)
            {
                return true;
            }
        }

        return false;
    }

    private static WaveFormat CreateFormat(AudioStreamHeader header, int channels)
    {
        return header.Encoding == AudioEncoding.IeeeFloat
            ? WaveFormat.CreateIeeeFloatWaveFormat(header.SampleRate, channels)
            : new WaveFormat(header.SampleRate, header.BitsPerSample, channels);
    }

    private IReadOnlyList<OutputPlan> BuildPlans(int sourceChannels)
    {
        if (outputs.Count == 0)
        {
            var allChannels = Enumerable.Range(0, sourceChannels).ToArray();
            return new[] { new OutputPlan(null, allChannels) };
        }

        var plans = new List<OutputPlan>(outputs.Count);
        foreach (var output in outputs)
        {
            if (output.Channels.Length == 0)
            {
                throw new InvalidOperationException($"Output '{output.Device ?? "(default device)"}' has no channels configured.");
            }

            foreach (var channel in output.Channels)
            {
                if (channel < 0 || channel >= sourceChannels)
                {
                    throw new InvalidOperationException(
                        $"Configured channel {channel} is out of range for the {sourceChannels}-channel stream.");
                }
            }

            plans.Add(new OutputPlan(output.Device, output.Channels));
        }

        return plans;
    }

    private async Task PumpAsync(Stream stream, ChannelRouter router, IReadOnlyList<ActiveOutput> activeOutputs, CancellationToken cancellationToken)
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

            await WaitForCapacityAsync(activeOutputs, cancellationToken);
            router.Route(readBuffer.AsSpan(0, read));

            if (!playing && activeOutputs[0].Buffer.BufferedDuration >= PreRollDuration)
            {
                StartPlayback(activeOutputs, "Pre-roll complete");
                playing = true;
            }

            if (playing && logStopwatch.Elapsed >= BufferLogInterval)
            {
                LogBufferLevels(activeOutputs);
                logStopwatch.Restart();
            }
        }

        if (!playing)
        {
            StartPlayback(activeOutputs, "Stream ended before the pre-roll target was reached");
        }
    }

    private void StartPlayback(IReadOnlyList<ActiveOutput> activeOutputs, string reason)
    {
        foreach (var output in activeOutputs)
        {
            output.Player.Play();
        }

        logger.Information(
            "{Reason}; starting playback on {OutputCount} output(s) with {BufferedMilliseconds} ms buffered.",
            reason,
            activeOutputs.Count,
            (long)activeOutputs[0].Buffer.BufferedDuration.TotalMilliseconds);
    }

    private void LogBufferLevels(IReadOnlyList<ActiveOutput> activeOutputs)
    {
        foreach (var output in activeOutputs)
        {
            var bufferedMilliseconds = (long)output.Buffer.BufferedDuration.TotalMilliseconds;
            var device = output.Plan.DeviceId ?? "(default device)";
            if (bufferedMilliseconds <= LowWaterMilliseconds)
            {
                logger.Warning("Output {Device} buffer running low: {BufferedMilliseconds} ms (risk of underrun).", device, bufferedMilliseconds);
            }
            else
            {
                logger.Information("Output {Device} buffer level: {BufferedMilliseconds} ms.", device, bufferedMilliseconds);
            }
        }
    }

    private sealed class ActiveOutput
    {
        public ActiveOutput(OutputPlan plan, BufferedWaveProvider buffer, IAudioPlayer player)
        {
            Plan = plan;
            Buffer = buffer;
            Player = player;
        }

        public OutputPlan Plan { get; }

        public BufferedWaveProvider Buffer { get; }

        public IAudioPlayer Player { get; }
    }
}
