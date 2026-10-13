// Saved profiles of the open game: load or unload one, edit its settings, delete it (renumbering the next ones).
import { useState } from 'react';
import { bridge, type ProfileSummary, type SetupSnapshot } from '@/bridge';
import { T, useT, type RichTags } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { deviceIcon } from '@/lib/devices';
import { Button, Dialog, Icon, PlayerTag, useConfirm, useToast } from '@/ui';
import { profileTitle } from './LobbyHeader';
import { ProfileSettingsDialog } from './ProfileSettingsDialog';

export interface ProfilesManagerProps {
  snap: SetupSnapshot;
  profiles: ProfileSummary[];
  /** The list of profiles is open. */
  open: boolean;
  onClose: () => void;
  /** Loads a profile; resolves true once loaded (failures are toasted). */
  onLoad: (profileId: number) => Promise<boolean>;
  onUnload: () => Promise<boolean>;
}

/** The list of profiles and, over it, the settings of one of them. */
export function ProfilesManager({ snap, profiles, open, onClose, onLoad, onUnload }: ProfilesManagerProps) {
  const t = useT();
  const run = useRun();
  const toast = useToast();
  const confirm = useConfirm();
  // Whose settings are open: a profile's name, or null for the profile the next session saves.
  const [editing, setEditing] = useState<{ name: string | null } | null>(null);
  const loadedId = snap.profile.loaded ? snap.profile.profileId : undefined;

  const edit = async (p: ProfileSummary | null) => {
    // The host edits the loaded profile, or the next one when none is loaded.
    if (p && p.id !== loadedId && !(await onLoad(p.id))) return;
    if (!p && loadedId !== undefined && !(await onUnload())) return;
    setEditing({ name: p ? profileTitle(p) : null });
  };

  const remove = async (p: ProfileSummary) => {
    const name = profileTitle(p);
    const later = profiles.some((other) => other.id > p.id);
    const placed = snap.players.length > 0;
    const message = later
      ? placed
        ? 'profiles.delete.messageRenumberedReset'
        : 'profiles.delete.messageRenumbered'
      : placed
        ? 'profiles.delete.messageReset'
        : 'profiles.delete.message';
    const ok = await confirm({
      title: t('profiles.delete.title', { profile: name }),
      message: t(message, { profile: name }),
      confirmLabel: t('common.delete'),
      danger: true,
      icon: 'trash',
    });
    if (!ok) return;
    const deleted = await run(async () => {
      await bridge.profiles.delete({ profileId: p.id });
      return true;
    });
    if (deleted) toast(t('profiles.delete.done', { profile: name }));
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} title={t('profiles.manager.title', { game: snap.gameName })} icon="users" className="w-[min(720px,92vw)]">
        <p className="text-13 text-ink-3">{t('profiles.manager.intro')}</p>
        {profiles.length === 0 ? (
          <p className="mt-4 rounded-ctl bg-pane-2 px-4 py-3 text-15 text-ink-2">{t('profiles.manager.empty')}</p>
        ) : (
          <ul className="m-0 mt-3 list-none p-0" aria-label={t('profiles.manager.listLabel')}>
            {profiles.map((p) => (
              <ProfileRow
                key={p.id}
                profile={p}
                loaded={p.id === loadedId}
                onLoad={() => void onLoad(p.id)}
                onUnload={() => void onUnload()}
                onEdit={() => void edit(p)}
                onDelete={() => void remove(p)}
              />
            ))}
          </ul>
        )}
        {loadedId === undefined && (
          <div className="mt-4 flex flex-wrap items-center gap-x-4 gap-y-2 rounded-ctl bg-pane-2 px-4 py-3">
            <p className="min-w-0 flex-1 text-13 leading-[1.45] text-ink-2">
              {snap.saveProfileOnLaunch ? (
                <T k="profiles.manager.nextProfileSaveOn" tags={NEXT_PROFILE_TAGS} />
              ) : (
                <T k="profiles.manager.nextProfileSaveOff" values={{ setting: t('setup.launch.saveProfile') }} tags={NEXT_PROFILE_TAGS} />
              )}
            </p>
            <Button variant="ghost" size="sm" onClick={() => void edit(null)} data-nav>
              <Icon name="sliders" />
              {t('common.settings')}
            </Button>
          </div>
        )}
      </Dialog>
      {editing && <ProfileSettingsDialog profileName={editing.name ?? undefined} gameName={snap.gameName} onClose={() => setEditing(null)} />}
    </>
  );
}

const NEXT_PROFILE_TAGS: RichTags = { b: (c) => <b className="font-[650] text-ink">{c}</b> };

function ProfileRow({
  profile: p,
  loaded,
  onLoad,
  onUnload,
  onEdit,
  onDelete,
}: {
  profile: ProfileSummary;
  loaded: boolean;
  onLoad: () => void;
  onUnload: () => void;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const t = useT();
  const name = profileTitle(p);
  return (
    <li className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-4 border-t border-line py-3.5 first:border-t-0">
      <div className="min-w-0">
        <div className="flex min-w-0 flex-wrap items-center gap-x-2.5 gap-y-1">
          <strong className="truncate text-16 font-[680]">{name}</strong>
          {loaded && <span className="inline-flex h-5 items-center rounded-tag bg-ink px-2 text-11 font-[750] text-on-light">{t('profiles.manager.loaded')}</span>}
          {p.autoPlay && (
            <span className="inline-flex h-5 items-center gap-1 rounded-tag bg-pane-3 px-2 text-11 font-[650] text-ink-2">
              <Icon name="play" className="size-2.5" />
              {t('profiles.manager.autoPlay')}
            </span>
          )}
        </div>
        {p.playerList.length > 0 && (
          <ul className="m-0 mt-1.5 flex list-none flex-wrap gap-x-3 gap-y-1 p-0" aria-label={t('profiles.manager.playersLabel', { profile: name })}>
            {p.playerList.map((pl) => (
              <li key={pl.number} className="inline-flex items-center gap-1.5 text-13 text-ink-2">
                <PlayerTag n={pl.number} size="xs" />
                <Icon name={deviceIcon(pl.device)} className="size-3.5 text-ink-3" />
                <span>{pl.nickname?.trim() || t('lobby.player.name', { number: pl.number })}</span>
                {pl.guests > 0 && <span className="text-ink-3">+{pl.guests}</span>}
              </li>
            ))}
          </ul>
        )}
        {p.notes.trim() && <p className="mt-1.5 line-clamp-2 text-13 leading-[1.45] text-ink-3">{p.notes.trim()}</p>}
      </div>
      <div className="flex items-center gap-1">
        {loaded ? (
          <Button variant="ghost" size="sm" onClick={onUnload} data-nav>
            {t('profiles.manager.unload')}
          </Button>
        ) : (
          <Button variant="ghost" size="sm" onClick={onLoad} data-nav>
            {t('profiles.manager.load')}
          </Button>
        )}
        <Button
          variant="quiet"
          size="icon"
          aria-label={t('profiles.manager.settingsLabel', { profile: name })}
          title={t(loaded ? 'common.settings' : 'profiles.manager.settingsHint')}
          onClick={onEdit}
          data-nav
        >
          <Icon name="sliders" />
        </Button>
        <Button variant="quiet" size="icon" aria-label={t('profiles.manager.deleteLabel', { profile: name })} title={t('common.delete')} onClick={onDelete} data-nav>
          <Icon name="trash" />
        </Button>
      </div>
    </li>
  );
}
