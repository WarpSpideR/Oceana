import { useCallback, useEffect, useRef, useState } from 'react'
import { encodeWavPcm16 } from './wav'

/** The maximum length of a recorded message, in seconds. */
export const MAX_RECORDING_SECONDS = 60

/** The sample rate the message is resampled to before upload (matches the server). */
const TARGET_SAMPLE_RATE = 48000

/** The lifecycle state of the recorder. */
export type RecorderStatus = 'idle' | 'recording' | 'recorded' | 'error'

/** The microphone recorder surface returned by {@link useAudioRecorder}. */
export interface AudioRecorder {
  /** The current recorder state. */
  status: RecorderStatus
  /** A human-readable error (e.g. permission denied), or null. */
  errorMessage: string | null
  /** Seconds elapsed in the current/last recording. */
  elapsedSeconds: number
  /** Object URL of the recorded audio for local `<audio>` preview, or null. */
  previewUrl: string | null
  /** Begins recording (requesting microphone access). */
  start: () => void
  /** Stops the in-progress recording. */
  stop: () => void
  /** Discards the recording and returns to idle. */
  reset: () => void
  /** Converts the recording to a mono, 48 kHz, 16-bit PCM WAV blob for upload. */
  getWavBlob: () => Promise<Blob>
}

/**
 * Records a short message from the user's microphone using `MediaRecorder`, exposes it for local
 * preview, and converts it (down-mixed to mono and resampled to 48 kHz) to a WAV blob on demand.
 * Isolates all browser media APIs so the surrounding UI stays testable.
 * @returns The recorder controls and state.
 */
export function useAudioRecorder(): AudioRecorder {
  const [status, setStatus] = useState<RecorderStatus>('idle')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [elapsedSeconds, setElapsedSeconds] = useState(0)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)

  const recorderRef = useRef<MediaRecorder | null>(null)
  const streamRef = useRef<MediaStream | null>(null)
  const chunksRef = useRef<Blob[]>([])
  const blobRef = useRef<Blob | null>(null)
  const timerRef = useRef<number | null>(null)
  const previewUrlRef = useRef<string | null>(null)

  const clearTimer = useCallback(() => {
    if (timerRef.current !== null) {
      window.clearInterval(timerRef.current)
      timerRef.current = null
    }
  }, [])

  const stopStream = useCallback(() => {
    streamRef.current?.getTracks().forEach((track) => track.stop())
    streamRef.current = null
  }, [])

  const revokePreview = useCallback(() => {
    if (previewUrlRef.current) {
      URL.revokeObjectURL(previewUrlRef.current)
      previewUrlRef.current = null
    }
  }, [])

  const stop = useCallback(() => {
    clearTimer()
    const recorder = recorderRef.current
    if (recorder && recorder.state !== 'inactive') {
      recorder.stop()
    }
  }, [clearTimer])

  const reset = useCallback(() => {
    stop()
    stopStream()
    revokePreview()
    chunksRef.current = []
    blobRef.current = null
    recorderRef.current = null
    setElapsedSeconds(0)
    setPreviewUrl(null)
    setErrorMessage(null)
    setStatus('idle')
  }, [stop, stopStream, revokePreview])

  const start = useCallback(async () => {
    reset()
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      streamRef.current = stream

      const recorder = new MediaRecorder(stream)
      recorderRef.current = recorder
      chunksRef.current = []

      recorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          chunksRef.current.push(event.data)
        }
      }
      recorder.onstop = () => {
        const blob = new Blob(chunksRef.current, { type: recorder.mimeType || 'audio/webm' })
        blobRef.current = blob
        revokePreview()
        const url = URL.createObjectURL(blob)
        previewUrlRef.current = url
        setPreviewUrl(url)
        setStatus('recorded')
        stopStream()
      }

      recorder.start()
      setStatus('recording')

      const startedAt = performance.now()
      timerRef.current = window.setInterval(() => {
        const seconds = Math.floor((performance.now() - startedAt) / 1000)
        setElapsedSeconds(seconds)
        if (seconds >= MAX_RECORDING_SECONDS) {
          stop()
        }
      }, 250)
    } catch {
      setStatus('error')
      setErrorMessage('Could not access the microphone. Check the browser permission and try again.')
    }
  }, [reset, revokePreview, stopStream, stop])

  const getWavBlob = useCallback(async () => {
    const blob = blobRef.current
    if (!blob) {
      throw new Error('There is no recording to broadcast.')
    }

    const arrayBuffer = await blob.arrayBuffer()
    const decodeContext = new AudioContext()
    let audioBuffer: AudioBuffer
    try {
      audioBuffer = await decodeContext.decodeAudioData(arrayBuffer)
    } finally {
      void decodeContext.close()
    }

    // Render through an offline context to down-mix to mono and resample to 48 kHz.
    const frameCount = Math.max(1, Math.ceil(audioBuffer.duration * TARGET_SAMPLE_RATE))
    const offline = new OfflineAudioContext(1, frameCount, TARGET_SAMPLE_RATE)
    const source = offline.createBufferSource()
    source.buffer = audioBuffer
    source.connect(offline.destination)
    source.start()
    const rendered = await offline.startRendering()

    return encodeWavPcm16(rendered.getChannelData(0), TARGET_SAMPLE_RATE)
  }, [])

  useEffect(() => {
    return () => {
      clearTimer()
      stopStream()
      revokePreview()
      const recorder = recorderRef.current
      if (recorder && recorder.state !== 'inactive') {
        recorder.stop()
      }
    }
  }, [clearTimer, stopStream, revokePreview])

  return { status, errorMessage, elapsedSeconds, previewUrl, start, stop, reset, getWavBlob }
}
