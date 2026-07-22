import { describe, expect, it } from 'vitest'
import { encodeWavPcm16 } from './wav'

function ascii(view: DataView, offset: number, length: number): string {
  let text = ''
  for (let i = 0; i < length; i++) {
    text += String.fromCharCode(view.getUint8(offset + i))
  }
  return text
}

describe('encodeWavPcm16', () => {
  it('produces a mono 16-bit PCM WAV with a correct header', async () => {
    const samples = new Float32Array([0, 1, -1, 0.5])
    const blob = encodeWavPcm16(samples, 48000)
    const view = new DataView(await blob.arrayBuffer())

    expect(blob.type).toBe('audio/wav')
    expect(ascii(view, 0, 4)).toBe('RIFF')
    expect(ascii(view, 8, 4)).toBe('WAVE')
    expect(ascii(view, 12, 4)).toBe('fmt ')
    expect(view.getUint16(20, true)).toBe(1) // PCM
    expect(view.getUint16(22, true)).toBe(1) // mono
    expect(view.getUint32(24, true)).toBe(48000) // sample rate
    expect(view.getUint16(34, true)).toBe(16) // bits per sample
    expect(ascii(view, 36, 4)).toBe('data')
    expect(view.getUint32(40, true)).toBe(samples.length * 2)
  })

  it('has the expected total byte length (44-byte header + PCM)', async () => {
    const samples = new Float32Array(10)
    const blob = encodeWavPcm16(samples, 48000)
    expect(blob.size).toBe(44 + 10 * 2)
  })

  it('scales and clamps samples to signed 16-bit', async () => {
    const samples = new Float32Array([0, 1, -1, 0.5, 2, -2])
    const view = new DataView(await encodeWavPcm16(samples, 48000).arrayBuffer())

    expect(view.getInt16(44, true)).toBe(0)
    expect(view.getInt16(46, true)).toBe(32767) // 1.0 -> max
    expect(view.getInt16(48, true)).toBe(-32768) // -1.0 -> min
    expect(view.getInt16(50, true)).toBe(16383) // 0.5
    expect(view.getInt16(52, true)).toBe(32767) // clamped from 2.0
    expect(view.getInt16(54, true)).toBe(-32768) // clamped from -2.0
  })
})
