using FastEndpoints;
using Oceana.Server.Infrastructure.Realtime;

namespace Oceana.Server.Features.Zones.RemoveZone;

/// <summary>
/// Endpoint that removes a zone.
/// </summary>
public sealed class RemoveZoneEndpoint(IZoneRegistry registry, IZoneStatusNotifier notifier)
    : Endpoint<RemoveZoneRequest>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Delete("/zones/{Id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveZoneRequest req, CancellationToken ct)
    {
        if (!registry.Remove(req.Id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await notifier.NotifyZoneRemovedAsync(req.Id);
        await Send.NoContentAsync(ct);
    }
}
