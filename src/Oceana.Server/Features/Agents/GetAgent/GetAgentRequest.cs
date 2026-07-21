namespace Oceana.Server.Features.Agents.GetAgent;

/// <summary>
/// Request to fetch a single agent by its identifier.
/// </summary>
public sealed class GetAgentRequest
{
    /// <summary>
    /// Gets or sets the identifier of the agent to fetch.
    /// </summary>
    public Guid Id { get; set; }
}
