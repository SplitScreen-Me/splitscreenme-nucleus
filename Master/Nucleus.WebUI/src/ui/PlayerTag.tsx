import type { HTMLAttributes } from 'react';
import { cn } from './cn';
import { playerClass } from './player';

export interface PlayerTagProps extends HTMLAttributes<HTMLSpanElement> {
  /** Player number, 1–8. */
  n: number;
  /** Outline when a slot is reserved, filled when it is taken. */
  outline?: boolean;
  size?: 'xs' | 'sm' | 'md' | 'lg';
}

const SIZE = {
  xs: 'h-[18px] min-w-6 px-[5px] text-11 rounded-[5px]',
  sm: 'h-5 min-w-[26px] px-1.5 text-12 rounded-[5px]',
  md: 'h-[22px] min-w-[30px] px-[7px] text-13 rounded-md',
  lg: 'h-6 min-w-[34px] px-2 text-14 rounded-md',
};

/** "P1"…"P8" in the player's colour. Colour means a player: this is where it lives. */
export function PlayerTag({ n, outline, size = 'md', className, ...rest }: PlayerTagProps) {
  return (
    <span
      className={cn(
        playerClass(n),
        'inline-flex shrink-0 items-center justify-center leading-none font-[800] stretch-narrow tabular-nums',
        outline ? 'bg-transparent text-pc shadow-[inset_0_0_0_1.5px_var(--pc)]' : 'bg-pc text-on-light',
        SIZE[size],
        className,
      )}
      {...rest}
    >
      P{n}
    </span>
  );
}

/** Player LED: the light a device shows when it belongs to someone. */
export function Led({ n, live, className }: { n?: number | null; live?: boolean; className?: string }) {
  return (
    <span
      aria-hidden="true"
      className={cn(
        n ? playerClass(n) : '',
        'inline-block size-2 shrink-0 rounded-full bg-pc',
        live && 'shadow-[0_0_0_3px_color-mix(in_oklab,var(--pc),transparent_70%)]',
        className,
      )}
    />
  );
}
