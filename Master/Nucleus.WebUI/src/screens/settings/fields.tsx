// One control per setting type, rendered from the schema. Every control saves through the saver: toggles, choices,
// captures and pickers at once, typed values when the field is left or Enter is pressed (Escape puts the value back).
import { useEffect, useId, useRef, useState, type KeyboardEvent, type ReactNode } from 'react';
import { bridge, type SettingChoice, type SettingField } from '@/bridge';
import { useRun } from '@/lib/bridge-ui';
import { Button, cn, Icon, Kbd, Select, Spinner, Switch } from '@/ui';
import { comboLabels, hotkeyOf, modifiersOf, type SettingsSaver } from './model';

export interface FieldProps {
  field: SettingField;
  value: string;
  disabled: boolean;
  saver: SettingsSaver;
  /** Id of the element naming the field (its label). */
  labelId: string;
  /** Id of the element describing it (help and error). */
  describedBy?: string;
  gamepadButtons: readonly SettingChoice[];
}

/** The control of one setting, by type. */
export function SettingControl(props: FieldProps) {
  switch (props.field.type) {
    case 'toggle':
      return <ToggleControl {...props} />;
    case 'choice':
      return <ChoiceControl {...props} />;
    case 'number':
    case 'text':
    case 'password':
      return <TypedControl {...props} />;
    case 'hotkey':
      return <HotkeyControl {...props} />;
    case 'gamepadCombo':
      return <ComboControl {...props} />;
    case 'filePath':
    case 'folderPath':
      return <PathControl {...props} />;
  }
}

function ToggleControl({ field, value, disabled, saver, labelId, describedBy }: FieldProps) {
  const [pending, setPending] = useState<boolean | null>(null);
  const checked = pending ?? value === 'true';
  return (
    <Switch
      checked={checked}
      disabled={disabled || saver.saving.has(field.key)}
      aria-labelledby={labelId}
      aria-describedby={describedBy}
      data-nav
      onChange={async (next) => {
        setPending(next);
        await saver.save(field, String(next));
        setPending(null);
      }}
      className="disabled:cursor-not-allowed disabled:opacity-50"
    />
  );
}

function ChoiceControl({ field, value, disabled, saver, labelId, describedBy }: FieldProps) {
  const known = field.choices.some((c) => c.value === value);
  const widest = Math.max(...field.choices.map((c) => c.label.length), value.length);
  return (
    <Select
      value={value}
      disabled={disabled || saver.saving.has(field.key)}
      aria-labelledby={labelId}
      aria-describedby={describedBy}
      data-nav
      onChange={(e) => void saver.save(field, e.target.value)}
      className={cn('max-w-[min(420px,40vw)] min-w-44 disabled:cursor-not-allowed disabled:opacity-50', widest > 30 && 'w-[min(420px,40vw)]')}
    >
      {!known && (
        <option value={value}>
          {value ? `${value} (not available)` : 'Not set'}
        </option>
      )}
      {field.choices.map((c) => (
        <option key={c.value} value={c.value}>
          {c.label}
        </option>
      ))}
    </Select>
  );
}

const inputFrame = 'flex h-10 items-center gap-2 rounded-ctl bg-pane-2 px-3';
const frameTone = (error: boolean) =>
  error
    ? 'shadow-[inset_0_0_0_1.5px_var(--color-danger)]'
    : 'shadow-[inset_0_0_0_1px_var(--color-line)] focus-within:shadow-[inset_0_0_0_1.5px_var(--color-ink)]';

/** Number, text and password fields: typed, saved when left or on Enter. */
function TypedControl({ field, value, disabled, saver, labelId, describedBy }: FieldProps) {
  const [draft, setDraft] = useState<string | null>(null);
  const [reveal, setReveal] = useState(false);
  const error = saver.errors[field.key];
  const shown = draft ?? value;

  const commit = async () => {
    if (draft === null) return;
    if (draft === value) {
      setDraft(null);
      saver.clearError(field.key);
      return;
    }
    if (await saver.save(field, draft)) setDraft(null);
  };
  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      void commit();
    } else if (e.key === 'Escape' && draft !== null) {
      e.preventDefault();
      e.stopPropagation();
      setDraft(null);
      saver.clearError(field.key);
    }
  };

  const number = field.type === 'number';
  const password = field.type === 'password';
  return (
    <div
      className={cn(
        inputFrame,
        frameTone(Boolean(error)),
        number ? 'w-[180px]' : 'w-[min(340px,34vw)]',
        disabled && 'opacity-50',
      )}
    >
      <input
        type={number ? 'number' : password && !reveal ? 'password' : 'text'}
        value={shown}
        min={field.min}
        max={field.max}
        step={field.step}
        maxLength={field.maxLength}
        disabled={disabled}
        spellCheck={false}
        autoComplete={password ? 'new-password' : 'off'}
        aria-labelledby={labelId}
        aria-describedby={describedBy}
        aria-invalid={error ? true : undefined}
        onChange={(e) => setDraft(e.target.value)}
        onBlur={() => void commit()}
        onKeyDown={onKeyDown}
        className={cn(
          'min-w-0 flex-1 border-0 bg-transparent text-15 text-ink outline-none placeholder:text-ink-3 disabled:cursor-not-allowed',
          number && 'data tabular-nums [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none',
        )}
      />
      {saver.saving.has(field.key) && <Spinner />}
      {field.unit && <span className="shrink-0 text-13 text-ink-3">{field.unit}</span>}
      {password && (
        <button
          type="button"
          aria-label={reveal ? 'Hide the password' : 'Show the password'}
          aria-pressed={reveal}
          disabled={disabled}
          onClick={() => setReveal((r) => !r)}
          className="-mr-1.5 grid size-8 place-items-center rounded-md border-0 bg-transparent text-ink-3 hover:bg-pane-3 hover:text-ink"
        >
          <Icon name={reveal ? 'eye' : 'lock'} className="size-4" />
        </button>
      )}
    </div>
  );
}

/** A keyboard shortcut: press the combination while the control listens. Escape cancels. */
function HotkeyControl({ field, value, disabled, saver, describedBy }: FieldProps) {
  const [listening, setListening] = useState(false);
  const [held, setHeld] = useState<string[]>([]);
  const stop = () => {
    setListening(false);
    setHeld([]);
  };
  return (
    <button
      type="button"
      data-own-arrows
      data-nav
      disabled={disabled || saver.saving.has(field.key)}
      aria-label={listening ? `${field.label}: press the shortcut, Escape cancels` : `${field.label}: ${value || 'not set'}. Change`}
      aria-describedby={describedBy}
      aria-pressed={listening}
      aria-live="polite"
      onClick={() => setListening((l) => !l)}
      onBlur={stop}
      onKeyDown={(e) => {
        if (!listening) return;
        if (e.key === 'Tab') {
          stop();
          return;
        }
        e.preventDefault();
        e.stopPropagation();
        if (e.key === 'Escape') {
          stop();
          return;
        }
        const combo = hotkeyOf(e.nativeEvent);
        if (!combo) {
          setHeld(modifiersOf(e.nativeEvent));
          return;
        }
        stop();
        if (combo !== value) void saver.save(field, combo);
      }}
      onKeyUp={(e) => listening && setHeld(modifiersOf(e.nativeEvent))}
      className={cn(
        'inline-flex h-10 min-w-[150px] items-center justify-between gap-3 rounded-ctl border-0 px-3 text-14 font-semibold',
        listening ? 'bg-ink text-on-light' : 'bg-pane-2 text-ink shadow-[inset_0_0_0_1px_var(--color-line)] hover:bg-pane-3',
        'disabled:cursor-not-allowed disabled:opacity-50',
      )}
    >
      {listening ? (
        <span>{held.length ? `${held.join(' + ')} + …` : 'Press the shortcut…'}</span>
      ) : value ? (
        <KeyCaps combo={value} />
      ) : (
        <span className="text-ink-3">Not set</span>
      )}
      <span className={cn('text-13 font-[560]', listening ? 'text-on-light/70' : 'text-ink-3')}>{listening ? 'Esc cancels' : 'Change'}</span>
    </button>
  );
}

/** "Ctrl+Alt+K" as key caps. */
export function KeyCaps({ combo }: { combo: string }) {
  return (
    <span className="inline-flex items-center gap-1">
      {combo.split('+').map((k, i) => (
        <Kbd key={`${k}-${i}`}>{k}</Kbd>
      ))}
    </span>
  );
}

/** Gamepad buttons as chips: face buttons round, the others as pills. */
export function ButtonChips({ labels, className }: { labels: string[]; className?: string }) {
  return (
    <span className={cn('inline-flex items-center gap-1.5', className)} aria-label={labels.join(' + ')} role="img">
      {labels.map((label, i) => (
        <span key={`${label}-${i}`} className="inline-flex items-center gap-1.5" aria-hidden="true">
          {i > 0 && <span className="text-12 text-ink-3">+</span>}
          <span
            className={cn(
              'inline-grid h-[22px] place-items-center bg-pane-3 text-11 leading-none font-[800] text-ink stretch-normal',
              label.length <= 2 ? 'min-w-[22px] rounded-full px-1' : 'rounded-md px-[7px]',
              label === 'A' && 'bg-ink text-on-light',
            )}
          >
            {label}
          </span>
        </span>
      ))}
    </span>
  );
}

/** A gamepad combination, captured by the host from a real controller (`settings.captureGamepadCombo`). */
function ComboControl({ field, value, disabled, saver, labelId, describedBy, gamepadButtons }: FieldProps) {
  const run = useRun();
  const [capturing, setCapturing] = useState<AbortController | null>(null);
  const count = field.comboButtons ?? 2;
  const live = useRef(capturing);
  live.current = capturing;
  useEffect(() => () => live.current?.abort(), []);

  const capture = async () => {
    const controller = new AbortController();
    setCapturing(controller);
    const combo = await run(() => bridge.settings.captureGamepadCombo({ buttons: count }, { timeoutMs: 0, signal: controller.signal }));
    setCapturing((c) => (c === controller ? null : c));
    if (combo && !controller.signal.aborted && combo !== value) await saver.save(field, combo);
  };

  if (capturing) {
    return (
      <div className="flex items-center gap-2.5" role="status" aria-live="polite">
        <span className="inline-flex h-10 items-center gap-2.5 rounded-ctl bg-ink px-3 text-14 font-semibold text-on-light">
          <Spinner className="border-on-light/30 border-t-on-light" />
          {count === 1 ? 'Press a button on a controller…' : 'Press two buttons together on a controller…'}
        </span>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => {
            capturing.abort();
            setCapturing(null);
          }}
          autoFocus
        >
          Cancel
        </Button>
      </div>
    );
  }
  return (
    <div className="flex items-center gap-3">
      {value ? <ButtonChips labels={comboLabels(value, gamepadButtons)} /> : <span className="text-small text-ink-3">Not set</span>}
      <Button
        variant="ghost"
        size="sm"
        disabled={disabled || saver.saving.has(field.key)}
        aria-labelledby={`${labelId} ${field.key}-change`}
        aria-describedby={describedBy}
        onClick={() => void capture()}
        data-nav
      >
        <Icon name="pad" />
        <span id={`${field.key}-change`}>Change</span>
      </Button>
    </div>
  );
}

/** A file or folder, picked with the native dialog. */
function PathControl({ field, value, disabled, saver, labelId, describedBy }: FieldProps) {
  const run = useRun();
  const folder = field.type === 'folderPath';
  const browseId = useId();
  const pick = async () => {
    const title = `${field.label}: pick a ${folder ? 'folder' : 'file'}`;
    const picked = await run(() =>
      folder
        ? bridge.dialogs.pickFolder({ title, initialDirectory: value || undefined }, { timeoutMs: 0 })
        : bridge.dialogs.pickFile({ title, filters: [], initialDirectory: value ? value.replace(/[\\/][^\\/]*$/, '') : undefined }, { timeoutMs: 0 }),
    );
    if (picked && picked !== value) await saver.save(field, picked);
  };
  return (
    <div className="flex w-[min(420px,40vw)] items-center gap-2">
      <div className={cn(inputFrame, frameTone(false), 'min-w-0 flex-1', disabled && 'opacity-50')} title={value}>
        <Icon name={folder ? 'folder' : 'file'} className="size-4 shrink-0 text-ink-3" />
        <span className="min-w-0 flex-1 truncate font-mono text-13 text-ink-2" aria-labelledby={labelId}>
          {value || 'Not set'}
        </span>
      </div>
      <Button
        variant="ghost"
        size="sm"
        disabled={disabled || saver.saving.has(field.key)}
        aria-labelledby={`${browseId} ${labelId}`}
        aria-describedby={describedBy}
        onClick={() => void pick()}
        data-nav
      >
        {saver.saving.has(field.key) ? <Spinner /> : null}
        <span id={browseId}>Browse</span>
      </Button>
    </div>
  );
}

/** One setting: label, help, badges and control; the validation message under it. */
export function SettingRow({
  field,
  value,
  disabled,
  saver,
  gamepadButtons,
}: Omit<FieldProps, 'labelId' | 'describedBy'>) {
  const labelId = useId();
  const helpId = useId();
  const error = saver.errors[field.key];
  const help: ReactNode = field.description;
  const wide = field.type === 'filePath' || field.type === 'folderPath';
  return (
    <div
      className={cn(
        'grid gap-x-6 gap-y-2 border-t border-line py-4 first:border-t-0',
        wide ? 'grid-cols-[minmax(0,1fr)_auto]' : 'grid-cols-[minmax(0,1fr)_minmax(0,auto)]',
        'items-center',
      )}
      data-setting={field.key}
    >
      <div className={cn('min-w-0', disabled && 'opacity-60')}>
        <div className="flex flex-wrap items-center gap-x-2.5 gap-y-1">
          <span id={labelId} className="text-15 font-semibold">
            {field.label}
          </span>
          {field.requiresRestart && (
            <span className="inline-flex h-5 items-center gap-1 rounded-tag bg-pane-3 px-2 text-11 font-[650] text-ink-2">
              <Icon name="refresh" className="size-3" />
              Applies after a restart
            </span>
          )}
        </div>
        {(help || error) && (
          <div id={helpId} className="mt-0.5 max-w-[62ch] text-13 leading-[1.45]">
            {help && <span className="block text-ink-3">{help}</span>}
            {error && (
              <span role="alert" className="mt-1 flex items-start gap-1.5 font-[560] text-danger">
                <Icon name="warning" className="mt-px size-[15px] shrink-0" />
                {error}
              </span>
            )}
          </div>
        )}
      </div>
      <div className="flex min-w-0 justify-end">
        <SettingControl
          field={field}
          value={value}
          disabled={disabled}
          saver={saver}
          gamepadButtons={gamepadButtons}
          labelId={labelId}
          describedBy={help || error ? helpId : undefined}
        />
      </div>
    </div>
  );
}
