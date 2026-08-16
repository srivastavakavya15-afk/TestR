/// <reference types="vite/client" />

interface ImportMetaEnv {

  readonly VITE_AUTH_ENABLED?: string

  readonly VITE_OIDC_AUTHORITY?: string
  readonly VITE_OIDC_CLIENT_ID?: string
  readonly VITE_OIDC_SCOPE?: string

  readonly VITE_OIDC_AUDIENCE?: string

  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
