import { QueryClient } from '@tanstack/react-query'
import { describe, expect, it } from 'vitest'
import { zoneKeys } from '../api/zoneKeys'
import type { ZoneInfo, ZonePlaybackState } from '../types'
import { applyZoneChanged, applyZonePlaybackChanged, applyZoneRemoved } from './zoneCache'

function makeZone(overrides: Partial<ZoneInfo> = {}): ZoneInfo {
  return {
    id: 'zone-1',
    name: 'Kitchen',
    devices: [],
    volume: 1,
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

describe('applyZonePlaybackChanged', () => {
  function state(playing: boolean): ZonePlaybackState {
    return {
      zoneId: 'zone-1',
      playing,
      sourceName: playing ? 'track.wav' : null,
      startedAtUtc: null,
      targeted: [],
      skipped: [],
    }
  }

  it('stores the state under the playback key when playing', () => {
    const client = new QueryClient()

    applyZonePlaybackChanged(client, state(true))

    expect(client.getQueryData<ZonePlaybackState | null>(zoneKeys.playback('zone-1'))?.playing).toBe(true)
  })

  it('clears to null when not playing', () => {
    const client = new QueryClient()
    client.setQueryData<ZonePlaybackState | null>(zoneKeys.playback('zone-1'), state(true))

    applyZonePlaybackChanged(client, state(false))

    expect(client.getQueryData<ZonePlaybackState | null>(zoneKeys.playback('zone-1'))).toBeNull()
  })
})
