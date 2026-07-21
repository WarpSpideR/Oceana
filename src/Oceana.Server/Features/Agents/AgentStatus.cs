namespace Oceana.Server.Features.Agents;

/// <summary>
/// Describes the current state of a registered agent.
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// The agent is registered but no stream is active.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// The server is establishing a connection to the agent.
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// The server is actively streaming audio to the agent.
    /// </summary>
    Streaming = 2,

    /// <summary>
    /// The most recent operation against the agent failed.
    /// </summary>
    Faulted = 3,
}
