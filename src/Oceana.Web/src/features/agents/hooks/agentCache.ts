import type { QueryClient } from '@tanstack/react-query'
import { agentKeys } from '../api/agentKeys'
import type { AgentInfo } from '../types'

/**
 * Applies an `AgentChanged` hub event to the query cache: upserts the agent in
 * the list query and refreshes its detail query. Pure with respect to the
 * passed client, so it can be unit-tested without a live hub connection.
 * @param queryClient The query client whose cache to update.
 * @param agent The agent snapshot pushed by the server.
 */
export function applyAgentChanged(queryClient: QueryClient, agent: AgentInfo): void {
  queryClient.setQueryData<AgentInfo[]>(agentKeys.list(), (previous) => {
    const list = previous ?? []
    const index = list.findIndex((a) => a.id === agent.id)
    if (index === -1) {
      return [...list, agent]
    }
    const next = list.slice()
    next[index] = agent
    return next
  })
  queryClient.setQueryData<AgentInfo>(agentKeys.detail(agent.id), agent)
}

/**
 * Applies an `AgentRemoved` hub event to the query cache: drops the agent from
 * the list query and discards its detail query.
 * @param queryClient The query client whose cache to update.
 * @param agentId The id of the removed agent.
 */
export function applyAgentRemoved(queryClient: QueryClient, agentId: string): void {
  queryClient.setQueryData<AgentInfo[]>(agentKeys.list(), (previous) =>
    (previous ?? []).filter((a) => a.id !== agentId),
  )
  queryClient.removeQueries({ queryKey: agentKeys.detail(agentId) })
}
