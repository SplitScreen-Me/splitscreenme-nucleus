import type { ButtonHTMLAttributes, ReactNode, Ref } from 'react';
import { Link, type LinkProps } from 'react-router';
import { cn } from './cn';

export type ButtonVariant = 'default' | 'primary' | 'ghost' | 'danger' | 'quiet';
export type ButtonSize = 'sm' | 'md' | 'lg' | 'icon' | 'icon-sm';

const BASE =
  'inline-flex items-center justify-center gap-2.5 rounded-ctl border-0 font-[650] whitespace-nowrap no-underline select-none ' +
  'transition-[transform,background-color,box-shadow,color] duration-(--dur-1) ease-snap active:translate-y-px active:scale-[0.99] ' +
  '[&_svg]:size-[18px] [&_svg]:shrink-0 ' +
  'disabled:cursor-not-allowed disabled:bg-pane-3 disabled:text-ink-3 disabled:shadow-none disabled:active:transform-none ' +
  'aria-disabled:cursor-not-allowed aria-disabled:bg-pane-3 aria-disabled:text-ink-3 aria-disabled:shadow-none';

const VARIANT: Record<ButtonVariant, string> = {
  default: 'bg-pane-3 text-ink hover:bg-[color-mix(in_oklab,var(--color-pane-3),white_10%)]',
  /* The brightest thing on screen is the thing you press. */
  primary: 'bg-ink text-on-light hover:bg-white hover:shadow-[0_0_0_4px_rgba(255,255,255,0.14)]',
  ghost: 'bg-transparent text-ink shadow-[inset_0_0_0_1px_var(--color-line-strong)] hover:bg-pane-2',
  danger:
    'bg-transparent text-danger shadow-[inset_0_0_0_1px_color-mix(in_oklab,var(--color-danger),transparent_55%)] ' +
    'hover:bg-[color-mix(in_oklab,var(--color-danger),transparent_88%)]',
  quiet: 'bg-transparent text-ink-2 hover:bg-pane-2 hover:text-ink',
};

const SIZE: Record<ButtonSize, string> = {
  sm: 'min-h-[34px] px-3.5 text-14',
  md: 'min-h-11 px-5 text-15',
  lg: 'min-h-14 px-7 text-17 rounded-[10px]',
  icon: 'size-10 min-h-10 p-0',
  'icon-sm': 'size-[30px] min-h-[30px] p-0 [&_svg]:size-4',
};

export function buttonClass(variant: ButtonVariant = 'default', size: ButtonSize = 'md', className?: string) {
  return cn(BASE, VARIANT[variant], SIZE[size], className);
}

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  ref?: Ref<HTMLButtonElement>;
  children?: ReactNode;
}

export function Button({ variant, size, className, type = 'button', ...rest }: ButtonProps) {
  return <button type={type} className={buttonClass(variant, size, className)} {...rest} />;
}

export interface ButtonLinkProps extends Omit<LinkProps, 'className'> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  className?: string;
  ref?: Ref<HTMLAnchorElement>;
}

/** A router link that looks like a button (navigation, not an action). */
export function ButtonLink({ variant, size, className, ...rest }: ButtonLinkProps) {
  return <Link className={buttonClass(variant, size, className)} {...rest} />;
}
