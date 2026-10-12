import type { ButtonHTMLAttributes, ReactNode, Ref } from 'react';
import { cn } from './cn';

export interface ChipProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  pressed?: boolean;
  count?: ReactNode;
  ref?: Ref<HTMLButtonElement>;
}

/** Filter / toggle pill. Pressed chips turn white: the selected state is the brightest. */
export function Chip({ pressed = false, count, className, children, ...rest }: ChipProps) {
  return (
    <button
      type="button"
      aria-pressed={pressed}
      className={cn(
        'inline-flex h-[34px] items-center gap-2 rounded-tag border-0 px-3.5 text-14 font-[560] whitespace-nowrap',
        'transition-colors duration-(--dur-1) [&_svg]:size-4',
        pressed ? 'bg-ink text-on-light' : 'bg-pane-2 text-ink-2 hover:bg-pane-3 hover:text-ink',
        className,
      )}
      {...rest}
    >
      {children}
      {count !== undefined && count !== null && count !== '' && <span className="data opacity-60">{count}</span>}
    </button>
  );
}
