namespace Oceana.Server.Features.Zones.GetZone;

/// <summary>
/// Request to fetch a zone by its identifier.
/// </summary>
public sealed class GetZoneRequest
{
    /// <summary>
    /// Gets or sets the identifier of the zone.
    /// </summary>
    public Guid Id { get; set; }
}
