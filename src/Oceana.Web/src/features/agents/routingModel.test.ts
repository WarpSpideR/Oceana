import { describe, expect, it } from 'vitest'
import { buildRouting, outputsToRows, parseChannels, type RoutingRow } from './routingModel'

describe('parseChannels', () => {
  it('parses comma-separated channels', () => {
    expect(parseChannels('0, 1, 2')).toEqual({ channels: [0, 1, 2] })
  })

  it('parses whitespace-separated channels', () => {
    expect(parseChannels('0 1  2')).toEqual({ channels: [0, 1, 2] })
  })

  it('rejects an empty list', () => {
    const result = parseChannels('   ')
    expect(result.channels).toEqual([])
    expect(result.error).toBeDefined()
  })

  it('rejects negative channels', () => {
    expect(parseChannels('0, -1').error).toBeDefined()
  })

  it('rejects non-integer channels', () => {
    expect(parseChannels('0, 1.5').error).toBeDefined()
    expect(parseChannels('a, b').error).toBeDefined()
  })
})

describe('buildRouting', () => {
  it('builds outputs for valid rows', () => {
    const rows: RoutingRow[] = [
      { device: 'dev-a', channelsText: '0, 1' },
      { device: null, channelsText: '2 3' },
    ]

    const result = buildRouting(rows)

    expect(result.valid).toBe(true)
    expect(result.outputs).toEqual([
      { device: 'dev-a', channels: [0, 1] },
      { device: null, channels: [2, 3] },
    ])
  })

  it('treats an empty row list as valid (all channels to default)', () => {
    const result = buildRouting([])
    expect(result.valid).toBe(true)
    expect(result.outputs).toEqual([])
  })

  it('marks the routing invalid when any row is invalid', () => {
    const rows: RoutingRow[] = [
      { device: 'dev-a', channelsText: '0' },
      { device: 'dev-b', channelsText: '' },
    ]

    const result = buildRouting(rows)

    expect(result.valid).toBe(false)
    expect(result.errors[0]).toBeUndefined()
    expect(result.errors[1]).toBeDefined()
  })
})

describe('outputsToRows', () => {
  it('round-trips outputs into editable rows', () => {
    const rows = outputsToRows([{ device: 'dev-a', channels: [0, 1] }])
    expect(rows).toEqual([{ device: 'dev-a', channelsText: '0, 1' }])
  })
})
