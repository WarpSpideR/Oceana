using FastEndpoints;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Zones.UpdateZone;

/// <summary>
/// Endpoint that replaces a zone's name and devices.
/// </summary>
public sealed class UpdateZoneEndpoint(IZoneRegistry registry, IZoneStatusNotifier notifier)
    : Endpoint<UpdateZoneRequest, ZoneInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Put("/zones/{Id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UpdateZoneRequest req, CancellationToken ct)
    {
        var result = registry.Update(req.Id, req.Name, req.Devices);
        switch (result.Status)
        {
            case ZoneUpdateStatus.NotFound:
                await Send.NotFoundAsync(ct);
                return;

            case ZoneUpdateStatus.NameConflict:
                AddError("A zone with this name already exists.");
                await Send.ErrorsAsync(409, ct);
                return;

            default:
                await notifier.NotifyZoneChangedAsync(result.Zone!);
                await Send.OkAsync(result.Zone!, ct);
                return;
        }
    }
}
