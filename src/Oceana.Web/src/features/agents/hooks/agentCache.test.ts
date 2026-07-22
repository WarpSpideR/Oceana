import { QueryClient } from '@tanstack/react-query'
import { describe, expect, it } from 'vitest'
import { agentKeys } from '../api/agentKeys'
import type { AgentInfo } from '../types'
import { applyAgentChanged, applyAgentRemoved } from './agentCache'

function makeAgent(overrides: Partial<AgentInfo> = {}): AgentInfo {
  return {
    id: 'agent-1',
    name: 'living-room',
    host: '127.0.0.1',
    port: 8090,
    connected: true,
    status: 'Idle',
    lastError: null,
    devices: [],
    routing: { outputs: [] },
    ...overrides,
  }
}

describe('applyAgentChanged', () => {
  it('appends a newly seen agent to the list cache', () => {
    const client = new QueryClient()
    client.setQueryData<AgentInfo[]>(agentKeys.list(), [])

    applyAgentChanged(client, makeAgent())

    expect(client.getQueryData<AgentInfo[]>(agentKeys.list())).toHaveLength(1)
  })

  it('updates an existing agent in place without duplicating it', () => {
    const client = new QueryClient()
    client.setQueryData<AgentInfo[]>(agentKeys.list(), [makeAgent({ status: 'Idle' })])

    applyAgentChanged(client, makeAgent({ status: 'Streaming' }))

    const list = client.getQueryData<AgentInfo[]>(agentKeys.list())
    expect(list).toHaveLength(1)
    expect(list?.[0].status).toBe('Streaming')
  })

  it('refreshes the detail cache for the agent', () => {
    const client = new QueryClient()

    applyAgentChanged(client, makeAgent({ status: 'Faulted', lastError: 'boom' }))

    const detail = client.getQueryData<AgentInfo>(agentKeys.detail('agent-1'))
    expect(detail?.status).toBe('Faulted')
    expect(detail?.lastError).toBe('boom')
  })

  it('starts a list from a cold cache', () => {
    const client = new QueryClient()

    applyAgentChanged(client, makeAgent())

    expect(client.getQueryData<AgentInfo[]>(agentKeys.list())).toHaveLength(1)
  })
})

describe('applyAgentRemoved', () => {
  it('drops the agent from the list cache', () => {
    const client = new QueryClient()
    client.setQueryData<AgentInfo[]>(agentKeys.list(), [
      makeAgent({ id: 'agent-1' }),
      makeAgent({ id: 'agent-2' }),
    ])

    applyAgentRemoved(client, 'agent-1')

    const list = client.getQueryData<AgentInfo[]>(agentKeys.list())
    expect(list).toHaveLength(1)
    expect(list?.[0].id).toBe('agent-2')
  })
})
