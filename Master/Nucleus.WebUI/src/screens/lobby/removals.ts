// Devices the user just took out of the setup: their activity does not make them join again at once (a pad that is
// still in someone's hands, or a drifting stick, would rejoin right after being removed).
const COOLDOWN_MS = 6000;
const removedAt = new Map<string, number>();

export function markRemoved(deviceIds: readonly string[]): void {
  const now = performance.now();
  for (const id of deviceIds) removedAt.set(id, now);
}

export function recentlyRemoved(deviceId: string): boolean {
  const at = removedAt.get(deviceId);
  if (at === undefined) return false;
  if (performance.now() - at < COOLDOWN_MS) return true;
  removedAt.delete(deviceId);
  return false;
}

/** The device joined or was placed on purpose: forget its removal. */
export const forgetRemoved = (deviceId: string) => removedAt.delete(deviceId);
