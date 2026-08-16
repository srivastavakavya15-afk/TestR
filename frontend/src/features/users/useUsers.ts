import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi, type CreateUserRequest, type UserDto } from '@/api/client'

export const usersKeys = {
  all: ['users'] as const,
  detail: (id: string) => ['users', id] as const,
}

export function useUsers() {
  return useQuery({
    queryKey: usersKeys.all,
    queryFn: ({ signal }) => usersApi.list({ signal }),
  })
}

export function useUser(id: string) {
  return useQuery({
    queryKey: usersKeys.detail(id),
    queryFn: ({ signal }) => usersApi.getById(id, { signal }),
  })
}

export function useCreateUser() {
  const queryClient = useQueryClient()

  return useMutation<UserDto, Error, CreateUserRequest>({
    mutationFn: (input) => usersApi.create(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKeys.all }),
  })
}

export function useDeleteUser() {
  const queryClient = useQueryClient()

  return useMutation<void, Error, string>({
    mutationFn: (id) => usersApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKeys.all }),
  })
}
