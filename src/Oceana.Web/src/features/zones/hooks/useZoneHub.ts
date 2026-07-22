import { useEffect, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { ZONES_HUB_URL } from '../../../shared/api/config'
import {
  createHubConnection,
  type HubStatus,
} from '../../../shared/realtime/createHubConnection'
import type { ZoneInfo, ZonePlaybackState } from '../types'
import { applyZoneChanged, applyZonePlaybackChanged, applyZoneRemoved } from './zoneCache'

/**
 * Opens the zone status hub connection and keeps the zones query cache live:
 * `ZoneChanged` upserts, `ZoneRemoved` deletes, `ZonePlaybackChanged` updates the
 * zone's playback state. Intended to be used once, near the composition root.
 * @returns The current hub connection status, for surfacing to the UI.
 */
export function useZoneHub(): HubStatus {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<HubStatus>('connecting')

  useEffect(() => {
    const connection = createHubConnection(ZONES_HUB_URL)
    let disposed = false

    const onChanged = (zone: ZoneInfo) => applyZoneChanged(queryClient, zone)
    const onRemoved = (zoneId: string) => applyZoneRemoved(queryClient, zoneId)
    const onPlaybackChanged = (state: ZonePlaybackState) =>
      applyZonePlaybackChanged(queryClient, state)

    connection.on('ZoneChanged', onChanged)
    connection.on('ZoneRemoved', onRemoved)
    connection.on('ZonePlaybackChanged', onPlaybackChanged)
    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => {
      if (!disposed) {
        setStatus('disconnected')
      }
    })

    setStatus('connecting')
    connection
      .start()
      .then(() => {
        if (!disposed) {
          setStatus('connected')
        }
      })
      .catch(() => {
        if (!disposed) {
          setStatus('disconnected')
        }
      })

    return () => {
      disposed = true
      connection.off('ZoneChanged', onChanged)
      connection.off('ZoneRemoved', onRemoved)
      connection.off('ZonePlaybackChanged', onPlaybackChanged)
      void connection.stop()
    }
  }, [queryClient])

  return status
}
