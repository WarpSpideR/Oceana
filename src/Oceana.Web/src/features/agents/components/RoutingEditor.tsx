import { useEffect, useMemo, useState } from 'react'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import AddIcon from '@mui/icons-material/Add'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined'
import { ApiError } from '../../../shared/api/http'
import { useAgentMutations } from '../hooks/useAgentMutations'
import { buildRouting, outputsToRows, type RoutingRow } from '../routingModel'
import type { AgentInfo } from '../types'

/** Sentinel Select value representing the agent's default device (routing `null`). */
const DEFAULT_DEVICE = '__default__'

/** Props for {@link RoutingEditor}. */
export interface RoutingEditorProps {
  /** The agent whose routing is being edited. */
  agent: AgentInfo
}

/**
 * Editor for an agent's channel-to-device routing. Each row maps an ordered
 * list of source channels to one device (or the default). Saving stores the
 * routing on the server, which applies it on the agent's next stream.
 * @param props The component props.
 * @returns The rendered routing editor.
 */
export function RoutingEditor({ agent }: RoutingEditorProps) {
  const { setRoutingMutation } = useAgentMutations(agent.id)
  const [rows, setRows] = useState<RoutingRow[]>(() => outputsToRows(agent.routing.outputs))

  // Reset the editor when switching to a different agent.
  useEffect(() => {
    setRows(outputsToRows(agent.routing.outputs))
    setRoutingMutation.reset()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [agent.id])

  const built = useMemo(() => buildRouting(rows), [rows])

  const updateRow = (index: number, patch: Partial<RoutingRow>) => {
    setRows((current) => current.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }
  const addRow = () => setRows((current) => [...current, { device: null, channelsText: '' }])
  const removeRow = (index: number) =>
    setRows((current) => current.filter((_, i) => i !== index))

  const save = () => {
    if (built.valid) {
      setRoutingMutation.mutate({ outputs: built.outputs })
    }
  }

  const error = setRoutingMutation.error
  const errorMessage = error instanceof ApiError ? error.message : error ? String(error) : null

  return (
    <Stack spacing={2}>
      <Typography variant="body2" color="text.secondary">
        Map source channels to output devices. Leave with no outputs to play all channels to
        the agent&apos;s default device. Changes apply on the agent&apos;s next stream.
      </Typography>

      {rows.length === 0 && (
        <Alert severity="info" variant="outlined">
          No outputs configured — all channels will play to the default device.
        </Alert>
      )}

      {rows.map((row, index) => {
        const selectValue = row.device ?? DEFAULT_DEVICE
        const isKnownDevice =
          row.device === null || agent.devices.some((d) => d.id === row.device)
        return (
          <Stack
            key={index}
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1.5}
            sx={{ alignItems: { sm: 'flex-start' } }}
          >
            <TextField
              select
              label="Device"
              value={selectValue}
              onChange={(event) =>
                updateRow(index, {
                  device: event.target.value === DEFAULT_DEVICE ? null : event.target.value,
                })
              }
              sx={{ minWidth: 240 }}
            >
              <MenuItem value={DEFAULT_DEVICE}>Default device</MenuItem>
              {agent.devices.map((device) => (
                <MenuItem key={device.id} value={device.id}>
                  {device.name}
                </MenuItem>
              ))}
              {!isKnownDevice && row.device !== null && (
                <MenuItem value={row.device}>{row.device} (not reported)</MenuItem>
              )}
            </TextField>

            <TextField
              label="Channels"
              value={row.channelsText}
              onChange={(event) => updateRow(index, { channelsText: event.target.value })}
              error={Boolean(built.errors[index])}
              helperText={built.errors[index] ?? 'e.g. 0, 1'}
              sx={{ flexGrow: 1 }}
            />

            <Tooltip title="Remove output">
              <IconButton
                aria-label="Remove output"
                onClick={() => removeRow(index)}
                sx={{ mt: { sm: 1 } }}
              >
                <DeleteOutlineIcon />
              </IconButton>
            </Tooltip>
          </Stack>
        )
      })}

      {errorMessage && <Alert severity="error">{errorMessage}</Alert>}
      {setRoutingMutation.isSuccess && (
        <Alert severity="success">Routing saved. It will apply on the next stream.</Alert>
      )}

      <Box>
        <Stack direction="row" spacing={1}>
          <Button startIcon={<AddIcon />} onClick={addRow}>
            Add output
          </Button>
          <Box sx={{ flexGrow: 1 }} />
          <Button onClick={() => setRows(outputsToRows(agent.routing.outputs))}>Reset</Button>
          <Button
            variant="contained"
            onClick={save}
            disabled={!built.valid || setRoutingMutation.isPending}
          >
            {setRoutingMutation.isPending ? 'Saving…' : 'Save routing'}
          </Button>
        </Stack>
      </Box>
    </Stack>
  )
}
