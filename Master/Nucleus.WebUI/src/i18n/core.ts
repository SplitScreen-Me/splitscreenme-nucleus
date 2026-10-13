// The translation engine, without React or the bridge (node tests run it as it is). Format of the language files, key
// rules and the C# twin (Nucleus.Gaming.Localization): docs/webui/i18n.md.

/** Strings of one language by dotted key (`common.ok`), plural forms as `key_one`, `key_other`... */
export type Messages = Readonly<Record<string, string>>;

/** A placeholder value: numbers are formatted for the language (pass a string to print digits as they are). */
export type ParamValue = string | number;

/** Placeholder values by name. A numeric `count` selects the plural form. */
export type Params = Readonly<Record<string, ParamValue>>;

/** CLDR plural categories, in the order of key suffixes. */
export const PLURAL_CATEGORIES = ['zero', 'one', 'two', 'few', 'many', 'other'] as const;

/** English: the source language, complete by definition. */
export const DEFAULT_LOCALE = 'en';

const PLURAL_SUFFIX = new RegExp(`_(${PLURAL_CATEGORIES.join('|')})$`);
const PLACEHOLDER = /\{([A-Za-z_][A-Za-z0-9_]*)\}/g;
const TAG = /<(\/?)([A-Za-z][A-Za-z0-9]*)\s*(\/?)>/g;

/**
 * Flattens a language file (objects nested by namespace, strings as leaves) to dotted keys. Properties whose name
 * starts with `_` (`_meta`, notes) and values that are not strings are skipped.
 */
export function flattenMessages(tree: unknown, prefix = '', into: Record<string, string> = {}): Record<string, string> {
  if (tree === null || typeof tree !== 'object' || Array.isArray(tree)) return into;
  for (const [name, value] of Object.entries(tree)) {
    if (name.startsWith('_')) continue;
    const key = prefix + name;
    if (typeof value === 'string') into[key] = value;
    else if (value !== null && typeof value === 'object' && !Array.isArray(value)) flattenMessages(value, `${key}.`, into);
  }
  return into;
}

/** `library.gameCount_one` → `library.gameCount`; other keys are returned as they are. */
export const baseKey = (key: string): string => key.replace(PLURAL_SUFFIX, '');

/** The plural suffix of a key (`one` for `x_one`), or undefined. */
export const pluralSuffix = (key: string): string | undefined => PLURAL_SUFFIX.exec(key)?.[1];

const intlLocales = new Map<string, string>();

/**
 * The tag to give Intl for `locale`: the tag itself when Intl knows its language (`fr-CA`, `en-XA` via `en`), else
 * English. Given a tag it doesn't know, Intl silently uses the runtime's own locale (the Windows language), so a
 * language file with an unusual tag would get, say, French lists and plural rules; C# uses the English rules too.
 */
export function intlLocale(locale: string): string {
  let tag = intlLocales.get(locale);
  if (tag === undefined) {
    try {
      tag = Intl.PluralRules.supportedLocalesOf([locale]).length ? locale : DEFAULT_LOCALE;
    } catch {
      tag = DEFAULT_LOCALE; // not a valid language tag
    }
    intlLocales.set(locale, tag);
  }
  return tag;
}

const pluralRules = new Map<string, Intl.PluralRules>();

/** CLDR category of `count` in `locale` (Intl.PluralRules; the C# PluralRules gives the same answers). */
export function pluralCategory(locale: string, count: number): Intl.LDMLPluralRule {
  let rules = pluralRules.get(locale);
  if (!rules) {
    rules = new Intl.PluralRules(intlLocale(locale));
    pluralRules.set(locale, rules);
  }
  return rules.select(count);
}

/** Keys to try for `key`, most specific first: with a numeric count, `key_zero` (0 only), `key_<category>`, `key_other`, `key`. */
export function candidateKeys(key: string, params: Params | undefined, locale: string): string[] {
  const count = params?.count;
  if (typeof count === 'number' && Number.isFinite(count)) {
    const keys = count === 0 ? [`${key}_zero`] : [];
    keys.push(`${key}_${pluralCategory(locale, count)}`, `${key}_other`, key);
    return keys;
  }
  return [key, `${key}_other`];
}

/** The template of `key` in `messages`, or undefined. */
export function findMessage(messages: Messages, key: string, params: Params | undefined, locale: string): string | undefined {
  for (const candidate of candidateKeys(key, params, locale)) {
    const value = messages[candidate];
    if (value !== undefined) return value;
  }
  return undefined;
}

/** Names of the placeholders of a template, in order, without duplicates. */
export function placeholdersOf(template: string): string[] {
  return [...new Set(Array.from(template.matchAll(PLACEHOLDER), (m) => m[1]!))];
}

/** Names of the tags of a template (`<kbd>`, `<b>`, `<br/>`), sorted, with duplicates: for comparing translations. */
export function tagsOf(template: string): string[] {
  return Array.from(template.matchAll(TAG), (m) => `${m[1]}${m[2]}${m[3]}`).sort();
}

/**
 * Replaces the placeholders of `template` with `format(value)`; a placeholder without a value stays as it is, so a
 * missing argument shows instead of disappearing.
 */
export function interpolate(template: string, params: Params | undefined, format: (value: ParamValue) => string): string {
  if (!params || !template.includes('{')) return template;
  return template.replace(PLACEHOLDER, (whole, name: string) => {
    const value = params[name];
    return value === undefined ? whole : format(value);
  });
}

/** The text without its tags: "Press <kbd>End</kbd>" → "Press End", `<br/>` → a line break. */
export function stripTags(text: string): string {
  return text.includes('<') ? text.replace(TAG, (_whole, _close, name: string) => (name.toLowerCase() === 'br' ? '\n' : '')) : text;
}

/** A piece of rich text: plain text, or a tag around more pieces (`<br/>` has none). */
export type RichNode = string | { tag: string; children: RichNode[] };

/**
 * Parses the tags of a template: `Press <kbd>{key}</kbd> to lock` → ['Press ', { tag: 'kbd', children: ['{key}'] },
 * ' to lock']. Unbalanced tags are kept as text, never as markup. Placeholders stay in the text pieces: values are
 * substituted after parsing, so a value can never inject a tag.
 */
export function parseRich(template: string): RichNode[] {
  const root: { tag: string; children: RichNode[] } = { tag: '', children: [] };
  const stack = [root];
  let last = 0;
  for (const match of template.matchAll(TAG)) {
    const [whole, close, name, selfClosing] = match as unknown as [string, string, string, string];
    const top = stack[stack.length - 1]!;
    if (match.index > last) top.children.push(template.slice(last, match.index));
    last = match.index + whole.length;
    if (selfClosing) top.children.push({ tag: name, children: [] });
    else if (!close) {
      const node = { tag: name, children: [] as RichNode[] };
      top.children.push(node);
      stack.push(node);
    } else if (stack.length > 1 && top.tag === name) stack.pop();
    else top.children.push(whole);
  }
  if (last < template.length) stack[stack.length - 1]!.children.push(template.slice(last));
  // Tags left open: their content stays, as text.
  while (stack.length > 1) {
    const open = stack.pop()!;
    const parent = stack[stack.length - 1]!;
    const at = parent.children.indexOf(open);
    parent.children.splice(at, 1, `<${open.tag}>`, ...open.children);
  }
  return root.children;
}

/**
 * The available locale that best matches the preferred ones (Windows or browser languages, most preferred first): an
 * exact match, else a parent tag (`zh-Hant` for `zh-Hant-TW`, then `zh`), else another region of the same language
 * (`pt-BR` for `pt-PT`), else the next preferred one, else English. Same rules as the C# LanguageDetector.
 */
export function matchLocale(preferred: readonly string[], available: readonly string[], fallback = DEFAULT_LOCALE): string {
  const find = (tag: string) => available.find((l) => l.toLowerCase() === tag.toLowerCase());
  const language = (tag: string) => tag.split('-')[0]!.toLowerCase();
  for (const raw of preferred) {
    const wanted = raw.trim().replace(/_/g, '-');
    if (!wanted) continue;
    for (let tag = wanted; tag; tag = tag.includes('-') ? tag.slice(0, tag.lastIndexOf('-')) : '') {
      const exact = find(tag);
      if (exact) return exact;
    }
    const sibling = available
      .filter((l) => language(l) === language(wanted))
      .sort((a, b) => a.length - b.length || a.localeCompare(b))[0];
    if (sibling) return sibling;
  }
  return find(fallback) ?? fallback;
}

/** What a translator needs: its language, the strings in it, English for the keys it lacks. */
export interface TranslatorOptions {
  locale: string;
  messages: Messages;
  /** English, the per-key fallback. */
  fallback: Messages;
  /** Formats placeholder values. Default: numbers with Intl.NumberFormat, strings as they are. */
  formatValue?: (value: ParamValue) => string;
  /** Called once per key found in neither language. */
  onMissing?: (key: string) => void;
}

/**
 * A bound `t`: the string of a key in the language, English when the language lacks it, else the key itself. `t`
 * returns plain text (tags removed); rich text goes through the `<T>` component.
 */
export interface Translator {
  readonly locale: string;
  t(key: string, params?: Params): string;
  /** The template of a key (language, then English), placeholders and tags untouched, or undefined. */
  template(key: string, params?: Params): string | undefined;
  has(key: string): boolean;
}

export function createTranslator({ locale, messages, fallback, formatValue, onMissing }: TranslatorOptions): Translator {
  const numbers = new Intl.NumberFormat(intlLocale(locale), { maximumFractionDigits: 3 });
  const format = formatValue ?? ((value: ParamValue) => (typeof value === 'number' ? numbers.format(value) : value));
  const reported = new Set<string>();
  const template = (key: string, params?: Params) =>
    findMessage(messages, key, params, locale) ?? (messages === fallback ? undefined : findMessage(fallback, key, params, DEFAULT_LOCALE));
  return {
    locale,
    template,
    has: (key) => template(key) !== undefined,
    t(key, params) {
      const found = template(key, params);
      if (found === undefined) {
        if (!reported.has(key)) {
          reported.add(key);
          onMissing?.(key);
        }
        return key;
      }
      // Tags go before the values are in: a value is never read as markup.
      return interpolate(stripTags(found), params, format);
    },
  };
}
