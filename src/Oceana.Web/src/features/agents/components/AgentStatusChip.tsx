import Chip from '@mui/material/Chip'
import Tooltip from '@mui/material/Tooltip'
import type { ChipProps } from '@mui/material/Chip'
import type { AgentStatus } from '../types'

/** Props for {@link AgentStatusChip}. */
export interface AgentStatusChipProps {
  /** The agent's streaming status. */
  status: AgentStatus
  /** The agent's most recent error, shown as a tooltip when faulted. */
  lastError?: string | null
}

const COLOURS: Record<AgentStatus, ChipProps['color']> = {
  Idle: 'default',
  Connecting: 'info',
  Streaming: 'success',
  Faulted: 'error',
}

/**
 * A colour-coded chip for an agent's streaming status. When faulted, the
 * agent's last error is revealed on hover.
 * @param props The component props.
 * @returns The rendered status chip.
 */
export function AgentStatusChip({ status, lastError }: AgentStatusChipProps) {
  const chip = (
    <Chip
      size="small"
      color={COLOURS[status]}
      label={status}
      variant={status === 'Idle' ? 'outlined' : 'filled'}
    />
  )

  if (status === 'Faulted' && lastError) {
    return <Tooltip title={lastError}>{chip}</Tooltip>
  }
  return chip
}
