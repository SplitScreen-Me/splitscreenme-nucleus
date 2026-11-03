// The Patreon supporters credits, from Settings > About: names rolling upward to music, as the legacy credits window
// (Patrons.cs) did, in a pane of the stage. Rules: supporters-core.ts. The host downloads the list and keeps a copy.
import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { bridge, useBridgeQuery } from '@/bridge';
import { T, useI18n } from '@/i18n';
import { errorMessage, useRun } from '@/lib/bridge-ui';
import { LINKS } from '@/lib/links';
import { useReducedMotion } from '@/lib/motion';
import { useScreenChrome } from '@/shell/chrome';
import { Button, cn, Icon, Pane, Spinner } from '@/ui';
import {
  CREDITS_FADE_MS,
  CREDITS_START_HOLD_MS,
  CREDITS_VOLUME,
  creditsStep,
  MUSIC_CREDIT_URL,
  readMuted,
  supportersView,
  writeMuted,
  type ScrollState,
} from './supporters-core';

function storage(): Storage | null {
  try {
    return globalThis.localStorage ?? null;
  } catch {
    return null;
  }
}

/** The list changes a few times a month: fetched once per app run, again with Retry. */
const STALE_MS = 30 * 60_000;

export default function SupportersScreen() {
  const { t } = useI18n();
  const navigate = useNavigate();
  const query = useBridgeQuery('supporters.get', undefined, { staleTime: STALE_MS, timeoutMs: 20_000 });
  const view = supportersView(query);
  const music = useCreditsMusic(view.kind === 'ready' ? view.musicUrl : null);
  const reduced = useReducedMotion();

  const back = useCallback(() => {
    if (window.history.length > 1) void navigate(-1);
    else void navigate('/settings#about');
  }, [navigate]);

  useScreenChrome([{ label: t('settings.title'), to: '/settings#about' }, { label: t('settings.supporters.crumb') }], {
    base: [
      { button: 'A', label: t('common.select') },
      ...(music.available
        ? [{ button: 'X' as const, label: music.playing ? t('settings.supporters.muteShort') : t('settings.supporters.unmuteShort'), action: music.toggle }]
        : []),
      { button: 'B', label: t('common.back') },
    ],
    aside:
      view.kind === 'ready' ? (
        <span>
          {reduced ? (
            <T k="settings.supporters.keysStill" values={{ mute: 'M', back: 'Esc' }} tags={{ b: (c) => <b>{c}</b> }} />
          ) : (
            <T k="settings.supporters.keys" values={{ pause: 'Space', mute: 'M', back: 'Esc' }} tags={{ b: (c) => <b>{c}</b> }} />
          )}
        </span>
      ) : undefined,
  });

  useKeys({ back, mute: music.available ? music.toggle : undefined });

  if (view.kind === 'loading') {
    return (
      <Pane className="grid h-full place-items-center text-small text-ink-2">
        <span role="status" className="inline-flex items-center gap-2.5">
          <Spinner /> {t('settings.supporters.loading')}
        </span>
      </Pane>
    );
  }

  if (view.kind === 'error') {
    return (
      <Pane className="grid h-full place-items-center p-8 text-center">
        <div role="alert" className="grid max-w-[52ch] justify-items-center gap-3">
          <Icon name="offline" className="size-7 text-warn" />
          <h1 className="type-h3">{t('settings.supporters.errorTitle')}</h1>
          <p className="text-small text-ink-2">{errorMessage(query.error)}</p>
          <div className="mt-2 flex gap-2">
            <Button variant="primary" size="sm" data-nav data-autofocus onClick={() => void query.refetch()} disabled={query.isFetching}>
              {query.isFetching ? <Spinner /> : <Icon name="refresh" />}
              {t('common.retry')}
            </Button>
            <Button variant="ghost" size="sm" data-nav onClick={back}>
              <Icon name="arrow-l" />
              {t('common.back')}
            </Button>
          </div>
        </div>
      </Pane>
    );
  }

  return <Credits names={view.names} savedAt={view.savedAt} music={music} onBack={back} />;
}

interface CreditsMusic {
  /** A track to play: the file exists and loads. */
  available: boolean;
  playing: boolean;
  toggle: () => void;
  element: ReactNode;
}

/**
 * The credits music: starts with the screen unless it was muted last time (remembered), fades in, loops, stops with
 * the screen. Autoplay may be refused (no user gesture yet); the toggle then reads "Music" and starts it.
 */
function useCreditsMusic(url: string | null): CreditsMusic {
  const audio = useRef<HTMLAudioElement>(null);
  const [playing, setPlaying] = useState(false);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    const a = audio.current;
    if (!a || !url) return;
    if (!readMuted(storage())) void play(a);
    return () => a.pause();
  }, [url]);

  const toggle = useCallback(() => {
    const a = audio.current;
    if (!a) return;
    const mute = !a.paused;
    writeMuted(storage(), mute);
    if (mute) a.pause();
    else void play(a);
  }, []);

  const element = url ? (
    <audio
      ref={audio}
      src={url}
      loop
      preload="auto"
      onPlay={() => setPlaying(true)}
      onPause={() => setPlaying(false)}
      onError={() => {
        setFailed(true);
        setPlaying(false);
      }}
    />
  ) : null;

  return { available: Boolean(url) && !failed, playing, toggle, element };
}

async function play(a: HTMLAudioElement): Promise<void> {
  a.volume = 0;
  try {
    await a.play();
  } catch {
    return; // refused (autoplay) or no file: the toggle stays on "Music"
  }
  const start = performance.now();
  const ramp = () => {
    const share = Math.min(1, (performance.now() - start) / CREDITS_FADE_MS);
    a.volume = CREDITS_VOLUME * share;
    if (share < 1 && !a.paused) requestAnimationFrame(ramp);
  };
  requestAnimationFrame(ramp);
}

/** Esc / Backspace go back, M mutes (not while typing or with a dialog open). Space is handled by the roll. */
function useKeys({ back, mute }: { back: () => void; mute?: () => void }) {
  const live = useRef({ back, mute });
  live.current = { back, mute };
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (e.defaultPrevented || e.ctrlKey || e.altKey || e.metaKey) return;
      if (target?.closest('input, textarea, select, [contenteditable="true"]') || document.querySelector('dialog[open]')) return;
      if (e.key === 'Escape' || e.key === 'Backspace') {
        e.preventDefault();
        live.current.back();
      } else if (e.key.toLowerCase() === 'm' && live.current.mute) {
        e.preventDefault();
        live.current.mute();
      }
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, []);
}

function Credits({ names, savedAt, music, onBack }: { names: string[]; savedAt: string | null; music: CreditsMusic; onBack: () => void }) {
  const { t, format } = useI18n();
  const run = useRun();
  const reduced = useReducedMotion();
  const scroller = useRef<HTMLElement>(null);
  const [hovered, setHovered] = useState(false);
  const [focusedInside, setFocusedInside] = useState(false);
  const [held, setHeld] = useState(false);
  const [ended, setEnded] = useState(false);
  const [rollKey, setRollKey] = useState(0);
  const paused = hovered || focusedInside || held;
  const live = useRef({ paused });
  live.current = { paused };

  // The roll takes the focus so the stick, the wheel and the arrow keys scroll it at once (it is the screen's content).
  useEffect(() => {
    scroller.current?.focus({ preventScroll: true });
  }, []);

  // The roll: one step per frame (creditsStep), from the title to the thanks. Reduced motion leaves a plain list.
  useEffect(() => {
    const el = scroller.current;
    if (!el || reduced) return;
    setEnded(false);
    let state: ScrollState = { pos: el.scrollTop, holdUntil: performance.now() + CREDITS_START_HOLD_MS };
    let last = 0;
    let frame = requestAnimationFrame(function step(now) {
      frame = requestAnimationFrame(step);
      const dt = last ? now - last : 0;
      last = now;
      const out = creditsStep(state, {
        top: el.scrollTop,
        max: el.scrollHeight - el.clientHeight,
        dt,
        now,
        paused: live.current.paused || document.hidden,
      });
      state = out.state;
      if (out.write !== null) el.scrollTop = out.write;
      setEnded(out.ended);
    });
    return () => cancelAnimationFrame(frame);
  }, [reduced, rollKey]);

  const replay = () => {
    const el = scroller.current;
    if (el) el.scrollTop = 0;
    setHeld(false);
    setRollKey((n) => n + 1);
    el?.focus({ preventScroll: true });
  };

  const open = (url: string) => void run(() => bridge.app.openExternal({ url }));
  const rolling = !reduced && !ended;

  return (
    <Pane className="flex h-full flex-col">
      {music.element}
      <header className="z-10 flex shrink-0 items-center gap-3 px-5 pt-4 pb-2">
        <Button variant="quiet" size="sm" data-nav onClick={onBack}>
          <Icon name="arrow-l" />
          {t('common.back')}
        </Button>
        {savedAt !== null && (
          <span className="inline-flex min-w-0 items-center gap-2 text-13 text-ink-3">
            <Icon name="offline" className="size-4 text-warn" />
            <span className="truncate">{t('settings.supporters.savedCopy', { date: savedAt ? format.date(savedAt, 'medium') : '…' })}</span>
          </span>
        )}
        <span className="ml-auto flex items-center gap-2">
          {rolling && paused && (
            <span role="status" className="rounded-tag bg-pane-3 px-2.5 py-1 text-12 font-[650] text-ink-2">
              {t('settings.supporters.paused')}
            </span>
          )}
          {music.available && (
            <Button
              variant="ghost"
              size="sm"
              data-nav
              aria-pressed={!music.playing}
              aria-label={music.playing ? t('settings.supporters.mute') : t('settings.supporters.unmute')}
              title={music.playing ? t('settings.supporters.mute') : t('settings.supporters.unmute')}
              onClick={music.toggle}
            >
              <Icon name={music.playing ? 'speaker' : 'mute'} />
              {music.playing ? t('settings.supporters.muteShort') : t('settings.supporters.unmuteShort')}
            </Button>
          )}
        </span>
      </header>

      <section
        ref={scroller}
        tabIndex={0}
        data-nav-frame="own"
        aria-labelledby="supporters-title"
        className="scroll-none relative min-h-0 flex-1 overflow-y-auto outline-none [mask-image:linear-gradient(180deg,transparent,#000_14%,#000_86%,transparent)]"
        onKeyDown={(e) => {
          if (e.key === ' ' && e.target === e.currentTarget) {
            e.preventDefault();
            setHeld((h) => !h);
          }
        }}
        onFocus={(e) => setFocusedInside(e.target !== e.currentTarget)}
        onBlur={(e) => {
          if (!e.currentTarget.contains(e.relatedTarget as Node | null) || e.relatedTarget === e.currentTarget) setFocusedInside(false);
        }}
      >
        <div className={cn('grid justify-items-center px-8 text-center', reduced ? 'py-14' : 'pt-[30vh] pb-[26vh]')}>
          <Icon name="heart" className="mb-5 size-9 text-ink" />
          <h1 id="supporters-title" className="type-display max-w-[16ch] stretch-wide">
            {t('settings.supporters.title')}
          </h1>
          <p className="mt-5 max-w-[56ch] text-17 text-balance text-ink-2">{t('settings.supporters.intro')}</p>
          <p className="data mt-3 text-15 text-ink-3">{t('settings.supporters.count', { count: names.length })}</p>

          <ul
            className="m-0 mt-20 grid w-full max-w-[1000px] list-none grid-cols-[repeat(auto-fill,minmax(200px,1fr))] gap-x-8 gap-y-4 p-0"
            onPointerMove={(e) => {
              // A real move: Chrome also sends still pointer events while content scrolls under the cursor.
              if (e.movementX || e.movementY) setHovered(true);
            }}
            onPointerLeave={() => setHovered(false)}
          >
            {names.map((name, i) => (
              <li key={`${i}:${name}`} className="min-w-0 text-18 leading-snug font-[620] [overflow-wrap:anywhere] text-ink-2">
                {name}
              </li>
            ))}
          </ul>

          <p className="type-h1 mt-32 stretch-wide">{t('settings.supporters.thanks')}</p>
          <Button variant="primary" size="lg" className="mt-10" data-nav onClick={() => open(LINKS.patreon)}>
            <Icon name="heart" />
            {t('settings.supporters.patreon')}
          </Button>
          <p className="mt-8 text-14 text-ink-3">
            <T
              k="settings.supporters.musicCredit"
              values={{
                link: (
                  <button
                    type="button"
                    data-nav
                    className="rounded border-0 bg-transparent p-0 font-mono text-13 text-ink-2 underline decoration-line-strong underline-offset-4 hover:text-ink"
                    onClick={() => open(MUSIC_CREDIT_URL)}
                  >
                    {MUSIC_CREDIT_URL}
                  </button>
                ),
              }}
            />
          </p>
          {!reduced && (
            <Button variant="ghost" size="sm" className="mt-8" data-nav onClick={replay}>
              <Icon name="refresh" />
              {t('settings.supporters.replay')}
            </Button>
          )}
        </div>
      </section>
    </Pane>
  );
}
