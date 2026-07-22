using Oceana.Server.Infrastructure.Persistence;

namespace Oceana.Server.Features.Zones;

/// <summary>
/// An in-memory <see cref="IZoneRegistry"/> with optional JSON persistence. Writes are serialised
/// by a lock so the unique-name index stays consistent with the zone store, and each change is
/// written through to the configured <see cref="IStateStore{T}"/>; the store is loaded on startup.
/// </summary>
public sealed class ZoneRegistry : IZoneRegistry
{
    private readonly object gate = new object();
    private readonly Dictionary<Guid, ZoneInfo> zones = new Dictionary<Guid, ZoneInfo>();
    private readonly Dictionary<string, Guid> namesToId = new Dictionary<string, Guid>();
    private readonly IStateStore<ZonesState>? store;

    /// <summary>
    /// Initialises a new instance of the <see cref="ZoneRegistry"/> class.
    /// </summary>
    /// <param name="store">The store to persist zones to and load them from; null disables persistence.</param>
    public ZoneRegistry(IStateStore<ZonesState>? store = null)
    {
        this.store = store;

        var state = store?.Load();
        if (state is not null)
        {
            foreach (var zone in state.Zones)
            {
                this.zones[zone.Id] = zone;
                this.namesToId[NameKey(zone.Name)] = zone.Id;
            }
        }
    }

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
            this.Persist();
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
            this.Persist();
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
            this.Persist();
            return true;
        }
    }

    private static string NameKey(string name) => name.Trim().ToLowerInvariant();

    // Called while holding the lock, so the persisted snapshot is always consistent.
    private void Persist() => this.store?.Save(new ZonesState(this.zones.Values.ToList()));
}
