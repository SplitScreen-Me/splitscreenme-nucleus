// Mock of the `i18n` service: English (the app's Master/i18n/en.json) and a pseudo-language, en-XA, whose strings are
// accented, longer and bracketed ("[Šéţţîñĝš ~~]"): with it, any text that skips t() stands out in plain English, and
// long translations can be tried. Host texts of the mock (settings labels, notices...) stay English, as they would
// before the host is translated. The choice survives reloads (localStorage 'nucleus.mock.language'); remove that key to
// see the first-launch picker again.
import type { BridgeHandlers, LanguageSummary } from '../generated/contract.ts';
import { BridgeError } from '../errors.ts';
import type { MockContext } from '../transports/mock.ts';
import { DEFAULT_LOCALE, matchLocale, type Messages } from '../../i18n/core.ts';
import { english, englishMeta } from '../../i18n/english.ts';
import type { MockState } from './state.ts';

/** The pseudo-language of the mock. */
export const PSEUDO_LOCALE = 'en-XA';

/** Languages of the mock: English and the pseudo-language. */
export const MOCK_LANGUAGES: readonly LanguageSummary[] = [
  { locale: DEFAULT_LOCALE, englishName: englishMeta.englishName, nativeName: englishMeta.nativeName, rightToLeft: false, completion: 100 },
  { locale: PSEUDO_LOCALE, englishName: 'Pseudo-language', nativeName: '[Ƥşéûđô]', rightToLeft: false, completion: 100 },
];

const STORAGE_KEY = 'nucleus.mock.language';
const ACCENTS: Readonly<Record<string, string>> = Object.fromEntries(
  [...'abcdeghiklnorstuyzABCDEGHIKLNORSTUYZ'].map((c, i) => [c, [...'àƀçđéĝĥîķļñöŕšţûýžÀßÇĐÉĜĤÎĶĻÑÖŔŠŢÛÝŽ'][i]!]),
);

/** "Press <kbd>{key}</kbd>" → "[Ƥŕéšš <kbd>{key}</kbd> ~~]": letters accented, placeholders and tags untouched. */
export function pseudoLocalize(text: string): string {
  const accented = text
    .split(/(\{[A-Za-z_][A-Za-z0-9_]*\}|<\/?[A-Za-z][A-Za-z0-9]*\s*\/?>)/)
    .map((part, i) => (i % 2 === 1 ? part : [...part].map((c) => ACCENTS[c] ?? c).join('')))
    .join('');
  const padding = '~'.repeat(Math.ceil(text.length * 0.3));
  return `[${accented}${padding ? ` ${padding}` : ''}]`;
}

let pseudo: Messages | undefined;
const pseudoMessages = (): Messages => (pseudo ??= Object.fromEntries(Object.entries(english).map(([key, text]) => [key, pseudoLocalize(text)])));

/** The language the mock starts in: the saved choice, else the browser's languages, else English. */
export function initialMockLanguage(): { locale: string; chosen: boolean } {
  const available = MOCK_LANGUAGES.map((l) => l.locale);
  try {
    const saved = globalThis.localStorage?.getItem(STORAGE_KEY);
    if (saved && available.includes(saved)) return { locale: saved, chosen: true };
  } catch {
    // No storage: not chosen.
  }
  return { locale: matchLocale(globalThis.navigator?.languages ?? [], available), chosen: false };
}

/**
 * The mock's `detected` language: the browser's languages stand for Windows'. English when none matches, and then
 * `detectedMatchesWindows` is false, as on an Italian Windows without an Italian file.
 */
export function detectMockLanguage(preferred: readonly string[]): { detected: string; detectedMatchesWindows: boolean } {
  const match = matchLocale(preferred, MOCK_LANGUAGES.map((l) => l.locale), '');
  return match ? { detected: match, detectedMatchesWindows: true } : { detected: DEFAULT_LOCALE, detectedMatchesWindows: false };
}

/** Saves and switches the mock's language, as `i18n.setLanguage` does; false for an unknown language. Shared with the settings mock. */
export function setMockLanguage({ emit }: MockContext, state: MockState, requested: string): boolean {
  const locale = MOCK_LANGUAGES.find((l) => l.locale.toLowerCase() === requested.toLowerCase())?.locale;
  if (!locale) return false;
  const changed = locale !== state.language.locale;
  state.language = { locale, chosen: true };
  state.settings['misc.appLanguage'] = locale;
  try {
    globalThis.localStorage?.setItem(STORAGE_KEY, locale);
  } catch {
    // The choice lasts until the reload.
  }
  if (changed) emit('i18n.languageChanged', { locale });
  return true;
}

/** Mock of the `i18n` service. */
export function createI18nMock(context: MockContext, state: MockState): BridgeHandlers['i18n'] {
  return {
    getState: () => ({
      current: state.language.locale,
      ...detectMockLanguage(globalThis.navigator?.languages ?? []),
      chosen: state.language.chosen,
      languages: [...MOCK_LANGUAGES],
    }),
    getStrings: ({ locale }) => {
      if (locale.toLowerCase() === DEFAULT_LOCALE) return { locale: DEFAULT_LOCALE, strings: { ...english } };
      if (locale.toLowerCase() === PSEUDO_LOCALE.toLowerCase()) return { locale: PSEUDO_LOCALE, strings: { ...pseudoMessages() } };
      throw new BridgeError('notFound', 'This language is not available.', { locale });
    },
    setLanguage: ({ locale }) => {
      if (!setMockLanguage(context, state, locale)) throw new BridgeError('invalidParams', 'This language is not available.', { locale });
    },
  };
}
