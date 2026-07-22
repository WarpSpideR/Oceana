/**
 * Encodes mono PCM samples as a 16-bit WAV (RIFF/WAVE) blob — the format the Oceana server
 * accepts for a broadcast. Keeping the encoder pure makes it unit-testable.
 */

const BITS_PER_SAMPLE = 16
const NUM_CHANNELS = 1

function writeAscii(view: DataView, offset: number, text: string): void {
  for (let i = 0; i < text.length; i++) {
    view.setUint8(offset + i, text.charCodeAt(i))
  }
}

/**
 * Encodes mono float samples (range -1..1) into a 16-bit PCM WAV blob.
 * @param samples The mono sample data.
 * @param sampleRate The sample rate in hertz.
 * @returns A `audio/wav` blob.
 */
export function encodeWavPcm16(samples: Float32Array, sampleRate: number): Blob {
  const blockAlign = (NUM_CHANNELS * BITS_PER_SAMPLE) / 8
  const byteRate = sampleRate * blockAlign
  const dataLength = samples.length * 2
  const buffer = new ArrayBuffer(44 + dataLength)
  const view = new DataView(buffer)

  writeAscii(view, 0, 'RIFF')
  view.setUint32(4, 36 + dataLength, true)
  writeAscii(view, 8, 'WAVE')
  writeAscii(view, 12, 'fmt ')
  view.setUint32(16, 16, true) // fmt chunk size
  view.setUint16(20, 1, true) // PCM
  view.setUint16(22, NUM_CHANNELS, true)
  view.setUint32(24, sampleRate, true)
  view.setUint32(28, byteRate, true)
  view.setUint16(32, blockAlign, true)
  view.setUint16(34, BITS_PER_SAMPLE, true)
  writeAscii(view, 36, 'data')
  view.setUint32(40, dataLength, true)

  let offset = 44
  for (let i = 0; i < samples.length; i++) {
    const clamped = Math.max(-1, Math.min(1, samples[i]))
    // Scale to signed 16-bit range.
    view.setInt16(offset, clamped < 0 ? clamped * 0x8000 : clamped * 0x7fff, true)
    offset += 2
  }

  return new Blob([buffer], { type: 'audio/wav' })
}
