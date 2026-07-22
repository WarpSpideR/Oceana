import Alert from '@mui/material/Alert'
import Collapse from '@mui/material/Collapse'
import type { HubStatus } from '../realtime/createHubConnection'

/** Props for {@link ConnectionBanner}. */
export interface ConnectionBannerProps {
  /** The current status of the live-updates hub connection. */
  status: HubStatus
}

const MESSAGES: Record<Exclude<HubStatus, 'connected'>, string> = {
  connecting: 'Connecting to the server for live updates…',
  reconnecting: 'Connection lost — reconnecting for live updates…',
  disconnected: 'Not connected to the server. Data may be out of date.',
}

/**
 * A dismissible-free banner that appears whenever the live-updates hub is not
 * connected, so the operator knows the agent list may be stale.
 * @param props The component props.
 * @returns The rendered banner, collapsed to nothing when connected.
 */
export function ConnectionBanner({ status }: ConnectionBannerProps) {
  const open = status !== 'connected'
  const severity = status === 'disconnected' ? 'error' : 'warning'

  return (
    <Collapse in={open} unmountOnExit>
      <Alert severity={severity} variant="filled" square sx={{ borderRadius: 0 }}>
        {open ? MESSAGES[status] : ''}
      </Alert>
    </Collapse>
  )
}
