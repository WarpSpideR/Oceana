using Oceana.Contracts;

namespace Oceana.Server.Features.Agents;

/// <summary>
/// A registered agent together with its current connection, streaming state, devices and routing.
/// </summary>
public sealed record AgentInfo
{
    /// <summary>
    /// Gets the stable identifier the agent assigned to itself.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the human-readable name of the agent.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the host name or IP address the server dials for audio (derived from the control connection).
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Gets the TCP port on which the agent listens for audio.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets a value indicating whether the agent's control connection is currently established.
    /// </summary>
    public bool Connected { get; init; }

    /// <summary>
    /// Gets the current streaming status of the agent.
    /// </summary>
    public AgentStatus Status { get; init; } = AgentStatus.Idle;

    /// <summary>
    /// Gets the message describing the most recent fault, or null when the agent is healthy.
    /// </summary>
    public string? LastError { get; init; }

    /// <summary>
    /// Gets the render devices reported by the agent.
    /// </summary>
    public IReadOnlyList<AudioDevice> Devices { get; init; } = Array.Empty<AudioDevice>();

    /// <summary>
    /// Gets the desired channel→device routing for the agent.
    /// </summary>
    public AgentRouting Routing { get; init; } = new AgentRouting(Array.Empty<AudioOutput>());
}
