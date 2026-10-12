import { useEffect, useId, useRef, useState, type ReactNode } from 'react';
import { cn } from './cn';
import { buttonClass, type ButtonSize, type ButtonVariant } from './Button';
import { Icon } from './Icon';

export interface MenuItem {
  id: string;
  label: string;
  description?: string;
  checked?: boolean;
}

export interface MenuButtonProps {
  label: ReactNode;
  items: MenuItem[];
  onSelect: (id: string) => void;
  /** Accessible name of the menu. */
  menuLabel: string;
  variant?: ButtonVariant;
  size?: ButtonSize;
  className?: string;
  describedBy?: string;
  align?: 'start' | 'end';
}

/** A button that opens a single-choice menu (menuitemradio). Arrow keys move, Escape closes. */
export function MenuButton({
  label,
  items,
  onSelect,
  menuLabel,
  variant = 'ghost',
  size = 'sm',
  className,
  describedBy,
  align = 'end',
}: MenuButtonProps) {
  const [open, setOpen] = useState(false);
  const root = useRef<HTMLDivElement>(null);
  const button = useRef<HTMLButtonElement>(null);
  const list = useRef<HTMLUListElement>(null);
  const menuId = useId();

  useEffect(() => {
    if (!open) return;
    const onDown = (e: PointerEvent) => {
      if (!root.current?.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('pointerdown', onDown);
    const first =
      list.current?.querySelector<HTMLButtonElement>('[aria-checked="true"]') ??
      list.current?.querySelector<HTMLButtonElement>('button');
    first?.focus();
    return () => document.removeEventListener('pointerdown', onDown);
  }, [open]);

  const close = (refocus = true) => {
    setOpen(false);
    if (refocus) button.current?.focus();
  };

  return (
    <div ref={root} className="relative">
      <button
        ref={button}
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={open ? menuId : undefined}
        aria-describedby={describedBy}
        onClick={() => setOpen((o) => !o)}
        className={buttonClass(variant, size, cn('gap-2 [&_svg]:size-4', className))}
      >
        {label}
        <Icon name="chev-d" />
      </button>
      {open && (
        <ul
          ref={list}
          id={menuId}
          role="menu"
          aria-label={menuLabel}
          onKeyDown={(e) => {
            const buttons = [...(list.current?.querySelectorAll<HTMLButtonElement>('button') ?? [])];
            const k = buttons.indexOf(document.activeElement as HTMLButtonElement);
            if (e.key === 'ArrowDown') {
              e.preventDefault();
              buttons[(k + 1) % buttons.length]?.focus();
            } else if (e.key === 'ArrowUp') {
              e.preventDefault();
              buttons[(k - 1 + buttons.length) % buttons.length]?.focus();
            } else if (e.key === 'Escape') {
              e.preventDefault();
              e.stopPropagation();
              close();
            } else if (e.key === 'Tab') {
              close(false);
            }
          }}
          className={cn(
            'absolute top-[calc(100%+6px)] z-30 m-0 min-w-[270px] list-none rounded-[10px] bg-pane-2 p-1.5',
            'shadow-[0_0_0_1px_var(--color-line-strong),0_20px_44px_rgba(0,0,0,0.6)] animate-fade-in',
            align === 'end' ? 'right-0' : 'left-0',
          )}
        >
          {items.map((item) => (
            <li key={item.id} role="none">
              <button
                type="button"
                role="menuitemradio"
                aria-checked={Boolean(item.checked)}
                onClick={() => {
                  onSelect(item.id);
                  close();
                }}
                className="grid w-full grid-cols-[18px_1fr] items-center gap-x-2 rounded-md border-0 bg-transparent px-2.5 py-2 text-left text-small hover:bg-pane-3 focus-visible:bg-pane-3 focus-visible:outline-none"
              >
                <Icon name="check" className={cn('size-4', !item.checked && 'invisible')} />
                <span>{item.label}</span>
                {item.description && <span className="col-start-2 text-12 text-ink-3">{item.description}</span>}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
