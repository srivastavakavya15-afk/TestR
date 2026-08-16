import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '@/api/client'
import { Button } from '@/shared/ui/Button'
import { TextField } from '@/shared/ui/TextField'
import { useToast } from '@/shared/ui/toastContext'
import { useCreateUser } from './useUsers'
import { userFormSchema, type UserFormInput, type UserFormValues } from './userFormSchema'

const FIELD_NAMES = ['name', 'age', 'city', 'state', 'pincode'] as const
type FieldName = (typeof FIELD_NAMES)[number]

function isFieldName(value: string): value is FieldName {
  return (FIELD_NAMES as readonly string[]).includes(value)
}

export function UserAddPage() {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const createUser = useCreateUser()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<UserFormInput, unknown, UserFormValues>({
    resolver: zodResolver(userFormSchema),
    defaultValues: { name: '', age: '', city: '', state: '', pincode: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    try {
      await createUser.mutateAsync(values)
      showToast('User added.')
      navigate('/', { replace: true })
    } catch (error) {

      if (error instanceof ApiError && error.fieldErrors) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          const message = messages[0]
          if (message && isFieldName(field)) {
            setError(field, { type: 'server', message })
          }
        }
        if (!Object.keys(error.fieldErrors).some(isFieldName)) {
          setError('root', { message: error.message })
        }
        return
      }

      setError('root', {
        message:
          error instanceof ApiError
            ? error.message
            : 'Could not reach the server. Please try again.',
      })
    }
  })

  return (
    <section className="mx-auto max-w-xl">
      <header className="mb-6">
        <h1 className="text-2xl font-semibold text-ink">Add user</h1>
        <p className="mt-1 text-sm text-ink-muted">All fields are required.</p>
      </header>

      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4 rounded-lg border border-line bg-surface p-6">
        {errors.root?.message && (
          <p role="alert" className="rounded-md border border-danger/30 bg-danger/5 px-3 py-2 text-sm font-medium text-danger">
            {errors.root.message}
          </p>
        )}

        <TextField
          label="Name"
          autoComplete="name"
          placeholder="Ada Lovelace"
          error={errors.name?.message}
          {...register('name')}
        />

        <TextField
          label="Age"
          type="number"
          inputMode="numeric"
          min={0}
          max={120}
          placeholder="36"
          error={errors.age?.message}
          {...register('age')}
        />

        <TextField
          label="City"
          autoComplete="address-level2"
          placeholder="London"
          error={errors.city?.message}
          {...register('city')}
        />

        <TextField
          label="State"
          autoComplete="address-level1"
          placeholder="Greater London"
          error={errors.state?.message}
          {...register('state')}
        />

        <TextField
          label="Pincode"
          autoComplete="postal-code"
          placeholder="560001"
          hint="4–10 characters."
          error={errors.pincode?.message}
          {...register('pincode')}
        />

        <div className="mt-2 flex items-center gap-3">
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving…' : 'Save user'}
          </Button>
          <Button variant="secondary" onClick={() => navigate('/')} disabled={isSubmitting}>
            Cancel
          </Button>
        </div>
      </form>
    </section>
  )
}
