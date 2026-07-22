/**
 * TypeScript mirrors of the Oceana server's agent DTOs. These match the JSON
 * emitted by the REST API and the SignalR status hub (enums as strings). See
 * `src/Oceana.Server/Features/Agents/AgentInfo.cs` and `src/Oceana.Contracts/*`.
 */

/** The streaming state of an agent. Serialised as a string by the server. */
export type AgentStatus = 'Idle' | 'Connecting' | 'Streaming' | 'Faulted'

/** An audio render device reported by an agent. */
export interface AudioDevice {
  /** The stable device id. */
  id: string
  /** The human-readable device name. */
  name: string
}

/** Routes an ordered set of source channels to one output device. */
export interface AudioOutput {
  /** The target device id or friendly name; `null` selects the default device. */
  device: string | null
  /** The ordered source channel indices routed to the device. */
  channels: number[]
}

/** The full channel-to-device routing for an agent. Empty = all channels to default. */
export interface AgentRouting {
  /** The configured outputs. */
  outputs: AudioOutput[]
}

/** A registered agent with its connection, streaming state, devices and routing. */
export interface AgentInfo {
  /** The stable identifier the agent assigned to itself. */
  id: string
  /** The human-readable agent name. */
  name: string
  /** The host or IP the server dials for audio. */
  host: string
  /** The TCP port on which the agent listens for audio. */
  port: number
  /** Whether the agent's control connection is currently established. */
  connected: boolean
  /** The current streaming status. */
  status: AgentStatus
  /** The most recent fault message, or `null` when healthy. */
  lastError: string | null
  /** The render devices reported by the agent. */
  devices: AudioDevice[]
  /** The desired channel-to-device routing. */
  routing: AgentRouting
}

/** The body of a start-stream request. */
export interface StartStreamRequest {
  /** The base tone frequency in hertz (20–20000). */
  frequency: number
  /** The number of channels to stream (1–8). */
  channels: number
  /** Optional duration in seconds (0.1–3600); omit to play until stopped. */
  durationSeconds?: number | null
}

/** The response returned when a stream starts. */
export interface StartStreamResponse {
  /** The target agent id. */
  agentId: string
  /** The base tone frequency that was started. */
  frequency: number
}

/** Client-side validation bounds, mirroring the server's FluentValidation rules. */
export const STREAM_LIMITS = {
  frequency: { min: 20, max: 20000, default: 440 },
  channels: { min: 1, max: 8, default: 2 },
  durationSeconds: { min: 0.1, max: 3600 },
} as const
