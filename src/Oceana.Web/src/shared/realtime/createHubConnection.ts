import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'

/** The lifecycle state of a hub connection, surfaced to the UI. */
export type HubStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

/**
 * Combines several hub statuses into a single worst-case status, so a banner can
 * reflect overall live-connection health: connected only when all are connected.
 * @param statuses The individual hub statuses.
 * @returns The combined status.
 */
export function combineHubStatus(...statuses: HubStatus[]): HubStatus {
  if (statuses.includes('disconnected')) {
    return 'disconnected'
  }
  if (statuses.includes('reconnecting')) {
    return 'reconnecting'
  }
  if (statuses.includes('connecting')) {
    return 'connecting'
  }
  return 'connected'
}

/**
 * Creates a SignalR hub connection with automatic reconnection enabled.
 *
 * The connection is returned unstarted; the caller is responsible for wiring
 * event handlers before calling `start()`.
 * @param url The absolute hub URL.
 * @returns The configured, unstarted {@link HubConnection}.
 */
export function createHubConnection(url: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(url)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
