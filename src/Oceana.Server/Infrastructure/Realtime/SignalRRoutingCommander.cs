using Microsoft.AspNetCore.SignalR;
using Oceana.Contracts;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// An <see cref="IAgentRoutingCommander"/> that pushes to an agent's group over the <see cref="AgentControlHub"/>.
/// </summary>
public sealed class SignalRRoutingCommander : IAgentRoutingCommander
{
    private readonly IHubContext<AgentControlHub, IAgentControlClient> hub;

    /// <summary>
    /// Initialises a new instance of the <see cref="SignalRRoutingCommander"/> class.
    /// </summary>
    /// <param name="hub">The control hub context used to reach connected agents.</param>
    public SignalRRoutingCommander(IHubContext<AgentControlHub, IAgentControlClient> hub)
    {
        this.hub = hub;
    }

    /// <inheritdoc/>
    public Task PushRoutingAsync(Guid agentId, AgentRouting routing)
    {
        return hub.Clients.Group(agentId.ToString()).SetRouting(routing);
    }
}
