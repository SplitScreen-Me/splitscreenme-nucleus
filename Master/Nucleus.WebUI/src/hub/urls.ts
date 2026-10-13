// Where the hub lives. The host sends the site root in `app.getInfo().hubUrl` (Settings.ini [Dev] HubUrl, else the
// default below); the host and the UI use the same value. Pure functions, no imports: the node tests load this file.

/** The public site: hub pages under /hub, the API under /api/v3. Same value as HubSite.DefaultUrl (C#). */
export const DEFAULT_HUB_URL = 'https://v3.splitscreen.me';

/**
 * Site root of a setting value, without query, fragment or trailing slash (a pasted `.../api/v3` is cut back to the
 * site), or undefined when it is not an absolute http(s) URL. Mirrors HubSite.Normalize.
 */
export function normalizeHubUrl(value: string | null | undefined): string | undefined {
  if (!value?.trim()) return undefined;
  let url: URL;
  try {
    url = new URL(value.trim());
  } catch {
    return undefined;
  }
  if ((url.protocol !== 'https:' && url.protocol !== 'http:') || !url.hostname || url.username || url.password) return undefined;
  const path = url.pathname.replace(/\/+$/, '').replace(/\/api\/v3$/i, '').replace(/\/+$/, '');
  return url.origin + path;
}

/** API v3 URL of a path (`/catalog`, `/handlers/<id>`…) on a site root. */
export const hubApiUrl = (site: string, path: string) => `${site}/api/v3${path}`;

/** The catalogue page of the site. */
export const hubCatalogueUrl = (site: string) => `${site}/hub`;

/** `connect-src` sources of a Content-Security-Policy, or undefined when there is no policy (or no directive). */
export function cspConnectSources(policy: string | null | undefined): string[] | undefined {
  if (!policy) return undefined;
  const directives = policy.split(';').map((d) => d.trim().split(/\s+/));
  const pick = (name: string) => directives.find((d) => d[0]?.toLowerCase() === name)?.slice(1);
  return pick('connect-src') ?? pick('default-src');
}

/**
 * Whether `connect-src` sources let the page fetch from a site: `*`, its scheme (`https:`), its exact origin or
 * `scheme://host:*`. No sources (no policy, as on the Vite dev server) allows everything.
 */
export function connectAllowed(site: string, sources: readonly string[] | undefined): boolean {
  if (!sources) return true;
  let url: URL;
  try {
    url = new URL(site);
  } catch {
    return false;
  }
  return sources.some((raw) => {
    const source = raw.toLowerCase().replace(/\/+$/, '');
    if (source === '*' || source === url.protocol) return true;
    if (source === url.origin) return true;
    return source.endsWith(':*') && source.slice(0, -2) === `${url.protocol}//${url.hostname}`;
  });
}
