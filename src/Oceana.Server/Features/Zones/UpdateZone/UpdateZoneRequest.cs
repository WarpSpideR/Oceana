namespace Oceana.Server.Features.Zones.UpdateZone;

/// <summary>
/// Request to replace a zone's name and devices. The identifier is bound from the route; the
/// remaining fields from the request body.
/// </summary>
public sealed class UpdateZoneRequest
{
    /// <summary>
    /// Gets or sets the identifier of the zone to update.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the new name of the zone.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the devices to assign to the zone.
    /// </summary>
    public List<ZoneDevice> Devices { get; set; } = new List<ZoneDevice>();
}
