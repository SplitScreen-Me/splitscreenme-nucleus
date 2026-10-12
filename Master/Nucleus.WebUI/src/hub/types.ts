// Public Hub API (https://hub.splitscreen.me/api/v1/*). Raw shapes as served, then the normalised
// shapes the UI uses. Older documents may lack genres, maxPlayers, playable* and trendScore.

export interface HubHandlerRaw {
  _id: string;
  owner?: string;
  ownerName?: string;
  createdAt?: string;
  updatedAt?: string;
  title?: string;
  description?: string;
  gameName?: string;
  gameDescription?: string;
  gameUrl?: string;
  gameId?: number;
  gameCover?: string;
  genres?: string[];
  stars?: number;
  downloadCount?: number;
  verified?: boolean;
  private?: boolean;
  publicAuthorized?: boolean;
  lastPublicationAt?: string;
  commentCount?: number;
  currentVersion?: number;
  currentPackage?: string;
  maxPlayers?: number;
  playableControllers?: boolean;
  playableMouseKeyboard?: boolean;
  playableMultiMouseKeyboard?: boolean;
  trendScore?: number;
}

export interface HubCommentRaw {
  _id: string;
  owner?: string;
  ownerName?: string;
  handlerId?: string;
  createdAt?: string;
  content?: string;
  replyTo?: string;
}

export interface HubInputs {
  /** undefined = the handler document predates the field (unknown). */
  controllers?: boolean;
  keyboardMouse?: boolean;
  multiKeyboardMouse?: boolean;
}

export interface HubHandler {
  id: string;
  ownerId: string;
  author: string;
  title: string;
  /** Handler notes, markdown. */
  description: string;
  gameName: string;
  gameDescription: string;
  gameUrl?: string;
  igdbGameId?: number;
  /** IGDB image id of the cover. */
  coverImageId?: string;
  genres: string[];
  stars: number;
  downloads: number;
  verified: boolean;
  comments: number;
  version: number;
  packageId?: string;
  createdAt: string;
  updatedAt: string;
  /** Last publication of a version (falls back to updatedAt). */
  publishedAt: string;
  maxPlayers?: number;
  inputs: HubInputs;
  trendScore: number;
}

export interface HubComment {
  id: string;
  ownerId: string;
  author: string;
  handlerId: string;
  createdAt: string;
  content: string;
  replyTo?: string;
}

/** A top-level comment with its replies, oldest reply first. */
export interface HubCommentThread extends HubComment {
  replies: HubComment[];
}
