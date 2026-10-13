// Public hub API v3 (<hubUrl>/api/v3, contract: /api/v3/openapi.json). Raw shapes as served (the fields the UI
// reads), then the normalised shapes the UI uses.

export interface HubInputsRaw {
  /** null = unknown: the handler was published before the hub asked. */
  controllers?: boolean | null;
  keyboardMouse?: boolean | null;
  multiKeyboardMouse?: boolean | null;
}

/** `HandlerSummary`: a listed handler as `/catalog` and the lists serve it. */
export interface HubHandlerSummaryRaw {
  id: string;
  title?: string;
  /** Canonical web page: the game page for the primary handler of a game, the handler page otherwise. */
  url?: string;
  gameName?: string;
  gameSlug?: string;
  igdbGameId?: number | null;
  coverImageId?: string | null;
  genres?: string[];
  /** null when the account was deleted. */
  authorId?: string | null;
  authorName?: string;
  /** null when the account was deleted or suspended. */
  authorUrl?: string | null;
  version?: number;
  verified?: boolean;
  maxPlayers?: number;
  inputs?: HubInputsRaw;
  downloads?: number;
  stars?: number;
  comments?: number;
  trendScore?: number;
  createdAt?: string;
  updatedAt?: string;
  publishedAt?: string;
}

/** `HandlerDetail` (`/handlers/{id}`): the summary plus the page content. */
export interface HubHandlerDetailRaw extends HubHandlerSummaryRaw {
  /** Handler notes by the author, markdown. */
  description?: string;
  gameDescription?: string | null;
}

/** `/catalog`: every listed handler, trending first. */
export interface HubCatalogRaw {
  generatedAt?: string;
  count?: number;
  items?: HubHandlerSummaryRaw[];
}

/** `Comment`: plain text, replies one level deep and flat. */
export interface HubCommentRaw {
  id: string;
  handlerId?: string;
  /** Root comment this one replies to; null for a root (and for a reply whose root is gone). */
  parentId?: string | null;
  authorId?: string | null;
  authorName?: string;
  body?: string;
  createdAt?: string;
  editedAt?: string | null;
}

/** `/handlers/{id}/comments`: newest first, `nextCursor` (the `before` of the next page) or null on the last page. */
export interface HubCommentPageRaw {
  items?: HubCommentRaw[];
  nextCursor?: string | null;
}

export interface HubInputs {
  /** undefined = unknown (the handler predates the question). */
  controllers?: boolean;
  keyboardMouse?: boolean;
  multiKeyboardMouse?: boolean;
}

export interface HubHandler {
  id: string;
  title: string;
  /** Web page on the site (game page or handler page), opened in the browser. */
  url: string;
  /** undefined when the account was deleted. */
  authorId?: string;
  author: string;
  /** Public profile page; undefined when the account was deleted or suspended. */
  authorUrl?: string;
  /** Handler notes, markdown. Empty in the catalogue: only the handler detail has them. */
  description: string;
  gameName: string;
  gameSlug: string;
  /** IGDB game summary. Empty in the catalogue. */
  gameDescription: string;
  igdbGameId?: number;
  /** IGDB image id of the cover. */
  coverImageId?: string;
  genres: string[];
  stars: number;
  downloads: number;
  verified: boolean;
  comments: number;
  /** Current version number. */
  version: number;
  createdAt: string;
  updatedAt: string;
  /** Last time the handler was made public (falls back to createdAt). */
  publishedAt: string;
  maxPlayers?: number;
  inputs: HubInputs;
  /** Downloads over the last 3 days. */
  trendScore: number;
}

export interface HubComment {
  id: string;
  /** undefined when the account was deleted. */
  authorId?: string;
  author: string;
  handlerId: string;
  createdAt: string;
  editedAt?: string;
  /** Plain text: render as text, never as HTML. */
  content: string;
  replyTo?: string;
}

/** A top-level comment with its replies, oldest reply first. */
export interface HubCommentThread extends HubComment {
  replies: HubComment[];
}
