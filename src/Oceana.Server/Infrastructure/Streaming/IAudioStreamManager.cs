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
