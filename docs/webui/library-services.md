# Library, handlers and settings services

Status: v1, 2026-10-11. Code: `Master/Nucleus.Coop.Services/{Startup,Library,Handlers,Settings}`, tests: `Master/Nucleus.Coop.Services.Tests/{Library,Handlers,Settings}`. Contracts: `Nucleus.Bridge/Contracts/{Library,Handlers,Settings}.cs`.

These services replace the library, game menu, handler install/update and settings parts of the WinForms `MainForm`. The legacy UI (`--legacy-ui`) still works and shares the same files: `userprofile.json`, `metaInfo.json`, `Settings.ini`, `handlers\`, `gui\`.

## 1. Startup (`Startup/NucleusCore.cs`)

`NucleusServices.Register` calls `NucleusCore.Start` first, on the UI thread:

1. `new GameManager()`, as the MainForm constructor did: parses every `handlers\*.js` with Jint, loads `content\userprofile.json`, registers raw input. `GameManager.CheckUpdatesOnLoad = false` skips the per-game update checks of `LoadUser` (the legacy UI keeps them), so step 4 runs the only check. The duration is logged (`[services] INFO GameManager loaded N handler(s)… in X ms`). It stays synchronous on purpose: about 160 ms for one handler, so expect a few seconds with a hundred handlers. Moving it off the UI thread would need the other services to wait for it.
2. Shared objects: `HubClient`, `HubStatus`, `LibraryNotifier`, `UserProfileStore`, `GameIconCache`, `ArtworkDownloader`, `HandlerUpdateChecker`.
3. `GameMetaInfo.Saved` is wired to `library.changed`.
4. In the background: a HEAD request to the hub sets `Hub.Connected` (the MainForm used to set it), then an update check of the library games. Every change of `Hub.Connected` made by the services is sent as `app.hubStatusChanged`; the host's `app.getInfo` reads `Hub.Connected` live.

`SettingsService` draws the two shortcut reminder images at startup when they are missing.

Threading: every service method runs on the UI thread (bridge dispatch). Network, extraction, icon rendering and reminder drawing run on workers. GameManager collections and `UserGameInfo` objects are only changed on the UI thread: workers hand results back through `UiDispatcher` or by awaiting without `ConfigureAwait(false)`.

## 2. `library.*` (`Library/LibraryService.cs`)

| Method | Legacy code | Notes |
|---|---|---|
| `list` | `Core_Interface.RefreshGames` / `NewUserGame`, `GameControl`, `GetGameIcon` | Every library entry with a loaded handler, unsorted. Games whose exe is missing are listed with `executableFound: false` (legacy hid them until restart). `updateAvailable` = `GenericGameInfo.UpdateAvailable && metaInfo.CheckUpdate`. `latestHandlerVersion` comes from the last hub check. `heroUrl` = first screenshot. |
| `get` | `UI_Actions.ChangeGame`, `GetGameDetails`, `InputIcons`, `PlaytimePanel` | No side effects: content-folder cleanup and `GameProfile` statics belong to the setup service. `architecture` from the PE header (`MachineSpecs.GetMachineArch`). `showNotesFirst` = `FirstLaunch && ![Misc] DisableForcedNote && notes`. |
| `setFavorite` | `GameControl.FavoriteBox_Click` | metaInfo `Favorite`. |
| `getMenu` / `runMenuAction` | `GameOptionMenuFunc` (27 items) | See section 3. |
| `addGame` | `SearchGame.Search` + `GameManager.TryAddGame` | `GameManager.GetGames` matching (`ExecutableNames`, then `ExecutableName`, then `ExecutableContext`). Several matches: `candidates`, nothing added. None: `notFound` reason `noHandler`. exFAT: `conflict` reason `exFat`. Then: `FirstLaunch = true`, update check, and artwork download in the background when the handler has a hub id and the hub answers (legacy asked first). |
| `setExecutable` | none (new) | Relinks a moved game. The exe must match the game's handler like `addGame`; metaInfo, profiles and artwork are keyed by game id and stay. |
| `remove` | `RemoveGame.Remove` | Removes every entry of the game, deletes cover, screenshots and the cached icon. Handler, metaInfo, profiles and backups stay (as before). Refused for the game of a running session. |
| `scanInstalledGames` | none (new) | Steam: `HKCU\Software\Valve\Steam\SteamPath` → `steamapps\libraryfolders.vdf` (old and new formats) → `appmanifest_*.acf`. Epic: `%ProgramData%\Epic\EpicGamesLauncher\Data\Manifests\*.item`. Tolerant VDF parser in `InstalledGamesScanner.cs`. |
| `findExecutable` | none (new) | Breadth-first, case-insensitive, depth ≤ 6, ≤ 20 000 folders, skips links and `_CommonRedist`, `Redist`, `DirectX`, `__Installer`… |
| `downloadArtwork` | `AssetsDownloader.DownloadAllGamesAssets/DownloadGameAssets` | Same files and rules (cover `t_cover_big`, up to 5 screenshots `t_original`, existing files kept). One game at a time, `library.artworkProgress` before each game and once at the end (`done == total`), cancellation between files, atomic writes. `unavailable` when the hub can't be reached or every game failed. |
| `reload` | `Core_Interface.RefreshHandlers` (`new GameManager()`) | Parses every handler on a worker, swaps them in on the UI thread and gives every library entry a fresh `UserGameInfo` (they cache their handler). Does not register raw input again. Refused during a session. Raises `handlers.changed` and `library.changed` (all games). |
| `getPreferences` / `setPreferences` | `SortOptionsPanel`, `SortGameFunction`, `ToggleFavoriteButton` | Stored as before: `[Misc] GameSortingOpt` = `Sort_1` descending, `Sort_2` max players, `Sort_3` play time, `Sort_4` last played, `Show_Type1/2/3` gamepads / one keyboard / several keyboards; `[Dev] ShowFavoriteOnly`. Sorting and filtering are the UI's job; the legacy "a filter with no match shows everything" rule is not reproduced. |

`library.changed` (coalesced for 250 ms) is sent for: favourite and menu toggles, add/remove/relink, icon rendered, artwork downloaded, update state or hub version changed, reload, "disable game profiles" setting, and every metaInfo save, which includes the play time written every 20 s during a session and once more after it ends. It replaces the 1.5 s polling timer of each list row.

Play time: `playtimeSeconds` reads the stored seconds (also tolerates `"1234.0"` and the displayed `HHh:MMm:SSs`). `lastPlayedAt` parses the culture-formatted string with the current culture first, then invariant, ISO 8601 and 15 common cultures, and returns UTC (null when unknown).

Icons (`GameIconCache`): custom icon (`metaInfo IconPath`: image, .ico or .exe) or the executable's icon, rendered to `gui\icons\cache\<GUID>.png` (largest icon up to 128 px through `PrivateExtractIcons`, falling back to the legacy 32 px shell icon), with a `<GUID>.src` sidecar so a changed source is rendered again. Rendering happens on a worker; `list` returns the previous PNG or `icons/default.png` meanwhile, then `library.changed` follows.

URLs: `https://assets.nucleus.example/<path under gui>?v=<last write ticks>` (path segments escaped).

## 3. Game menu (`Library/GameMenuBuilder.cs`, `Library/GameMenuActions.cs`)

`GameMenuActions.CreateContext` captures a `GameMenuContext` (handler flags, metaInfo, settings, folders on disk); `GameMenuBuilder.Build` is pure and unit-tested. Items keep the legacy order and labels; separators are inserted between non-empty groups.

| Id | Kind | Shown when | Action |
|---|---|---|---|
| `notes` | action, `clientAction: showNotes` | handler has notes | The UI shows `library.get` notes; a server call is a no-op. |
| `details` | action, `clientAction: showDetails` | always | The UI shows `library.get` details; a server call is a no-op. |
| `openHandler` | action | always | `[Dev] TextEditorPath` if it exists, else Notepad++, else Notepad. |
| `openExeFolder` | action | always | Explorer; `notFound` when the folder is gone. |
| `openContentFolder` | action | `content\<GUID>` exists | Explorer. |
| `deleteContentFolder` | action, danger, confirm | same | `CleanGameContent.CleanContentFolderAsync`: kills leftover processes, deletes the folder. |
| `openUserConfig` / `deleteUserConfig`, `openUserSave` / `deleteUserSave`, `openDocumentsConfig` / `deleteDocumentsConfig`, `openDocumentsSave` / `deleteDocumentsSave` | submenus | at least one `<profile root>\<path>` exists | Children `openUserConfig:<full path>` etc. Labels as before ("Nucleus: Bob", user name); a repeated label gets its path appended. |
| `openBackup` / `deleteBackup` | submenus | backups exist | Children `openBackup:<nickname>`. |
| `useApiIndex` | toggle | `((XInput && !Reroute && !ProtoDinput) || ProtoXinput || SDL2) && !UseDevReorder && !CreateSingleDeviceFile && !DInput` | Turning it on turns button press off. |
| `useGamepadButtonPress` | toggle | profiles not disabled globally nor for the game | Turning it on turns index mode off. |
| `useApiIndexForGuests` | toggle | the `useApiIndex` rule && `PlayersPerInstance > 0` && `!UseApiIndex` && `!DisableProfiles` | |
| `keepInstancesFolder` | toggle | handler does not set `KeepSymLinkOnExit` | |
| `disableProfiles` | toggle | profiles not disabled globally | |
| `disableHandlerUpdate` | toggle (checked = updates off) | always | Turning updates back on checks the hub in the background. |
| `changeIcon` | action, `clientAction: pickFile` + `fileFilters` | always | The UI picks a file, then calls with `value` = its path (png, jpg, jpeg, jfif, gif, bmp, tif, tiff, ico, exe). |
| `gameAssets` › `openCoverFolder`, `openScreenshotsFolder` | submenu | cover / screenshots exist | Explorer (the cover is selected). |
| `steamLanguage` | choice | `(SteamID && UseGoldberg) || UseGoldbergNoOGSteamDlls || NeedsSteamEmulation` | `value` in the listed languages ("App Setting" first). |
| `remove` | action, danger, confirm | always | `library.remove`. |

`runMenuAction` rebuilds the menu and refuses ids it does not contain (`notFound`, reason `itemUnavailable`), so a dynamic id can only target a folder the menu offers. Destructive items are refused during a session. The gamepad toggles and `disableProfiles` raise `LibraryHooks.SetupInvalidated(gameId)` for the game being set up, and the two index/button-press toggles set `GameProfile.UseXinputIndex` like the legacy code.

## 4. `handlers.*` (`Handlers/HandlersService.cs`)

| Method | Legacy code | Notes |
|---|---|---|
| `listInstalled` | `HubWebView.BuildHandlersDatas` | Every loaded handler, with hub id, version, `inLibrary`. |
| `install` | `DownloadPrompt` (download mode) | `GET /api/v1/handler/{id}` → download `…/cdn/storage/packages/{currentPackage}/original/handler-{id}-v{currentVersion}.nc?download=true` (the hub counts it) into a hidden `handlers\.install-xxxx\` work folder, extract, install, `GameManager.AddScript`. Replaces an installed version of the same game. Returns `executableNames` (all accepted executables, for the picker filter) and `suggestedFolder` from `GameManager.AutoSearchGameInstallPath` (Steam) when the game is not in the library. |
| `update` | `UI_Functions.Button_UpdateAvailable_Click` + `DownloadPrompt(…, overwriteWithoutAsking: true)` | Same as `install` with the game's hub id. The library entry is rebound by `AddScript`; its exe path keeps its case. |
| `installFromFile` | `UI_Functions.ExtrtactHandlerDragDrop`, `ExtractHandler.Extract` | `.nc`/`.zip`: installed as `handlers\<sanitized Game.GameName>.js` + `handlers\<name>\`. `.js`: copied (not moved) under its own file name. `conflict` reason `alreadyInstalled` when `<name>.js` exists or another installed script declares the same `Game.GUID`, unless `overwrite`; with `overwrite`, that other script and its folder are removed too. |
| `checkUpdates` | `GameManager.LoadUser` / `Hub.CheckUpdateAvailable` | Library games with update checks on, 4 requests at a time; changes arrive through `library.changed`. `unavailable` when every request failed. |
| `screenshots` | `HubCache.GetScreenshotsUri` | `GET /api/v1/screenshots/{id}`, `t_thumb` → `t_screenshot_big`, `https:` prefix. |

`handlers.progress` (with `gameId` for updates, and on `done`) reports `downloading` (percent when the size is known), `extracting` (real percentage), `done` or `failed` (with the error, "Cancelled." on cancellation). Installs run one at a time and are refused during a session.

Extraction core (`HandlerFiles`), extracted from `DownloadPrompt.ExtractHandler`: same layout (`handler.js` → `<title>.js`, every top-level folder merged into `<title>\`). Script fields are read by line prefix like before (`HandlerScript`), but `Game.ExecutableNames` no longer matches `Game.ExecutableName`, indentation and single quotes are accepted, and the value stops at its closing quote.

HTTP (`HubClient`): own `HttpClient` with `WebRequestHandler`, per-request certificate validation that keeps the default result (the process-wide accept-all callback set by legacy helpers is not used, and not changed), TLS 1.2 ensured, timeouts (API 10 s, images 20 s, downloads fail after 30 s without data), hub ids validated before they go into URLs.

`IHandlerUpdater` (implemented by `HandlersService`, passed by `NucleusServices.Register` to the setup session): `IsUpdateAvailable`, `LatestVersion`, `CheckAsync` (asks the hub now), `UpdateAsync`.

## 5. `settings.*` (`Settings/`)

Schema (`SettingsCatalog`), values mapped onto the `App_*` classes (setters write `Settings.ini`), defaults from the shipped `Settings.ini`:

| Section | Fields | Content |
|---|---|---|
| `general` | 10 | Steam language, Epic/GOG language, network adapter (live list), use nicknames in game, keep Nucleus user accounts, Nucleus accounts password, desktop scale 100 %, input lock reminder, debug log, disable game profiles |
| `players` | 2 | Assign gamepads by index / on button press (defaults for new games). Nicknames and Steam IDs: `getPlayers` / `setPlayers`. |
| `hotkeys` | 10 | 9 shortcuts (`Ctrl|Alt|Shift` + letter or digit, as stored) and the lock key |
| `gamepad` | 17 | 11 shortcut pairs (stored `1024+32768`), UI navigation on/off, left click, right click, drag and drop (single buttons), stick dead zone, gamepad type |
| `audio` | 9 | Default device for all / per instance, instances 1-8 (live device list; value = endpoint id) |
| `layout` | 11 | Divisions (max players recomputed on save), split-screen background and colour, hide desktop only, 3 cutscenes options, Windows merger, lossless hook, merger resolution (modes of every screen) |

Field metadata: the password field is `password` with `maxLength` 127; gamepad fields carry `comboButtons` (2 for shortcuts, 1 for the navigation clicks); `visibleWhen` / `enabledWhen`: assignment modes only when profiles are on, audio instances only in per-instance mode, background colour and "hide desktop only" with split-screen division, the two cutscenes options without "mute audio only", lossless hook and merger resolution with the Windows merger. `settingsSchema.gamepadButtons` lists every code with its name for the current gamepad type.

Gamepad codes: 1 D-pad Up, 2 Down, 4 Left, 8 Right, 16 Start, 32 Back, 64 Left Stick, 128 Right Stick, 256 LB, 512 RB, 1024 Guide, 4096 A, 8192 B, 16384 X, 32768 Y, 9999 RT, 10000 LT (`GamepadComboFormat.Buttons`, with PlayStation names).

`setValues` validates everything first (`SettingsValidator`) and writes nothing when a field is invalid: types and ranges, hotkey format, duplicate hotkeys, duplicate gamepad pairs (compared by the sum of their codes, as the shortcut thread does), both triggers in one pair, the two assignment modes both on, "mute audio only" with the other cutscenes options, password length (127) and characters the ANSI `Settings.ini` can't hold. A problem that already exists between untouched fields does not block other saves. Choice fields accept the stored value even when the PC no longer offers it (unplugged device). After the save: `GamepadShortcuts.UpdateShortcutsValue` / `GamepadNavigation.UpdateUINavSettings` when gamepad values changed, `library.changed` and `LibraryHooks.GameProfilesSettingChanged` for "disable game profiles", reminder images redrawn, `settings.changed` with the changed keys.

`setPlayers`: 32 slots; empty nickname = `PlayerN`, empty Steam ID = default; nickname ≤ 9 characters, no file-name characters (they become folder names), unique (case-insensitive); Steam ID = 17 digits, unique among effective IDs (an empty slot counts as its default; two empty slots are not duplicates any more). Writes `[ControllerMapping]`, `[SteamIDs]`, `Nicknames.json`, `SteamIds.json`.

`captureGamepadCombo({ buttons })` (`GamepadComboCapture`): polls the 4 XInput slots on a worker (Guide button through `XInputGetStateSecret`, SharpDX fallback without it) and waits until every button is released. With `buttons: 2` it returns the first two buttons pressed together on one pad (`1024+32768`; a button released alone starts over); with `buttons: 1`, the first button pressed (`512`). Triggers count above 30/255. `unavailable` when no pad is connected or after 2 minutes; cancellation stops it.

Reminder images (`ShortcutsReminderRenderer`): `gui\shortcuts\KbShortcutsReminder.jpeg` (514×424) and `XShortcutsReminder.jpeg` (416×680), the proportions of the boxes the in-game `ShortcutsReminder` window stretches them into. A dark list with keycaps; gamepad buttons use the theme's `gamepads\<type>\*.png` pictures when present, names otherwise.

### Skipped legacy settings

| Setting | Why |
|---|---|
| Theme (`[Theme] Theme`) | The theme system is obsolete with the web UI (restart-based WinForms theming). |
| Window and form positions (`WindowSize`, `WindowLocation`, `SettingsLocation`, `ProfileSettingsLocation`, `GamepadSettingsLocation`, `AutoSearchLocation`) | WinForms placement; the host keeps its own `HostWindowPlacement`. |
| "Controller Shortcuts Setup" button | The window became the `gamepad` section. |
| Goldberg emulator updater button | An action, not a setting; can come later as its own call. |
| Audio refresh button, "Default: device" label | The lists are re-read on every `getSchema`; the default device is in the audio section description. |
| Screen identify on merger rows, read-only "Window Resolution" box, max players box | Helper/display only; max players is computed on save. |
| Nickname/Steam ID swap behaviour of the combo boxes | UI behaviour; the server validates uniqueness. |
| Ini-only keys never shown by the legacy form (`VGMOnly`, `DisableForcedNote`, `NucleusMultiInstances`, `DisablePathCheck`, `TextEditorPath`, `Blur`, `OSDColor`, `EnableLogAnswer`) | Not in the legacy UI; they keep working from `Settings.ini`. |
| `GameSortingOpt`, `ShowFavoriteOnly` | Exposed as `library.getPreferences`. |

## 6. Behaviour changes and fixed legacy bugs

- `metaInfo.json`: every read-modify-write is serialized with a lock; a first run without the file now saves the real defaults (it saved `CheckUpdate=false`, `SaveProfile=false`, `FirstLaunch=false` for the first handler); play time parsing no longer throws on a bad value (`GameMetaInfo.cs`).
- `userprofile.json`: saved by `UserProfileStore` (latest snapshot always written, atomically) instead of `GameManager.SaveUserProfile`, which dropped saves requested while one was running. Exe paths keep their case (`UserGameInfo.InitializeDefault` lowercases every `I`).
- Adding a game that is already in the library moves it to the new exe instead of listing the same game twice.
- Gamepad "Alt+Tab" (ReleaseCursor) used the Toggle top most combination (`GamepadShortcuts.cs`); digit hotkeys registered the wrong key (`HotkeysRegistration.cs`: `Enum.Parse(Keys, "1")` is the left mouse button). Both fixed for both UIs.
- Installs never delete or move the user's file; archives with unsafe paths are refused; progress is a real percentage; a script with the same `Game.GUID` under another file name is detected.
- Menu: dynamic items target real paths (legacy rebuilt the path from the label and opened `%USERPROFILE%\<path>` for the Documents root); "guests by index" combines both halves of the legacy rule; the inert "/!\ OneDrive /!\" item is dropped.
- Settings saves are atomic (validated first); two empty Steam IDs are no longer duplicates; duplicate hotkeys and gamepad combinations are refused.
- Artwork download after adding a game happens without asking.

## 7. Legacy code: what moved, what stayed

New code lives in the services; the legacy forms were not changed and keep their own copies of this logic: `DownloadPrompt`/`HubWebView` (extraction), `SearchGame`, `RemoveGame`, `GameOptionMenuFunc`, `SortGameFunction`, `AssetsDownloader`, `GetGameIcon`, `Settings`, `XInputShortcutsSetup`. Their extraction mixes modal dialogs with the logic (overwrite prompt in the middle of an extraction, `Application.DoEvents` loops), so sharing the code would have meant rewriting them.

NucleusGaming files touched: `Coop/GameManager.cs` (`CheckUpdatesOnLoad` flag), `Coop/GameMetaInfo.cs` (lock, `Saved` event, `TotalPlayTimeRaw`/`LastPlayedAtRaw`, first-run defaults, tolerant parse), `Tools/CleanGameContent/CleanGameContent.cs` (`CleanContentFolderAsync`), `Coop/InputManagement/Gamepads/GamepadShortcuts.cs` (ReleaseCursor), `Coop/InputManagement/HotkeysRegistration.cs` (digit keys).

Host file touched (agreed with the lead): `NucleusCoopTool/Host/Services/AppService.cs`, `AppInfo.HubOnline` reads `Hub.Connected` live instead of the startup check.

## 8. Known gaps

- `GameManager` loads synchronously on the UI thread at startup (see section 1).
- `GameManager.AddScript` still shows a WinForms message box for handlers that throw `ArgumentException`; handler script errors still show the legacy "Error in handler" window.
- `deleteContentFolder` may show the legacy "Risk of crash!" window when files stay locked.
- Icons are taken from the executable's icon resources (up to 128 px); executables without icons fall back to the shell's 32 px icon.
- The reminder images can't be replaced while the in-game reminder shows them (logged, kept until the next save).
- `Nucleus.Coop.Services` and `Nucleus.Coop.Services.Tests` are not in `NucleusCoop.sln` yet (the services project builds through the exe's project reference). Tests: `dotnet test Master/Nucleus.Coop.Services.Tests/bin/Debug/net472/Nucleus.Coop.Services.Tests.dll` after building the test project with MSBuild.
