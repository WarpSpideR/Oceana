using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Represents an Oceana Agent.
    /// </summary>
    public interface IOceanaAgent
    {
        /// <summary>
        /// Gets information about available audio sources.
        /// </summary>
        /// <returns>Collection of audio source information.</returns>
        IEnumerable<AudioSourceInfo> GetAudioSources();

        /// <summary>
        /// Gets information about available audio sinks.
        /// </summary>
        /// <returns>Collection of audio sink information.</returns>
        IEnumerable<AudioSinkInfo> GetAudioSinks();
    }
}
