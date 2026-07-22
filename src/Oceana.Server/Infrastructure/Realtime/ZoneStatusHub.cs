using Microsoft.AspNetCore.SignalR;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// The SignalR hub that pushes zone changes to connected front ends.
/// </summary>
public sealed class ZoneStatusHub : Hub<IZoneStatusClient>
{
}
