import { apiUrl } from './config'

/**
 * The validation error body FastEndpoints returns on a 400 response: a map of
 * field name to the list of messages for that field.
 */
export type ValidationErrors = Record<string, string[]>

/** The shape of the JSON error body the Oceana server returns on failure. */
interface ErrorBody {
  statusCode?: number
  message?: string
  errors?: ValidationErrors
}

/** An error thrown when the server responds with a non-success status code. */
export class ApiError extends Error {
  /** The HTTP status code of the failed response. */
  public readonly status: number

  /** The field-level validation errors, when the server returned any. */
  public readonly validationErrors?: ValidationErrors

  /**
   * Initialises a new instance of the {@link ApiError} class.
   * @param status The HTTP status code.
   * @param message A human-readable description of the failure.
   * @param validationErrors The field-level validation errors, if any.
   */
  public constructor(status: number, message: string, validationErrors?: ValidationErrors) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
  }
}

async function parseError(response: Response): Promise<ApiError> {
  let body: ErrorBody | undefined
  try {
    body = (await response.json()) as ErrorBody
  } catch {
    // Non-JSON (or empty) error body; fall back to the status text.
  }

  const message =
    body?.message ?? response.statusText ?? `Request failed with status ${response.status}`
  return new ApiError(response.status, message, body?.errors)
}

/**
 * Sends a request to a server REST endpoint and returns the parsed JSON body.
 *
 * A `204 No Content` response resolves to `undefined`. Any non-success status
 * code rejects with an {@link ApiError} carrying the status and, for `400`
 * responses, the field-level validation errors.
 * @typeParam T The expected response body type.
 * @param path The endpoint path beginning with `/api`.
 * @param init Optional `fetch` options; `Content-Type: application/json` is
 * applied automatically when a body is present.
 * @returns The parsed response body, or `undefined` for an empty response.
 */
export async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  if (init?.body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(apiUrl(path), { ...init, headers })

  if (!response.ok) {
    throw await parseError(response)
  }

  if (response.status === 204 || response.headers.get('Content-Length') === '0') {
    return undefined as T
  }

  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

/** Serialises a value to a JSON request body. */
export function jsonBody(value: unknown): string {
  return JSON.stringify(value)
}

/**
 * Extracts the most useful human-readable message from a thrown error.
 *
 * For an {@link ApiError} it prefers the server's field-level validation
 * messages (which carry the specific reason, e.g. a duplicate-name conflict)
 * over the generic top-level message.
 * @param error The caught error (typically a mutation/query error).
 * @returns A display message, or null when there is no error.
 */
export function errorMessage(error: unknown): string | null {
  if (error instanceof ApiError) {
    if (error.validationErrors) {
      const messages = Object.values(error.validationErrors).flat()
      if (messages.length > 0) {
        return messages.join(' ')
      }
    }
    return error.message
  }
  if (error instanceof Error) {
    return error.message
  }
  return error ? String(error) : null
}
