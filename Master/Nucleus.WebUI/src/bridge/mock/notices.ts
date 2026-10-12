// Notices the host raises outside a session (handler load or script errors, an unloaded profile, locked game
// files): they wait until the user dismisses them. The mock starts with none; `seedNotices` adds realistic ones.
import type { AppNotice, BridgeHandlers } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import type { MockState } from './state.ts';

type NewNotice = Omit<AppNotice, 'id' | 'at'>;

/** Notices `seedNotices` adds: a handler that does not compile, and a profile that could not be applied. */
const SAMPLE_NOTICES: readonly NewNotice[] = [
  {
    level: 'error',
    source: 'handler',
    title: 'Error in handler',
    message: [
      'There is an error in the game handler Broken.js. The game this handler is for will not appear in the list. If the issue has been fixed, try reloading the handlers.',
      '',
      'Error: SyntaxError: Unexpected token ")" at line 42, column 17',
      '',
      'Line 40: Game.SupportsMultipleKeyboardsAndMice = true;',
      'Line 41: Game.HookInit = true;',
      'Line 42: Game.Play = function () {)',
      'Line 43:     Context.ModifySaveFile(Context.SavePath, Context.SavePath, SaveType.INI, [',
      'Line 44:         new Nucleus.IniSaveInfo("Network", "Port", Context.Options["Port"]),',
      '',
      'Handler file: C:\\Nucleus Co-op\\handlers\\Broken.js',
    ].join('\n'),
  },
  {
    level: 'warning',
    source: 'setup',
    title: 'Profile unloaded',
    message: 'The profile has been unloaded: its devices are no longer connected.\nPlug them in and load the profile again.',
  },
];

/** Adds the sample notices, at startup (before the UI connects, so without an event). */
export function seedNotices(state: MockState): void {
  for (const notice of SAMPLE_NOTICES) state.notices.push({ ...notice, id: `notice-${++state.noticeSeq}`, at: new Date().toISOString() });
}

/** Mock of the `notices` service. */
export function createNoticesMock(context: MockContext, state: MockState): BridgeHandlers['notices'] {
  return {
    list: () => state.notices,
    dismiss: ({ id }) => {
      const index = state.notices.findIndex((notice) => notice.id === id);
      if (index < 0) return;
      state.notices.splice(index, 1);
      context.emit('notices.changed', { count: state.notices.length });
    },
  };
}
