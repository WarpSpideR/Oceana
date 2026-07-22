import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import StopIcon from '@mui/icons-material/Stop'
import UploadFileIcon from '@mui/icons-material/UploadFile'
import { errorMessage } from '../../../shared/api/http'
import { decodeFileToWav } from '../audio/decodeFileToPcm'
import { useZonePlayback } from '../hooks/useZonePlayback'
import { useZonePlaybackMutations } from '../hooks/useZoneMutations'
import type { BroadcastSkipReason } from '../types'

/** Props for {@link ZonePlaybackPanel}. */
export interface ZonePlaybackPanelProps {
  /** The zone. */
  zone: { id: string; name: string }
  /** Whether at least one of the zone's devices is on a connected agent. */
  canPlay: boolean
}

const SKIP_REASON_LABEL: Record<BroadcastSkipReason, string> = {
  Offline: 'offline',
  Busy: 'busy',
  NoActiveDevices: 'no active device',
}

function formatElapsed(totalSeconds: number): string {
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}

/**
 * Play-audio controls for a zone: pick a file and play it to the zone's devices, or (when playing)
 * show now-playing and stop. Decoding/resampling happens in the browser before upload.
 * @param props The component props.
 * @returns The rendered panel.
 */
export function ZonePlaybackPanel({ zone, canPlay }: ZonePlaybackPanelProps) {
  const { data: playback } = useZonePlayback(zone.id)
  const { playMutation, stopMutation } = useZonePlaybackMutations(zone.id)
  const [file, setFile] = useState<File | null>(null)
  const [converting, setConverting] = useState(false)
  const [convertError, setConvertError] = useState<string | null>(null)
  const [now, setNow] = useState(() => Date.now())

  const playing = playback?.playing === true
  useEffect(() => {
    if (!playing) {
      return
    }
    const timer = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(timer)
  }, [playing])

  const busy = converting || playMutation.isPending

  const play = async () => {
    if (!file) {
      return
    }
    setConvertError(null)
    setConverting(true)
    try {
      const wav = await decodeFileToWav(file)
      playMutation.mutate({ wav, name: file.name })
    } catch {
      setConvertError('Could not read that audio file. Try a different one.')
    } finally {
      setConverting(false)
    }
  }

  if (playing && playback) {
    const elapsed = playback.startedAtUtc
      ? Math.max(0, Math.floor((now - Date.parse(playback.startedAtUtc)) / 1000))
      : 0
    return (
      <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
        <Box>
          <Typography variant="subtitle1">{playback.sourceName ?? 'Audio'}</Typography>
          <Typography variant="body2" color="text.secondary">
            Playing to {playback.targeted.length} agent(s) · {formatElapsed(elapsed)}
          </Typography>
        </Box>
        <Button
          variant="outlined"
          color="warning"
          startIcon={<StopIcon />}
          onClick={() => stopMutation.mutate()}
          disabled={stopMutation.isPending}
        >
          Stop
        </Button>
      </Stack>
    )
  }

  const serverError = errorMessage(playMutation.error)
  const skipped = playMutation.data?.skipped ?? []

  return (
    <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
      <Typography variant="body2" color="text.secondary">
        Play an audio file (any common format) on this zone&apos;s devices.
      </Typography>

      <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
        <Button component="label" variant="outlined" startIcon={<UploadFileIcon />} disabled={busy}>
          Choose file
          <input
            type="file"
            accept="audio/*"
            hidden
            onChange={(event) => {
              setFile(event.target.files?.[0] ?? null)
              playMutation.reset()
            }}
          />
        </Button>
        <Typography variant="body2" color="text.secondary">
          {file ? file.name : 'No file chosen'}
        </Typography>
      </Stack>

      <Button
        variant="contained"
        startIcon={<PlayArrowIcon />}
        onClick={play}
        disabled={!canPlay || !file || busy}
      >
        {busy ? 'Starting…' : 'Play'}
      </Button>

      {!canPlay && (
        <Typography variant="body2" color="text.secondary">
          Add a device from a connected agent to play to this zone.
        </Typography>
      )}

      {convertError && <Alert severity="error">{convertError}</Alert>}
      {serverError && <Alert severity="error">{serverError}</Alert>}
      {playMutation.data && !playMutation.data.playing && (
        <Alert severity="warning">No reachable devices — nothing is playing.</Alert>
      )}
      {skipped.length > 0 && (
        <Alert severity="info">
          Skipped: {skipped.map((s) => `${s.agentName} (${SKIP_REASON_LABEL[s.reason]})`).join(', ')}.
        </Alert>
      )}
    </Stack>
  )
}
