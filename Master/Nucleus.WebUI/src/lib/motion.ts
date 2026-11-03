// The user's motion preference, live: Windows "Animation effects" off sets prefers-reduced-motion in WebView2.
import { useSyncExternalStore } from 'react';

const QUERY = '(prefers-reduced-motion: reduce)';

function media(): MediaQueryList | null {
  return typeof matchMedia === 'function' ? matchMedia(QUERY) : null;
}

function subscribe(onChange: () => void): () => void {
  const list = media();
  list?.addEventListener('change', onChange);
  return () => list?.removeEventListener('change', onChange);
}

/** Whether animations should stay still. Re-renders when the setting changes. */
export function useReducedMotion(): boolean {
  return useSyncExternalStore(
    subscribe,
    () => media()?.matches ?? false,
    () => false,
  );
}
