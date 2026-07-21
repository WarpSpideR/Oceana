namespace Oceana.Server.Features.Agents.StopStream;

/// <summary>
/// Request to stop the active stream to an agent.
/// </summary>
public sealed class StopStreamRequest
{
    /// <summary>
    /// Gets or sets the identifier of the agent whose stream should be stopped.
    /// </summary>
    public Guid Id { get; set; }
}
