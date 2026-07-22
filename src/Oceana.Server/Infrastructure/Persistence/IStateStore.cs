namespace Oceana.Server.Infrastructure.Persistence;

/// <summary>
/// Persists and restores a single snapshot of state of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The state type persisted as a unit.</typeparam>
public interface IStateStore<T>
    where T : class
{
    /// <summary>
    /// Loads the persisted state.
    /// </summary>
    /// <returns>The persisted state, or null when nothing has been persisted (or it is unreadable).</returns>
    T? Load();

    /// <summary>
    /// Persists the given state, replacing any previously stored state.
    /// </summary>
    /// <param name="state">The state to persist.</param>
    void Save(T state);
}
