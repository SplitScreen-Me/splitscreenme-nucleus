# Nucleus Co-op web UI: architecture and bridge

Status: draft v1, 2026-10-11. Scope: replace the WinForms `MainForm` with a WebView2 window that hosts a React UI (visual direction "Bezel", see `redesign-draft/style-1`). Game launch internals (hooks, handler JS, window placement) stay in C#.

## 1. Layers

```
┌──────────────────────────── Nucleus.Coop.exe (WinForms host, net472, x86) ──────────────────────────┐
│ HostForm: frameless window + WebView2 (virtual hosts, dev server, devtools in Debug)                 │
│ Composition root: creates BridgeHost, registers services, wires WebView2Transport                     │
└───────────────┬───────────────────────────────────────────────────────────────────────┬──────────────┘
                │ JSON-RPC 2.0 over WebView2 web messages (strings)                     │ https://assets.nucleus.example/*
┌───────────────▼──────────────────────────────┐                     ┌──────────────────▼──────────────┐
│ Nucleus.WebUI (React 19, TS, Tailwind v4)    │                     │ Local files (covers, icons…)    │
│ src/bridge/  generated contract + runtime    │──── fetch ────────▶ │ v3.splitscreen.me /api/v3/*     │
│ src/hub/     public hub API (TanStack Query) │                     │ (public, CORS *)                │
└──────────────────────────────────────────────┘                     └─────────────────────────────────┘
        ▲ contract.ts generated from
┌───────┴──────────────────────────────────────┐   implements   ┌─────────────────────────────────────┐
│ Nucleus.Bridge (net472, no UI deps)          │◀──────────────│ Nucleus.Coop.Services (net472)       │
│ Contracts/  service interfaces, DTOs, events │               │ headless app logic, extracted from   │
│ Runtime/    BridgeHost, dispatcher, errors   │               │ forms; wraps Nucleus.Gaming          │
└──────────────────────────────────────────────┘               └─────────────────────────────────────┘
```

| Project | Kind | Depends on | Owns |
|---|---|---|---|
| `Master/Nucleus.Bridge` | SDK-style class library, net472 | Newtonsoft.Json 13.0.3, Microsoft.Web.WebView2 | Bridge runtime, attributes, the contract (C# interfaces + DTOs), the WebView2 transport |
| `Master/Nucleus.Bridge.Gen` | SDK-style console, net472 | Nucleus.Bridge | Generates `Nucleus.WebUI/src/bridge/generated/contract.ts` from the contract |
| `Master/Nucleus.Bridge.Tests` | SDK-style test project | Nucleus.Bridge | Dispatcher, serialization, errors, cancellation, generator snapshot |
| `Master/Nucleus.Coop.Services` | SDK-style class library, net472 | Nucleus.Gaming, Nucleus.Bridge | Service implementations (library, handlers, setup, session, settings…) |
| `Master/NucleusCoopTool` (`Nucleus.Coop.exe`) | existing WinForms exe | all of the above | `HostForm`, window service, composition root, startup checks |
| `Master/Nucleus.WebUI` | Vite app | none (build output copied to `webui\` next to the exe) | All UI |

Rules:
- The contract has no dependency on WinForms, WPF, `System.Drawing` or Nucleus.Gaming types. DTOs are plain classes.
- Services never touch WinForms controls. When an existing feature lives inside a form, its logic moves to `Nucleus.Coop.Services` (or `Nucleus.Gaming`), and the form, if it survives, calls the extracted code.
- The UI never reads local files directly: data comes through the bridge, images through the `assets` virtual host.
- The old `MainForm` and the rest of the WinForms UI were removed on 2026-10-12. `--legacy-ui` is still accepted so old shortcuts keep working: it is logged and ignored.

## 2. Protocol

JSON-RPC 2.0 messages, serialized as strings (`chrome.webview.postMessage(string)` / `CoreWebView2.PostWebMessageAsString`).

| Direction | Shape |
|---|---|
| UI → host request | `{"jsonrpc":"2.0","id":7,"method":"library.setFavorite","params":{"gameId":"…","favorite":true}}` |
| host → UI result | `{"jsonrpc":"2.0","id":7,"result":…}` (`result` is `null` for void) |
| host → UI error | `{"jsonrpc":"2.0","id":7,"error":{"code":-32004,"message":"Game not found","data":{"kind":"notFound"}}}` |
| host → UI event | `{"jsonrpc":"2.0","method":"setup.changed","params":{…}}` (notification, no `id`) |
| UI → host cancel | `{"jsonrpc":"2.0","method":"$/cancel","params":{"id":7}}` |

Handshake: the UI calls `app.hello` first. The host answers `{ protocolVersion, contractHash, appVersion, debug }`. If `contractHash` differs from the hash compiled into the UI, the UI shows a blocking "UI and app versions don't match" screen (stale `webui` folder).

Serialization (both directions, one shared `BridgeJson.Settings`): camelCase properties, enums as camelCase strings, `DateTime` as ISO 8601 UTC, nulls omitted, no `$type`.

Error codes:

| code | kind | when |
|---|---|---|
| -32700 / -32600 | `parse` / `invalidRequest` | malformed message |
| -32601 | `methodNotFound` | unknown method |
| -32602 | `invalidParams` | params don't bind or fail validation |
| -32603 | `internal` | unexpected exception (message is generic, details logged) |
| -32800 | `cancelled` | cancelled through `$/cancel` |
| -32001 | `unavailable` | hub offline, device missing… |
| -32002 | `conflict` | e.g. a session is already running |
| -32004 | `notFound` | unknown game, handler, profile… |

Services throw `BridgeException(BridgeErrorKind kind, string message, object data = null)` for expected failures. Any other exception becomes `internal`.

## 3. Contract conventions (C#)

```csharp
/// <summary>Installed games.</summary>
[BridgeService("library")]
public interface ILibraryService
{
    /// <summary>All games in the user's library.</summary>
    Task<IReadOnlyList<GameSummary>> List();

    /// <summary>Marks or unmarks a game as favourite.</summary>
    Task SetFavorite(SetFavoriteRequest request);
}

[BridgeEvent("library.changed", Invalidates = new[] { "library.list", "library.get" })]
public sealed class LibraryChanged { public string[] GameIds { get; set; } }
```

- Method name on the wire: `<service>.<camelCaseMethod>`.
- A method takes zero parameters or one DTO (named `…Request`), optionally followed by a `CancellationToken` (not serialized). Returns `Task`, `Task<T>`, `T` or `void`.
- Events are DTO classes marked `[BridgeEvent(name)]`. `Invalidates` lists queries the UI must refetch when the event arrives (the TS runtime does it automatically).
- Pattern for stateful screens (setup, session): the host owns the state. A `get` query returns a full snapshot; commands mutate it; every change emits a `*.changed` event carrying the new snapshot. The UI renders the latest snapshot and never keeps its own copy of host state.
- Long operations (download, disk scan) return when done and report progress through events carrying an operation id.
- XML doc comments are required on every service, method, DTO and property; the generator copies them to TSDoc.
- Breaking a contract changes `contractHash` automatically (hash of the generated file).

## 4. C# runtime

```csharp
var host = new BridgeHost(BridgeJson.Settings, logger);
host.Register<ILibraryService>(new LibraryService(...));
host.Register<IWindowService>(new WindowService(hostForm));
var transport = new WebView2Transport(webView.CoreWebView2, host, allowedOrigin: "https://ui.nucleus.example");
// anywhere, any thread:
host.Events.Emit(new LibraryChanged { GameIds = ids });
```

- `BridgeHost` is transport-agnostic: `Task<string> HandleAsync(string message)` plus `event Action<string> Outgoing`. It builds a method table by reflection once (`[BridgeService]` interfaces), binds params with Newtonsoft, awaits, serializes the result.
- Threading: WebView2 raises `WebMessageReceived` on the UI thread and the dispatcher invokes services there, so existing Nucleus code that expects the UI thread keeps working. Services must not block: IO is async, CPU-heavy work goes to `Task.Run`. `Events.Emit` is thread-safe and posts to the UI thread.
- Cancellation: one `CancellationTokenSource` per in-flight request, cancelled on `$/cancel` or when the page reloads.
- Logging: every call is logged with duration at Debug level; errors at Warning or Error with the request id.
- Security: `WebView2Transport` ignores messages whose `Source` is not the UI origin. The WebView never navigates away from the UI: external links open in the default browser.

## 5. TypeScript runtime (`Nucleus.WebUI/src/bridge/`)

```ts
import { bridge } from '@/bridge';

await bridge.library.setFavorite({ gameId, favorite: true });   // typed proxy
const games = useBridgeQuery('library.list');                     // TanStack Query, key = [method, params]
const fav = useBridgeMutation('library.setFavorite');
useBridgeEvent('devices.input', (e) => flash(e.deviceId));
const setup = useBridgeSnapshot('setup.get', 'setup.changed');    // query + live updates from events
```

- `generated/contract.ts` (do not edit): DTO types, enum unions, `BridgeMethods` map (`params` / `result` per method), `BridgeEvents` map, `eventInvalidations`, `contractHash`, method name list for the proxy.
- `client.ts`: request ids, pending map, timeouts (default 30 s, per call override), `AbortSignal` → `$/cancel`, `BridgeError` (`code`, `kind`, `data`), event dispatch, automatic invalidation.
- Transports: `webview2.ts` (`window.chrome.webview`) and `mock.ts`. The mock implements the generated `BridgeHandlers` type, so the compiler forces every method to have a fake, and lets the UI run in a normal browser (`npm run dev`) and on the review URL.
- `react.ts`: `useBridgeQuery`, `useBridgeMutation`, `useBridgeEvent`, `useBridgeSnapshot`, `BridgeProvider` (handshake + version mismatch screen).
- Translations: every user-visible text goes through `src/i18n` (`t`, `<T>`, `format`) in the UI and `Nucleus.Gaming.Localization` (`I18n.T`) in C#, from the language files of `Master/i18n` (one per language, English as the per-key fallback). Format, key naming, namespaces and owners, plurals, rich text, how host texts reach the UI and the checks: [i18n.md](i18n.md).

## 6. Host window

- `HostForm` is frameless. The WebView2 fills the client area minus a 6 px bezel kept by the form for resize hit-testing (the bezel is black, which matches the design). Dragging uses CSS `app-region: drag` (`CoreWebView2Settings.IsNonClientRegionSupportEnabled = true`). Double-click on the title bar toggles maximize.
- Virtual hosts: `https://ui.nucleus.example/` → `webui\` (built UI), `https://assets.nucleus.example/` → `gui\` (covers, screenshots, rendered icons), `https://handlers.nucleus.example/` → `handlers\` (images shipped with handlers, used by custom steps). In Debug, if `webui\dev-server.txt` exists, the WebView loads the URL it contains (Vite dev server, hot reload) instead.
- User data folder: `webview\ui`, separate from the removed legacy hub browser's `webview\cache` (which ran with browser extensions and may still exist in older installs; two environments with different options can't share a folder). Dev tools, context menu and reload accelerators only in Debug.
- The host takes over the old `MainForm` duties: `Globals.MainWindowHandle` (target of the kill hotkey's `WM_CLOSE`), `Globals.MainOSD`, gamepad threads, window title "Nucleus Co-op", close handling during a session (end the session on a worker, never on the UI thread), working directory = install folder.
- Dropped files: JS cannot read a dropped file's path. `bridge.getFilePaths(files)` sends the `File` objects with `chrome.webview.postMessageWithAdditionalObjects` as the reserved request `$/filePaths`; the transport answers with their absolute paths, which the UI then passes to normal methods (for example `handlers.installFromFile`).
- WebView2 SDK: upgrade from 1.0.2210.55 to 1.0.4191.47 everywhere (packages.config and PackageReference must use the same version).
- Startup checks in `Program.cs` stay. The WebView2 runtime check moves before window creation (it already exists).

## 7. Build

- `Nucleus.WebUI`: `npm run build` → `dist/`. An MSBuild target in `Nucleus.Coop.csproj` copies `dist/` to `$(TargetDir)webui\` when it exists (the C# build does not run npm).
- `npm run bridge:gen` builds and runs `Nucleus.Bridge.Gen`; `--check` mode fails when `contract.ts` is stale.

## 8. Contract catalogue

| Service | Purpose | State model |
|---|---|---|
| `app` | handshake, info, open URL/path, log, restart as admin | stateless |
| `window` | minimize, maximize, close, title; `window.stateChanged` | events |
| `dialogs` | native file and folder pickers | stateless |
| `library` | games, details, favourites, per-game menu (descriptors + actions), add/remove, artwork, list preferences; `library.changed` | queries + invalidation |
| `handlers` | install/update from hub (SHA-256 checked), install from file, update checks, screenshots; `handlers.progress`, `handlers.changed` | queries + invalidation |
| `settings` | schema-driven settings (values as strings), player identities, gamepad combo capture; `settings.changed` | queries + invalidation |
| `setup` | the lobby: devices, screens, layouts, placement, options, custom steps; `setup.changed`, `setup.deviceActivity`, `setup.deviceLost` | host-owned snapshot |
| `profiles` | saved setups of the open game, profile settings; `profiles.changed` | queries + invalidation |
| `session` | play, stop, live state; `session.changed`, `session.notice` | host-owned snapshot |
| `notices` | out-of-game engine messages (handler load and script errors, setup and profile errors, locked game files), kept until dismissed; `notices.changed` | queries + invalidation |
| `i18n` | languages of the `i18n` folder, the user's choice, the strings of a language; `i18n.languageChanged` (the UI then refetches every query: host texts are built in the language in use). See [i18n.md](i18n.md) | queries + invalidation |

### Engine prompts

Every message box and prompt of Nucleus.Gaming goes through `Nucleus.Gaming.Forms.UserPrompts` (`Ask(PromptRequest)` and shortcuts: `Report`, `Notice`, `YesNo`, `WaitForOk`, `PickLauncher`, `AskText`, `PickProcess`). Its default handler shows the native windows (plain WinForms). The services install `Notices/WebUserPrompts`, which routes by `PromptOrigin`:

- `Handler` and `Setup` messages become `notices` (the UI shows them in a dialog, after the session when one is live);
- `LaunchReminder` (the input lock reminder) is skipped: the setup shows it as the `inputLock` check of "Before you launch";
- `LaunchFailure` (the crash report of a failed launch) goes to `SessionSnapshot.errorDetails`;
- `Session` prompts (over the game windows), questions, and anything raised while `IHostShell.PageState` is `Unavailable` (WebView failed, renderer crashed, window closing) stay native. While the page is `Loading` (startup, reload), notices wait for it.

### Hub (API v3)

The hub is the SplitScreen.Me site, `https://v3.splitscreen.me` by default (`Nucleus.Gaming.HubSite`). Settings.ini `[Dev] HubUrl` points the app at another site, a local one during development; the value is read once at startup. The host and the UI use the same value: the UI reads it from `app.getInfo().hubUrl`. Contract of the API: `<hubUrl>/api/v3/openapi.json`, guide at `<hubUrl>/doc/api`.

- **UI** (`src/hub`): `GET /catalog` (every listed handler, persisted in localStorage for offline starts), `GET /handlers/{id}` (notes, game summary), `GET /handlers/{id}/comments` (pages of 200, following `nextCursor`). Called straight from the WebView (CORS `*`, no cookies). Web links come from the API (`url`: the game page `/hub/<slug>` or the handler page; `authorUrl`: `/hub/authors/<username>`), the "Hub" link of Settings is `<hubUrl>/hub`.
- **Host** (`HubClient`, `Nucleus.Coop.Services/Handlers`): `GET /health` as the online check, `GET /handlers/{id}` for installs, update checks (`version`), screenshots (`screenshots[].imageId`) and covers (`coverImageId`), `GET /handlers/{id}/download?version=n` for installs, counted by the hub once per address, handler, version and day. The download is checked against `currentVersion.sha256` before anything is extracted; a mismatch deletes the file and fails the install (`conflict`, `reason: checksumMismatch`). Tests and manual checks use `count=false`.
- **Legacy** `Nucleus.Gaming.Coop.Generic.Hub.CheckUpdateAvailable` (run when a handler script loads) reads `version` from `GET /handlers/{id}` too.
- **CSP**: the packaged UI's `connect-src` allows `'self'` and `https://v3.splitscreen.me` only. A build for another site needs its origin at build time (`NUCLEUS_HUB_ORIGINS=http://localhost:3000 npm run build`); otherwise the hub screens show an error naming the setting (`src/hub/site.ts`) while the host side works. The Vite dev server (`webui\dev-server.txt`) has no CSP, so any site works there.
