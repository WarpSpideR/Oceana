using FastEndpoints;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Zones.SetZoneVolume;

/// <summary>
/// Endpoint that sets a zone's playback volume.
/// </summary>
public sealed class SetZoneVolumeEndpoint(IZoneRegistry registry, IZoneStatusNotifier notifier)
    : Endpoint<SetZoneVolumeRequest, ZoneInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Put("/zones/{Id}/volume");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetZoneVolumeRequest req, CancellationToken ct)
    {
        var result = registry.SetVolume(req.Id, req.Volume);
        if (result.Status == ZoneUpdateStatus.NotFound)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await notifier.NotifyZoneChangedAsync(result.Zone!);
        await Send.OkAsync(result.Zone!, ct);
    }
}
