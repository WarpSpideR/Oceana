import { useEffect, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CircularProgress from '@mui/material/CircularProgress'
import Link from '@mui/material/Link'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CampaignIcon from '@mui/icons-material/Campaign'
import { ApiError, errorMessage } from '../../../shared/api/http'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { useAgents } from '../../agents/hooks/useAgents'
import { BroadcastDialog } from '../components/BroadcastDialog'
import { DevicePicker } from '../components/DevicePicker'
import { ZonePlaybackPanel } from '../components/ZonePlaybackPanel'
import { ZoneVolumeControl } from '../components/ZoneVolumeControl'
import { useZone } from '../hooks/useZone'
import { useZoneMutations } from '../hooks/useZoneMutations'
import type { ZoneDevice } from '../types'

/** A titled section card grouping the zone's controls. */
function SectionCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Card variant="outlined">
      <CardContent>
        <Typography variant="h6" gutterBottom>
          {title}
        </Typography>
        {children}
      </CardContent>
    </Card>
  )
}

/**
 * The zone detail page: rename a zone, assign/unassign devices, or delete it.
 * @returns The rendered zone detail page.
 */
export function ZoneDetailPage() {
  const { id = '' } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: zone, isPending, isError, error } = useZone(id)
  const { updateZoneMutation, removeZoneMutation } = useZoneMutations(id)
  const { data: agents } = useAgents()

  const [name, setName] = useState('')
  const [devices, setDevices] = useState<ZoneDevice[]>([])
  const [removeOpen, setRemoveOpen] = useState(false)
  const [broadcastOpen, setBroadcastOpen] = useState(false)

  // Seed the editor when the zone first loads or when switching zones; live
  // updates to the same zone don't clobber in-progress edits.
  useEffect(() => {
    if (zone) {
      setName(zone.name)
      setDevices(zone.devices)
    }
    updateZoneMutation.reset()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [zone?.id])

  const backLink = (
    <Link
      component={RouterLink}
      to="/zones"
      sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5 }}
    >
      <ArrowBackIcon fontSize="small" /> All zones
    </Link>
  )

  if (isPending) {
    return (
      <Stack sx={{ py: 6, alignItems: 'center' }}>
        <CircularProgress />
      </Stack>
    )
  }

  if (isError) {
    const notFound = error instanceof ApiError && error.status === 404
    return (
      <Stack spacing={2}>
        {backLink}
        <Alert severity={notFound ? 'warning' : 'error'}>
          {notFound
            ? 'This zone no longer exists.'
            : `Could not load the zone: ${error instanceof Error ? error.message : 'Unknown error'}`}
        </Alert>
      </Stack>
    )
  }

  const trimmedName = name.trim()
  const save = () => {
    if (trimmedName.length > 0) {
      updateZoneMutation.mutate({ name: trimmedName, devices })
    }
  }

  const updateErrorMessage = errorMessage(updateZoneMutation.error)

  const knownAgents = agents ?? []
  const canBroadcast = zone.devices.some((device) => {
    const agent = knownAgents.find((candidate) => candidate.id === device.agentId)
    return agent?.connected === true && agent.devices.some((reported) => reported.id === device.deviceId)
  })

  return (
    <Stack spacing={3}>
      {backLink}

      <Typography variant="h4" component="h1">
        {zone.name}
      </Typography>

      <SectionCard title="Zone">
        <Stack spacing={2}>
          <TextField
            label="Name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            error={trimmedName.length === 0}
            helperText={trimmedName.length === 0 ? 'A zone must have a name.' : ' '}
            sx={{ maxWidth: 360 }}
          />

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Devices ({devices.length})
            </Typography>
            <DevicePicker value={devices} onChange={setDevices} />
          </Box>

          {updateErrorMessage && <Alert severity="error">{updateErrorMessage}</Alert>}
          {updateZoneMutation.isSuccess && <Alert severity="success">Zone saved.</Alert>}

          <Stack direction="row" spacing={1}>
            <Box sx={{ flexGrow: 1 }} />
            <Button
              onClick={() => {
                setName(zone.name)
                setDevices(zone.devices)
              }}
            >
              Reset
            </Button>
            <Button
              variant="contained"
              onClick={save}
              disabled={trimmedName.length === 0 || updateZoneMutation.isPending}
            >
              {updateZoneMutation.isPending ? 'Saving…' : 'Save zone'}
            </Button>
          </Stack>
        </Stack>
      </SectionCard>

      <SectionCard title="Volume">
        <ZoneVolumeControl zone={zone} />
      </SectionCard>

      <SectionCard title="Play audio">
        <ZonePlaybackPanel zone={zone} canPlay={canBroadcast} />
      </SectionCard>

      <SectionCard title="Broadcast a message">
        <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
          <Typography variant="body2" color="text.secondary">
            Record a message from your microphone and play it on this zone&apos;s devices.
          </Typography>
          <Button
            variant="contained"
            startIcon={<CampaignIcon />}
            onClick={() => setBroadcastOpen(true)}
            disabled={!canBroadcast}
          >
            Record &amp; broadcast
          </Button>
          {!canBroadcast && (
            <Typography variant="body2" color="text.secondary">
              Add a device from a connected agent to broadcast to this zone.
            </Typography>
          )}
        </Stack>
      </SectionCard>

      <SectionCard title="Danger zone">
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
          <Button color="error" variant="outlined" onClick={() => setRemoveOpen(true)}>
            Delete zone
          </Button>
          <Typography variant="body2" color="text.secondary">
            Permanently removes this zone. Agents and devices are unaffected.
          </Typography>
        </Stack>
      </SectionCard>

      <ConfirmDialog
        open={removeOpen}
        title="Delete zone?"
        message={`Delete "${zone.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        destructive
        busy={removeZoneMutation.isPending}
        onCancel={() => setRemoveOpen(false)}
        onConfirm={() =>
          removeZoneMutation.mutate(undefined, {
            onSuccess: () => navigate('/zones'),
            onSettled: () => setRemoveOpen(false),
          })
        }
      />

      <BroadcastDialog zone={zone} open={broadcastOpen} onClose={() => setBroadcastOpen(false)} />
    </Stack>
  )
}
