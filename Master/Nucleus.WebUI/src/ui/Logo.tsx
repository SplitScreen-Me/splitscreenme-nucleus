import { cn } from './cn';

const MARK = {
  mono: ['currentColor', 'currentColor', 'currentColor'],
  color: ['var(--color-p1)', 'var(--color-p2)', 'var(--color-p3)'],
} as const;

/**
 * The mark: a screen split for two, and one more player about to join.
 * Top pane, bottom-left pane, and a dot where the next screen will open.
 */
export function LogoMark({ variant = 'mono', className }: { variant?: keyof typeof MARK; className?: string }) {
  const [a, b, c] = MARK[variant];
  return (
    <svg viewBox="0 0 32 32" className={cn('size-7 shrink-0', className)} aria-hidden="true" focusable="false">
      <rect x="2" y="3" width="28" height="12.5" rx="3.4" fill={a} />
      <rect x="2" y="17.5" width="13" height="11.5" rx="3.4" fill={b} />
      <circle cx="23.5" cy="23.25" r="5.75" fill={c} />
    </svg>
  );
}

/** Mark + wordmark, as in the title bar. */
export function Logo({ className, variant = 'mono' }: { className?: string; variant?: keyof typeof MARK }) {
  return (
    <span className={cn('inline-flex items-center gap-2.5 text-ink', className)}>
      <LogoMark variant={variant} className="size-[22px]" />
      <span className="stretch-wide text-15 font-[800] tracking-[-0.02em] whitespace-nowrap">Nucleus Co-op</span>
    </span>
  );
}
