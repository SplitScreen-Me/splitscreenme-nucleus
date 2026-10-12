import type { InputHTMLAttributes, ReactNode, Ref, SelectHTMLAttributes } from 'react';
import { cn } from './cn';
import { Icon, type IconName } from './Icon';

export interface FieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  icon?: IconName;
  /** Content after the input: a count, a key hint. */
  trailing?: ReactNode;
  size?: 'md' | 'lg';
  /** Class for the wrapping label. */
  className?: string;
  inputClassName?: string;
  ref?: Ref<HTMLInputElement>;
}

/** Text field: a recessed control with an optional leading icon. The whole frame lights on focus. */
export function Field({ icon, trailing, size = 'md', className, inputClassName, ref, ...input }: FieldProps) {
  return (
    <label
      className={cn(
        'flex items-center gap-2.5 bg-pane-2 text-ink-2 shadow-[inset_0_0_0_1px_var(--color-line)]',
        'focus-within:shadow-[inset_0_0_0_1.5px_var(--color-ink)]',
        size === 'lg'
          ? 'h-16 rounded-xl px-5 text-18 [&>svg]:size-[22px]'
          : 'h-11 rounded-ctl px-3.5 [&>svg]:size-[18px]',
        className,
      )}
    >
      {icon && <Icon name={icon} />}
      <input
        ref={ref}
        className={cn(
          'min-w-0 flex-1 border-0 bg-transparent text-ink outline-none placeholder:text-ink-3',
          inputClassName,
        )}
        {...input}
      />
      {trailing}
    </label>
  );
}

/** Compact recessed control used in dense side panels (lobby player rows, options). */
export function MiniField({
  className,
  children,
  title,
}: {
  className?: string;
  children: ReactNode;
  title?: string;
}) {
  return (
    <label
      title={title}
      className={cn(
        'flex h-8 min-w-0 items-center gap-1.5 rounded-md bg-pane-2 px-2 text-12 text-ink-3',
        'shadow-[inset_0_0_0_1px_var(--color-line)] focus-within:shadow-[inset_0_0_0_1.5px_var(--color-ink)]',
        '[&>svg]:size-[15px] [&>svg]:shrink-0',
        className,
      )}
    >
      {children}
    </label>
  );
}

const CHEVRON =
  "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%23a3abba' stroke-width='2' stroke-linecap='round'%3E%3Cpath d='M6 9l6 6 6-6'/%3E%3C/svg%3E\")";

/** Bare select for use inside MiniField. */
export function MiniSelect({ className, style, ...rest }: SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <select
      className={cn(
        'min-w-0 flex-1 appearance-none border-0 bg-transparent bg-[length:12px] bg-[right_center] bg-no-repeat pr-3.5 text-12 text-ink outline-none',
        className,
      )}
      style={{ backgroundImage: CHEVRON, ...style }}
      {...rest}
    />
  );
}

/** Bare text input for use inside MiniField. */
export function MiniInput({ className, ...rest }: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={cn('min-w-0 flex-1 border-0 bg-transparent text-12 text-ink outline-none', className)}
      {...rest}
    />
  );
}

/** Standalone select styled like a field (sort menus, settings). */
export function Select({ className, style, ...rest }: SelectHTMLAttributes<HTMLSelectElement>) {
  return (
    <select
      className={cn(
        'h-9 appearance-none rounded-ctl border-0 bg-pane-2 bg-[length:16px] bg-[right_10px_center] bg-no-repeat pr-[34px] pl-3',
        'text-14 font-semibold text-ink shadow-[inset_0_0_0_1px_var(--color-line)] focus-visible:shadow-[inset_0_0_0_1.5px_var(--color-ink)]',
        className,
      )}
      style={{ backgroundImage: CHEVRON, ...style }}
      {...rest}
    />
  );
}
