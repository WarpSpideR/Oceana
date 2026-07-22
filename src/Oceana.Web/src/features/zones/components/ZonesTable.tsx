import { Link as RouterLink } from 'react-router-dom'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import type { ZoneInfo } from '../types'

/** Props for {@link ZonesTable}. */
export interface ZonesTableProps {
  /** The zones to display. */
  zones: ZoneInfo[]
}

/**
 * A table of zones with their assigned-device counts, each linking to its detail page.
 * @param props The component props.
 * @returns The rendered table.
 */
export function ZonesTable({ zones }: ZonesTableProps) {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table aria-label="Zones">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell align="right">Devices</TableCell>
            <TableCell align="right" />
          </TableRow>
        </TableHead>
        <TableBody>
          {zones.map((zone) => (
            <TableRow key={zone.id} hover>
              <TableCell>{zone.name}</TableCell>
              <TableCell align="right">{zone.devices.length}</TableCell>
              <TableCell align="right">
                <Button component={RouterLink} to={`/zones/${zone.id}`} size="small">
                  Manage
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
