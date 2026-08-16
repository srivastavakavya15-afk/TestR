import { createContext, useContext } from 'react'

export type ToastTone = 'success' | 'error'

export interface Toast {
  id: number
  tone: ToastTone
  message: string
}

export interface ToastApi {
  showToast: (message: string, tone?: ToastTone) => void
}

export const ToastContext = createContext<ToastApi>({ showToast: () => {} })

export function useToast(): ToastApi {
  return useContext(ToastContext)
}
