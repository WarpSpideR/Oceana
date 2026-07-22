using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// Broadcasts zone changes to connected clients.
/// </summary>
public interface IZoneStatusNotifier
{
    /// <summary>
    /// Notifies clients that a zone was created or changed.
    /// </summary>
    /// <param name="zone">The updated zone snapshot.</param>
    /// <returns>A task that completes once the notification has been dispatched.</returns>
    Task NotifyZoneChangedAsync(ZoneInfo zone);

    /// <summary>
    /// Notifies clients that a zone was removed.
    /// </summary>
    /// <param name="zoneId">The identifier of the removed zone.</param>
    /// <returns>A task that completes once the notification has been dispatched.</returns>
    Task NotifyZoneRemovedAsync(Guid zoneId);
}
