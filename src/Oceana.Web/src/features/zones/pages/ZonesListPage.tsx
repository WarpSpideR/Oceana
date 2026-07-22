import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import AddIcon from '@mui/icons-material/Add'
import { CreateZoneDialog } from '../components/CreateZoneDialog'
import { ZonesTable } from '../components/ZonesTable'
import { useZones } from '../hooks/useZones'

/**
 * The zones page: a live list of every configured zone, with a create action.
 * @returns The rendered zones list page.
 */
export function ZonesListPage() {
  const { data: zones, isPending, isError, error, refetch } = useZones()
  const navigate = useNavigate()
  const [createOpen, setCreateOpen] = useState(false)

  return (
    <Stack spacing={3}>
      <Stack direction="row" spacing={2} sx={{ alignItems: 'flex-start' }}>
        <Box sx={{ flexGrow: 1 }}>
          <Typography variant="h4" component="h1">
            Zones
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Groups of audio devices that audio can be streamed to together.
          </Typography>
        </Box>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreateOpen(true)}>
          New zone
        </Button>
      </Stack>

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
          Could not load zones: {error instanceof Error ? error.message : 'Unknown error'}
        </Alert>
      )}

      {!isPending && !isError && zones.length === 0 && (
        <Paper variant="outlined" sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" gutterBottom>
            No zones yet
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Create a zone, then assign devices from your agents to it.
          </Typography>
        </Paper>
      )}

      {!isPending && !isError && zones.length > 0 && <ZonesTable zones={zones} />}

      <CreateZoneDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(zone) => {
          setCreateOpen(false)
          navigate(`/zones/${zone.id}`)
        }}
      />
    </Stack>
  )
}
