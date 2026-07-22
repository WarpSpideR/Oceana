import { useEffect, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { AGENTS_HUB_URL } from '../../../shared/api/config'
import {
  createHubConnection,
  type HubStatus,
} from '../../../shared/realtime/createHubConnection'
import type { AgentInfo } from '../types'
import { applyAgentChanged, applyAgentRemoved } from './agentCache'

/**
 * Opens the front-end status hub connection and keeps the agents query cache
 * live: `AgentChanged` upserts, `AgentRemoved` deletes. The connection is the
 * live source of truth, so REST mutations need no optimistic updates.
 *
 * Intended to be used once, near the composition root.
 * @returns The current hub connection status, for surfacing to the UI.
 */
export function useAgentHub(): HubStatus {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<HubStatus>('connecting')

  useEffect(() => {
    const connection = createHubConnection(AGENTS_HUB_URL)
    let disposed = false

    const onChanged = (agent: AgentInfo) => applyAgentChanged(queryClient, agent)
    const onRemoved = (agentId: string) => applyAgentRemoved(queryClient, agentId)

    connection.on('AgentChanged', onChanged)
    connection.on('AgentRemoved', onRemoved)
    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => {
      if (!disposed) {
        setStatus('disconnected')
      }
    })

    setStatus('connecting')
    connection
      .start()
      .then(() => {
        if (!disposed) {
          setStatus('connected')
        }
      })
      .catch(() => {
        if (!disposed) {
          setStatus('disconnected')
        }
      })

    return () => {
      disposed = true
      connection.off('AgentChanged', onChanged)
      connection.off('AgentRemoved', onRemoved)
      void connection.stop()
    }
  }, [queryClient])

  return status
}
