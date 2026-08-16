import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router-dom'
import { RequireAuth } from '@/shared/auth/RequireAuth'
import { Spinner } from '@/shared/ui/Spinner'

const UserListPage = lazy(() =>
  import('@/features/users/UserListPage').then((m) => ({ default: m.UserListPage })),
)
const UserAddPage = lazy(() =>
  import('@/features/users/UserAddPage').then((m) => ({ default: m.UserAddPage })),
)

export function AppRoutes() {
  return (
    <Suspense fallback={<Spinner />}>
      <Routes>
        <Route path="/" element={<UserListPage />} />
        <Route
          path="/add"
          element={
            <RequireAuth>
              <UserAddPage />
            </RequireAuth>
          }
        />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Suspense>
  )
}

function NotFound() {
  return (
    <div className="rounded-lg border border-dashed border-line bg-surface p-10 text-center">
      <h1 className="font-medium text-ink">Page not found</h1>
      <p className="mt-1 text-sm text-ink-muted">Try the List or Add links above.</p>
    </div>
  )
}
