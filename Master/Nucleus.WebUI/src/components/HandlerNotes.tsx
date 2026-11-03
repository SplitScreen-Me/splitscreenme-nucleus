import type { ReactNode } from 'react';
import { bridge } from '@/bridge';
import { cn } from '@/ui';
import { notesParagraphs, parseLinks, parseNotes, shortUrl, type InlineToken, type NotesToken, type Rgb } from './notes-markup';

// Handler notes use the Nucleus markup (GameDetails.notes), not markdown: see notes-markup.ts for the syntax
// (`*text*`, `*text|r,g,b*`, legacy `*<text>*` and `*<text|r,g,b>*`, `[label]{url}`, plain URLs).
// Everything else is plain text: nothing is interpreted as HTML.

/** Links leave the app: the default browser opens them (the WebView never navigates away). */
export function ExternalLink({ href, children, className }: { href: string; children: ReactNode; className?: string }) {
  return (
    <a
      href={href}
      title={href}
      rel="noopener noreferrer"
      onClick={(e) => {
        e.preventDefault();
        void bridge.app.openExternal({ url: href }).catch(() => undefined);
      }}
      className={cn('text-ink underline decoration-line-strong underline-offset-3 hover:decoration-ink', className)}
    >
      {children}
    </a>
  );
}

/** Author colours are kept, lifted when they would be too dark to read on a pane. */
function readableColour([r, g, b]: Rgb): string {
  const lin = (c: number) => {
    const s = c / 255;
    return s <= 0.04045 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4;
  };
  const luminance = 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b);
  const rgb = `rgb(${r} ${g} ${b})`;
  return luminance < 0.18 ? `color-mix(in oklab, ${rgb}, white ${Math.round((0.18 - luminance) * 300)}%)` : rgb;
}

function renderInline(token: InlineToken, key: string | number): ReactNode {
  switch (token.kind) {
    case 'text':
      return token.text;
    case 'link':
      return (
        <ExternalLink key={key} href={token.href}>
          {token.label}
        </ExternalLink>
      );
    case 'url':
      return (
        <ExternalLink key={key} href={token.href}>
          {shortUrl(token.href)}
        </ExternalLink>
      );
  }
}

function renderToken(token: NotesToken, key: number): ReactNode {
  if (token.kind !== 'highlight') return renderInline(token, key);
  const children = token.children.map((child, i) => renderInline(child, `${key}-${i}`));
  const colour = token.colour;
  // Emphasis is bold ink. The default highlight (legacy `*<text>*`, GreenYellow before v3) is the `ok` green: a
  // status colour, never a player colour (colour means a player everywhere else).
  return (
    <strong
      key={key}
      className={cn('font-[650]', colour === undefined ? 'text-ink' : colour === 'default' ? 'text-ok' : undefined)}
      style={colour !== undefined && colour !== 'default' ? { color: readableColour(colour) } : undefined}
    >
      {children}
    </strong>
  );
}

/**
 * Plain text with its links: `[label]{url}` and plain URLs become links that open in the browser, everything else is
 * text as it is (handler option descriptions, notices). Renders inline: the parent keeps its own wrapping.
 */
export function LinkifiedText({ children }: { children: string }) {
  return <>{parseLinks(children).map((token, i) => renderInline(token, i))}</>;
}

/** Handler notes in the Nucleus markup, rendered safely. Paragraphs on blank lines, line breaks kept. */
export function HandlerNotes({ children, className }: { children: string; className?: string }) {
  return (
    <div className={cn('grid max-w-[70ch] gap-[0.9em] leading-[1.6] text-ink-2 [overflow-wrap:anywhere]', className)}>
      {notesParagraphs(children).map((p, i) => (
        <p key={i} className="whitespace-pre-line">
          {parseNotes(p).map(renderToken)}
        </p>
      ))}
    </div>
  );
}
