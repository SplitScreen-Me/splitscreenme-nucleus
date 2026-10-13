// Hub screenshots: the host reads them from the handler detail of the API v3 (`handlers.screenshots`, IGDB URLs).
import { useBridgeQuery } from '@/bridge';
import type { IgdbSize } from './art';

/** Screenshot URLs of a hub handler's game, cached for a day. */
export const useScreenshots = (handlerId: string | undefined) =>
  useBridgeQuery('handlers.screenshots', { handlerId: handlerId ?? '' }, { enabled: Boolean(handlerId), staleTime: 24 * 3600_000, gcTime: 24 * 3600_000 });

/** The same IGDB image at another size (`…/t_screenshot_big/abc.jpg` → `…/t_1080p/abc.jpg`). Other URLs are unchanged. */
export function igdbResize(url: string | undefined, size: IgdbSize): string | undefined {
  return url?.replace(/\/t_[a-z0-9_]+\//i, `/t_${size}/`);
}
