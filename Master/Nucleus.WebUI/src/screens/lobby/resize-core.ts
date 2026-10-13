// Resizing a player's area in the lobby: pointer drags and arrow keys turned into `setup.resize` steps. Erasable
// TypeScript with type-only imports, so src/bridge/__tests__/resize-core.test.ts runs it under `node --test`.
import type { Edge, Rect } from '../../bridge/generated/contract.ts';

/** An area inside a monitor, as shares of its width and height. */
export interface Frac {
  left: number;
  top: number;
  width: number;
  height: number;
}

/** One resize step as a share of the monitor: `x` of its width (left and right edges), `y` of its height. */
export interface StepFrac {
  x: number;
  y: number;
}

/** The corner the arrow keys move in resize mode. */
export type Corner = 'bottomRight' | 'topLeft';

const EPSILON = 1e-6;

const horizontal = (edge: Edge) => edge === 'left' || edge === 'right';

/** A step of a monitor as shares of it, from the host's step in pixels; null where players can't be resized. */
export function stepFracOf(screen: { bounds: Pick<Rect, 'width' | 'height'>; resizeStepWidth: number; resizeStepHeight: number }): StepFrac | null {
  const { bounds, resizeStepWidth: w, resizeStepHeight: h } = screen;
  if (!(w > 0 && h > 0 && bounds.width > 0 && bounds.height > 0)) return null;
  return { x: w / bounds.width, y: h / bounds.height };
}

/**
 * The steps an edge may move by and keep the area on the monitor and at least one step wide and high: positive grows,
 * negative shrinks. The host has its own rules (other players, minimum sizes): this only keeps the preview sensible.
 */
export function clampSteps(frac: Frac, edge: Edge, steps: number, step: StepFrac): number {
  const size = horizontal(edge) ? step.x : step.y;
  if (!(size > 0) || !Number.isFinite(steps)) return 0;
  const length = horizontal(edge) ? frac.width : frac.height;
  const room = edge === 'left' ? frac.left : edge === 'top' ? frac.top : edge === 'right' ? 1 - frac.left - frac.width : 1 - frac.top - frac.height;
  const grow = Math.max(0, Math.floor(room / size + EPSILON));
  const shrink = Math.max(0, Math.floor((length - size) / size + EPSILON));
  const clamped = Math.max(-shrink, Math.min(grow, Math.trunc(steps)));
  return clamped === 0 ? 0 : clamped;
}

/** Whether two areas share more than an edge. */
export function overlaps(a: Frac, b: Frac): boolean {
  return (
    a.left < b.left + b.width - EPSILON &&
    b.left < a.left + a.width - EPSILON &&
    a.top < b.top + b.height - EPSILON &&
    b.top < a.top + a.height - EPSILON
  );
}

/** Growing steps cut where the area would run into another player's (the host stops at that step too). */
export function clampToOthers(frac: Frac, edge: Edge, steps: number, step: StepFrac, others: readonly Frac[]): number {
  for (let k = 1; k <= steps; k++) {
    const grown = movedFrac(frac, edge, k, step);
    if (others.some((other) => overlaps(grown, other))) return k - 1;
  }
  return steps;
}

/**
 * Whole steps a dragged edge moved (`dx`, `dy`: the pointer's move as shares of the monitor), kept on the monitor and
 * out of the other players' areas.
 */
export function dragSteps(frac: Frac, edge: Edge, dx: number, dy: number, step: StepFrac, others: readonly Frac[] = []): number {
  const along = edge === 'left' ? -dx : edge === 'right' ? dx : edge === 'top' ? -dy : dy;
  const size = horizontal(edge) ? step.x : step.y;
  if (!(size > 0)) return 0;
  return clampToOthers(frac, edge, clampSteps(frac, edge, Math.round(along / size), step), step, others);
}

/** The area with an edge moved by `steps` (positive grows), as the host would draw it if every step succeeds. */
export function movedFrac(frac: Frac, edge: Edge, steps: number, step: StepFrac): Frac {
  const n = clampSteps(frac, edge, steps, step);
  switch (edge) {
    case 'left':
      return { ...frac, left: frac.left - n * step.x, width: frac.width + n * step.x };
    case 'right':
      return { ...frac, width: frac.width + n * step.x };
    case 'top':
      return { ...frac, top: frac.top - n * step.y, height: frac.height + n * step.y };
    case 'bottom':
      return { ...frac, height: frac.height + n * step.y };
  }
}

/**
 * What an arrow key does in resize mode: it moves the active corner that way. The bottom right corner grows the area
 * to the right and down; the top left corner grows it to the left and up.
 */
export function arrowResize(corner: Corner, key: string): { edge: Edge; steps: 1 | -1 } | null {
  const bottomRight = corner === 'bottomRight';
  switch (key) {
    case 'ArrowRight':
      return bottomRight ? { edge: 'right', steps: 1 } : { edge: 'left', steps: -1 };
    case 'ArrowLeft':
      return bottomRight ? { edge: 'right', steps: -1 } : { edge: 'left', steps: 1 };
    case 'ArrowDown':
      return bottomRight ? { edge: 'bottom', steps: 1 } : { edge: 'top', steps: -1 };
    case 'ArrowUp':
      return bottomRight ? { edge: 'bottom', steps: -1 } : { edge: 'top', steps: 1 };
    default:
      return null;
  }
}

/** The other corner. */
export const otherCorner = (corner: Corner): Corner => (corner === 'bottomRight' ? 'topLeft' : 'bottomRight');

/** The size in pixels an area of a monitor stands for, for the label shown while resizing. */
export function pixelSize(frac: Frac, monitor: Pick<Rect, 'width' | 'height'>): { width: number; height: number } {
  return { width: Math.round(frac.width * monitor.width), height: Math.round(frac.height * monitor.height) };
}
