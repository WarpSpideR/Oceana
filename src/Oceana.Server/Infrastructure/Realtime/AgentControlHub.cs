using System.Net;
using Microsoft.AspNetCore.SignalR;
using Oceana.Contracts;
using Oceana.Server.Features.Agents;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// The SignalR hub that agents connect to in order to self-register and receive routing changes.
/// </summary>
public sealed class AgentControlHub : Hub<IAgentControlClient>
{
    private readonly IAgentRegistry registry;
    private readonly IStatusNotifier notifier;

    /// <summary>
    /// Initialises a new instance of the <see cref="AgentControlHub"/> class.
    /// </summary>
    /// <param name="registry">The agent registry.</param>
    /// <param name="notifier">The notifier used to inform the front end of changes.</param>
    public AgentControlHub(IAgentRegistry registry, IStatusNotifier notifier)
    {
        this.registry = registry;
        this.notifier = notifier;
    }

    /// <summary>
    /// Called by an agent to register (or re-register) itself; returns the agent's desired routing.
    /// </summary>
    /// <param name="registration">The details reported by the agent.</param>
    /// <returns>The routing the agent should adopt.</returns>
    public async Task<AgentRouting> Register(AgentRegistration registration)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, registration.AgentId.ToString());
        var host = ResolveHost(Context.GetHttpContext()?.Connection.RemoteIpAddress);
        var agent = registry.RegisterOrUpdate(registration, host, Context.ConnectionId);
        await notifier.NotifyAgentChangedAsync(agent);
        return agent.Routing;
    }

    /// <inheritdoc/>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var agent = registry.MarkOffline(Context.ConnectionId);
        if (agent is not null)
        {
            await notifier.NotifyAgentChangedAsync(agent);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string ResolveHost(IPAddress? remote)
    {
        if (remote is null)
        {
            return "127.0.0.1";
        }

        if (remote.IsIPv4MappedToIPv6)
        {
            return remote.MapToIPv4().ToString();
        }

        // Normalise loopback (e.g. ::1) to the IPv4 loopback the agent's audio listener binds.
        return IPAddress.IsLoopback(remote) ? "127.0.0.1" : remote.ToString();
    }
}
