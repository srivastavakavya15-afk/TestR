import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import { ApiError } from '@/api/client'
import { AuthProvider } from '@/shared/auth/AuthProvider'
import { ToastProvider } from '@/shared/ui/toast'
import { NavBar } from './NavBar'
import { AppRoutes } from './routes'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,

      retry: (failureCount, error) =>
        error instanceof ApiError && error.status < 500 ? false : failureCount < 2,
    },
  },
})

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ToastProvider>
          <BrowserRouter>
            <div className="flex min-h-full flex-col">
              <NavBar />
              <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
                <AppRoutes />
              </main>
            </div>
          </BrowserRouter>
        </ToastProvider>
      </AuthProvider>
    </QueryClientProvider>
  )
}
