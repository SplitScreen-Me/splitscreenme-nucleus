// English, built into the UI from the app's own file (Master/i18n/en.json, also copied next to NucleusCoop.exe): the
// per-key fallback of every language, and what screens show before the host answers.
import englishFile from '../../../i18n/en.json' with { type: 'json' };
import { flattenMessages, type Messages } from './core.ts';

/** English strings by dotted key. */
export const english: Messages = Object.freeze(flattenMessages(englishFile));

/** Metadata of the English file. */
export const englishMeta = englishFile._meta;
