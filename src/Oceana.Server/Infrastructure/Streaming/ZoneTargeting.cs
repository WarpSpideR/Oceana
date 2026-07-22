using Oceana.Server.Features.Agents;
using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Classifies a zone's devices (grouped by owning agent) into agents that can be played to now and
/// agents that must be skipped (offline, busy, or with no currently-reported device). Shared by the
/// zone broadcast and zone playback fan-outs.
/// </summary>
public static class ZoneTargeting
{
    /// <summary>
    /// Classifies the zone's agents for a fan-out.
    /// </summary>
    /// <param name="registry">The agent registry.</param>
    /// <param name="streamManager">The stream manager (used to detect busy agents).</param>
    /// <param name="zone">The zone whose devices to classify.</param>
    /// <returns>The reachable targets and the skips with reasons.</returns>
    public static ZoneTargetingResult Classify(
        IAgentRegistry registry,
        IAudioStreamManager streamManager,
        ZoneInfo zone)
    {
        var targets = new List<ZoneTarget>();
        var skips = new List<BroadcastSkip>();

        foreach (var group in zone.Devices.GroupBy(device => device.AgentId))
        {
            var agentId = group.Key;
            var deviceIds = group.Select(device => device.DeviceId).Distinct().ToArray();
            var agent = registry.Get(agentId);

            if (agent is null || !agent.Connected)
            {
                skips.Add(new BroadcastSkip(agentId, agent?.Name ?? agentId.ToString(), BroadcastSkipReason.Offline));
                continue;
            }

            if (streamManager.IsStreaming(agentId))
            {
                skips.Add(new BroadcastSkip(agentId, agent.Name, BroadcastSkipReason.Busy));
                continue;
            }

            var reported = new HashSet<string>(agent.Devices.Select(device => device.Id), StringComparer.Ordinal);
            var activeDeviceIds = deviceIds.Where(reported.Contains).ToArray();
            if (activeDeviceIds.Length == 0)
            {
                skips.Add(new BroadcastSkip(agentId, agent.Name, BroadcastSkipReason.NoActiveDevices));
                continue;
            }

            targets.Add(new ZoneTarget(agent, activeDeviceIds));
        }

        return new ZoneTargetingResult(targets, skips);
    }
}
