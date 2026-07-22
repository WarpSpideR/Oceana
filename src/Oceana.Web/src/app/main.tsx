import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { Providers } from './Providers'
import { router } from './router'
import './global.css'

const container = document.getElementById('root')
if (!container) {
  throw new Error('Root element #root was not found in the document.')
}

createRoot(container).render(
  <StrictMode>
    <Providers>
      <RouterProvider router={router} />
    </Providers>
  </StrictMode>,
)
