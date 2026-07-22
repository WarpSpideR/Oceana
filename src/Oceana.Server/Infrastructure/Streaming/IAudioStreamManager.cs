namespace Oceana.Server.Infrastructure.Streaming;

/// <summary>
/// Manages active audio streams from the server to agents.
/// </summary>
public interface IAudioStreamManager
{
    /// <summary>
    /// Starts streaming a test tone to the given agent.
    /// </summary>
    /// <param name="agentId">The identifier of the agent to stream to.</param>
    /// <param name="options">The tone parameters.</param>
    /// <returns>True when a new stream was started; false when the agent is unknown or already streaming.</returns>
    bool TryStartStream(Guid agentId, ToneOptions options);

    /// <summary>
    /// Streams a finite in-memory PCM buffer to the given agent, awaiting completion.
    /// Runs at most one stream per agent, sharing the same guard as the test-tone stream.
    /// </summary>
    /// <param name="agentId">The identifier of the agent to stream to.</param>
    /// <param name="pcm">The interleaved PCM samples to stream.</param>
    /// <param name="format">The format of the PCM buffer.</param>
    /// <param name="volume">
    /// A gain (0.0–1.0) read once per ~20 ms chunk so changes apply live; applied to 16-bit PCM only.
    /// </param>
    /// <param name="cancellationToken">A token used to stop the stream.</param>
    /// <returns>
    /// True once the buffer has been streamed; false when the agent is unknown or already streaming.
    /// </returns>
    Task<bool> TryStreamPcmAsync(
        Guid agentId,
        ReadOnlyMemory<byte> pcm,
        StreamFormat format,
        Func<double> volume,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stops any active stream to the given agent.
    /// </summary>
    /// <param name="agentId">The identifier of the agent.</param>
    /// <returns>True when a stream was stopped; otherwise false.</returns>
    bool StopStream(Guid agentId);

    /// <summary>
    /// Gets a value indicating whether the given agent currently has an active stream.
    /// </summary>
    /// <param name="agentId">The identifier of the agent.</param>
    /// <returns>True when a stream is active for the agent.</returns>
    bool IsStreaming(Guid agentId);
}
