// The mock's i18n service (npm run dev): English from the app's en.json, the en-XA pseudo-language, the choice through
// i18n.setLanguage or the first setting of General, and i18n.languageChanged.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { detectMockLanguage, pseudoLocalize, PSEUDO_LOCALE } from '../mock/i18n.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

function mockBridge() {
  const client = new BridgeClient({ transport: createMockBridgeTransport({ notices: false }) });
  return { client, bridge: createBridgeProxy(client) };
}

test('the mock starts in English, not chosen yet, with the pseudo-language available', async () => {
  const { bridge } = mockBridge();
  const state = await bridge.i18n.getState();
  assert.equal(state.current, 'en');
  assert.equal(state.chosen, false);
  assert.deepEqual(
    state.languages.map((l) => l.locale),
    ['en', PSEUDO_LOCALE],
  );
  const strings = await bridge.i18n.getStrings({ locale: 'en' });
  assert.equal(strings.strings['common.ok'], 'OK');
});

test('detected falls back to English, saying it does not match Windows (here the browser languages)', async () => {
  assert.deepEqual(detectMockLanguage(['fr-FR', 'en-US']), { detected: 'en', detectedMatchesWindows: true });
  assert.deepEqual(detectMockLanguage(['en-xa']), { detected: PSEUDO_LOCALE, detectedMatchesWindows: true });
  assert.deepEqual(detectMockLanguage(['it-IT', 'it']), { detected: 'en', detectedMatchesWindows: false });
  assert.deepEqual(detectMockLanguage([]), { detected: 'en', detectedMatchesWindows: false });

  const { bridge } = mockBridge();
  const { detected, detectedMatchesWindows } = await bridge.i18n.getState();
  assert.deepEqual({ detected, detectedMatchesWindows }, detectMockLanguage(globalThis.navigator?.languages ?? []));
});

test('setLanguage saves the choice, switches and emits languageChanged; the setting follows', async () => {
  const { client, bridge } = mockBridge();
  const changes: string[] = [];
  client.on('i18n.languageChanged', (e) => changes.push(e.locale));

  await bridge.i18n.setLanguage({ locale: 'EN-xa' });
  await delay(5);
  const state = await bridge.i18n.getState();
  assert.equal(state.current, PSEUDO_LOCALE);
  assert.equal(state.chosen, true);
  assert.deepEqual(changes, [PSEUDO_LOCALE]);
  assert.equal((await bridge.settings.getValues())['misc.appLanguage'], PSEUDO_LOCALE);

  const pseudo = await bridge.i18n.getStrings({ locale: PSEUDO_LOCALE });
  assert.equal(pseudo.strings['shell.titleBar.freeDevices_other'], pseudoLocalize('{count} free'));
  await assert.rejects(bridge.i18n.setLanguage({ locale: 'xx' }), (e) => BridgeError.is(e, 'invalidParams'));
  await assert.rejects(bridge.i18n.getStrings({ locale: 'xx' }), (e) => BridgeError.is(e, 'notFound'));
});

test('the language setting is the first of General and switches the language', async () => {
  const { client, bridge } = mockBridge();
  const changes: string[] = [];
  client.on('i18n.languageChanged', (e) => changes.push(e.locale));

  const schema = await bridge.settings.getSchema();
  const first = schema.sections.find((s) => s.id === 'general')?.fields[0];
  assert.equal(first?.key, 'misc.appLanguage');
  assert.deepEqual(
    first?.choices.map((c) => c.value),
    ['en', PSEUDO_LOCALE],
  );

  assert.deepEqual(await bridge.settings.setValues({ values: { 'misc.appLanguage': PSEUDO_LOCALE } }), []);
  await delay(5);
  assert.deepEqual(changes, [PSEUDO_LOCALE]);
  assert.equal((await bridge.i18n.getState()).chosen, true);
});

test('pseudo-localization accents letters, keeps placeholders and tags, and lengthens the text', () => {
  assert.equal(pseudoLocalize('Press <kbd>{key}</kbd> to lock'), '[Pŕéšš <kbd>{key}</kbd> ţö ļöçķ ~~~~~~~~~]');
  assert.equal(pseudoLocalize(''), '[]');
});
