namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Parameters controlling test-tone generation for a stream.
/// </summary>
public sealed record ToneOptions
{
    /// <summary>
    /// Gets the base tone frequency, in hertz. Channel n is generated at this frequency multiplied by (n + 1).
    /// </summary>
    public double Frequency { get; init; } = 440.0;

    /// <summary>
    /// Gets the number of channels to generate.
    /// </summary>
    public int Channels { get; init; } = 2;

    /// <summary>
    /// Gets the optional duration; when null the tone plays until the stream is cancelled.
    /// </summary>
    public TimeSpan? Duration { get; init; }
}
