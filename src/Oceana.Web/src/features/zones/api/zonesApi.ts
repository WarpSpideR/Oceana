import { jsonBody, request } from '../../../shared/api/http'
import type { CreateZoneRequest, UpdateZoneRequest, ZoneInfo } from '../types'

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
