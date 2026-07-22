import { useEffect, useState } from 'react'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import TextField from '@mui/material/TextField'
import { errorMessage } from '../../../shared/api/http'
import { useCreateZone } from '../hooks/useZoneMutations'
import type { ZoneInfo } from '../types'

/** Props for {@link CreateZoneDialog}. */
export interface CreateZoneDialogProps {
  /** Whether the dialog is open. */
  open: boolean
  /** Called when the dialog should close. */
  onClose: () => void
  /** Called with the created zone after a successful create. */
  onCreated: (zone: ZoneInfo) => void
}

/**
 * Dialog for creating a new (initially empty) zone by name. Devices are assigned
 * afterwards on the zone's detail page.
 * @param props The component props.
 * @returns The rendered dialog.
 */
export function CreateZoneDialog({ open, onClose, onCreated }: CreateZoneDialogProps) {
  const createZoneMutation = useCreateZone()
  const [name, setName] = useState('')

  useEffect(() => {
    if (open) {
      setName('')
      createZoneMutation.reset()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  const trimmed = name.trim()
  const submit = () => {
    if (trimmed.length > 0) {
      createZoneMutation.mutate(
        { name: trimmed, devices: [] },
        { onSuccess: (zone) => onCreated(zone) },
      )
    }
  }

  const errorText = errorMessage(createZoneMutation.error)

  return (
    <Dialog
      open={open}
      onClose={createZoneMutation.isPending ? undefined : onClose}
      fullWidth
      maxWidth="xs"
    >
      <DialogTitle>New zone</DialogTitle>
      <DialogContent>
        <TextField
          autoFocus
          fullWidth
          label="Zone name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              submit()
            }
          }}
          sx={{ mt: 1 }}
        />
        {errorText && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {errorText}
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={createZoneMutation.isPending}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={submit}
          disabled={trimmed.length === 0 || createZoneMutation.isPending}
        >
          {createZoneMutation.isPending ? 'Creating…' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
