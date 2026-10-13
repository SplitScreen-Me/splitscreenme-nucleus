import { Suspense, useEffect, useRef, useState } from 'react';
import { Outlet, useNavigate } from 'react-router';
import { FileDrop } from '@/features/file-drop';
import { FirstRunLanguageDialog } from '@/features/language';
import { NoticeDialog } from '@/features/notices';
import { useT } from '@/i18n';
import { setupPath } from '@/lib/library';
import { useAppInfo, useSessionSnapshot, useSettingsValues } from '@/lib/live';
import { GlyphStyleProvider, Icon, Spinner } from '@/ui';
import { ChromeProvider } from './chrome';
import { CommandPalette } from './CommandPalette';
import { CouchNavProvider } from './couch-nav';
import { HintBar } from './HintBar';
import { Rail } from './Rail';
import { TitleBar } from './TitleBar';

/**
 * The window is a bezel: a black frame, the title bar and hints sit on it, the rail and the
 * stage are lit panes separated by the app gutter.
 */
export function AppShell() {
  return (
    <ChromeProvider>
      <ShellBody />
    </ChromeProvider>
  );
}

function ShellBody() {
  const t = useT();
  const [palette, setPalette] = useState(false);
  const { data: values } = useSettingsValues();
  const glyphs = values?.['gamepad.type']?.toLowerCase().startsWith('play') ? 'playstation' : 'xbox';
  // Keeps the session state live for the whole app (palette, rail, lobby redirects).
  useSessionSnapshot();
  useStartupGame();

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        setPalette((p) => !p);
      }
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, []);

  return (
    <GlyphStyleProvider value={glyphs}>
      <CouchNavProvider enabled>
        <div className="grid h-screen min-h-[600px] min-w-[1080px] grid-cols-[84px_minmax(0,1fr)] grid-rows-[46px_auto_minmax(0,1fr)_42px] gap-x-gutter px-gutter [grid-template-areas:'title_title'_'rail_banner'_'rail_stage'_'hints_hints']">
          <a
            href="#stage"
            onClick={(e) => {
              e.preventDefault();
              document.getElementById('stage')?.focus();
            }}
            className="fixed top-2 left-2 z-50 -translate-y-20 rounded-ctl bg-ink px-3 py-2 text-small font-semibold text-on-light focus:translate-y-0"
          >
            {t('shell.skipToContent')}
          </a>
          <TitleBar onOpenPalette={() => setPalette(true)} />
          <Rail />
          <OfflineBanner />
          <main id="stage" tabIndex={-1} className="relative min-h-0 min-w-0 overflow-auto rounded-pane outline-none [grid-area:stage]">
            <Suspense fallback={<StageLoading />}>
              <Outlet />
            </Suspense>
          </main>
          <HintBar />
        </div>
        <CommandPalette open={palette} onClose={() => setPalette(false)} />
        <FileDrop />
        <NoticeDialog />
        <FirstRunLanguageDialog />
      </CouchNavProvider>
    </GlyphStyleProvider>
  );
}

/** A game passed on the command line (desktop shortcut) opens its setup once, at startup. */
function useStartupGame() {
  const { data: info } = useAppInfo();
  const navigate = useNavigate();
  const done = useRef(false);
  useEffect(() => {
    if (done.current || !info) return;
    done.current = true;
    if (info.startupGameId) navigate(setupPath(info.startupGameId), { replace: true });
  }, [info, navigate]);
}

/** The hub is out of reach: say what still works. Collapses to nothing when online. */
function OfflineBanner() {
  const { data: info } = useAppInfo();
  const t = useT();
  if (!info || info.hubOnline) return null;
  return (
    <div
      role="status"
      className="mb-gutter flex items-center gap-2.5 rounded-pane bg-pane px-4 py-2 text-13 text-ink-2 [grid-area:banner]"
    >
      <Icon name="offline" className="size-4 text-warn" />
      <b className="font-[650] text-ink">{t('shell.offlineBanner.title')}</b>
      <span className="truncate">{t('shell.offlineBanner.message')}</span>
    </div>
  );
}

function StageLoading() {
  const t = useT();
  return (
    <div className="grid h-full place-items-center rounded-pane bg-pane text-small text-ink-2">
      <span className="inline-flex items-center gap-2.5">
        <Spinner /> {t('common.loading')}
      </span>
    </div>
  );
}
