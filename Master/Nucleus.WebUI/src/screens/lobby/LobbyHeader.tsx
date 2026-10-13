import { useId } from 'react';
import type { GameDetails, GameSummary, ProfileSummary, SetupSnapshot } from '@/bridge';
import { format, t, useT } from '@/i18n';
import { Button, ButtonLink, Icon, MenuButton, Pane } from '@/ui';

export interface LobbyHeaderProps {
  snap: SetupSnapshot;
  details?: GameDetails;
  summary?: GameSummary;
  profiles: ProfileSummary[];
  onOpenNotes: () => void;
  onLoadProfile: (profileId: number) => void;
  onUnloadProfile: () => void;
  /** Opens the list of profiles: settings, delete. */
  onManageProfiles: () => void;
}

/** A profile's title, or "Profile 3" when it has none (in the language in use; callers re-render through their hooks). */
export const profileTitle = (p: ProfileSummary) => p.title.trim() || t('profiles.defaultTitle', { number: p.id });

/** "2 players, 1 gamepad, starts by itself. Notes": a list of facts, joined the language's way. */
function profileDescription(p: ProfileSummary): string {
  const parts = [t('profiles.summary.players', { count: p.players })];
  if (p.gamepads) parts.push(t('profiles.summary.gamepads', { count: p.gamepads }));
  if (p.keyboardMice) parts.push(t('profiles.summary.keyboardMice', { count: p.keyboardMice }));
  if (p.autoPlay) parts.push(t('profiles.summary.autoPlay'));
  const summary = format.list(parts, 'unit');
  const notes = p.notes.trim();
  return notes ? t('profiles.summary.withNotes', { summary, notes }) : summary;
}

/** Header strip: which game, which handler, which profile. */
export function LobbyHeader({ snap, details, summary, profiles, onOpenNotes, onLoadProfile, onUnloadProfile, onManageProfiles }: LobbyHeaderProps) {
  const t = useT();
  const labelId = useId();
  const cover = summary?.coverUrl ?? details?.summary.coverUrl ?? summary?.iconUrl;
  const version = summary?.handlerVersion ?? details?.summary.handlerVersion;
  const match = snap.profile;
  const loaded = match.loaded ? profiles.find((p) => p.id === match.profileId) : undefined;
  const profilesEnabled = details?.profilesEnabled ?? true;

  return (
    <Pane as="section" aria-label={t('lobby.header.label')} className="z-[5] col-span-full flex min-h-[76px] items-center gap-3.5 overflow-visible py-[11px] pr-3.5 pl-[11px]">
      <ButtonLink to="/library" variant="quiet" size="icon" aria-label={t('lobby.backToLibrary')} data-nav>
        <Icon name="arrow-l" />
      </ButtonLink>
      {cover ? (
        <img src={cover} alt="" className="h-[53px] w-10 shrink-0 rounded-md bg-pane-2 object-cover" />
      ) : (
        <span className="h-[53px] w-10 shrink-0 rounded-md bg-pane-2" aria-hidden="true" />
      )}
      <div className="min-w-0">
        <h1 className="truncate text-26 leading-none font-[800] tracking-[-0.02em] stretch-wide">{snap.gameName}</h1>
        <p className="mt-[7px] flex items-center gap-3.5 text-small whitespace-nowrap text-ink-2">
          <span className="data">{version !== undefined ? t('lobby.header.handlerVersion', { version: String(version) }) : t('lobby.header.handlerFromFile')}</span>
          {details?.maintainer && <span className="max-[1180px]:hidden">{t('lobby.header.maintainer', { name: details.maintainer })}</span>}
          <span className="max-[1300px]:hidden">{t('lobby.header.maxPlayers', { count: snap.capabilities.maxPlayers })}</span>
          {snap.capabilities.playersPerInstance > 1 && (
            <span className="max-[1400px]:hidden">{t('lobby.header.playersPerCopy', { count: snap.capabilities.playersPerInstance })}</span>
          )}
        </p>
      </div>
      <div className="ml-auto flex shrink-0 items-center gap-2">
        {details?.notes && (
          <Button variant="ghost" size="sm" onClick={onOpenNotes} data-nav className="[&_svg]:size-4">
            <Icon name="book" />
            {t('lobby.header.notes')}
          </Button>
        )}
        {match.loaded && (
          <span className="data hidden items-center gap-1.5 rounded-tag bg-pane-2 px-3 py-1.5 text-13 text-ink-2 min-[1240px]:inline-flex" aria-live="polite">
            <Icon name="users" className="size-4 text-ink-3" />
            {t(match.autoPlay ? 'profiles.status.assignedAutoPlay' : 'profiles.status.assigned', { assigned: match.assigned, total: match.total })}
          </span>
        )}
        {profilesEnabled ? (
          <>
            <span id={labelId} className="text-small text-ink-3 max-[1300px]:hidden">
              {t('profiles.picker.label')}
            </span>
            <MenuButton
              label={
                match.loaded
                  ? loaded
                    ? profileTitle(loaded)
                    : match.profileId !== undefined
                      ? t('profiles.defaultTitle', { number: match.profileId })
                      : t('profiles.picker.unknown')
                  : t(profiles.length ? 'profiles.picker.none' : 'profiles.picker.noneSaved')
              }
              menuLabel={t('profiles.picker.menuLabel')}
              describedBy={labelId}
              items={[
                { id: 'none', label: t('profiles.picker.none'), description: t('profiles.picker.noneDescription'), checked: !match.loaded },
                ...profiles.map((p) => ({ id: String(p.id), label: profileTitle(p), description: profileDescription(p), checked: match.profileId === p.id })),
              ]}
              onSelect={(id) => {
                if (id === 'none') {
                  if (match.loaded) onUnloadProfile();
                } else if (Number(id) !== match.profileId) onLoadProfile(Number(id));
              }}
            />
            <Button
              variant="quiet"
              size="icon"
              aria-label={t('profiles.picker.manage')}
              title={t('profiles.picker.manageHint')}
              onClick={onManageProfiles}
              data-nav
              className="-ml-1"
            >
              <Icon name="sliders" />
            </Button>
          </>
        ) : (
          <span className="text-small text-ink-3">{t('profiles.picker.disabled')}</span>
        )}
      </div>
    </Pane>
  );
}
