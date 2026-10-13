// API v3 answers → the shapes the UI uses. Pure, relative imports only: the node tests load this file. Missing names
// get stand-ins in the language in use when the answer is read.
import { t } from '../i18n/store.ts';
import type { HubComment, HubCommentRaw, HubCommentThread, HubHandler, HubHandlerDetailRaw } from './types.ts';

const num = (v: unknown, fallback = 0) => (typeof v === 'number' && Number.isFinite(v) ? v : fallback);
const str = (v: unknown, fallback = '') => (typeof v === 'string' ? v : fallback);
const opt = (v: unknown) => (typeof v === 'string' && v ? v : undefined);
const bool = (v: unknown) => (typeof v === 'boolean' ? v : undefined);

/** A `HandlerSummary` (catalogue) or `HandlerDetail` (with notes and game summary). */
export function normalizeHandler(raw: HubHandlerDetailRaw): HubHandler {
  const createdAt = str(raw.createdAt, str(raw.updatedAt, new Date(0).toISOString()));
  const updatedAt = str(raw.updatedAt, createdAt);
  const maxPlayers = num(raw.maxPlayers, 0);
  return {
    id: raw.id,
    title: str(raw.title, str(raw.gameName, t('common.hubData.untitledHandler'))),
    url: str(raw.url),
    authorId: opt(raw.authorId),
    author: str(raw.authorName, t('common.hubData.unknownAuthor')),
    authorUrl: opt(raw.authorUrl),
    description: str(raw.description),
    gameName: str(raw.gameName, str(raw.title, t('common.hubData.unknownGame'))),
    gameSlug: str(raw.gameSlug),
    gameDescription: str(raw.gameDescription),
    igdbGameId: typeof raw.igdbGameId === 'number' ? raw.igdbGameId : undefined,
    coverImageId: opt(raw.coverImageId),
    genres: Array.isArray(raw.genres) ? raw.genres.filter((g): g is string => typeof g === 'string').slice(0, 3) : [],
    stars: num(raw.stars),
    downloads: num(raw.downloads),
    verified: raw.verified === true,
    comments: Math.max(0, num(raw.comments)),
    version: num(raw.version, 1),
    createdAt,
    updatedAt,
    publishedAt: str(raw.publishedAt, createdAt),
    maxPlayers: maxPlayers > 0 ? maxPlayers : undefined,
    inputs: {
      controllers: bool(raw.inputs?.controllers),
      keyboardMouse: bool(raw.inputs?.keyboardMouse),
      multiKeyboardMouse: bool(raw.inputs?.multiKeyboardMouse),
    },
    trendScore: num(raw.trendScore),
  };
}

/** Only answers with an id are handlers (a guard against odd items). */
export const isHandler = (raw: unknown): raw is HubHandlerDetailRaw =>
  typeof raw === 'object' && raw !== null && typeof (raw as { id?: unknown }).id === 'string' && (raw as { id: string }).id.length > 0;

export function normalizeComment(raw: HubCommentRaw): HubComment {
  return {
    id: raw.id,
    authorId: opt(raw.authorId),
    author: str(raw.authorName, t('common.hubData.someone')),
    handlerId: str(raw.handlerId),
    createdAt: str(raw.createdAt, new Date(0).toISOString()),
    editedAt: opt(raw.editedAt),
    content: str(raw.body),
    replyTo: opt(raw.parentId),
  };
}

/** Groups replies under their parent. Threads newest first, replies oldest first. Orphans become threads. */
export function threadComments(comments: HubComment[]): HubCommentThread[] {
  const byId = new Map(comments.map((c) => [c.id, c]));
  const threads = new Map<string, HubCommentThread>();
  for (const c of comments) {
    if (!c.replyTo || !byId.has(c.replyTo)) threads.set(c.id, { ...c, replies: [] });
  }
  for (const c of comments) {
    if (c.replyTo && byId.has(c.replyTo)) {
      // Walk up to the thread root (v3 replies point at the root already; older data may chain).
      let root = byId.get(c.replyTo)!;
      while (root.replyTo && byId.has(root.replyTo)) root = byId.get(root.replyTo)!;
      threads.get(root.id)?.replies.push(c);
    }
  }
  const list = [...threads.values()].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
  list.forEach((t) => t.replies.sort((a, b) => a.createdAt.localeCompare(b.createdAt)));
  return list;
}
