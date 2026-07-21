namespace Oceana.Server.Features.Agents.StartStream;

/// <summary>
/// Describes the test-tone stream that was accepted for an agent.
/// </summary>
public sealed record StartStreamResponse
{
    /// <summary>
    /// Gets the identifier of the agent being streamed to.
    /// </summary>
    public Guid AgentId { get; init; }

    /// <summary>
    /// Gets the tone frequency, in hertz.
    /// </summary>
    public double Frequency { get; init; }
}
