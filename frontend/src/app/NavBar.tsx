import { NavLink } from 'react-router-dom'
import { Button } from '@/shared/ui/Button'
import { useAuthSession } from '@/shared/auth/AuthContext'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-md px-3 py-2 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand ${
    isActive ? 'bg-brand/10 text-brand' : 'text-ink-muted hover:bg-surface-muted hover:text-ink'
  }`

export function NavBar() {
  const { enabled, isAuthenticated, userName, signIn, signOut } = useAuthSession()

  return (
    <header className="border-b border-line bg-surface">
      <nav
        aria-label="Main"
        className="mx-auto flex max-w-5xl flex-wrap items-center gap-3 px-4 py-3 sm:px-6"
      >
        <span className="mr-2 text-base font-semibold text-ink">User Directory</span>

        <NavLink to="/" end className={linkClass}>
          List
        </NavLink>
        <NavLink to="/add" className={linkClass}>
          Add
        </NavLink>

        {enabled && (
          <div className="ml-auto flex items-center gap-3">
            {isAuthenticated ? (
              <>
                {userName && <span className="text-sm text-ink-muted">{userName}</span>}
                <Button variant="secondary" onClick={signOut}>
                  Sign out
                </Button>
              </>
            ) : (
              <Button onClick={signIn}>Sign in</Button>
            )}
          </div>
        )}
      </nav>
    </header>
  )
}
