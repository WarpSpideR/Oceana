using FastEndpoints;
using Oceana.Server.Infrastructure.Audio;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Zones.PlayZone;

/// <summary>
/// Endpoint that plays an uploaded audio file (mono/stereo, 48 kHz, 16-bit PCM WAV in the request
/// body) to every reachable device in a zone.
/// </summary>
public sealed class PlayZoneEndpoint(IZoneRegistry registry, IZonePlaybackService playback)
    : EndpointWithoutRequest<ZonePlaybackState>
{
    private const int MaxBodyBytes = 256 * 1024 * 1024;

    /// <inheritdoc/>
    public override void Configure()
    {
        // No request DTO: the body is raw audio/wav, so model binding is bypassed; the route id,
        // query name and body are read manually below.
        Post("/zones/{Id}/play");
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
            AddError("The audio is too large.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        if (!WavReader.TryRead(buffer.ToArray(), out var audio, out var error))
        {
            AddError(error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var name = Query<string>("name", isRequired: false);
        var sourceName = string.IsNullOrWhiteSpace(name) ? "Audio" : name;

        var state = playback.Start(zone, sourceName, audio.Pcm, audio.Format);
        await Send.ResponseAsync(state, 202, ct);
    }
}
