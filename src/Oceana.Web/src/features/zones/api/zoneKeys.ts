/**
 * Centralised TanStack Query keys for the zones slice, keeping cache reads/writes
 * (including live updates from the SignalR hub) consistent.
 */
export const zoneKeys = {
  /** Root key namespacing every zones query. */
  all: ['zones'] as const,
  /** Key for the full zone list query. */
  list: () => [...zoneKeys.all, 'list'] as const,
  /** Key for a single zone's detail query. */
  detail: (id: string) => [...zoneKeys.all, 'detail', id] as const,
  /** Key for a single zone's playback state query. */
  playback: (id: string) => [...zoneKeys.all, 'playback', id] as const,
}
