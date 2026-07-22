using FastEndpoints;

namespace Oceana.Server.Features.Zones.GetZone;

/// <summary>
/// Endpoint that returns a single zone by its identifier.
/// </summary>
public sealed class GetZoneEndpoint(IZoneRegistry registry)
    : Endpoint<GetZoneRequest, ZoneInfo>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Get("/zones/{Id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetZoneRequest req, CancellationToken ct)
    {
        var zone = registry.Get(req.Id);
        if (zone is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(zone, ct);
    }
}
