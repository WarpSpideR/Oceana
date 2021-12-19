using NAudio.Wave;

namespace Oceana
{
    /// <summary>
    /// Extension methods to ease working with different <see cref="WaveFormat"/>s.
    /// </summary>
    internal static class NAudioWaveFormatExtensions
    {
        /// <summary>
        /// Converts a <see cref="WaveFormat"/> to its corresponding
        /// <see cref="AudioFormat"/>.
        /// </summary>
        /// <param name="wave"><see cref="WaveFormat"/> to convert.</param>
        /// <returns>The equivelant <see cref="AudioFormat"/>.</returns>
        public static AudioFormat ToAudioFormat(this WaveFormat wave)
            => new AudioFormat((short)wave.Channels, wave.SampleRate);
    }
}
