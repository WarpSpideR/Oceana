using Oceana.Contracts;

namespace Oceana.Server.Features.Agents.SetRouting;

/// <summary>
/// Request to set an agent's channel→device routing. The identifier is bound from the route; the
/// outputs from the request body.
/// </summary>
public sealed class SetRoutingRequest
{
    /// <summary>
    /// Gets or sets the identifier of the target agent.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the outputs to route; an empty list means play all channels to the default device.
    /// </summary>
    public List<AudioOutput> Outputs { get; set; } = new List<AudioOutput>();
}
