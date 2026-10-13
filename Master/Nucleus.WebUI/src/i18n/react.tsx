// React side of the translations: the provider that follows the host's language, the hooks, and <T> for rich text.
import {
  createContext,
  createElement,
  Fragment,
  isValidElement,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useSyncExternalStore,
  type ReactElement,
  type ReactNode,
} from 'react';
import { useQueryClient, type Query } from '@tanstack/react-query';
import { bridge, bridgeServices, useBridgeEvent, useBridgeQuery, type LanguageState } from '@/bridge';
import { DEFAULT_LOCALE, parseRich, type Params, type ParamValue, type RichNode } from './core.ts';
import { english } from './english.ts';
import { activateLanguage, getI18n, subscribeI18n, type I18nSnapshot } from './store.ts';
import type { MessageKey, MessageParams, TFunction } from './types.ts';

/** How a tag of a rich string renders: `(children) => <Kbd>{children}</Kbd>`. */
export type TagRenderer = (children: ReactNode) => ReactNode;

/** Renderers by tag name (`kbd`, `b`, `link`...). */
export type RichTags = Readonly<Record<string, TagRenderer>>;

/** What `useI18n()` gives. */
export interface I18nValue extends I18nSnapshot {
  /** Available languages, the one matching Windows, whether the user has chosen. Undefined until the host answers. */
  state?: LanguageState;
  /** Saves `locale` as the user's language and switches to it (the page follows through `i18n.languageChanged`). */
  setLanguage: (locale: string) => Promise<void>;
  /** Tag renderers for `<T>` and `<RichText>` (plain elements, overridden by I18nProvider's `tags`). */
  tags: RichTags;
}

const BASE_TAGS: RichTags = {
  b: (c) => createElement('strong', null, c),
  strong: (c) => createElement('strong', null, c),
  i: (c) => createElement('em', null, c),
  em: (c) => createElement('em', null, c),
  kbd: (c) => createElement('kbd', null, c),
  code: (c) => createElement('code', null, c),
  br: () => createElement('br'),
};

const I18nContext = createContext<Omit<I18nValue, keyof I18nSnapshot> | null>(null);

/** Prefix of the hub queries (`hubKeys` in src/hub/queries.ts; not imported: src/hub imports this module). */
const HUB_QUERIES = ['hub'] as const;

/** Query keys of bridge methods whose answers hold host texts: all of them except the handshake and i18n itself. */
function holdsHostText(query: Query): boolean {
  const [method] = query.queryKey;
  if (typeof method !== 'string' || method === 'app.hello') return false;
  const [service, name] = method.split('.') as [string, string | undefined];
  if (service === 'i18n' || !name) return false;
  return Object.hasOwn(bridgeServices, service) && Object.hasOwn(bridgeServices[service as keyof typeof bridgeServices], name);
}

export interface I18nProviderProps {
  children: ReactNode;
  /** Shown until the strings of the host's language are in (only on a start in a language not cached yet). */
  loading?: ReactNode;
  /** App-wide tag renderers (for example `kbd` as the design system's Kbd). */
  tags?: RichTags;
}

/**
 * Follows the host's language: loads its strings (English is built in), switches the page, and after
 * `i18n.languageChanged` refetches every bridge query, since host texts come in the language in use. Place it inside
 * BridgeProvider.
 */
export function I18nProvider({ children, loading = null, tags }: I18nProviderProps): ReactNode {
  const queryClient = useQueryClient();
  const snapshot = useSyncExternalStore(subscribeI18n, getI18n, getI18n);
  const state = useBridgeQuery('i18n.getState', undefined, { staleTime: Infinity });
  const current = state.data?.current;
  const foreign = current !== undefined && current !== DEFAULT_LOCALE;
  const strings = useBridgeQuery('i18n.getStrings', { locale: current ?? DEFAULT_LOCALE }, { enabled: foreign, staleTime: Infinity });
  const rtl = state.data?.languages.find((l) => l.locale === current)?.rightToLeft ?? false;

  useEffect(() => {
    if (current === undefined) return;
    if (!foreign || strings.isError) activateLanguage(DEFAULT_LOCALE, english);
    else if (strings.data?.locale === current) activateLanguage(current, strings.data.strings, rtl);
  }, [current, foreign, rtl, strings.data, strings.isError]);

  useBridgeEvent('i18n.languageChanged', () => {
    void queryClient.invalidateQueries({ predicate: holdsHostText });
    // Hub data holds stand-ins worded when it was fetched (hub.fallbacks.*): fetch it again (the browser cache
    // revalidates with the ETags). Hub errors word themselves when read (HubError).
    void queryClient.invalidateQueries({ queryKey: HUB_QUERIES });
  });

  const setLanguage = useCallback(
    async (locale: string) => {
      await bridge.i18n.setLanguage({ locale });
      // Choosing the language already in use changes nothing but "chosen": no event for it.
      await queryClient.invalidateQueries({ queryKey: ['i18n.getState'] });
    },
    [queryClient],
  );

  const value = useMemo(() => ({ state: state.data, setLanguage, tags: { ...BASE_TAGS, ...tags } }), [state.data, setLanguage, tags]);
  // The first render waits for the host's language; later switches keep the page (and its state) in the old language
  // until the new strings are in.
  const started = useRef(false);
  if (state.isError || (current !== undefined && (snapshot.locale === current || (foreign && strings.isError)))) started.current = true;
  return createElement(I18nContext.Provider, { value }, started.current ? children : loading);
}

/**
 * The language in use: `t`, `format`, `locale`, the language state and `setLanguage`. Re-renders the component when
 * the language changes. Works outside I18nProvider too (startup screens), without the state.
 */
export function useI18n(): I18nValue {
  const snapshot = useSyncExternalStore(subscribeI18n, getI18n, getI18n);
  const extra = useContext(I18nContext);
  return useMemo(
    () => ({
      ...snapshot,
      state: extra?.state,
      tags: extra?.tags ?? BASE_TAGS,
      setLanguage: extra?.setLanguage ?? ((locale: string) => bridge.i18n.setLanguage({ locale })),
    }),
    [snapshot, extra],
  );
}

/** `t` of the language in use; the component re-renders when it changes. */
export function useT(): TFunction {
  return useSyncExternalStore(subscribeI18n, getI18n, getI18n).t;
}

/** A value of a rich string: text, a number (formatted for the language) or an element. */
export type RichValue = ParamValue | ReactElement;

type RichValues<K extends MessageKey> = [MessageParams<K>] extends [undefined]
  ? { values?: undefined }
  : { values: { readonly [P in keyof NonNullable<MessageParams<K>>]: NonNullable<MessageParams<K>>[P] | ReactElement } };

/** Props of {@link T}. */
export type TProps<K extends MessageKey> = {
  /** Key in en.json. */
  k: K;
  /** Renderers of this string's tags, over the app-wide ones. */
  tags?: RichTags;
} & RichValues<K>;

/**
 * A string with markup: `"Press <kbd>{key}</kbd> to lock"` →
 * `<T k="lobby.lockHint" values={{ key: 'End' }} />`. Tags render through `tags` (app-wide defaults: b, strong, i, em,
 * kbd, code, br; an unknown tag renders its content only), values may be elements. Nothing is ever parsed as HTML.
 */
export function T<K extends MessageKey>(props: TProps<K>): ReactNode {
  const { translator } = useI18n();
  const values = props.values as Readonly<Record<string, RichValue>> | undefined;
  const template = translator.template(props.k, plainParams(values)) ?? props.k;
  return createElement(RichText, { text: template, values, tags: props.tags });
}

/** Props of {@link RichText}. */
export interface RichTextProps {
  /** A string with tags and placeholders, for example a text the host built. */
  text: string;
  values?: Readonly<Record<string, RichValue>>;
  tags?: RichTags;
}

/** Renders a string with tags and placeholders (a host text, or a template from `<T>`), never as HTML. */
export function RichText({ text, values, tags }: RichTextProps): ReactNode {
  const { format, tags: appTags } = useI18n();
  const renderers = tags ? { ...appTags, ...tags } : appTags;
  const formatValue = (value: RichValue): ReactNode => (typeof value === 'number' ? format.number(value) : value);
  return createElement(Fragment, null, ...renderNodes(parseRich(text), values, renderers, formatValue));
}

function renderNodes(
  nodes: readonly RichNode[],
  values: Readonly<Record<string, RichValue>> | undefined,
  tags: RichTags,
  formatValue: (value: RichValue) => ReactNode,
): ReactNode[] {
  return nodes.map((node, index) => {
    if (typeof node === 'string') return createElement(Fragment, { key: index }, ...fillPlaceholders(node, values, formatValue));
    const children = renderNodes(node.children, values, tags, formatValue);
    const render = tags[node.tag];
    return createElement(Fragment, { key: index }, render ? render(createElement(Fragment, null, ...children)) : children);
  });
}

function fillPlaceholders(text: string, values: Readonly<Record<string, RichValue>> | undefined, formatValue: (value: RichValue) => ReactNode): ReactNode[] {
  if (!values || !text.includes('{')) return [text];
  const out: ReactNode[] = [];
  let last = 0;
  for (const match of text.matchAll(/\{([A-Za-z_][A-Za-z0-9_]*)\}/g)) {
    const value = values[match[1]!];
    if (value === undefined) continue;
    out.push(text.slice(last, match.index), isValidElement(value) ? createElement(Fragment, { key: match.index }, value) : formatValue(value));
    last = match.index + match[0].length;
  }
  out.push(text.slice(last));
  return out;
}

/** The values that select a plural form (numbers and strings), without the elements. */
function plainParams(values: Readonly<Record<string, RichValue>> | undefined): Params | undefined {
  if (!values) return undefined;
  const plain: Record<string, ParamValue> = {};
  for (const [name, value] of Object.entries(values)) if (typeof value === 'number' || typeof value === 'string') plain[name] = value;
  return plain;
}
