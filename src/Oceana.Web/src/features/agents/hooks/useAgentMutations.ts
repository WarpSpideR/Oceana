import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  removeAgent,
  setRouting,
  startStream,
  stopStream,
} from '../api/agentsApi'
import { agentKeys } from '../api/agentKeys'
import type { AgentRouting, StartStreamRequest } from '../types'

/**
 * Mutations for driving a single agent: routing, stream start/stop and removal.
 *
 * The server broadcasts an `AgentChanged`/`AgentRemoved` event over the status
 * hub after each of these succeeds, which updates the cache; the `onSettled`
 * invalidation here is a safety net for when the hub is momentarily down.
 * @param agentId The target agent id.
 * @returns The four mutation objects.
 */
export function useAgentMutations(agentId: string) {
  const queryClient = useQueryClient()
  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: agentKeys.all })
  }

  const setRoutingMutation = useMutation({
    mutationFn: (routing: AgentRouting) => setRouting(agentId, routing),
    onSettled: invalidate,
  })

  const startStreamMutation = useMutation({
    mutationFn: (req: StartStreamRequest) => startStream(agentId, req),
    onSettled: invalidate,
  })

  const stopStreamMutation = useMutation({
    mutationFn: () => stopStream(agentId),
    onSettled: invalidate,
  })

  const removeAgentMutation = useMutation({
    mutationFn: () => removeAgent(agentId),
    onSettled: invalidate,
  })

  return {
    setRoutingMutation,
    startStreamMutation,
    stopStreamMutation,
    removeAgentMutation,
  }
}
