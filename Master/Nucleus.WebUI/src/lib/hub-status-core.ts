// The hub status in the UI, the pure part: the offline banner's state, and when a hub request of the WebView is worth
// telling the host about. src/bridge/__tests__/hub-status.test.ts runs it under `node --test` (keep this file erasable
// TypeScript with no imports).
//
// The host owns the status (`app.getInfo().hubOnline`, `app.hubStatusChanged`): it checks `/api/v3/health` every 30 s
// while offline and every 5 min while online, never during a session. The WebView reads the catalogue and handler pages
// itself, so it may notice first: a request that fails while the host says online, or works while it says offline,
// asks the host to check (`handlers.checkHub` without force), at most every 30 s. The host rate-limits those checks too.

/** At most one check asked by the UI's own hub requests in this time (the host's offline retry interval). */
export const ASK_INTERVAL_MS = 30_000;

/** What the host says of the hub: online, offline, or not checked yet (`hubChecked` false). */
export type HubKnown = 'online' | 'offline' | 'unknown';

export function hubKnown(info: { hubOnline: boolean; hubChecked: boolean } | undefined): HubKnown {
  if (!info || !info.hubChecked) return 'unknown';
  return info.hubOnline ? 'online' : 'offline';
}

/** The parts of a failed hub request this reads (a `HubError` of src/hub/api.ts, a fetch abort…). */
export interface HubFailure {
  name?: string;
  /** HTTP status, when the hub answered. */
  status?: number;
}

/**
 * Whether a hub request of the WebView reached the hub: `null` (it worked) or an answer below 500 (404, 429: the site
 * is up) mean yes; no answer, or a 5xx (the proxy in front of a site that is down), mean no; a cancelled request says
 * nothing (undefined).
 */
export function reachedHub(error: HubFailure | null | undefined): boolean | undefined {
  if (error === null) return true;
  if (!error || error.name === 'AbortError') return undefined;
  return error.status !== undefined && error.status < 500;
}

/**
 * Whether to ask the host to check the hub after a request of the WebView: only when the request disagrees with what
 * the host says (it failed while online, or worked while offline) and nothing was asked in the last 30 s.
 */
export function shouldAskHost(known: HubKnown, reached: boolean | undefined, lastAskedAt: number | undefined, now: number, interval = ASK_INTERVAL_MS): boolean {
  if (known === 'unknown' || reached === undefined || (known === 'online') === reached) return false;
  return lastAskedAt === undefined || now - lastAskedAt >= interval;
}

/** Where the "Retry now" of the offline banner is: not pressed, checking, or the check found the hub still offline. */
export type RetryState = 'idle' | 'checking' | 'failed';

/** What the offline banner shows: nothing (online, or not checked yet), the offline message, a check, or "still offline". */
export type OfflineBannerState = 'hidden' | 'offline' | 'checking' | 'stillOffline';

export function offlineBannerState(known: HubKnown, retry: RetryState): OfflineBannerState {
  if (known !== 'offline') return 'hidden';
  if (retry === 'checking') return 'checking';
  return retry === 'failed' ? 'stillOffline' : 'offline';
}

/** Whether the hub just came back (offline, then online): the hub data is fetched again and the user told. */
export const cameBackOnline = (before: HubKnown, after: HubKnown) => before === 'offline' && after === 'online';
