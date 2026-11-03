// The Nucleus markup of handler notes, option descriptions and notices (src/components/notes-markup.ts): the v3 forms,
// the legacy 2.4 / 2.5 `*<text>*` and `*<text|r,g,b>*` forms, links and plain URLs. Lives with the bridge tests so
// `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { isWebUrl, notesParagraphs, parseLinks, parseNotes, shortUrl, type NotesToken } from '../../components/notes-markup.ts';

const text = (t: string) => ({ kind: 'text', text: t }) as const;
const link = (label: string, href: string) => ({ kind: 'link', label, href }) as const;
const url = (href: string) => ({ kind: 'url', href }) as const;
const hl = (children: NotesToken[], colour?: readonly [number, number, number] | 'default'): NotesToken =>
  ({ kind: 'highlight', ...(colour === undefined ? {} : { colour }), children }) as NotesToken;

test('plain text is one text token', () => {
  assert.deepEqual(parseNotes('Press END to lock the inputs.'), [text('Press END to lock the inputs.')]);
  assert.deepEqual(parseNotes(''), []);
});

test('v3 emphasis and coloured text', () => {
  assert.deepEqual(parseNotes("just *don't touch the game's resolution*. Then play."), [
    text('just '),
    hl([text("don't touch the game's resolution")]),
    text('. Then play.'),
  ]);
  assert.deepEqual(parseNotes('*Warning|255,80,80* read this'), [hl([text('Warning')], [255, 80, 80]), text(' read this')]);
  assert.deepEqual(parseNotes('*Warning | 255 , 80 ,80*'), [hl([text('Warning')], [255, 80, 80])]);
  assert.deepEqual(parseNotes('*NEW* script'), [hl([text('NEW')]), text(' script')]);
});

test('legacy highlight in the default colour and in an author colour', () => {
  assert.deepEqual(parseNotes('Your notes, *<important text>*, more.'), [text('Your notes, '), hl([text('important text')], 'default'), text(', more.')]);
  assert.deepEqual(parseNotes('Your notes, *<colored text here|216,222,18>*, more text....'), [
    text('Your notes, '),
    hl([text('colored text here')], [216, 222, 18]),
    text(', more text....'),
  ]);
  assert.deepEqual(parseNotes('*<spaced| 1 , 2,3 >*'), [hl([text('spaced')], [1, 2, 3])]);
  // A legacy highlight may hold stars and span a line break (the legacy pattern only refuses brackets).
  assert.deepEqual(parseNotes('*<a*b\nc>*'), [hl([text('a*b\nc')], 'default')]);
});

test('a legacy part that is not a colour stays in the text, in the default colour', () => {
  assert.deepEqual(parseNotes('*<left|right>*'), [hl([text('left|right')], 'default')]);
  assert.deepEqual(parseNotes('*<a|1,2>*'), [hl([text('a|1,2')], 'default')]);
  assert.deepEqual(parseNotes('*<a|b|9,9,9>*'), [hl([text('a|b')], [9, 9, 9])]);
});

test('channels above 255 are read as 255, like the legacy renderer', () => {
  assert.deepEqual(parseNotes('*<hot|300,0,999999>*'), [hl([text('hot')], [255, 0, 255])]);
  assert.deepEqual(parseNotes('*hot|256,999,12*'), [hl([text('hot')], [255, 255, 12])]);
});

test('the legacy forms no longer show their brackets', () => {
  const tokens = parseNotes('*<colored|216,222,18>*');
  assert.equal(JSON.stringify(tokens).includes('<'), false);
});

test('lone stars stay as they are', () => {
  assert.deepEqual(parseNotes('2 * 3 * 4 = 24'), [text('2 * 3 * 4 = 24')]);
  assert.deepEqual(parseNotes('a * b'), [text('a * b')]);
  assert.deepEqual(parseNotes('ends with a star*'), [text('ends with a star*')]);
  assert.deepEqual(parseNotes('***FIXED desync ***'), [text('***FIXED desync ***')]);
  assert.deepEqual(parseNotes('* not this *'), [text('* not this *')]);
  // Emphasis never crosses a line break.
  assert.deepEqual(parseNotes('*first\nsecond*'), [text('*first\nsecond*')]);
  assert.deepEqual(parseNotes('*<>*'), [text('*<>*')]);
});

test('links, several per text', () => {
  assert.deepEqual(parseNotes('See [FAQ]{https://www.splitscreen.me/docs/faq} and [the mod]{ http://example.com/mod } now.'), [
    text('See '),
    link('FAQ', 'https://www.splitscreen.me/docs/faq'),
    text(' and '),
    link('the mod', 'http://example.com/mod'),
    text(' now.'),
  ]);
});

test('plain URLs, without the punctuation that follows them', () => {
  assert.deepEqual(parseNotes('Get it at https://nexusmods.com/eldenring/mods/510.'), [
    text('Get it at '),
    url('https://nexusmods.com/eldenring/mods/510'),
    text('.'),
  ]);
  assert.deepEqual(parseNotes('(see https://example.com/a?b=1), then https://example.com/c!'), [
    text('(see '),
    url('https://example.com/a?b=1'),
    text('), then '),
    url('https://example.com/c'),
    text('!'),
  ]);
});

test('only http and https become links', () => {
  assert.deepEqual(parseNotes('[click]{javascript:alert(1)}'), [text('[click]{javascript:alert(1)}')]);
  assert.deepEqual(parseNotes('[open]{file:///C:/Windows}'), [text('[open]{file:///C:/Windows}')]);
  assert.deepEqual(parseNotes('ftp://example.com and steam://run/1'), [text('ftp://example.com and steam://run/1')]);
  assert.deepEqual(parseLinks('[x]{data:text/html,hi} mailto:a@b.c'), [text('[x]{data:text/html,hi} mailto:a@b.c')]);
  assert.equal(isWebUrl('https://example.com'), true);
  assert.equal(isWebUrl('javascript:alert(1)'), false);
  assert.equal(isWebUrl('https://'), false);
});

test('links and URLs inside highlights', () => {
  assert.deepEqual(parseNotes('*Read [the guide]{https://example.com/g} first*'), [
    hl([text('Read '), link('the guide', 'https://example.com/g'), text(' first')]),
  ]);
  assert.deepEqual(parseNotes('*<mod: https://example.com/m|0,200,0>*'), [hl([text('mod: '), url('https://example.com/m')], [0, 200, 0])]);
});

test('mixed markup in one paragraph', () => {
  assert.deepEqual(parseNotes('*<Important>*: install [the patch]{https://example.com/p}, then *restart|255,200,0* the game (https://example.com/help).'), [
    hl([text('Important')], 'default'),
    text(': install '),
    link('the patch', 'https://example.com/p'),
    text(', then '),
    hl([text('restart')], [255, 200, 0]),
    text(' the game ('),
    url('https://example.com/help'),
    text(').'),
  ]);
});

test('parseLinks leaves the highlight markup as text', () => {
  assert.deepEqual(parseLinks('*not bold* and [a link]{https://example.com} or https://example.org/x.'), [
    text('*not bold* and '),
    link('a link', 'https://example.com'),
    text(' or '),
    url('https://example.org/x'),
    text('.'),
  ]);
  assert.deepEqual(parseLinks('Line one\nhttps://example.com\nline three'), [text('Line one\n'), url('https://example.com'), text('\nline three')]);
});

test('paragraphs split on blank lines, line breaks kept', () => {
  assert.deepEqual(notesParagraphs('One\r\ntwo\r\n\r\n\r\nThree\n  \nFour\n'), ['One\ntwo', 'Three', 'Four']);
  assert.deepEqual(notesParagraphs('\n\n  \n'), []);
});

test('plain URLs are shown shortened', () => {
  assert.equal(shortUrl('https://www.nexusmods.com/eldenring/mods/510'), 'nexusmods.com/eldenring/mods/510');
  assert.equal(shortUrl('https://example.com/'), 'example.com');
  assert.equal(shortUrl('https://example.com/a/very/long/path/that/goes/on/and/on'), 'example.com/a/very/long/path/that/goes/on…');
});
