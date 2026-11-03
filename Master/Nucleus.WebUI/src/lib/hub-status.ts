// The hub status in the UI: what the host says (app.getInfo, live on app.hubStatusChanged), the "Retry now" of the
// offline banner, and the hub requests of the WebView feeding the host's status. Rules in hub-status-core.ts.
import { useCallback, useEffect, useRef, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { bridge } from '@/bridge';
import { t } from '@/i18n';
import { useToast } from '@/ui';
import { cameBackOnline, hubKnown, reachedHub, shouldAskHost, type HubFailure, type HubKnown, type RetryState } from './hub-status-core';
import { useAppInfo } from './live';

export { offlineBannerState, type HubKnown, type OfflineBannerState } from './hub-status-core';

/** Last state the host gave, for the hub requests made outside React (src/hub/queries.ts). */
let known: HubKnown = 'unknown';
let lastAskedAt: number | undefined;

/**
 * Tells the host what a hub request of the WebView saw (`null`: it worked; else its error), when that disagrees with
 * the host's status, at most every 30 s. The host checks `/health` and announces a change (`app.hubStatusChanged`).
 */
export function reportHubRequest(error: unknown): void {
  const reached = reachedHub(error === null ? null : (error as HubFailure | undefined));
  const now = Date.now();
  if (!shouldAskHost(known, reached, lastAskedAt, now)) return;
  lastAskedAt = now;
  bridge.handlers.checkHub({ force: false }).catch(() => undefined);
}

/** What the host says of the hub, live. */
export function useHubKnown(): HubKnown {
  const { data: info } = useAppInfo();
  return hubKnown(info);
}

/**
 * Follows the host's hub status for the whole app (mounted once, by the shell): when the hub comes back, every hub
 * query (catalogue, handler pages, comments, news) is fetched again and a toast says so. The library's update flags
 * come back by themselves: the host checks the updates again and `app.hubStatusChanged` refetches the library.
 */
export function useHubStatusSync(): HubKnown {
  const state = useHubKnown();
  const queryClient = useQueryClient();
  const toast = useToast();
  const before = useRef<HubKnown>(state);

  useEffect(() => {
    known = state;
    if (cameBackOnline(before.current, state)) {
      void queryClient.invalidateQueries({ queryKey: ['hub'] });
      toast(t('shell.offlineBanner.backOnline'), { icon: 'globe' });
    }
    before.current = state;
  }, [state, queryClient, toast]);

  return state;
}

/** The "Retry now" of the offline banner: a forced check; `failed` while the hub stays offline. */
export function useHubRetry(): { retry: RetryState; check: () => Promise<void> } {
  const state = useHubKnown();
  const [retry, setRetry] = useState<RetryState>('idle');

  // A change of status (the host's own check, or this one) starts over.
  useEffect(() => setRetry('idle'), [state]);

  const check = useCallback(async () => {
    setRetry('checking');
    let online = false;
    try {
      online = await bridge.handlers.checkHub({ force: true }, { timeoutMs: 20_000 });
    } catch {
      online = false;
    }
    setRetry(online ? 'idle' : 'failed');
  }, []);

  return { retry, check };
}
