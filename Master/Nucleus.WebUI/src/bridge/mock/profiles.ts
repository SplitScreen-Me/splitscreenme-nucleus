import type { BridgeHandlers } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict, notFound } from './failures.ts';
import {
  editableSettings,
  launchProfileSettings,
  profileSummary,
  profilesEnabled,
  storedSettings,
  validateProfileSettings,
} from './profile-model.ts';
import { applyProfile, clearAll, requireEditable, requireSetup, unloadProfile } from './setup-model.ts';
import { publishSetup } from './setup-snapshot.ts';
import type { MockProfile, MockSetup, MockState } from './state.ts';

/**
 * Mock of the `profiles` service, for the game whose setup is open. Loading a profile restores its layouts and
 * options in the setup, whose detection then matches devices to its players; deleting one renumbers the next ones.
 */
export function createProfilesMock(context: MockContext, state: MockState): BridgeHandlers['profiles'] {
  const profilesOf = (setup: MockSetup) => (state.profiles[setup.gameId] ??= []);

  const findProfile = (setup: MockSetup, profileId: number): MockProfile =>
    profilesOf(setup)[profileId - 1] ?? notFound(`${setup.gameName} has no profile ${profileId}.`);

  return {
    list: () => profilesOf(requireSetup(state)).map(profileSummary),
    load: ({ profileId }) => {
      const setup = requireEditable(state);
      if (!profilesEnabled(state, setup.gameId)) conflict('profilesDisabled', `Profiles are disabled for ${setup.gameName}.`);
      applyProfile(setup, findProfile(setup, profileId), profileId);
      return publishSetup(context, state, setup);
    },
    unload: () => {
      const setup = requireEditable(state);
      unloadProfile(setup);
      return publishSetup(context, state, setup);
    },
    delete: ({ profileId }) => {
      const setup = requireEditable(state);
      findProfile(setup, profileId);
      profilesOf(setup).splice(profileId - 1, 1);
      // As the host (GameProfile.DeleteProfile): the setup is reset, every player goes back to the tray.
      clearAll(setup);
      publishSetup(context, state, setup);
      context.emit('profiles.changed', { gameId: setup.gameId });
      context.emit('library.changed', { gameIds: [setup.gameId] });
    },
    getSettings: () => {
      const setup = requireSetup(state);
      return setup.profile
        ? editableSettings(findProfile(setup, setup.profile.id).settings, false)
        : editableSettings(launchProfileSettings(state, setup), true);
    },
    saveSettings: (edited) => {
      const setup = requireEditable(state);
      const loaded = setup.profile ? findProfile(setup, setup.profile.id) : undefined;
      const errors = validateProfileSettings(edited, loaded ? loaded.seats.length : setup.players.length);
      if (errors.length > 0) return errors;
      const stored = storedSettings(edited);
      if (loaded && setup.profile) {
        loaded.settings = stored;
        setup.profile.autoPlay = stored.autoPlay;
      } else {
        setup.pendingSettings = stored;
      }
      publishSetup(context, state, setup);
      context.emit('profiles.changed', { gameId: setup.gameId });
      return [];
    },
  };
}
