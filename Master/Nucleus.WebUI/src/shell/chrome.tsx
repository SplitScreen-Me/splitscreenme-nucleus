// Screen chrome: each screen declares its breadcrumb and its button hints; the shell renders them.
import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import type { PadButton } from '@/ui';

export interface Crumb {
  label: string;
  to?: string;
}

export interface Hint {
  /** Gamepad button, or a keyboard key name when `key` is set. */
  button?: PadButton;
  key?: string;
  label: string;
  /** What the gamepad button does on this screen. A without action clicks the focused element. */
  action?: () => void;
}

export interface HintSet {
  /** Hints when nothing more specific is focused. */
  base: Hint[];
  /** Hints for the focused element, chosen by its data-hints attribute. */
  byFocus?: Record<string, Hint[]>;
  /** Extra text on the right of the bar (keyboard tips). */
  aside?: ReactNode;
}

interface ChromeState {
  crumbs: Crumb[];
  hints: HintSet;
  focusKey: string | null;
}

interface ChromeApi {
  setCrumbs: (c: Crumb[]) => void;
  setHints: (h: HintSet) => void;
  /** The hint list currently shown. Actions are always the latest ones the screen rendered. */
  activeHints: () => Hint[];
}

const EMPTY: HintSet = { base: [] };
const StateCtx = createContext<ChromeState>({ crumbs: [], hints: EMPTY, focusKey: null });
const ApiCtx = createContext<ChromeApi>({ setCrumbs: () => {}, setHints: () => {}, activeHints: () => [] });

export function ChromeProvider({ children }: { children: ReactNode }) {
  const [crumbs, setCrumbs] = useState<Crumb[]>([]);
  const [hints, setHintsState] = useState<HintSet>(EMPTY);
  const [focusKey, setFocusKey] = useState<string | null>(null);
  const hintsRef = useRef<HintSet>(EMPTY);
  const focusRef = useRef<string | null>(null);

  useEffect(() => {
    const onFocus = (e: FocusEvent) => {
      const el = (e.target as Element | null)?.closest?.('[data-hints]');
      const key = el?.getAttribute('data-hints') ?? null;
      focusRef.current = key;
      setFocusKey(key);
    };
    document.addEventListener('focusin', onFocus);
    return () => document.removeEventListener('focusin', onFocus);
  }, []);

  const api = useMemo<ChromeApi>(
    () => ({
      setCrumbs: (c) => setCrumbs((old) => (JSON.stringify(old) === JSON.stringify(c) ? old : c)),
      setHints: (h) => {
        hintsRef.current = h;
        // Re-render only when what is shown changes; actions are read from the ref.
        setHintsState((old) => (signature(old) === signature(h) ? old : h));
      },
      activeHints: () => pick(hintsRef.current, focusRef.current),
    }),
    [],
  );

  const state = useMemo(() => ({ crumbs, hints, focusKey }), [crumbs, hints, focusKey]);
  return (
    <ApiCtx value={api}>
      <StateCtx value={state}>{children}</StateCtx>
    </ApiCtx>
  );
}

function pick(set: HintSet, focusKey: string | null): Hint[] {
  return (focusKey && set.byFocus?.[focusKey]) || set.base;
}

function signature(set: HintSet): string {
  const line = (l: Hint[]) => l.map((h) => `${h.button ?? h.key}:${h.label}`).join('|');
  return [line(set.base), ...Object.entries(set.byFocus ?? {}).map(([k, v]) => `${k}=${line(v)}`), typeof set.aside === 'string' ? set.aside : ''].join('#');
}

export const useChromeState = () => useContext(StateCtx);
export const useChromeApi = () => useContext(ApiCtx);

/** Shown hints for the current focus. */
export function useVisibleHints(): Hint[] {
  const { hints, focusKey } = useChromeState();
  return pick(hints, focusKey);
}

/** Declares the screen's breadcrumb and hints. Call on every render; updates are cheap. */
export function useScreenChrome(crumbs: Crumb[], hints: HintSet): void {
  const api = useChromeApi();
  const crumbKey = JSON.stringify(crumbs);
  useEffect(() => {
    api.setCrumbs(JSON.parse(crumbKey) as Crumb[]);
  }, [api, crumbKey]);
  useEffect(() => {
    api.setHints(hints);
  });
  useEffect(() => () => api.setHints(EMPTY), [api]);
}
