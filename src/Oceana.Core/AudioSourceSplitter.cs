using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana.Core
{
    /// <summary>
    /// Component that can split an <see cref="IAudioSource"/>
    /// into multiple <see cref="IAudioSource"/>s.
    /// </summary>
    public class AudioSourceSplitter : IMultipleAudioSource
    {
        private readonly IAudioSource Source;
        private readonly List<Range> Outputs;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioSourceSplitter"/> class.
        /// </summary>
        /// <param name="source"><see cref="IAudioSource"/> to be split.</param>
        public AudioSourceSplitter(IAudioSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Outputs = new List<Range>();
        }

        /// <inheritdoc/>
        public int Count => Outputs.Count;

        /// <summary>
        /// Creates an new audio output.
        /// </summary>
        /// <param name="channel">Channel to create the output from.</param>
        public void CreateAudioSource(short channel)
            => CreateAudioSource(new Range(channel, channel));

        /// <summary>
        /// Creates a new audio output.
        /// </summary>
        /// <param name="inputChannels">The <see cref="Range"/> of channels
        /// from the input <see cref="IAudioSource"/> to use.</param>
        public void CreateAudioSource(Range inputChannels)
        {
            if (inputChannels.Start.Value < 0
                || inputChannels.End.Value >= Source.Format.Channels)
            {
                throw new ArgumentOutOfRangeException(nameof(inputChannels));
            }

            Outputs.Add(inputChannels);
        }

        /// <inheritdoc/>
        public IAudioSource GetAudioSource(int index)
        {
            throw new NotImplementedException();
        }
    }
}
