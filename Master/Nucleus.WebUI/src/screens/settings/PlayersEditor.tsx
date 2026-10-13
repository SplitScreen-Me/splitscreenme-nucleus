import { useId, useMemo, useState } from 'react';
import { bridge, useBridgeQuery, type PlayerIdentity, type ValidationError } from '@/bridge';
import { useT } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { Button, cn, Icon, PlayerTag, Spinner, useToast } from '@/ui';

const FIRST = 8;

/**
 * Global player slots: the nickname and Steam ID each player number uses in game. Edited as one list and saved with
 * one button; the host validates the whole list and saves nothing while a row is wrong.
 */
export function PlayersEditor() {
  const { data, isPending, isError } = useBridgeQuery('settings.getPlayers');
  const t = useT();
  const run = useRun();
  const toast = useToast();
  const listId = useId();
  const [draft, setDraft] = useState<PlayerIdentity[] | null>(null);
  const [errors, setErrors] = useState<ValidationError[]>([]);
  const [showAll, setShowAll] = useState(false);
  const [saving, setSaving] = useState(false);

  const players = draft ?? data?.players ?? [];
  const dirty = draft !== null && JSON.stringify(draft) !== JSON.stringify(data?.players ?? []);
  const shown = showAll ? players : players.slice(0, FIRST);
  const rowErrors = useMemo(() => {
    const byRow = new Map<number, ValidationError[]>();
    for (const e of errors) {
      if (e.index === undefined) continue;
      byRow.set(e.index, [...(byRow.get(e.index) ?? []), e]);
    }
    return byRow;
  }, [errors]);
  const general = errors.filter((e) => e.index === undefined);
  // Rows with an error stay visible even when the list is folded.
  const hiddenErrors = [...rowErrors.keys()].filter((i) => i >= shown.length).length;

  const edit = (index: number, patch: Partial<PlayerIdentity>) => {
    setDraft(players.map((p, i) => (i === index ? { ...p, ...patch } : p)));
  };

  const save = async () => {
    if (!draft || !data) return;
    setSaving(true);
    const problems = await run(() => bridge.settings.setPlayers({ players: draft, nicknameHistory: data.nicknameHistory }));
    setSaving(false);
    if (problems === undefined) return;
    setErrors(problems);
    if (problems.length === 0) {
      setDraft(null);
      toast(t('settings.players.saved'));
    } else if (problems.some((p) => p.index !== undefined && p.index >= FIRST)) {
      setShowAll(true);
    }
  };

  if (isPending) {
    return (
      <p className="flex items-center gap-2 border-t border-line py-4 text-small text-ink-2">
        <Spinner /> {t('settings.players.loading')}
      </p>
    );
  }
  if (isError || !data) {
    return <p className="border-t border-line py-4 text-small text-ink-2">{t('settings.players.error')}</p>;
  }

  return (
    <div className="border-t border-line pt-4 pb-5 first:border-t-0 first:pt-1" aria-labelledby={`${listId}-title`} role="group">
      <div className="mb-3 flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <h3 id={`${listId}-title`} className="text-15 font-semibold">
          {t('settings.players.editorTitle')}
        </h3>
        <p className="text-13 text-ink-3">{t('settings.players.editorHint')}</p>
      </div>
      <datalist id={`${listId}-names`}>
        {data.nicknameHistory.map((n) => (
          <option key={n} value={n} />
        ))}
      </datalist>
      <div className="grid grid-cols-[44px_minmax(0,1fr)_minmax(0,1.1fr)] items-center gap-x-3 gap-y-1.5">
        <span />
        <span className="text-12 font-[650] text-ink-3">{t('settings.players.nickname')}</span>
        <span className="text-12 font-[650] text-ink-3">{t('settings.players.steamId')}</span>
        {shown.map((p, i) => {
          const errs = rowErrors.get(i) ?? [];
          const bad = (field: string) => errs.some((e) => e.field.endsWith(field));
          return (
            <div key={p.number} className="contents">
              <PlayerTag n={p.number} size="md" className="justify-self-start" />
              <input
                value={p.nickname}
                // i18n-ignore: the nickname the game gets when the slot is empty (PlayerIdentitiesRules.DefaultNickname), not a text
                placeholder={`Player${p.number}`}
                list={`${listId}-names`}
                spellCheck={false}
                aria-label={t('settings.players.nicknameLabel', { number: p.number })}
                aria-invalid={bad('nickname') || undefined}
                onChange={(e) => edit(i, { nickname: e.target.value })}
                className={cn(cellInput, bad('nickname') && invalid)}
              />
              <input
                value={p.steamId}
                placeholder={t('common.default')}
                inputMode="numeric"
                spellCheck={false}
                aria-label={t('settings.players.steamIdLabel', { number: p.number })}
                aria-invalid={bad('steamId') || undefined}
                onChange={(e) => edit(i, { steamId: e.target.value.trim() })}
                className={cn(cellInput, 'data text-15 tabular-nums', bad('steamId') && invalid)}
              />
              {errs.length > 0 && (
                <p role="alert" className="col-start-2 col-end-4 -mt-0.5 mb-1 flex items-start gap-1.5 text-13 font-[560] text-danger">
                  <Icon name="warning" className="mt-px size-[15px] shrink-0" />
                  {errs.map((e) => e.message).join(' ')}
                </p>
              )}
            </div>
          );
        })}
      </div>
      {general.length > 0 && (
        <p role="alert" className="mt-3 flex items-start gap-1.5 text-13 font-[560] text-danger">
          <Icon name="warning" className="mt-px size-[15px] shrink-0" />
          {general.map((e) => e.message).join(' ')}
        </p>
      )}
      <div className="mt-4 flex flex-wrap items-center gap-2">
        {players.length > FIRST && (
          <Button variant="quiet" size="sm" onClick={() => setShowAll((s) => !s)} data-nav>
            <Icon name="chev-d" className={cn('transition-transform duration-(--dur-2)', showAll && 'rotate-180')} />
            {showAll ? t('settings.players.showFirst', { count: FIRST }) : t('settings.players.showAll', { count: players.length })}
            {hiddenErrors > 0 && !showAll && <span className="text-danger">{t('settings.players.hiddenErrors', { count: hiddenErrors })}</span>}
          </Button>
        )}
        <span className="ml-auto flex gap-2">
          {dirty && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                setDraft(null);
                setErrors([]);
              }}
              data-nav
            >
              {t('settings.players.undo')}
            </Button>
          )}
          <Button variant="primary" size="sm" disabled={!dirty || saving} onClick={() => void save()} data-nav>
            {saving ? <Spinner className="border-on-light/30 border-t-on-light" /> : <Icon name="check" />}
            {t('settings.players.save')}
          </Button>
        </span>
      </div>
    </div>
  );
}

const cellInput =
  'h-9 min-w-0 rounded-md border-0 bg-pane-2 px-2.5 text-15 text-ink shadow-[inset_0_0_0_1px_var(--color-line)] outline-none placeholder:text-ink-3 focus:shadow-[inset_0_0_0_1.5px_var(--color-ink)]';
const invalid = 'shadow-[inset_0_0_0_1.5px_var(--color-danger)] focus:shadow-[inset_0_0_0_1.5px_var(--color-danger)]';
