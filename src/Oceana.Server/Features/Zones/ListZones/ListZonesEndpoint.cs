using FastEndpoints;

namespace Oceana.Server.Features.Zones.ListZones;

/// <summary>
/// Endpoint that returns every configured zone.
/// </summary>
public sealed class ListZonesEndpoint(IZoneRegistry registry)
    : EndpointWithoutRequest<IReadOnlyCollection<ZoneInfo>>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Get("/zones");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(registry.GetAll(), ct);
    }
}
