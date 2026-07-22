import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { API_BASE_URL } from './config'
import { ApiError, request } from './http'

describe('request', () => {
  const fetchMock = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('calls the absolute server URL for the given path', async () => {
    fetchMock.mockResolvedValue(new Response('[]', { status: 200 }))

    await request('/api/agents')

    const [url] = fetchMock.mock.calls[0]
    expect(url).toBe(`${API_BASE_URL}/api/agents`)
  })

  it('sets a JSON content type when a body is present', async () => {
    fetchMock.mockResolvedValue(new Response('{}', { status: 200 }))

    await request('/api/agents/1/routing', {
      method: 'PUT',
      body: JSON.stringify({ outputs: [] }),
    })

    const init = fetchMock.mock.calls[0][1]
    const headers = new Headers(init?.headers)
    expect(init?.method).toBe('PUT')
    expect(headers.get('Content-Type')).toBe('application/json')
  })

  it('parses and returns the JSON response body', async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify([{ id: 'a', name: 'living-room' }]), { status: 200 }),
    )

    const agents = await request<{ id: string; name: string }[]>('/api/agents')

    expect(agents).toEqual([{ id: 'a', name: 'living-room' }])
  })

  it('resolves to undefined for a 204 response', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }))

    const result = await request<void>('/api/agents/1/stream', { method: 'DELETE' })

    expect(result).toBeUndefined()
  })

  it('throws an ApiError carrying the status and validation errors on 400', async () => {
    const body = {
      statusCode: 400,
      message: 'Validation failed',
      errors: { 'outputs[0].channels': ['Each output must route at least one channel.'] },
    }
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify(body), {
        status: 400,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    const error = await request('/api/agents/1/routing', {
      method: 'PUT',
      body: '{}',
    }).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    const apiError = error as ApiError
    expect(apiError.status).toBe(400)
    expect(apiError.message).toBe('Validation failed')
    expect(apiError.validationErrors?.['outputs[0].channels']).toHaveLength(1)
  })

  it('throws an ApiError on 404', async () => {
    fetchMock.mockResolvedValue(new Response('', { status: 404, statusText: 'Not Found' }))

    const error = await request('/api/agents/missing').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(404)
  })
})
