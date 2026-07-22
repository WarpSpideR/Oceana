using System.Collections.Concurrent;
using System.Diagnostics;
using Oceana.Protocol;
using Oceana.Server.Features.Agents;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Coordinates outbound audio streams to agents, running at most one stream per agent.
/// </summary>
public sealed class AudioStreamManager : IAudioStreamManager, IAsyncDisposable
{
    private const int ChunkMilliseconds = 20;

    // The agent does not drain its buffer on EOF, so hold the connection open briefly after the
    // last sample is sent so the buffered audio (pre-roll + jitter buffer) finishes playing.
    private const int TailHoldMilliseconds = 750;

    private readonly ConcurrentDictionary<Guid, StreamState> streams = new ConcurrentDictionary<Guid, StreamState>();
    private readonly IAgentConnectionFactory connectionFactory;
    private readonly IAgentRegistry registry;
    private readonly IStatusNotifier notifier;
    private readonly ILogger<AudioStreamManager> logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="AudioStreamManager"/> class.
    /// </summary>
    /// <param name="connectionFactory">The factory used to open connections to agents.</param>
    /// <param name="registry">The registry used to look up agents and record status changes.</param>
    /// <param name="notifier">The notifier used to broadcast status changes.</param>
    /// <param name="logger">The logger used to report streaming activity.</param>
    public AudioStreamManager(
        IAgentConnectionFactory connectionFactory,
        IAgentRegistry registry,
        IStatusNotifier notifier,
        ILogger<AudioStreamManager> logger)
    {
        this.connectionFactory = connectionFactory;
        this.registry = registry;
        this.notifier = notifier;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public bool TryStartStream(Guid agentId, ToneOptions options)
    {
        var agent = registry.Get(agentId);
        if (agent is null)
        {
            return false;
        }

        var cancellation = new CancellationTokenSource();
        var state = new StreamState(cancellation);
        if (!streams.TryAdd(agentId, state))
        {
            cancellation.Dispose();
            return false;
        }

        var header = new AudioStreamHeader(
            AudioEncoding.Pcm,
            options.Channels,
            ToneGenerator.SampleRate,
            ToneGenerator.BitsPerSample);
        state.Task = Task.Run(() =>
            RunStreamAsync(agent, header, (stream, ct) => PumpToneAsync(stream, options, ct), cancellation.Token));
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> TryStreamPcmAsync(
        Guid agentId,
        ReadOnlyMemory<byte> pcm,
        StreamFormat format,
        Func<double> volume,
        CancellationToken cancellationToken)
    {
        var agent = registry.Get(agentId);
        if (agent is null)
        {
            return false;
        }

        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var state = new StreamState(cancellation);
        if (!streams.TryAdd(agentId, state))
        {
            cancellation.Dispose();
            return false;
        }

        state.Task = RunStreamAsync(
            agent,
            format.ToHeader(),
            (stream, ct) => PumpBufferAsync(stream, pcm, format, volume, ct),
            cancellation.Token);

        try
        {
            await state.Task;
        }
        finally
        {
            cancellation.Dispose();
        }

        return true;
    }

    /// <inheritdoc/>
    public bool StopStream(Guid agentId)
    {
        if (streams.TryRemove(agentId, out var state))
        {
            state.Cancellation.Cancel();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public bool IsStreaming(Guid agentId)
    {
        return streams.ContainsKey(agentId);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        foreach (var state in streams.Values)
        {
            state.Cancellation.Cancel();
        }

        foreach (var state in streams.Values)
        {
            if (state.Task is not null)
            {
                try
                {
                    await state.Task;
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "A stream task faulted during shutdown.");
                }
            }
        }
    }

    private static async Task PumpToneAsync(Stream stream, ToneOptions options, CancellationToken cancellationToken)
    {
        var generator = new ToneGenerator(options.Frequency, options.Channels);
        var framesPerChunk = ToneGenerator.SampleRate * ChunkMilliseconds / 1000;
        var chunk = new byte[framesPerChunk * generator.BytesPerFrame];
        var totalFrames = options.Duration is { } duration
            ? (long)(duration.TotalSeconds * ToneGenerator.SampleRate)
            : (long?)null;

        var clock = Stopwatch.StartNew();
        long framesSent = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (totalFrames is { } total && framesSent >= total)
            {
                break;
            }

            var written = generator.Fill(chunk);
            await stream.WriteAsync(chunk.AsMemory(0, written), cancellationToken);
            framesSent += written / generator.BytesPerFrame;

            // Real-time pacing: sleep only until wall-clock catches up to the audio timeline,
            // so cumulative production tracks real time without the drift of a fixed per-chunk delay.
            var scheduledMilliseconds = (double)framesSent / ToneGenerator.SampleRate * 1000.0;
            var aheadMilliseconds = scheduledMilliseconds - clock.Elapsed.TotalMilliseconds;
            if (aheadMilliseconds > 1.0)
            {
                await Task.Delay((int)aheadMilliseconds, cancellationToken);
            }
        }
    }

    private static async Task PumpBufferAsync(
        Stream stream,
        ReadOnlyMemory<byte> pcm,
        StreamFormat format,
        Func<double> volume,
        CancellationToken cancellationToken)
    {
        var framesPerChunk = format.SampleRate * ChunkMilliseconds / 1000;
        var chunkBytes = framesPerChunk * format.BytesPerFrame;
        var canApplyGain = format is { Encoding: AudioEncoding.Pcm, BitsPerSample: 16 };
        var scratch = canApplyGain ? new byte[chunkBytes] : Array.Empty<byte>();

        var clock = Stopwatch.StartNew();
        long framesSent = 0;
        var offset = 0;

        while (offset < pcm.Length && !cancellationToken.IsCancellationRequested)
        {
            var take = Math.Min(chunkBytes, pcm.Length - offset);
            var chunk = pcm.Slice(offset, take);

            // Read the gain once per chunk so live volume changes apply within ~20 ms. Skip the
            // copy entirely at (near-)unity so full-volume playback stays zero-copy.
            var gain = canApplyGain ? volume() : 1.0;
            if (gain >= 0.999)
            {
                await stream.WriteAsync(chunk, cancellationToken);
            }
            else
            {
                PcmGain.ApplyInt16(chunk.Span, scratch.AsSpan(0, take), Math.Clamp(gain, 0.0, 1.0));
                await stream.WriteAsync(scratch.AsMemory(0, take), cancellationToken);
            }

            offset += take;
            framesSent += take / format.BytesPerFrame;

            // Real-time pacing: sleep only until the wall clock catches up to the audio timeline.
            var scheduledMilliseconds = (double)framesSent / format.SampleRate * 1000.0;
            var aheadMilliseconds = scheduledMilliseconds - clock.Elapsed.TotalMilliseconds;
            if (aheadMilliseconds > 1.0)
            {
                await Task.Delay((int)aheadMilliseconds, cancellationToken);
            }
        }

        // Keep the connection open so the agent's buffered tail plays out before it is closed.
        await Task.Delay(TailHoldMilliseconds, cancellationToken);
    }

    private async Task RunStreamAsync(
        AgentInfo agent,
        AudioStreamHeader header,
        Func<Stream, CancellationToken, Task> pump,
        CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(agent.Id, AgentStatus.Connecting, null);

        try
        {
            await using var connection = await connectionFactory.ConnectAsync(agent.Host, agent.Port, cancellationToken);

            await header.WriteToAsync(connection.Stream, cancellationToken);

            await UpdateStatusAsync(agent.Id, AgentStatus.Streaming, null);
            logger.LogInformation(
                "Streaming {Channels} channel(s) to agent {Agent} ({Host}:{Port}).",
                header.Channels,
                agent.Name,
                agent.Host,
                agent.Port);

            await pump(connection.Stream, cancellationToken);
            await UpdateStatusAsync(agent.Id, AgentStatus.Idle, null);
        }
        catch (OperationCanceledException)
        {
            await UpdateStatusAsync(agent.Id, AgentStatus.Idle, null);
            logger.LogInformation("Stream to agent {Agent} was stopped.", agent.Name);
        }
        catch (Exception ex)
        {
            await UpdateStatusAsync(agent.Id, AgentStatus.Faulted, ex.Message);
            logger.LogError(ex, "Stream to agent {Agent} failed.", agent.Name);
        }
        finally
        {
            streams.TryRemove(agent.Id, out _);
        }
    }

    private async Task UpdateStatusAsync(Guid agentId, AgentStatus status, string? lastError)
    {
        var updated = registry.UpdateStatus(agentId, status, lastError);
        if (updated is not null)
        {
            await notifier.NotifyAgentChangedAsync(updated);
        }
    }

    private sealed class StreamState
    {
        public StreamState(CancellationTokenSource cancellation)
        {
            Cancellation = cancellation;
        }

        public CancellationTokenSource Cancellation { get; }

        public Task? Task { get; set; }
    }
}
