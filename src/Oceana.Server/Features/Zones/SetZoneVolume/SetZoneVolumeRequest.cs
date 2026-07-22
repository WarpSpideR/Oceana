namespace Oceana.Server.Features.Zones.SetZoneVolume;

/// <summary>
/// Request to set a zone's playback volume. The identifier is bound from the route; the volume
/// from the request body.
/// </summary>
public sealed class SetZoneVolumeRequest
{
    /// <summary>
    /// Gets or sets the identifier of the zone.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the volume, from 0.0 (silent) to 1.0 (full).
    /// </summary>
    public double Volume { get; set; }
}
