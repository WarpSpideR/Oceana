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

        state.Task = Task.Run(() => RunStreamAsync(agent, options, cancellation.Token));
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

    private async Task RunStreamAsync(AgentInfo agent, ToneOptions options, CancellationToken cancellationToken)
    {
        await UpdateStatusAsync(agent.Id, AgentStatus.Connecting, null);

        try
        {
            await using var connection = await connectionFactory.ConnectAsync(agent.Host, agent.Port, cancellationToken);

            var header = new AudioStreamHeader(
                AudioEncoding.Pcm,
                options.Channels,
                ToneGenerator.SampleRate,
                ToneGenerator.BitsPerSample);
            await header.WriteToAsync(connection.Stream, cancellationToken);

            await UpdateStatusAsync(agent.Id, AgentStatus.Streaming, null);
            logger.LogInformation(
                "Streaming {Frequency} Hz tone ({Channels} channel(s)) to agent {Agent} ({Host}:{Port}).",
                options.Frequency,
                options.Channels,
                agent.Name,
                agent.Host,
                agent.Port);

            await PumpToneAsync(connection.Stream, options, cancellationToken);
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
