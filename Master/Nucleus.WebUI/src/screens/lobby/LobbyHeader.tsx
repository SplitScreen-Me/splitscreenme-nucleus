import { useId } from 'react';
import type { GameDetails, GameSummary, ProfileSummary, SetupSnapshot } from '@/bridge';
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

export const profileTitle = (p: ProfileSummary) => p.title.trim() || `Profile ${p.id}`;

function profileDescription(p: ProfileSummary): string {
  const parts = [`${p.players} ${p.players === 1 ? 'player' : 'players'}`];
  if (p.gamepads) parts.push(`${p.gamepads} ${p.gamepads === 1 ? 'gamepad' : 'gamepads'}`);
  if (p.keyboardMice) parts.push(`${p.keyboardMice} keyboard and mouse`);
  if (p.autoPlay) parts.push('starts by itself');
  return parts.join(', ') + (p.notes.trim() ? `. ${p.notes.trim()}` : '');
}

/** Header strip: which game, which handler, which profile. */
export function LobbyHeader({ snap, details, summary, profiles, onOpenNotes, onLoadProfile, onUnloadProfile, onManageProfiles }: LobbyHeaderProps) {
  const labelId = useId();
  const cover = summary?.coverUrl ?? details?.summary.coverUrl ?? summary?.iconUrl;
  const version = summary?.handlerVersion ?? details?.summary.handlerVersion;
  const match = snap.profile;
  const loaded = match.loaded ? profiles.find((p) => p.id === match.profileId) : undefined;
  const profilesEnabled = details?.profilesEnabled ?? true;

  return (
    <Pane as="section" aria-label="Game" className="z-[5] col-span-full flex min-h-[76px] items-center gap-3.5 overflow-visible py-[11px] pr-3.5 pl-[11px]">
      <ButtonLink to="/library" variant="quiet" size="icon" aria-label="Back to library" data-nav>
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
          <span className="data">{version !== undefined ? `Handler version ${version}` : 'Handler installed from a file'}</span>
          {details?.maintainer && <span className="max-[1180px]:hidden">by {details.maintainer}</span>}
          <span className="max-[1300px]:hidden">Up to {snap.capabilities.maxPlayers} players</span>
          {snap.capabilities.playersPerInstance > 1 && (
            <span className="max-[1400px]:hidden">{snap.capabilities.playersPerInstance} players per copy</span>
          )}
        </p>
      </div>
      <div className="ml-auto flex shrink-0 items-center gap-2">
        {details?.notes && (
          <Button variant="ghost" size="sm" onClick={onOpenNotes} data-nav className="[&_svg]:size-4">
            <Icon name="book" />
            Handler notes
          </Button>
        )}
        {match.loaded && (
          <span className="data hidden items-center gap-1.5 rounded-tag bg-pane-2 px-3 py-1.5 text-13 text-ink-2 min-[1240px]:inline-flex" aria-live="polite">
            <Icon name="users" className="size-4 text-ink-3" />
            {match.assigned} of {match.total} in{match.autoPlay ? ', starts by itself' : ''}
          </span>
        )}
        {profilesEnabled ? (
          <>
            <span id={labelId} className="text-small text-ink-3 max-[1300px]:hidden">
              Profile
            </span>
            <MenuButton
              label={match.loaded ? (loaded ? profileTitle(loaded) : `Profile ${match.profileId ?? ''}`) : profiles.length ? 'No profile' : 'No saved profile'}
              menuLabel="Profiles"
              describedBy={labelId}
              items={[
                { id: 'none', label: 'No profile', description: 'Place players freely', checked: !match.loaded },
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
              aria-label="Manage profiles"
              title="Profile settings, delete a profile"
              onClick={onManageProfiles}
              data-nav
              className="-ml-1"
            >
              <Icon name="sliders" />
            </Button>
          </>
        ) : (
          <span className="text-small text-ink-3">Profiles are off for this game</span>
        )}
      </div>
    </Pane>
  );
}
