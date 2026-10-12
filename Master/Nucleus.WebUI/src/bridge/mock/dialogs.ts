import type { BridgeHandlers } from '../generated/contract.ts';
import { sleep } from './util.ts';

/** Executable names a filter label lists, for example "Elden Ring executable (eldenring.exe, start_protected_game.exe)". */
const namesInLabel = (label: string | undefined) => [...(label ?? '').matchAll(/([\w .+-]+\.exe)\b/gi)].map((m) => (m[1] ?? '').trim());

/**
 * Mock of the `dialogs` service: returns plausible paths after a short delay, as if the user picked them. When the
 * filter label names the executables the UI expects, the "picked" file is the first of them, in the initial folder.
 */
export function createDialogsMock(): BridgeHandlers['dialogs'] {
  return {
    pickFile: async ({ filters, initialDirectory }) => {
      await sleep(250);
      const folder = (initialDirectory ?? 'D:\\Games\\Picked').replace(/\\+$/, '');
      const expected = namesInLabel(filters[0]?.name)[0];
      if (expected) return `${folder}\\${expected}`;
      const extension = filters[0]?.extensions[0] ?? 'exe';
      return `${folder}\\game.${extension}`;
    },
    pickFolder: async ({ initialDirectory }) => {
      await sleep(250);
      return initialDirectory ?? 'D:\\Games';
    },
  };
}
