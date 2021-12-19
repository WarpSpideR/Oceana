using System;
using System.Collections.Generic;
using System.Text;

namespace Oceana
{
    /// <summary>
    /// Base class for an audio component.
    /// </summary>
    public abstract class AudioComponent
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AudioComponent"/> class.
        /// </summary>
        public AudioComponent()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the unique identifier for this <see cref="AudioComponent"/>.
        /// </summary>
        public Guid Id { get; }
    }
}
