// Last known catalogue, kept in localStorage so the app starts offline with it.
// Every access is guarded: storage can be disabled, full, or wiped by the WebView profile.
import type { HubHandler } from './types';

const KEY = 'nucleus.hub.catalog.v3';
// The API v1 copy (other field names), left by earlier builds: removed on the first read.
const LEGACY_KEYS = ['nucleus.hub.allhandlers.v1'];

interface Stored {
  savedAt: number;
  handlers: HubHandler[];
}

export function loadCatalogue(): Stored | undefined {
  try {
    LEGACY_KEYS.forEach((key) => localStorage.removeItem(key));
    const raw = localStorage.getItem(KEY);
    if (!raw) return undefined;
    const parsed = JSON.parse(raw) as Partial<Stored>;
    if (typeof parsed.savedAt !== 'number' || !Array.isArray(parsed.handlers) || !parsed.handlers.length) return undefined;
    return { savedAt: parsed.savedAt, handlers: parsed.handlers };
  } catch {
    return undefined;
  }
}

export function saveCatalogue(handlers: HubHandler[], savedAt = Date.now()): void {
  try {
    localStorage.setItem(KEY, JSON.stringify({ savedAt, handlers } satisfies Stored));
  } catch {
    // Quota or disabled storage: retry once without the long markdown fields, else give up quietly.
    try {
      const slim = handlers.map((h) => ({ ...h, description: '', gameDescription: '' }));
      localStorage.setItem(KEY, JSON.stringify({ savedAt, handlers: slim } satisfies Stored));
    } catch {
      /* offline start will show the empty state */
    }
  }
}
