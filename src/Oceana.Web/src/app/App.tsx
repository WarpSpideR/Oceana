import { Link as RouterLink, NavLink, Outlet } from 'react-router-dom'
import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import GraphicEqIcon from '@mui/icons-material/GraphicEq'
import { ConnectionBanner } from '../shared/components/ConnectionBanner'
import { useHubStatus } from './hubStatusContext'

const navButtonSx = {
  color: 'inherit',
  '&.active': { backgroundColor: 'rgba(255, 255, 255, 0.16)' },
}

/**
 * The application shell: a top app bar with primary navigation, the
 * live-connection banner, and the routed page content.
 * @returns The rendered layout.
 */
export function App() {
  const hubStatus = useHubStatus()

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="static">
        <Toolbar>
          <GraphicEqIcon sx={{ mr: 1.5 }} />
          <Typography
            variant="h6"
            component={RouterLink}
            to="/agents"
            sx={{ color: 'inherit', textDecoration: 'none', mr: 3 }}
          >
            Oceana
          </Typography>
          <Stack direction="row" spacing={1} sx={{ flexGrow: 1 }}>
            <Button component={NavLink} to="/agents" sx={navButtonSx}>
              Agents
            </Button>
            <Button component={NavLink} to="/zones" sx={navButtonSx}>
              Zones
            </Button>
          </Stack>
        </Toolbar>
      </AppBar>

      <ConnectionBanner status={hubStatus} />

      <Container maxWidth="md" component="main" sx={{ py: 4, flexGrow: 1 }}>
        <Outlet />
      </Container>
    </Box>
  )
}
