// Saved profiles of the open game: load or unload one, edit its settings, delete it (renumbering the next ones).
import { useState } from 'react';
import { bridge, type ProfileSummary, type SetupSnapshot } from '@/bridge';
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
    const ok = await confirm({
      title: `Delete ${name}?`,
      message: (
        <>
          {`${name} is deleted for good.`}
          {later && ' The profiles after it are renumbered.'}
          {placed && ' The setup starts over: every player goes back to the tray.'}
        </>
      ),
      confirmLabel: 'Delete',
      danger: true,
      icon: 'trash',
    });
    if (!ok) return;
    const deleted = await run(async () => {
      await bridge.profiles.delete({ profileId: p.id });
      return true;
    });
    if (deleted) toast(`${name} deleted`);
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} title={`Profiles of ${snap.gameName}`} icon="users" className="w-[min(720px,92vw)]">
        <p className="text-13 text-ink-3">
          A profile keeps the layouts, the options and who plays where. Loading one restores them; its players join as they press a button.
        </p>
        {profiles.length === 0 ? (
          <p className="mt-4 rounded-ctl bg-pane-2 px-4 py-3 text-15 text-ink-2">No saved profile yet.</p>
        ) : (
          <ul className="m-0 mt-3 list-none p-0" aria-label="Saved profiles">
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
              <b className="font-[650] text-ink">Next profile. </b>
              {snap.saveProfileOnLaunch
                ? 'The next session saves this setup as a new profile with these settings.'
                : 'Turn on “Save a profile when the session starts” to save this setup as a new profile.'}
            </p>
            <Button variant="ghost" size="sm" onClick={() => void edit(null)} data-nav>
              <Icon name="sliders" />
              Settings
            </Button>
          </div>
        )}
      </Dialog>
      {editing && <ProfileSettingsDialog profileName={editing.name ?? undefined} gameName={snap.gameName} onClose={() => setEditing(null)} />}
    </>
  );
}

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
  const name = profileTitle(p);
  return (
    <li className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-4 border-t border-line py-3.5 first:border-t-0">
      <div className="min-w-0">
        <div className="flex min-w-0 flex-wrap items-center gap-x-2.5 gap-y-1">
          <strong className="truncate text-16 font-[680]">{name}</strong>
          {loaded && <span className="inline-flex h-5 items-center rounded-tag bg-ink px-2 text-11 font-[750] text-on-light">Loaded</span>}
          {p.autoPlay && (
            <span className="inline-flex h-5 items-center gap-1 rounded-tag bg-pane-3 px-2 text-11 font-[650] text-ink-2">
              <Icon name="play" className="size-2.5" />
              Starts by itself
            </span>
          )}
        </div>
        {p.playerList.length > 0 && (
          <ul className="m-0 mt-1.5 flex list-none flex-wrap gap-x-3 gap-y-1 p-0" aria-label={`Players of ${name}`}>
            {p.playerList.map((pl) => (
              <li key={pl.number} className="inline-flex items-center gap-1.5 text-13 text-ink-2">
                <PlayerTag n={pl.number} size="xs" />
                <Icon name={deviceIcon(pl.device)} className="size-3.5 text-ink-3" />
                <span>{pl.nickname?.trim() || `Player ${pl.number}`}</span>
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
            Unload
          </Button>
        ) : (
          <Button variant="ghost" size="sm" onClick={onLoad} data-nav>
            Load
          </Button>
        )}
        <Button
          variant="quiet"
          size="icon"
          aria-label={`Settings of ${name}`}
          title={loaded ? 'Settings' : 'Load it and open its settings'}
          onClick={onEdit}
          data-nav
        >
          <Icon name="sliders" />
        </Button>
        <Button variant="quiet" size="icon" aria-label={`Delete ${name}`} title="Delete" onClick={onDelete} data-nav>
          <Icon name="trash" />
        </Button>
      </div>
    </li>
  );
}
