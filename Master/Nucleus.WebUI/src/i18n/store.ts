// The language of the page: one snapshot (language, strings, formatters) for the whole app, replaced when the language
// changes. React reads it through I18nProvider / useI18n (react.tsx); other code uses `t` and `format` from here.
import { createTranslator, DEFAULT_LOCALE, type Messages, type Params, type Translator } from './core.ts';
import { english } from './english.ts';
import { createFormatters, type Formatters } from './format.ts';
import type { MessageKey, TFunction } from './types.ts';

/** Everything that depends on the language in use. */
export interface I18nSnapshot {
  /** Language in use (`en`, `fr`, `pt-BR`). */
  readonly locale: string;
  /** Written right to left. */
  readonly rtl: boolean;
  readonly t: TFunction;
  readonly format: Formatters;
  readonly translator: Translator;
}

const CACHE_KEY = 'nucleus.i18n';

interface CachedLanguage {
  locale: string;
  rtl: boolean;
  messages: Record<string, string>;
}

let snapshot: I18nSnapshot = initialSnapshot();
const listeners = new Set<() => void>();

/** The current snapshot. */
export function getI18n(): I18nSnapshot {
  return snapshot;
}

/** Calls `listener` after each change of language; returns the unsubscribe function. */
export function subscribeI18n(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

/**
 * Switches the page to `locale` with its own strings (English fills the gaps): `<html lang dir>`, every `t`, every
 * formatter. The strings are kept in localStorage so the next start shows its first screen in this language.
 */
export function activateLanguage(locale: string, messages: Messages, rtl = false): void {
  snapshot = buildSnapshot(locale, messages, rtl);
  applyToDocument(snapshot);
  writeCache(locale === DEFAULT_LOCALE ? undefined : { locale, rtl, messages: { ...messages } });
  for (const listener of [...listeners]) listener();
}

/**
 * Translates in the language in use, outside React (event handlers, lib code). Components use `useT()` /
 * `useI18n()`, which re-render them when the language changes.
 */
export const t: TFunction = (key, ...params) => snapshot.t(key, ...params);

/** `t` for a key computed at runtime. Prefer typed keys (a `Record<Kind, MessageKey>`, `isMessageKey`). */
export const tDynamic = (key: string, params?: Params): string => snapshot.translator.t(key, params);

/** Whether `key` is a key of en.json (a base key for plural strings). */
export const isMessageKey = (key: string): key is MessageKey => english[key] !== undefined || english[`${key}_other`] !== undefined;

/** Formatters of the language in use, outside React. Components use `useI18n().format`. */
export const format: Formatters = new Proxy({} as Formatters, {
  get: (_target, name) => snapshot.format[name as keyof Formatters],
});

function buildSnapshot(locale: string, messages: Messages, rtl: boolean): I18nSnapshot {
  const translator = createTranslator({
    locale,
    messages,
    fallback: english,
    onMissing: (key) => console.warn(`[i18n] missing string "${key}" (not in ${locale} nor in English)`),
  });
  const t = ((key: string, params?: Params) => translator.t(key, params)) as TFunction;
  const formatters = createFormatters(locale, (key, params) => translator.t(key, params));
  return { locale, rtl, t, format: formatters, translator };
}

function initialSnapshot(): I18nSnapshot {
  const cached = readCache();
  const initial = cached ? buildSnapshot(cached.locale, cached.messages, cached.rtl) : buildSnapshot(DEFAULT_LOCALE, english, false);
  applyToDocument(initial);
  return initial;
}

function applyToDocument({ locale, rtl }: I18nSnapshot): void {
  const root = globalThis.document?.documentElement;
  if (!root) return;
  root.lang = locale;
  root.dir = rtl ? 'rtl' : 'ltr';
}

function readCache(): CachedLanguage | undefined {
  try {
    const raw = globalThis.localStorage?.getItem(CACHE_KEY);
    if (!raw) return undefined;
    const value = JSON.parse(raw) as Partial<CachedLanguage>;
    return typeof value.locale === 'string' && value.messages && typeof value.messages === 'object'
      ? { locale: value.locale, rtl: value.rtl === true, messages: value.messages }
      : undefined;
  } catch {
    return undefined;
  }
}

function writeCache(value: CachedLanguage | undefined): void {
  try {
    if (value) globalThis.localStorage?.setItem(CACHE_KEY, JSON.stringify(value));
    else globalThis.localStorage?.removeItem(CACHE_KEY);
  } catch {
    // Storage full or unavailable: the next start shows English until the host answers.
  }
}
