import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getZone } from '../api/zonesApi'
import { zoneKeys } from '../api/zoneKeys'
import type { ZoneInfo } from '../types'

/**
 * Loads a single zone by id, seeding from the list cache when available so the
 * detail view renders instantly. The SignalR hub keeps it live.
 * @param id The zone id.
 * @returns The TanStack Query result for the zone.
 */
export function useZone(id: string) {
  const queryClient = useQueryClient()

  return useQuery({
    queryKey: zoneKeys.detail(id),
    queryFn: () => getZone(id),
    initialData: () =>
      queryClient.getQueryData<ZoneInfo[]>(zoneKeys.list())?.find((zone) => zone.id === id),
  })
}
