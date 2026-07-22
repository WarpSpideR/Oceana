namespace Oceana.Server.Features.Zones;

/// <summary>
/// Stores the configured set of zones. Zone names are unique, compared case-insensitively.
/// </summary>
public interface IZoneRegistry
{
    /// <summary>
    /// Creates a new zone with a generated identifier.
    /// </summary>
    /// <param name="name">The zone name; must be unique (case-insensitive).</param>
    /// <param name="devices">The devices to assign to the zone.</param>
    /// <returns>The created zone, or null when the name is already taken.</returns>
    ZoneInfo? Create(string name, IReadOnlyList<ZoneDevice> devices);

    /// <summary>
    /// Returns every configured zone.
    /// </summary>
    /// <returns>A snapshot of all zones.</returns>
    IReadOnlyCollection<ZoneInfo> GetAll();

    /// <summary>
    /// Returns the zone with the given identifier.
    /// </summary>
    /// <param name="id">The identifier of the zone.</param>
    /// <returns>The zone, or null when no zone has the given identifier.</returns>
    ZoneInfo? Get(Guid id);

    /// <summary>
    /// Replaces a zone's name and devices.
    /// </summary>
    /// <param name="id">The identifier of the zone to update.</param>
    /// <param name="name">The new name; must be unique (case-insensitive), ignoring this zone's own current name.</param>
    /// <param name="devices">The devices to assign to the zone.</param>
    /// <returns>The outcome of the update, carrying the updated zone on success.</returns>
    ZoneUpdateResult Update(Guid id, string name, IReadOnlyList<ZoneDevice> devices);

    /// <summary>
    /// Removes the zone with the given identifier.
    /// </summary>
    /// <param name="id">The identifier of the zone to remove.</param>
    /// <returns>True when a zone was removed; otherwise false.</returns>
    bool Remove(Guid id);
}
