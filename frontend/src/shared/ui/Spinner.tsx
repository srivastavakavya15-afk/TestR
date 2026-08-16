interface SpinnerProps {
  label?: string
}

export function Spinner({ label = 'Loading' }: SpinnerProps) {
  return (
    <div className="flex items-center justify-center gap-3 py-12" role="status">
      <span
        className="size-5 animate-spin rounded-full border-2 border-line border-t-brand"
        aria-hidden="true"
      />
      <span className="text-sm text-ink-muted">{label}…</span>
    </div>
  )
}
