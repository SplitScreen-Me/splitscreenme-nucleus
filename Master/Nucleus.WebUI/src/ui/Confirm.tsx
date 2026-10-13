import { createContext, useCallback, useContext, useRef, useState, type ReactNode } from 'react';
import { useT } from '@/i18n';
import { Button } from './Button';
import { Dialog } from './Dialog';
import type { IconName } from './Icon';

export interface ConfirmOptions {
  title: string;
  /** The question or what will happen, in plain words. */
  message: ReactNode;
  /** Label of the confirming button, a verb: "Delete", "Replace". */
  confirmLabel?: string;
  cancelLabel?: string;
  /** Destructive: the confirming button is red. */
  danger?: boolean;
  icon?: IconName;
}

type ConfirmFn = (options: ConfirmOptions) => Promise<boolean>;

const ConfirmContext = createContext<ConfirmFn>(async () => false);

/** One confirmation at a time, on the native dialog. `useConfirm()` resolves true when the user confirms. */
export function ConfirmProvider({ children }: { children: ReactNode }) {
  const [current, setCurrent] = useState<ConfirmOptions | null>(null);
  const resolver = useRef<((ok: boolean) => void) | null>(null);
  const t = useT();

  const confirm = useCallback<ConfirmFn>(
    (options) =>
      new Promise<boolean>((resolve) => {
        resolver.current?.(false);
        resolver.current = resolve;
        setCurrent(options);
      }),
    [],
  );

  const settle = (ok: boolean) => {
    resolver.current?.(ok);
    resolver.current = null;
    setCurrent(null);
  };

  return (
    <ConfirmContext value={confirm}>
      {children}
      <Dialog
        open={current !== null}
        onClose={() => settle(false)}
        title={current?.title ?? ''}
        icon={current?.icon ?? (current?.danger ? 'warning' : undefined)}
        className="w-[min(520px,90vw)]"
      >
        {current && (
          <div className="grid gap-6">
            <div className="text-15 leading-[1.55] text-ink-2">{current.message}</div>
            <div className="flex justify-end gap-2">
              <Button variant="ghost" onClick={() => settle(false)} data-nav>
                {current.cancelLabel ?? t('common.cancel')}
              </Button>
              <Button variant={current.danger ? 'danger' : 'primary'} onClick={() => settle(true)} data-nav autoFocus>
                {current.confirmLabel ?? t('common.continue')}
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </ConfirmContext>
  );
}

/** Asks the user to confirm: `if (await confirm({ title, message, danger: true })) …`. */
export function useConfirm(): ConfirmFn {
  return useContext(ConfirmContext);
}
