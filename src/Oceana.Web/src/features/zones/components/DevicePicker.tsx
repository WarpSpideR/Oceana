import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Checkbox from '@mui/material/Checkbox'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import FormControlLabel from '@mui/material/FormControlLabel'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useAgents } from '../../agents/hooks/useAgents'
import { isDeviceSelected, resolveZoneDevices, toggleDevice } from '../zoneModel'
import type { ZoneDevice } from '../types'

/** Props for {@link DevicePicker}. */
export interface DevicePickerProps {
  /** The currently selected device assignments. */
  value: ZoneDevice[]
  /** Called with the new selection when a device is toggled. */
  onChange: (devices: ZoneDevice[]) => void
}

/**
 * Lets the operator choose which agents' devices belong to a zone. Devices are
 * grouped by agent; assignments whose agent/device is no longer present are shown
 * separately as unavailable so they can still be removed.
 * @param props The component props.
 * @returns The rendered device picker.
 */
export function DevicePicker({ value, onChange }: DevicePickerProps) {
  const { data: agents, isPending, isError } = useAgents()

  if (isPending) {
    return (
      <Stack sx={{ py: 3, alignItems: 'center' }}>
        <CircularProgress />
      </Stack>
    )
  }

  if (isError) {
    return <Alert severity="error">Could not load agents to choose devices from.</Alert>
  }

  const unavailable = resolveZoneDevices(value, agents).filter((d) => !d.available)

  return (
    <Stack spacing={2}>
      {agents.length === 0 && (
        <Alert severity="info" variant="outlined">
          No agents are connected. Start an agent to assign its devices to this zone.
        </Alert>
      )}

      {agents.map((agent) => (
        <Box key={agent.id}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 0.5 }}>
            <Typography variant="subtitle2">{agent.name}</Typography>
            <Chip
              size="small"
              label={agent.connected ? 'Connected' : 'Offline'}
              color={agent.connected ? 'success' : 'default'}
              variant={agent.connected ? 'filled' : 'outlined'}
            />
          </Stack>
          {agent.devices.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              No devices reported.
            </Typography>
          ) : (
            <Stack sx={{ pl: 1 }}>
              {agent.devices.map((device) => (
                <FormControlLabel
                  key={device.id}
                  control={
                    <Checkbox
                      checked={isDeviceSelected(value, agent.id, device.id)}
                      onChange={() => onChange(toggleDevice(value, agent.id, device.id))}
                    />
                  }
                  label={device.name}
                />
              ))}
            </Stack>
          )}
        </Box>
      ))}

      {unavailable.length > 0 && (
        <Box>
          <Divider sx={{ mb: 1 }} />
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Unavailable ({unavailable.length})
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            These assignments reference an agent or device that isn&apos;t currently present.
          </Typography>
          <Stack sx={{ pl: 1 }}>
            {unavailable.map((device) => (
              <FormControlLabel
                key={`${device.agentId}:${device.deviceId}`}
                control={
                  <Checkbox
                    checked
                    onChange={() => onChange(toggleDevice(value, device.agentId, device.deviceId))}
                  />
                }
                label={`${device.deviceName ?? device.deviceId} (agent ${device.agentName ?? device.agentId})`}
              />
            ))}
          </Stack>
        </Box>
      )}
    </Stack>
  )
}
