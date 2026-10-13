import { useRef, useState, type ReactNode } from 'react';
import { bridge, useBridgeEvent, type ArtworkProgress } from '@/bridge';
import { DEFAULT_HUB_URL, hubCatalogueUrl } from '@/hub';
import { useT } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { LINKS } from '@/lib/links';
import { useAppInfo } from '@/lib/live';
import { Button, cn, Icon, Pane, Spinner, useConfirm, type IconName } from '@/ui';

/** Versions, folders, maintenance actions and community links. */
export function About() {
  const t = useT();
  const { data: info } = useAppInfo();
  const run = useRun();
  const confirm = useConfirm();
  const [busy, setBusy] = useState<'updates' | 'reload' | null>(null);

  const links: Array<{ label: string; url: string; icon: IconName }> = [
    { label: 'splitscreen.me', url: LINKS.website, icon: 'globe' }, // i18n-ignore: domain name
    { label: t('settings.about.links.docs'), url: LINKS.docs, icon: 'book' },
    { label: t('settings.about.links.faq'), url: LINKS.faq, icon: 'info' },
    { label: 'Discord', url: LINKS.discord, icon: 'discord' }, // i18n-ignore: brand name
    { label: t('settings.about.links.hub'), url: hubCatalogueUrl(info?.hubUrl || DEFAULT_HUB_URL), icon: 'compass' },
  ];

  const once = async (what: 'updates' | 'reload', call: () => Promise<unknown>, success: string) => {
    setBusy(what);
    await run(call, { success });
    setBusy(null);
  };

  const restartAsAdmin = async () => {
    const ok = await confirm({
      title: t('settings.about.admin.confirmTitle'),
      message: t('settings.about.admin.confirmMessage'),
      confirmLabel: t('common.restart'),
      icon: 'refresh',
    });
    if (ok) await run(() => bridge.app.restartAsAdmin());
  };

  return (
    <Pane as="section" id="set-about" aria-labelledby="set-about-title" className="scroll-mt-0 px-7 pt-6 pb-7">
      <h2 id="set-about-title" className="type-h3">
        {t('settings.about.title')}
      </h2>
      <p className="mt-1 mb-5 text-small text-ink-3">{t('settings.about.tagline')}</p>

      <dl className="m-0 mb-5 grid grid-cols-[repeat(auto-fit,minmax(190px,1fr))] gap-gutter">
        <Fact label={t('settings.about.facts.version')}>{info?.appVersion ?? '…'}</Fact>
        <Fact label="WebView2">{info?.webViewVersion ?? '…'}</Fact>
        <Fact label={t('settings.about.facts.hub')}>
          {info ? (
            <span className="inline-flex items-center gap-1.5" title={info.hubUrl}>
              <span aria-hidden="true" className={cn('size-2 rounded-full', info.hubOnline ? 'bg-ok' : 'bg-warn')} />
              {info.hubOnline ? t('settings.about.hubOnline') : t('settings.about.hubOffline')}
            </span>
          ) : (
            '…'
          )}
        </Fact>
        <Fact label={t('settings.about.facts.admin')}>{info ? (info.isAdmin ? t('common.yes') : t('common.no')) : '…'}</Fact>
      </dl>

      <div className="grid">
        <FolderRow label={t('settings.about.folders.install')} openLabel={t('settings.about.folders.openInstall')} path={info?.installPath} />
        <FolderRow label={t('settings.about.folders.handlers')} openLabel={t('settings.about.folders.openHandlers')} path={info?.handlersPath} />
        <ActionRow label={t('settings.about.updates.label')} description={t('settings.about.updates.description')}>
          <Button
            variant="ghost"
            size="sm"
            disabled={busy !== null || info?.hubOnline === false}
            onClick={() => void once('updates', () => bridge.handlers.checkUpdates({ timeoutMs: 120_000 }), t('settings.about.updates.done'))}
            data-nav
          >
            {busy === 'updates' ? <Spinner /> : <Icon name="update" />}
            {t('settings.about.updates.button')}
          </Button>
        </ActionRow>
        <ActionRow label={t('settings.about.reload.label')} description={t('settings.about.reload.description')}>
          <Button
            variant="ghost"
            size="sm"
            disabled={busy !== null}
            onClick={() => void once('reload', () => bridge.library.reload({ timeoutMs: 120_000 }), t('settings.about.reload.done'))}
            data-nav
          >
            {busy === 'reload' ? <Spinner /> : <Icon name="refresh" />}
            {t('settings.about.reload.button')}
          </Button>
        </ActionRow>
        <ArtworkRow disabled={info?.hubOnline === false} />
        {info && !info.isAdmin && (
          <ActionRow label={t('settings.about.admin.label')} description={t('settings.about.admin.description')}>
            <Button variant="ghost" size="sm" onClick={() => void restartAsAdmin()} data-nav>
              <Icon name="seal" />
              {t('settings.about.admin.button')}
            </Button>
          </ActionRow>
        )}
      </div>

      <div className="mt-5 flex flex-wrap gap-2">
        {links.map((l) => (
          <Button key={l.url} variant="ghost" size="sm" onClick={() => void run(() => bridge.app.openExternal({ url: l.url }))} data-nav>
            <Icon name={l.icon} />
            {l.label}
          </Button>
        ))}
      </div>
    </Pane>
  );
}

function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="min-w-0 rounded-ctl bg-pane-2 px-4 py-3">
      <dt className="text-12 text-ink-3">{label}</dt>
      <dd className="data m-0 truncate text-15 font-[650]">{children}</dd>
    </div>
  );
}

function ActionRow({ label, description, children }: { label: string; description?: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-6 border-t border-line py-3.5">
      <div className="min-w-0">
        <div className="text-15 font-semibold">{label}</div>
        {description && <div className="mt-0.5 max-w-[62ch] text-13 leading-[1.45] text-ink-3">{description}</div>}
      </div>
      <div className="flex justify-end gap-2">{children}</div>
    </div>
  );
}

function FolderRow({ label, openLabel, path }: { label: string; openLabel: string; path?: string }) {
  const t = useT();
  const run = useRun();
  return (
    <ActionRow label={label}>
      <span className="max-w-[min(420px,36vw)] truncate font-mono text-13 text-ink-2" title={path}>
        {path ?? '…'}
      </span>
      <Button
        variant="ghost"
        size="sm"
        disabled={!path}
        aria-label={openLabel}
        onClick={() => path && void run(() => bridge.app.openPath({ path }))}
        data-nav
      >
        <Icon name="folder" />
        {t('common.open')}
      </Button>
    </ActionRow>
  );
}

/** Covers and screenshots of every game, with progress; can be stopped between two games. */
function ArtworkRow({ disabled }: { disabled: boolean }) {
  const t = useT();
  const run = useRun();
  const [progress, setProgress] = useState<ArtworkProgress | null>(null);
  const [controller, setController] = useState<AbortController | null>(null);
  const active = useRef<AbortController | null>(null);

  useBridgeEvent('library.artworkProgress', (p) => {
    if (active.current) setProgress(p);
  });

  const start = async () => {
    const c = new AbortController();
    active.current = c;
    setController(c);
    setProgress(null);
    await run(() => bridge.library.downloadArtwork({}, { signal: c.signal, timeoutMs: 0 }), { success: t('settings.about.artwork.done') });
    if (active.current === c) active.current = null;
    setController((old) => (old === c ? null : old));
    setProgress(null);
  };

  const share = progress && progress.total > 0 ? progress.done / progress.total : 0;
  return (
    <ActionRow label={t('settings.about.artwork.label')} description={t('settings.about.artwork.description')}>
      {controller ? (
        <div className="flex items-center gap-3" role="status" aria-live="polite">
          <span className="grid w-[180px] gap-1.5">
            <span className="data text-13 text-ink-2">
              {progress
                ? t('settings.about.artwork.progress', { done: progress.done, count: progress.total })
                : t('settings.about.artwork.starting')}
            </span>
            <span className="block h-1 overflow-hidden rounded-full bg-pane-3" aria-hidden="true">
              <span className="block h-full origin-left bg-ink transition-transform duration-(--dur-2)" style={{ transform: `scaleX(${share})` }} />
            </span>
          </span>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              controller.abort();
              active.current = null;
              setController(null);
              setProgress(null);
            }}
            data-nav
          >
            {t('common.stop')}
          </Button>
        </div>
      ) : (
        <Button variant="ghost" size="sm" disabled={disabled} onClick={() => void start()} data-nav>
          <Icon name="download" />
          {t('settings.about.artwork.button')}
        </Button>
      )}
    </ActionRow>
  );
}
