import { createContext, useContext, type ReactNode } from 'react';
import { cn } from './cn';

export type PadButton = 'A' | 'B' | 'X' | 'Y' | 'LB' | 'RB' | 'Menu' | 'View';

export type GlyphStyle = 'xbox' | 'playstation';

const LABEL: Record<GlyphStyle, Record<PadButton, string>> = {
  xbox: { A: 'A', B: 'B', X: 'X', Y: 'Y', LB: 'LB', RB: 'RB', Menu: '☰', View: '⧉' },
  playstation: { A: '✕', B: '○', X: '□', Y: '△', LB: 'L1', RB: 'R1', Menu: '☰', View: '⧉' },
};
const SPOKEN: Record<GlyphStyle, Record<PadButton, string>> = {
  xbox: { A: 'A button', B: 'B button', X: 'X button', Y: 'Y button', LB: 'Left bumper', RB: 'Right bumper', Menu: 'Menu button', View: 'View button' },
  playstation: { A: 'Cross button', B: 'Circle button', X: 'Square button', Y: 'Triangle button', LB: 'L1', RB: 'R1', Menu: 'Options button', View: 'Create button' },
};

const GlyphStyleContext = createContext<GlyphStyle>('xbox');
/** Which controller family the hints are drawn for (Settings > Interface). */
export const GlyphStyleProvider = GlyphStyleContext.Provider;

/** A gamepad face button. A is white: it is the press-to-confirm button everywhere. */
export function GamepadGlyph({
  button,
  className,
  size = 'md',
  inverse = false,
  decorative = false,
}: {
  button: PadButton;
  className?: string;
  size?: 'sm' | 'md';
  /** Dark glyph, for use on a white (primary) button. */
  inverse?: boolean;
  /** Hidden from assistive tech (inside a labelled button). */
  decorative?: boolean;
}) {
  const style = useContext(GlyphStyleContext);
  const wide = LABEL[style][button].length > 1;
  return (
    <span
      role={decorative ? undefined : 'img'}
      aria-label={decorative ? undefined : SPOKEN[style][button]}
      aria-hidden={decorative || undefined}
      className={cn(
        'inline-grid shrink-0 place-items-center leading-none font-[800] stretch-normal',
        size === 'sm' ? 'h-5 min-w-5 text-10' : 'h-[22px] min-w-[22px] text-11',
        wide ? 'rounded-md px-[7px]' : 'rounded-full',
        inverse ? 'bg-on-light text-ink' : button === 'A' ? 'bg-ink text-on-light' : 'bg-pane-3 text-ink',
        className,
      )}
    >
      {LABEL[style][button]}
    </span>
  );
}

/** A keyboard key. */
export function Kbd({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <kbd
      className={cn(
        'inline-grid h-[22px] min-w-6 shrink-0 place-items-center rounded-[5px] bg-pane-3 px-1.5',
        'font-sans text-12 font-[650] leading-none text-ink shadow-[inset_0_-1px_0_var(--color-line-strong)]',
        className,
      )}
    >
      {children}
    </kbd>
  );
}
