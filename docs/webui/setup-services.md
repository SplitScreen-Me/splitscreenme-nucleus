# Setup, profiles and session services

Status: v1, 2026-10-11. Scope: the headless replacement of the WinForms game setup (`SetupScreenControl`, `Core_Interface` step machine, `PlayerOptionsControl`, `JSUserInputControl`, `ProfilesList`, `ProfileSettings`) and of the Play/Stop logic, behind the bridge contracts `setup.*`, `profiles.*` and `session.*` (`Master/Nucleus.Bridge/Contracts/Setup.cs`, `Profiles.cs`, `Session.cs`). The legacy UI was removed on 2026-10-12. The WinForms setup screen (`SetupScreenControl`, `Draw`, `UserInputControl`) and its seams (section 5) were removed on 2026-10-12: the setup services are the only users of `BoundsFunctions`/`DevicesFunctions`.

Code: `Master/Nucleus.Coop.Services/Setup`, `Profiles`, `Session`. Seams: `Master/NucleusGaming` (refactors R1 to R13 below). Tests: `Master/Nucleus.Coop.Services.Tests/Setup`, `Profiles`, `Session`.

## 1. Pieces

| Class | Role | Legacy counterpart |
|---|---|---|
| `Setup/SetupSession` | Shared state of the three services: open game, current step, pump, coalesced `setup.changed`, legacy event wiring, notice hook | `Core_Interface` statics, `UI_Actions.ChangeGame`, `SetupScreenControl.OnPaint` side effects |
| `Setup/SetupService` (`ISetupService`) | Commands: open/close, navigation, placement, options, identity, audio, checks, devices | `SetupScreenControl` mouse handlers, `Core_Interface.GoToStep/StepCanPlay`, options controls, game menu toggles |
| `Setup/SetupSnapshotBuilder` | Maps `GameProfile` + `BoundsFunctions.Screens` + the handler to `SetupSnapshot` | `Draw.*` (what the setup screen showed) |
| `Setup/SetupFlowRules` (pure) | Visible steps, Next/Back, CanPlay, where a matched profile goes | `Core_Interface.GoToStep/StepCanPlay` |
| `Setup/OptionsCodec` (pure) | Option kinds, labels, stored values, profile text → list item, defaults merge | `PlayerOptionsControl`, `JSUserInputControl` |
| `Setup/DeviceIds`, `Setup/SnapshotMath` (pure) | Device ids/names/kinds, layout grids, slot occupancy | `Draw`, `UserScreen.UpdateSubBounds` |
| `Setup/SetupChecks` | "Before you launch" checks and fixes | `CheckGameRequirements` (message boxes) |
| `Setup/AudioOutputs` | Render endpoints (NAudio, cached) | `ProfileSettings.RefreshAudioList` |
| `Profiles/ProfilesService` (`IProfilesService`) | List, load, unload, delete, settings | `ProfilesList`, `ProfileSettings` |
| `Session/SessionService` (`ISessionService`) | Play, Stop, end handling, instances, actions | `Core_Interface.PlayClicked`, `I_GameHandlerEndFunc`, `Handler_Ended`, `HotkeyListener` |
| `Session/SessionLog` | Launch log ring buffer, `session.log` | `GenericGameHandler.Log` (debug-log.txt) |
| `Session/SessionSummaryBuilder` (pure) | `SessionSnapshot.LastSession`: players and device names, duration, outcome, error lines | none (new) |

Registration: `NucleusServices.Register` creates one `SetupSession` and the three services after the library services (which create `GameManager`). `SetupSession.HandlerUpdater` is the handlers service (`IHandlerUpdater`) for the handler update check.

## 2. Contract methods and the code behind them

### setup

| Method | Implementation |
|---|---|
| `open` | `SetupSession.Open`: `GameProfile.GameInfo/Game` (as `Core_Interface.Current_UserGameInfo`), `CleanGameContent.CleanContentFolderAsync` (not awaited) unless KeepSymLink, `new GameProfile().InitializeDefault()`, `GenMissingIdFromPlayerSteamIDs` (was in `SetupScreenControl.Initialize`), `BoundsFunctions.Initialize`, `profile.SyncScreens()`, `DevicesFunctions.Initialize`, options defaults (`OptionsCodec.Merge`), players step. Requirements are no longer message boxes: they are checks. |
| `close` | Stops the device timers and the screens watcher (`RefreshUI(true)`). Refused during a session. |
| `get` | `SetupSession.Commit()`: builds the snapshot; `Revision` increases only when the snapshot differs from the previous one. During a session it returns the snapshot taken at Play. |
| `next` / `back` | `SetupFlowRules.Next/Back` after re-evaluating each custom step's `UpdateRequired`. `back` sets `GameProfile.Ready = false` (as `GotoPrevStep`). |
| `setLayout` | `BoundsFunctions.SetLayout` = the layout icon (`ApplyScreenLayout`, shared with the mouse handler): players of the screen go back to the tray. Same layout: no change. Refused with a profile (`profileLocked`). |
| `join` | `BoundsFunctions.Join`: first free slot (screens in order; a raw keyboard or mouse completes a lone complementary device first); when full, the first screen whose layout can grow switches to the next layout (FullScreen → DualHorizontal → FourPlayers → SixPlayers → EightPlayers → SixteenPlayers; DualVertical → FourPlayers) and its players are placed again in order, keeping their numbers; with a profile, the next expected ghost slot. `full` otherwise. |
| `assignToSlot` | `BoundsFunctions.AssignToSlot`: the drop logic (`CheckFreeSpace`, split out of `GetFreeSpace`) with the destination taken from the slot. A lone complementary raw device makes a keyboard+mouse player; another player in the slot swaps places with the device's player (it goes to the tray when the device came from the tray); both keep their player numbers. Refused swap with a profile. Moving one device of a keyboard+mouse pair moves only that device (legacy drag semantics). |
| `assignAt` | `BoundsFunctions.AssignAt`: point → virtual canvas, `GetFreeSpace` then `CanFillGap` (manual layout, gaps). |
| `unassign` | `BoundsFunctions.Unassign` (`RemovePlayer`, or guest removal). |
| `addGuest` / `removeGuest` | `BoundsFunctions.AddGuest/RemoveGuest`: hover rules of `OnMouseMove` (gamepads, `PlayersPerInstance`, profile `CurrentMaxGuests`), `RemoveGuestCore` shared with the right click. |
| `toggleExpand` | `BoundsFunctions.ToggleExpand` → `ToggleExpandCore` (the right-click block, extracted unchanged). |
| `resize` | `BoundsFunctions.Resize` → `ResizeStep` (the sizer blocks of `EditPlayerBounds`, extracted; one step = cell/20, one cell in the manual layout), repeated `steps` times; it stops at the first step that can't be made. The snapshot gives the step of each screen in pixels (`SetupScreen.ResizeStepWidth/Height`, `SnapshotMath.ResizeStep`, 0 where players can't be resized), so the UI turns a drag into steps. |
| `autoFill` | `BoundsFunctions.AutoFill`: revealed, unplaced devices in free slots, in device order (see behaviour changes). |
| `clearAll` | `GameProfile.Instance.Reset()` (unassign + unload), option values kept when no profile was loaded. |
| `setOptionChoice` / `setOptionText` | Stores the original `GameOption.List` item (string, double, ExpandoObject) or enum value, or the text, in `profile.Options[key]`. |
| `selectStepItem` | Stores the card object (ExpandoObject); on the current step, moves to the next step (`CanPlayUpdated(true, true)` of `JSUserInputControl`). `itemIndex` is `OptionChoice.Index` (the list index; non-card items are not listed). |
| `setPlayerIdentity` | `PlayersIdentityCache.SetNicknameAt/SetSteamIdAt` (global slots, settings.ini) + JSON history backups; also the loaded profile's template for that player; the pump refreshes `Nickname/SteamID`. |
| `setPlayerAudio` | `GameProfile.AudioCustomSettings = true`, `AudioInstances["AudioInstance{n}"]` (players 1 to 8). |
| `runCheckAction` | `gameFound` → conflict `needsPicker` (the UI opens its picker, then `library.setExecutable`); `handlerUpToDate` → `IHandlerUpdater.UpdateAsync`, then the setup reopens on the new handler; `admin` → restart elevated (`runas`) and exit, as `CheckGameRequirements.RestartAsAdmin`. |
| `setUseApiIndex` | `MetaInfo.UseApiIndex` (and button-press mode off when on), `GameProfile.UseXinputIndex` (devices rebuilt), reload of a loaded profile, players step. |
| `setSaveProfileOnLaunch` | `MetaInfo.SaveProfile`. |
| `refreshDevices` | `DInputManager.Refresh`, SDL refresh on its helper thread, raw keyboards/mice rescanned (`GameProfile.RefreshKeyboardAndMouse`, placed ones unassigned first), audio outputs. `deviceLost` is muted for 3 s. |
| `rumble` | XInput: full vibration for 300 ms (timer, never blocks); SDL: `SDL_GameControllerRumble`; others: `rumbleNotSupported`. |

### profiles

| Method | Implementation |
|---|---|
| `list` | `GameProfile.ReadProfiles()` (new, parses `Profile[N].json` by name instead of relying on the length sort) → `ProfilesService.Summary` (the data of the old preview text). |
| `load` | `GameProfile.Instance.LoadGameProfile(id)` with its error box captured into the error of the call; then `OptionsCodec.Merge` (defaults + saved values mapped back to list items, cards restored → `autoSelected`), tray, players step, pump. Devices are matched by the pump as they are detected or used; once complete the flow moves on and AutoPlay starts the session. |
| `unload` | `GameProfile.Instance.Reset()`. |
| `delete` | `GameProfile.DeleteProfile(id)` (moved from `ProfilesList.DeleteBtn_Click`: delete, renumber to keep `Profile[1..n]`, reset). Emits `profiles.changed`. |
| `getSettings` | `ProfileSettingsModel.FromCurrent()` (the form's `UpdateUiValues`/`GetPlayersNickNameAndIds`) + choices: network interfaces, audio outputs, merger resolutions (the form's enumerations). |
| `saveSettings` | `ProfileSettingsModel.Apply()` (the form's `CloseBtnPicture_Click`): validation returned as `ValidationError` (`slots.nickname`, `slots.steamId`… with `index`), then statics; a saved profile is rewritten (`UpdateGameProfile`) and reloaded. Steam IDs imposed by the handler are kept. |

### session

| Method | Implementation |
|---|---|
| `get` | State machine: `idle` (no setup), `setup`, `launching` (from Play until `GameProfile.Saved`, set when Play finished placing the windows), `running`, `ending`. Instances come from the launch players (the cloned profile's list). `LastSession`, built when a session ends or fails to start and kept until the next Play: the players and device names of the setup snapshot taken at Play, start and end, whether it reached `running`, the error, and the error lines of the log (20 at most, with their count). |
| `play` | Requires `Flow.CanPlay`. Pump once (fresh identities), then `GameProfile.InitializeHandlerStartup(OnHandlerEnded)` on the UI thread (it disposes the device timers, reloads the script, `CleanClone`, starts the handler threads), `shell.MinimizeWindow()`, state `launching`, 500 ms poll. Launch dialogs stay native (they are ownerless and run on the play thread). |
| `stop` | Refused while inputs are locked (`End` would return at once). State `ending`, `End(true)` on a worker (about 9 s of sleeps); if `End` returned early anyway, the previous state comes back with an error. |
| Ended (any thread) | Posted to the UI thread once: `I_GameHandler = null`, `SetupSession.ResetAfterSession()` (always), `shell.RestoreWindow()`, state `setup`, `profiles.changed` (the launch may have saved a profile). A handler that ended without raising `Ended` is caught by the poll. |
| `runAction` | The function each hotkey calls (`HotkeyListener.WndProc`), on a background STA thread like the hotkey listener: `ShowHideWindows`, `ChangeForegroundWindow`, `ResetingWindows = true`, `ToggleCutScenesMode`, `SwitchLayout`, `ShortcutsReminder.Toggle(7)`, `WindowsMerger.SwitchChildFocus`. Refused while inputs are locked, except the cutscenes mode (as the hotkeys). `Actions` lists them with their `App_Hotkeys` strings and availability (`Saved` for layout and cutscenes, reminders present, merger on). |
| `getLog` | `SessionLog`: every `GenericGameHandler.Log` line of the current or last session (500 lines), level and player guessed from the words. |

### Events

| Event | When |
|---|---|
| `setup.changed` | Coalesced (about 100 ms) after commands, device ticks (300 ms), flashes, profile matching, monitor changes. Only when the snapshot changed (JSON comparison); never during a session. |
| `setup.deviceActivity` | `PlayerInfo.FlashChanged` edges (on/off), deduplicated per device. `FlashIcon` already limits the rate (250 ms, off 500 ms after the last input). |
| `setup.deviceLost` | `DevicesFunctions.AssignedDeviceLost` (a placed device or a guest unplugged), with the player number and nickname. The flow goes back to the players step, as the WinForms UI did. |
| `profiles.changed` | Delete, save of a saved profile's settings, after a session. |
| `session.changed` | State changes, poll (500 ms) when the snapshot changed, setup open/close. |
| `session.notice` | Every message of the on-screen display during a session (`WPF_OSD.MessageShown`): hotkeys, launch, profile code ("Handler Profile Saved. Setup Finished"). |
| `session.log` | New log lines, batched at most every 200 ms. |

## 3. The pump

The WinForms setup screen did business logic while painting: `GameProfile.FindProfilePlayers` (profile matching and auto play), `GameProfile.UpdateProfilePlayerIdentity` (nickname, Steam ID, guests per player) and the reveal of keyboards and mice (`Draw.UIDevices`). A hidden control never paints, so:

- R2 moved the first two into `GameProfile.ProcessProfileAssignment()` (it took a `dragging` flag while the WinForms screen existed). `SetupSession.Pump()` calls it after every command, after every device tick (`DevicesFunctions.DevicesUpdated`) and on every input flash.
- R3 moved the reveal into `PlayerInfo.FlashIcon`.
- `GameProfile.Reset()` rebuilds the options from the raw handler values (`""` for lists). Resets that don't come from a command (monitor change, profile matching errors) are caught through `GameProfile.OnReset`, which posts the defaults merge.
- The pump never calls `UpdateDevices` (which raises `DevicesUpdated`), so it can't loop. The players step validity is computed by `SetupSession.PlayersValid()` (the `DevicesFunctions.CanProceed` rule plus the profile guests).
- `FindProfilePlayers` raises `GameProfile.CanPlayChanged(true, true)` and `OnReadyAutoPlayTrigger` from inside the loop: the session posts its reaction (`OnProfileMatched`) instead of acting in the loop. It goes to the first required custom step without a card, else the last step, and with AutoPlay starts the session only when the flow allows it (a card that couldn't be restored stops the auto play instead of launching with the profile's JSON text).

## 4. Threading

- Every state change runs on the UI thread (the WebView2 host's WinForms context, `IHostShell.UiContext`). Bridge calls arrive there.
- R4: in headless mode `DevicesFunctions.Dispatch` posts to the UI thread. The 300 ms device tick and the 150 ms polling tick post their whole body (one queued at most), so `DevicesList`, `AssignedDevices` and the screens are only touched on the UI thread. The screens watcher (3 s) posts too. Nothing uses `Control.Invoke`.
- Measured with the smoke test (Valheim handler: 32 OpenXinput slots probed, raw keyboards and mice, no gamepad connected): UI thread callbacks delayed 0.2 ms on average, 7 ms at most.
- SDL is initialized, refreshed (it sleeps 1 s) and hot-plugged on a dedicated STA thread with a message loop ("Nucleus SDL"); the ticks skip SDL while it refreshes (`Monitor.TryEnter`). `DevicesFunctions.Dispatch` is required: `BoundsFunctions.Initialize` and `DevicesFunctions.Initialize` throw without it, and the timer ticks do nothing.
- Raw input keeps its own thread; `FlashIcon` and `FlashChanged` come from it and are posted.
- `End` runs on a worker; `Ended` is posted. Session actions run on a background STA thread. Nothing blocks the UI thread except the existing synchronous work of `InitializeHandlerStartup` (script reload, clone, thread start), as in the WinForms UI.

## 5. Refactors (made while the WinForms UI still ran on the same code)

The NucleusCoopTool parts of R12 and R13 (`Core_Interface.cs`) were deleted with the legacy UI. The WinForms halves of R1 to R7 went with the setup screen on 2026-10-12: `AssignmentChanged`, the `dragging` parameter, the paint-side reveal, the `parent` branches and the WinForms timer/SDL mode, the mouse handlers and cursor moves; `InitializeHeadless` became `BoundsFunctions.Initialize`.

| # | File | Change |
|---|---|---|
| R1 | `Coop/GameProfile.cs`, `Controls/SetupScreen/SetupScreenControl.cs` | `GameProfile.CanPlayChanged` / `AssignmentChanged` events replace the direct `SetupScreenControl.Instance` calls (`Reset`, `FindProfilePlayers`); the setup screen subscribes and forwards (same Invoke-when-needed behaviour). |
| R1b | `Coop/GameProfile.cs` | `GameProfile.Notify(kind, title, text, ms)` hook replaces the 7 `MainOSD.Show` / `NucleusMessageBox.Show` calls; default = the OSD for information, `UserPrompts.Notice` (origin `Setup`, so a `notices` entry in the web UI) for errors. |
| R2 | `Coop/GameProfile.cs`, `SetupScreenControl.cs` | `ProcessProfileAssignment(dragging)` extracted from `OnPaint`, which calls it before drawing. |
| R3 | `Coop/PlayerInfo.cs` | `FlashIcon` reveals keyboards/mice and raises `PlayerInfo.FlashChanged`; the paint-side reveal in `Draw.cs` stays as a harmless duplicate. |
| R4 | `Controls/SetupScreen/DevicesFunctions.cs` | `Dispatch`, `CanProceed`, `DevicesUpdated`, `AssignedDeviceLost`, `IsRunning`, `RequestSdlRefresh`; null-safe `parent`; idempotent `Initialize` (old timers disposed), `PollingTimer` nulled; `DeletePlayer` guards the screens; `UpdateUseGamepadApiIndex` also drops the assignments of the rebuilt devices; `UpdateDevices` public; SDL helper thread in headless mode. |
| R5 | `Controls/SetupScreen/BoundsFunctions.cs` | `SurfaceSize` virtual canvas (1280×720) used when there is no control (the control's live size otherwise), `IsInitialized`, `InitializeHeadless`, `StopScreensWatcher`, `ScreensChanged`; previous watcher disposed; the watcher returns while a session runs and posts through `Dispatch`; `parent?.Invalidate`, cursor moves only with a control. |
| R6 | `Coop/GameProfile.cs` | `BoundsFunctions.IsInitialized` replaces the `SetupScreenControl.Instance != null` guards (so headless profiles get screens); `SyncScreens()`; `FindScreenOrAlternative` no longer loops forever without screens or with a negative index. |
| R7 | `Controls/SetupScreen/BoundsFunctions.cs` | Commands without a mouse (`SetLayout`, `AssignToSlot` with swap, `AssignAt`, `Unassign`, `AddGuest`, `RemoveGuest`, `ToggleExpand`, `Resize`, `AutoFill`, `Join`) returning a refusal reason. The mouse handlers call the shared cores (`ApplyScreenLayout`, `RemoveGuestCore`, `ToggleExpandCore`, `ResizeStep`, `CheckFreeSpace`). `AddPlayer` is idempotent and the drag hover undoes its keyboard/mouse merge when the device moves on (no more duplicates in `DevicesToMerge` or inflated counters). |
| R8 | `Coop/PlayerInfo.cs`, `DevicesFunctions.cs`, `Coop/InputManagement/RawInputManager.cs` | `PlayerInfo.DeviceKey` set at detection: `dinput:{guid}`, `xinput:{slot}`, `sdl:{index}` (or `sdl:h{handle}`), `kb:single`, `rawkb:{hid}`, `rawmouse:{hid}`, `virtual-kb`, `virtual-mouse`, `fake:*`. The services make duplicates unique (`#2`). Not upgraded when an XInput/SDL pad learns its GUID (the id must not change under the UI). |
| R9 | `Coop/GameProfile.cs`, new `Coop/ProfileSettingsModel.cs` | `UpdateProfilePlayerIdentity` hardened (unassigned or 33rd player, missing handler Steam IDs, unparsable values); `ReadProfiles()`, `DeleteProfile(id)` (from `ProfilesList`); `ProfileSettingsModel` (validation + apply of `ProfileSettings`), creating `game profiles\` before the history backups. `RefreshKeyboardAndMouse` public. The options codec lives in the services (it would break the WinForms card auto-select if applied in `LoadGameProfile`). |
| R10 | `Coop/GameProfile.cs` | `InitializeHandlerStartup(Action onEnded)`: `Ended` subscribed between `MakeHandler` and `Initialize`. The parameterless overload (legacy UI only) was removed with it. It throws when the handler script no longer parses, instead of launching the previously parsed version (`session.play` answers `unavailable`, the script error is a notice). |
| R11 | `Coop/InputManagement/RawInputProcessor.cs`, `Coop/GameManager.cs` | `Globals.MainOSD?.Show` in the raw input loop; raw input registered once per process (one shared `RawInputProcessor`, no second window on a handlers refresh). |
| R12 | new `Coop/InputManagement/Gamepads/GamepadThreads.cs`, `NucleusCoopTool/UI/Core_Interface.cs` | `GamepadThreads.Start()` (no UI control), returns false on Windows 7/Vista; `InitializeGamepadThreads` calls it and keeps its WinForms touches. Hosts call `GamepadThreads.Start()`. |
| R13 | `NucleusCoopTool/UI/Core_Interface.cs` | The Stop button no longer resets the setup when `End` returned early because inputs are locked (the reset would empty the running handler's player list). |
| Log hook | `Coop/Generic/GenericGameHandler.cs` | `GenericGameHandler.LogHook` (static, null by default), called by `Log(string)`. |
| OSD event | `Forms/WPF_OSD.cs` | `WPF_OSD.MessageShown` (static, no subscriber by default), raised by `Show` for the main OSD only. |

## 6. Behaviour changes compared with the WinForms UI

- Every required custom step is shown (the WinForms UI showed `CustomSteps[0]` for each of them).
- The options step is shown only when an editable option exists (all-hidden or only unsupported options no longer show an empty or crashing page). Unsupported options (scalar values) keep the handler value.
- Choice values loaded from a profile are mapped back to the list items (numbers and enums work; cards are matched by `Name`, not by substring), and options added to the handler since the profile was saved get their defaults.
- A matched profile with custom steps goes to the first custom step without a card instead of jumping straight to the last step before devices are matched; AutoPlay doesn't launch when a card couldn't be restored.
- Requirements (OneDrive Documents, game location, administrator rights, Nucleus location) are blocking checks instead of message boxes when the game is selected; opening the setup always works.
- After every session end the setup starts again from a new `GameProfile` on the reloaded handler (devices detected again), keeping the option values. The WinForms UI reset only after the Stop button and needed the game to be selected again. A loaded profile is unloaded.
- `clearAll` keeps the option values when no profile is loaded (the profile "Unload" of the WinForms UI reset them).
- `autoFill` doesn't need API indexes, places only revealed devices, skips manual screens instead of stopping, and no longer skips every other device.
- `setLayout` with the current layout changes nothing.
- Keyboards and mice are revealed by input even when the setup screen isn't the current step (the WinForms UI revealed them only while painting).
- `setup.deviceLost` is reported on every step; the flow goes back to the players step only when it was further.
- **XInput pads identified by a press (2026-10-12, `GamepadIdentity`).** Without the slot mode, a press only revealed an XInput pad when exactly one DirectInput device saw it at the same moment. With DS4Windows/ViGEm, Steam Input, a stuck button or a D-pad press, the pad stayed unrevealed forever, so every placement was refused (`deviceNotRevealed`).
  - Matching now skips devices already identified as another pad, prefers the XInput views (`IG_`) when several devices are pressed, and counts the hat (D-pad).
  - Without a unique match, handlers that only need the XInput slot reveal the pad with the slot-mode GUID. The others keep it hidden: x360ce, device reordering or single device file, XInput reroute, ProtoInput or Nucleus DirectInput hook, or a script that reads `GamepadGuid`/`RawHID`/`HIDDeviceID`. For those, the press still flashes the pad, and the hint `padNotIdentified` shows for 10 s.
  - The disconnect check uses the OpenXinput controller that detected the pad, instead of the system `xinput1_3.dll`.
  - Handlers with both DirectInput and XInput no longer "dispose" the DirectInput view of XInput pads on every tick. That disposal triggered a full DirectInput refresh about once a second, which removed placed DirectInput pads from their slots.
  - `DInputManager.Refresh` stops the previous watcher thread and locks the device lists.
- The Stop button of the WinForms UI no longer resets while inputs are locked (R13); toggling API indexes no longer leaves stale assignments (R4); the drag hover no longer duplicates keyboard/mouse entries (R7).

## 7. Known limitations

- Hardware paths were not tested here: no gamepad was connected. XInput/DInput/SDL detection, button-press identification, rumble, guests and DirectInput hot-plug are the legacy code paths running on the UI thread; SDL on its helper thread is untested with a real SDL handler.
- The smoke test covered one monitor, raw keyboards/mice (virtual devices revealed by injected input) and saved profiles with a keyboard+mouse player. It did not launch a game: Play/Stop/End, instance stages, session actions and the launch log were not run end to end.
- Instance stages are approximate: Waiting (no process), Starting (process, window not placed), Ready (`ProcessData.Setted` or `Finished`), Exited (`Process.HasExited`). Running is detected with `GameProfile.Saved`.
- The session log levels and player numbers are guessed from the words of each line.
- `session.notice` needs `Globals.MainOSD` (created by the host): messages shown while it is null are not mirrored.
- `InitializeHandlerStartup` keeps resolving the handler by executable name (`GameManager.GetGame(ExePath)`), as before.
- `AssignToSlot` moves one device of a keyboard+mouse pair (the WinForms drag semantics); a swap moves whole players.
- `getSettings` takes about 200 ms on the UI thread (audio endpoints, network interfaces, display modes).
- Device ids of raw devices whose HID name can't be read are `rawmouse:`/`rawkb:` (made unique with `#2`).
