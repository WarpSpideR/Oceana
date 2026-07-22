import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { API_BASE_URL } from '../../../shared/api/config'
import { broadcastToZone } from './zonesApi'

describe('broadcastToZone', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('POSTs the WAV blob as audio/wav and returns the summary', async () => {
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
})
