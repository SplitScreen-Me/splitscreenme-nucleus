// Checks shared by the save operations (settings, players, profile settings). Each returns what is wrong, or
// undefined when the value is valid.

/** Longest nickname a game accepts. */
const NICKNAME_MAX = 16;

/** Problem with a number that must be in `[min, max]` (and a multiple of `step` from `min`, when given). */
export function rangeProblem(value: number, min: number, max: number, step?: number): string | undefined {
  if (!Number.isFinite(value)) return 'Enter a number.';
  if (value < min || value > max) return `Must be between ${min} and ${max}.`;
  if (step && Math.abs(Math.round((value - min) / step) * step - (value - min)) > 1e-9) return `Must be a multiple of ${step}.`;
  return undefined;
}

/** Problem with a player nickname (empty means the default). */
export function nicknameProblem(nickname: string): string | undefined {
  if (nickname.length > NICKNAME_MAX) return `A nickname can be ${NICKNAME_MAX} characters at most.`;
  if (/[\\/:*?"<>|]/.test(nickname)) return 'A nickname cannot contain \\ / : * ? " < > |.';
  return undefined;
}

/** Problem with a Steam ID (empty means the default). */
export function steamIdProblem(steamId: string): string | undefined {
  return steamId === '' || /^7656119\d{10}$/.test(steamId) ? undefined : 'A Steam ID has 17 digits and starts with 7656119.';
}

/** Problem with an absolute Windows path. */
export function pathProblem(path: string): string | undefined {
  return /^[A-Za-z]:\\/.test(path) || path.startsWith('\\\\') ? undefined : 'Enter an absolute path, for example D:\\Games.';
}
