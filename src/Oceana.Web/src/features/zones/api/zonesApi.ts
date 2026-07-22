import { jsonBody, request } from '../../../shared/api/http'
import type {
  BroadcastResult,
  CreateZoneRequest,
  UpdateZoneRequest,
  ZoneInfo,
  ZonePlaybackState,
} from '../types'

/** Fetches all zones. `GET /api/zones`. */
export function listZones(): Promise<ZoneInfo[]> {
  return request<ZoneInfo[]>('/api/zones')
}

/** Fetches a single zone by id. `GET /api/zones/{id}`. */
export function getZone(id: string): Promise<ZoneInfo> {
  return request<ZoneInfo>(`/api/zones/${id}`)
}

/** Creates a zone. `POST /api/zones`. */
export function createZone(body: CreateZoneRequest): Promise<ZoneInfo> {
  return request<ZoneInfo>('/api/zones', {
    method: 'POST',
    body: jsonBody(body),
  })
}

/** Replaces a zone's name and devices. `PUT /api/zones/{id}`. */
export function updateZone(id: string, body: UpdateZoneRequest): Promise<ZoneInfo> {
  return request<ZoneInfo>(`/api/zones/${id}`, {
    method: 'PUT',
    body: jsonBody(body),
  })
}

/** Removes a zone. `DELETE /api/zones/{id}`. */
export function removeZone(id: string): Promise<void> {
  return request<void>(`/api/zones/${id}`, { method: 'DELETE' })
}

/**
 * Sets a zone's playback volume. `PUT /api/zones/{id}/volume`.
 * @param id The zone id.
 * @param volume The volume, 0.0 (silent)–1.0 (full).
 * @returns The updated zone.
 */
export function setZoneVolume(id: string, volume: number): Promise<ZoneInfo> {
  return request<ZoneInfo>(`/api/zones/${id}/volume`, {
    method: 'PUT',
    body: jsonBody({ volume }),
  })
}

/**
 * Broadcasts a recorded message (a mono/48 kHz/16-bit PCM WAV) to a zone.
 * `POST /api/zones/{id}/broadcast`.
 * @param id The zone id.
 * @param wav The WAV audio blob.
 * @returns The broadcast summary (agents targeted and skipped).
 */
export function broadcastToZone(id: string, wav: Blob): Promise<BroadcastResult> {
  return request<BroadcastResult>(`/api/zones/${id}/broadcast`, {
    method: 'POST',
    body: wav,
    // Explicit content type suppresses the default JSON header for this binary upload.
    headers: { 'Content-Type': 'audio/wav' },
  })
}

/**
 * Plays an audio file (a 48 kHz, 16-bit PCM WAV) to a zone. `POST /api/zones/{id}/play`.
 * @param id The zone id.
 * @param wav The WAV audio blob.
 * @param name A display name for the source (e.g. the file name).
 * @returns The resulting playback state.
 */
export function playToZone(id: string, wav: Blob, name: string): Promise<ZonePlaybackState> {
  return request<ZonePlaybackState>(`/api/zones/${id}/play?name=${encodeURIComponent(name)}`, {
    method: 'POST',
    body: wav,
    headers: { 'Content-Type': 'audio/wav' },
  })
}

/** Stops a zone's current playback. `DELETE /api/zones/{id}/play`. */
export function stopZonePlayback(id: string): Promise<void> {
  return request<void>(`/api/zones/${id}/play`, { method: 'DELETE' })
}

/** Fetches a zone's current playback state, or null when nothing is playing. `GET /api/zones/{id}/playback`. */
export async function getZonePlayback(id: string): Promise<ZonePlaybackState | null> {
  return (await request<ZonePlaybackState | null>(`/api/zones/${id}/playback`)) ?? null
}
