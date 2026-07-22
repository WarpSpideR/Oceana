namespace Oceana.Server.Infrastructure.Persistence;

/// <summary>
/// A test double for <see cref="IStateStore{T}"/> that keeps the last saved state in memory.
/// </summary>
/// <typeparam name="T">The state type.</typeparam>
public sealed class InMemoryStateStore<T> : IStateStore<T>
    where T : class
{
    /// <summary>
    /// Initialises a new instance of the <see cref="InMemoryStateStore{T}"/> class.
    /// </summary>
    /// <param name="initial">The state to return from the first <see cref="Load"/>.</param>
    public InMemoryStateStore(T? initial = null)
    {
        Current = initial;
    }

    /// <summary>
    /// Gets the most recently saved (or initial) state.
    /// </summary>
    public T? Current { get; private set; }

    /// <summary>
    /// Gets the number of times <see cref="Save"/> has been called.
    /// </summary>
    public int SaveCount { get; private set; }

    /// <inheritdoc/>
    public T? Load() => Current;

    /// <inheritdoc/>
    public void Save(T state)
    {
        Current = state;
        SaveCount++;
    }
}
