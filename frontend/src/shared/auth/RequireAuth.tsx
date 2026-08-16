import type { ReactNode } from 'react'
import { Button } from '@/shared/ui/Button'
import { Spinner } from '@/shared/ui/Spinner'
import { useAuthSession } from './AuthContext'

interface RequireAuthProps {
  children: ReactNode
}

export function RequireAuth({ children }: RequireAuthProps) {
  const { isAuthenticated, isLoading, error, signIn } = useAuthSession()

  if (isLoading) {
    return <Spinner label="Checking your session" />
  }

  if (!isAuthenticated) {
    return (
      <div className="mx-auto max-w-md rounded-lg border border-line bg-surface p-8 text-center">
        <h1 className="text-lg font-semibold text-ink">Sign in required</h1>
        <p className="mt-2 text-sm text-ink-muted">
          You need to be signed in to add a user. Browsing the list stays open to everyone.
        </p>
        {error && (
          <p role="alert" className="mt-3 text-sm font-medium text-danger">
            {error}
          </p>
        )}
        <Button className="mt-5" onClick={signIn}>
          Sign in
        </Button>
      </div>
    )
  }

  return <>{children}</>
}
