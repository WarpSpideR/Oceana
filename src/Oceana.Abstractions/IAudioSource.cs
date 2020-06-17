namespace Oceana
{
    /// <summary>
    /// A source audio stream provider.
    /// </summary>
    public interface IAudioSource
    {
        /// <summary>
        /// Gets the format of the audio source.
        /// </summary>
        AudioFormat Format { get; }

        /// <summary>
        /// Reads the requested number of audio samples from the audio
        /// source. Note that the result will be of length samples * channels.
        /// </summary>
        /// <param name="samples">The number of samples requested.</param>
        /// <returns>A array of audio samples.</returns>
        float[] Read(int samples);
    }
}
