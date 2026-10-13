import { useEffect, useRef, useState, type KeyboardEvent, type PointerEvent } from 'react';
import { useT } from '@/i18n';
import { buttonClass, cn, Icon } from '@/ui';

const HOLD_MS = 1000;

/**
 * Stop is destructive (every copy closes), so it asks for a one-second hold, with the fill as
 * feedback. Works with pointer and with Enter / Space held down. While inputs are locked the host refuses to stop, so the
 * button says why instead of filling.
 */
export function HoldToStop({
  stopped,
  stopping,
  blocked,
  onConfirm,
  onBlocked,
}: {
  stopped: boolean;
  stopping: boolean;
  /** Inputs are locked: stopping is refused until they are unlocked. */
  blocked?: boolean;
  onConfirm: () => void;
  /** Pressed while blocked. */
  onBlocked?: () => void;
}) {
  const t = useT();
  const [progress, setProgress] = useState(0);
  const [holding, setHolding] = useState(false);
  const start = useRef(0);
  const raf = useRef(0);
  const confirm = useRef(onConfirm);
  confirm.current = onConfirm;

  const reset = () => {
    cancelAnimationFrame(raf.current);
    start.current = 0;
    setHolding(false);
    setProgress(0);
  };

  useEffect(() => () => cancelAnimationFrame(raf.current), []);
  useEffect(() => {
    if (stopped) reset();
  }, [stopped]);

  const frame = (now: number) => {
    const k = Math.min(1, (now - start.current) / HOLD_MS);
    setProgress(k);
    if (k >= 1) {
      start.current = 0;
      setHolding(false);
      confirm.current();
    } else raf.current = requestAnimationFrame(frame);
  };

  const begin = (e: PointerEvent | KeyboardEvent) => {
    if (stopped || stopping || start.current) return;
    if (blocked) {
      e.preventDefault();
      onBlocked?.();
      return;
    }
    e.preventDefault();
    start.current = performance.now();
    setHolding(true);
    raf.current = requestAnimationFrame(frame);
  };
  const end = () => {
    if (!start.current) return;
    reset();
  };

  const label = stopped
    ? t('session.stop.stopped')
    : stopping
      ? t('session.stop.stopping')
      : blocked
        ? t('session.stop.blocked')
        : holding
          ? t('session.stop.holding')
          : t('session.stop.hold');

  return (
    <>
      <button
        type="button"
        disabled={stopped || stopping}
        aria-disabled={blocked || undefined}
        aria-describedby="hold-to-stop-hint"
        onPointerDown={(e) => {
          if (e.button === 0) begin(e);
        }}
        onPointerUp={end}
        onPointerLeave={end}
        onPointerCancel={end}
        onKeyDown={(e) => {
          if ((e.key === 'Enter' || e.key === ' ') && !e.repeat) begin(e);
        }}
        onKeyUp={(e) => {
          if (e.key === 'Enter' || e.key === ' ') end();
        }}
        onBlur={end}
        className={cn(buttonClass('danger', 'md'), 'relative isolate min-w-[152px] overflow-hidden touch-none', blocked && 'opacity-70')}
      >
        <span
          aria-hidden="true"
          className="absolute inset-0 -z-10 origin-left bg-danger/28"
          style={{ transform: `scaleX(${progress})` }}
        />
        <Icon name={blocked ? 'lock' : 'stop'} />
        <span>{label}</span>
      </button>
      <span id="hold-to-stop-hint" className="sr-only">
        {blocked ? t('session.stop.blockedHint') : t('session.stop.hint')}
      </span>
    </>
  );
}
