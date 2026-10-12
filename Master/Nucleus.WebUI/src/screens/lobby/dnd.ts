// Drag and drop inside the lobby: players and devices travel as custom types (readable during dragover),
// never as files, so the window-wide file drop ignores them.
import type { DragEvent } from 'react';

export const PLAYER_TYPE = 'application/x-nucleus-player';
export const DEVICE_TYPE = 'application/x-nucleus-device';

export type Dragged = { kind: 'player'; id: string } | { kind: 'device'; id: string };

export function startDrag(e: DragEvent, dragged: Dragged): void {
  e.dataTransfer.setData(dragged.kind === 'player' ? PLAYER_TYPE : DEVICE_TYPE, dragged.id);
  e.dataTransfer.setData('text/plain', `${dragged.kind}:${dragged.id}`);
  e.dataTransfer.effectAllowed = 'move';
}

/** What is being dragged over (the id is only readable on drop). */
export function dragKind(e: DragEvent): Dragged['kind'] | null {
  const types = e.dataTransfer.types;
  if (types.includes(PLAYER_TYPE)) return 'player';
  if (types.includes(DEVICE_TYPE)) return 'device';
  return null;
}

export function readDrop(e: DragEvent): Dragged | null {
  const player = e.dataTransfer.getData(PLAYER_TYPE);
  if (player) return { kind: 'player', id: player };
  const device = e.dataTransfer.getData(DEVICE_TYPE);
  return device ? { kind: 'device', id: device } : null;
}
