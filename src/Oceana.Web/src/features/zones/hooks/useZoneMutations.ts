import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  broadcastToZone,
  createZone,
  playToZone,
  removeZone,
  setZoneVolume,
  stopZonePlayback,
  updateZone,
} from '../api/zonesApi'
import { zoneKeys } from '../api/zoneKeys'
import type { CreateZoneRequest, UpdateZoneRequest, ZoneInfo, ZonePlaybackState } from '../types'

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

/**
 * Mutation for setting a zone's volume. On success the returned zone is written into the caches so
 * the slider and list reflect it immediately (the hub `ZoneChanged` also keeps it live).
 * @param zoneId The target zone id.
 * @returns The set-volume mutation (takes a 0–1 volume).
 */
export function useSetZoneVolume(zoneId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (volume: number) => setZoneVolume(zoneId, volume),
    onSuccess: (zone) => {
      queryClient.setQueryData<ZoneInfo>(zoneKeys.detail(zoneId), zone)
      queryClient.setQueryData<ZoneInfo[]>(zoneKeys.list(), (previous) =>
        previous?.map((z) => (z.id === zone.id ? zone : z)),
      )
    },
  })
}

/**
 * Mutation for broadcasting a recorded message to a zone. A broadcast doesn't change zone state,
 * so there is no cache invalidation.
 * @param zoneId The target zone id.
 * @returns The broadcast mutation (takes a WAV blob).
 */
export function useBroadcastToZone(zoneId: string) {
  return useMutation({
    mutationFn: (wav: Blob) => broadcastToZone(zoneId, wav),
  })
}

/**
 * Mutations for starting and stopping file playback to a zone. On success the returned/known
 * playback state is written into the cache; the hub keeps it live thereafter.
 * @param zoneId The target zone id.
 * @returns The play and stop mutations.
 */
export function useZonePlaybackMutations(zoneId: string) {
  const queryClient = useQueryClient()

  const playMutation = useMutation({
    mutationFn: ({ wav, name }: { wav: Blob; name: string }) => playToZone(zoneId, wav, name),
    onSuccess: (state) => {
      queryClient.setQueryData<ZonePlaybackState | null>(
        zoneKeys.playback(zoneId),
        state.playing ? state : null,
      )
    },
  })

  const stopMutation = useMutation({
    mutationFn: () => stopZonePlayback(zoneId),
    onSuccess: () => {
      queryClient.setQueryData<ZonePlaybackState | null>(zoneKeys.playback(zoneId), null)
    },
  })

  return { playMutation, stopMutation }
}
