import { createContext, useContext } from 'react'

export interface AuthSession {

  enabled: boolean
  isAuthenticated: boolean
  isLoading: boolean

  userName?: string
  error?: string
  signIn: () => void
  signOut: () => void
}

export const anonymousSession: AuthSession = {
  enabled: false,
  isAuthenticated: true,
  isLoading: false,
  signIn: () => {},
  signOut: () => {},
}

export const AuthContext = createContext<AuthSession>(anonymousSession)

export function useAuthSession(): AuthSession {
  return useContext(AuthContext)
}
