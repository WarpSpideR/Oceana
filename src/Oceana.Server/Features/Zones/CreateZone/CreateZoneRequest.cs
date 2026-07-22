namespace Oceana.Server.Features.Zones.CreateZone;

/// <summary>
/// Request to create a zone.
/// </summary>
public sealed class CreateZoneRequest
{
    /// <summary>
    /// Gets or sets the name of the zone.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the devices to assign to the zone.
    /// </summary>
    public List<ZoneDevice> Devices { get; set; } = new List<ZoneDevice>();
}
