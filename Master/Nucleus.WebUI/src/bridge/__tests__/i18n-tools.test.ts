// The language file checks and the generated keys (scripts/i18n/catalog.ts, npm run i18n:gen / i18n:check) and the
// hard-coded string finder (scripts/i18n/lint.ts, npm run i18n:lint).
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import { parseSync } from 'vite';
import { analyzeEnglish, checkTranslation, csharpKeys, generateKeysModule } from '../../../scripts/i18n/catalog.ts';
import { groupOf, isUserText, lintProgram, type AstNode } from '../../../scripts/i18n/lint.ts';

const ENGLISH = {
  _meta: { locale: 'en', englishName: 'English', nativeName: 'English' },
  common: { ok: 'OK' },
  library: {
    gameCount_one: '{count} game',
    gameCount_other: '{count} games',
    hello: 'Hello {name}',
    lockHint: 'Press <kbd>{key}</kbd> to lock',
  },
};

const french = (extra: Record<string, unknown>, meta: Record<string, unknown> = { locale: 'fr', englishName: 'French', nativeName: 'Français' }) => ({
  _meta: meta,
  ...extra,
});

test('the real en.json is valid and keys.ts is up to date', () => {
  const tree = JSON.parse(readFileSync(new URL('../../../../i18n/en.json', import.meta.url), 'utf8')) as unknown;
  const { keys, problems } = analyzeEnglish(tree);
  assert.deepEqual(problems, []);
  const generated = readFileSync(new URL('../../i18n/generated/keys.ts', import.meta.url), 'utf8').replace(/\r\n/g, '\n');
  assert.equal(generated, generateKeysModule(keys), 'run npm run i18n:gen');
});

test('English keys: placeholders, plural forms and tags become the types of t()', () => {
  const { keys, problems } = analyzeEnglish(ENGLISH);
  assert.deepEqual(problems, []);
  assert.deepEqual(keys.get('library.gameCount'), { placeholders: ['count'], plural: true, rich: false, tags: [], text: '{count} games' });
  const module = generateKeysModule(keys);
  assert.match(module, /'common\.ok': undefined;/);
  assert.match(module, /'library\.gameCount': \{ count: number \};/);
  assert.match(module, /'library\.hello': \{ name: ParamValue \};/);
  assert.match(module, /export type RichMessageKey = 'library\.lockHint';/);
});

test('English problems: names, plural forms, values, tags, _meta', () => {
  const { problems } = analyzeEnglish({
    _meta: { locale: 'en', englishName: 'English' },
    Common: { ok: 'OK' },
    a: { b_one: 'one', b: 'plain', c_one: 'no other', bad: 3, tag: 'a <b>bold' },
  });
  const messages = problems.map((p) => `${p.key}: ${p.message}`);
  assert.ok(messages.some((m) => m.startsWith('Common: ') && m.includes('namespace')));
  assert.ok(messages.some((m) => m.startsWith('a.b: ') && m.includes('both a plain string and a plural')));
  assert.ok(messages.some((m) => m.startsWith('a.c: ') && m.includes('c_other')));
  assert.ok(messages.some((m) => m.startsWith('a.bad: ')));
  assert.ok(messages.some((m) => m.startsWith('a.tag: ') && m.includes('unbalanced')));
  assert.ok(messages.some((m) => m.startsWith('_meta.nativeName: ')));
});

test('translations: unknown keys, plural misuse, placeholders and tags must follow English; missing keys are fine', () => {
  const { keys } = analyzeEnglish(ENGLISH);
  const ok = checkTranslation('fr.json', french({ common: { ok: "D'accord" }, library: { gameCount_one: 'Un jeu', gameCount_many: '{count} de jeux', gameCount_other: '{count} jeux' } }), keys);
  assert.deepEqual(ok.problems, []);
  assert.equal(ok.translated, 2);

  const bad = checkTranslation(
    'fr.json',
    french(
      {
        common: { ok: 'OK', nope: 'x' },
        library: { gameCount: 'jeux', hello: 'Bonjour {nom}', lockHint: 'Appuyez sur {key} pour verrouiller' },
      },
      { locale: 'de', englishName: 'French' },
    ),
    keys,
  );
  const messages = bad.problems.map((p) => `${p.key}: ${p.message}`);
  assert.ok(messages.some((m) => m.startsWith('common.nope: ') && m.includes('not in en.json')));
  assert.ok(messages.some((m) => m.startsWith('library.gameCount: ') && m.includes('plural string in English')));
  assert.ok(messages.some((m) => m.startsWith('library.hello: ') && m.includes('{nom}')));
  assert.ok(messages.some((m) => m.startsWith('library.hello: ') && m.includes('drops {name}')));
  assert.ok(messages.some((m) => m.startsWith('library.lockHint: ') && m.includes('same tags')));
  assert.ok(messages.some((m) => m.startsWith('_meta.locale: ')));
  assert.ok(messages.some((m) => m.startsWith('_meta.nativeName: ')));
});

test('C# keys are read from T("…") and TPlain("…") calls, not from comments', () => {
  const source = [
    'using static Nucleus.Gaming.Localization.I18n;',
    '/// <code>T("doc.example")</code>',
    'var a = T("engine.a.title");',
    'var b = I18n.TPlain( "engine.b", new { file });',
    'var c = T(dynamicKey);',
  ].join('\n');
  assert.deepEqual(csharpKeys(source), ['engine.a.title', 'engine.b']);
  assert.deepEqual(csharpKeys('var a = T("x.y");'), [], 'files that do not use I18n are skipped');
});

function lint(source: string, file = 'src/screens/demo/Demo.tsx') {
  const parsed = parseSync(file, source, { lang: 'tsx' });
  assert.deepEqual(parsed.errors, []);
  return lintProgram(parsed.program as unknown as AstNode, source, file).map((f) => `${f.line} ${f.kind}: ${f.text}`);
}

test('the linter finds JSX text, text props, text properties, toasts and document.title', () => {
  const source = [
    "export function Demo({ n, on }: { n: number; on: boolean }) {",
    "  toast('Saved the profile');",
    "  document.title = `${n} games`;",
    "  useScreenChrome([{ label: 'Settings', button: 'A' }]);",
    '  return (',
    '    <div className={on ? "grid gap-2" : "hidden"} title="Your library" data-id="x">',
    '      Welcome back',
    "      {on ? 'Yes' : 'No'}",
    '      <input placeholder={`Search ${n} games`} aria-label="Search" />',
    '      <Kbd>Ctrl K</Kbd>',
    '      <span>{n}</span>',
    '      <b>Nucleus Co-op</b>',
    '      <i>·</i>',
    '      {t("common.ok")}',
    '    </div>',
    '  );',
    '}',
  ].join('\n');
  assert.deepEqual(lint(source), [
    '2 call toast: Saved the profile',
    '3 assign title: {…} games',
    '4 property label: Settings',
    '6 attribute title: Your library',
    '7 jsx-text: Welcome back',
    '8 jsx-expression: Yes',
    '8 jsx-expression: No',
    '9 attribute placeholder: Search {…} games',
    '9 attribute aria-label: Search',
  ]);
});

test('the linter skips lines marked i18n-ignore and files marked i18n-ignore-file', () => {
  const source = [
    'export const A = () => (',
    '  <p>',
    '    {/* i18n-ignore: sample data shown as is */}',
    '    Lorem ipsum',
    '    <span title="Kept">Shown</span> {/* i18n-ignore */}',
    '  </p>',
    ');',
  ].join('\n');
  assert.deepEqual(lint(source), []);
  assert.deepEqual(lint(`// i18n-ignore-file: developer screen\nexport const B = () => <p>Debug</p>;`), []);
});

test('user text: words, not keys, ids, URLs, brands or punctuation', () => {
  assert.equal(isUserText('Save'), true);
  assert.equal(isUserText('Open Steam'), true);
  assert.equal(isUserText('Steam'), false);
  assert.equal(isUserText('Nucleus Co-op'), false);
  assert.equal(isUserText('common.ok'), false);
  assert.equal(isUserText('set-general'), false);
  assert.equal(isUserText('https://v3.splitscreen.me'), false);
  assert.equal(isUserText(' · '), false);
  assert.equal(isUserText('42'), false);
  assert.equal(groupOf('src/screens/library/YourGames.tsx'), 'screens/library');
  assert.equal(groupOf('src/shell/TitleBar.tsx'), 'shell');
  assert.equal(groupOf('src/App.tsx'), '(root)');
});
