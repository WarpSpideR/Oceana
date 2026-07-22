import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import LinearProgress from '@mui/material/LinearProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import MicIcon from '@mui/icons-material/Mic'
import ReplayIcon from '@mui/icons-material/Replay'
import StopIcon from '@mui/icons-material/Stop'
import { errorMessage } from '../../../shared/api/http'
import { MAX_RECORDING_SECONDS, useAudioRecorder } from '../audio/useAudioRecorder'
import { useBroadcastToZone } from '../hooks/useZoneMutations'
import type { BroadcastResult, BroadcastSkipReason } from '../types'

/** Props for {@link BroadcastDialog}. */
export interface BroadcastDialogProps {
  /** The target zone. */
  zone: { id: string; name: string }
  /** Whether the dialog is open. */
  open: boolean
  /** Called when the dialog should close. */
  onClose: () => void
}

const SKIP_REASON_LABEL: Record<BroadcastSkipReason, string> = {
  Offline: 'offline',
  Busy: 'busy',
  NoActiveDevices: 'no active device',
}

function formatElapsed(seconds: number): string {
  const clamped = Math.min(seconds, MAX_RECORDING_SECONDS)
  const minutes = Math.floor(clamped / 60)
  const remainder = clamped % 60
  return `${minutes}:${remainder.toString().padStart(2, '0')}`
}

/**
 * Dialog for recording a message from the microphone, previewing it, and broadcasting it to a zone.
 * @param props The component props.
 * @returns The rendered dialog.
 */
export function BroadcastDialog({ zone, open, onClose }: BroadcastDialogProps) {
  const recorder = useAudioRecorder()
  const broadcastMutation = useBroadcastToZone(zone.id)
  const [converting, setConverting] = useState(false)
  const [convertError, setConvertError] = useState<string | null>(null)
  const [result, setResult] = useState<BroadcastResult | null>(null)

  const { reset } = recorder
  useEffect(() => {
    if (open) {
      reset()
      setConverting(false)
      setConvertError(null)
      setResult(null)
      broadcastMutation.reset()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const busy = converting || broadcastMutation.isPending

  const broadcast = async () => {
    setConvertError(null)
    setConverting(true)
    try {
      const wav = await recorder.getWavBlob()
      broadcastMutation.mutate(wav, { onSuccess: setResult })
    } catch {
      setConvertError('Could not process the recording. Please try again.')
    } finally {
      setConverting(false)
    }
  }

  const serverError = errorMessage(broadcastMutation.error)

  return (
    <Dialog open={open} onClose={busy ? undefined : onClose} fullWidth maxWidth="xs">
      <DialogTitle>Broadcast to {zone.name}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          {recorder.status !== 'error' && !result && (
            <Typography variant="body2" color="text.secondary">
              Record a short message (up to {MAX_RECORDING_SECONDS}s); it will play on every reachable
              device in this zone.
            </Typography>
          )}

          {recorder.status === 'error' && recorder.errorMessage && (
            <Alert severity="error">{recorder.errorMessage}</Alert>
          )}

          {recorder.status === 'recording' && (
            <Stack spacing={1}>
              <Typography variant="h5" component="p" sx={{ textAlign: 'center', fontVariantNumeric: 'tabular-nums' }}>
                {formatElapsed(recorder.elapsedSeconds)} / {formatElapsed(MAX_RECORDING_SECONDS)}
              </Typography>
              <LinearProgress
                variant="determinate"
                value={(Math.min(recorder.elapsedSeconds, MAX_RECORDING_SECONDS) / MAX_RECORDING_SECONDS) * 100}
              />
            </Stack>
          )}

          {recorder.status === 'recorded' && recorder.previewUrl && (
            <Stack spacing={1}>
              {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
              <audio controls src={recorder.previewUrl} style={{ width: '100%' }} />
            </Stack>
          )}

          {convertError && <Alert severity="error">{convertError}</Alert>}
          {serverError && <Alert severity="error">{serverError}</Alert>}

          {result && (
            <Alert severity={result.targeted.length > 0 ? 'success' : 'warning'}>
              {result.targeted.length > 0
                ? `Broadcasting to ${result.targeted.length} agent(s).`
                : 'No reachable devices — nothing was broadcast.'}
              {result.skipped.length > 0 && (
                <>
                  <br />
                  Skipped:{' '}
                  {result.skipped
                    .map((s) => `${s.agentName} (${SKIP_REASON_LABEL[s.reason]})`)
                    .join(', ')}
                  .
                </>
              )}
            </Alert>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        {recorder.status === 'idle' && !result && (
          <Button startIcon={<MicIcon />} variant="contained" onClick={recorder.start}>
            Record
          </Button>
        )}
        {recorder.status === 'recording' && (
          <Button startIcon={<StopIcon />} variant="contained" color="warning" onClick={recorder.stop}>
            Stop
          </Button>
        )}
        {recorder.status === 'recorded' && !result && (
          <>
            <Button startIcon={<ReplayIcon />} onClick={recorder.start} disabled={busy}>
              Re-record
            </Button>
            <Button variant="contained" onClick={broadcast} disabled={busy}>
              {busy ? 'Broadcasting…' : 'Broadcast'}
            </Button>
          </>
        )}
        <Button onClick={onClose} disabled={busy}>
          {result ? 'Close' : 'Cancel'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
