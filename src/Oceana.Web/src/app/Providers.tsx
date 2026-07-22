import { useState, type ReactNode } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { useAgentHub } from '../features/agents/hooks/useAgentHub'
import { useZoneHub } from '../features/zones/hooks/useZoneHub'
import { combineHubStatus } from '../shared/realtime/createHubConnection'
import { createQueryClient } from './queryClient'
import { HubStatusContext } from './hubStatusContext'
import { theme } from './theme'

/**
 * Wires the live-updates hubs (agents + zones, which need the query client) and
 * exposes their combined status to the app shell.
 */
function HubStatusProvider({ children }: { children: ReactNode }) {
  const agentStatus = useAgentHub()
  const zoneStatus = useZoneHub()
  const status = combineHubStatus(agentStatus, zoneStatus)
  return <HubStatusContext.Provider value={status}>{children}</HubStatusContext.Provider>
}

/**
 * Composition root for cross-cutting providers: TanStack Query, the MUI theme
 * and baseline styles, and the SignalR-backed live-updates connection.
 * @param props The component props.
 * @returns The provider tree wrapping the app.
 */
export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(createQueryClient)

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme} defaultMode="system">
        <CssBaseline />
        <HubStatusProvider>{children}</HubStatusProvider>
      </ThemeProvider>
    </QueryClientProvider>
  )
}
