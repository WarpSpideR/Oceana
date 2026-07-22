using Oceana.Server.Features.Zones;

namespace Oceana.Server.Infrastructure.Realtime;

/// <summary>
/// The strongly-typed client methods a connected front end implements to receive zone updates.
/// </summary>
public interface IZoneStatusClient
{
    /// <summary>
    /// Called when a zone is created or changes.
    /// </summary>
    /// <param name="zone">The updated zone snapshot.</param>
    /// <returns>A task representing the client invocation.</returns>
    Task ZoneChanged(ZoneInfo zone);

    /// <summary>
    /// Called when a zone is removed.
    /// </summary>
    /// <param name="zoneId">The identifier of the removed zone.</param>
    /// <returns>A task representing the client invocation.</returns>
    Task ZoneRemoved(Guid zoneId);
}
