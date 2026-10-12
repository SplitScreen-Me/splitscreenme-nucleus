import type { HubComment, HubCommentRaw, HubCommentThread, HubHandler, HubHandlerRaw } from './types';

const num = (v: unknown, fallback = 0) => (typeof v === 'number' && Number.isFinite(v) ? v : fallback);
const str = (v: unknown, fallback = '') => (typeof v === 'string' ? v : fallback);
const bool = (v: unknown) => (typeof v === 'boolean' ? v : undefined);

export function normalizeHandler(raw: HubHandlerRaw): HubHandler {
  const updatedAt = str(raw.updatedAt, str(raw.createdAt, new Date(0).toISOString()));
  const maxPlayers = num(raw.maxPlayers, 0);
  return {
    id: raw._id,
    ownerId: str(raw.owner),
    author: str(raw.ownerName, 'Unknown author'),
    title: str(raw.title, str(raw.gameName, 'Untitled handler')),
    description: str(raw.description),
    gameName: str(raw.gameName, str(raw.title, 'Unknown game')),
    gameDescription: str(raw.gameDescription),
    gameUrl: raw.gameUrl || undefined,
    igdbGameId: typeof raw.gameId === 'number' ? raw.gameId : undefined,
    coverImageId: raw.gameCover || undefined,
    genres: Array.isArray(raw.genres) ? raw.genres.filter((g): g is string => typeof g === 'string').slice(0, 3) : [],
    stars: num(raw.stars),
    downloads: num(raw.downloadCount),
    verified: raw.verified === true,
    comments: Math.max(0, num(raw.commentCount)),
    version: num(raw.currentVersion, 1),
    packageId: raw.currentPackage || undefined,
    createdAt: str(raw.createdAt, updatedAt),
    updatedAt,
    publishedAt: str(raw.lastPublicationAt, updatedAt),
    maxPlayers: maxPlayers > 0 ? maxPlayers : undefined,
    inputs: {
      controllers: bool(raw.playableControllers),
      keyboardMouse: bool(raw.playableMouseKeyboard),
      multiKeyboardMouse: bool(raw.playableMultiMouseKeyboard),
    },
    trendScore: num(raw.trendScore),
  };
}

/** Only public, authorised handlers are listed (the endpoint already filters; this is a guard). */
export function isListable(raw: HubHandlerRaw): boolean {
  return raw.private !== true && raw.publicAuthorized !== false && typeof raw._id === 'string';
}

export function normalizeComment(raw: HubCommentRaw): HubComment {
  return {
    id: raw._id,
    ownerId: str(raw.owner),
    author: str(raw.ownerName, 'Someone'),
    handlerId: str(raw.handlerId),
    createdAt: str(raw.createdAt, new Date(0).toISOString()),
    content: str(raw.content),
    replyTo: raw.replyTo || undefined,
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
      // Walk up to the thread root (replies to replies).
      let root = byId.get(c.replyTo)!;
      while (root.replyTo && byId.has(root.replyTo)) root = byId.get(root.replyTo)!;
      threads.get(root.id)?.replies.push(c);
    }
  }
  const list = [...threads.values()].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
  list.forEach((t) => t.replies.sort((a, b) => a.createdAt.localeCompare(b.createdAt)));
  return list;
}
