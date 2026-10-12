import type { ButtonHTMLAttributes, InputHTMLAttributes, ReactNode } from 'react';
import { cn } from './cn';
import { Icon } from './Icon';

export interface SwitchProps extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'onChange'> {
  checked: boolean;
  onChange: (checked: boolean) => void;
}

/** On/off switch. On is white, like every selected state. */
export function Switch({ checked, onChange, className, ...rest }: SwitchProps) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      onClick={() => onChange(!checked)}
      className={cn(
        'relative h-[22px] w-[38px] shrink-0 rounded-full border-0 p-0 transition-colors duration-(--dur-1)',
        checked ? 'bg-ink' : 'bg-pane-3',
        className,
      )}
      {...rest}
    >
      <span
        aria-hidden="true"
        className={cn(
          'absolute top-[3px] left-[3px] size-4 rounded-full transition-[transform,background-color] duration-(--dur-2) ease-snap',
          checked ? 'translate-x-4 bg-on-light' : 'bg-ink-2',
        )}
      />
    </button>
  );
}

/** Square checkbox with a white checked state. Wrap it in a <label> with its text. */
export function Checkbox({ className, ...rest }: Omit<InputHTMLAttributes<HTMLInputElement>, 'type'>) {
  return (
    <span className={cn('relative inline-grid size-5 shrink-0 place-items-center', className)}>
      <input
        type="checkbox"
        className={cn(
          'peer m-0 size-5 cursor-pointer appearance-none rounded-md shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]',
          'checked:bg-ink checked:shadow-none',
        )}
        {...rest}
      />
      <Icon
        name="check"
        strokeWidth={2.6}
        className="pointer-events-none absolute size-3.5 text-on-light opacity-0 peer-checked:opacity-100"
      />
    </span>
  );
}

/** Segmented control: one choice out of a few, as a row of pressable cells. */
export function Segmented<T extends string>({
  options,
  value,
  onChange,
  label,
  className,
}: {
  options: ReadonlyArray<{ value: T; label: ReactNode; ariaLabel?: string }>;
  value: T;
  onChange: (value: T) => void;
  label: string;
  className?: string;
}) {
  return (
    <div
      role="group"
      aria-label={label}
      className={cn('grid gap-1 rounded-[10px] bg-pane-2 p-1', className)}
      style={{ gridTemplateColumns: `repeat(${options.length}, minmax(0, 1fr))` }}
    >
      {options.map((o) => (
        <button
          key={o.value}
          type="button"
          aria-pressed={o.value === value}
          aria-label={o.ariaLabel}
          onClick={() => onChange(o.value)}
          className={cn(
            'h-[34px] rounded-[7px] border-0 text-14 font-[650] stretch-narrow tabular-nums transition-colors duration-(--dur-1)',
            o.value === value ? 'bg-ink text-on-light' : 'bg-transparent text-ink-2 hover:bg-pane-3 hover:text-ink',
          )}
        >
          {o.label}
        </button>
      ))}
    </div>
  );
}

/** Small busy indicator. Always paired with words. */
export function Spinner({ className }: { className?: string }) {
  return (
    <span
      aria-hidden="true"
      className={cn('inline-block size-3.5 shrink-0 animate-spin rounded-full border-2 border-line-strong border-t-ink', className)}
    />
  );
}

/** Verified mark: a small white seal, not a colour. */
export function Verified({ children = 'Verified', className }: { children?: ReactNode; className?: string }) {
  return (
    <span className={cn('inline-flex items-center gap-[5px] text-small font-semibold text-ink [&>svg]:size-4', className)}>
      <Icon name="seal" />
      {children}
    </span>
  );
}
