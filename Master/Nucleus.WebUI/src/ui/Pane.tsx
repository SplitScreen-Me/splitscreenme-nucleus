import type { HTMLAttributes, Ref } from 'react';
import { cn } from './cn';

type PaneTag = 'div' | 'section' | 'aside' | 'article' | 'header' | 'nav' | 'details';

export interface PaneProps extends HTMLAttributes<HTMLElement> {
  as?: PaneTag;
  /** Secondary surface (one step lighter). */
  tone?: 'base' | 'raised';
  ref?: Ref<HTMLElement>;
  open?: boolean;
}

/** The core surface: a lit pane on the black bezel. Panes are screens: 14px radius, no border. */
export function Pane({ as: Tag = 'div', tone = 'base', className, ...rest }: PaneProps) {
  const Comp = Tag as 'div';
  return (
    <Comp
      className={cn('relative overflow-hidden rounded-pane', tone === 'raised' ? 'bg-pane-2' : 'bg-pane', className)}
      {...(rest as HTMLAttributes<HTMLDivElement>)}
    />
  );
}
