import { cn } from './cn';
import { Icon, type IconName } from './Icon';
import { playerClass } from './player';

/** Player pips: one light per seat, in join order. The one place a catalogue card uses colour. */
export function Pips({ players, size = 'md', className }: { players?: number | null; size?: 'md' | 'lg'; className?: string }) {
  if (!players || players < 1) {
    return <span className={cn('text-small text-ink-3', className)}>Players unknown</span>;
  }
  const shown = Math.min(players, 8);
  return (
    <span role="img" aria-label={`Up to ${players} players`} className={cn('inline-flex items-center gap-0.5', className)}>
      {Array.from({ length: shown }, (_, i) => (
        <i
          key={i}
          className={cn(playerClass(i + 1), 'block rounded-[2px] bg-pc', size === 'lg' ? 'h-4 w-1.5' : 'h-3 w-[5px]')}
        />
      ))}
      <b className={cn('data ml-1.5 leading-none font-bold text-ink', size === 'lg' ? 'text-16' : 'text-small')}>{players}</b>
    </span>
  );
}

export interface InputSupportFlags {
  controllers?: boolean;
  keyboardMouse?: boolean;
  multiKeyboardMouse?: boolean;
}

export const INPUT_KINDS: ReadonlyArray<{ key: keyof InputSupportFlags; icon: IconName; label: string }> = [
  { key: 'controllers', icon: 'pad', label: 'Controllers' },
  { key: 'keyboardMouse', icon: 'kbm', label: 'Keyboard and mouse' },
  { key: 'multiKeyboardMouse', icon: 'multi', label: 'Several keyboards and mice' },
];

/** Supported inputs as three icons, dimmed when missing. */
export function InputSupport({ flags, size = 'md', className }: { flags: InputSupportFlags; size?: 'sm' | 'md' | 'lg'; className?: string }) {
  const on = INPUT_KINDS.filter((k) => flags[k.key]).map((k) => k.label.toLowerCase());
  const label = on.length ? `Plays with: ${on.join(', ')}` : 'Input support unknown';
  const iconSize = size === 'sm' ? 'size-4' : size === 'lg' ? 'size-[22px]' : 'size-[18px]';
  return (
    <span role="img" aria-label={label} className={cn('inline-flex text-ink-2', size === 'sm' ? 'gap-1' : 'gap-1.5', className)}>
      {INPUT_KINDS.map((k) => (
        <span key={k.key} title={`${k.label}${flags[k.key] ? '' : ' (not supported)'}`}>
          <Icon name={k.icon} className={cn(iconSize, !flags[k.key] && 'opacity-25')} />
        </span>
      ))}
    </span>
  );
}

/** "with controllers, keyboard and mouse or several keyboards and mice" */
export function inputSentence(flags: InputSupportFlags): string {
  const parts = [
    flags.controllers && 'controllers',
    flags.keyboardMouse && 'keyboard and mouse',
    flags.multiKeyboardMouse && 'several keyboards and mice',
  ].filter((p): p is string => Boolean(p));
  if (!parts.length) return '';
  if (parts.length === 1) return `with ${parts[0]}`;
  return `with ${parts.slice(0, -1).join(', ')} or ${parts[parts.length - 1]}`;
}
