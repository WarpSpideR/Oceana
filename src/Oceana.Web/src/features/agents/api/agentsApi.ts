import { jsonBody, request } from '../../../shared/api/http'
import type {
  AgentInfo,
  AgentRouting,
  StartStreamRequest,
  StartStreamResponse,
} from '../types'

/** Fetches all registered agents. `GET /api/agents`. */
export function listAgents(): Promise<AgentInfo[]> {
  return request<AgentInfo[]>('/api/agents')
}

/** Fetches a single agent by id. `GET /api/agents/{id}`. */
export function getAgent(id: string): Promise<AgentInfo> {
  return request<AgentInfo>(`/api/agents/${id}`)
}

/**
 * Stores and pushes an agent's channel-to-device routing. `PUT /api/agents/{id}/routing`.
 * @param id The target agent id.
 * @param routing The desired routing.
 * @returns The updated agent.
 */
export function setRouting(id: string, routing: AgentRouting): Promise<AgentInfo> {
  return request<AgentInfo>(`/api/agents/${id}/routing`, {
    method: 'PUT',
    body: jsonBody(routing),
  })
}

/**
 * Starts a test-tone stream to an agent. `POST /api/agents/{id}/stream`.
 * @param id The target agent id.
 * @param req The stream parameters.
 * @returns The start-stream acknowledgement.
 */
export function startStream(id: string, req: StartStreamRequest): Promise<StartStreamResponse> {
  return request<StartStreamResponse>(`/api/agents/${id}/stream`, {
    method: 'POST',
    body: jsonBody(req),
  })
}

/** Stops the active stream to an agent. `DELETE /api/agents/{id}/stream`. */
export function stopStream(id: string): Promise<void> {
  return request<void>(`/api/agents/${id}/stream`, { method: 'DELETE' })
}

/** Removes an agent from the registry. `DELETE /api/agents/{id}`. */
export function removeAgent(id: string): Promise<void> {
  return request<void>(`/api/agents/${id}`, { method: 'DELETE' })
}
