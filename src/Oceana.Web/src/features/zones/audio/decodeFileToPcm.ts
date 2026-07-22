import { encodeWavPcm16 } from './wav'

/** The sample rate the file is resampled to before upload (matches the server/agent). */
const TARGET_SAMPLE_RATE = 48000

/** Zone playback is stereo. */
const TARGET_CHANNELS = 2

/**
 * Decodes an audio file (any format the browser supports — MP3/AAC/WAV/FLAC/OGG), resamples it to
 * 48 kHz stereo, and encodes it as a 16-bit PCM WAV blob for upload. Mono sources up-mix to stereo.
 * @param file The audio file chosen by the user.
 * @returns An `audio/wav` blob (48 kHz, stereo, 16-bit PCM).
 */
export async function decodeFileToWav(file: File): Promise<Blob> {
  const arrayBuffer = await file.arrayBuffer()

  const decodeContext = new AudioContext()
  let audioBuffer: AudioBuffer
  try {
    audioBuffer = await decodeContext.decodeAudioData(arrayBuffer)
  } finally {
    void decodeContext.close()
  }

  const frameCount = Math.max(1, Math.ceil(audioBuffer.duration * TARGET_SAMPLE_RATE))
  const offline = new OfflineAudioContext(TARGET_CHANNELS, frameCount, TARGET_SAMPLE_RATE)
  const source = offline.createBufferSource()
  source.buffer = audioBuffer
  source.connect(offline.destination)
  source.start()
  const rendered = await offline.startRendering()

  const left = rendered.getChannelData(0)
  const right = rendered.numberOfChannels > 1 ? rendered.getChannelData(1) : left
  return encodeWavPcm16([left, right], TARGET_SAMPLE_RATE)
}
