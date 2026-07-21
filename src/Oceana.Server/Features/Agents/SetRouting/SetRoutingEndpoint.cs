using FastEndpoints;
using Oceana.Contracts;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Agents.SetRouting;

/// <summary>
/// Endpoint that sets an agent's desired routing and pushes it to the agent if connected.
/// </summary>
public sealed class SetRoutingEndpoint(IAgentRegistry registry, IAgentRoutingCommander commander, IStatusNotifier notifier)
    : Endpoint<SetRoutingRequest, AgentInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Put("/agents/{Id}/routing");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetRoutingRequest req, CancellationToken ct)
    {
        var routing = new AgentRouting(req.Outputs);
        var updated = registry.SetDesiredRouting(req.Id, routing);
        if (updated is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await commander.PushRoutingAsync(req.Id, routing);
        await notifier.NotifyAgentChangedAsync(updated);
        await Send.OkAsync(updated, ct);
    }
}
