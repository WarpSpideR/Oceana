using System.Collections.Generic;

namespace Oceana
{
    /// <summary>
    /// Holds mapping information about the channels for
    /// a <see cref="SplitAudioSource"/>.
    /// </summary>
    internal class AudioSplitMap
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AudioSplitMap"/> class.
        /// </summary>
        /// <param name="source">Split audio source being mappend.</param>
        /// <param name="channels">Ordered list of channels that will be
        /// output to the <see cref="SplitAudioSource"/>.</param>
        public AudioSplitMap(SplitAudioSource source, IList<int> channels)
        {
            Source = source;
            Channels = channels;
        }

        /// <summary>
        /// Gets the <see cref="SplitAudioSource"/> being mapped.
        /// </summary>
        public SplitAudioSource Source { get; }

        /// <summary>
        /// Gets an ordered list of channels that will be output to the
        /// <see cref="SplitAudioSource"/>.
        /// </summary>
        public IList<int> Channels { get; }
    }
}
