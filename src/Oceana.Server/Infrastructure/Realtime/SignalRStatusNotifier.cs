using Microsoft.AspNetCore.SignalR;
using Oceana.Server.Features.Agents;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// An <see cref="IStatusNotifier"/> that broadcasts changes over the <see cref="AgentStatusHub"/>.
/// </summary>
public sealed class SignalRStatusNotifier : IStatusNotifier
{
    private readonly IHubContext<AgentStatusHub, IAgentStatusClient> hub;

    /// <summary>
    /// Initialises a new instance of the <see cref="SignalRStatusNotifier"/> class.
    /// </summary>
    /// <param name="hub">The hub context used to reach connected clients.</param>
    public SignalRStatusNotifier(IHubContext<AgentStatusHub, IAgentStatusClient> hub)
    {
        this.hub = hub;
    }

    /// <inheritdoc/>
    public Task NotifyAgentChangedAsync(AgentInfo agent)
    {
        return hub.Clients.All.AgentChanged(agent);
    }

    /// <inheritdoc/>
    public Task NotifyAgentRemovedAsync(Guid agentId)
    {
        return hub.Clients.All.AgentRemoved(agentId);
    }
}
