namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Parameters controlling test-tone generation for a stream.
/// </summary>
public sealed record ToneOptions
{
    /// <summary>
    /// Gets the tone frequency, in hertz.
    /// </summary>
    public double Frequency { get; init; } = 440.0;

    /// <summary>
    /// Gets the optional duration; when null the tone plays until the stream is cancelled.
    /// </summary>
    public TimeSpan? Duration { get; init; }
}
