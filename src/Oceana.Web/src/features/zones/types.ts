/**
 * TypeScript mirrors of the Oceana server's zone DTOs (see
 * `src/Oceana.Server/Features/Zones/*`).
 */

/** A single audio device assigned to a zone: a device belonging to an agent. */
export interface ZoneDevice {
  /** The identifier of the agent that owns the device. */
  agentId: string
  /** The agent-scoped stable device id. */
  deviceId: string
}

/** A named group of audio devices that audio can be streamed to together. */
export interface ZoneInfo {
  /** The stable zone identifier. */
  id: string
  /** The human-readable zone name. */
  name: string
  /** The devices assigned to the zone. */
  devices: ZoneDevice[]
}

/** Body of a create-zone request. */
export interface CreateZoneRequest {
  /** The zone name (required, unique). */
  name: string
  /** The devices to assign. */
  devices: ZoneDevice[]
}

/** Body of an update-zone request (id is supplied in the route). */
export interface UpdateZoneRequest {
  /** The new zone name. */
  name: string
  /** The devices to assign. */
  devices: ZoneDevice[]
}

/** Why an agent in a zone was skipped by a broadcast. */
export type BroadcastSkipReason = 'Offline' | 'Busy' | 'NoActiveDevices'

/** An agent a broadcast is playing to. */
export interface BroadcastTarget {
  /** The agent id. */
  agentId: string
  /** The agent name. */
  agentName: string
  /** How many of the agent's devices the message plays on. */
  deviceCount: number
}

/** An agent in the zone the broadcast skipped, with the reason. */
export interface BroadcastSkip {
  /** The agent id. */
  agentId: string
  /** The agent name, or its id when unknown. */
  agentName: string
  /** Why the agent was skipped. */
  reason: BroadcastSkipReason
}

/** The summary the server returns when a broadcast starts. */
export interface BroadcastResult {
  /** The zone that was broadcast to. */
  zoneId: string
  /** The agents the message is playing to. */
  targeted: BroadcastTarget[]
  /** The agents that were skipped, with reasons. */
  skipped: BroadcastSkip[]
}
