namespace Oceana.Server.Features.Agents;

/// <summary>
/// A registered agent together with its current streaming state.
/// </summary>
public sealed record AgentInfo
{
    /// <summary>
    /// Gets the unique identifier assigned to the agent when it was registered.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the human-readable name of the agent.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the host name or IP address on which the agent is listening.
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Gets the TCP port on which the agent is listening.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets the current status of the agent.
    /// </summary>
    public AgentStatus Status { get; init; } = AgentStatus.Idle;

    /// <summary>
    /// Gets the message describing the most recent fault, or null when the agent is healthy.
    /// </summary>
    public string? LastError { get; init; }
}
