// Settings view-model: conditions, saving one value at a time, key captures. Values travel as strings.
import { useCallback, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, type SettingChoice, type SettingCondition, type SettingField } from '@/bridge';
import { useRun } from '@/lib/bridge-ui';
import type { IconName } from '@/ui';

/** Whether a condition holds for the current values. No condition always holds. */
export const holds = (condition: SettingCondition | undefined, values: Record<string, string>) =>
  !condition || condition.values.includes(values[condition.key] ?? '');

/** Icon of a section, by its id. */
export function sectionIcon(id: string): IconName {
  const icons: Record<string, IconName> = {
    general: 'sliders',
    players: 'users',
    hotkeys: 'keyboard',
    gamepad: 'pad',
    audio: 'speaker',
    layout: 'layers',
    advanced: 'terminal',
    about: 'info',
  };
  return icons[id] ?? 'sliders';
}

/** "1024+32768" → ["Guide", "Y"], with the labels of the schema (unknown codes stay as they are). */
export function comboLabels(value: string, buttons: readonly SettingChoice[]): string[] {
  if (!value) return [];
  return value.split('+').map((code) => buttons.find((b) => b.value === code.trim())?.label ?? code);
}

export interface SettingsSaver {
  /** Saves one value. Resolves true when saved; false when refused (the error is in `errors`) or failed (toasted). */
  save: (field: SettingField, value: string) => Promise<boolean>;
  /** Validation messages, by setting key. */
  errors: Record<string, string>;
  /** Keys being saved. */
  saving: ReadonlySet<string>;
  clearError: (key: string) => void;
}

/**
 * Saves settings one value at a time with `settings.setValues`. The host validates everything and saves nothing when
 * a value is invalid: its messages are shown under the field. Saved values update the cache at once; the
 * `settings.changed` event then refetches them.
 */
export function useSettingsSaver(knownKeys: ReadonlySet<string>): SettingsSaver {
  const run = useRun();
  const queryClient = useQueryClient();
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [saving, setSaving] = useState<ReadonlySet<string>>(new Set());

  const clearError = useCallback((key: string) => setErrors(({ [key]: _gone, ...rest }) => rest), []);

  const save = useCallback(
    async (field: SettingField, value: string) => {
      const key = field.key;
      setSaving((s) => new Set(s).add(key));
      const problems = await run(() => bridge.settings.setValues({ values: { [key]: value } }));
      setSaving((s) => {
        const next = new Set(s);
        next.delete(key);
        return next;
      });
      if (problems === undefined) return false;
      if (problems.length > 0) {
        setErrors((old) => {
          const next = { ...old };
          for (const p of problems) next[knownKeys.has(p.field) ? p.field : key] = p.message;
          return next;
        });
        return false;
      }
      clearError(key);
      queryClient.setQueryData<Record<string, string>>(bridgeQueryKey('settings.getValues'), (old) => (old ? { ...old, [key]: value } : old));
      return true;
    },
    [run, queryClient, clearError, knownKeys],
  );

  return { save, errors, saving, clearError };
}

const MODIFIERS = new Set(['Control', 'Alt', 'Shift', 'Meta', 'AltGraph', 'OS']);
const NAMED: Record<string, string> = {
  ' ': 'Space',
  Spacebar: 'Space',
  Del: 'Delete',
  Esc: 'Escape',
  ArrowUp: 'Up',
  ArrowDown: 'Down',
  ArrowLeft: 'Left',
  ArrowRight: 'Right',
};

/** Modifiers held during a key event, in the stored order. */
export function modifiersOf(e: Pick<KeyboardEvent, 'ctrlKey' | 'altKey' | 'shiftKey'>): string[] {
  return [e.ctrlKey && 'Ctrl', e.altKey && 'Alt', e.shiftKey && 'Shift'].filter((m): m is string => Boolean(m));
}

/**
 * The shortcut a key event stands for, as Settings.ini stores it ("Ctrl+Alt+K"), or null while only modifiers are
 * held. Letters follow the keyboard layout (like Windows virtual keys); digits and F keys are taken from the key position.
 */
export function hotkeyOf(e: Pick<KeyboardEvent, 'key' | 'code' | 'ctrlKey' | 'altKey' | 'shiftKey'>): string | null {
  if (MODIFIERS.has(e.key)) return null;
  let key: string;
  if (/^Digit\d$/.test(e.code) || /^Numpad\d$/.test(e.code)) key = e.code.slice(-1);
  else if (/^F\d{1,2}$/.test(e.key)) key = e.key;
  else if (e.key.length === 1 && /[a-z]/i.test(e.key)) key = e.key.toUpperCase();
  else if (/^Key[A-Z]$/.test(e.code)) key = e.code.slice(3);
  else key = NAMED[e.key] ?? (e.key.length === 1 ? e.key.toUpperCase() : e.key);
  return [...modifiersOf(e), key].join('+');
}
