// The translation engine (src/i18n/core.ts) and the formatters (src/i18n/format.ts): lookups with the English fallback,
// plural forms, placeholders, rich text, language matching. The C# side (Nucleus.Gaming.Localization) has the same
// rules; the plural cases are shared with its tests (fixtures/plurals.json).
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import {
  candidateKeys,
  createTranslator,
  flattenMessages,
  interpolate,
  intlLocale,
  matchLocale,
  parseRich,
  pluralCategory,
  stripTags,
  type Messages,
} from '../../i18n/core.ts';
import { english } from '../../i18n/english.ts';
import { createFormatters } from '../../i18n/format.ts';

const EN: Messages = flattenMessages({
  _meta: { locale: 'en' },
  common: { ok: 'OK', cancel: 'Cancel' },
  library: {
    gameCount_zero: 'No games',
    gameCount_one: '{count} game',
    gameCount_other: '{count} games',
    hello: 'Hello {name}',
    lockHint: 'Press <kbd>{key}</kbd> to lock',
  },
});

const FR: Messages = flattenMessages({
  _meta: { locale: 'fr' },
  _note: 'ignored',
  common: { ok: "D'accord", count: 3 },
  library: { gameCount_one: '{count} jeu', gameCount_other: '{count} jeux', hello: 'Bonjour {name}' },
});

test('language files flatten to dotted keys, without _meta, notes or values that are not strings', () => {
  assert.deepEqual(Object.keys(FR).sort(), ['common.ok', 'library.gameCount_one', 'library.gameCount_other', 'library.hello']);
  assert.equal(english['common.ok'], 'OK');
  assert.equal(english._meta, undefined);
});

test('the language in use wins, English fills the gaps, unknown keys come back as they are (reported once)', () => {
  const missing: string[] = [];
  const fr = createTranslator({ locale: 'fr', messages: FR, fallback: EN, onMissing: (k) => missing.push(k) });

  assert.equal(fr.t('common.ok'), "D'accord");
  assert.equal(fr.t('common.cancel'), 'Cancel');
  assert.equal(fr.t('library.nope'), 'library.nope');
  assert.equal(fr.t('library.nope'), 'library.nope');
  assert.deepEqual(missing, ['library.nope']);
  assert.equal(fr.has('common.cancel'), true);
  assert.equal(fr.has('library.gameCount'), true);
  assert.equal(fr.has('library.nope'), false);
});

test('count picks the plural form: _zero for 0 when it exists, then the CLDR category of the language', () => {
  const en = createTranslator({ locale: 'en', messages: EN, fallback: EN });
  const fr = createTranslator({ locale: 'fr', messages: FR, fallback: EN });

  assert.equal(en.t('library.gameCount', { count: 0 }), 'No games');
  assert.equal(en.t('library.gameCount', { count: 1 }), '1 game');
  assert.equal(en.t('library.gameCount', { count: 1234 }), '1,234 games');
  assert.equal(fr.t('library.gameCount', { count: 0 }), '0 jeu');
  assert.equal(fr.t('library.gameCount', { count: 1 }), '1 jeu');
  assert.equal(fr.t('library.gameCount', { count: 1234 }), `${new Intl.NumberFormat('fr').format(1234)} jeux`);
  assert.deepEqual(candidateKeys('x', { count: 0 }, 'en'), ['x_zero', 'x_other', 'x_other', 'x']);
  assert.deepEqual(candidateKeys('x', undefined, 'en'), ['x', 'x_other']);
});

test('plural categories match the cases shared with the C# PluralRules', () => {
  const fixture = JSON.parse(readFileSync(new URL('./fixtures/plurals.json', import.meta.url), 'utf8')) as Record<string, Record<string, string>>;
  for (const [locale, cases] of Object.entries(fixture)) {
    if (locale.startsWith('_')) continue;
    for (const [number, expected] of Object.entries(cases)) {
      assert.equal(pluralCategory(locale, Number(number)), expected, `${locale} ${number}`);
    }
  }
});

test('placeholders: values in, missing ones kept, numbers formatted, strings untouched', () => {
  const format = (v: string | number) => (typeof v === 'number' ? `#${v}` : v);
  assert.equal(interpolate('Hello {name}, {n} new', { name: 'Léa', n: 3 }, format), 'Hello Léa, #3 new');
  assert.equal(interpolate('Hello {name}', {}, format), 'Hello {name}');
  assert.equal(interpolate('Literal { braces } and {not-a-name}', { name: 'x' }, format), 'Literal { braces } and {not-a-name}');
});

test('t() returns plain text: tags dropped from the template, values never read as markup', () => {
  const en = createTranslator({ locale: 'en', messages: EN, fallback: EN });
  assert.equal(en.t('library.lockHint', { key: 'End' }), 'Press End to lock');
  assert.equal(en.t('library.hello', { name: '<b>Max</b>' }), 'Hello <b>Max</b>');
  assert.equal(stripTags('a<br/>b <i>c</i>'), 'a\nb c');
});

test('rich text parses into nested tags; unbalanced tags stay text', () => {
  assert.deepEqual(parseRich('Press <kbd>{key}</kbd> to <b>lock <i>now</i></b><br/>!'), [
    'Press ',
    { tag: 'kbd', children: ['{key}'] },
    ' to ',
    { tag: 'b', children: ['lock ', { tag: 'i', children: ['now'] }] },
    { tag: 'br', children: [] },
    '!',
  ]);
  assert.deepEqual(parseRich('a <b>b'), ['a ', '<b>', 'b']);
  assert.deepEqual(parseRich('a </b> c'), ['a ', '</b>', ' c']);
  assert.deepEqual(parseRich('1 < 2 > 0'), ['1 < 2 > 0']);
});

test('language matching: exact, parent tag, same language, next preferred, English', () => {
  const available = ['en', 'fr', 'pt-BR', 'zh-Hant', 'de'];
  const cases: [string[], string][] = [
    [['fr-FR'], 'fr'],
    [['PT-br'], 'pt-BR'],
    [['pt-PT'], 'pt-BR'],
    [['zh-Hant-TW'], 'zh-Hant'],
    [['de_AT'], 'de'],
    [['ja-JP'], 'en'],
    [['ja-JP', 'de-CH', 'fr'], 'de'],
    [[], 'en'],
  ];
  for (const [preferred, expected] of cases) assert.equal(matchLocale(preferred, available), expected, preferred.join(','));
  assert.equal(matchLocale(['pt-AO'], ['en', 'pt-BR', 'pt']), 'pt');
});

test('formatters follow the language', () => {
  const phrases = (key: string, params?: Record<string, string | number>) => (key === 'common.time.justNow' ? 'just now' : `in ${params?.month}`);
  const en = createFormatters('en', phrases);
  const fr = createFormatters('fr', phrases);

  assert.equal(en.compact(145563), '146K');
  assert.equal(en.compact(6077633), '6.1M');
  assert.equal(en.number(1234.5), '1,234.5');
  assert.equal(fr.number(1234.5), new Intl.NumberFormat('fr').format(1234.5));
  assert.equal(en.bytes(1536), '1.5 kB');
  assert.equal(en.bytes(734003200), '700 MB');
  assert.equal(en.bytes(12), '12 byte');
  assert.equal(en.percent(0.42), '42%');
  assert.equal(en.duration(8040), '2 hr 14 min');
  assert.equal(en.duration(7200), '2 hr');
  assert.equal(en.duration(10), '1 min');
  assert.match(fr.duration(8040), /^2\sh 14\smin$/);
  assert.equal(en.list(['Rémi', 'Léa', 'Max']), 'Rémi, Léa, and Max');
  assert.equal(fr.list(['Rémi', 'Léa', 'Max']), 'Rémi, Léa et Max');
  assert.equal(en.clock(3_723_000), '1:02:03');
  assert.equal(en.date('2024-07-31T12:00:00Z', 'medium'), 'Jul 31, 2024');
  assert.equal(en.date(undefined), '');
  assert.equal(en.date('not a date'), '');

  const now = new Date(2026, 9, 13, 12);
  const daysAgo = (n: number) => new Date(2026, 9, 13 - n, 9).toISOString();
  assert.equal(en.when(daysAgo(0), now), 'today');
  assert.equal(en.when(daysAgo(1), now), 'yesterday');
  assert.equal(en.when(daysAgo(3), now), '3 days ago');
  assert.equal(en.when(daysAgo(8), now), 'last week');
  assert.equal(en.when(daysAgo(15), now), '2 weeks ago');
  assert.equal(en.when(new Date(2026, 8, 1).toISOString(), now), 'in September');
  assert.equal(en.when(new Date(2025, 2, 1).toISOString(), now), 'in March 2025');
  assert.equal(fr.when(daysAgo(1), now), 'hier');

  const t = now.getTime();
  assert.equal(en.ago(new Date(t - 30_000).toISOString(), t), 'just now');
  assert.equal(en.ago(new Date(t - 5 * 60_000).toISOString(), t), '5 minutes ago');
  assert.equal(en.ago(new Date(t - 3 * 864e5).toISOString(), t), '3 days ago');
});

test('a tag Intl does not know formats and pluralizes as English, not as the runtime language', () => {
  assert.equal(intlLocale('fr-CA'), 'fr-CA');
  assert.equal(intlLocale('en-XA'), 'en-XA');
  assert.equal(intlLocale('xx'), 'en');
  assert.equal(intlLocale('not a tag'), 'en');

  const xx = createFormatters('xx', () => '');
  assert.equal(xx.locale, 'xx');
  assert.equal(xx.list(['Steam', 'Epic']), 'Steam and Epic');
  assert.equal(xx.number(1234.5), '1,234.5');
  assert.equal(pluralCategory('xx', 0), 'other');
  assert.equal(pluralCategory('xx', 1), 'one');
  const t = createTranslator({ locale: 'xx', messages: {}, fallback: { n: '{value}' } });
  assert.equal(t.t('n', { value: 1234.5 }), '1,234.5');
});
