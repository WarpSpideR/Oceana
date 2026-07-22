namespace Oceana.Server.Features.Zones;

/// <summary>
/// The persisted snapshot of all configured zones.
/// </summary>
/// <param name="Zones">The zones to persist.</param>
public sealed record ZonesState(IReadOnlyList<ZoneInfo> Zones);
