// Site news for the in-browser mock (`npm run dev`, review URL): the real hub has no news to show on demand, so the
// ticker reads these instead. `localStorage['nucleus.mock.news']` picks what the "site" answers:
//   (unset)  three items, one per severity
//   'one'    a single long item (it loops)
//   'none'   no news (the ticker stays hidden)
//   'live'   the real site of app.getInfo().hubUrl (`GET /api/v3/news`)
//   '[...]'  a JSON array of raw items, as the API sends them
// Not part of the bridge mock: news come from the site, not from the host.
import type { NewsAnswerRaw } from '../../hub/news.ts';

const HOUR = 3_600_000;

function iso(ms: number): string {
  return new Date(ms).toISOString();
}

/** The default answer: an announcement with a link, a warning, a critical item. */
export function mockNewsItems(now = Date.now()): NewsAnswerRaw {
  return {
    items: [
      {
        id: 'mock-beta',
        message: 'Nucleus Co-op 3.0 beta: the new interface is here. Tell us what you think on Discord!',
        url: 'https://discord.gg/qnbZtpECYg',
        severity: 'info',
        startsAt: iso(now - 2 * HOUR),
        endsAt: null,
        updatedAt: '2026-11-01T09:00:00.000Z',
      },
      {
        id: 'mock-maintenance',
        message: 'Hub maintenance on Saturday from 08:00 to 10:00 UTC: handler downloads and updates may fail during that time.',
        url: null,
        severity: 'warning',
        startsAt: iso(now - HOUR),
        endsAt: iso(now + 72 * HOUR),
        updatedAt: '2026-11-02T18:30:00.000Z',
      },
      {
        id: 'mock-update',
        message: 'Update now: 3.0.0-beta-7 fixes a crash when launching ProtoInput handlers.',
        url: 'https://v3.splitscreen.me/download',
        severity: 'critical',
        startsAt: iso(now - HOUR),
        endsAt: null,
        updatedAt: '2026-11-03T07:15:00.000Z',
      },
    ],
  };
}

function oneLongItem(now: number): NewsAnswerRaw {
  return {
    items: [
      {
        id: 'mock-long',
        message:
          'The handler of Stardew Valley was updated for the 1.6 patch: open Downloads to update it, then check your profiles, since the new version moves the saves of every instance.',
        url: 'https://v3.splitscreen.me/hub',
        severity: 'info',
        startsAt: iso(now - HOUR),
        endsAt: null,
        updatedAt: '2026-11-03T10:00:00.000Z',
      },
    ],
  };
}

/** What the mock site answers, or null for 'live' (fetch the real site). */
export function mockNewsAnswer(mode = readMode(), now = Date.now()): NewsAnswerRaw | null {
  if (mode === 'live') return null;
  if (mode === 'none') return { items: [] };
  if (mode === 'one') return oneLongItem(now);
  if (mode?.trim().startsWith('[')) {
    try {
      return { items: JSON.parse(mode) as unknown };
    } catch {
      return { items: [] };
    }
  }
  return mockNewsItems(now);
}

function readMode(): string | null {
  try {
    return globalThis.localStorage?.getItem('nucleus.mock.news') ?? null;
  } catch {
    return null;
  }
}
