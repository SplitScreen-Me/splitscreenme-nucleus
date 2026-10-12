import { useEffect } from 'react';
import { useRouteError } from 'react-router';
import { bridge } from '@/bridge';
import { ButtonLink, Icon, Pane } from '@/ui';
import { useScreenChrome } from '@/shell/chrome';

/** Unknown route, or a screen that crashed (errorElement). */
export function NotFound({ crashed = false }: { crashed?: boolean }) {
  if (crashed) return <Crashed />;
  return <Missing />;
}

function Missing() {
  useScreenChrome([{ label: 'Not found' }], { base: [{ button: 'B', label: 'Back' }] });
  return (
    <Pane className="grid h-full place-items-center p-8">
      <div className="grid max-w-[46ch] justify-items-start gap-4">
        <GhostScreen />
        <h1 className="type-h2">This screen does not exist</h1>
        <p className="text-ink-2">The link may be from an older version of Nucleus Co-op.</p>
        <ButtonLink to="/library" variant="primary">
          <Icon name="library" />
          Back to library
        </ButtonLink>
      </div>
    </Pane>
  );
}

function Crashed() {
  const error = useRouteError();
  const message = error instanceof Error ? error.message : 'Unknown error';
  // The app log keeps the details for bug reports.
  useEffect(() => {
    const detail = error instanceof Error ? (error.stack ?? error.message) : String(error);
    void bridge.app.log({ level: 'error', message: detail, source: 'screen' }).catch(() => undefined);
  }, [error]);
  return (
    <div className="grid h-screen place-items-center bg-bezel p-8 text-ink">
      <div className="grid max-w-[56ch] justify-items-start gap-4 rounded-pane bg-pane p-8">
        <GhostScreen />
        <h1 className="type-h2">Something broke on this screen</h1>
        <p className="text-ink-2">Nucleus Co-op is still running. Go back to the library and try again; if it happens again, copy the details below into a bug report.</p>
        <pre className="max-h-40 w-full overflow-auto rounded-ctl bg-pane-2 p-3 text-12 text-ink-2">{message}</pre>
        <a href="#/library" className="font-semibold text-ink underline underline-offset-4">
          Back to library
        </a>
      </div>
    </div>
  );
}

function GhostScreen() {
  return (
    <div aria-hidden="true" className="grid aspect-[16/10] w-[120px] grid-cols-2 gap-1">
      <i className="rounded-[5px] bg-pane-3" />
      <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
    </div>
  );
}
