import { createContext, useContext } from 'react'
import type { HubStatus } from '../shared/realtime/createHubConnection'

/** Context carrying the live-updates hub status down to the app shell. */
export const HubStatusContext = createContext<HubStatus>('connecting')

/** Reads the current live-updates hub status. */
export function useHubStatus(): HubStatus {
  return useContext(HubStatusContext)
}
