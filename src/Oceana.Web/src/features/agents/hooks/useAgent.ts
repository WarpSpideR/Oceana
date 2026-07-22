import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getAgent } from '../api/agentsApi'
import { agentKeys } from '../api/agentKeys'
import type { AgentInfo } from '../types'

/**
 * Loads a single agent by id, seeding from the list cache when available so the
 * detail view renders instantly. The SignalR hub keeps it live.
 * @param id The agent id.
 * @returns The TanStack Query result for the agent.
 */
export function useAgent(id: string) {
  const queryClient = useQueryClient()

  return useQuery({
    queryKey: agentKeys.detail(id),
    queryFn: () => getAgent(id),
    initialData: () =>
      queryClient
        .getQueryData<AgentInfo[]>(agentKeys.list())
        ?.find((agent) => agent.id === id),
  })
}
