// The hub site this app talks to: `app.getInfo().hubUrl`, read once from the host (it does not change while the app
// runs), and checked against the page's Content-Security-Policy. The packaged UI only allows the default site
// (plus the origins given in NUCLEUS_HUB_ORIGINS at build time, see vite.config.ts); the Vite dev server has no CSP.
import { bridge } from '@/bridge';
import { HubError } from './api';
import { connectAllowed, cspConnectSources, DEFAULT_HUB_URL, normalizeHubUrl } from './urls';

let pending: Promise<string> | undefined;

/** Site root of the hub, without a trailing slash. Rejects with a HubError when the CSP blocks it. */
export function hubSite(): Promise<string> {
  pending ??= resolveSite().catch((err: unknown) => {
    pending = undefined; // try again on the next call (the bridge may not be ready yet)
    throw err;
  });
  return pending;
}

async function resolveSite(): Promise<string> {
  const info = await bridge.app.getInfo();
  const site = normalizeHubUrl(info.hubUrl) ?? DEFAULT_HUB_URL;
  const policy = document.querySelector<HTMLMetaElement>('meta[http-equiv="Content-Security-Policy"]')?.content;
  if (!connectAllowed(site, cspConnectSources(policy))) {
    const origin = new URL(site).origin;
    throw new HubError(
      // i18n-ignore: developer setup only (a [Dev] HubUrl the build does not allow), with build variable names.
      `The Content-Security-Policy of this interface build does not allow ${origin} (Settings.ini [Dev] HubUrl). Rebuild the interface with NUCLEUS_HUB_ORIGINS=${origin}, or load it from the Vite dev server.`,
    );
  }
  return site;
}
