using Oceana.Contracts;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// The persisted configuration of a known agent: its identity, last-seen devices and desired
/// routing. Live state (connection, host/port, streaming status) is not persisted — it is
/// re-derived when the agent reconnects.
/// </summary>
/// <param name="Id">The agent's stable identifier.</param>
/// <param name="Name">The agent's last-seen name.</param>
/// <param name="Devices">The agent's last-seen render devices.</param>
/// <param name="Routing">The agent's desired channel-to-device routing.</param>
public sealed record AgentConfig(
    Guid Id,
    string Name,
    IReadOnlyList<AudioDevice> Devices,
    AgentRouting Routing);
