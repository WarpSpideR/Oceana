using FastEndpoints;

namespace Oceana.Server.Features.Agents.GetAgent;

/// <summary>
/// Endpoint that returns a single agent by its identifier.
/// </summary>
public sealed class GetAgentEndpoint(IAgentRegistry registry)
    : Endpoint<GetAgentRequest, AgentInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Get("/agents/{Id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAgentRequest req, CancellationToken ct)
    {
        var agent = registry.Get(req.Id);
        if (agent is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(agent, ct);
    }
}
