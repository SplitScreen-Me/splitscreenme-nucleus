// The Nucleus markup of handler texts (GameDetails.notes, option descriptions, notices), parsed into tokens. Pure: no
// React, no DOM, so `npm run test:bridge` tests it in Node. Nothing is ever interpreted as HTML.
//
//   *text*               emphasis (v3)
//   *text|r,g,b*         text in an author colour (v3)
//   *<text>*             highlight in the default highlight colour (legacy 2.4 / 2.5)
//   *<text|r,g,b>*       text in an author colour (legacy 2.4 / 2.5)
//   [label]{url}         link
//   https://…            plain URLs are links
//
// Only http and https links become links: anything else stays plain text.

/** An author colour, each channel 0-255 (larger values are read as 255, like the legacy renderer). */
export type Rgb = readonly [number, number, number];

/** Text, links and plain URLs: what {@link parseLinks} returns, and what highlights contain. */
export type InlineToken =
  | { kind: 'text'; text: string }
  /** `[label]{url}`. */
  | { kind: 'link'; label: string; href: string }
  /** A plain URL, shown shortened (see {@link shortUrl}). */
  | { kind: 'url'; href: string };

/** A piece of a handler text. */
export type NotesToken =
  | InlineToken
  | {
      kind: 'highlight';
      /**
       * Absent: emphasis (`*text*`). `'default'`: the default highlight colour (`*<text>*`, GreenYellow in the legacy
       * app). Otherwise the author's colour.
       */
      colour?: Rgb | 'default';
      /** The highlighted text; it may hold links and URLs. */
      children: InlineToken[];
    };

const LINK = String.raw`\[([^\]\n]+)\]\{\s*(https?:\/\/[^}\s]+)\s*\}`;
// The legacy pattern (TransparentRichTextBox.Highlight): anything but brackets and angle brackets between `*<` and `>*`.
const LEGACY = String.raw`\*<([^\[\]<>]+)>\*`;
// v3: no space right inside the stars, so "2 * 3 * 4" stays as it is; the colour comes last. A `*<` that is not a
// legacy highlight (brackets inside) is not emphasis either.
const EMPHASIS = String.raw`\*(?=[^\s<])([^*\n]+?)(?:\s*\|\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3}))?(?<=\S)\*`;
// A plain URL never ends with punctuation that usually follows it in a sentence.
const BARE_URL = String.raw`https?:\/\/[^\s<>"'\`]+[^\s<>"'\`.,;:!?)\]}*]`;

const TOKEN = new RegExp(`${LINK}|${LEGACY}|${EMPHASIS}|(${BARE_URL})`, 'g');
const LINKS = new RegExp(`${LINK}|(${BARE_URL})`, 'g');
const LEGACY_COLOUR = /^\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*$/;

/** Whether a link target is a web address (http or https, with a host). */
export function isWebUrl(href: string): boolean {
  try {
    const url = new URL(href);
    return (url.protocol === 'http:' || url.protocol === 'https:') && url.hostname.length > 0;
  } catch {
    return false;
  }
}

/** "nexusmods.com/eldenring/mods/510" for a plain URL shown as its own label. */
export function shortUrl(href: string): string {
  try {
    const u = new URL(href);
    const path = u.pathname.replace(/\/$/, '');
    return u.hostname.replace(/^www\./, '') + (path.length > 32 ? `${path.slice(0, 30)}…` : path);
  } catch {
    return href;
  }
}

const channel = (value: string) => Math.min(255, Number(value));

function pushText(out: NotesToken[], text: string): void {
  if (!text) return;
  const last = out[out.length - 1];
  if (last?.kind === 'text') last.text += text;
  else out.push({ kind: 'text', text });
}

/** `[label]{url}` and plain URLs; everything else is text. */
export function parseLinks(text: string): InlineToken[] {
  const out: InlineToken[] = [];
  let last = 0;
  for (const m of text.matchAll(LINKS)) {
    const at = m.index ?? 0;
    const [whole, label, href, bare] = m;
    const token: InlineToken | null =
      label !== undefined && href !== undefined
        ? isWebUrl(href)
          ? { kind: 'link', label, href }
          : null
        : bare !== undefined && isWebUrl(bare)
          ? { kind: 'url', href: bare }
          : null;
    if (!token) continue; // stays text, with what follows
    pushText(out, text.slice(last, at));
    out.push(token);
    last = at + whole.length;
  }
  pushText(out, text.slice(last));
  return out;
}

/** Legacy `*<…>*` content: "text" (default colour) or "text|r,g,b". */
function legacyHighlight(inner: string): NotesToken {
  const bar = inner.lastIndexOf('|');
  const colour = bar >= 0 ? LEGACY_COLOUR.exec(inner.slice(bar + 1)) : null;
  if (colour) {
    const [, r = '0', g = '0', b = '0'] = colour;
    return { kind: 'highlight', colour: [channel(r), channel(g), channel(b)], children: parseLinks(inner.slice(0, bar)) };
  }
  return { kind: 'highlight', colour: 'default', children: parseLinks(inner) };
}

/** One paragraph (or any text) of handler markup as tokens. Line breaks stay in the text tokens. */
export function parseNotes(text: string): NotesToken[] {
  const out: NotesToken[] = [];
  let last = 0;
  for (const m of text.matchAll(TOKEN)) {
    const at = m.index ?? 0;
    const [whole, label, href, legacy, emphasis, r, g, b, bare] = m;
    let token: NotesToken | null = null;
    if (label !== undefined && href !== undefined) {
      token = isWebUrl(href) ? { kind: 'link', label, href } : null;
    } else if (legacy !== undefined) {
      token = legacyHighlight(legacy);
    } else if (emphasis !== undefined) {
      token =
        r !== undefined && g !== undefined && b !== undefined
          ? { kind: 'highlight', colour: [channel(r), channel(g), channel(b)], children: parseLinks(emphasis) }
          : { kind: 'highlight', children: parseLinks(emphasis) };
    } else if (bare !== undefined) {
      token = isWebUrl(bare) ? { kind: 'url', href: bare } : null;
    }
    if (!token) continue;
    pushText(out, text.slice(last, at));
    out.push(token);
    last = at + whole.length;
  }
  pushText(out, text.slice(last));
  return out;
}

/** Paragraphs of a handler text: separated by blank lines, single line breaks kept, empty ones dropped. */
export function notesParagraphs(text: string): string[] {
  return text
    .replace(/\r\n?/g, '\n')
    .split(/\n\s*\n/)
    .map((p) => p.replace(/^\n+|\n+$/g, ''))
    .filter((p) => p.trim().length > 0);
}
