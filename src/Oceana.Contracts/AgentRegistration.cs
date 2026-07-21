namespace Oceana.Contracts;

/// <summary>
/// The details an agent reports to the server when it registers over the control connection.
/// </summary>
public sealed class AgentRegistration
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AgentRegistration"/> class.
    /// </summary>
    /// <param name="agentId">The stable, agent-assigned identifier.</param>
    /// <param name="name">The human-readable agent name (typically the machine name).</param>
    /// <param name="audioPort">The TCP port on which the agent listens for audio.</param>
    /// <param name="devices">The render devices available on the agent.</param>
    public AgentRegistration(Guid agentId, string name, int audioPort, IReadOnlyList<AudioDevice> devices)
    {
        AgentId = agentId;
        Name = name;
        AudioPort = audioPort;
        Devices = devices;
    }

    /// <summary>
    /// Gets the stable, agent-assigned identifier.
    /// </summary>
    public Guid AgentId { get; }

    /// <summary>
    /// Gets the human-readable agent name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the TCP port on which the agent listens for audio.
    /// </summary>
    public int AudioPort { get; }

    /// <summary>
    /// Gets the render devices available on the agent.
    /// </summary>
    public IReadOnlyList<AudioDevice> Devices { get; }
}
