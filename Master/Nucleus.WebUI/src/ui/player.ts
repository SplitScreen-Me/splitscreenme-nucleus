/** Static class names so Tailwind sees them all. Index 0 = player 1. */
const PLAYER_CLASSES = ['player-1', 'player-2', 'player-3', 'player-4', 'player-5', 'player-6', 'player-7', 'player-8'] as const;

/** Class that sets --pc to player n's colour (1–8). Out-of-range numbers wrap. */
export function playerClass(n: number): string {
  return PLAYER_CLASSES[(((n - 1) % 8) + 8) % 8] ?? 'player-1';
}

/** CSS colour value of player n, for inline styles (SVG fills, canvas). */
export function playerColor(n: number): string {
  return `var(--color-p${(((n - 1) % 8) + 8) % 8 + 1})`;
}

export const MAX_PLAYER_COLOURS = 8;
