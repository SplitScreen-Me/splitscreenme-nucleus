import type { ReactNode } from 'react';
import ReactMarkdown, { type Components } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { bridge } from '@/bridge';
import { cn } from '@/ui';

/** "nexusmods.com/eldenring/mods/510" for a bare URL used as its own label. */
function shortUrl(href: string): string {
  try {
    const u = new URL(href);
    const path = u.pathname.replace(/\/$/, '');
    return u.hostname.replace(/^www\./, '') + (path.length > 28 ? `${path.slice(0, 26)}…` : path);
  } catch {
    return href;
  }
}

const textOf = (node: ReactNode): string =>
  typeof node === 'string' ? node : Array.isArray(node) ? node.map(textOf).join('') : '';

/** Links leave the app: the default browser opens them (the WebView never navigates away). */
function ExternalLink({ href, children }: { href?: string; children?: ReactNode }) {
  if (!href || !/^https?:\/\//i.test(href)) return <span>{children}</span>;
  const label = textOf(children);
  return (
    <a
      href={href}
      rel="nofollow ugc noopener noreferrer"
      target="_blank"
      onClick={(e) => {
        e.preventDefault();
        void bridge.app.openExternal({ url: href }).catch(() => undefined);
      }}
      title={href}
    >
      {label === href ? shortUrl(href) : children}
    </a>
  );
}

const COMPONENTS: Components = {
  a: ({ href, children }) => <ExternalLink href={href}>{children}</ExternalLink>,
  // Authors' headings stay modest inside a pane.
  h1: ({ children }) => <h3>{children}</h3>,
  h2: ({ children }) => <h3>{children}</h3>,
  h3: ({ children }) => <h4>{children}</h4>,
  // Remote images in user content are not loaded; the link stays.
  img: ({ src, alt }) => (typeof src === 'string' ? <ExternalLink href={src}>{alt || 'Image'}</ExternalLink> : null),
};

/**
 * Safe markdown for handler notes and comments: CommonMark + GFM (autolinks, lists, tables),
 * raw HTML skipped, javascript: URLs dropped by react-markdown's default urlTransform.
 */
export function Markdown({ children, className, compact = false }: { children: string; className?: string; compact?: boolean }) {
  return (
    <div
      className={cn(
        'max-w-[68ch] text-ink-2 [overflow-wrap:anywhere]',
        '[&_p]:mb-[1em] [&_ul]:mb-[1em] [&_ol]:mb-[1em] [&>*:last-child]:mb-0',
        '[&_ul]:grid [&_ul]:list-disc [&_ul]:gap-[0.5em] [&_ul]:pl-[1.2em] [&_ol]:list-decimal [&_ol]:pl-[1.4em] [&_li::marker]:text-ink-3',
        '[&_a]:text-ink [&_a]:underline [&_a]:decoration-line-strong [&_a]:underline-offset-3 hover:[&_a]:decoration-ink',
        '[&_strong]:text-ink [&_h3]:mt-[1.2em] [&_h3]:mb-[0.5em] [&_h3]:text-16 [&_h3]:text-ink [&_h4]:mt-[1em] [&_h4]:mb-[0.4em] [&_h4]:text-15 [&_h4]:text-ink',
        '[&_code]:rounded [&_code]:bg-pane-2 [&_code]:px-1 [&_code]:text-[0.875em] [&_pre]:mb-[1em] [&_pre]:overflow-auto [&_pre]:rounded-ctl [&_pre]:bg-pane-2 [&_pre]:p-3',
        '[&_blockquote]:mb-[1em] [&_blockquote]:border-l-2 [&_blockquote]:border-line-strong [&_blockquote]:pl-3',
        '[&_table]:mb-[1em] [&_td]:border [&_td]:border-line [&_td]:px-2 [&_td]:py-1 [&_th]:border [&_th]:border-line [&_th]:px-2 [&_th]:py-1',
        compact ? 'text-15 leading-[1.5]' : 'leading-[1.6]',
        className,
      )}
    >
      <ReactMarkdown remarkPlugins={[remarkGfm]} skipHtml components={COMPONENTS}>
        {children}
      </ReactMarkdown>
    </div>
  );
}
