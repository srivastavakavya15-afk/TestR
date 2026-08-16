import { useEffect, useMemo, type ReactNode } from 'react'
import { AuthProvider as OidcProvider, useAuth } from 'react-oidc-context'
import { WebStorageStateStore } from 'oidc-client-ts'
import { setAccessTokenProvider } from '@/api/client'
import { AuthContext, anonymousSession, type AuthSession } from './AuthContext'
import { authConfig, isAuthActive } from './authConfig'

const oidcSettings = {
  authority: authConfig.authority,
  client_id: authConfig.clientId,
  redirect_uri: `${window.location.origin}/`,
  post_logout_redirect_uri: `${window.location.origin}/`,
  response_type: 'code',
  scope: authConfig.scope,

  ...(authConfig.audience ? { extraQueryParams: { audience: authConfig.audience } } : {}),
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),

  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname)
  },
}

function OidcSessionBridge({ children }: { children: ReactNode }) {
  const auth = useAuth()
  const accessToken = auth.user?.access_token

  useEffect(() => {
    setAccessTokenProvider(() => accessToken)
    return () => setAccessTokenProvider(() => undefined)
  }, [accessToken])

  const session = useMemo<AuthSession>(
    () => ({
      enabled: true,
      isAuthenticated: auth.isAuthenticated,
      isLoading: auth.isLoading,
      userName: auth.user?.profile.name ?? auth.user?.profile.preferred_username,
      error: auth.error?.message,
      signIn: () => void auth.signinRedirect(),
      signOut: () => void auth.signoutRedirect(),
    }),
    [auth],
  )

  return <AuthContext.Provider value={session}>{children}</AuthContext.Provider>
}

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  if (!isAuthActive) {
    return <AuthContext.Provider value={anonymousSession}>{children}</AuthContext.Provider>
  }

  return (
    <OidcProvider {...oidcSettings}>
      <OidcSessionBridge>{children}</OidcSessionBridge>
    </OidcProvider>
  )
}
