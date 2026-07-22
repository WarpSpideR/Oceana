namespace Oceana.Server.Features.Zones;

/// <summary>
/// An in-memory <see cref="IZoneRegistry"/>. Writes are serialised by a lock so the unique-name
/// index stays consistent with the zone store; nothing is persisted, so it is empty on restart.
/// </summary>
public sealed class ZoneRegistry : IZoneRegistry
{
    private readonly object gate = new object();
    private readonly Dictionary<Guid, ZoneInfo> zones = new Dictionary<Guid, ZoneInfo>();
    private readonly Dictionary<string, Guid> namesToId = new Dictionary<string, Guid>();

    /// <inheritdoc/>
    public ZoneInfo? Create(string name, IReadOnlyList<ZoneDevice> devices)
    {
        var trimmed = name.Trim();
        var key = NameKey(trimmed);

        lock (this.gate)
        {
            if (this.namesToId.ContainsKey(key))
            {
                return null;
            }

            var zone = new ZoneInfo
            {
                Id = Guid.NewGuid(),
                Name = trimmed,
                Devices = devices,
            };

            this.zones[zone.Id] = zone;
            this.namesToId[key] = zone.Id;
            return zone;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<ZoneInfo> GetAll()
    {
        lock (this.gate)
        {
            return this.zones.Values.ToArray();
        }
    }

    /// <inheritdoc/>
    public ZoneInfo? Get(Guid id)
    {
        lock (this.gate)
        {
            return this.zones.TryGetValue(id, out var zone) ? zone : null;
        }
    }

    /// <inheritdoc/>
    public ZoneUpdateResult Update(Guid id, string name, IReadOnlyList<ZoneDevice> devices)
    {
        var trimmed = name.Trim();
        var key = NameKey(trimmed);

        lock (this.gate)
        {
            if (!this.zones.TryGetValue(id, out var existing))
            {
                return ZoneUpdateResult.NotFound;
            }

            if (this.namesToId.TryGetValue(key, out var owner) && owner != id)
            {
                return ZoneUpdateResult.NameConflict;
            }

            this.namesToId.Remove(NameKey(existing.Name));
            var updated = existing with { Name = trimmed, Devices = devices };
            this.zones[id] = updated;
            this.namesToId[key] = id;
            return ZoneUpdateResult.Success(updated);
        }
    }

    /// <inheritdoc/>
    public bool Remove(Guid id)
    {
        lock (this.gate)
        {
            if (!this.zones.Remove(id, out var removed))
            {
                return false;
            }

            this.namesToId.Remove(NameKey(removed.Name));
            return true;
        }
    }

    private static string NameKey(string name) => name.Trim().ToLowerInvariant();
}
