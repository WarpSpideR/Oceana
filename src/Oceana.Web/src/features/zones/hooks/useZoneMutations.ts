import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createZone, removeZone, updateZone } from '../api/zonesApi'
import { zoneKeys } from '../api/zoneKeys'
import type { CreateZoneRequest, UpdateZoneRequest } from '../types'

/**
 * Mutation for creating a zone. The server broadcasts `ZoneChanged` on success,
 * which updates the cache; the `onSettled` invalidation is a safety net.
 * @returns The create-zone mutation.
 */
export function useCreateZone() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateZoneRequest) => createZone(body),
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: zoneKeys.all })
    },
  })
}

/**
 * Mutations for updating and removing a single zone.
 * @param zoneId The target zone id.
 * @returns The update and remove mutations.
 */
export function useZoneMutations(zoneId: string) {
  const queryClient = useQueryClient()
  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: zoneKeys.all })
  }

  const updateZoneMutation = useMutation({
    mutationFn: (body: UpdateZoneRequest) => updateZone(zoneId, body),
    onSettled: invalidate,
  })

  const removeZoneMutation = useMutation({
    mutationFn: () => removeZone(zoneId),
    onSettled: invalidate,
  })

  return { updateZoneMutation, removeZoneMutation }
}
