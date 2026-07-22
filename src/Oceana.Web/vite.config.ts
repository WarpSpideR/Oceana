/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
// The SPA talks to the Oceana server directly using CORS (the server allows any
// origin in development), so no dev proxy is configured. The server base URL is
// resolved at runtime from VITE_API_BASE_URL (see src/shared/api/config.ts).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/shared/test/setup.ts'],
    css: false,
  },
})
