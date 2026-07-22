import { describe, expect, it } from 'vitest'
import type { AgentInfo } from '../agents/types'
import type { ZoneDevice } from './types'
import { isDeviceSelected, resolveZoneDevices, toggleDevice } from './zoneModel'

function agent(overrides: Partial<AgentInfo> = {}): AgentInfo {
  return {
    id: 'agent-1',
    name: 'living-room',
    host: '127.0.0.1',
    port: 8090,
    connected: true,
    status: 'Idle',
    lastError: null,
    devices: [{ id: 'device-1', name: 'Speakers' }],
    routing: { outputs: [] },
    ...overrides,
  }
}

describe('toggleDevice / isDeviceSelected', () => {
  it('adds a device when absent', () => {
    const result = toggleDevice([], 'agent-1', 'device-1')
    expect(result).toEqual([{ agentId: 'agent-1', deviceId: 'device-1' }])
    expect(isDeviceSelected(result, 'agent-1', 'device-1')).toBe(true)
  })

  it('removes a device when present', () => {
    const start: ZoneDevice[] = [{ agentId: 'agent-1', deviceId: 'device-1' }]
    const result = toggleDevice(start, 'agent-1', 'device-1')
    expect(result).toEqual([])
    expect(isDeviceSelected(result, 'agent-1', 'device-1')).toBe(false)
  })

  it('distinguishes the same device id under different agents', () => {
    const start: ZoneDevice[] = [{ agentId: 'agent-1', deviceId: 'device-1' }]
    expect(isDeviceSelected(start, 'agent-2', 'device-1')).toBe(false)
    const result = toggleDevice(start, 'agent-2', 'device-1')
    expect(result).toHaveLength(2)
  })
})

describe('resolveZoneDevices', () => {
  it('marks a device available and resolves names when its agent and device are present', () => {
    const resolved = resolveZoneDevices([{ agentId: 'agent-1', deviceId: 'device-1' }], [agent()])
    expect(resolved[0]).toMatchObject({
      available: true,
      agentName: 'living-room',
      deviceName: 'Speakers',
    })
  })

  it('marks a device unavailable when its agent is missing', () => {
    const resolved = resolveZoneDevices([{ agentId: 'ghost', deviceId: 'device-1' }], [agent()])
    expect(resolved[0].available).toBe(false)
    expect(resolved[0].agentName).toBeUndefined()
  })

  it('marks a device unavailable when the agent no longer reports it', () => {
    const resolved = resolveZoneDevices(
      [{ agentId: 'agent-1', deviceId: 'gone' }],
      [agent()],
    )
    expect(resolved[0].available).toBe(false)
    expect(resolved[0].agentName).toBe('living-room')
    expect(resolved[0].deviceName).toBeUndefined()
  })
})
