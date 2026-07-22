import type { AudioOutput } from './types'

/** A single editable routing row in the {@link RoutingEditor}. */
export interface RoutingRow {
  /** The target device id/name, or `null` for the agent's default device. */
  device: string | null
  /** The raw, user-entered channel list (comma/space separated). */
  channelsText: string
}

/** The result of parsing one row's channel text. */
export interface ParsedChannels {
  /** The parsed channel indices (empty when invalid). */
  channels: number[]
  /** A validation message when the text is invalid, otherwise `undefined`. */
  error?: string
}

/**
 * Parses a user-entered channel list into ordered channel indices.
 *
 * Accepts comma- and/or whitespace-separated non-negative integers. Mirrors the
 * server's rule that every output must route at least one non-negative channel.
 * @param text The raw channel text.
 * @returns The parsed channels and any validation error.
 */
export function parseChannels(text: string): ParsedChannels {
  const tokens = text.split(/[\s,]+/).filter((token) => token.length > 0)
  if (tokens.length === 0) {
    return { channels: [], error: 'Enter at least one channel.' }
  }

  const channels: number[] = []
  for (const token of tokens) {
    const value = Number(token)
    if (!Number.isInteger(value) || value < 0) {
      return { channels: [], error: 'Channels must be non-negative whole numbers.' }
    }
    channels.push(value)
  }
  return { channels }
}

/** The result of building routing from editor rows. */
export interface BuiltRouting {
  /** The outputs to send to the server (valid only when `valid` is true). */
  outputs: AudioOutput[]
  /** Per-row validation errors, aligned by index with the input rows. */
  errors: (string | undefined)[]
  /** Whether every row parsed successfully. */
  valid: boolean
}

/**
 * Builds an {@link AudioOutput} array from editor rows, validating each.
 *
 * An empty row list is valid and yields no outputs (the server interprets that
 * as "play all channels to the default device").
 * @param rows The editor rows.
 * @returns The built outputs, per-row errors and overall validity.
 */
export function buildRouting(rows: RoutingRow[]): BuiltRouting {
  const outputs: AudioOutput[] = []
  const errors: (string | undefined)[] = []
  let valid = true

  for (const row of rows) {
    const parsed = parseChannels(row.channelsText)
    errors.push(parsed.error)
    if (parsed.error) {
      valid = false
      continue
    }
    outputs.push({ device: row.device, channels: parsed.channels })
  }

  return { outputs, errors, valid }
}

/**
 * Converts an agent's stored routing into editor rows for display.
 * @param outputs The stored outputs.
 * @returns The equivalent editor rows.
 */
export function outputsToRows(outputs: AudioOutput[]): RoutingRow[] {
  return outputs.map((output) => ({
    device: output.device,
    channelsText: output.channels.join(', '),
  }))
}
