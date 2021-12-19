using System;
using System.Collections.Generic;

namespace Oceana
{
    /// <summary>
    /// Component that can split an <see cref="IAudioSource"/>
    /// into multiple <see cref="IAudioSource"/>s.
    /// </summary>
    public class AudioSourceSplitter : IMultipleAudioSource
    {
        private readonly IAudioSource Source;
        private readonly List<AudioSplitMap> Outputs;

        /// <summary>
        /// Initialises a new instance of the <see cref="AudioSourceSplitter"/> class.
        /// </summary>
        /// <param name="source"><see cref="IAudioSource"/> to be split.</param>
        public AudioSourceSplitter(IAudioSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Outputs = new List<AudioSplitMap>();
            Format = Source.Format;
        }

        /// <inheritdoc/>
        public int Count => Outputs.Count;

        /// <inheritdoc/>
        public AudioFormat Format { get; }

        /// <summary>
        /// Creates an new audio output.
        /// </summary>
        /// <param name="channel">Channel to create the output from.</param>
        /// <returns>The newly created <see cref="IAudioSource"/>.</returns>
        public IAudioSource CreateAudioSource(short channel)
            => CreateAudioSource(new List<int> { channel });

        /// <summary>
        /// Creates a new audio output.
        /// </summary>
        /// <param name="inputChannels">An ordered list of input channels
        /// that are to be mapped to the new audio source.</param>
        /// <returns>The newly created <see cref="IAudioSource"/>.</returns>
        public IAudioSource CreateAudioSource(IList<int> inputChannels)
        {
            _ = inputChannels ?? throw new ArgumentNullException(nameof(inputChannels));

            foreach (var channel in inputChannels)
            {
                if (channel < 0
                    || channel >= Source.Format.Channels)
                {
                    throw new ArgumentOutOfRangeException(nameof(inputChannels));
                }
            }

            var source = new SplitAudioSource(this, inputChannels.Count);

            Outputs.Add(new AudioSplitMap(source, inputChannels));

            return source;
        }

        /// <inheritdoc/>
        public IAudioSource GetAudioSource(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used by child sources to request more samples.
        /// </summary>
        /// <param name="samples">The number of samples being requested.</param>
        internal void RequestRead(int samples)
        {
            var input = Source.Read(samples);

            for (var sample = 0; sample < samples; sample++)
            {
                foreach (var output in Outputs)
                {
                    foreach (var channel in output.Channels)
                    {
                        var index = (sample * Format.Channels) + channel;
                        output.Source.Write(input[index]);
                    }
                }
            }
        }
    }
}
