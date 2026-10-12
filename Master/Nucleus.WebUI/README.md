# Nucleus.WebUI

The Nucleus Co-op desktop interface: a React app hosted in the WebView2 window that replaces the WinForms `MainForm`. Visual direction "Bezel" (panes on a black bezel, colour means a player, Archivo width axis as hierarchy, couch-first), ported from `redesign-draft/style-1`.

Stack: Vite 8, React 19, TypeScript 7 (strict), Tailwind CSS 4 (CSS-first `@theme`), TanStack Query 5, React Router 8 (hash router), react-markdown 10 + remark-gfm. Fonts are self-hosted (`@fontsource-variable/archivo` wdth build: weight 100–900 and width 62–125, JetBrains Mono for logs). Nothing is loaded from a CDN at runtime except the public Hub API and remote game art.

## Run

```sh
npm install
npm run dev          # http://localhost:5173 — runs on the mock host in a normal browser
npm run typecheck    # tsc -b, zero errors expected
npm run build        # typecheck + production build into dist/
npm run test:bridge  # bridge runtime and mock host tests (node --test)
npm run preview      # serve dist/ on http://localhost:4173
```

Node 22.12+ (Vite 8 requirement).

The build uses relative URLs (`base: './'`) and a hash router, so `dist/` works from the WebView2 virtual host folder mapping (`https://ui.nucleus.example/` → `webui\`) with no server fallback. A Content Security Policy is injected into `dist/index.html` (scripts and fonts local, `connect-src` limited to the Hub, images from `https:`).

## Talking to the app: the bridge

Everything the UI asks of Nucleus Co-op goes through the typed bridge in `src/bridge/` (JSON-RPC over WebView2 web messages). The C# contract (`Master/Nucleus.Bridge/Contracts`) is the source of truth; `src/bridge/generated/contract.ts` is generated from it (`npm run bridge:gen`, never edit it by hand). Full guide: `docs/webui/bridge-howto.md`.

```tsx
import { bridge, useBridgeQuery, useBridgeMutation, useBridgeEvent, useBridgeSnapshot, BridgeError } from '@/bridge';

const { data: games } = useBridgeQuery('library.list');                       // refetched by library.changed
const { data: setup } = useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot); // host-owned state, live
useBridgeEvent('setup.deviceActivity', (e) => flash(e.deviceId));
await bridge.setup.join({ deviceId });                                         // commands from event handlers
```

- `App.tsx` wraps the app in `QueryClientProvider` → `BridgeProvider` (handshake; a stale `webui` folder shows "UI and app versions don't match") → toasts, confirmations, the add-game flow, the router.
- Host data is pushed: the events listed in the contract refetch the queries they make stale; snapshots (setup, session, window) are replaced by the snapshot each `*.changed` event carries. Never keep a copy of host state.
- Errors are `BridgeError { kind, message, data }`. Refusals are `conflict` with `data.reason` (`slotOccupied`, `inputsLocked`, `needsPicker`…); host messages of expected failures are written for users. `useRun()` (`src/lib/bridge-ui.ts`) runs a call and toasts its failure (cancellations stay silent).
- Shared UI-side logic: `src/lib/live.ts` (setup/session/window snapshots, app info, settings values), `src/lib/library.ts` (game status, art, matching names), `src/lib/downloads.ts` (the downloads store, see below), `src/features/add-game.tsx` (pick an executable → `library.addGame`, handler chooser, `library.setExecutable`), `src/features/file-drop.tsx` (drop a handler file anywhere), `src/components/HandlerNotes.tsx` (the Nucleus notes markup).
- The hub catalogue (`allhandlers`, `handler/:id`, `comments/:id`) is read directly from the public API (`src/hub`); only calls without CORS go through the host (`handlers.screenshots`, package downloads inside `handlers.install`).
- Dev tools: `await __bridge.library.list()` in the console; `localStorage.setItem('nucleus.bridge.trace', '1')` then reload logs every message.

What the UI does with each service, the UI-side features and the remaining contract wishes are in `docs/ui-bridge-notes.md`.

## Mock mode (normal browser)

Outside WebView2 the bridge loads the mock host (`src/bridge/mock/`, its own chunk, never downloaded by the app): a library of 13 games with artwork, two monitors, gamepads and keyboards that reveal themselves and show activity on timers (press-to-join happens by itself), profiles, a session that launches instances one by one, a schema-driven settings set, installs with progress. The hub catalogue, handler pages and comments are real (fetched live from hub.splitscreen.me). Dropping a handler file uses the file name as its path. The mock's pickers return the first executable the UI expects, in the folder it suggests.

## Running in the app

1. `npm run build`.
2. Build the exe (`Master/`): `MSBuild NucleusCoopTool\Nucleus.Coop.csproj -p:Configuration=Debug -p:Platform=AnyCPU -p:SolutionDir=<Master>\`. The `CopyWebUi` target copies `dist\` to `bin\Debug\webui\`.
3. Run `bin\Debug\NucleusCoop.exe`.

Hot reload inside the app: run `npm run dev`, put `http://localhost:5173/` on the first line of `bin\Debug\webui\dev-server.txt`, start the Debug build (delete the file to go back to the built UI). Remote debugging: set `NUCLEUS_WEBVIEW_DEBUG_PORT=9333` before starting the exe, then `chromium.connectOverCDP('http://127.0.0.1:9333')` (Playwright) or `edge://inspect`. Details: `docs/webui/host.md`.

## Structure

```
src/
  main.tsx, App.tsx        entry, providers (QueryClient, BridgeProvider, toasts, confirm, add game), routes
  bridge/                  bridge runtime, generated contract, mock host (owned by the bridge work)
  features/                cross-screen flows: add a game / locate it, drop a handler file
  hub/                     public Hub API client: fetch, normalise, persist, TanStack hooks, catalogue filters
  lib/                     bridge helpers, live snapshots, library derivations, downloads store, formatting, art
  components/              app-level shared components (Markdown, HandlerNotes)
  shell/                   app chrome: title bar (device tray, window buttons), rail, hint bar, offline banner,
                           command palette, couch navigation
  ui/                      design-system primitives (Button, Chip, PlayerTag, Field, Dialog, Confirm, Toast…)
  screens/                 one folder per route
  styles/index.css         tokens → Tailwind @theme, base layer, custom utilities and variants
docs/
  ui-bridge-notes.md       what the UI does with each service, UI-side features, contract wishes
  screens/                 screenshots of each route: mock-*.png (browser + mock), app-*.png (in the app); local only, not versioned (30 MB)
```

## Routes

| Route | Screen |
|---|---|
| `#/library` | Library: featured game, Found on this PC, Your games (sort, filters, favourites, game menu), popular on the Hub |
| `#/library/:gameId/setup` | Lobby: press to join, screens and slots, layouts, players, options and custom steps, profiles, checks, launch |
| `#/session` | Running session: instances and their stages, actions with hotkeys, stop, log, handler notes |
| `#/games` | Get games: the Hub catalogue with search, filters and sort (state in the URL) |
| `#/games/:handlerId` | Handler page: split preview, compatibility, notes, comments, install / update |
| `#/downloads` | Handler installs and updates, pending handler updates |
| `#/settings` | Settings (schema from the app), player identities, about |

## Conventions

- **Host access**: only through `@/bridge` (`bridge.*`, `useBridgeQuery`, `useBridgeMutation`, `useBridgeEvent`, `useBridgeSnapshot`). No `window.chrome.webview`, no file paths read by the UI, images come as ready URLs (remote or `https://assets.nucleus.example/…`). Links leave the app through `bridge.app.openExternal`.
- **Hub access**: only through `@/hub` hooks (`useAllHandlers`, `useHubHandler`, `useHubComments`). Simple GETs, no custom headers (the Hub has no preflight route). The last catalogue is saved in localStorage so the app starts offline.
- **Design tokens**: colours, radii, gutter, motion live in `styles/index.css` `@theme`. The palette is closed (`--color-*: initial`): `bezel`, `pane`, `pane-2`, `pane-3`, `line`, `line-strong`, `ink`, `ink-2`, `ink-3`, `on-light`, `ok`, `warn`, `danger`, and `p1`…`p8`.
- **Colour means a player**: the eight player colours are used only for players, slots and devices. Set the colour with `playerClass(n)` (utility `player-N` sets `--pc`), then use `bg-pc`, `text-pc`, `shadow-[…var(--pc)]`. Everything else is monochrome; status uses `ok` / `warn` / `danger` with an icon and words.
- **Type**: one family, width carries hierarchy: `type-display`, `type-h1` (wide), `type-h2` (112%), `type-h3`, and `stretch-wide | stretch-semi | stretch-normal | stretch-narrow`; `data` is the condensed tabular voice for numbers and compact facts.
- **Layout**: panes (`Pane`, 14px radius) on the black bezel with the app gutter (`gap-gutter`, 4px). Screens must not overflow at 1280×720 and 1920×1080; the stage scrolls vertically when a screen is taller. Scrollbars are styled thumbs (no native bar).
- **Couch-first**: every screen declares its breadcrumb and gamepad hints with `useScreenChrome(crumbs, { base, byFocus, aside })`. Arrow keys move between `data-nav` elements; the pad (`shell/couch-nav.tsx`, rules in `couch-nav-core.ts`) also reaches every focusable control, except under `data-nav="off"` (an element or a whole subtree: the title bar, mouse-only overlays). With nothing focused, the first move lands on `data-autofocus` (or the first target inside it), else on the stage. The right stick scrolls; d-pad up/down scrolls a page when nothing is that way. Pads belong to players in the lobby and the session: the running session calls `useSuspendCouchNav()`; the lobby calls `useSharedCouchNav()`, where every pad moves focus but only the pad that moved last (within 10 s) presses buttons and B never leaves the screen; joining comes from the host's device activity, on Start only, which never acts on the UI. An empty place clicked or activated waits for a controller: the lobby calls `useCapturedCouchNav()`, pads then only run hint actions (B cancels), and the next press of a free pad takes the place. Hints only name what a button does whatever has focus: A is always "Select".
- **Accessibility**: focus is always visible (white frame), dialogs use native `<dialog>`, icons are decorative unless given a `label`, live regions announce lobby and toast changes, `prefers-reduced-motion` zeroes durations and animations globally.
- **Copy**: English, sentence case, plain verbs; say what happens and what to do next.
- **User content**: hub markdown goes through `@/components/Markdown` (raw HTML skipped, remote images not loaded); handler notes (Nucleus markup) through `@/components/HandlerNotes`.
