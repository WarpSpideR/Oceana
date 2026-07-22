import type { AgentInfo } from '../agents/types'
import type { ZoneDevice } from './types'

/** A stable key for a device across agents. */
export function deviceKey(agentId: string, deviceId: string): string {
  return `${agentId}:${deviceId}`
}

/** Whether the given device is present in a list of zone assignments. */
export function isDeviceSelected(
  devices: ZoneDevice[],
  agentId: string,
  deviceId: string,
): boolean {
  return devices.some((d) => d.agentId === agentId && d.deviceId === deviceId)
}

/**
 * Toggles a device's membership in a zone-assignment list, returning a new list.
 * @param devices The current assignments.
 * @param agentId The device's agent id.
 * @param deviceId The device id.
 * @returns A new list with the device added if absent, or removed if present.
 */
export function toggleDevice(
  devices: ZoneDevice[],
  agentId: string,
  deviceId: string,
): ZoneDevice[] {
  if (isDeviceSelected(devices, agentId, deviceId)) {
    return devices.filter((d) => !(d.agentId === agentId && d.deviceId === deviceId))
  }
  return [...devices, { agentId, deviceId }]
}

/** A zone device enriched with display info resolved against the current agents. */
export interface ResolvedZoneDevice {
  /** The device's agent id. */
  agentId: string
  /** The device id. */
  deviceId: string
  /** The agent's display name, when the agent is currently known. */
  agentName?: string
  /** The device's display name, when the device is currently reported. */
  deviceName?: string
  /** Whether the referenced agent and device are currently present. */
  available: boolean
}

/**
 * Resolves a zone's device assignments against the current agents, adding display
 * names and marking assignments whose agent/device is no longer present as
 * unavailable.
 * @param devices The zone's device assignments.
 * @param agents The currently known agents.
 * @returns The resolved, display-ready device list.
 */
export function resolveZoneDevices(
  devices: ZoneDevice[],
  agents: AgentInfo[],
): ResolvedZoneDevice[] {
  return devices.map((device) => {
    const agent = agents.find((a) => a.id === device.agentId)
    const reported = agent?.devices.find((d) => d.id === device.deviceId)
    return {
      agentId: device.agentId,
      deviceId: device.deviceId,
      agentName: agent?.name,
      deviceName: reported?.name,
      available: agent !== undefined && reported !== undefined,
    }
  })
}
