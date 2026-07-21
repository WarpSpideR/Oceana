using FastEndpoints;
using Oceana.Server.Features.Agents.GetAgent;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Agents.RegisterAgent;

/// <summary>
/// Endpoint that registers a new agent and announces it to connected clients.
/// </summary>
public sealed class RegisterAgentEndpoint(IAgentRegistry registry, IStatusNotifier notifier)
    : Endpoint<RegisterAgentRequest, AgentInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Post("/agents");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RegisterAgentRequest req, CancellationToken ct)
    {
        var agent = registry.Register(req.Name, req.Host, req.Port);
        await notifier.NotifyAgentChangedAsync(agent);
        await Send.CreatedAtAsync<GetAgentEndpoint>(new { agent.Id }, agent, cancellation: ct);
    }
}
