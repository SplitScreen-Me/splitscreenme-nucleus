// First launch: the user picks the language of the app, the one matching Windows preselected (the app already shows
// it), else English. Afterwards the language is the first setting of Settings > General. See docs/webui/i18n.md.
import { useEffect, useMemo, useRef, useState } from 'react';
import type { LanguageSummary } from '@/bridge';
import { useI18n } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { Button, cn, Dialog, Icon } from '@/ui';

/** The first-launch language picker. Mounted once by the shell; shows while no language is saved. */
export function FirstRunLanguageDialog() {
  const { t, format, state, setLanguage } = useI18n();
  const run = useRun();
  const [picked, setPicked] = useState<string>();
  const [saving, setSaving] = useState(false);

  const open = state !== undefined && !state.chosen;
  const languages = useMemo(
    () => [...(state?.languages ?? [])].sort((a, b) => a.nativeName.localeCompare(b.nativeName)),
    [state?.languages],
  );
  // Until the user picks, the app runs in the language matching Windows, else English: it is the preselected one.
  const selected = picked ?? state?.current ?? state?.detected;
  const selectedLanguage = languages.find((l) => l.locale === selected);
  // "Your Windows language" only on a real match: English as the fallback (Italian Windows, no Italian file) isn't one.
  const windowsLanguage = state?.detectedMatchesWindows ? state.detected : undefined;

  // Focus starts on the preselected language (the dialog would put it on its close button): keys and pads go from there.
  const list = useRef<HTMLUListElement>(null);
  useEffect(() => {
    if (!open) return;
    const frame = requestAnimationFrame(() => list.current?.querySelector<HTMLElement>('[aria-checked="true"]')?.focus());
    return () => cancelAnimationFrame(frame);
  }, [open]);

  const confirm = async (locale: string | undefined) => {
    if (!locale || saving) return;
    setSaving(true);
    await run(() => setLanguage(locale));
    setSaving(false);
  };

  return (
    <Dialog
      open={open}
      // Escape, B and the close button keep the selected language: the app needs one.
      onClose={() => open && void confirm(selected)}
      title={t('language.firstRun.title')}
      icon="globe"
      className="w-[min(560px,90vw)]"
    >
      <div className="grid gap-5">
        <p className="text-15 leading-[1.55] text-ink-2">{t('language.firstRun.description')}</p>
        <ul ref={list} role="radiogroup" aria-label={t('language.firstRun.listLabel')} className="m-0 grid list-none gap-2 p-0">
          {languages.map((language) => (
            <li key={language.locale}>
              <LanguageOption
                language={language}
                selected={language.locale === selected}
                detected={language.locale === windowsLanguage}
                completion={language.completion < 100 ? t('language.firstRun.completion', { percent: format.percent(language.completion / 100) }) : undefined}
                detectedLabel={t('language.firstRun.detected')}
                onPick={() => setPicked(language.locale)}
                onConfirm={() => void confirm(language.locale)}
              />
            </li>
          ))}
        </ul>
        <div className="flex justify-end">
          <Button variant="primary" onClick={() => void confirm(selected)} disabled={saving || !selectedLanguage} data-nav>
            {t('language.firstRun.confirm', { language: selectedLanguage?.nativeName ?? '' })}
          </Button>
        </div>
      </div>
    </Dialog>
  );
}

interface LanguageOptionProps {
  language: LanguageSummary;
  selected: boolean;
  detected: boolean;
  completion?: string;
  detectedLabel: string;
  onPick: () => void;
  onConfirm: () => void;
}

/** One language: its own name (in its own language), its English name, and whether it matches Windows. */
function LanguageOption({ language, selected, detected, completion, detectedLabel, onPick, onConfirm }: LanguageOptionProps) {
  return (
    <button
      type="button"
      role="radio"
      aria-checked={selected}
      onClick={onPick}
      onDoubleClick={onConfirm}
      data-nav
      className={cn(
        'flex w-full items-center gap-3 rounded-ctl border-0 bg-pane-2 px-4 py-3 text-left text-ink transition-[box-shadow,background-color] duration-(--dur-1)',
        selected ? 'shadow-[inset_0_0_0_2px_var(--color-ink)]' : 'hover:bg-pane-3',
      )}
    >
      <span className="grid min-w-0 flex-1 gap-0.5">
        <b lang={language.locale} className="truncate text-16 font-[680]">
          {language.nativeName}
        </b>
        {(language.englishName !== language.nativeName || completion) && (
          <span className="truncate text-13 text-ink-3">{[language.englishName !== language.nativeName && language.englishName, completion].filter(Boolean).join(' · ')}</span>
        )}
      </span>
      {detected && (
        <span className="inline-flex shrink-0 items-center gap-1.5 rounded-tag bg-pane-3 px-2.5 py-1 text-12 font-[650] text-ink-2">
          <Icon name="monitor" className="size-3.5" />
          {detectedLabel}
        </span>
      )}
      <span
        aria-hidden="true"
        className={cn('grid size-6 shrink-0 place-items-center rounded-full', selected ? 'bg-ink text-on-light' : 'shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]')}
      >
        {selected && <Icon name="check" strokeWidth={2.6} className="size-3.5" />}
      </span>
    </button>
  );
}
