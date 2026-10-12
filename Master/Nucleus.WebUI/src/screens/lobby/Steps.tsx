import { useState } from 'react';
import type { OptionChoice, SetupCustomStep, SetupOption, SetupSnapshot } from '@/bridge';
import { cn, Field, Icon, Pane, Segmented, Select } from '@/ui';
import type { LobbyCommands } from './commands';

/** Options step: the handler's options, each with its control. */
export function OptionsStep({ snap, commands }: { snap: SetupSnapshot; commands: LobbyCommands }) {
  const editable = snap.options.filter((o) => o.kind !== 'unsupported');
  return (
    <Pane as="section" aria-labelledby="lobby-options-title" className="min-h-0 overflow-y-auto px-8 pt-7 pb-8" data-autofocus>
      <h2 id="lobby-options-title" className="type-h2">
        Handler options
      </h2>
      <p className="mt-2 max-w-[62ch] text-ink-2">
        The handler author offers these choices. They apply to every copy of {snap.gameName}
        {snap.profile.loaded ? ' and come from the loaded profile' : ''}.
      </p>
      {editable.length === 0 && <p className="mt-6 text-ink-3">This handler has no option you can change here.</p>}
      <div className="mt-6 grid max-w-[880px] border-t border-line">
        {snap.options.map((o) => (
          <OptionRow key={o.key} option={o} commands={commands} />
        ))}
      </div>
    </Pane>
  );
}

function OptionRow({ option: o, commands }: { option: SetupOption; commands: LobbyCommands }) {
  const [text, setText] = useState<string | null>(null);
  const id = `opt-${o.key}`;
  const short = o.choices.length > 0 && o.choices.length <= 4 && o.choices.every((c) => c.label.length <= 14);
  const commitText = () => {
    if (text !== null && text !== (o.text ?? '')) void commands.setOptionText(o.key, text);
    setText(null);
  };
  return (
    <div className="grid grid-cols-[minmax(0,1fr)_minmax(220px,340px)] items-center gap-6 border-b border-line py-4">
      <div className="min-w-0">
        <label htmlFor={o.kind === 'unsupported' ? undefined : id} id={`${id}-label`} className="block text-16 font-[650]">
          {o.name}
        </label>
        {o.description && <p className="mt-0.5 text-13 leading-[1.45] text-ink-3">{o.description}</p>}
      </div>
      <div className="min-w-0 justify-self-stretch">
        {o.kind === 'choice' &&
          (short ? (
            <Segmented
              label={o.name}
              value={String(o.selectedIndex ?? -1)}
              options={o.choices.map((c) => ({ value: String(c.index), label: c.label }))}
              onChange={(v) => void commands.setOptionChoice(o.key, Number(v))}
            />
          ) : (
            <Select
              id={id}
              className="w-full"
              value={o.selectedIndex === undefined ? '' : String(o.selectedIndex)}
              onChange={(e) => void commands.setOptionChoice(o.key, Number(e.target.value))}
            >
              {o.selectedIndex === undefined && <option value="">Choose…</option>}
              {o.choices.map((c) => (
                <option key={c.index} value={c.index}>
                  {c.label}
                </option>
              ))}
            </Select>
          ))}
        {o.kind === 'text' && (
          <Field
            id={id}
            autoComplete="off"
            spellCheck={false}
            value={text ?? o.text ?? ''}
            onChange={(e) => setText(e.target.value)}
            onBlur={commitText}
            onKeyDown={(e) => {
              if (e.key === 'Enter') e.currentTarget.blur();
              if (e.key === 'Escape') {
                setText(null);
                e.currentTarget.blur();
              }
            }}
          />
        )}
        {o.kind === 'unsupported' && (
          <p className="flex items-center gap-2 text-13 text-ink-3">
            <Icon name="info" className="size-4" />
            Uses the handler's default value
          </p>
        )}
      </div>
    </div>
  );
}

/** A handler-defined step: pick one item, shown as cards. */
export function CustomStep({ step, commands }: { step: SetupCustomStep; commands: LobbyCommands }) {
  const pictures = step.items.some((item) => item.imageUrl);
  return (
    <Pane as="section" aria-labelledby="lobby-step-title" className="min-h-0 overflow-y-auto px-8 pt-7 pb-8" data-autofocus>
      <h2 id="lobby-step-title" className="type-h2">
        {step.title}
      </h2>
      <p className="mt-2 max-w-[62ch] text-ink-2">
        {step.required ? 'Pick one to continue.' : 'Optional: pick one, or go on without.'}
        {step.autoSelected ? ' The loaded profile already chose for you.' : ''}
      </p>
      <ul
        role="radiogroup"
        aria-labelledby="lobby-step-title"
        className={cn(
          'm-0 mt-6 grid list-none gap-3 p-0',
          pictures ? 'grid-cols-[repeat(auto-fill,minmax(230px,1fr))]' : 'grid-cols-[repeat(auto-fill,minmax(260px,1fr))]',
        )}
      >
        {step.items.map((item) => (
          <li key={item.index}>
            <StepCard
              item={item}
              selected={step.selectedIndex === item.index}
              fromProfile={step.autoSelected && step.selectedIndex === item.index}
              onPick={() => void commands.selectStepItem(step.index, item.index)}
            />
          </li>
        ))}
      </ul>
    </Pane>
  );
}

function StepCard({ item, selected, fromProfile, onPick }: { item: OptionChoice; selected: boolean; fromProfile: boolean; onPick: () => void }) {
  const [broken, setBroken] = useState(false);
  return (
    <button
      type="button"
      role="radio"
      aria-checked={selected}
      onClick={onPick}
      data-nav
      data-nav-frame="own"
      className={cn(
        'group relative grid h-full w-full content-start overflow-hidden rounded-cover border-0 bg-pane-2 p-0 text-left text-ink transition-[box-shadow,background-color] duration-(--dur-1)',
        'outline-3 outline-offset-3 outline-transparent nav-focus:outline-ink',
        selected ? 'shadow-[inset_0_0_0_2px_var(--color-ink)]' : 'hover:bg-pane-3',
      )}
    >
      {item.imageUrl && !broken && (
        <img src={item.imageUrl} alt="" loading="lazy" onError={() => setBroken(true)} className="aspect-video w-full bg-pane-3 object-cover" />
      )}
      <span className="grid gap-1 p-4">
        <span className="flex items-center gap-2">
          <b className="min-w-0 flex-1 text-16 font-[680]">{item.label}</b>
          <span
            aria-hidden="true"
            className={cn('grid size-6 shrink-0 place-items-center rounded-full', selected ? 'bg-ink text-on-light' : 'shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]')}
          >
            {selected && <Icon name="check" strokeWidth={2.6} className="size-3.5" />}
          </span>
        </span>
        {item.details && <span className="text-13 leading-[1.45] text-ink-2">{item.details}</span>}
        {fromProfile && <span className="text-12 font-[650] text-ink-3">Chosen by the profile</span>}
      </span>
    </button>
  );
}
