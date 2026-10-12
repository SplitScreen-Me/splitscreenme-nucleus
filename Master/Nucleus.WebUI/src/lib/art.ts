/** IGDB image sizes we use. See https://api-docs.igdb.com/#images */
export type IgdbSize = 'cover_small_2x' | 'cover_big' | 'cover_big_2x' | 'screenshot_med' | 'screenshot_big' | '720p' | '1080p';

/** IGDB image URL from an image id (Hub `gameCover`, screenshot `image_id`). */
export function igdbImage(imageId: string | null | undefined, size: IgdbSize = 'cover_big_2x'): string | undefined {
  return imageId ? `https://images.igdb.com/igdb/image/upload/t_${size}/${imageId}.jpg` : undefined;
}

export type SteamArtKind = 'hero' | 'capsule' | 'header';

const STEAM_FILE: Record<SteamArtKind, string> = {
  hero: 'library_hero.jpg',
  capsule: 'library_600x900.jpg',
  header: 'header.jpg',
};

/** Steam CDN art by app id (for example the store id of a Steam game found on this PC). */
export function steamArt(appId: number | null | undefined, kind: SteamArtKind): string | undefined {
  return appId ? `https://cdn.akamai.steamstatic.com/steam/apps/${appId}/${STEAM_FILE[kind]}` : undefined;
}
