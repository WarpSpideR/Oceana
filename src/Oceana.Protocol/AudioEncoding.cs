namespace Oceana.Protocol;

/// <summary>
/// Identifies how the audio samples that follow the handshake header are encoded on the wire.
/// </summary>
public enum AudioEncoding : byte
{
    /// <summary>
    /// Integer pulse-code-modulation samples, whose width is given by the header's bits-per-sample field.
    /// </summary>
    Pcm = 0,

    /// <summary>
    /// 32-bit IEEE floating-point samples.
    /// </summary>
    IeeeFloat = 1,
}
