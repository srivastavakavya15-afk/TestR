import { z } from 'zod'

export const userFormSchema = z.object({
  name: z
    .string()
    .trim()
    .min(2, 'Name must be between 2 and 100 characters.')
    .max(100, 'Name must be between 2 and 100 characters.'),

  age: z.coerce
    .number({ message: 'Age is required.' })
    .int('Age must be a whole number.')
    .min(0, 'Age must be between 0 and 120.')
    .max(120, 'Age must be between 0 and 120.'),
  city: z.string().trim().min(1, 'City is required.').max(100, 'City must be at most 100 characters.'),
  state: z
    .string()
    .trim()
    .min(1, 'State is required.')
    .max(100, 'State must be at most 100 characters.'),
  pincode: z
    .string()
    .trim()
    .min(4, 'Pincode must be between 4 and 10 characters.')
    .max(10, 'Pincode must be between 4 and 10 characters.'),
})

export type UserFormValues = z.infer<typeof userFormSchema>

export type UserFormInput = z.input<typeof userFormSchema>
