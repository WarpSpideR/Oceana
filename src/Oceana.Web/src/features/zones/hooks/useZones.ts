import { useQuery } from '@tanstack/react-query'
import { listZones } from '../api/zonesApi'
import { zoneKeys } from '../api/zoneKeys'

/**
 * Loads the full zone list. The initial fetch seeds the cache; the SignalR hub
 * ({@link useZoneHub}) keeps it live thereafter.
 * @returns The TanStack Query result for the zone list.
 */
export function useZones() {
  return useQuery({
    queryKey: zoneKeys.list(),
    queryFn: listZones,
  })
}
