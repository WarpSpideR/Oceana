using Oceana.Contracts;
using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Fans a recorded PCM message out to every reachable device in a zone. For each agent that owns
/// zone devices it pushes a temporary routing (the message's single channel to those devices),
/// streams the message, then restores the agent's configured routing.
/// </summary>
public sealed class ZoneBroadcaster : IZoneBroadcaster
{
    // The agent snapshots its routing when the audio connection is accepted, so allow the pushed
    // routing to arrive before dialing.
    private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(250);

    private readonly IAgentRegistry registry;
    private readonly IAudioStreamManager streamManager;
    private readonly IAgentRoutingCommander commander;
    private readonly ILogger<ZoneBroadcaster> logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="ZoneBroadcaster"/> class.
    /// </summary>
    /// <param name="registry">The agent registry.</param>
    /// <param name="streamManager">The stream manager used to send audio.</param>
    /// <param name="commander">The commander used to push routing to agents.</param>
    /// <param name="logger">The logger.</param>
    public ZoneBroadcaster(
        IAgentRegistry registry,
        IAudioStreamManager streamManager,
        IAgentRoutingCommander commander,
        ILogger<ZoneBroadcaster> logger)
    {
        this.registry = registry;
        this.streamManager = streamManager;
        this.commander = commander;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public ZoneBroadcastResult PlanAndStart(ZoneInfo zone, ReadOnlyMemory<byte> pcm, StreamFormat format)
    {
        var classification = ZoneTargeting.Classify(registry, streamManager, zone);

        foreach (var target in classification.Targets)
        {
            _ = Task.Run(() => StreamToAgentAsync(target.Agent, target.DeviceIds, pcm, format, zone.Volume));
        }

        var targeted = classification.Targets
            .Select(target => new BroadcastTarget(target.Agent.Id, target.Agent.Name, target.DeviceIds.Count))
            .ToArray();

        return new ZoneBroadcastResult(zone.Id, targeted, classification.Skips);
    }

    /// <summary>
    /// Streams the message to one agent: push routing to the target devices, let it settle, stream,
    /// then restore the agent's configured routing. Exposed for testing the orchestration.
    /// </summary>
    /// <param name="agent">The target agent.</param>
    /// <param name="deviceIds">The agent's device ids to play on.</param>
    /// <param name="pcm">The PCM message.</param>
    /// <param name="format">The PCM format.</param>
    /// <param name="volume">The zone's volume (0.0–1.0) to apply to the message.</param>
    /// <returns>A task that completes when the message has been streamed and routing restored.</returns>
    public async Task StreamToAgentAsync(
        AgentInfo agent,
        IReadOnlyList<string> deviceIds,
        ReadOnlyMemory<byte> pcm,
        StreamFormat format,
        double volume)
    {
        var priorRouting = registry.Get(agent.Id)?.Routing ?? new AgentRouting(Array.Empty<AudioOutput>());
        var channels = Enumerable.Range(0, format.Channels).ToArray();
        var broadcastRouting = new AgentRouting(
            deviceIds.Select(id => new AudioOutput(id, channels)).ToArray());

        try
        {
            await commander.PushRoutingAsync(agent.Id, broadcastRouting);
            await Task.Delay(SettleDelay);
            await streamManager.TryStreamPcmAsync(agent.Id, pcm, format, () => volume, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Broadcast to agent {Agent} failed.", agent.Name);
        }
        finally
        {
            try
            {
                await commander.PushRoutingAsync(agent.Id, priorRouting);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to restore routing for agent {Agent} after a broadcast.", agent.Name);
            }
        }
    }
}
