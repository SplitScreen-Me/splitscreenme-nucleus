// Rules of the language files (docs/webui/i18n.md), the typed keys module generated from en.json, and the checks of the
// other languages. Pure functions: scripts/i18n.ts reads and writes the files, the bridge tests call them directly.
import { baseKey, parseRich, placeholdersOf, pluralSuffix, PLURAL_CATEGORIES, tagsOf, type RichNode } from '../../src/i18n/core.ts';

/** One problem found in a language file. */
export interface Problem {
  /** File name, for example `fr.json`. */
  file: string;
  /** Dotted key, or empty for the whole file. */
  key: string;
  message: string;
}

/** What en.json says about a base key (plural forms merged). */
export interface KeyInfo {
  /** Placeholders of every form. */
  placeholders: string[];
  /** Has `_one` / `_other`... forms: takes a numeric `count`. */
  plural: boolean;
  /** Contains tags: render it with `<T>` to keep them. */
  rich: boolean;
  /** Tags of each form, sorted, to compare translations against. */
  tags: string[];
  /** English text (the `_other` form for plurals), for the generated doc comments. */
  text: string;
}

/** Key segments: camelCase, starting with a lowercase letter. */
const SEGMENT = /^[a-z][a-zA-Z0-9]*$/;

/** A language tag usable as a file name: `en`, `fr`, `pt-BR`, `zh-Hant`. */
export const LOCALE_TAG = /^[a-z]{2,3}(-[A-Za-z0-9]{2,8})*$/;

/** Strings of a parsed language file, and its structural problems (wrong names, values that are not strings...). */
export function readTree(file: string, tree: unknown): { strings: Map<string, string>; problems: Problem[] } {
  const strings = new Map<string, string>();
  const problems: Problem[] = [];
  const walk = (node: Record<string, unknown>, prefix: string) => {
    for (const [name, value] of Object.entries(node)) {
      if (name.startsWith('_')) continue;
      const key = prefix + name;
      if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
        if (!SEGMENT.test(name)) problems.push({ file, key, message: `"${name}" is not a valid namespace name (camelCase, starting with a lowercase letter).` });
        walk(value as Record<string, unknown>, `${key}.`);
      } else if (typeof value === 'string') {
        const suffix = pluralSuffix(name);
        const stem = suffix ? name.slice(0, -suffix.length - 1) : name;
        if (!SEGMENT.test(stem)) problems.push({ file, key, message: `"${name}" is not a valid key name (camelCase, plural forms as name_one, name_other...).` });
        strings.set(key, value);
      } else {
        problems.push({ file, key, message: `must be a string or an object, not ${Array.isArray(value) ? 'an array' : value === null ? 'null' : `a ${typeof value}`}.` });
      }
    }
  };
  if (tree === null || typeof tree !== 'object' || Array.isArray(tree)) problems.push({ file, key: '', message: 'must contain a JSON object.' });
  else walk(tree as Record<string, unknown>, '');
  return { strings, problems };
}

/** Problems of the `_meta` block: locale (= file name), englishName, nativeName, direction. */
export function checkMeta(file: string, tree: unknown): Problem[] {
  const locale = file.replace(/\.json$/, '');
  const meta = (tree as { _meta?: Record<string, unknown> } | null)?._meta;
  const problems: Problem[] = [];
  if (!LOCALE_TAG.test(locale)) problems.push({ file, key: '', message: `the file name must be a language tag, for example fr.json or pt-BR.json.` });
  if (!meta || typeof meta !== 'object') return [...problems, { file, key: '_meta', message: 'is missing: { "locale", "englishName", "nativeName" }.' }];
  if (meta.locale !== locale) problems.push({ file, key: '_meta.locale', message: `must be "${locale}", the file name.` });
  for (const field of ['englishName', 'nativeName'] as const) {
    if (typeof meta[field] !== 'string' || !(meta[field] as string).trim()) problems.push({ file, key: `_meta.${field}`, message: 'is required.' });
  }
  if (meta.direction !== undefined && meta.direction !== 'ltr' && meta.direction !== 'rtl') problems.push({ file, key: '_meta.direction', message: 'must be "ltr" or "rtl".' });
  return problems;
}

/** Problems of the tags of one string: unbalanced or unclosed tags. */
function tagProblems(file: string, key: string, text: string): Problem[] {
  const leftovers: string[] = [];
  const visit = (nodes: readonly RichNode[]) => {
    for (const node of nodes) {
      if (typeof node === 'string') {
        if (/<\/?[A-Za-z][A-Za-z0-9]*\s*\/?>/.test(node)) leftovers.push(node);
      } else visit(node.children);
    }
  };
  visit(parseRich(text));
  return leftovers.length > 0 ? [{ file, key, message: 'has unbalanced tags (each <tag> needs its </tag>; <br/> closes itself).' }] : [];
}

/** Base keys of en.json with their placeholders, plural forms and tags; problems of the English file itself. */
export function analyzeEnglish(tree: unknown, file = 'en.json'): { keys: Map<string, KeyInfo>; problems: Problem[] } {
  const { strings, problems } = readTree(file, tree);
  problems.push(...checkMeta(file, tree));
  const groups = new Map<string, Map<string, string>>();
  for (const [key, text] of strings) {
    const base = baseKey(key);
    const forms = groups.get(base) ?? new Map<string, string>();
    forms.set(pluralSuffix(key) ?? '', text);
    groups.set(base, forms);
    problems.push(...tagProblems(file, key, text));
  }

  const keys = new Map<string, KeyInfo>();
  for (const [base, forms] of [...groups].sort(([a], [b]) => (a < b ? -1 : a > b ? 1 : 0))) {
    const plural = [...forms.keys()].some((suffix) => suffix !== '');
    if (plural && forms.has('')) problems.push({ file, key: base, message: 'is both a plain string and a plural string: keep the plural forms only.' });
    if (plural && !forms.has('other')) problems.push({ file, key: base, message: `has plural forms but no ${base.split('.').pop()}_other.` });
    if ([...strings.keys()].some((k) => k.startsWith(`${base}.`))) problems.push({ file, key: base, message: 'is both a string and a namespace.' });
    const texts = [...forms.values()];
    const tagSets = new Set(texts.map((text) => tagsOf(text).join(' ')));
    if (tagSets.size > 1) problems.push({ file, key: base, message: 'plural forms must use the same tags.' });
    keys.set(base, {
      placeholders: [...new Set(texts.flatMap(placeholdersOf))].sort(),
      plural,
      rich: texts.some((text) => tagsOf(text).length > 0),
      tags: tagsOf(forms.get('other') ?? forms.get('') ?? texts[0]!),
      text: forms.get('other') ?? forms.get('') ?? texts[0]!,
    });
  }
  return { keys, problems };
}

/**
 * Problems of a translation against English: keys English doesn't have, plural forms on plain strings (or plain
 * strings for plural ones), placeholders English doesn't use (or English placeholders the translation drops), other
 * tags. Missing keys are not problems: they show in English. Also returns how many English keys it translates.
 */
export function checkTranslation(file: string, tree: unknown, english: Map<string, KeyInfo>): { problems: Problem[]; translated: number } {
  const { strings, problems } = readTree(file, tree);
  problems.push(...checkMeta(file, tree));
  const groups = new Map<string, Map<string, string>>();
  for (const [key, text] of strings) {
    const base = baseKey(key);
    const info = english.get(base);
    if (!info) {
      problems.push({ file, key, message: 'is not in en.json (removed or misspelled key).' });
      continue;
    }
    const suffix = pluralSuffix(key);
    if (info.plural && !suffix) problems.push({ file, key, message: `is a plural string in English: use ${key.split('.').pop()}_one, _other... (the forms of this language).` });
    if (!info.plural && suffix) problems.push({ file, key, message: `is not a plural string in English: use ${base.split('.').pop()} without _${suffix}.` });
    const forms = groups.get(base) ?? new Map<string, string>();
    forms.set(suffix ?? '', text);
    groups.set(base, forms);
    problems.push(...tagProblems(file, key, text));
  }

  for (const [base, forms] of groups) {
    const info = english.get(base)!;
    const allowed = new Set(info.plural ? [...info.placeholders, 'count'] : info.placeholders);
    const used = new Set<string>();
    for (const [suffix, text] of forms) {
      const key = suffix ? `${base}_${suffix}` : base;
      for (const name of placeholdersOf(text)) {
        used.add(name);
        if (!allowed.has(name)) problems.push({ file, key, message: `uses {${name}}, which English doesn't have.` });
      }
      if (tagsOf(text).join(' ') !== info.tags.join(' ')) problems.push({ file, key, message: `must use the same tags as English (${info.tags.join(' ') || 'none'}).` });
    }
    const missing = info.placeholders.filter((name) => !used.has(name) && !(info.plural && name === 'count'));
    if (missing.length > 0) problems.push({ file, key: base, message: `drops ${missing.map((n) => `{${n}}`).join(', ')}.` });
    if (info.plural && !forms.has('other')) problems.push({ file, key: base, message: `needs ${base.split('.').pop()}_other.` });
  }
  return { problems, translated: groups.size };
}

/** Plural categories, for messages. */
export const PLURAL_FORMS = PLURAL_CATEGORIES.join(', ');

/** The TypeScript module of the keys of en.json (src/i18n/generated/keys.ts). */
export function generateKeysModule(keys: Map<string, KeyInfo>): string {
  const lines = [
    '// Generated by `npm run i18n:gen` from Master/i18n/en.json: do not edit. Change en.json, then run it again;',
    '// `npm run i18n:check` fails while this file is stale. See docs/webui/i18n.md.',
    "import type { ParamValue } from '../core.ts';",
    '',
    '/** Placeholders of each string of en.json (`undefined`: none). Plural strings take a numeric `count`. */',
    'export interface MessageParamsMap {',
  ];
  for (const [key, info] of keys) {
    const params = info.placeholders.filter((name) => !(info.plural && name === 'count')).map((name) => `${name}: ParamValue`);
    if (info.plural) params.unshift('count: number');
    lines.push(`  /** ${docText(info.text)} */`);
    lines.push(`  '${key}': ${params.length ? `{ ${params.join('; ')} }` : 'undefined'};`);
  }
  lines.push('}', '');
  const rich = [...keys].filter(([, info]) => info.rich).map(([key]) => `'${key}'`);
  lines.push('/** Strings with tags (`<kbd>`, `<b>`...): render them with `<T>` to keep their markup; `t()` drops it. */');
  lines.push(`export type RichMessageKey = ${rich.length ? rich.join(' | ') : 'never'};`, '');
  return lines.join('\n');
}

function docText(text: string): string {
  const flat = text.replace(/\s+/g, ' ').replace(/\*\//g, '* /');
  return flat.length > 110 ? `${flat.slice(0, 107)}...` : flat;
}

/** Keys used by C# code with a literal key: `T("...")`, `TPlain("...")`, `I18n.T("...")` (comments and doc examples aside). */
export function csharpKeys(source: string): string[] {
  if (!/\bI18n\b/.test(source)) return [];
  const code = source
    .split('\n')
    .filter((line) => !line.trimStart().startsWith('//'))
    .join('\n');
  return Array.from(code.matchAll(/\bT(?:Plain)?\(\s*"([A-Za-z0-9_.]+)"/g), (m) => m[1]!);
}
