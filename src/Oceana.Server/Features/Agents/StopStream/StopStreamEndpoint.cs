using FastEndpoints;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Agents.StopStream;

/// <summary>
/// Endpoint that stops the active stream to an agent.
/// </summary>
public sealed class StopStreamEndpoint(IAudioStreamManager streamManager)
    : Endpoint<StopStreamRequest>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Delete("/agents/{Id}/stream");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(StopStreamRequest req, CancellationToken ct)
    {
        if (streamManager.StopStream(req.Id))
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.NotFoundAsync(ct);
    }
}
