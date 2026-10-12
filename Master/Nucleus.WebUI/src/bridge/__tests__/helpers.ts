import type { BridgeTransport } from '../client.ts';

/** A transport recording what the client sends, and letting the test play the host. */
export interface FakeTransport extends BridgeTransport {
  /** Parsed messages sent by the client, in order. */
  sent: Record<string, unknown>[];
  /** Files attached to each `sendWithFiles` call. */
  files: (readonly File[])[];
  /** Delivers a message from the "host". */
  receive(message: unknown): void;
}

export function fakeTransport({ withFiles = true } = {}): FakeTransport {
  const listeners = new Set<(message: string) => void>();
  const transport: FakeTransport = {
    sent: [],
    files: [],
    send: (message) => void transport.sent.push(JSON.parse(message) as Record<string, unknown>),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
    receive: (message) => listeners.forEach((listener) => listener(typeof message === 'string' ? message : JSON.stringify(message))),
  };
  if (withFiles) {
    transport.sendWithFiles = (message, files) => {
      transport.send(message);
      transport.files.push(files);
    };
  }
  return transport;
}

export const delay = (ms: number) => new Promise<void>((resolve) => setTimeout(resolve, ms));
