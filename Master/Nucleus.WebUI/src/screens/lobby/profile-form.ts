// Profile settings form: the draft and its validation errors, without React. Erasable TypeScript with type-only
// imports, so src/bridge/__tests__/profile-form.test.ts runs it under `node --test`.
import type { ProfileSettings, ProfileSlot, ValidationError } from '../../bridge/generated/contract.ts';

/** Process priorities the host accepts for a player's copy of the game. */
export const PRIORITY_CLASSES = ['Normal', 'AboveNormal', 'High', 'RealTime'] as const;

/** Read-only parts of the settings: the choice lists and whether the profile is new. */
type Fixed = 'isNew' | 'networkChoices' | 'audioChoices' | 'mergerResolutionChoices' | 'splitDividerColorChoices';

/** A top-level field the form edits. */
export type ProfileField = Exclude<keyof ProfileSettings, Fixed | 'slots'>;

/** A field of a player slot the form edits. */
export type SlotField = Exclude<keyof ProfileSlot, 'steamIdLocked'>;

/** Key of a field in the error map: `title`, or `slots.steamId#2` for the Steam ID of the third player. */
export const fieldKey = (field: string, index?: number | null) => (index === undefined || index === null ? field : `${field}#${index}`);

/** Key of a slot field. */
export const slotKey = (field: SlotField, index: number) => fieldKey(`slots.${field}`, index);

/** Validation messages by field key; two messages for one field are joined. */
export function errorsByField(errors: readonly ValidationError[]): Record<string, string> {
  const byField: Record<string, string> = {};
  for (const e of errors) {
    const key = fieldKey(e.field, e.index);
    byField[key] = byField[key] ? `${byField[key]} ${e.message}` : e.message;
  }
  return byField;
}

/** The messages no field of the form shows (for example a wrong number of slots): they go above the form. */
export function generalErrors(errors: readonly ValidationError[], shown: (key: string) => boolean): string[] {
  return errors.filter((e) => !shown(fieldKey(e.field, e.index))).map((e) => e.message);
}

/** The draft with one field changed. */
export function withField<K extends ProfileField>(draft: ProfileSettings, field: K, value: ProfileSettings[K]): ProfileSettings {
  return { ...draft, [field]: value };
}

/** The draft with fields of one player slot changed. */
export function withSlot(draft: ProfileSettings, index: number, patch: Partial<Pick<ProfileSlot, SlotField>>): ProfileSettings {
  return { ...draft, slots: draft.slots.map((slot, i) => (i === index ? { ...slot, ...patch } : slot)) };
}

/** The errors left once a field is edited: its own message goes, the host checks it again on save. */
export function withoutError(errors: readonly ValidationError[], key: string): ValidationError[] {
  return errors.filter((e) => fieldKey(e.field, e.index) !== key);
}

/** Whether the draft differs from the saved settings (the choice lists don't count). */
export function isDirty(saved: ProfileSettings | undefined, draft: ProfileSettings | null): boolean {
  if (!saved || !draft) return false;
  const editable = ({ isNew: _n, networkChoices: _a, audioChoices: _b, mergerResolutionChoices: _c, splitDividerColorChoices: _d, ...rest }: ProfileSettings) =>
    JSON.stringify(rest);
  return editable(saved) !== editable(draft);
}

/** Slot fields edited in the "advanced" part of the form: it opens by itself when one of them is wrong. */
export const ADVANCED_SLOT_FIELDS: readonly SlotField[] = ['priorityClass', 'idealProcessor', 'affinity'];

/** Whether an error concerns an advanced slot field. */
export const isAdvancedError = (e: ValidationError) => ADVANCED_SLOT_FIELDS.some((f) => e.field === `slots.${f}`);

/** A whole number typed in a field, or undefined while it isn't one. */
export function parseWhole(text: string): number | undefined {
  const trimmed = text.trim();
  return /^-?\d+$/.test(trimmed) ? Number(trimmed) : undefined;
}
