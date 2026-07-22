import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { ReactNode } from 'react'
import type { ZoneInfo } from '../types'
import { ZonesListPage } from './ZonesListPage'

const fetchMock = vi.fn<typeof fetch>()

function renderPage(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>,
  )
}

function zone(overrides: Partial<ZoneInfo> = {}): ZoneInfo {
  return {
    id: 'zone-1',
    name: 'Kitchen',
    devices: [{ agentId: 'agent-1', deviceId: 'device-1' }],
    ...overrides,
  }
}

describe('ZonesListPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the empty state when there are no zones', async () => {
    fetchMock.mockResolvedValue(new Response('[]', { status: 200 }))

    renderPage(<ZonesListPage />)

    expect(await screen.findByText('No zones yet')).toBeInTheDocument()
  })

  it('renders a row for each zone with a manage link', async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify([zone({ name: 'Kitchen' })]), { status: 200 }),
    )

    renderPage(<ZonesListPage />)

    expect(await screen.findByText('Kitchen')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /manage/i })).toHaveAttribute('href', '/zones/zone-1')
  })

  it('shows an error with a retry action when the request fails', async () => {
    fetchMock.mockResolvedValue(new Response('', { status: 500, statusText: 'Server Error' }))

    renderPage(<ZonesListPage />)

    expect(await screen.findByText(/could not load zones/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument()
  })
})
