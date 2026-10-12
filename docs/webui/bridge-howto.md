# Bridge how-to

How the React UI talks to the C# host, and how to extend it. The design (protocol, error codes) is in [architecture.md](architecture.md); this page is the practical guide.

```
C# contract (Nucleus.Bridge/Contracts)  ──npm run bridge:gen──▶  src/bridge/generated/contract.ts
        │ implemented by services                                    │ typed proxy, hooks, mocks
        ▼                                                            ▼
BridgeHost ◀── JSON-RPC over WebView2 web messages ──▶ BridgeClient (src/bridge)
```

One source of truth: the C# interfaces and DTOs. The generator turns them into TypeScript types, and the host dispatches with the same model (`ContractModel`), so both sides always agree. If the UI and the app are built from different contracts, the UI shows a "versions don't match" screen instead of failing randomly.

## Commands

| What | Command (from `Master/`) |
|---|---|
| Regenerate `contract.ts` | `cd Nucleus.WebUI && npm run bridge:gen` (or `node scripts/bridge-gen.mjs`) |
| Fail when `contract.ts` is stale (CI) | `node scripts/bridge-gen.mjs --check` |
| C# tests | `dotnet test Nucleus.Bridge.Tests` |
| TypeScript tests | `cd Nucleus.WebUI && node --test "src/bridge/__tests__/*.test.ts"` |
| Accept a generator snapshot change | `UPDATE_SNAPSHOTS=1 dotnet test Nucleus.Bridge.Tests`, then review the diff of `Snapshots/ShelfContract.ts` |

`bridge-gen` builds `Nucleus.Bridge.Gen` with MSBuild (found with vswhere; set `MSBUILD` to override) and runs it. It leaves `contract.ts` untouched when nothing changed, and prints a warning for every member without XML documentation.

## Add a method

Example: rename a game.

**1. Contract** (`Master/Nucleus.Bridge/Contracts/Library.cs`). One request DTO, XML docs everywhere (they become the TSDoc the UI developer sees):

```csharp
[BridgeService("library")]
public interface ILibraryService
{
    /// <summary>Renames a game in the library. Throws <c>notFound</c> for an unknown game.</summary>
    void Rename(RenameGameRequest request);
}

/// <summary>Parameters of <see cref="ILibraryService.Rename"/>.</summary>
public sealed class RenameGameRequest
{
    /// <summary>Game id.</summary>
    public string GameId { get; set; } = "";

    /// <summary>New name, 1 to 80 characters.</summary>
    public string Name { get; set; } = "";
}
```

Rules, checked when the model is built (the generator, the tests and the host all fail with a list of every problem):

- Wire name: `<service>.<camelCaseMethod>` → `library.rename`. No overloads, no generic methods.
- Zero parameters or one DTO, optionally followed by a `CancellationToken` (not serialized). Returns `void`, `T`, `Task` or `Task<T>`.
- Types: `string`/`Guid` → `string`; numbers → `number`; `bool` → `boolean`; `DateTime`/`DateTimeOffset` → `IsoDateString`; enums → unions of camelCase strings (+ a `xxxValues` array); arrays, `List<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`, `HashSet<T>` → `T[]`; `Dictionary<string, T>` → `Record<string, T>`; other classes → DTO interfaces, discovered transitively. Generic DTOs, `object`, `TimeSpan`, `byte[]`, non-string dictionary keys, `[Flags]` enums are refused with a clear message.
- Nullability comes from C# nullable annotations: `string? Source` → `source?: string` (optional; nulls are omitted on the wire), `Task<string?>` → `string | null`, `List<string?>` → `(string | null)[]`, `int?`/`DeviceKind?` likewise.

**2. Implementation** (`Nucleus.Coop.Services`). Expected failures throw `BridgeException`; anything else becomes a generic `internal` error and is logged with its stack trace:

```csharp
public void Rename(RenameGameRequest request)
{
    var game = _games.Find(request.GameId)
        ?? throw new BridgeException(BridgeErrorKind.NotFound, "Game not found", new { request.GameId });
    if (request.Name.Length is 0 or > 80)
        throw new BridgeException(BridgeErrorKind.InvalidParams, "The name must have 1 to 80 characters.");
    game.Name = request.Name;
    _events.Emit(new LibraryChanged { GameIds = { game.Id } });   // refetches library.list/get in the UI
}
```

Services run on the UI thread (the thread that called `HandleAsync`). Don't block it: await IO, push CPU work to `Task.Run`.

**3. Registration** happens once per service in the composition root (`HostForm`), see [Host wiring](#host-wiring). A new method on an already registered service needs nothing.

**4. Regenerate**: `npm run bridge:gen`. The contract hash changes, so a stale `webui` folder is detected at startup.

**5. Mock**: the compiler now reports that `library.rename` is missing in `src/bridge/mock/library.ts`. Add it (see [Mocks](#mocks)).

**6. Use it** from React:

```tsx
const rename = useBridgeMutation('library.rename');
rename.mutate({ gameId, name }, { onError: (e) => toast(e.message) });
// or anywhere: await bridge.library.rename({ gameId, name });
```

## Add an event

```csharp
/// <summary>A game was renamed.</summary>
[BridgeEvent("library.renamed", Invalidates = new[] { "library.list", "library.get" })]
public sealed class GameRenamed
{
    /// <summary>Game id.</summary>
    public string GameId { get; set; } = "";
}
```

- Name: `<service>.<camelCaseName>`. `Invalidates` lists the methods whose cached results become stale; the UI refetches their queries automatically (any params). Unknown method names are refused.
- Emit from any thread: `events.Emit(new GameRenamed { GameId = id })`. Services receive `IBridgeEvents` (`host.Events`), easy to fake in tests. The payload is serialized immediately (later changes to the object are not sent) and delivered on the UI thread, in emit order.
- Listen in React with `useBridgeEvent('library.renamed', (e) => …)`, or outside React with `bridgeClient.on(...)` (returns an unsubscribe function).
- The mock emits it with `context.emit('library.renamed', { gameId })`.

## Using the bridge from React

Wrap the app once, inside TanStack's provider:

```tsx
<QueryClientProvider client={queryClient}>
  <BridgeProvider connecting={<Splash />} renderMismatch={(m) => <VersionMismatch {...m} />}>
    <App />
  </BridgeProvider>
</QueryClientProvider>
```

`BridgeProvider` calls `app.hello`, renders its children only when the contract hash and protocol version match, and plugs event invalidation into the `QueryClient`. Every hook must be used inside it.

| API | Use |
|---|---|
| `useBridgeQuery('library.list')`<br>`useBridgeQuery('library.get', { gameId }, { enabled, select, timeoutMs, … })` | A method's result as a TanStack query. Key: `bridgeQueryKey(method, params)` = `[method]` or `[method, params]`. Unmounting or a key change cancels the call (the host gets `$/cancel`). |
| `useBridgeMutation('library.setFavorite', { onSuccess, timeoutMs, … })` | A method as a mutation: `mutate(params)` / `mutateAsync(params)`; `mutate()` for methods without params. |
| `useBridgeEvent('setup.deviceActivity', handler)` | Subscribes while mounted; always calls the latest `handler`. |
| `useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot)` | Host-owned state, live. See below. |
| `bridge.library.list()`, `bridge.library.get({ gameId }, { signal, timeoutMs })` | Typed proxy, for event handlers and non-React code. Hovers show the C# docs. |
| `bridge.getFilePaths(files)` | Absolute paths of dropped files. See below. |
| `bridgeClient.call('library.list')`, `bridgeClient.on(event, fn)` | The raw client behind the proxy. |

All types of the contract (`GameSummary`, `SetupSnapshot`, `DeviceKind`, `deviceKindValues`…) are exported by `@/bridge` too.

### The snapshot pattern

For stateful screens (setup, session, window) the host owns the state: a `get` query returns a full snapshot, commands change it, and every change emits a `*.changed` event carrying the new snapshot. Render the latest snapshot; never keep a copy of host state.

```tsx
const { data: setup } = useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot);
const assign = useBridgeMutation('setup.assignToSlot');   // the new snapshot arrives through setup.changed
```

The hook fetches the snapshot once, then writes each event's snapshot into the query cache. A fetch that was in flight when an event arrived does not overwrite the newer snapshot. The selector is optional when the event payload *is* the snapshot type, required otherwise (the compiler tells you). Don't list the `get` method in the event's `Invalidates`: the payload already is the new state.

### Errors

Calls reject with `BridgeError { kind, code, message, data }`:

| kind | code | when |
|---|---|---|
| `notFound`, `conflict`, `unavailable` | -32004, -32002, -32001 | expected failures thrown by services (`BridgeException`) |
| `invalidParams` | -32602 | params don't bind, or a service rejects them |
| `internal` | -32603 | unexpected exception; generic message, details in the Nucleus log |
| `cancelled` | -32800 | aborted signal, unmount, page reload |
| `methodNotFound` | -32601 | unknown method, or no implementation registered for its service |
| `timeout` | -32099 | client-side only: no answer in time (default 30 s) |

`data` is the third argument of `BridgeException` (or of `new BridgeError(kind, message, data)` in mocks), for example `{ reason: 'slotOccupied' }`.

```ts
try {
  await bridge.setup.assignToSlot({ deviceId, screenIndex, slotIndex });
} catch (e) {
  if (BridgeError.is(e, 'conflict') && (e.data as { reason?: string }).reason === 'slotOccupied') showSlotTaken();
  else throw e;
}
```

### Cancellation and timeouts

- Every call has a timeout: 30 s by default, `{ timeoutMs }` per call (0 waits forever, for example for `settings.captureGamepadCombo`). On timeout the client rejects with `timeout` and tells the host to cancel.
- Pass an `AbortSignal` (`{ signal }`); TanStack passes its own to `useBridgeQuery`, so unmounting cancels. The promise rejects with `cancelled` immediately, and the host receives `$/cancel`.
- On the host, a method that takes a `CancellationToken` sees it cancelled; if it throws `OperationCanceledException` the answer is `cancelled`. Methods without a token simply finish, and their late answer is ignored.
- When the page reloads or navigates, the host cancels every request in flight and drops their answers.

### Dropped files

Page scripts cannot read the path of a dropped file; the host can. `bridge.getFilePaths` sends the `File` objects to the host with `chrome.webview.postMessageWithAdditionalObjects`; `WebView2Transport` answers the reserved method `$/filePaths` itself (it is not a service):

```tsx
<div onDragOver={(e) => e.preventDefault()} onDrop={async (e) => {
  e.preventDefault();
  const [path] = await bridge.getFilePaths(e.dataTransfer.files);
  if (path) await bridge.handlers.installFromFile({ path, overwrite: false });
}} />
```

An empty list resolves to `[]` without a round trip. The mock returns the file names.

## Mocks

Outside WebView2 (`npm run dev`, the review URL), `src/bridge/instance.ts` loads `src/bridge/mock/` instead of talking to the host (in its own chunk, so the real app never downloads it). The mock implements the generated `BridgeHandlers` type, so the compiler flags any method without a fake.

- One file per service, `mock/<service>.ts`, exporting `create<Service>Mock(...)` that returns `BridgeHandlers['<service>']`. Shared state lives in `mock/state.ts`, data in `mock/fixtures.ts`, and `mock/index.ts` assembles everything in `createMockBridgeTransport()`.
- A handler takes the params (none for methods without params) and returns the result or a promise; `void` methods return nothing. Results go through JSON like the real wire.
- Throw `new BridgeError('conflict', 'That slot is taken.', { reason: 'slotOccupied' })` for expected failures; other exceptions become `internal` errors and are logged.
- Push events with `context.emit(event, payload)`, exactly when the host would.

```ts
export function createWindowMock({ emit }: MockContext): BridgeHandlers['window'] {
  const state: WindowState = { maximized: false, minimized: false, focused: true, dpiScale: 1 };
  return {
    getState: () => state,
    toggleMaximize: () => {
      state.maximized = !state.maximized;
      emit('window.stateChanged', { state });
    },
    // …
  };
}
```

Tests can run the whole mock host in Node: `new BridgeClient({ transport: createMockBridgeTransport() })`.

## Host wiring

In the composition root, on the UI thread, once the WebView is initialized:

```csharp
var host = new BridgeHost(SynchronizationContext.Current, entry =>
{
    if (entry.Level >= BridgeLogLevel.Info || debug) LogManager.Log("[bridge] " + entry);
});
host.Register<IAppService>(new AppService(host.ContractHash, …));   // app.hello returns host.ContractHash and BridgeHost.ProtocolVersion
host.Register<IWindowService>(new WindowService(this, host.Events));
host.Register<ILibraryService>(new LibraryService(…, host.Events));
_transport = new WebView2Transport(webView.CoreWebView2, host, allowedOrigin: "https://ui.nucleus.example");
// on close: _transport.Dispose();
```

- `BridgeHost` is transport-agnostic: `Task<string?> HandleAsync(string message)` returns the answer (null for notifications), and `event Action<string> Outgoing` carries events. Tests drive it directly.
- `WebView2Transport` ignores messages whose source is not `allowedOrigin` (use the dev server origin in Debug, for example `http://localhost:5173`), posts nothing while another origin is displayed, cancels requests in flight when a new document loads, and answers `$/filePaths`. It requires a host created with the UI thread's `SynchronizationContext`.
- A method whose service is not registered answers `methodNotFound`, so services can be registered progressively.

## Debugging

- **Host log** (every call, Debug level): `library.setFavorite #7 ok (3 ms)`, `setup.assignToSlot #12 failed: conflict: That slot is taken. (1 ms)`, `library.get #9 failed: NullReferenceException: … (2 ms)` followed by the stack trace (Error), `event setup.changed`, `page reloaded: cancelled 2 request(s) in flight` (Info), `ignored a message from https://…` (Warning).
- **UI trace**: in the dev tools console, `localStorage.setItem('nucleus.bridge.trace', '1')`, then reload: every message is logged as `[bridge] → {…}` / `[bridge] ← {…}`. Remove the key to stop.
- **Manual calls**: the proxy is available as `__bridge` in the dev tools console: `await __bridge.library.list()`. The WebView enables dev tools in Debug builds only; in a normal browser (mock) they are always there.
- **"UI and app versions don't match"**: the `webui` folder was built from another contract. Rebuild the UI (`npm run build`) or regenerate (`npm run bridge:gen`) and rebuild both.
- **Why did the hash change?** `ContractModel.Default.ToJson()` returns the canonical model (wire names and types, no docs); diff two dumps. Comments never change the hash.
- **Generator errors** list every convention problem with its location, for example `ILibraryService.Get (parameter 'id'): the parameter must be a request DTO class, for example GetRequest.`
