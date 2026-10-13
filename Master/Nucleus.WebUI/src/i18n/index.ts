// Translations of the UI. Guide: docs/webui/i18n.md. In components:
//
//   const { t, format } = useI18n();          // or: const t = useT();
//   t('common.cancel')                         // typed keys: an unknown key is a compile error
//   t('shell.titleBar.freeDevices', { count }) // plural forms chosen by count; placeholders are checked too
//   <T k="lobby.lockHint" values={{ key: 'End' }} />   // strings with tags (<kbd>, <b>...)
//   format.compact(508_000), format.bytes(n), format.date(iso), format.duration(seconds), format.list(names)
//
// Outside React (event handlers, lib code): `t` and `format` from here read the language in use.
export { I18nProvider, RichText, T, useI18n, useT, type I18nProviderProps, type I18nValue, type RichTags, type RichTextProps, type RichValue, type TagRenderer, type TProps } from './react.tsx';
export { activateLanguage, format, getI18n, isMessageKey, subscribeI18n, t, tDynamic, type I18nSnapshot } from './store.ts';
export { DEFAULT_LOCALE, matchLocale, stripTags, type Messages, type Params, type ParamValue } from './core.ts';
export type { DateInput, Formatters } from './format.ts';
export type { MessageKey, MessageParams, TFunction } from './types.ts';
export type { RichMessageKey } from './generated/keys.ts';
