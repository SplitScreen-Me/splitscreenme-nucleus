// Pure geometry of the setup: layout grids, areas of cells, and their pixel rectangles.
import type { Edge, Rect, ScreenLayout } from '../generated/contract.ts';
import type { Grid, GridArea } from './state.ts';

/** Fine grid of the manual layout. */
const MANUAL_GRID: Grid = { rows: 12, columns: 12 };
/** Size of a player placed on the manual grid: a quarter of the monitor. */
const MANUAL_SPAN = 6;

const LAYOUT_GRIDS: Readonly<Record<Exclude<ScreenLayout, 'custom'>, Grid>> = {
  fullScreen: { rows: 1, columns: 1 },
  dualHorizontal: { rows: 2, columns: 1 },
  dualVertical: { rows: 1, columns: 2 },
  fourPlayers: { rows: 2, columns: 2 },
  sixPlayers: { rows: 2, columns: 3 },
  eightPlayers: { rows: 2, columns: 4 },
  sixteenPlayers: { rows: 4, columns: 4 },
  manual: MANUAL_GRID,
};

/** Grid of a layout. */
export function layoutGrid(layout: ScreenLayout, customGrid: Grid): Grid {
  return layout === 'custom' ? customGrid : LAYOUT_GRIDS[layout];
}

/** Number of cells of a grid. */
export const cellCount = (grid: Grid) => grid.rows * grid.columns;

/** Area of a slot (slots are numbered row by row). */
export const slotArea = (grid: Grid, slotIndex: number): GridArea => ({
  col: slotIndex % grid.columns,
  row: Math.floor(slotIndex / grid.columns),
  colSpan: 1,
  rowSpan: 1,
});

/** Slot of the top-left cell of an area. */
export const slotIndexOf = (grid: Grid, area: GridArea) => area.row * grid.columns + area.col;

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value));

/** Area under a point (0 to 1 across and down the monitor): its cell, or a manual-size area centred on it. */
export function areaAt(grid: Grid, x: number, y: number, manual: boolean): GridArea {
  if (manual) {
    return {
      col: clamp(Math.round(x * grid.columns - MANUAL_SPAN / 2), 0, grid.columns - MANUAL_SPAN),
      row: clamp(Math.round(y * grid.rows - MANUAL_SPAN / 2), 0, grid.rows - MANUAL_SPAN),
      colSpan: MANUAL_SPAN,
      rowSpan: MANUAL_SPAN,
    };
  }
  return {
    col: clamp(Math.floor(x * grid.columns), 0, grid.columns - 1),
    row: clamp(Math.floor(y * grid.rows), 0, grid.rows - 1),
    colSpan: 1,
    rowSpan: 1,
  };
}

/** Whether two areas share a cell. */
export function overlaps(a: GridArea, b: GridArea): boolean {
  return a.col < b.col + b.colSpan && b.col < a.col + a.colSpan && a.row < b.row + b.rowSpan && b.row < a.row + a.rowSpan;
}

/** Whether two areas are the same. */
export function sameArea(a: GridArea, b: GridArea): boolean {
  return a.col === b.col && a.row === b.row && a.colSpan === b.colSpan && a.rowSpan === b.rowSpan;
}

/** Whether an area lies inside a grid and covers at least one cell. */
export function fits(area: GridArea, grid: Grid): boolean {
  const inside = area.col >= 0 && area.row >= 0 && area.col + area.colSpan <= grid.columns && area.row + area.rowSpan <= grid.rows;
  return inside && area.colSpan >= 1 && area.rowSpan >= 1;
}

/** Pixel rectangle of an area of a monitor. Neighbouring areas share their edges exactly. */
export function areaRect(bounds: Rect, grid: Grid, area: GridArea): Rect {
  const x0 = Math.round((area.col * bounds.width) / grid.columns);
  const x1 = Math.round(((area.col + area.colSpan) * bounds.width) / grid.columns);
  const y0 = Math.round((area.row * bounds.height) / grid.rows);
  const y1 = Math.round(((area.row + area.rowSpan) * bounds.height) / grid.rows);
  return { x: bounds.x + x0, y: bounds.y + y0, width: x1 - x0, height: y1 - y0 };
}

/** Smallest rectangle containing every rectangle. */
export function unionRect(rects: readonly Rect[]): Rect {
  const left = Math.min(...rects.map((r) => r.x));
  const top = Math.min(...rects.map((r) => r.y));
  const right = Math.max(...rects.map((r) => r.x + r.width));
  const bottom = Math.max(...rects.map((r) => r.y + r.height));
  return { x: left, y: top, width: right - left, height: bottom - top };
}

/** The area with one edge moved outwards by `steps` cells (inwards when negative). */
export function moveEdge(area: GridArea, edge: Edge, steps: number): GridArea {
  switch (edge) {
    case 'left':
      return { ...area, col: area.col - steps, colSpan: area.colSpan + steps };
    case 'right':
      return { ...area, colSpan: area.colSpan + steps };
    case 'top':
      return { ...area, row: area.row - steps, rowSpan: area.rowSpan + steps };
    case 'bottom':
      return { ...area, rowSpan: area.rowSpan + steps };
  }
}

const GROW_ORDER: readonly Edge[] = ['right', 'left', 'bottom', 'top'];

/** The area grown over every free neighbouring cell, sideways first. */
export function expandArea(area: GridArea, grid: Grid, isFree: (candidate: GridArea) => boolean): GridArea {
  let current = area;
  for (const edge of GROW_ORDER) {
    for (let next = moveEdge(current, edge, 1); fits(next, grid) && isFree(next); next = moveEdge(current, edge, 1)) {
      current = next;
    }
  }
  return current;
}
