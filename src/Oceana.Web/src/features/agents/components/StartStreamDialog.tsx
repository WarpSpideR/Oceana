import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import { ApiError } from '../../../shared/api/http'
import { useAgentMutations } from '../hooks/useAgentMutations'
import { STREAM_LIMITS, type StartStreamRequest } from '../types'

/** Props for {@link StartStreamDialog}. */
export interface StartStreamDialogProps {
  /** The target agent. */
  agent: { id: string; name: string }
  /** Whether the dialog is open. */
  open: boolean
  /** Called when the dialog should close. */
  onClose: () => void
}

interface FieldErrors {
  frequency?: string
  channels?: string
  durationSeconds?: string
}

function validate(
  frequency: string,
  channels: string,
  duration: string,
): { request?: StartStreamRequest; errors: FieldErrors } {
  const errors: FieldErrors = {}

  const freq = Number(frequency)
  if (!frequency || Number.isNaN(freq) || freq < STREAM_LIMITS.frequency.min || freq > STREAM_LIMITS.frequency.max) {
    errors.frequency = `Enter ${STREAM_LIMITS.frequency.min}–${STREAM_LIMITS.frequency.max} Hz.`
  }

  const chan = Number(channels)
  if (
    !channels ||
    !Number.isInteger(chan) ||
    chan < STREAM_LIMITS.channels.min ||
    chan > STREAM_LIMITS.channels.max
  ) {
    errors.channels = `Enter a whole number ${STREAM_LIMITS.channels.min}–${STREAM_LIMITS.channels.max}.`
  }

  let durationSeconds: number | null = null
  if (duration.trim() !== '') {
    const dur = Number(duration)
    if (
      Number.isNaN(dur) ||
      dur < STREAM_LIMITS.durationSeconds.min ||
      dur > STREAM_LIMITS.durationSeconds.max
    ) {
      errors.durationSeconds = `Enter ${STREAM_LIMITS.durationSeconds.min}–${STREAM_LIMITS.durationSeconds.max} s, or leave blank.`
    } else {
      durationSeconds = dur
    }
  }

  if (Object.keys(errors).length > 0) {
    return { errors }
  }
  return { request: { frequency: freq, channels: chan, durationSeconds }, errors }
}

/**
 * Dialog for starting a test-tone stream to an agent, with client-side
 * validation mirroring the server's bounds.
 * @param props The component props.
 * @returns The rendered dialog.
 */
export function StartStreamDialog({ agent, open, onClose }: StartStreamDialogProps) {
  const { startStreamMutation } = useAgentMutations(agent.id)
  const [frequency, setFrequency] = useState(String(STREAM_LIMITS.frequency.default))
  const [channels, setChannels] = useState(String(STREAM_LIMITS.channels.default))
  const [duration, setDuration] = useState('')

  // Reset form state and any prior error each time the dialog opens.
  useEffect(() => {
    if (open) {
      setFrequency(String(STREAM_LIMITS.frequency.default))
      setChannels(String(STREAM_LIMITS.channels.default))
      setDuration('')
      startStreamMutation.reset()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const { request, errors } = validate(frequency, channels, duration)

  const submit = () => {
    if (request) {
      startStreamMutation.mutate(request, { onSuccess: onClose })
    }
  }

  const error = startStreamMutation.error
  const errorMessage = error instanceof ApiError ? error.message : error ? String(error) : null

  return (
    <Dialog open={open} onClose={startStreamMutation.isPending ? undefined : onClose} fullWidth maxWidth="xs">
      <DialogTitle>Start stream to {agent.name}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Frequency (Hz)"
            type="number"
            value={frequency}
            onChange={(event) => setFrequency(event.target.value)}
            error={Boolean(errors.frequency)}
            helperText={errors.frequency ?? 'Base tone; each channel plays a multiple of this.'}
          />
          <TextField
            label="Channels"
            type="number"
            value={channels}
            onChange={(event) => setChannels(event.target.value)}
            error={Boolean(errors.channels)}
            helperText={errors.channels ?? '1–8 channels.'}
          />
          <TextField
            label="Duration (seconds)"
            type="number"
            value={duration}
            onChange={(event) => setDuration(event.target.value)}
            error={Boolean(errors.durationSeconds)}
            helperText={errors.durationSeconds ?? 'Leave blank to play until stopped.'}
          />
          {errorMessage && <Alert severity="error">{errorMessage}</Alert>}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={startStreamMutation.isPending}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={submit}
          disabled={Boolean(request) === false || startStreamMutation.isPending}
        >
          {startStreamMutation.isPending ? 'Starting…' : 'Start'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
