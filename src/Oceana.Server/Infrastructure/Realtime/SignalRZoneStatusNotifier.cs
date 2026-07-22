using Microsoft.AspNetCore.SignalR;
using Oceana.Server.Features.Zones;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// An <see cref="IZoneStatusNotifier"/> that broadcasts changes over the <see cref="ZoneStatusHub"/>.
/// </summary>
public sealed class SignalRZoneStatusNotifier : IZoneStatusNotifier
{
    private readonly IHubContext<ZoneStatusHub, IZoneStatusClient> hub;

    /// <summary>
    /// Initialises a new instance of the <see cref="SignalRZoneStatusNotifier"/> class.
    /// </summary>
    /// <param name="hub">The hub context used to reach connected clients.</param>
    public SignalRZoneStatusNotifier(IHubContext<ZoneStatusHub, IZoneStatusClient> hub)
    {
        this.hub = hub;
    }

    /// <inheritdoc/>
    public Task NotifyZoneChangedAsync(ZoneInfo zone)
    {
        return this.hub.Clients.All.ZoneChanged(zone);
    }

    /// <inheritdoc/>
    public Task NotifyZoneRemovedAsync(Guid zoneId)
    {
        return this.hub.Clients.All.ZoneRemoved(zoneId);
    }

    /// <inheritdoc/>
    public Task NotifyZonePlaybackChangedAsync(ZonePlaybackState state)
    {
        return this.hub.Clients.All.ZonePlaybackChanged(state);
    }
}
