namespace Oceana
{
    /// <summary>
    /// An audio source that is capable of providing more
    /// than one source of audio.
    /// </summary>
    public interface IMultipleAudioSource
    {
        /// <summary>
        /// Gets the number of <see cref="IAudioSource"/>s available.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the <see cref="IAudioSource"/> at the given index.
        /// </summary>
        /// <param name="index">Index of the <see cref="IAudioSource"/>
        /// to retrieve.</param>
        /// <returns>The corresponding <see cref="IAudioSource"/>.</returns>
        IAudioSource GetAudioSource(int index);
    }
}
