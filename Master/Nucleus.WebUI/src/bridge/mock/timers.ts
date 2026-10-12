/** Pending timers of one simulation (an open setup, a session), cleared together when it ends. */
export type TimerSet = Set<ReturnType<typeof setTimeout>>;

/** Runs `run` after `ms` milliseconds, unless `timers` is cleared first. */
export function schedule(timers: TimerSet, ms: number, run: () => void): void {
  const timer = setTimeout(() => {
    timers.delete(timer);
    run();
  }, ms);
  timers.add(timer);
}

/** Cancels every pending timer of `timers`. */
export function clearTimers(timers: TimerSet): void {
  for (const timer of timers) clearTimeout(timer);
  timers.clear();
}
