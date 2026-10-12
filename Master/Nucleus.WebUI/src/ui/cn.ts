/** Joins class names, skipping falsy values. No merging: callers own conflicts. */
export function cn(...parts: Array<string | false | null | undefined | 0>): string {
  return parts.filter(Boolean).join(' ');
}
