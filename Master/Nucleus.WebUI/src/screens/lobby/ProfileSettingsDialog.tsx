// Settings of the loaded profile, or of the profile the next session saves: edited as one draft, saved with one
// button. The host validates everything and saves nothing while a field is wrong; its messages show under the fields.
import { useId, useMemo, useRef, useState, type ReactNode, type RefObject } from 'react';
import { bridge, useBridgeQuery, type ProfileSettings, type SettingChoice, type ValidationError } from '@/bridge';
import { useRun } from '@/lib/bridge-ui';
import { Button, cn, Dialog, Icon, PlayerTag, Select, Spinner, Switch, useConfirm, useToast } from '@/ui';
import {
  errorsByField,
  generalErrors,
  isAdvancedError,
  isDirty,
  parseWhole,
  PRIORITY_CLASSES,
  slotKey,
  withField,
  withoutError,
  withSlot,
  type ProfileField,
  type SlotField,
} from './profile-form';

export interface ProfileSettingsDialogProps {
  /** Name of the loaded profile; absent for the profile the next session saves. */
  profileName?: string;
  gameName: string;
  onClose: () => void;
}

/** A field of the settings that is on or off. */
type BoolField = { [K in ProfileField]: ProfileSettings[K] extends boolean ? K : never }[ProfileField];

const PRIORITY_LABELS: Record<(typeof PRIORITY_CLASSES)[number], string> = {
  Normal: 'Normal',
  AboveNormal: 'Above normal',
  High: 'High',
  RealTime: 'Real time',
};

/** Mounted while open: every opening reads the settings again. */
export function ProfileSettingsDialog({ profileName, gameName, onClose }: ProfileSettingsDialogProps) {
  const { data, isPending, isError } = useBridgeQuery('profiles.getSettings');
  const run = useRun();
  const toast = useToast();
  const confirm = useConfirm();
  const [draft, setDraft] = useState<ProfileSettings | null>(null);
  const [errors, setErrors] = useState<ValidationError[]>([]);
  const [saving, setSaving] = useState(false);
  const [advanced, setAdvanced] = useState(false);
  const form = useRef<HTMLFormElement>(null);

  const settings = draft ?? data;
  const dirty = isDirty(data, draft);
  const isNew = settings?.isNew ?? !profileName;
  const byField = useMemo(() => errorsByField(errors), [errors]);

  const close = async () => {
    if (dirty) {
      const discard = await confirm({
        title: 'Discard your changes?',
        message: isNew ? 'The next profile keeps the settings it had.' : 'The profile keeps its saved settings.',
        confirmLabel: 'Discard',
        danger: true,
      });
      if (!discard) return;
    }
    onClose();
  };

  const set = <K extends ProfileField>(field: K, value: ProfileSettings[K]) => {
    if (!settings) return;
    setDraft(withField(settings, field, value));
    setErrors((old) => withoutError(old, field));
  };
  const setSlot = (index: number, field: SlotField, value: string) => {
    if (!settings) return;
    setDraft(withSlot(settings, index, { [field]: value }));
    setErrors((old) => withoutError(old, slotKey(field, index)));
  };

  const save = async () => {
    if (!settings || saving) return;
    setSaving(true);
    const problems = await run(() => bridge.profiles.saveSettings(settings));
    setSaving(false);
    if (problems === undefined) return;
    setErrors(problems);
    if (problems.length === 0) {
      toast(isNew ? 'Settings kept for the next profile' : `${profileName ?? 'Profile'} saved. Its players join again as they press a button.`);
      onClose();
      return;
    }
    if (problems.some(isAdvancedError)) setAdvanced(true);
    requestAnimationFrame(() => form.current?.querySelector<HTMLElement>('[aria-invalid="true"]')?.focus());
  };

  const title = isNew ? 'Settings of the next profile' : `Settings of ${profileName ?? 'the profile'}`;

  return (
    <Dialog open onClose={() => void close()} title={title} icon="sliders" className="w-[min(820px,92vw)]">
      {isPending ? (
        <p className="flex items-center gap-2.5 py-6 text-small text-ink-2" role="status">
          <Spinner /> Reading the profile settings…
        </p>
      ) : isError || !settings ? (
        <div className="grid justify-items-start gap-4 py-2">
          <p className="text-ink-2">The profile settings could not be read.</p>
          <Button variant="ghost" onClick={onClose} data-nav>
            Close
          </Button>
        </div>
      ) : (
        <SettingsForm
          formRef={form}
          settings={settings}
          gameName={gameName}
          byField={byField}
          errors={errors}
          advanced={advanced}
          onAdvanced={setAdvanced}
          set={set}
          setSlot={setSlot}
          footer={
            <div className="sticky -bottom-[22px] z-10 -mx-[22px] mt-5 -mb-[22px] flex items-center gap-2 border-t border-line bg-pane px-[22px] py-3.5">
              <span className="min-w-0 flex-1 text-13 text-ink-3">
                {errors.length > 0 ? (
                  <span className="font-[560] text-danger">Fix the fields in red, then save again.</span>
                ) : isNew ? (
                  'Used when a session saves this setup as a new profile.'
                ) : (
                  'Saving reloads the profile.'
                )}
              </span>
              <Button variant="ghost" onClick={() => void close()} data-nav>
                Cancel
              </Button>
              <Button variant="primary" type="submit" disabled={!dirty || saving} data-nav>
                {saving ? <Spinner className="border-on-light/30 border-t-on-light" /> : <Icon name="check" />}
                Save
              </Button>
            </div>
          }
          onSubmit={() => void save()}
        />
      )}
    </Dialog>
  );
}

interface FormProps {
  formRef: RefObject<HTMLFormElement | null>;
  settings: ProfileSettings;
  gameName: string;
  byField: Record<string, string>;
  errors: ValidationError[];
  advanced: boolean;
  onAdvanced: (open: boolean) => void;
  set: <K extends ProfileField>(field: K, value: ProfileSettings[K]) => void;
  setSlot: (index: number, field: SlotField, value: string) => void;
  footer: ReactNode;
  onSubmit: () => void;
}

function SettingsForm({ formRef, settings: s, gameName, byField, errors, advanced, onAdvanced, set, setSlot, footer, onSubmit }: FormProps) {
  // Fields shown now: a message for a hidden one goes above the form instead of getting lost.
  const shown = new Set<string>([
    'title',
    'notes',
    'autoPlay',
    'useNicknames',
    'customAudio',
    'windowSetupDelayMs',
    'pauseBetweenInstancesSec',
    'autoDesktopScaling',
    'stopUiNavigation',
    'splitDivider',
    'customLayoutHorizontalLines',
    'customLayoutVerticalLines',
    'customLayoutMaxPlayers',
    'windowsMerger',
    'cutscenesKeepAspectRatio',
    'cutscenesMuteAudioOnly',
    'cutscenesUnfocus',
    'network',
    ...(s.splitDivider ? ['splitDividerHideDesktopOnly', 'splitDividerColor'] : []),
    ...(s.windowsMerger ? ['losslessHook', 'mergerResolution'] : []),
    ...s.slots.flatMap((_, i) => [
      slotKey('nickname', i),
      slotKey('steamId', i),
      ...(s.customAudio ? [slotKey('audioDevice', i)] : []),
      ...(advanced ? [slotKey('priorityClass', i), slotKey('idealProcessor', i), slotKey('affinity', i)] : []),
    ]),
  ]);
  const general = generalErrors(errors, (key) => shown.has(key));
  const err = (field: string) => byField[field];

  const toggle = (field: BoolField, label: string, help?: string) => (
    <Row key={field} label={label} help={help} error={err(field)}>
      {({ labelId, describedBy }) => (
        <Switch
          checked={Boolean(s[field])}
          onChange={(on) => set(field, on)}
          aria-labelledby={labelId}
          aria-describedby={describedBy}
          aria-invalid={err(field) ? true : undefined}
          data-nav
        />
      )}
    </Row>
  );

  return (
    <form
      ref={formRef}
      noValidate
      onSubmit={(e) => {
        e.preventDefault();
        onSubmit();
      }}
    >
      <p className="mb-2 text-13 text-ink-3">
        {s.isNew
          ? `${gameName}: the next profile a session saves gets these settings. Per-player settings follow the players placed now.`
          : `${gameName}: these settings are saved in the profile.`}
      </p>
      {general.length > 0 && (
        <p role="alert" className="mb-2 flex items-start gap-1.5 rounded-ctl bg-pane-2 px-3 py-2.5 text-13 font-[560] text-danger">
          <Icon name="warning" className="mt-px size-[15px] shrink-0" />
          {general.join(' ')}
        </p>
      )}

      <Section title="Profile">
        <Row label="Title" help="Shown in the list of profiles. Empty means “Profile” and its number." error={err('title')}>
          {(ids) => <TextInput {...ids} value={s.title} invalid={Boolean(err('title'))} maxLength={64} placeholder="Profile" onChange={(v) => set('title', v)} />}
        </Row>
        <Row label="Notes" error={err('notes')} block>
          {({ labelId, describedBy }) => (
            <textarea
              value={s.notes}
              rows={3}
              spellCheck={false}
              aria-labelledby={labelId}
              aria-describedby={describedBy}
              aria-invalid={err('notes') ? true : undefined}
              onChange={(e) => set('notes', e.target.value)}
              className={cn(
                'min-h-[76px] w-full resize-y rounded-ctl border-0 bg-pane-2 px-3 py-2.5 text-15 leading-[1.45] text-ink outline-none placeholder:text-ink-3',
                tone(Boolean(err('notes'))),
              )}
              placeholder="Who plays where, which controller goes to whom…"
            />
          )}
        </Row>
        {toggle('autoPlay', 'Start by itself', 'Launches as soon as every player of the profile has joined.')}
      </Section>

      <Section title="Players">
        {toggle('useNicknames', 'Use the nicknames in game')}
        {toggle('customAudio', 'Per-player audio', 'Each copy of the game plays on the output chosen for its player.')}
        {s.slots.length === 0 ? (
          <p className="border-t border-line py-3.5 text-13 text-ink-3">Place players in the lobby to set their nickname, Steam ID and audio output.</p>
        ) : (
          <SlotsTable settings={s} byField={byField} setSlot={setSlot} />
        )}
        {s.slots.length > 0 && (
          <div className="border-t border-line pt-3">
            <Button variant="quiet" size="sm" aria-expanded={advanced} onClick={() => onAdvanced(!advanced)} data-nav>
              <Icon name="chev-d" className={cn('transition-transform duration-(--dur-2)', advanced && 'rotate-180')} />
              Processor and priority
            </Button>
            {advanced && <AdvancedTable settings={s} byField={byField} setSlot={setSlot} />}
          </div>
        )}
      </Section>

      <Section title="Windows">
        <Row label="Window setup delay" help="Time each window gets to settle before the next one is set up." error={err('windowSetupDelayMs')}>
          {(ids) => (
            <NumberInput {...ids} value={s.windowSetupDelayMs} unit="ms" min={0} invalid={Boolean(err('windowSetupDelayMs'))} onChange={(v) => set('windowSetupDelayMs', v)} />
          )}
        </Row>
        <Row label="Pause between copies" help="Wait between the launches of two copies of the game." error={err('pauseBetweenInstancesSec')}>
          {(ids) => (
            <NumberInput
              {...ids}
              value={s.pauseBetweenInstancesSec}
              unit="s"
              min={0}
              invalid={Boolean(err('pauseBetweenInstancesSec'))}
              onChange={(v) => set('pauseBetweenInstancesSec', v)}
            />
          )}
        </Row>
        {toggle('autoDesktopScaling', 'Adjust Windows scaling', 'Sets the display scaling the game windows need during the session.')}
        {toggle('stopUiNavigation', 'Turn off gamepad navigation of the app', 'During the session, gamepads only drive the game.')}
        {toggle('splitDivider', 'Background behind the windows', 'Fills the space around the game windows.')}
        {s.splitDivider && toggle('splitDividerHideDesktopOnly', 'Only hide the desktop', 'Hides the desktop instead of drawing the background colour.')}
        {s.splitDivider && (
          <Row label="Background colour" error={err('splitDividerColor')}>
            {(ids) => (
              <ChoiceInput {...ids} value={s.splitDividerColor} choices={s.splitDividerColorChoices} invalid={Boolean(err('splitDividerColor'))} onChange={(v) => set('splitDividerColor', v)} />
            )}
          </Row>
        )}
      </Section>

      <Section title="Custom layout" note="Used when a screen has the custom layout.">
        <Row label="Horizontal lines" help="0 to 5." error={err('customLayoutHorizontalLines')}>
          {(ids) => (
            <NumberInput
              {...ids}
              value={s.customLayoutHorizontalLines}
              min={0}
              max={5}
              invalid={Boolean(err('customLayoutHorizontalLines'))}
              onChange={(v) => set('customLayoutHorizontalLines', v)}
            />
          )}
        </Row>
        <Row label="Vertical lines" help="0 to 5." error={err('customLayoutVerticalLines')}>
          {(ids) => (
            <NumberInput
              {...ids}
              value={s.customLayoutVerticalLines}
              min={0}
              max={5}
              invalid={Boolean(err('customLayoutVerticalLines'))}
              onChange={(v) => set('customLayoutVerticalLines', v)}
            />
          )}
        </Row>
        <Row label="Players" help="Most players the custom layout holds." error={err('customLayoutMaxPlayers')}>
          {(ids) => (
            <NumberInput {...ids} value={s.customLayoutMaxPlayers} min={1} invalid={Boolean(err('customLayoutMaxPlayers'))} onChange={(v) => set('customLayoutMaxPlayers', v)} />
          )}
        </Row>
      </Section>

      <Section title="Merged window">
        {toggle('windowsMerger', 'Merge the game windows', 'Shows every copy of the game in one window.')}
        {s.windowsMerger && toggle('losslessHook', 'Lossless hook', 'Uses the lossless hook with the merged window.')}
        {s.windowsMerger && (
          <Row label="Resolution" help="Size of the merged window." error={err('mergerResolution')}>
            {(ids) => (
              <ChoiceInput {...ids} value={s.mergerResolution} choices={s.mergerResolutionChoices} invalid={Boolean(err('mergerResolution'))} onChange={(v) => set('mergerResolution', v)} />
            )}
          </Row>
        )}
      </Section>

      <Section title="Cutscene mode">
        {toggle('cutscenesKeepAspectRatio', 'Keep the aspect ratio')}
        {toggle('cutscenesMuteAudioOnly', 'Only mute the audio', 'The other windows keep their size and are only muted.')}
        {toggle('cutscenesUnfocus', 'Unfocus the other windows')}
      </Section>

      <Section title="Network">
        <Row label="Network interface" help="The connection the copies of the game use." error={err('network')}>
          {(ids) => <ChoiceInput {...ids} value={s.network} choices={s.networkChoices} invalid={Boolean(err('network'))} onChange={(v) => set('network', v)} />}
        </Row>
      </Section>

      {footer}
    </form>
  );
}

// ---------------------------------------------------------------- players

const cellInput =
  'h-9 w-full min-w-0 rounded-md border-0 bg-pane-2 px-2.5 text-15 text-ink shadow-[inset_0_0_0_1px_var(--color-line)] outline-none placeholder:text-ink-3 focus:shadow-[inset_0_0_0_1.5px_var(--color-ink)] disabled:cursor-not-allowed disabled:opacity-60';
// A ring over the frame (box-shadow layers), so it shows whatever shadow the control draws.
const cellInvalid = 'inset-ring-[1.5px] inset-ring-danger';
const cellSelect = 'h-9 w-full min-w-0 text-14 font-[560]';

interface SlotsProps {
  settings: ProfileSettings;
  byField: Record<string, string>;
  setSlot: (index: number, field: SlotField, value: string) => void;
}

/** Nickname, Steam ID and audio output of each player. */
function SlotsTable({ settings: s, byField, setSlot }: SlotsProps) {
  const audio = s.customAudio;
  return (
    <div className="border-t border-line py-3.5">
      <div className={cn('grid items-center gap-x-3 gap-y-1.5', audio ? 'grid-cols-[44px_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1.2fr)]' : 'grid-cols-[44px_minmax(0,1fr)_minmax(0,1.1fr)]')}>
        <span />
        <span className="text-12 font-[650] text-ink-3">Nickname</span>
        <span className="text-12 font-[650] text-ink-3">Steam ID</span>
        {audio && <span className="text-12 font-[650] text-ink-3">Audio output</span>}
        {s.slots.map((slot, i) => {
          const fields: SlotField[] = audio ? ['nickname', 'steamId', 'audioDevice'] : ['nickname', 'steamId'];
          const messages = fields.map((f) => byField[slotKey(f, i)]).filter(Boolean);
          const bad = (f: SlotField) => Boolean(byField[slotKey(f, i)]);
          return (
            <div key={i} className="contents">
              <PlayerTag n={i + 1} size="md" className="justify-self-start" />
              <input
                value={slot.nickname}
                placeholder={`Player${i + 1}`}
                spellCheck={false}
                maxLength={24}
                aria-label={`Player ${i + 1} nickname`}
                aria-invalid={bad('nickname') || undefined}
                onChange={(e) => setSlot(i, 'nickname', e.target.value)}
                className={cn(cellInput, bad('nickname') && cellInvalid)}
              />
              <span className="relative flex min-w-0 items-center">
                <input
                  value={slot.steamId}
                  placeholder="Default"
                  inputMode="numeric"
                  spellCheck={false}
                  disabled={slot.steamIdLocked}
                  title={slot.steamIdLocked ? 'The handler sets the Steam IDs of this game.' : undefined}
                  aria-label={`Player ${i + 1} Steam ID${slot.steamIdLocked ? ', set by the handler' : ''}`}
                  aria-invalid={bad('steamId') || undefined}
                  onChange={(e) => setSlot(i, 'steamId', e.target.value.trim())}
                  className={cn(cellInput, 'data text-15 tabular-nums', slot.steamIdLocked && 'pr-8', bad('steamId') && cellInvalid)}
                />
                {slot.steamIdLocked && <Icon name="lock" className="pointer-events-none absolute right-2.5 size-4 text-ink-3" />}
              </span>
              {audio && (
                <Select
                  value={slot.audioDevice}
                  aria-label={`Player ${i + 1} audio output`}
                  aria-invalid={bad('audioDevice') || undefined}
                  onChange={(e) => setSlot(i, 'audioDevice', e.target.value)}
                  className={cn(cellSelect, bad('audioDevice') && cellInvalid)}
                >
                  <ChoiceOptions value={slot.audioDevice} choices={s.audioChoices} />
                </Select>
              )}
              {messages.length > 0 && (
                <p role="alert" className="col-[2/-1] -mt-0.5 mb-1 flex items-start gap-1.5 text-13 font-[560] text-danger">
                  <Icon name="warning" className="mt-px size-[15px] shrink-0" />
                  {messages.join(' ')}
                </p>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

/** Process priority, ideal processor and affinity of each player's copy. */
function AdvancedTable({ settings: s, byField, setSlot }: SlotsProps) {
  return (
    <div className="mt-3 grid grid-cols-[44px_minmax(0,1fr)_minmax(0,0.8fr)_minmax(0,1.2fr)] items-center gap-x-3 gap-y-1.5 pb-1">
      <span />
      <span className="text-12 font-[650] text-ink-3">Priority</span>
      <span className="text-12 font-[650] text-ink-3" title="* lets Windows choose">
        Ideal processor
      </span>
      <span className="text-12 font-[650] text-ink-3" title="Processor numbers separated by commas, empty for all">
        Affinity
      </span>
      {s.slots.map((slot, i) => {
        const fields: SlotField[] = ['priorityClass', 'idealProcessor', 'affinity'];
        const messages = fields.map((f) => byField[slotKey(f, i)]).filter(Boolean);
        const bad = (f: SlotField) => Boolean(byField[slotKey(f, i)]);
        const priorities: SettingChoice[] = PRIORITY_CLASSES.map((value) => ({ value, label: PRIORITY_LABELS[value] }));
        return (
          <div key={i} className="contents">
            <PlayerTag n={i + 1} size="md" className="justify-self-start" />
            <Select
              value={slot.priorityClass}
              aria-label={`Player ${i + 1} process priority`}
              aria-invalid={bad('priorityClass') || undefined}
              onChange={(e) => setSlot(i, 'priorityClass', e.target.value)}
              className={cn(cellSelect, bad('priorityClass') && cellInvalid)}
            >
              <ChoiceOptions value={slot.priorityClass} choices={priorities} />
            </Select>
            <input
              value={slot.idealProcessor}
              placeholder="*"
              spellCheck={false}
              aria-label={`Player ${i + 1} ideal processor, * for any`}
              aria-invalid={bad('idealProcessor') || undefined}
              onChange={(e) => setSlot(i, 'idealProcessor', e.target.value.trim())}
              className={cn(cellInput, 'data tabular-nums', bad('idealProcessor') && cellInvalid)}
            />
            <input
              value={slot.affinity}
              placeholder="All processors"
              spellCheck={false}
              aria-label={`Player ${i + 1} processor affinity, numbers separated by commas`}
              aria-invalid={bad('affinity') || undefined}
              onChange={(e) => setSlot(i, 'affinity', e.target.value.replace(/\s+/g, ''))}
              className={cn(cellInput, 'data tabular-nums', bad('affinity') && cellInvalid)}
            />
            {messages.length > 0 && (
              <p role="alert" className="col-[2/-1] -mt-0.5 mb-1 flex items-start gap-1.5 text-13 font-[560] text-danger">
                <Icon name="warning" className="mt-px size-[15px] shrink-0" />
                {messages.join(' ')}
              </p>
            )}
          </div>
        );
      })}
    </div>
  );
}

// ---------------------------------------------------------------- building blocks

function Section({ title, note, children }: { title: string; note?: string; children: ReactNode }) {
  const id = useId();
  return (
    <section aria-labelledby={id} className="mt-5 first-of-type:mt-3">
      <div className="mb-1 flex flex-wrap items-baseline gap-x-3">
        <h3 id={id} className="text-12 font-[700] tracking-[0.08em] text-ink-3 uppercase">
          {title}
        </h3>
        {note && <span className="text-12 text-ink-3">{note}</span>}
      </div>
      <div className="rounded-ctl bg-pane-2/40 px-4 shadow-[inset_0_0_0_1px_var(--color-line)]">{children}</div>
    </section>
  );
}

interface FieldIds {
  labelId: string;
  describedBy?: string;
}

/** One setting: label, help and the validation message on the left, the control on the right (or below). */
function Row({ label, help, error, block, children }: { label: string; help?: string; error?: string; block?: boolean; children: (ids: FieldIds) => ReactNode }) {
  const labelId = useId();
  const helpId = useId();
  return (
    <div className={cn('grid gap-x-6 gap-y-2 border-t border-line py-3.5 first:border-t-0', block ? 'grid-cols-1' : 'grid-cols-[minmax(0,1fr)_auto] items-center')}>
      <div className="min-w-0">
        <span id={labelId} className="text-15 font-semibold">
          {label}
        </span>
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
      <div className={cn('flex min-w-0', !block && 'justify-end')}>{children({ labelId, describedBy: help || error ? helpId : undefined })}</div>
    </div>
  );
}

const tone = (bad: boolean) =>
  bad
    ? 'shadow-[inset_0_0_0_1.5px_var(--color-danger)]'
    : 'shadow-[inset_0_0_0_1px_var(--color-line)] focus-within:shadow-[inset_0_0_0_1.5px_var(--color-ink)] focus:shadow-[inset_0_0_0_1.5px_var(--color-ink)]';

function TextInput({
  labelId,
  describedBy,
  value,
  invalid,
  maxLength,
  placeholder,
  onChange,
}: FieldIds & { value: string; invalid: boolean; maxLength?: number; placeholder?: string; onChange: (value: string) => void }) {
  return (
    <div className={cn('flex h-10 w-[min(340px,34vw)] items-center rounded-ctl bg-pane-2 px-3', tone(invalid))}>
      <input
        value={value}
        maxLength={maxLength}
        placeholder={placeholder}
        spellCheck={false}
        aria-labelledby={labelId}
        aria-describedby={describedBy}
        aria-invalid={invalid || undefined}
        onChange={(e) => onChange(e.target.value)}
        className="min-w-0 flex-1 border-0 bg-transparent text-15 text-ink outline-none placeholder:text-ink-3"
      />
    </div>
  );
}

/** A whole number: typed freely, passed on once it is one (it goes back to the last number when left otherwise). */
function NumberInput({
  labelId,
  describedBy,
  value,
  invalid,
  min,
  max,
  unit,
  onChange,
}: FieldIds & { value: number; invalid: boolean; min?: number; max?: number; unit?: string; onChange: (value: number) => void }) {
  const [text, setText] = useState<string | null>(null);
  return (
    <div className={cn('flex h-10 w-[150px] items-center gap-2 rounded-ctl bg-pane-2 px-3', tone(invalid))}>
      <input
        type="text"
        inputMode="numeric"
        value={text ?? String(value)}
        aria-labelledby={labelId}
        aria-describedby={describedBy}
        aria-invalid={invalid || undefined}
        aria-valuemin={min}
        aria-valuemax={max}
        onChange={(e) => {
          setText(e.target.value);
          const n = parseWhole(e.target.value);
          if (n !== undefined) onChange(n);
        }}
        onBlur={() => setText(null)}
        className="data min-w-0 flex-1 border-0 bg-transparent text-15 text-ink tabular-nums outline-none"
      />
      {unit && <span className="shrink-0 text-13 text-ink-3">{unit}</span>}
    </div>
  );
}

function ChoiceOptions({ value, choices }: { value: string; choices: readonly SettingChoice[] }) {
  const known = choices.some((c) => c.value === value);
  return (
    <>
      {!known && <option value={value}>{value ? `${value} (not available)` : 'Not set'}</option>}
      {choices.map((c) => (
        <option key={c.value} value={c.value}>
          {c.label}
        </option>
      ))}
    </>
  );
}

function ChoiceInput({
  labelId,
  describedBy,
  value,
  choices,
  invalid,
  onChange,
}: FieldIds & { value: string; choices: readonly SettingChoice[]; invalid: boolean; onChange: (value: string) => void }) {
  return (
    <Select
      value={value}
      aria-labelledby={labelId}
      aria-describedby={describedBy}
      aria-invalid={invalid || undefined}
      onChange={(e) => onChange(e.target.value)}
      className={cn('max-w-[min(360px,36vw)] min-w-44', invalid && cellInvalid)}
      data-nav
    >
      <ChoiceOptions value={value} choices={choices} />
    </Select>
  );
}
