import type { MessageParamsMap } from './generated/keys.ts';

/** Every key of Master/i18n/en.json (plural forms under their base key: `library.gameCount`, not `_one`). */
export type MessageKey = keyof MessageParamsMap;

/** Placeholder values a key takes; `undefined` when it has none. */
export type MessageParams<K extends MessageKey> = MessageParamsMap[K];

/**
 * The rest arguments of `t(key, ...)`: nothing for a key without placeholders, its values otherwise. A key typed as any
 * `MessageKey` (computed at runtime) takes optional values; an unknown key falls here too, so the compiler reports
 * the key itself rather than a missing argument.
 */
export type ParamsArgs<K extends MessageKey> = MessageKey extends K
  ? [params?: MessageParamsMap[MessageKey]]
  : [MessageParamsMap[K]] extends [undefined]
    ? [params?: undefined]
    : [params: MessageParamsMap[K]];

/**
 * Translates a key of en.json in the language in use: plain text (tags removed), placeholders replaced, the plural
 * form chosen by `count`. Unknown keys and wrong or missing values are compile errors.
 */
export type TFunction = <K extends MessageKey>(key: K, ...params: ParamsArgs<K>) => string;
