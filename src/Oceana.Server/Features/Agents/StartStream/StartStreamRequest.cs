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
    /// Gets or sets the base tone frequency, in hertz. Each channel plays a distinct multiple of this.
    /// </summary>
    public double Frequency { get; set; } = 440.0;

    /// <summary>
    /// Gets or sets the number of channels to stream.
    /// </summary>
    public int Channels { get; set; } = 2;

    /// <summary>
    /// Gets or sets the optional duration, in seconds; when null the tone plays until it is stopped.
    /// </summary>
    public double? DurationSeconds { get; set; }
}
