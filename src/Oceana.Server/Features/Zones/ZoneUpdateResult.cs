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

/// <summary>
/// The result of an update attempt: a status and, when successful, the updated zone.
/// </summary>
/// <param name="Status">The outcome of the update.</param>
/// <param name="Zone">The updated zone when <paramref name="Status"/> is <see cref="ZoneUpdateStatus.Updated"/>; otherwise null.</param>
public readonly record struct ZoneUpdateResult(ZoneUpdateStatus Status, ZoneInfo? Zone)
{
    /// <summary>
    /// Creates a successful result carrying the updated zone.
    /// </summary>
    /// <param name="zone">The updated zone.</param>
    /// <returns>An <see cref="ZoneUpdateResult"/> with status <see cref="ZoneUpdateStatus.Updated"/>.</returns>
    public static ZoneUpdateResult Success(ZoneInfo zone) => new ZoneUpdateResult(ZoneUpdateStatus.Updated, zone);

    /// <summary>
    /// Gets a result indicating that no matching zone was found.
    /// </summary>
    public static ZoneUpdateResult NotFound { get; } = new ZoneUpdateResult(ZoneUpdateStatus.NotFound, null);

    /// <summary>
    /// Gets a result indicating that another zone already uses the requested name.
    /// </summary>
    public static ZoneUpdateResult NameConflict { get; } = new ZoneUpdateResult(ZoneUpdateStatus.NameConflict, null);
}
