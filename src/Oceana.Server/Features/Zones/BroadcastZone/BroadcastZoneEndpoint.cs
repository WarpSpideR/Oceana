using FastEndpoints;
using Oceana.Server.Infrastructure.Audio;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Zones.BroadcastZone;

/// <summary>
/// Endpoint that broadcasts a recorded message (uploaded as a mono/48 kHz/16-bit PCM WAV in the
/// request body) to every reachable device in a zone.
/// </summary>
public sealed class BroadcastZoneEndpoint(IZoneRegistry registry, IZoneBroadcaster broadcaster)
    : EndpointWithoutRequest<ZoneBroadcastResult>
{
    private const int MaxBodyBytes = 16 * 1024 * 1024;

    /// <inheritdoc/>
    public override void Configure()
    {
        // No request DTO: the body is raw audio/wav, so model binding (and its content-type
        // negotiation) is bypassed; the route id and body are read manually below.
        Post("/zones/{Id}/broadcast");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var zone = registry.Get(id);
        if (zone is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        using var buffer = new MemoryStream();
        await HttpContext.Request.Body.CopyToAsync(buffer, ct);

        if (buffer.Length == 0)
        {
            AddError("No audio was uploaded.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        if (buffer.Length > MaxBodyBytes)
        {
            AddError("The recording is too large.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        if (!WavReader.TryRead(buffer.ToArray(), out var audio, out var error))
        {
            AddError(error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var result = broadcaster.PlanAndStart(zone, audio.Pcm, audio.Format);
        await Send.ResponseAsync(result, 202, ct);
    }
}
