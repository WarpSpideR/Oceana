import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AgentDetailPage } from '../features/agents/pages/AgentDetailPage'
import { AgentsListPage } from '../features/agents/pages/AgentsListPage'
import { ZoneDetailPage } from '../features/zones/pages/ZoneDetailPage'
import { ZonesListPage } from '../features/zones/pages/ZonesListPage'
import { App } from './App'

/** The application route table. */
export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <Navigate to="/agents" replace /> },
      { path: 'agents', element: <AgentsListPage /> },
      { path: 'agents/:id', element: <AgentDetailPage /> },
      { path: 'zones', element: <ZonesListPage /> },
      { path: 'zones/:id', element: <ZoneDetailPage /> },
      { path: '*', element: <Navigate to="/agents" replace /> },
    ],
  },
])
