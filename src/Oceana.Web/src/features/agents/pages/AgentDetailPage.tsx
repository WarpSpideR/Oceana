import { useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import Link from '@mui/material/Link'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import StopIcon from '@mui/icons-material/Stop'
import { ApiError } from '../../../shared/api/http'
import { ConfirmDialog } from '../../../shared/components/ConfirmDialog'
import { AgentStatusChip } from '../components/AgentStatusChip'
import { RoutingEditor } from '../components/RoutingEditor'
import { StartStreamDialog } from '../components/StartStreamDialog'
import { useAgent } from '../hooks/useAgent'
import { useAgentMutations } from '../hooks/useAgentMutations'

/** A titled section card used to group the agent's controls. */
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
 * The agent detail page: inspect an agent and drive it — edit routing, start or
 * stop a test-tone stream, or remove it from the registry.
 * @returns The rendered agent detail page.
 */
export function AgentDetailPage() {
  const { id = '' } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { data: agent, isPending, isError, error } = useAgent(id)
  const { stopStreamMutation, removeAgentMutation } = useAgentMutations(id)

  const [startOpen, setStartOpen] = useState(false)
  const [stopOpen, setStopOpen] = useState(false)
  const [removeOpen, setRemoveOpen] = useState(false)

  const backLink = (
    <Link component={RouterLink} to="/agents" sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5 }}>
      <ArrowBackIcon fontSize="small" /> All agents
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
            ? 'This agent is no longer registered.'
            : `Could not load the agent: ${error instanceof Error ? error.message : 'Unknown error'}`}
        </Alert>
      </Stack>
    )
  }

  const isStreaming = agent.status === 'Streaming' || agent.status === 'Connecting'

  return (
    <Stack spacing={3}>
      {backLink}

      <Box>
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
          <Typography variant="h4" component="h1">
            {agent.name}
          </Typography>
          <AgentStatusChip status={agent.status} lastError={agent.lastError} />
          <Chip
            size="small"
            label={agent.connected ? 'Connected' : 'Offline'}
            color={agent.connected ? 'success' : 'default'}
            variant={agent.connected ? 'filled' : 'outlined'}
          />
        </Stack>
      </Box>

      {agent.lastError && agent.status === 'Faulted' && (
        <Alert severity="error">Last error: {agent.lastError}</Alert>
      )}

      <SectionCard title="Details">
        <List dense disablePadding>
          <ListItem disableGutters>
            <ListItemText primary="Identifier" secondary={agent.id} />
          </ListItem>
          <ListItem disableGutters>
            <ListItemText primary="Audio address" secondary={`${agent.host}:${agent.port}`} />
          </ListItem>
        </List>
        <Divider sx={{ my: 1 }} />
        <Typography variant="subtitle2" gutterBottom>
          Devices ({agent.devices.length})
        </Typography>
        {agent.devices.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No devices reported.
          </Typography>
        ) : (
          <List dense disablePadding>
            {agent.devices.map((device) => (
              <ListItem key={device.id} disableGutters>
                <ListItemText primary={device.name} secondary={device.id} />
              </ListItem>
            ))}
          </List>
        )}
      </SectionCard>

      <SectionCard title="Streaming">
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <Button
            variant="contained"
            startIcon={<PlayArrowIcon />}
            onClick={() => setStartOpen(true)}
            disabled={!agent.connected || isStreaming}
          >
            Start stream
          </Button>
          <Button
            variant="outlined"
            color="warning"
            startIcon={<StopIcon />}
            onClick={() => setStopOpen(true)}
            disabled={!isStreaming || stopStreamMutation.isPending}
          >
            Stop stream
          </Button>
          {!agent.connected && (
            <Typography variant="body2" color="text.secondary">
              Agent is offline.
            </Typography>
          )}
        </Stack>
      </SectionCard>

      <SectionCard title="Routing">
        <RoutingEditor agent={agent} />
      </SectionCard>

      <SectionCard title="Danger zone">
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
          <Button color="error" variant="outlined" onClick={() => setRemoveOpen(true)}>
            Remove agent
          </Button>
          <Typography variant="body2" color="text.secondary">
            Stops any active stream and removes the agent from the registry.
          </Typography>
        </Stack>
      </SectionCard>

      <StartStreamDialog agent={agent} open={startOpen} onClose={() => setStartOpen(false)} />

      <ConfirmDialog
        open={stopOpen}
        title="Stop stream?"
        message={`Stop the active stream to ${agent.name}?`}
        confirmLabel="Stop stream"
        busy={stopStreamMutation.isPending}
        onCancel={() => setStopOpen(false)}
        onConfirm={() =>
          stopStreamMutation.mutate(undefined, { onSettled: () => setStopOpen(false) })
        }
      />

      <ConfirmDialog
        open={removeOpen}
        title="Remove agent?"
        message={`Remove ${agent.name} from the registry? It will reappear if it reconnects.`}
        confirmLabel="Remove"
        destructive
        busy={removeAgentMutation.isPending}
        onCancel={() => setRemoveOpen(false)}
        onConfirm={() =>
          removeAgentMutation.mutate(undefined, {
            onSuccess: () => navigate('/agents'),
            onSettled: () => setRemoveOpen(false),
          })
        }
      />
    </Stack>
  )
}
