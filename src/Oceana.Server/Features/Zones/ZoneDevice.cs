namespace Oceana.Server.Features.Zones;

/// <summary>
/// Identifies a single audio device assigned to a zone: a device belonging to a specific agent.
/// </summary>
public sealed record ZoneDevice
{
    /// <summary>
    /// Gets the identifier of the agent that owns the device.
    /// </summary>
    public Guid AgentId { get; init; }

    /// <summary>
    /// Gets the agent-scoped stable identifier of the device.
    /// </summary>
    public string DeviceId { get; init; } = string.Empty;
}
