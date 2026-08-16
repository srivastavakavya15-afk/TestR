import createClient from 'openapi-fetch'
import type { components, paths } from './generated/schema'

export type UserDto = components['schemas']['UserDto']
export type CreateUserRequest = components['schemas']['CreateUserRequest']
export type UpdateUserRequest = components['schemas']['UpdateUserRequest']

export type FieldErrors = Record<string, string[]>

export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors?: FieldErrors

  constructor(status: number, title: string, fieldErrors?: FieldErrors) {
    super(title)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }

  get isValidation(): boolean {
    return this.status === 400 && this.fieldErrors !== undefined
  }
}

let accessTokenProvider: () => string | undefined = () => undefined

export function setAccessTokenProvider(provider: () => string | undefined): void {
  accessTokenProvider = provider
}

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? '/'

const http = createClient<paths>({
  baseUrl,

  fetch: (request) => globalThis.fetch(request),
})

http.use({
  onRequest({ request }) {
    const token = accessTokenProvider()
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`)
    }
    return request
  },
})

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function toFieldErrors(value: unknown): FieldErrors | undefined {
  if (!isRecord(value)) return undefined

  const entries = Object.entries(value).flatMap<[string, string[]]>(([field, messages]) =>
    Array.isArray(messages) && messages.every((m) => typeof m === 'string')
      ? [[field, messages]]
      : [],
  )

  return entries.length > 0 ? Object.fromEntries(entries) : undefined
}

function toApiError(status: number, body: unknown): ApiError {
  const problem = isRecord(body) ? body : {}
  const fieldErrors = toFieldErrors(problem.errors)
  const title = typeof problem.title === 'string' ? problem.title : `Request failed (${status})`

  const detail = typeof problem.detail === 'string' ? problem.detail : undefined

  const message = !fieldErrors && detail ? detail : title

  return new ApiError(status, message, fieldErrors)
}

function unwrap<T>(result: {
  data?: T
  error?: unknown
  response: Response
}): T {
  if (result.error !== undefined || !result.response.ok) {
    throw toApiError(result.response.status, result.error)
  }
  return result.data as T
}

export interface RequestOptions {
  signal?: AbortSignal
}

export const usersApi = {
  async list({ signal }: RequestOptions = {}): Promise<UserDto[]> {
    return unwrap(await http.GET('/api/users', { signal }))
  },

  async getById(id: string, { signal }: RequestOptions = {}): Promise<UserDto> {
    return unwrap(await http.GET('/api/users/{id}', { params: { path: { id } }, signal }))
  },

  async create(body: CreateUserRequest, { signal }: RequestOptions = {}): Promise<UserDto> {
    return unwrap(await http.POST('/api/users', { body, signal }))
  },

  async update(
    id: string,
    body: UpdateUserRequest,
    { signal }: RequestOptions = {},
  ): Promise<UserDto> {
    return unwrap(await http.PUT('/api/users/{id}', { params: { path: { id } }, body, signal }))
  },

  async remove(id: string, { signal }: RequestOptions = {}): Promise<void> {
    const result = await http.DELETE('/api/users/{id}', { params: { path: { id } }, signal })
    if (result.error !== undefined || !result.response.ok) {
      throw toApiError(result.response.status, result.error)
    }
  },
}
