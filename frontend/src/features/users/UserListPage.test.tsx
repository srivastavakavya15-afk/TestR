import { describe, expect, it } from 'vitest'
import { screen, within } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { server } from '@/test/server'
import { renderWithProviders } from '@/test/renderWithProviders'
import { UserListPage } from './UserListPage'

const ada = {
  id: '019ffae0-4cfe-743d-a595-383d3fe88f1a',
  name: 'Ada Lovelace',
  age: 36,
  city: 'London',
  state: 'Greater London',
  pincode: 'WC1E',
  createdAtUtc: '2026-08-13T11:27:11.615Z',
}

describe('UserListPage', () => {
  it('renders each user returned by the API', async () => {
    server.use(http.get('/api/users', () => HttpResponse.json([ada])))

    renderWithProviders(<UserListPage />)

    const row = within(await screen.findByRole('row', { name: /Ada Lovelace/ }))
    expect(row.getByText('36')).toBeInTheDocument()
    expect(row.getByText('London')).toBeInTheDocument()
    expect(row.getByText('Greater London')).toBeInTheDocument()
    expect(row.getByText('WC1E')).toBeInTheDocument()
  })

  it('shows a spinner while the request is in flight', () => {
    server.use(http.get('/api/users', () => new Promise(() => {})))

    renderWithProviders(<UserListPage />)

    expect(screen.getByRole('status')).toHaveTextContent(/loading users/i)
  })

  it('shows an empty state when there are no users', async () => {
    server.use(http.get('/api/users', () => HttpResponse.json([])))

    renderWithProviders(<UserListPage />)

    expect(await screen.findByText('No users yet')).toBeInTheDocument()
    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('surfaces a server error instead of rendering an empty table', async () => {
    server.use(
      http.get('/api/users', () =>
        HttpResponse.json(
          { title: 'An unexpected error occurred.', status: 500, detail: 'Database unavailable.' },
          { status: 500 },
        ),
      ),
    )

    renderWithProviders(<UserListPage />)

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Could not load users')
    expect(alert).toHaveTextContent('Database unavailable.')
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument()
  })
})
