import { Link } from 'react-router-dom'
import { ApiError, type UserDto } from '@/api/client'
import { Button } from '@/shared/ui/Button'
import { Spinner } from '@/shared/ui/Spinner'
import { useUsers } from './useUsers'

export function UserListPage() {
  const { data: users, isPending, isError, error, refetch, isFetching } = useUsers()

  return (
    <section>
      <header className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-ink">Users</h1>
          <p className="mt-1 text-sm text-ink-muted">Everyone in the directory, newest first.</p>
        </div>
        <Link
          to="/add"
          className="inline-flex items-center justify-center rounded-md bg-brand px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-strong focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand"
        >
          Add user
        </Link>
      </header>

      {isPending && <Spinner label="Loading users" />}

      {isError && <ErrorState error={error} onRetry={() => void refetch()} />}

      {!isPending && !isError && (users.length === 0 ? <EmptyState /> : <UserTable users={users} />)}

      {!isPending && isFetching && (
        <p className="mt-3 text-xs text-ink-muted" role="status">
          Refreshing…
        </p>
      )}
    </section>
  )
}

function ErrorState({ error, onRetry }: { error: Error; onRetry: () => void }) {
  const message =
    error instanceof ApiError
      ? error.message
      : 'Could not reach the server. Check that the API is running.'

  return (
    <div
      role="alert"
      className="rounded-lg border border-danger/30 bg-danger/5 p-6 text-center"
    >
      <p className="font-medium text-danger">Could not load users</p>
      <p className="mt-1 text-sm text-ink-muted">{message}</p>
      <Button variant="secondary" className="mt-4" onClick={onRetry}>
        Try again
      </Button>
    </div>
  )
}

function EmptyState() {
  return (
    <div className="rounded-lg border border-dashed border-line bg-surface p-10 text-center">
      <p className="font-medium text-ink">No users yet</p>
      <p className="mt-1 text-sm text-ink-muted">Add the first one to get started.</p>
      <Link
        to="/add"
        className="mt-4 inline-flex items-center justify-center rounded-md bg-brand px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-strong focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand"
      >
        Add user
      </Link>
    </div>
  )
}

function UserTable({ users }: { users: UserDto[] }) {
  return (
    <div className="overflow-x-auto rounded-lg border border-line bg-surface">
      <table className="w-full min-w-[36rem] border-collapse text-left text-sm">
        <caption className="sr-only">Users in the directory</caption>
        <thead>
          <tr className="border-b border-line bg-surface-muted">
            <th scope="col" className="px-4 py-3 font-medium text-ink-muted">
              Name
            </th>
            <th scope="col" className="px-4 py-3 font-medium text-ink-muted">
              Age
            </th>
            <th scope="col" className="px-4 py-3 font-medium text-ink-muted">
              City
            </th>
            <th scope="col" className="px-4 py-3 font-medium text-ink-muted">
              State
            </th>
            <th scope="col" className="px-4 py-3 font-medium text-ink-muted">
              Pincode
            </th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id} className="border-b border-line last:border-b-0">
              <td className="px-4 py-3 font-medium text-ink">{user.name}</td>
              <td className="px-4 py-3 tabular-nums text-ink">{user.age}</td>
              <td className="px-4 py-3 text-ink">{user.city}</td>
              <td className="px-4 py-3 text-ink">{user.state}</td>
              <td className="px-4 py-3 tabular-nums text-ink">{user.pincode}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
