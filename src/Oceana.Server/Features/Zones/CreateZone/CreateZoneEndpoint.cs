using FastEndpoints;
using Oceana.Server.Features.Zones.GetZone;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Zones.CreateZone;

/// <summary>
/// Endpoint that creates a zone.
/// </summary>
public sealed class CreateZoneEndpoint(IZoneRegistry registry, IZoneStatusNotifier notifier)
    : Endpoint<CreateZoneRequest, ZoneInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Post("/zones");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CreateZoneRequest req, CancellationToken ct)
    {
        var zone = registry.Create(req.Name, req.Devices);
        if (zone is null)
        {
            AddError("A zone with this name already exists.");
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await notifier.NotifyZoneChangedAsync(zone);
        await Send.CreatedAtAsync<GetZoneEndpoint>(new { Id = zone.Id }, zone, cancellation: ct);
    }
}
