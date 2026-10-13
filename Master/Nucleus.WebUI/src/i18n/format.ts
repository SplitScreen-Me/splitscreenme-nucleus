// Numbers, dates, durations, sizes and lists for a language, with Intl. Never build them by hand: the order, separators
// and words change between languages ("1,234.5" / "1 234,5", "2 hr 14 min" / "2 h 14 min", "Jul 31, 2024" / "31 juil. 2024").
import { intlLocale, type Params } from './core.ts';

/** A date as the bridge and the hub send it (ISO 8601), a Date, or milliseconds since the epoch. */
export type DateInput = string | number | Date | undefined | null;

/** Formatters bound to one language. Get them with `useI18n().format` in components, `format` elsewhere. */
export interface Formatters {
  readonly locale: string;
  /** 1234.5 → "1,234.5". */
  number(value: number, options?: Intl.NumberFormatOptions): string;
  /** 0.42 → "42%". */
  percent(ratio: number, maximumFractionDigits?: number): string;
  /** 145563 → "146K", 6077633 → "6.1M" (download counts, ratings). */
  compact(value: number): string;
  /** 1536 → "1.5 kB", 734003200 → "700 MB" (1024 based, like File Explorer). */
  bytes(bytes: number): string;
  /** A date: "Jul 31, 2024" (medium, default), "7/31/24" (short), "July 31, 2024" (long), or Intl options. Empty for no or bad dates. */
  date(value: DateInput, style?: 'short' | 'medium' | 'long' | 'full' | Intl.DateTimeFormatOptions): string;
  /** "3:05 PM". */
  time(value: DateInput): string;
  /** "Jul 31, 2024, 3:05 PM". */
  dateTime(value: DateInput): string;
  /** Age of something: "just now", "5 minutes ago", "3 days ago", "2 weeks ago", "1 month ago", "2 years ago". */
  ago(value: DateInput, now?: number): string;
  /** When in the calendar: "today", "yesterday", "3 days ago", "last week", "2 weeks ago", "in September", "in March 2024". */
  when(value: DateInput, now?: Date): string;
  /** 8040 s → "2 hr 14 min", 300 s → "5 min" (at least one minute). */
  duration(seconds: number): string;
  /** Running time, "1:02:03". */
  clock(ms: number): string;
  /** ["Rémi", "Léa", "Max"] → "Rémi, Léa, and Max" ("or" with 'disjunction', no word with 'unit'). */
  list(items: readonly string[], type?: Intl.ListFormatType): string;
}

/** The phrases the formatters need that Intl has no word for. */
export type FormatPhrases = (key: 'common.time.justNow' | 'common.time.inMonth', params?: Params) => string;

const DAY = 864e5;
const BYTE_UNITS = ['byte', 'kilobyte', 'megabyte', 'gigabyte', 'terabyte'] as const;

function toDate(value: DateInput): Date | undefined {
  if (value === undefined || value === null || value === '') return undefined;
  const d = value instanceof Date ? value : new Date(value);
  return Number.isNaN(d.getTime()) ? undefined : d;
}

/** Formatters for `locale`. `phrases` gives the few words Intl lacks ("just now", "in {month}"). */
export function createFormatters(locale: string, phrases: FormatPhrases): Formatters {
  // Intl gets English for a tag it doesn't know (instead of the runtime's language); `locale` stays the app's tag.
  const intl = intlLocale(locale);
  const cache = new Map<string, unknown>();
  const cached = <T>(kind: string, options: object, make: () => T): T => {
    const key = `${kind}:${JSON.stringify(options)}`;
    let value = cache.get(key) as T | undefined;
    if (value === undefined) {
      value = make();
      cache.set(key, value);
    }
    return value;
  };
  const nf = (options: Intl.NumberFormatOptions = {}) => cached('n', options, () => new Intl.NumberFormat(intl, options));
  const df = (options: Intl.DateTimeFormatOptions) => cached('d', options, () => new Intl.DateTimeFormat(intl, options));
  const rtf = cached('r', {}, () => new Intl.RelativeTimeFormat(intl, { numeric: 'auto' }));
  const unit = (value: number, name: string) => nf({ style: 'unit', unit: name, unitDisplay: 'short', maximumFractionDigits: 1 }).format(value);
  const startOfDay = (d: Date) => new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();

  const styles: Record<string, Intl.DateTimeFormatOptions> = {
    short: { dateStyle: 'short' },
    medium: { dateStyle: 'medium' },
    long: { dateStyle: 'long' },
    full: { dateStyle: 'full' },
  };

  return {
    locale,
    number: (value, options) => nf(options).format(value),
    percent: (ratio, maximumFractionDigits = 0) => nf({ style: 'percent', maximumFractionDigits }).format(ratio),
    // Intl's compact rounding: whole numbers from two digits on (146K), one decimal below (6.1M).
    compact: (value) => nf({ notation: 'compact' }).format(value),
    bytes(bytes) {
      let value = Math.max(0, bytes);
      let index = 0;
      while (value >= 1024 && index < BYTE_UNITS.length - 1) {
        value /= 1024;
        index++;
      }
      return nf({ style: 'unit', unit: BYTE_UNITS[index], unitDisplay: 'short', maximumFractionDigits: value < 10 && index > 0 ? 1 : 0 }).format(value);
    },
    date(value, style = 'medium') {
      const d = toDate(value);
      return d ? df(typeof style === 'string' ? styles[style]! : style).format(d) : '';
    },
    time(value) {
      const d = toDate(value);
      return d ? df({ timeStyle: 'short' }).format(d) : '';
    },
    dateTime(value) {
      const d = toDate(value);
      return d ? df({ dateStyle: 'medium', timeStyle: 'short' }).format(d) : '';
    },
    ago(value, now = Date.now()) {
      const d = toDate(value);
      if (!d) return '';
      const s = Math.max(0, (now - d.getTime()) / 1000);
      if (s < 60) return phrases('common.time.justNow');
      if (s < 3600) return rtf.format(-Math.floor(s / 60), 'minute');
      if (s < 86400) return rtf.format(-Math.floor(s / 3600), 'hour');
      if (s < 86400 * 14) return rtf.format(-Math.floor(s / 86400), 'day');
      if (s < 86400 * 60) return rtf.format(-Math.floor(s / (86400 * 7)), 'week');
      if (s < 86400 * 365) return rtf.format(-Math.floor(s / (86400 * 30)), 'month');
      return rtf.format(-Math.floor(s / (86400 * 365)), 'year');
    },
    when(value, now = new Date()) {
      const d = toDate(value);
      if (!d) return '';
      const days = Math.round((startOfDay(now) - startOfDay(d)) / DAY);
      if (days <= 0) return rtf.format(0, 'day');
      if (days < 7) return rtf.format(-days, 'day');
      if (days < 21) return rtf.format(-Math.floor(days / 7), 'week');
      const sameYear = d.getFullYear() === now.getFullYear();
      const month = df(sameYear ? { month: 'long' } : { month: 'long', year: 'numeric' }).format(d);
      return phrases('common.time.inMonth', { month });
    },
    duration(seconds) {
      const h = Math.floor(seconds / 3600);
      const m = Math.round((seconds % 3600) / 60);
      const parts = [h ? unit(h, 'hour') : '', h && !m ? '' : unit(h ? m : Math.max(1, m), 'minute')].filter(Boolean);
      return cached('l', { type: 'unit', style: 'narrow' }, () => new Intl.ListFormat(intl, { type: 'unit', style: 'narrow' })).format(parts);
    },
    clock(ms) {
      const s = Math.max(0, Math.floor(ms / 1000));
      return `${Math.floor(s / 3600)}:${String(Math.floor((s % 3600) / 60)).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`;
    },
    list: (items, type = 'conjunction') => cached('l', { type }, () => new Intl.ListFormat(intl, { type, style: 'long' })).format(items),
  };
}
