using Oceana.Contracts;

namespace Oceana.Agent.Windows.Playback;

/// <summary>
/// A thread-safe holder for the agent's current channel→device routing. Initialised empty (all
/// channels to the default device) and replaced when the server pushes new routing.
/// </summary>
public sealed class RoutingStore
{
    private readonly object gate = new object();
    private IReadOnlyList<AudioOutput> current = Array.Empty<AudioOutput>();

    /// <summary>
    /// Gets the current routing snapshot.
    /// </summary>
    public IReadOnlyList<AudioOutput> Current
    {
        get
        {
            lock (gate)
            {
                return current;
            }
        }
    }

    /// <summary>
    /// Replaces the current routing.
    /// </summary>
    /// <param name="outputs">The new routing.</param>
    public void Set(IReadOnlyList<AudioOutput> outputs)
    {
        lock (gate)
        {
            current = outputs;
        }
    }
}
