using System.Collections.Concurrent;
using Oceana.Contracts;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Plays a finite PCM source to every reachable device in a zone. For each target agent it pushes a
/// routing mapping the source's channels to the zone's devices, streams the buffer, then restores
/// the agent's configured routing. Tracks per-zone state so playback can be stopped and surfaced as
/// now-playing, and broadcasts changes over the zone status hub.
/// </summary>
public sealed class ZonePlaybackService : IZonePlaybackService
{
    // The agent snapshots its routing when the audio connection is accepted, so allow the pushed
    // routing to arrive before dialing.
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(250);

    private readonly ConcurrentDictionary<Guid, ZonePlayback> playbacks = new ConcurrentDictionary<Guid, ZonePlayback>();
    private readonly IAgentRegistry registry;
    private readonly IZoneRegistry zoneRegistry;
    private readonly IAudioStreamManager streamManager;
    private readonly IAgentRoutingCommander commander;
    private readonly IZoneStatusNotifier notifier;
    private readonly ILogger<ZonePlaybackService> logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="ZonePlaybackService"/> class.
    /// </summary>
    /// <param name="registry">The agent registry.</param>
    /// <param name="zoneRegistry">The zone registry, used to read the live per-zone volume.</param>
    /// <param name="streamManager">The stream manager used to send audio.</param>
    /// <param name="commander">The commander used to push routing to agents.</param>
    /// <param name="notifier">The notifier used to broadcast playback state.</param>
    /// <param name="logger">The logger.</param>
    public ZonePlaybackService(
        IAgentRegistry registry,
        IZoneRegistry zoneRegistry,
        IAudioStreamManager streamManager,
        IAgentRoutingCommander commander,
        IZoneStatusNotifier notifier,
        ILogger<ZonePlaybackService> logger)
    {
        this.registry = registry;
        this.zoneRegistry = zoneRegistry;
        this.streamManager = streamManager;
        this.commander = commander;
        this.notifier = notifier;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public ZonePlaybackState Start(ZoneInfo zone, string sourceName, ReadOnlyMemory<byte> pcm, StreamFormat format)
    {
        CancelExisting(zone.Id);

        var classification = ZoneTargeting.Classify(this.registry, this.streamManager, zone);
        var targeted = classification.Targets
            .Select(target => new BroadcastTarget(target.Agent.Id, target.Agent.Name, target.DeviceIds.Count))
            .ToArray();

        if (targeted.Length == 0)
        {
            var idle = new ZonePlaybackState(zone.Id, false, sourceName, DateTimeOffset.UtcNow, targeted, classification.Skips);
            this.Notify(idle);
            return idle;
        }

        var cancellation = new CancellationTokenSource();
        var state = new ZonePlaybackState(
            zone.Id,
            true,
            sourceName,
            DateTimeOffset.UtcNow,
            targeted,
            classification.Skips);
        var entry = new ZonePlayback(cancellation, state);
        this.playbacks[zone.Id] = entry;

        var zoneId = zone.Id;
        Func<double> volume = () => this.zoneRegistry.Get(zoneId)?.Volume ?? 1.0;
        var agentTasks = classification.Targets
            .Select(target => Task.Run(() =>
                this.PlayToAgentAsync(target.Agent, target.DeviceIds, pcm, format, volume, cancellation.Token)))
            .ToArray();
        _ = Task.Run(() => this.AwaitCompletionAsync(zone.Id, entry, agentTasks));

        this.Notify(state);
        return state;
    }

    /// <inheritdoc/>
    public bool Stop(Guid zoneId)
    {
        if (!this.playbacks.TryRemove(zoneId, out var entry))
        {
            return false;
        }

        entry.Cancellation.Cancel();
        this.Notify(ZonePlaybackState.Idle(zoneId));
        return true;
    }

    /// <inheritdoc/>
    public ZonePlaybackState? Get(Guid zoneId) =>
        this.playbacks.TryGetValue(zoneId, out var entry) ? entry.State : null;

    private void CancelExisting(Guid zoneId)
    {
        if (this.playbacks.TryRemove(zoneId, out var existing))
        {
            existing.Cancellation.Cancel();
        }
    }

    private async Task PlayToAgentAsync(
        AgentInfo agent,
        IReadOnlyList<string> deviceIds,
        ReadOnlyMemory<byte> pcm,
        StreamFormat format,
        Func<double> volume,
        CancellationToken cancellationToken)
    {
        var priorRouting = this.registry.Get(agent.Id)?.Routing ?? new AgentRouting(Array.Empty<AudioOutput>());
        var channels = Enumerable.Range(0, format.Channels).ToArray();
        var playbackRouting = new AgentRouting(
            deviceIds.Select(id => new AudioOutput(id, channels)).ToArray());

        try
        {
            await this.commander.PushRoutingAsync(agent.Id, playbackRouting);
            await Task.Delay(SettleDelay, cancellationToken);
            await this.streamManager.TryStreamPcmAsync(agent.Id, pcm, format, volume, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Stopped — expected.
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Playback to agent {Agent} failed.", agent.Name);
        }
        finally
        {
            try
            {
                await this.commander.PushRoutingAsync(agent.Id, priorRouting);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to restore routing for agent {Agent} after playback.", agent.Name);
            }
        }
    }

    private async Task AwaitCompletionAsync(Guid zoneId, ZonePlayback entry, Task[] agentTasks)
    {
        try
        {
            await Task.WhenAll(agentTasks);
        }
        catch (Exception ex)
        {
            this.logger.LogDebug(ex, "A playback agent task faulted for zone {Zone}.", zoneId);
        }

        // Natural end: remove only if this exact playback is still current (not replaced/stopped).
        if (this.playbacks.TryRemove(new KeyValuePair<Guid, ZonePlayback>(zoneId, entry)))
        {
            this.Notify(ZonePlaybackState.Idle(zoneId));
        }

        entry.Cancellation.Dispose();
    }

    private void Notify(ZonePlaybackState state)
    {
        _ = NotifySafeAsync();

        async Task NotifySafeAsync()
        {
            try
            {
                await this.notifier.NotifyZonePlaybackChangedAsync(state);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Failed to notify playback state for zone {Zone}.", state.ZoneId);
            }
        }
    }

    private sealed class ZonePlayback
    {
        public ZonePlayback(CancellationTokenSource cancellation, ZonePlaybackState state)
        {
            Cancellation = cancellation;
            State = state;
        }

        public CancellationTokenSource Cancellation { get; }

        public ZonePlaybackState State { get; }
    }
}
