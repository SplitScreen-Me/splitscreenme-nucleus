import { BridgeError } from '../errors.ts';

/** Refuses a command the current state does not allow: a `conflict` error with data `{ reason }`, like the host. */
export function conflict(reason: string, message: string): never {
  throw new BridgeError('conflict', message, { reason });
}

/** Fails because an id (game, device, player, profile…) does not exist. */
export function notFound(message: string): never {
  throw new BridgeError('notFound', message);
}

/** Fails because a parameter is malformed or out of range. */
export function invalidParams(message: string): never {
  throw new BridgeError('invalidParams', message);
}
