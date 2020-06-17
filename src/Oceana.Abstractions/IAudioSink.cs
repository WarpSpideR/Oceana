namespace Oceana
{
    /// <summary>
    /// An output for audio data.
    /// </summary>
    public interface IAudioSink
    {
        /// <summary>
        /// Attempts to register an <see cref="IAudioSource"/> mapping it's
        /// channels starting from the given channel number.
        /// </summary>
        /// <param name="source"><see cref="IAudioSource"/> to sink.</param>
        /// <param name="channelStart">Number of the first channel to map to.</param>
        void RegisterSource(IAudioSource source, int channelStart);
    }
}
