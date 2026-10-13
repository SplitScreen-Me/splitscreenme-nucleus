#!/usr/bin/env node
// Language files and translations (docs/webui/i18n.md). Run with Node's type stripping (Node 22.18+ / 23.6+):
//
//   node scripts/i18n.ts gen      npm run i18n:gen    src/i18n/generated/keys.ts from Master/i18n/en.json
//   node scripts/i18n.ts check    npm run i18n:check  fails on a stale keys.ts, a malformed en.json, a translation with
//                                                     unknown keys, other placeholders / tags than English or other plural
//                                                     forms than its language's, a C# T("…") key missing from en.json, a
//                                                     language without a rule in the C# PluralRules
//   node scripts/i18n.ts lint     npm run i18n:lint   hard-coded user-visible strings in src, counted by folder
//        [--list] [--max=N] [--json] [paths...]       --list prints each one; --max fails above N; paths filter (src/shell)
import { existsSync, mkdirSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parseSync } from 'vite';
import { analyzeEnglish, checkCsharpPluralRule, checkTranslation, csharpKeys, csharpPluralLocales, generateKeysModule, type KeyInfo, type Problem } from './i18n/catalog.ts';
import { groupOf, lintProgram, type AstNode, type Finding } from './i18n/lint.ts';

const webui = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const master = resolve(webui, '..');
const i18nDir = join(master, 'i18n');
const englishFile = join(i18nDir, 'en.json');
const keysFile = join(webui, 'src/i18n/generated/keys.ts');
const pluralRulesFile = join(master, 'NucleusGaming/Localization/PluralRules.cs');

const [command, ...args] = process.argv.slice(2);
if (command === 'gen') gen();
else if (command === 'check') check();
else if (command === 'lint') lint(args);
else {
  console.error('Usage: node scripts/i18n.ts gen | check | lint [--list] [--max=N] [--json] [paths...]');
  process.exit(2);
}

function readJson(path: string): { tree?: unknown; problem?: Problem } {
  try {
    return { tree: JSON.parse(readFileSync(path, 'utf8').replace(/^﻿/, '')) };
  } catch (e) {
    return { problem: { file: relative(i18nDir, path), key: '', message: `is not valid JSON: ${(e as Error).message}` } };
  }
}

function english(): { keys: Map<string, KeyInfo>; problems: Problem[] } {
  const { tree, problem } = readJson(englishFile);
  if (problem) return { keys: new Map(), problems: [problem] };
  return analyzeEnglish(tree);
}

function sameText(a: string, b: string): boolean {
  return a.replace(/\r\n/g, '\n') === b.replace(/\r\n/g, '\n');
}

function printProblems(problems: readonly Problem[]): void {
  for (const p of problems) console.error(`  ${p.file}${p.key ? ` ${p.key}` : ''}: ${p.message}`);
}

function gen(): void {
  const { keys, problems } = english();
  if (problems.length > 0) {
    console.error('en.json has problems:');
    printProblems(problems);
    process.exit(1);
  }
  const content = generateKeysModule(keys);
  const old = existsSync(keysFile) ? readFileSync(keysFile, 'utf8') : '';
  if (sameText(old, content)) {
    console.log(`${relative(webui, keysFile)} is up to date (${keys.size} keys).`);
    return;
  }
  mkdirSync(dirname(keysFile), { recursive: true });
  writeFileSync(keysFile, content);
  console.log(`Wrote ${relative(webui, keysFile)}: ${keys.size} keys.`);
}

function check(): void {
  const problems: Problem[] = [];
  const { keys, problems: englishProblems } = english();
  problems.push(...englishProblems);

  if (englishProblems.length === 0) {
    const old = existsSync(keysFile) ? readFileSync(keysFile, 'utf8') : '';
    if (!sameText(old, generateKeysModule(keys))) {
      problems.push({ file: relative(webui, keysFile), key: '', message: 'is stale: run npm run i18n:gen.' });
    }
  }

  const coverage: string[] = [];
  const base = new Set(keys.keys());
  // Plural forms of the host texts: the C# PluralRules must know each language, as Intl does for the UI.
  const csharpPlurals = existsSync(pluralRulesFile) ? csharpPluralLocales(readFileSync(pluralRulesFile, 'utf8')) : undefined;
  if (!csharpPlurals) problems.push({ file: relative(master, pluralRulesFile), key: '', message: 'is missing: the language files cannot be checked against the C# plural rules.' });
  for (const name of readdirSync(i18nDir).filter((f) => f.endsWith('.json') && f !== 'en.json').sort()) {
    const { tree, problem } = readJson(join(i18nDir, name));
    if (problem) {
      problems.push(problem);
      continue;
    }
    const result = checkTranslation(name, tree, keys);
    problems.push(...result.problems);
    if (csharpPlurals) problems.push(...checkCsharpPluralRule(name, csharpPlurals));
    coverage.push(`  ${name.padEnd(12)} ${result.translated}/${base.size} strings (${Math.floor((result.translated / Math.max(1, base.size)) * 100)}%)`);
  }

  // C# texts: every literal key of T("…") / TPlain("…") must exist in en.json.
  let csharpUses = 0;
  for (const file of csharpFiles(master)) {
    for (const key of csharpKeys(readFileSync(file, 'utf8'))) {
      csharpUses++;
      if (!base.has(key)) problems.push({ file: relative(master, file), key, message: 'is used by C# code but not in en.json.' });
    }
  }

  console.log(`en.json: ${keys.size} strings; C#: ${csharpUses} literal keys checked.`);
  if (coverage.length > 0) console.log(['Translations:', ...coverage].join('\n'));
  if (problems.length > 0) {
    console.error(`i18n:check found ${problems.length} problem(s):`);
    printProblems(problems);
    process.exit(1);
  }
  console.log('i18n:check passed.');
}

function* csharpFiles(dir: string): Generator<string> {
  const skip = new Set(['bin', 'obj', 'packages', 'ThirdParty', 'node_modules', '.vs', 'Nucleus.WebUI', 'i18n']);
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const path = join(dir, entry.name);
    if (entry.isDirectory()) {
      if (!skip.has(entry.name) && !entry.name.endsWith('.Tests')) yield* csharpFiles(path);
    } else if (entry.name.endsWith('.cs')) yield path;
  }
}

function* sourceFiles(dir: string): Generator<string> {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const path = join(dir, entry.name);
    const rel = relative(webui, path).replace(/\\/g, '/');
    if (entry.isDirectory()) {
      // The bridge (runtime, generated contract, dev-only mock host) and the i18n runtime have no UI text of their own.
      if (['src/bridge', 'src/i18n'].includes(rel) || entry.name === '__tests__' || entry.name === 'generated') continue;
      yield* sourceFiles(path);
    } else if (/\.tsx?$/.test(entry.name) && !/\.(test|d)\.tsx?$/.test(entry.name)) yield path;
  }
}

function lint(options: string[]): void {
  const list = options.includes('--list');
  const json = options.includes('--json');
  const max = Number(options.find((o) => o.startsWith('--max='))?.slice(6) ?? Number.NaN);
  const filters = options.filter((o) => !o.startsWith('--')).map((f) => f.replace(/\\/g, '/').replace(/\/$/, ''));

  const findings: Finding[] = [];
  let files = 0;
  for (const path of sourceFiles(join(webui, 'src'))) {
    const rel = relative(webui, path).replace(/\\/g, '/');
    if (filters.length > 0 && !filters.some((f) => rel === f || rel.startsWith(`${f}/`))) continue;
    files++;
    const source = readFileSync(path, 'utf8');
    const parsed = parseSync(rel, source, { lang: rel.endsWith('x') ? 'tsx' : 'ts' });
    if (parsed.errors.length > 0) {
      console.warn(`  ${rel}: not parsed (${parsed.errors[0]?.message}), skipped`);
      continue;
    }
    findings.push(...lintProgram(parsed.program as unknown as AstNode, source, rel));
  }

  if (json) {
    console.log(JSON.stringify(findings, null, 2));
  } else {
    if (list) {
      for (const f of findings) console.log(`${f.file}:${f.line}:${f.column}  ${f.kind}  "${f.text.length > 90 ? `${f.text.slice(0, 87)}...` : f.text}"`);
      if (findings.length > 0) console.log('');
    }
    const counts = new Map<string, { strings: number; files: Set<string> }>();
    for (const f of findings) {
      const group = groupOf(f.file);
      const entry = counts.get(group) ?? { strings: 0, files: new Set<string>() };
      entry.strings++;
      entry.files.add(f.file);
      counts.set(group, entry);
    }
    console.log('Hard-coded user-visible strings (use t() / <T>, or an i18n-ignore comment saying why):');
    for (const [group, { strings, files: inFiles }] of [...counts].sort(([a], [b]) => a.localeCompare(b))) {
      console.log(`  ${group.padEnd(22)} ${String(strings).padStart(5)}  (${inFiles.size} file${inFiles.size === 1 ? '' : 's'})`);
    }
    console.log(`  ${'total'.padEnd(22)} ${String(findings.length).padStart(5)}  (${files} files scanned)`);
    if (!list && findings.length > 0) console.log('List them: npm run i18n:lint -- --list [src/folder]');
  }
  if (Number.isFinite(max) && findings.length > max) {
    console.error(`i18n:lint: ${findings.length} strings, more than --max=${max}.`);
    process.exit(1);
  }
}
