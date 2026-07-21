using FastEndpoints;
using Oceana.Server.Infrastructure.Streaming;

namespace Oceana.Server.Features.Agents.StartStream;

/// <summary>
/// Endpoint that starts streaming a test tone to an agent.
/// </summary>
public sealed class StartStreamEndpoint(IAgentRegistry registry, IAudioStreamManager streamManager)
    : Endpoint<StartStreamRequest, StartStreamResponse>
{
    /// <inheritdoc/>
    public override void Configure()
    {
        Post("/agents/{Id}/stream");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(StartStreamRequest req, CancellationToken ct)
    {
        if (registry.Get(req.Id) is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var options = new ToneOptions
        {
            Frequency = req.Frequency,
            Channels = req.Channels,
            Duration = req.DurationSeconds is { } seconds ? TimeSpan.FromSeconds(seconds) : null,
        };

        if (!streamManager.TryStartStream(req.Id, options))
        {
            AddError("The agent is already streaming.");
            await Send.ErrorsAsync(409, ct);
            return;
        }

        var response = new StartStreamResponse { AgentId = req.Id, Frequency = req.Frequency };
        await Send.ResponseAsync(response, 202, ct);
    }
}
