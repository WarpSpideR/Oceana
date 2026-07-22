import { QueryClient } from '@tanstack/react-query'

/**
 * Creates the app's TanStack Query client. Data is kept fresh primarily by the
 * SignalR status hub, so background refetching is relaxed.
 * @returns A configured {@link QueryClient}.
 */
export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 30_000,
        refetchOnWindowFocus: false,
        retry: 1,
      },
    },
  })
}
