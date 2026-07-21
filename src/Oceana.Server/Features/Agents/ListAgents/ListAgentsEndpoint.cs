using FastEndpoints;

namespace Oceana.Server.Features.Agents.ListAgents;

/// <summary>
/// Endpoint that returns every registered agent.
/// </summary>
public sealed class ListAgentsEndpoint(IAgentRegistry registry)
    : EndpointWithoutRequest<IReadOnlyCollection<AgentInfo>>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Get("/agents");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(registry.GetAll(), ct);
    }
}
