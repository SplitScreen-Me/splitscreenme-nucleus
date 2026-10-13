// Formatting helpers, kept for the screens that call them: each follows the language in use (src/i18n/format.ts). New
// code uses `useI18n().format` in components, which also re-renders them when the language changes.
// Relative imports: node tests load this file (src/screens/session/summary.ts).
import { format } from '../i18n/store.ts';

/** 145563 → "146K", 6077633 → "6.1M". */
export const fmtCount = (n: number): string => format.compact(n);

/** 1234 → "1,234". */
export const fmtNumber = (n: number): string => format.number(n);

/** "Jul 31, 2024" by default; `opts` picks other parts. Empty for no or bad dates. */
export function fmtDate(iso: string | undefined, opts: Intl.DateTimeFormatOptions = { day: 'numeric', month: 'short', year: 'numeric' }): string {
  return format.date(iso, opts);
}

/** "July 31, 2024" */
export const fmtLongDate = (iso: string | undefined): string => format.date(iso, { day: 'numeric', month: 'long', year: 'numeric' });

/** "today", "yesterday", "3 days ago", "last week", "in September", "in March 2024". */
export const fmtWhen = (iso: string | undefined, now = new Date()): string => format.when(iso, now);

/** Relative age for comments: "3 days ago", "2 weeks ago", "1 month ago", "2 years ago". */
export const fmtAgo = (iso: string | undefined, now = Date.now()): string => format.ago(iso, now);

/** 8040 → "2 hr 14 min", 300 → "5 min". */
export const fmtDuration = (sec: number): string => format.duration(sec);

/** 0:00:00 clock for running sessions. */
export const fmtClock = (ms: number): string => format.clock(ms);

/**
 * "2 games", "1 game": English only.
 * @deprecated Use a plural string: `t('library.gameCount', { count })` with `gameCount_one` / `gameCount_other` in en.json.
 */
export const plural = (n: number, one: string, many = `${one}s`) => `${n} ${n === 1 ? one : many}`;

/** "Rémi, Léa, and Max", in the language in use. */
export const listJoin = (items: string[]): string => format.list(items);
