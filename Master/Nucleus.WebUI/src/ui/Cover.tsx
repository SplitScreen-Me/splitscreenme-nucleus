import { useState, type ReactNode } from 'react';
import { cn } from './cn';

export interface CoverProps {
  /** Image URL. When missing or broken, a striped blank with the game name stands in. */
  src?: string | null;
  /** Second choice when src fails (e.g. Steam capsule → IGDB cover). */
  fallbackSrc?: string | null;
  /** Game name for the blank cover. */
  title: string;
  ratio?: '3/4' | '2/3' | '16/9';
  eager?: boolean;
  className?: string;
  imgClassName?: string;
  children?: ReactNode;
}

const RATIO = { '3/4': 'aspect-[3/4]', '2/3': 'aspect-[2/3]', '16/9': 'aspect-video' };

/** Game cover art. Decorative: the game name is always written next to it. */
export function Cover({ src, fallbackSrc, title, ratio = '3/4', eager, className, imgClassName, children }: CoverProps) {
  const [stage, setStage] = useState(0);
  const url = stage === 0 ? src || fallbackSrc : stage === 1 && src && fallbackSrc ? fallbackSrc : null;
  return (
    <div className={cn('relative overflow-hidden rounded-cover bg-pane-2', RATIO[ratio], className)}>
      {url ? (
        <img
          src={url}
          alt=""
          loading={eager ? 'eager' : 'lazy'}
          decoding="async"
          draggable={false}
          onError={() => setStage((s) => s + 1)}
          className={cn('absolute inset-0 size-full object-cover', imgClassName)}
        />
      ) : (
        <span
          aria-hidden="true"
          className="absolute inset-0 grid place-items-center p-4 text-center text-[1.05rem] leading-[1.05] font-[800] text-ink-2 stretch-wide"
          style={{
            background:
              'repeating-linear-gradient(135deg, var(--color-pane-2) 0 10px, var(--color-pane-3) 10px 11px)',
          }}
        >
          {title}
        </span>
      )}
      {children}
    </div>
  );
}
