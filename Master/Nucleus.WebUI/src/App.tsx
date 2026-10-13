import { lazy, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createHashRouter, Navigate } from 'react-router';
import { RouterProvider } from 'react-router/dom';
import { BridgeProvider, type BridgeError, type BridgeMismatch } from '@/bridge';
import { AddGameProvider } from '@/features/add-game';
import { I18nProvider, useT, type RichTags } from '@/i18n';
import { AppShell } from '@/shell/AppShell';
import { ConfirmProvider, Icon, Kbd, LogoMark, Spinner, ToastProvider } from '@/ui';
import { NotFound } from '@/screens/NotFound';

const LibraryScreen = lazy(() => import('@/screens/library/LibraryScreen'));
const LobbyScreen = lazy(() => import('@/screens/lobby/LobbyScreen'));
const SessionScreen = lazy(() => import('@/screens/session/SessionScreen'));
const CatalogScreen = lazy(() => import('@/screens/catalog/CatalogScreen'));
const HandlerScreen = lazy(() => import('@/screens/catalog/HandlerScreen'));
const DownloadsScreen = lazy(() => import('@/screens/downloads/DownloadsScreen'));
const SettingsScreen = lazy(() => import('@/screens/settings/SettingsScreen'));

// Host data is pushed: events refetch what they make stale (see eventInvalidations). Host errors are not
// transient, so bridge queries are not retried. Bridge calls never leave the PC, so they run whatever
// navigator.onLine says ('always'); hub queries set their own network mode, retry and staleness.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: { refetchOnWindowFocus: false, retry: false, networkMode: 'always' },
    mutations: { networkMode: 'always' },
  },
});

// Hash routing: WebView2 serves the build from a virtual host folder with no index.html fallback.
const router = createHashRouter([
  {
    element: <AppShell />,
    errorElement: <NotFound crashed />,
    children: [
      { index: true, element: <Navigate to="/library" replace /> },
      { path: 'library', element: <LibraryScreen /> },
      { path: 'library/:gameId/setup', element: <LobbyScreen /> },
      { path: 'session', element: <SessionScreen /> },
      { path: 'games', element: <CatalogScreen /> },
      { path: 'games/:handlerId', element: <HandlerScreen /> },
      { path: 'downloads', element: <DownloadsScreen /> },
      { path: 'settings', element: <SettingsScreen /> },
      { path: '*', element: <NotFound /> },
    ],
  },
]);

// Tags of translated strings rendered by the design system (<T>, <RichText>); see docs/webui/i18n.md.
const RICH_TAGS: RichTags = { kbd: (children) => <Kbd>{children}</Kbd> };

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BridgeProvider connecting={<Splash />} renderMismatch={(m) => <Mismatch {...m} />} renderError={(e) => <HostDown error={e} />}>
        <I18nProvider loading={<Splash />} tags={RICH_TAGS}>
          <ToastProvider>
            <ConfirmProvider>
              <AddGameProvider>
                <RouterProvider router={router} />
              </AddGameProvider>
            </ConfirmProvider>
          </ToastProvider>
        </I18nProvider>
      </BridgeProvider>
    </QueryClientProvider>
  );
}

// Before the host answers, screens use the language of the last start (kept by the i18n store), else English.
function Splash() {
  const t = useT();
  return (
    <div className="grid h-screen place-items-center bg-bezel" role="status">
      <span className="inline-flex items-center gap-3 text-small text-ink-2">
        <Spinner /> {t('shell.startup.starting')}
      </span>
    </div>
  );
}

function Blocker({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div role="alert" className="grid h-screen place-items-center bg-bezel p-8">
      <div className="grid max-w-[60ch] justify-items-start gap-4 rounded-pane bg-pane p-9">
        <LogoMark />
        <h1 className="type-h2">{title}</h1>
        {children}
      </div>
    </div>
  );
}

/** The webui folder was built from another contract than the app (stale install). */
function Mismatch({ ui, host }: BridgeMismatch) {
  const t = useT();
  return (
    <Blocker title={t('shell.startup.mismatchTitle')}>
      <p className="text-ink-2">{t('shell.startup.mismatchMessage', { version: host.appVersion })}</p>
      <p className="data text-13 text-ink-3">
        {t('shell.startup.mismatchDetails', {
          uiContract: ui.contractHash,
          uiProtocol: String(ui.protocolVersion),
          appContract: host.contractHash,
          appProtocol: String(host.protocolVersion),
        })}
      </p>
    </Blocker>
  );
}

function HostDown({ error }: { error: BridgeError }) {
  const t = useT();
  return (
    <Blocker title={t('shell.startup.hostDownTitle')}>
      <p className="flex items-start gap-2 text-ink-2">
        <Icon name="warning" className="mt-0.5 size-5 text-warn" />
        {error.kind === 'timeout' ? t('shell.startup.hostDownTimeout') : t('shell.startup.hostDownMessage')}
      </p>
      {/* The bridge's own message is English: a detail for bug reports, under the translated explanation. */}
      {error.message && <p className="data text-13 text-ink-3">{t('shell.startup.hostDownDetails', { error: error.message })}</p>}
    </Blocker>
  );
}
