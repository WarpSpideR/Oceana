import { QueryClient } from '@tanstack/react-query'
import { describe, expect, it } from 'vitest'
import { zoneKeys } from '../api/zoneKeys'
import type { ZoneInfo } from '../types'
import { applyZoneChanged, applyZoneRemoved } from './zoneCache'

function makeZone(overrides: Partial<ZoneInfo> = {}): ZoneInfo {
  return {
    id: 'zone-1',
    name: 'Kitchen',
    devices: [],
    ...overrides,
  }
}

describe('applyZoneChanged', () => {
  it('appends a newly seen zone to the list cache', () => {
    const client = new QueryClient()
    client.setQueryData<ZoneInfo[]>(zoneKeys.list(), [])

    applyZoneChanged(client, makeZone())

    expect(client.getQueryData<ZoneInfo[]>(zoneKeys.list())).toHaveLength(1)
  })

  it('updates an existing zone in place without duplicating it', () => {
    const client = new QueryClient()
    client.setQueryData<ZoneInfo[]>(zoneKeys.list(), [makeZone({ name: 'Kitchen' })])

    applyZoneChanged(client, makeZone({ name: 'Kitchen & Diner' }))

    const list = client.getQueryData<ZoneInfo[]>(zoneKeys.list())
    expect(list).toHaveLength(1)
    expect(list?.[0].name).toBe('Kitchen & Diner')
  })

  it('refreshes the detail cache for the zone', () => {
    const client = new QueryClient()

    applyZoneChanged(client, makeZone({ devices: [{ agentId: 'a', deviceId: 'd' }] }))

    expect(client.getQueryData<ZoneInfo>(zoneKeys.detail('zone-1'))?.devices).toHaveLength(1)
  })
})

describe('applyZoneRemoved', () => {
  it('drops the zone from the list cache', () => {
    const client = new QueryClient()
    client.setQueryData<ZoneInfo[]>(zoneKeys.list(), [
      makeZone({ id: 'zone-1' }),
      makeZone({ id: 'zone-2', name: 'Lounge' }),
    ])

    applyZoneRemoved(client, 'zone-1')

    const list = client.getQueryData<ZoneInfo[]>(zoneKeys.list())
    expect(list).toHaveLength(1)
    expect(list?.[0].id).toBe('zone-2')
  })
})
