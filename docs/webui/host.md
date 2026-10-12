# Nucleus Co-op web UI: the host window

How `NucleusCoop.exe` hosts the React UI in a WebView2 window. Design context: [architecture.md](architecture.md), sections 1, 6 and 7. Code: `Master/NucleusCoopTool/Host/`.

## 1. Startup

`Program.Main` sets the working directory to the install folder (Settings.ini and logs are resolved from it), then `RunApplication` runs the existing startup checks unchanged (DPI, exFAT, settings, single instance, VC runtime, WebView2 runtime, install path, hub, file integrity, updater, logs). Then:

| Command line | Result |
|---|---|
| `NucleusCoop.exe` | Web UI (`HostApplication.Run`) |
| `NucleusCoop.exe <gameGuid>` | Web UI; the GUID is in `HostStartup.GameId` and in `app.getInfo().startupGameId` |
| `NucleusCoop.exe --legacy-ui [<gameGuid>]` | Same as without the switch. The WinForms `MainForm` it used to open was removed (2026-10-12); the switch is logged in `content\app.log` and ignored, so old shortcuts keep working |

`HostApplication.Run` (UI thread):

1. Switches the UI thread to per-monitor v2 DPI awareness (the process stays per-monitor v1 as set in `Program.cs`), so the window, the WebView and the native pickers follow monitor DPI changes.
2. Resolves `HostEnvironment` (folders, start URL, Debug options).
3. Creates `HostForm`, then `CompositionRoot` (bridge and services), then runs the message loop.
4. When the window is first shown: creates `Globals.MainOSD` (WPF OSD) and starts the gamepad shortcuts thread, which `MainForm` used to do. (The legacy gamepad navigation thread, which moved the mouse cursor with the left stick, was removed: the page navigates with gamepads itself, see `Nucleus.WebUI/src/shell/couch-nav.tsx`.)
5. When the window closes: quits with `Process.Kill`, like the legacy UI (the game handler and gamepad threads are foreground threads that never end). If a session is running, closing (title bar, Alt+F4, `window.close`, the kill hotkey's `WM_CLOSE` to `Globals.MainWindowHandle`) hides the window, runs `End(false)` on a worker thread, waits for `HasEnded` (90 s at most), then quits. The UI thread is never blocked.

## 2. The window (`HostForm`)

- Native frame behaviour without a native frame: the window keeps `WS_THICKFRAME` and the caption styles (so Aero Snap, Win+arrows, minimize/maximize animations, the system menu and resizing keep working) and `WM_NCCALCSIZE` removes the non-client area. A one-pixel `DwmExtendFrameIntoClientArea` keeps the DWM shadow; dark mode and rounded corners are requested from DWM (Windows 11).
- Bezel: the WebView fills the client area minus a 6 DIP black bezel (9 px at 150 %). `WM_NCHITTEST` turns the bezel into resize borders, with longer corner zones. When maximized, the top bezel row is a caption (drag down or double-click to restore) and the rest is inert.
- Maximized: the client area is clamped to the monitor work area, so nothing is off-screen and the taskbar stays visible. An auto-hide taskbar keeps a one-pixel strip so it can still be revealed.
- Dragging: the page marks its title bar with CSS `app-region: drag` (and its buttons `app-region: no-drag`). `CoreWebView2Settings.IsNonClientRegionSupportEnabled` makes WebView2 move the window, maximize on double-click and show the system menu on right-click.
- Minimum size 1100 x 680 DIP (clamped to the monitor work area). First run: 1360 x 840 DIP centred on the primary monitor.
- Placement: `[Misc] HostWindowPlacement=x,y,width,height,maximized` in Settings.ini (physical pixels, normal bounds). It is ignored when less than 160 x 80 px of it would be visible on the connected screens.
- DPI: `WM_DPICHANGED` applies the suggested rectangle and rescales the bezel; WebView2 follows the monitor scale by itself.
- No white flash: form, WebView control and `DefaultBackgroundColor` are black.
- Title: always "Nucleus Co-op" (other code uses `FindWindow(null, "Nucleus Co-op")`), so `window.setTitle` is accepted and ignored.
- `Globals.MainWindowHandle` is the host window handle.

WinForms detail: after a restore from maximized, WinForms re-applies its stashed bounds while handling `WM_WINDOWPOSCHANGED` and converts a client size to a window size with `AdjustWindowRectEx`, which would add the removed frame at every restore. `HostForm.SetBoundsCore` ignores calls made during `WM_WINDOWPOSCHANGED`; maximize/minimize from the bridge use `ShowWindow`.

## 3. WebView2

| | |
|---|---|
| SDK | Microsoft.Web.WebView2 1.0.4191.47 (Nucleus.Coop packages.config and Nucleus.Bridge PackageReference use the same version) |
| User data folder | `webview\ui` (the removed legacy hub view used `webview\cache` with browser extensions, and older installs may still have it; two environments with different options cannot share a folder) |
| UI | `https://ui.nucleus.example/` → `webui\` (access: same origin only) |
| Assets | `https://assets.nucleus.example/` → `gui\` (covers, screenshots, icons; CORS allowed so canvas can read pixels) |
| Handler images | `https://handlers.nucleus.example/` → `handlers\` (images in `<img>`; CORS reads denied) |
| Start page | `https://ui.nucleus.example/index.html` |

Navigation: the WebView only shows the UI origin (plus the dev server origin in Debug). Any other navigation is cancelled; when the user started it (link click, `target=_blank`, `window.open`), an http(s) URL opens in the default browser. Other schemes (`file:`, `mailto:`, `steam:`…) are blocked and logged. Permissions are denied except clipboard read for the UI. Host objects are disabled; the page talks to the host only through the bridge (`chrome.webview.postMessage`).

Failures:
- Runtime missing: a native message offers the download page, then the app closes (`Program.cs` already offers to install it at startup).
- Initialization error (for example the folder is locked by another process with other options): Retry / Cancel.
- `ProcessFailed`: renderer crash → message, then reload; renderer unresponsive → "Reload now?"; browser process exit → message, then the WebView is recreated (new environment and CoreWebView2, the bridge transport is re-attached). More than 3 crashes in a minute → message and quit. GPU/utility process failures are only logged.

## 4. Bridge wiring (`CompositionRoot`)

- `BridgeHost(uiSynchronizationContext, log)`: bridge log lines go to `content\app.log` (Debug-level lines only when `[Misc] DebugLog=True`).
- Host services (`Host/Services/`): `app` (`AppService`), `window` (`WindowService`, emits `window.stateChanged` on maximize, restore, minimize, focus and DPI changes), `dialogs` (`DialogsService`: `OpenFileDialog` and the Vista `IFileOpenDialog` folder picker, modal to the window, posted out of the WebView2 event handler, null on cancel).
- Extension point: `CompositionRoot.RegisterAppServices` calls `NucleusServices.Register(bridge, shell)` (Master/Nucleus.Coop.Services). `shell` is `HostShell : IHostShell` (window handle, UI synchronization context, minimize, restore).
- A `WebView2Transport` is attached to each CoreWebView2 before its first navigation, with the start page origin as the only allowed origin, and disposed before the CoreWebView2 goes away.

## 5. Build and run

Prerequisites: Visual Studio 2022 Build Tools (MSBuild 17, .NET Framework 4.7.2 targeting pack), .NET SDK (for the SDK-style Nucleus.Bridge and Nucleus.Coop.Services projects), the WebView2 runtime. The C++ projects and the ProtoInput submodule are not needed to build the C# app, but they are needed to launch games (see [Building native components](#building-native-components)).

```powershell
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
cd Master
& $msbuild NucleusCoopTool\Nucleus.Coop.csproj -restore -p:RestorePackagesConfig=true -p:Configuration=Debug -p:Platform=AnyCPU -p:SolutionDir="$PWD\"
```

The web UI is built separately (`npm run build` in `Master/Nucleus.WebUI`). After each C# build, the `CopyWebUi` target copies `Nucleus.WebUI\dist\` to `bin\<Configuration>\webui\` (old files removed, `dev-server.txt` kept). Without `dist`, `WebUiPlaceholder` writes `webui\index.html` from `Host\WebUiPlaceholder.html`: a page with a draggable title bar, window buttons and an `app.hello` check, so the window never shows an error page and the bridge can be tested without the React app.

Run `bin\Debug\NucleusCoop.exe` (any working directory; `Main` switches to the exe folder).

### Output folder layout

```
NucleusCoop.exe, NucleusCoop.exe.config     probing privatePath="bin;webview"
bin\                                         managed assemblies: Nucleus.Gaming, Nucleus.Bridge, Nucleus.Coop.Services, WindowScrape,
                                             Jint, Newtonsoft.Json, NAudio, SharpDX x3, Ionic.Zip.Reduced, EasyHook.dll (+ .pdb)
StartGame.exe(.config), Nucleus.IJx86/x64.exe(.config)   helper exes; their .config probes bin\ (privatePath="bin")
EasyHook32/64.dll, Nucleus.Hook*/SHook*.dll, ProtoInput*, SDL2.dll, openxinput1_3.dll   natives, at the root (in-process DllImport
                                             by bare name, or injected from the helpers' folder)
webview\Microsoft.Web.WebView2.{Core,WinForms}.dll        1.0.4191.47, from copyoutput\webview
webview\runtimes\win-{x86,x64,arm64}\native\WebView2Loader.dll  found by WebView2.Core relative to its own folder
webview\ui\                                  web UI browser profile (created at first run)
webui\                                       built UI (or the placeholder), optional dev-server.txt
docs\                                        readme.txt, readme Proto Input.txt (handler-author docs)
gui\, handlers\, utils\, docs\, Settings.ini from copyoutput (post-build xcopy, which also resets Settings.ini at each build)
content\app.log                              app log, host lines are tagged [WebUI]
```

The `MoveManagedReferencesToBin` target of `Nucleus.Coop.csproj` sets `DestinationSubDirectory=bin\` on every copy-local reference except `StartGame.*`. `AllowedReferenceRelatedFileExtensions` leaves out the library `*.xml` docs. The install root is `Nucleus.Gaming.InstallPaths.Root`: the folder of `NucleusCoop.exe` in every process, including StartGame and the injectors, and including when one of them sits in `bin\`. `InstallPaths.BinDir` is `<root>\bin`. `Globals.NucleusInstallRoot`, `NucleusPaths.InstallRoot` and `HostEnvironment` use it.

`StartChecks.CheckFilesIntegrity` kills the app when one of its files is missing. The list holds paths relative to the install root, with `bin\` for managed assemblies, and includes the helper `.exe.config` files. Native files are the usual cause: `Nucleus.Hook32/64.dll`, `Nucleus.SHook32/64.dll` (C++ projects), `Nucleus.IJx86.exe`/`Nucleus.IJx64.exe` (Nucleus.Inject, built through the solution as x86/x64) and the `ProtoInput*` binaries (ProtoInput submodule), or the same files from a release. The check only tests that the files exist, so empty files with those names let the UI start, but a game launch then fails. Build the real ones as described below.

### Building native components

Extra prerequisites: git, and the C++ part of the Build Tools (MSVC v143, a Windows 10/11 SDK). x360ce is not built: its binaries are in `copyoutput\utils\x360ce`, so the `Submodules/x360ce` submodule is not needed.

From the repository root (PowerShell), for Debug and Release:

```powershell
powershell -ExecutionPolicy Bypass -File Master\tools\build-native.ps1
# -Configuration Release: one configuration only. -NoReferenceBuild: do not rebuild Nucleus.Gaming (use the one from the C# build).
```

Without `-NoReferenceBuild`, Nucleus.Inject rebuilds Nucleus.Gaming for the solution platforms x86/x64, where WindowScrape is not built (CS0246 `WindowScrape`). Build the solution first, then run the script with `-NoReferenceBuild`.

The `Submodules/ProtoInput` gitlink must follow what `NucleusGaming\Coop\ProtoInput\ProtoInput.cs` imports. Upstream pinned a 2021 commit (`dd29bf8`) while the C# calls functions added in 2026: launching a ProtoInput handler then failed with `EntryPointNotFoundException` (`SetTranslateXinputtoMKB`). It now points to ProtoInput `master` `e18e06b` (2026-07-08), and step 7 of the script fails the build if an imported function is missing.

The script runs the commands below, then checks every file of `CheckFilesIntegrity` in `bin\<Configuration>`: present, not empty, and PE machine x86/x64 matching the `32`/`x86` or `64`/`x64` suffix.

```powershell
$msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
git -c core.longpaths=true submodule update --init Submodules/ProtoInput

# ProtoInput, Release x86 then x64 -> Submodules\ProtoInput\src\ProtoInput\Release\ (11 .dll/.exe files)
foreach ($p in 'x86', 'x64') {
  & $msbuild Submodules\ProtoInput\src\ProtoInput\ProtoInput.sln "-t:Xinput\DEVOBJ;Build" -p:Configuration=Release -p:Platform=$p `
    -p:PlatformToolset=v143 -p:WindowsTargetPlatformVersion=10.0 -p:BuildProjectReferences=false `
    "-p:ForceImportBeforeCppTargets=$PWD\Master\tools\ProtoInput.v143.props"
}

# Nucleus.Hook, Nucleus.SHook, Nucleus.Inject: their post-build steps copy to Master\NucleusCoopTool\bin\<Configuration>
# (Nucleus.Inject: Nucleus.IJx86/x64.exe and their .exe.config, which probes bin\)
foreach ($c in 'Debug', 'Release') { foreach ($p in 'x86', 'x64') {
  & $msbuild Master\NucleusCoop.sln "-t:Nucleus_Hook;Nucleus_SHook;Nucleus_Inject" -p:Configuration=$c -p:Platform=$p
} }

# ProtoInput -> bin (the Nucleus.Coop post-build copies the same files at every C# build once they exist).
# ProtoInputHost.exe (the standalone ProtoInput GUI) is not copied: nothing launches it.
foreach ($c in 'Debug', 'Release') {
  Copy-Item Submodules\ProtoInput\src\ProtoInput\Release\*.dll, Submodules\ProtoInput\src\ProtoInput\Release\ProtoInputIJ*.exe Master\NucleusCoopTool\bin\$c
}
```

| Detail | Reason |
|---|---|
| Solution platforms x86 and x64 | `Any CPU` (and a `Nucleus.Coop.csproj` build) only builds Nucleus.Hook/SHook as Win32 and Nucleus.Inject as x86. The 64-bit hooks and `Nucleus.IJx64.exe` need `Platform=x64`. |
| `PlatformToolset=v143`, `WindowsTargetPlatformVersion=10.0` | ProtoInput targets v142 (VS 2019) and SDK 10.0.18362, which VS 2022 does not install. |
| `Master\tools\ProtoInput.v143.props` | Avoids editing the submodule: adds `/permissive` (v143 rejects missing `typename` in BlackBone and ProtoInputHooks), defines `_NTDDI_WIN10` (easyhook.h sets `NTDDI_VERSION` to this undefined macro, which breaks `fileapi.h` in recent SDKs), and adds the user-level ATL folder when it exists. |
| `BuildProjectReferences=false` | EasyHookDll has build-order references to EasyHook.csproj and EasyHookSvc.csproj, which are not in the solution and have no `Debug\|AnyCPU` configuration. Visual Studio skips them too. |
| `-t:Xinput\DEVOBJ;Build` | OpenXinput links `DEVOBJ.lib`, but the solution does not declare the dependency. |
| ATL | BlackBone (ProtoInput) needs ATL (`atlbase.h`, `atls.lib`). Either install the VS component (admin): `& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\setup.exe" modify --installPath "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools" --add Microsoft.VisualStudio.Component.VC.ATL --passive`, or let the script download the same packages (`Microsoft.VC.<msvc>.ATL.{Headers,X86,X64}.base` from the VS 2022 catalog, SHA-256 checked) to `%LOCALAPPDATA%\NucleusBuildDeps\atl\<MSVC version>\` without admin rights. |
| Runtime | The hooks link the CRT statically. `ProtoInputUtilDynamic32/64.dll` need the VC++ 2015-2022 redistributable (x86 and x64). |

Build output that stays out of git: `Master\x86\`, `Master\x64\`, `Nucleus.Inject\bin\`, and the build folders inside the submodule (ignored by its `.gitignore`).

## 6. Debug vs Release

| | Debug | Release |
|---|---|---|
| Dev tools (F12, Ctrl+Shift+I), context menu, browser accelerators (F5, Ctrl+R, Ctrl+F…) | on | off |
| `webui\dev-server.txt` | used | ignored |
| `NUCLEUS_WEBVIEW_DEBUG_PORT` | used | ignored |
| `app.hello().debug` | true | false |
| Zoom, pinch zoom, swipe navigation, autofill, password save, status bar | off | off |

### Dev server (hot reload)

1. `npm run dev` in `Master/Nucleus.WebUI` (Vite prints its URL, for example `http://localhost:5173/`).
2. Put that URL on the first line of `bin\Debug\webui\dev-server.txt`.
3. Start the Debug build. The WebView loads the dev server, and the bridge accepts messages from that origin only. Delete the file to go back to the built UI.

### Remote debugging

```powershell
$env:NUCLEUS_WEBVIEW_DEBUG_PORT = "9333"
Start-Process .\bin\Debug\NucleusCoop.exe
# then http://127.0.0.1:9333/json, edge://inspect, or Playwright:
#   const browser = await chromium.connectOverCDP('http://127.0.0.1:9333');
#   const page = browser.contexts()[0].pages()[0];
```

The port is passed as `--remote-debugging-port` through `CoreWebView2EnvironmentOptions.AdditionalBrowserArguments`. Browser arguments are part of the environment identity: do not run two Debug instances with different ports at the same time.
