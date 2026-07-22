namespace Oceana.Server.Features.Zones;

/// <summary>
/// The outcome of attempting to update a zone.
/// </summary>
public enum ZoneUpdateStatus
{
    /// <summary>
    /// The zone was updated successfully.
    /// </summary>
    Updated = 0,

    /// <summary>
    /// No zone with the given identifier exists.
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// Another zone already uses the requested name.
    /// </summary>
    NameConflict = 2,
}
