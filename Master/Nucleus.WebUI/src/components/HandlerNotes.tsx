import { Fragment, type ReactNode } from 'react';
import { bridge } from '@/bridge';
import { cn } from '@/ui';

// Handler notes use the Nucleus markup (GameDetails.notes), not markdown:
//   *text*          highlight
//   *text|r,g,b*    coloured text
//   [label]{url}    link
//   plain URLs are links
// Everything else is plain text: nothing is interpreted as HTML.
const TOKEN =
  /\[([^\]\n]+)\]\{\s*(https?:\/\/[^}\s]+)\s*\}|\*([^*\n]+?)(?:\|\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*)?\*|(https?:\/\/[^\s<>"'`]+[^\s<>"'`.,;:!?)\]])/g;
const URL_ONLY = /(https?:\/\/[^\s<>"'`]+[^\s<>"'`.,;:!?)\]])/g;

/** "nexusmods.com/eldenring/mods/510" for a bare URL used as its own label. */
function shortUrl(href: string): string {
  try {
    const u = new URL(href);
    const path = u.pathname.replace(/\/$/, '');
    return u.hostname.replace(/^www\./, '') + (path.length > 32 ? `${path.slice(0, 30)}…` : path);
  } catch {
    return href;
  }
}

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
function readableColour(r: number, g: number, b: number): string {
  const lin = (c: number) => {
    const s = Math.min(255, c) / 255;
    return s <= 0.04045 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4;
  };
  const luminance = 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b);
  const rgb = `rgb(${Math.min(255, r)} ${Math.min(255, g)} ${Math.min(255, b)})`;
  return luminance < 0.18 ? `color-mix(in oklab, ${rgb}, white ${Math.round((0.18 - luminance) * 300)}%)` : rgb;
}

function autolink(text: string, key: string): ReactNode[] {
  return text.split(URL_ONLY).map((part, i) =>
    i % 2 === 1 ? (
      <ExternalLink key={`${key}-${i}`} href={part}>
        {shortUrl(part)}
      </ExternalLink>
    ) : (
      <Fragment key={`${key}-${i}`}>{part}</Fragment>
    ),
  );
}

function inline(text: string): ReactNode[] {
  const out: ReactNode[] = [];
  let last = 0;
  for (const m of text.matchAll(TOKEN)) {
    const at = m.index ?? 0;
    if (at > last) out.push(text.slice(last, at));
    const [, label, href, emphasis, r, g, b, bare] = m;
    if (label !== undefined && href !== undefined) {
      out.push(
        <ExternalLink key={at} href={href}>
          {label}
        </ExternalLink>,
      );
    } else if (emphasis !== undefined) {
      const colour = r !== undefined && g !== undefined && b !== undefined ? readableColour(Number(r), Number(g), Number(b)) : undefined;
      out.push(
        <strong key={at} className={cn('font-[650]', !colour && 'text-ink')} style={colour ? { color: colour } : undefined}>
          {autolink(emphasis, String(at))}
        </strong>,
      );
    } else if (bare !== undefined) {
      out.push(
        <ExternalLink key={at} href={bare}>
          {shortUrl(bare)}
        </ExternalLink>,
      );
    }
    last = at + m[0].length;
  }
  if (last < text.length) out.push(text.slice(last));
  return out;
}

/** Handler notes in the Nucleus markup, rendered safely. Paragraphs on blank lines, line breaks kept. */
export function HandlerNotes({ children, className }: { children: string; className?: string }) {
  const paragraphs = children
    .replace(/\r\n?/g, '\n')
    .split(/\n\s*\n/)
    .map((p) => p.replace(/^\n+|\n+$/g, ''))
    .filter((p) => p.trim().length > 0);
  return (
    <div className={cn('grid max-w-[70ch] gap-[0.9em] leading-[1.6] text-ink-2 [overflow-wrap:anywhere]', className)}>
      {paragraphs.map((p, i) => (
        <p key={i} className="whitespace-pre-line">
          {inline(p)}
        </p>
      ))}
    </div>
  );
}
