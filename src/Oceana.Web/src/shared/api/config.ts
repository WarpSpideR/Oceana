/**
 * Runtime configuration for reaching the Oceana server.
 *
 * The base URL comes from the `VITE_API_BASE_URL` environment variable (see
 * `.env.example`); when unset it falls back to the server's Kestrel default.
 * The SPA calls the server directly and relies on the server's permissive CORS
 * policy in development, so there is no dev proxy.
 */

const DEFAULT_BASE_URL = 'http://localhost:5069'

/** The Oceana server base URL, without a trailing slash. */
export const API_BASE_URL: string = (
  import.meta.env.VITE_API_BASE_URL ?? DEFAULT_BASE_URL
).replace(/\/+$/, '')

/**
 * Builds an absolute URL for a REST endpoint under the server's `/api` prefix.
 * @param path A path beginning with `/api`, e.g. `/api/agents`.
 * @returns The absolute URL.
 */
export function apiUrl(path: string): string {
  return `${API_BASE_URL}${path}`
}

/** The absolute URL of the agent-status SignalR hub. */
export const AGENTS_HUB_URL: string = `${API_BASE_URL}/hubs/agents`

/** The absolute URL of the zone-status SignalR hub. */
export const ZONES_HUB_URL: string = `${API_BASE_URL}/hubs/zones`
