import { useEffect, useId, useRef, type ReactNode } from 'react';
import { cn } from './cn';
import { Button } from './Button';
import { Icon, type IconName } from './Icon';

export interface DialogProps {
  open: boolean;
  onClose: () => void;
  title: ReactNode;
  icon?: IconName;
  children: ReactNode;
  className?: string;
  /** Extra content in the header, before the close button. */
  headerExtra?: ReactNode;
}

/** Modal dialog on the native <dialog>: focus trap, Escape and top layer come from the platform. */
export function Dialog({ open, onClose, title, icon, children, className, headerExtra }: DialogProps) {
  const ref = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    if (open && !el.open) el.showModal();
    if (!open && el.open) el.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      aria-labelledby={titleId}
      onClose={onClose}
      onCancel={(e) => {
        e.preventDefault();
        onClose();
      }}
      onClick={(e) => {
        // A click on the backdrop lands on the dialog element itself.
        if (e.target === e.currentTarget) onClose();
      }}
      className={cn(
        'm-auto max-h-[80vh] w-[min(640px,90vw)] overflow-hidden rounded-pane border-0 bg-pane p-0 text-ink',
        'shadow-[0_0_0_1px_var(--color-line-strong),0_30px_80px_rgba(0,0,0,0.6)] backdrop:bg-black/75',
        'open:animate-fade-in',
        className,
      )}
    >
      <div className="flex items-center gap-3 border-b border-line px-5 py-4">
        {icon && <Icon name={icon} />}
        <h2 id={titleId} className="type-h3">
          {title}
        </h2>
        <span className="ml-auto flex items-center gap-2">
          {headerExtra}
          <Button variant="quiet" size="icon" aria-label="Close" onClick={onClose}>
            <Icon name="x" />
          </Button>
        </span>
      </div>
      <div className="max-h-[calc(80vh-72px)] overflow-auto px-[22px] pt-[18px] pb-[22px]">{children}</div>
    </dialog>
  );
}
