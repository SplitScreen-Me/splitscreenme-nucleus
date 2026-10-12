import { createContext, useCallback, useContext, useEffect, useLayoutEffect, useRef, useState, type ReactNode } from 'react';
import { Icon, type IconName } from './Icon';

interface ToastItem {
  id: number;
  message: string;
  icon: IconName;
}

type ToastFn = (message: string, options?: { icon?: IconName; durationMs?: number }) => void;

const ToastContext = createContext<ToastFn>(() => {});

/**
 * One toast at a time, white on the bezel, just above the hint bar. Announced politely. The toast is a manual popover,
 * re-shown for every message, so it stays above a modal dialog opened before it.
 */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [item, setItem] = useState<ToastItem | null>(null);
  const timer = useRef<number | undefined>(undefined);
  const seq = useRef(0);
  const layer = useRef<HTMLDivElement>(null);

  const toast = useCallback<ToastFn>((message, options) => {
    window.clearTimeout(timer.current);
    seq.current += 1;
    setItem({ id: seq.current, message, icon: options?.icon ?? 'check' });
    timer.current = window.setTimeout(() => setItem(null), options?.durationMs ?? 2800);
  }, []);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  useLayoutEffect(() => {
    const el = layer.current;
    if (!el?.showPopover) return;
    try {
      if (el.matches(':popover-open')) el.hidePopover();
      if (item) el.showPopover();
    } catch {
      // Popover unsupported: the fixed layer still shows outside modal dialogs.
    }
  }, [item]);

  return (
    <ToastContext value={toast}>
      {children}
      <div
        ref={layer}
        popover="manual"
        role="status"
        aria-live="polite"
        className="pointer-events-none fixed inset-x-0 top-auto bottom-[58px] z-50 m-0 flex w-full justify-center overflow-visible border-0 bg-transparent p-0 text-inherit"
      >
        {item && (
          <div
            key={item.id}
            className="inline-flex max-w-[min(720px,90vw)] animate-toast-in items-center gap-2.5 rounded-[10px] bg-ink py-[11px] pr-[18px] pl-3.5 text-small font-[650] text-on-light shadow-[0_12px_32px_rgba(0,0,0,0.5)]"
          >
            <Icon name={item.icon} strokeWidth={2.2} className="size-[18px]" />
            <span className="min-w-0">{item.message}</span>
          </div>
        )}
      </div>
    </ToastContext>
  );
}

export function useToast(): ToastFn {
  return useContext(ToastContext);
}
