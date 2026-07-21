namespace Oceana.Server.Features.Agents.StartStream;

/// <summary>
/// Request to start a test-tone stream to an agent. The identifier is bound from the route;
/// the remaining fields are bound from the request body.
/// </summary>
public sealed class StartStreamRequest
{
    /// <summary>
    /// Gets or sets the identifier of the target agent.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tone frequency, in hertz.
    /// </summary>
    public double Frequency { get; set; } = 440.0;

    /// <summary>
    /// Gets or sets the optional duration, in seconds; when null the tone plays until it is stopped.
    /// </summary>
    public double? DurationSeconds { get; set; }
}
