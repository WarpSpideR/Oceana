import type { QueryClient } from '@tanstack/react-query'
import { zoneKeys } from '../api/zoneKeys'
import type { ZoneInfo } from '../types'

/**
 * Applies a `ZoneChanged` hub event to the query cache: upserts the zone in the
 * list query and refreshes its detail query. Pure with respect to the passed
 * client, so it can be unit-tested without a live hub connection.
 * @param queryClient The query client whose cache to update.
 * @param zone The zone snapshot pushed by the server.
 */
export function applyZoneChanged(queryClient: QueryClient, zone: ZoneInfo): void {
  queryClient.setQueryData<ZoneInfo[]>(zoneKeys.list(), (previous) => {
    const list = previous ?? []
    const index = list.findIndex((z) => z.id === zone.id)
    if (index === -1) {
      return [...list, zone]
    }
    const next = list.slice()
    next[index] = zone
    return next
  })
  queryClient.setQueryData<ZoneInfo>(zoneKeys.detail(zone.id), zone)
}

/**
 * Applies a `ZoneRemoved` hub event to the query cache: drops the zone from the
 * list query and discards its detail query.
 * @param queryClient The query client whose cache to update.
 * @param zoneId The id of the removed zone.
 */
export function applyZoneRemoved(queryClient: QueryClient, zoneId: string): void {
  queryClient.setQueryData<ZoneInfo[]>(zoneKeys.list(), (previous) =>
    (previous ?? []).filter((z) => z.id !== zoneId),
  )
  queryClient.removeQueries({ queryKey: zoneKeys.detail(zoneId) })
}
