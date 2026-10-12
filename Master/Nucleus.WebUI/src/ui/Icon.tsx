import type { SVGProps } from 'react';
import { ICONS, type IconDef, type IconName } from './icons';
import { cn } from './cn';

export type { IconName } from './icons';

export interface IconProps extends Omit<SVGProps<SVGSVGElement>, 'name'> {
  name: IconName;
  /** Accessible name. Without it the icon is decorative (aria-hidden). */
  label?: string;
}

/** Inline SVG icon from the Bezel sprite. Size with Tailwind (`size-4`), colour with `text-*`. */
export function Icon({ name, label, className, strokeWidth, ...rest }: IconProps) {
  const def: IconDef = ICONS[name];
  return (
    <svg
      viewBox="0 0 24 24"
      className={cn('size-5 shrink-0', className)}
      fill={def.filled ? 'currentColor' : 'none'}
      stroke={def.filled ? 'none' : 'currentColor'}
      strokeWidth={def.filled ? undefined : (strokeWidth ?? 1.75)}
      strokeLinecap="round"
      strokeLinejoin="round"
      role={label ? 'img' : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
      focusable="false"
      {...rest}
    >
      <path d={def.d} />
    </svg>
  );
}
