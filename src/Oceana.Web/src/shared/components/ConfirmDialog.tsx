import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogContentText from '@mui/material/DialogContentText'
import DialogTitle from '@mui/material/DialogTitle'

/** Props for {@link ConfirmDialog}. */
export interface ConfirmDialogProps {
  /** Whether the dialog is visible. */
  open: boolean
  /** The dialog title. */
  title: string
  /** The confirmation message shown to the operator. */
  message: string
  /** The label of the confirm button. Defaults to `Confirm`. */
  confirmLabel?: string
  /** Whether the action is destructive, colouring the confirm button red. */
  destructive?: boolean
  /** Whether the confirm action is in progress, disabling the buttons. */
  busy?: boolean
  /** Called when the operator confirms the action. */
  onConfirm: () => void
  /** Called when the operator dismisses the dialog. */
  onCancel: () => void
}

/**
 * A reusable yes/no confirmation dialog, used to gate destructive or disruptive
 * actions such as removing an agent or stopping a stream.
 * @param props The component props.
 * @returns The rendered dialog.
 */
export function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = 'Confirm',
  destructive = false,
  busy = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={busy ? undefined : onCancel}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <DialogContentText>{message}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={busy}>
          Cancel
        </Button>
        <Button
          onClick={onConfirm}
          disabled={busy}
          variant="contained"
          color={destructive ? 'error' : 'primary'}
          autoFocus
        >
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
