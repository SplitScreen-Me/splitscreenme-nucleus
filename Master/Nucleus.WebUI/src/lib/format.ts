// Formatting in the product voice: plain, short, English (en-GB dates like the mockups).

/** 145563 → "146k", 6077633 → "6.1M". */
export function fmtCount(n: number): string {
  if (n >= 1e6) return `${(n / 1e6).toFixed(n >= 1e7 ? 0 : 1).replace(/\.0$/, '')}M`;
  if (n >= 1e3) return `${(n / 1e3).toFixed(n >= 1e4 ? 0 : 1).replace(/\.0$/, '')}k`;
  return String(n);
}

export const fmtNumber = (n: number) => n.toLocaleString('en');

/** "31 Jul 2024" */
export function fmtDate(iso: string | undefined, opts: Intl.DateTimeFormatOptions = { day: 'numeric', month: 'short', year: 'numeric' }): string {
  if (!iso) return '';
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? '' : d.toLocaleDateString('en-GB', opts);
}

/** "31 July 2024" */
export const fmtLongDate = (iso: string | undefined) => fmtDate(iso, { day: 'numeric', month: 'long', year: 'numeric' });

const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();

/** "today", "yesterday", "3 days ago", "last week", "in September", "in March 2024". */
export function fmtWhen(iso: string | undefined, now = new Date()): string {
  if (!iso) return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  const days = Math.round((startOfDay(now) - startOfDay(d)) / 864e5);
  if (days <= 0) return 'today';
  if (days === 1) return 'yesterday';
  if (days < 7) return `${days} days ago`;
  if (days < 14) return 'last week';
  if (days < 21) return `${Math.floor(days / 7)} weeks ago`;
  const sameYear = d.getFullYear() === now.getFullYear();
  return `in ${d.toLocaleDateString('en-GB', sameYear ? { month: 'long' } : { month: 'long', year: 'numeric' })}`;
}

/** Relative age for comments: "3 days ago", "2 weeks ago", "1 month ago", "2 years ago". */
export function fmtAgo(iso: string | undefined, now = Date.now()): string {
  if (!iso) return '';
  const t = Date.parse(iso);
  if (Number.isNaN(t)) return '';
  const s = Math.max(0, (now - t) / 1000);
  const unit = (v: number, u: string) => `${v} ${u}${v === 1 ? '' : 's'} ago`;
  if (s < 60) return 'just now';
  if (s < 3600) return unit(Math.floor(s / 60), 'minute');
  if (s < 86400) return unit(Math.floor(s / 3600), 'hour');
  if (s < 86400 * 14) return unit(Math.floor(s / 86400), 'day');
  if (s < 86400 * 60) return unit(Math.floor(s / (86400 * 7)), 'week');
  if (s < 86400 * 365) return unit(Math.floor(s / (86400 * 30)), 'month');
  return unit(Math.floor(s / (86400 * 365)), 'year');
}

/** 8040 → "2 h 14 min", 300 → "5 min". */
export function fmtDuration(sec: number): string {
  const h = Math.floor(sec / 3600);
  const m = Math.round((sec % 3600) / 60);
  if (h && m) return `${h} h ${m} min`;
  if (h) return `${h} h`;
  return `${Math.max(1, m)} min`;
}

/** 0:00:00 clock for running sessions. */
export function fmtClock(ms: number): string {
  const s = Math.max(0, Math.floor(ms / 1000));
  return `${Math.floor(s / 3600)}:${String(Math.floor((s % 3600) / 60)).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`;
}

/** "2 games", "1 game" */
export const plural = (n: number, one: string, many = `${one}s`) => `${n} ${n === 1 ? one : many}`;

/** "Rémi, Léa and Max" */
export function listJoin(items: string[]): string {
  if (items.length <= 1) return items.join('');
  return `${items.slice(0, -1).join(', ')} and ${items[items.length - 1]}`;
}
