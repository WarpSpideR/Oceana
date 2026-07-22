import { useQuery } from '@tanstack/react-query'
import { listAgents } from '../api/agentsApi'
import { agentKeys } from '../api/agentKeys'

/**
 * Loads the full agent list. The initial fetch seeds the cache; the SignalR
 * hub ({@link useAgentHub}) keeps it live thereafter.
 * @returns The TanStack Query result for the agent list.
 */
export function useAgents() {
  return useQuery({
    queryKey: agentKeys.list(),
    queryFn: listAgents,
  })
}
