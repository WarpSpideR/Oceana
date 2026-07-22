import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { AgentsTable } from '../components/AgentsTable'
import { useAgents } from '../hooks/useAgents'

/**
 * The dashboard page: a live list of every registered agent. Agents self-register
 * over the control connection and appear here automatically via the status hub.
 * @returns The rendered agents list page.
 */
export function AgentsListPage() {
  const { data: agents, isPending, isError, error, refetch } = useAgents()

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h4" component="h1">
          Agents
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Playback agents currently registered with the server.
        </Typography>
      </Box>

      {isPending && (
        <Stack sx={{ py: 6, alignItems: 'center' }}>
          <CircularProgress />
        </Stack>
      )}

      {isError && (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => void refetch()}>
              Retry
            </Button>
          }
        >
          Could not load agents: {error instanceof Error ? error.message : 'Unknown error'}
        </Alert>
      )}

      {!isPending && !isError && agents.length === 0 && (
        <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" gutterBottom>
            No agents connected
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Start an Oceana agent — it self-registers with the server and will appear here.
          </Typography>
        </Paper>
      )}

      {!isPending && !isError && agents.length > 0 && <AgentsTable agents={agents} />}
    </Stack>
  )
}
