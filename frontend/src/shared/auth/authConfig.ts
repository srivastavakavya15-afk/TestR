export interface AuthConfig {
  enabled: boolean
  authority: string
  clientId: string
  scope: string

  audience?: string
}

const env = import.meta.env

export const authConfig: AuthConfig = {
  enabled: env.VITE_AUTH_ENABLED === 'true',
  authority: env.VITE_OIDC_AUTHORITY ?? '',
  clientId: env.VITE_OIDC_CLIENT_ID ?? '',
  scope: env.VITE_OIDC_SCOPE ?? 'openid profile email',
  audience: env.VITE_OIDC_AUDIENCE || undefined,
}

export const isAuthActive: boolean =
  authConfig.enabled && authConfig.authority !== '' && authConfig.clientId !== ''
