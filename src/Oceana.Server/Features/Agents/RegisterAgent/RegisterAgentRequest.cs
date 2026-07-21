namespace Oceana.Server.Features.Agents.RegisterAgent;

/// <summary>
/// Request to register a new agent with the server.
/// </summary>
public sealed class RegisterAgentRequest
{
    /// <summary>
    /// Gets or sets the human-readable name of the agent.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host name or IP address on which the agent is listening.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TCP port on which the agent is listening.
    /// </summary>
    public int Port { get; set; } = 8090;
}
