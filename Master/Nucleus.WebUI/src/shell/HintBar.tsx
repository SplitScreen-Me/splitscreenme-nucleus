import { useAppInfo } from '@/lib/live';
import { GamepadGlyph, Kbd } from '@/ui';
import { useChromeState, useVisibleHints } from './chrome';

/** Gamepad / keyboard legend for the current screen, always at the bottom of the window. */
export function HintBar() {
  const hints = useVisibleHints();
  const { hints: set } = useChromeState();
  const { data: info } = useAppInfo();
  const version = info ? `Nucleus Co-op ${info.appVersion}` : '';

  return (
    <footer className="flex min-w-0 items-center gap-6 px-3.5 text-13 font-[560] text-ink-2 [grid-area:hints]" aria-label="Controls">
      <ul className="m-0 flex min-w-0 list-none items-center gap-6 overflow-hidden p-0" aria-label="Button hints">
        {hints.map((h) => (
          <li key={`${h.button ?? h.key}-${h.label}`} className="inline-flex shrink-0 items-center gap-2 whitespace-nowrap">
            {h.button ? <GamepadGlyph button={h.button} /> : <Kbd>{h.key}</Kbd>}
            {h.label}
          </li>
        ))}
      </ul>
      <span className="ml-auto flex shrink-0 items-center gap-4 whitespace-nowrap text-ink-3 [&_b]:font-semibold [&_b]:text-ink-2">
        {set.aside}
        <span>{version}</span>
      </span>
    </footer>
  );
}
