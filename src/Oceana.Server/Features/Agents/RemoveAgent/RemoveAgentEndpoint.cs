using FastEndpoints;
using Oceana.Server.Infrastructure.Realtime;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Agents.RemoveAgent;

/// <summary>
/// Endpoint that removes an agent, stopping any active stream first.
/// </summary>
public sealed class RemoveAgentEndpoint(
    IAgentRegistry registry,
    IAudioStreamManager streamManager,
    IStatusNotifier notifier)
    : Endpoint<RemoveAgentRequest>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Delete("/agents/{Id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveAgentRequest req, CancellationToken ct)
    {
        streamManager.StopStream(req.Id);
        if (!registry.Remove(req.Id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await notifier.NotifyAgentRemovedAsync(req.Id);
        await Send.NoContentAsync(ct);
    }
}
