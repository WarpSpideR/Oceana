import { Link as RouterLink } from 'react-router-dom'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Paper from '@mui/material/Paper'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import { AgentStatusChip } from './AgentStatusChip'
import type { AgentInfo } from '../types'

/** Props for {@link AgentsTable}. */
export interface AgentsTableProps {
  /** The agents to display. */
  agents: AgentInfo[]
}

/**
 * A table of registered agents with their connection, status, address and
 * device count, each linking to its detail page.
 * @param props The component props.
 * @returns The rendered table.
 */
export function AgentsTable({ agents }: AgentsTableProps) {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table aria-label="Registered agents">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Connection</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Address</TableCell>
            <TableCell align="right">Devices</TableCell>
            <TableCell align="right" />
          </TableRow>
        </TableHead>
        <TableBody>
          {agents.map((agent) => (
            <TableRow key={agent.id} hover>
              <TableCell>{agent.name}</TableCell>
              <TableCell>
                <Chip
                  size="small"
                  label={agent.connected ? 'Connected' : 'Offline'}
                  color={agent.connected ? 'success' : 'default'}
                  variant={agent.connected ? 'filled' : 'outlined'}
                />
              </TableCell>
              <TableCell>
                <AgentStatusChip status={agent.status} lastError={agent.lastError} />
              </TableCell>
              <TableCell>
                {agent.host}:{agent.port}
              </TableCell>
              <TableCell align="right">{agent.devices.length}</TableCell>
              <TableCell align="right">
                <Button
                  component={RouterLink}
                  to={`/agents/${agent.id}`}
                  size="small"
                >
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
