using FastEndpoints;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Zones.PlayZone;

/// <summary>
/// Endpoint that stops a zone's current playback.
/// </summary>
public sealed class StopZonePlaybackEndpoint(IZonePlaybackService playback)
    : EndpointWithoutRequest
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Delete("/zones/{Id}/play");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        if (playback.Stop(id))
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.NotFoundAsync(ct);
    }
}
