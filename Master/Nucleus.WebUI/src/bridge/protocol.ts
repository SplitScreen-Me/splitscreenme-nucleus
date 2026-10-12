// JSON-RPC 2.0 messages exchanged with the host (see docs/webui/architecture.md, "Protocol").

/** A request: the host answers with a {@link ResponseMessage} carrying the same id. */
export interface RequestMessage {
  jsonrpc: '2.0';
  id: number;
  method: string;
  params?: unknown;
}

/** A notification: an event from the host, or `$/cancel` from the UI. No answer. */
export interface NotificationMessage {
  jsonrpc: '2.0';
  method: string;
  params?: unknown;
}

/** The error member of a failed response. */
export interface WireError {
  code: number;
  message: string;
  data?: unknown;
}

/** The answer to a request: `result` (null for void methods) or `error`. */
export type ResponseMessage =
  | { jsonrpc: '2.0'; id: number | string | null; result: unknown }
  | { jsonrpc: '2.0'; id: number | string | null; error: WireError };

/** Notification telling the host to stop a request: `params: { id }`. */
export const CANCEL_METHOD = '$/cancel';

/** Reserved request answered by the WebView2 transport: paths of the `File` objects posted with it. */
export const FILE_PATHS_METHOD = '$/filePaths';
