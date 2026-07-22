import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { ReactNode } from 'react'
import type { AgentInfo } from '../types'
import { AgentsListPage } from './AgentsListPage'

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

function agent(overrides: Partial<AgentInfo> = {}): AgentInfo {
  return {
    id: 'agent-1',
    name: 'living-room',
    host: '127.0.0.1',
    port: 8090,
    connected: true,
    status: 'Idle',
    lastError: null,
    devices: [{ id: 'dev-a', name: 'Speakers' }],
    routing: { outputs: [] },
    ...overrides,
  }
}

describe('AgentsListPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    fetchMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the empty state when no agents are registered', async () => {
    fetchMock.mockResolvedValue(new Response('[]', { status: 200 }))

    renderPage(<AgentsListPage />)

    expect(await screen.findByText('No agents connected')).toBeInTheDocument()
  })

  it('renders a row for each registered agent', async () => {
    fetchMock.mockResolvedValue(
      new Response(JSON.stringify([agent({ name: 'living-room' })]), { status: 200 }),
    )

    renderPage(<AgentsListPage />)

    expect(await screen.findByText('living-room')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /manage/i })).toHaveAttribute(
      'href',
      '/agents/agent-1',
    )
  })

  it('shows an error with a retry action when the request fails', async () => {
    fetchMock.mockResolvedValue(new Response('', { status: 500, statusText: 'Server Error' }))

    renderPage(<AgentsListPage />)

    expect(await screen.findByText(/could not load agents/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument()
  })
})
