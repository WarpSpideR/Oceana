import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Slider from '@mui/material/Slider'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import VolumeDownIcon from '@mui/icons-material/VolumeDown'
import VolumeUpIcon from '@mui/icons-material/VolumeUp'
import { errorMessage } from '../../../shared/api/http'
import { useSetZoneVolume } from '../hooks/useZoneMutations'

/** Props for {@link ZoneVolumeControl}. */
export interface ZoneVolumeControlProps {
  /** The zone whose volume is being controlled. */
  zone: { id: string; volume: number }
}

function toPercent(volume: number): number {
  return Math.round(volume * 100)
}

/**
 * A slider for a zone's playback volume (0–100%). Dragging updates the label live; releasing
 * persists the value (which applies to audio already playing within ~20 ms).
 * @param props The component props.
 * @returns The rendered control.
 */
export function ZoneVolumeControl({ zone }: ZoneVolumeControlProps) {
  const setVolume = useSetZoneVolume(zone.id)
  const [percent, setPercent] = useState(() => toPercent(zone.volume))

  // Re-sync from the server value (e.g. another client changed it) when not mid-interaction.
  useEffect(() => {
    setPercent(toPercent(zone.volume))
  }, [zone.volume])

  const error = errorMessage(setVolume.error)

  return (
    <Stack spacing={1} sx={{ maxWidth: 420 }}>
      <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
        <VolumeDownIcon color="action" />
        <Slider
          aria-label="Zone volume"
          value={percent}
          min={0}
          max={100}
          onChange={(_, value) => setPercent(value as number)}
          onChangeCommitted={(_, value) => setVolume.mutate((value as number) / 100)}
        />
        <VolumeUpIcon color="action" />
        <Typography variant="body2" sx={{ width: 44, textAlign: 'right', fontVariantNumeric: 'tabular-nums' }}>
          {percent}%
        </Typography>
      </Stack>
      {error && <Alert severity="error">{error}</Alert>}
    </Stack>
  )
}
