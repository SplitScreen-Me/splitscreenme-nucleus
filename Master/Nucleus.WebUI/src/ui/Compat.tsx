import { useI18n, type MessageKey } from '@/i18n';
import { cn } from './cn';
import { Icon, type IconName } from './Icon';
import { playerClass } from './player';

/** Player pips: one light per seat, in join order. The one place a catalogue card uses colour. */
export function Pips({ players, size = 'md', className }: { players?: number | null; size?: 'md' | 'lg'; className?: string }) {
  const { t, format } = useI18n();
  if (!players || players < 1) {
    return <span className={cn('text-small text-ink-3', className)}>{t('ui.compat.playersUnknown')}</span>;
  }
  const shown = Math.min(players, 8);
  return (
    <span role="img" aria-label={t('ui.compat.maxPlayers', { count: players })} className={cn('inline-flex items-center gap-0.5', className)}>
      {Array.from({ length: shown }, (_, i) => (
        <i
          key={i}
          className={cn(playerClass(i + 1), 'block rounded-[2px] bg-pc', size === 'lg' ? 'h-4 w-1.5' : 'h-3 w-[5px]')}
        />
      ))}
      <b className={cn('data ml-1.5 leading-none font-bold text-ink', size === 'lg' ? 'text-16' : 'text-small')}>{format.number(players)}</b>
    </span>
  );
}

export interface InputSupportFlags {
  controllers?: boolean;
  keyboardMouse?: boolean;
  multiKeyboardMouse?: boolean;
}

/** The input kinds, in display order. `label` and `inList` are keys: `t(kind.label)`. */
export const INPUT_KINDS: ReadonlyArray<{ key: keyof InputSupportFlags; icon: IconName; label: MessageKey; inList: MessageKey }> = [
  { key: 'controllers', icon: 'pad', label: 'common.inputs.controllers', inList: 'ui.compat.inputsInList.controllers' },
  { key: 'keyboardMouse', icon: 'kbm', label: 'common.inputs.keyboardMouse', inList: 'ui.compat.inputsInList.keyboardMouse' },
  { key: 'multiKeyboardMouse', icon: 'multi', label: 'common.inputs.multiKeyboardMouse', inList: 'ui.compat.inputsInList.multiKeyboardMouse' },
];

/** Supported inputs as three icons, dimmed when missing. */
export function InputSupport({ flags, size = 'md', className }: { flags: InputSupportFlags; size?: 'sm' | 'md' | 'lg'; className?: string }) {
  const { t, format } = useI18n();
  const on = INPUT_KINDS.filter((k) => flags[k.key]).map((k) => t(k.inList));
  const label = on.length ? t('ui.compat.playsWith', { inputs: format.list(on, 'unit') }) : t('common.inputs.unknown');
  const iconSize = size === 'sm' ? 'size-4' : size === 'lg' ? 'size-[22px]' : 'size-[18px]';
  return (
    <span role="img" aria-label={label} className={cn('inline-flex text-ink-2', size === 'sm' ? 'gap-1' : 'gap-1.5', className)}>
      {INPUT_KINDS.map((k) => (
        <span key={k.key} title={flags[k.key] ? t(k.label) : t('ui.compat.notSupported', { input: t(k.label) })}>
          <Icon name={k.icon} className={cn(iconSize, !flags[k.key] && 'opacity-25')} />
        </span>
      ))}
    </span>
  );
}

/** The inputs a handler supports, one name per combination: for texts written whole for each combination. */
export type InputCombo =
  | 'none'
  | 'controllers'
  | 'keyboardMouse'
  | 'multiKeyboardMouse'
  | 'controllersKeyboardMouse'
  | 'controllersMultiKeyboardMouse'
  | 'keyboardMouseMultiKeyboardMouse'
  | 'all';

const COMBOS: Readonly<Record<string, InputCombo>> = {
  '000': 'none',
  '100': 'controllers',
  '010': 'keyboardMouse',
  '001': 'multiKeyboardMouse',
  '110': 'controllersKeyboardMouse',
  '101': 'controllersMultiKeyboardMouse',
  '011': 'keyboardMouseMultiKeyboardMouse',
  '111': 'all',
};

/** Which combination of inputs (controllers, keyboard and mouse, several keyboards and mice) `flags` describes. */
export function inputCombo(flags: InputSupportFlags): InputCombo {
  return COMBOS[[flags.controllers, flags.keyboardMouse, flags.multiKeyboardMouse].map((on) => (on ? '1' : '0')).join('')] ?? 'none';
}
