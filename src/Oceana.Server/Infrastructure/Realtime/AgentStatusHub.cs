using Microsoft.AspNetCore.SignalR;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// The SignalR hub that pushes agent status changes to connected front ends.
/// </summary>
public sealed class AgentStatusHub : Hub<IAgentStatusClient>
{
}
