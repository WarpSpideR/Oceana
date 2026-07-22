/**
 * Centralised TanStack Query keys for the agents slice, so cache reads/writes
 * (including live updates from the SignalR hub) stay consistent.
 */
export const agentKeys = {
  /** Root key namespacing every agents query. */
  all: ['agents'] as const,
  /** Key for the full agent list query. */
  list: () => [...agentKeys.all, 'list'] as const,
  /** Key for a single agent's detail query. */
  detail: (id: string) => [...agentKeys.all, 'detail', id] as const,
}
