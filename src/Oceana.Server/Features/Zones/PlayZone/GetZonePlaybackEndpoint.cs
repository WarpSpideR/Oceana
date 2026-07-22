using FastEndpoints;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Zones.PlayZone;

/// <summary>
/// Endpoint that returns a zone's current playback state.
/// </summary>
public sealed class GetZonePlaybackEndpoint(IZoneRegistry registry, IZonePlaybackService playback)
    : EndpointWithoutRequest<ZonePlaybackState>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Get("/zones/{Id}/playback");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        if (registry.Get(id) is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var state = playback.Get(id);
        if (state is null)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.OkAsync(state, ct);
    }
}
