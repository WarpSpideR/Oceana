import { createTheme } from '@mui/material/styles'

/**
 * The application MUI theme. Uses the system colour-scheme preference so the
 * dashboard renders in light or dark to match the operator's environment.
 */
export const theme = createTheme({
  cssVariables: true,
  colorSchemes: {
    light: true,
    dark: true,
  },
  palette: {
    primary: {
      main: '#0b5394',
    },
  },
  shape: {
    borderRadius: 8,
  },
})
