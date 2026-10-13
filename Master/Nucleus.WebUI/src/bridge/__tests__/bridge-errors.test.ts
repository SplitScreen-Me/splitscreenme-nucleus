// What the UI tells the user about a failed call (src/lib/bridge-errors.ts): host messages as they are, the bridge's
// own failures (English, for the log) in the language in use.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { errorMessage, refusalReason } from '../../lib/bridge-errors.ts';
import { BridgeError } from '../errors.ts';
import { english } from '../../i18n/english.ts';

test('host messages of expected failures are shown as they are', () => {
  assert.equal(errorMessage(new BridgeError('conflict', 'Ce joueur a déjà une place.', { reason: 'slotOccupied' })), 'Ce joueur a déjà une place.');
  assert.equal(errorMessage(new BridgeError('unavailable', 'Le hub est hors ligne.', { reason: 'offline' })), 'Le hub est hors ligne.');
  assert.equal(errorMessage(new BridgeError('invalidParams', 'Adresse non valide.', { url: 'x' })), 'Adresse non valide.');
});

test('failures of the bridge itself are worded by the UI', () => {
  const cases: [BridgeError, string][] = [
    [new BridgeError('unavailable', "The 'library' service couldn't start: boom", { reason: 'serviceFailed', service: 'library' }), english['common.errors.serviceFailed']!],
    [new BridgeError('invalidParams', "Invalid params for 'library.get': bad", { reason: 'badParams' }), english['common.errors.internal']!],
    [new BridgeError('unavailable', 'library.list could not be sent: Error', { reason: 'notSent' }), english['common.errors.unreachable']!],
    [new BridgeError('unavailable', 'This transport cannot read file paths.', { reason: 'noFilePaths' }), english['common.errors.failed']!],
    [new BridgeError('timeout', 'library.list did not answer within 15000 ms.'), english['common.errors.timeout']!],
  ];
  for (const [error, expected] of cases) assert.equal(errorMessage(error), expected, error.message);
  assert.equal(refusalReason(cases[0]![0]), 'serviceFailed');
});
