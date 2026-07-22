/**
 * Encodes PCM samples as a 16-bit WAV (RIFF/WAVE) blob — the format the Oceana server accepts for
 * broadcasts (mono) and zone playback (stereo). Keeping the encoder pure makes it unit-testable.
 */

const BITS_PER_SAMPLE = 16

function writeAscii(view: DataView, offset: number, text: string): void {
  for (let i = 0; i < text.length; i++) {
    view.setUint8(offset + i, text.charCodeAt(i))
  }
}

function toInt16(sample: number): number {
  const clamped = Math.max(-1, Math.min(1, sample))
  return clamped < 0 ? clamped * 0x8000 : clamped * 0x7fff
}

/**
 * Encodes one or more channels of float samples (range -1..1) into a 16-bit PCM WAV blob. All
 * channels must have the same length; samples are interleaved per frame.
 * @param channels The per-channel sample data (e.g. `[mono]` or `[left, right]`).
 * @param sampleRate The sample rate in hertz.
 * @returns An `audio/wav` blob.
 */
export function encodeWavPcm16(channels: Float32Array[], sampleRate: number): Blob {
  const channelCount = channels.length
  const frameCount = channelCount > 0 ? channels[0].length : 0
  const blockAlign = (channelCount * BITS_PER_SAMPLE) / 8
  const byteRate = sampleRate * blockAlign
  const dataLength = frameCount * blockAlign
  const buffer = new ArrayBuffer(44 + dataLength)
  const view = new DataView(buffer)

  writeAscii(view, 0, 'RIFF')
  view.setUint32(4, 36 + dataLength, true)
  writeAscii(view, 8, 'WAVE')
  writeAscii(view, 12, 'fmt ')
  view.setUint32(16, 16, true) // fmt chunk size
  view.setUint16(20, 1, true) // PCM
  view.setUint16(22, channelCount, true)
  view.setUint32(24, sampleRate, true)
  view.setUint32(28, byteRate, true)
  view.setUint16(32, blockAlign, true)
  view.setUint16(34, BITS_PER_SAMPLE, true)
  writeAscii(view, 36, 'data')
  view.setUint32(40, dataLength, true)

  let offset = 44
  for (let frame = 0; frame < frameCount; frame++) {
    for (let channel = 0; channel < channelCount; channel++) {
      view.setInt16(offset, toInt16(channels[channel][frame]), true)
      offset += 2
    }
  }

  return new Blob([buffer], { type: 'audio/wav' })
}
