namespace Oceana.Contracts;

/// <summary>
/// The full channel→device routing for an agent.
/// </summary>
public sealed class AgentRouting
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AgentRouting"/> class.
    /// </summary>
    /// <param name="outputs">The configured outputs; empty means play all channels to the default device.</param>
    public AgentRouting(IReadOnlyList<AudioOutput> outputs)
    {
        Outputs = outputs;
    }

    /// <summary>
    /// Gets the configured outputs; empty means play all channels to the default device.
    /// </summary>
    public IReadOnlyList<AudioOutput> Outputs { get; }
}
