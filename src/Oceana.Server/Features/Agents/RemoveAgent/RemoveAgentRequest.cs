namespace Oceana.Server.Features.Agents.RemoveAgent;

/// <summary>
/// Request to remove an agent by its identifier.
/// </summary>
public sealed class RemoveAgentRequest
{
    /// <summary>
    /// Gets or sets the identifier of the agent to remove.
    /// </summary>
    public Guid Id { get; set; }
}
