import { describe, expect, it, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { server } from '@/test/server'
import { renderWithProviders } from '@/test/renderWithProviders'
import { UserAddPage } from './UserAddPage'

const navigate = vi.fn()

vi.mock('react-router-dom', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react-router-dom')>()),
  useNavigate: () => navigate,
}))

async function fillValidForm(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText('Name'), 'Ada Lovelace')
  await user.type(screen.getByLabelText('Age'), '36')
  await user.type(screen.getByLabelText('City'), 'London')
  await user.type(screen.getByLabelText('State'), 'Greater London')
  await user.type(screen.getByLabelText('Pincode'), 'WC1E')
}

describe('UserAddPage', () => {
  it('blocks submission and shows inline messages when fields are invalid', async () => {
    const user = userEvent.setup()
    const createRequests: unknown[] = []
    server.use(
      http.post('/api/users', async ({ request }) => {
        createRequests.push(await request.json())
        return HttpResponse.json({}, { status: 201 })
      }),
    )

    renderWithProviders(<UserAddPage />)

    await user.type(screen.getByLabelText('Name'), 'A')
    await user.type(screen.getByLabelText('Age'), '999')
    await user.click(screen.getByRole('button', { name: 'Save user' }))

    expect(
      await screen.findByText('Name must be between 2 and 100 characters.'),
    ).toBeInTheDocument()
    expect(screen.getByText('Age must be between 0 and 120.')).toBeInTheDocument()
    expect(screen.getByText('City is required.')).toBeInTheDocument()
    expect(screen.getByText('State is required.')).toBeInTheDocument()
    expect(screen.getByText('Pincode must be between 4 and 10 characters.')).toBeInTheDocument()

    expect(createRequests).toHaveLength(0)
    expect(navigate).not.toHaveBeenCalled()
  })

  it('posts the form, shows a success toast, and redirects to the list', async () => {
    const user = userEvent.setup()
    let received: unknown
    server.use(
      http.post('/api/users', async ({ request }) => {
        received = await request.json()
        return HttpResponse.json(
          {
            id: '019ffae0-4cfe-743d-a595-383d3fe88f1a',
            name: 'Ada Lovelace',
            age: 36,
            city: 'London',
            state: 'Greater London',
            pincode: 'WC1E',
            createdAtUtc: '2026-08-13T11:27:11.615Z',
          },
          { status: 201 },
        )
      }),
    )

    renderWithProviders(<UserAddPage />)
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: 'Save user' }))

    expect(await screen.findByText('User added.')).toBeInTheDocument()
    await waitFor(() => expect(navigate).toHaveBeenCalledWith('/', { replace: true }))

    expect(received).toEqual({
      name: 'Ada Lovelace',
      age: 36,
      city: 'London',
      state: 'Greater London',
      pincode: 'WC1E',
    })
  })

  it('maps server-side field errors onto the matching inputs', async () => {
    const user = userEvent.setup()
    server.use(
      http.post('/api/users', () =>
        HttpResponse.json(
          {
            title: 'One or more validation errors occurred.',
            status: 400,
            errors: { pincode: ['Pincode is not valid for that state.'] },
          },
          { status: 400 },
        ),
      ),
    )

    renderWithProviders(<UserAddPage />)
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: 'Save user' }))

    expect(await screen.findByText('Pincode is not valid for that state.')).toBeInTheDocument()
    expect(screen.getByLabelText('Pincode')).toHaveAttribute('aria-invalid', 'true')
    expect(navigate).not.toHaveBeenCalled()
  })

  it('reports a transport failure without losing what the user typed', async () => {
    const user = userEvent.setup()
    server.use(http.post('/api/users', () => HttpResponse.error()))

    renderWithProviders(<UserAddPage />)
    await fillValidForm(user)
    await user.click(screen.getByRole('button', { name: 'Save user' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Could not reach the server. Please try again.',
    )
    expect(screen.getByLabelText('Name')).toHaveValue('Ada Lovelace')
    expect(navigate).not.toHaveBeenCalled()
  })
})
