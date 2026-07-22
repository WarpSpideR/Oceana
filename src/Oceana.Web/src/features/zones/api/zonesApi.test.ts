import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { API_BASE_URL } from '../../../shared/api/config'
import { broadcastToZone, playToZone, setZoneVolume } from './zonesApi'

describe('zonesApi audio uploads', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('broadcastToZone POSTs the WAV blob as audio/wav and returns the summary', async () => {
    const summary = { zoneId: 'zone-1', targeted: [], skipped: [] }
    fetchMock.mockResolvedValue(new Response(JSON.stringify(summary), { status: 202 }))
    const wav = new Blob([new Uint8Array([1, 2, 3])], { type: 'audio/wav' })

    const result = await broadcastToZone('zone-1', wav)

    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe(`${API_BASE_URL}/api/zones/zone-1/broadcast`)
    expect(init?.method).toBe('POST')
    expect(new Headers(init?.headers).get('Content-Type')).toBe('audio/wav')
    expect(init?.body).toBe(wav)
    expect(result).toEqual(summary)
  })

  it('playToZone POSTs to /play with the file name and audio/wav', async () => {
    const state = { zoneId: 'zone-1', playing: true, sourceName: 'my track.mp3', startedAtUtc: null, targeted: [], skipped: [] }
    fetchMock.mockResolvedValue(new Response(JSON.stringify(state), { status: 202 }))
    const wav = new Blob([new Uint8Array([1, 2, 3])], { type: 'audio/wav' })

    const result = await playToZone('zone-1', wav, 'my track.mp3')

    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe(`${API_BASE_URL}/api/zones/zone-1/play?name=my%20track.mp3`)
    expect(init?.method).toBe('POST')
    expect(new Headers(init?.headers).get('Content-Type')).toBe('audio/wav')
    expect(init?.body).toBe(wav)
    expect(result.playing).toBe(true)
  })

  it('setZoneVolume PUTs the volume as JSON', async () => {
    const updated = { id: 'zone-1', name: 'Kitchen', devices: [], volume: 0.4 }
    fetchMock.mockResolvedValue(new Response(JSON.stringify(updated), { status: 200 }))

    const result = await setZoneVolume('zone-1', 0.4)

    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe(`${API_BASE_URL}/api/zones/zone-1/volume`)
    expect(init?.method).toBe('PUT')
    expect(init?.body).toBe(JSON.stringify({ volume: 0.4 }))
    expect(result.volume).toBe(0.4)
  })
})
