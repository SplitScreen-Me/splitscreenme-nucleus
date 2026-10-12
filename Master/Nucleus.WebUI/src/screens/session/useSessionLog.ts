import { useEffect, useMemo, useRef, useState } from 'react';
import { useBridgeEvent, useBridgeQuery, type SessionLogEntry } from '@/bridge';

/**
 * The session log: `session.getLog` (lines of the current or last session) plus the `session.log` lines pushed since,
 * de-duplicated by `seq` and in order. A new session (another `startedAt`) starts over.
 */
export function useSessionLog(startedAt: string | undefined): SessionLogEntry[] {
  const { data: base = [], refetch } = useBridgeQuery('session.getLog');
  const [pushed, setPushed] = useState<SessionLogEntry[]>([]);
  const session = useRef(startedAt);

  useBridgeEvent('session.log', ({ entries }) => setPushed((old) => [...old, ...entries]));

  useEffect(() => {
    if (!startedAt || session.current === startedAt) return;
    session.current = startedAt;
    setPushed([]);
    void refetch();
  }, [startedAt, refetch]);

  return useMemo(() => {
    const bySeq = new Map<number, SessionLogEntry>();
    for (const entry of base) bySeq.set(entry.seq, entry);
    for (const entry of pushed) bySeq.set(entry.seq, entry);
    return [...bySeq.values()].sort((a, b) => a.seq - b.seq);
  }, [base, pushed]);
}
