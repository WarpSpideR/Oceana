namespace Oceana.Server.Features.Zones.RemoveZone;

/// <summary>
/// Request to remove a zone by its identifier.
/// </summary>
public sealed class RemoveZoneRequest
{
    /// <summary>
    /// Gets or sets the identifier of the zone to remove.
    /// </summary>
    public Guid Id { get; set; }
}
