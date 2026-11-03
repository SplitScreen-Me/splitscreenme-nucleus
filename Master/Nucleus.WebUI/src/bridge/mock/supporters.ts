// The Patreon supporters credits. The host downloads the list from GitHub and keeps a copy; the mock serves invented
// names. `localStorage['nucleus.mock.supporters']` picks the answer:
//   (unset)   a fresh download
//   'cache'   the saved copy (offline)
//   'error'   no download and no copy (`unavailable`)
//   'slow'    a fresh download after 2 s (to see the loading state)
// The music is served by the Vite dev server from NucleusCoopTool\gui (vite.config.ts, devGuiAssets).
import { BridgeError } from '../errors.ts';
import type { BridgeHandlers, SupportersList } from '../generated/contract.ts';

/** Where the dev server serves `NucleusCoopTool\gui\audio\patreon_credits.mp3`. */
export const MOCK_MUSIC_URL = '/__gui/audio/patreon_credits.mp3';

/** Invented names (the real list is public data, but tests and screenshots don't need it), in several scripts. */
export const MOCK_SUPPORTERS: readonly string[] = [
  'Mara Quill', 'Tobias Renn', 'Lukáš Novák', 'Juniper Vale', 'Hiro Tanaka', 'Amélie Fortin', 'Oskar Lind', 'Priya Natarajan',
  'Couch Commander', 'Diego Salvatierra', 'Nadia Petrova', 'Финн Орлов', 'Grace Okafor', 'Sven Halvorsen', 'Isabela Rocha',
  'Theo Marchetti', 'Wren Ashdown', 'Kenji Mori', 'Splitscreen Sam', 'Lea Brunner', 'Mateo Ibarra', 'Zoë Lambert',
  'Ravi Shah', 'Elif Demir', 'Owen Pryce', 'Camille Roussel', 'Bram de Vries', 'Ingrid Solberg', 'Yusuf Aydın',
  'The Lan Party Crew', 'Chiara Bellini', 'Anton Weber', 'Mei Lin', 'Jonah Fairweather', 'Sofia Andersson', 'Pavel Kral',
  'Lucía Moreno', 'Quentin Lefèvre', 'Aiko Suzuki', 'Rafael Costa', 'Hannah Kowalski', 'Emeka Obi', 'Freya Nilsen',
  'Marcus Thorne', 'Valentina Ruiz', 'Dmitri Volkov', 'Ines Ferreira', 'Felix Hartmann', 'Noor Haddad', 'Arlo Bennett',
  'Saoirse Kelly', 'Tomasz Zielinski', 'Beatriz Lima', 'Henrik Dahl', 'Olivia Grant', 'Kai Nakamura', 'Margaux Petit',
  'Adebayo Ade', 'Clara Schulz', 'Rowan Hale', 'Yara Mansour', 'Elias Berg', 'Paloma Vidal', 'Nico Ferraro',
  'Агата Смирнова', 'Leon Fischer', 'Ruby Castellanos', 'Hugo Martin', 'Selin Kaya', 'Jasper Quinn', 'Maëlle Garnier',
  'Two Pads One Sofa', 'Ida Johansson', 'Bruno Alves', 'Keira Walsh', 'Emil Novak', 'Ana Paula Souza', 'Lars Eriksen',
];

function readMode(): string | null {
  try {
    return globalThis.localStorage?.getItem('nucleus.mock.supporters') ?? null;
  } catch {
    return null;
  }
}

/** Mock of the `supporters` service. */
export function createSupportersMock(mode: () => string | null = readMode): BridgeHandlers['supporters'] {
  return {
    get: async (): Promise<SupportersList> => {
      const which = mode();
      if (which === 'error') {
        throw new BridgeError('unavailable', "The supporters list can't be downloaded right now. Check your internet connection and try again.");
      }
      if (which === 'slow') await new Promise((resolve) => setTimeout(resolve, 2000));
      const names = [...MOCK_SUPPORTERS];
      return which === 'cache'
        ? { names, fromCache: true, savedAt: '2026-10-28T19:42:00.000Z', musicUrl: MOCK_MUSIC_URL }
        : { names, fromCache: false, musicUrl: MOCK_MUSIC_URL };
    },
  };
}
