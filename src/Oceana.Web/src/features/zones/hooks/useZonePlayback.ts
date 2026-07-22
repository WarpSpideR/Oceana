import { useQuery } from '@tanstack/react-query'
import { getZonePlayback } from '../api/zonesApi'
import { zoneKeys } from '../api/zoneKeys'

/**
 * Loads a zone's current playback state. The initial fetch seeds the cache; the
 * SignalR hub ({@link useZoneHub}) keeps it live via `ZonePlaybackChanged`.
 * @param zoneId The zone id.
 * @returns The TanStack Query result for the zone's playback state (null when idle).
 */
export function useZonePlayback(zoneId: string) {
  return useQuery({
    queryKey: zoneKeys.playback(zoneId),
    queryFn: () => getZonePlayback(zoneId),
  })
}
