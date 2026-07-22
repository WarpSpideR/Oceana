import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

// React Testing Library does not auto-clean up under Vitest, so unmount and
// clear the DOM between tests to keep them isolated.
afterEach(() => {
  cleanup()
})
