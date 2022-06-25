# CHANGELOG / RELEASE NOTES

## v2.1.1 - May 24, 2022

1. Added Steamless command line version support: "Game.UseSteamless = true;", "Game.SteamlessArgs = "";", "Game.SteamlessTiming = 2500;".
1. Fixed nicknames not working when using "Game.GoldbergExperimentalSteamClient = true;".
1. Fixed Player Steam IDs setting to 0 when using "Game.PlayerSteamIDs = [];".
1. Added game descriptions, they get downloaded to gui\descriptions.
1. Added new line Context.EditRegKeyNoBackup, will not create a backup of the registry when editing.
1. Fixed an unknown bug breaking the Nucleus window shape in some cases (maximizing without using the app maximizing button).
1. Fixed changing the default text editor in Settings.ini not working.
1. Added blur to background images, the blur can be disabled by setting Blur = 0 in Settings.ini.
1. Other minor UI improvements and changes.

## v2.1 - May 5, 2022

1. Added Context.HandlersFolder (path to the root of Nucleus Co-op handlers folder: NucleusCo-op\handlers).
1. Fixed app crash when a handler throws an error (sometimes on app close).
1. Fixed random crashes while clicking on the game list.
1. All monitors in use should be correctly scaled to 100% when a game starts now.
1. Added UI option to enable/disable the auto setting of the desktop scale to 100% (enabled by default).
1. All UI elements (pictures) can be customized now (see the default theme folder).
1. Splashscreen fixes, you can skip it now by clicking on it if it shows for too long.
1. Added UI options in settings to disable/enable the splashscreen and click sound in the settings tab and moved the "mouse click" setting to the setting.ini instead of theme.ini.
1. Other UI related details.
1. Some UI code optimizations.
1. New and improved Nucleus Co-op themes.
1. Added theme switch option in settings.
1. Links can be clicked in handler notes now.
1. Added option to use custom text in Context.RunAdditionalFiles prompt(s) + a boolean to show or not the file path. See readme.txt.
1. New Documents path registry key backup/restoration handling, should fully fix Nucleus changing the location of the Documents folder sometimes after a crash.
1. Added custom virtual devices icons.
1. First attempt to fix Turkish "Ä±" bug, requires to be tested in real conditions.
1. Fixed account_name.txt being edited while UseGoldberg is not used.
1. Added new input device detection in setup screen, keyboards and mice icons will only show in the UI if a key is pressed or a mouse moved now.
1. Added an option in theme.ini to round the Nucleus Co-op window corners (enabled by default).
1. Added multiple Nucleus Co-op instances check (can be disabled in settings.ini).
1. Added the possibility to choose the app font in theme.ini (size can be adjusted).
1. Fixed a crash that occurred when custom icon pictures were deleted.
1. Added new "icons" folder inside the Nucleus Co-op "gui" folder, custom icon paths are now saved in the "icons.ini" inside that folder instead of being saved in settings.ini.
1. Fixed crash that occurred when an user had a custom Documents folder in the root of his drive and clicked game options in the UI.
1. Fixed "Game.SymlinkFiles = [""];" and updated so that it can work under "Game.Play = function() {" using "Context.ProceedSymlink();".
1. Help gif updated.
1. Fixed Nucleus Co-op reporting the incorrect line number when a handler has an error, can still show the number with an offset of +1 if the line number returned is a float.
1. Fixed a Nucleus Co-op silent crash that happened when controllers got disconnected and reconnected multiple times.
1. Added Game.SetTopMostAtEnd = true; Sets the game windows to top most at the very end.
1. Added .ini option to hide the Nucleus Co-op offline icon.
1. Added handler notes magnifier option.
1. Added new supported inputs UI icons, display what input devices a handler supports.
1. Added Player Steam IDs fields to the Nucleus Nicknames settings tab (now named Players), you can change the instances Player Steam IDs when a handler uses goldberg or SSE via the UI now.
1. Added new Nicknames/Player Steam IDs switcher, you can quickly switch the order of the nicknames and Player Steam IDs you set up.
1. Fixed minor UI glitch.
1. Last hooks prompt will show now when only using `Game.PromptBetweenInstances=true;` with `Game.SetTopMostAtEnd = true;`
1. Added option in Settings.ini to change the default text editor.
1. Selection not working and scaling issues fixed for Nucleus UI options that use images.

## v2.0 - February 25, 2022

1. New overhauled and customizable user interface with support for themes, game covers and screenshots.
1. Fixed ui scaling issues at more than 100% desktop scale (and all other issues/bugs related to it).
1. Fixed multi-monitor vertical setup drawing to not overlap input device list.
1. Quality of life improvements such as but not limited to: new discord invite link, Nucleus Co-op github release link and much more.
1. Added Nermintingas Galaxy emulator support.
1. SplitCalculator(Josivan88) integration.
1. Renamed scripts to handlers.
1. Added new handler callbacks.
1. New player nickname assignation.
1. New player and input order processing.
1. New optional splitscreen division(visualy similar to a native splitscreen game).

## v1.1.3 - September 28, 2021

1. Fixed only 4 Xinput controllers showing
1. Fixed controller index being inconsistent with the Nucleus UI
1. Fixed duplicate controller icons (and similar bugs)
1. Fixed Ctrl+T working unreliably when Proto Input was injected
1. Removed xinput1_4 dependency to fix crashing on Windows 7
1. Added more script callbacks

## v1.1.2 - September 1, 2021

1. Fixed fake cursor not showing in some games

## v1.1.1 - August 30, 2021

1. Added script updater
1. Fixed the Proto Input controller hooks
1. Fixed incorrect controller icons being displayed
1. Fixed the fake cursor not hiding when it should in some games

## v1.1.0 - August 18, 2021

1. Integrated Proto Input (github.com/ilyaki/protoinput) hooks
1. Greatly improved keyboard/mouse input
1. Complete rewrite of all hooks, plus new injection method support
1. Fixed most bugs relating to keyboard/mouse input, including CursorVisibilityHook crashing
1. New in-game GUI to change hooks and input settings while the game is running
1. Improved fake focus, including a message filter system for all windows messages
1. Smarter scripting so you can use a combination of keyboards/mice and controllers smoothly
1. Custom mouse cursor support
1. Added support for more than 4 Xinput controllers with OpenXinput
1. Added DirectInput controller redirection
1. Rewrite of the input locking system - no more laggy cursor, and Windows UI elements won't randomly open
1. A bunch of misc changes and bug fixes

## v1.0.2 R5 FINAL - January 2, 2020

1. Fixed bug that would cause the incorrect document path to be used for subsequent players when using Nucleus environment and start up hooks
1. Document path in registry will now only be changed if it needs to (only if playing a game that uses Documents for game files)
1. Some fixes for device layout screen
1. Updated Goldberg emulator to latest git build

## v1.0.1 R5 FINAL - December 30, 2020

1. Fixed app not opening for some users
1. Fixed bug with expanding single keyboard vertically
1. Added NucluesUserRoot to context (get userprofile paths for a player's respective nucleusplayer Windows account)
1. Other minor bug fixes/tweaks

## v1.0 R5 FINAL - December 24, 2020

1. Audio routing per instance; specify each game to run through different audio sources
1. Added support for Nemirtingas Epic Emulator
1. Added support for games that use Documents for game files (and support for users with custom document folders)
1. Nucleus user accounts on windows can now retain save data between sessions (including Halo MCC, your game saves will remain intact)
1. Nucleus on close will now remove files it created when using original game folders (keeping your original game folder untouched)
1. Registry edits done by Nucleus now only persist the game session (Nucleus backs up and restores, as to note touch your original game settings)
1. Added alot of new scripting features that greatly increases game compatibility list
1. New function, custom prompts. Scripts can prompt users for input that can be used in scripts for logic
1. New xinput hooks (alot more effective way of hooking and handling xinput messages) (Thanks to @Ilyaki)
1. Fixes for custom layouts and stretching KMB inputs on layouts
1. Fixes for status window, performs alot better (but still every once and awhile may not work)
1. Updated alpha 10 custom xinput dlls to enable mouse and keyboard input for instances
1. XinputPlus assign gamepad based on player's gamepad index NOT player's index
1. Added support for wizark952's dinput8 blocker
1. Added some support for Goldberg's steam client loader
1. Added options to set window styles at very end, ignore window border check, and option to enable windows at end
1. Added function Game.IgnoreThirdPartyPrompt; when launching via third party, ignore the prompt to press ok when game is launched
1. Changes to LaunchAsDifferentUsers - added option to keep user accounts and ability to transfer user account data (if not keeping)
1. Made some back-end changes to Nucleus environment
1. Revamped settings window, tabs are now used to seperate different sub-settings
1. Changes to Context values, functions and bug fixes
1. Updated Goldberg emulator to latest git build
1. Added prelimary and experimental function to write to process memory; Game.WriteToProcessMemory
1. LOTS of miscellaneous changes/bug-fixes

## v0.9.9.9 r4 - April 12, 2020

1. Improved script downloader. Handlers are cached and viewable up to a specified # of results at a time (via "pages"). User can also pre-sort and use drop-down as an alternative way to sort when searching
1. Replaced LaunchAsDifferentUsers with a new and improved method, which will now also utilize current user's Nucleus environment. The previous method has been retained as LaunchAsDifferentUsersAlt
1. Added an optional status window to appear when launching and closing Nucleus, to show what Nucleus is doing
1. Added an alternative method of changing IP per instance. Alt method will create temporary loopback adapters for each player (however, it will not change any metric)
1. Added the ability to extract existing script handlers (.nc files)
1. Added a new option in game scripts to call Game.Play after the game instance has launched
1. Added a new option in game scripts to not copy files from UserProfile\<Config\>\<Save\>Path when using Nucleus environment
1. Added a new option in game scripts to ignore checking if launcher exe exists in game folder
1. Added a new Context function, ReadRegKey. Will return a string value for a specified key
1. Added new Context function, RunAdditionalFiles. User can specify files that need to be run before launcher/game
1. Added functions to Context so you can now get a list of all found files and their paths that match a given pattern. Can also change a creation date and modified date of a file
1. Added an option for DirSymlinkCopyInstead to copy all subfolders and files in addition
1. Added new values for Player, SID, Adapter and UserProfile (for use with some of the new functions in this update)
1. Improved editing and deleting of registry keys, in addition keys in HKEY_USERS are now accessible
1. Changed prompts from message boxes to Forms (more control, shold now be on top most of the time)
1. Migrated from SlimDX to SharpDX for reading controllers
1. Updated rename mutexes to work with "Type:|" prefixes introduced in a recent update
1. Updated external libraries to latest versions (Jint, DotNetZip, Newtonsoft Json)
1. Potential fix for bug that was eating CPU resources when using Direct Input/Xinput Reroute
1. Fixed XInputPlus bug that would only recognize dinput dll if it was all lowercase
1. Fixed bugs when using DrawFakeMouseCursor, added option DrawFakeMouseForControllers [thanks to @Ilyaki]
1. Fixed some bugs in script downloader
1. Fixed backed up registry table not being restored
1. Fixed DirSymlinkCopyInstead not copying folder itself
1. Fixed ChangeExe not working when KeepSymLinkOnExit is being used
1. Fixed Nucleus post hook dlls being injected too early when using ProcessChangesAtEnd
1. Number of other bug fixes
1. Minor tweaks/changes

## v0.9.9.9 r3 - March 26, 2020

1. Fixes and improvements to Game.LaunchAsDifferentUsers
1. Fixed error message on the controller layout screen when using dinput / xinput reroute
1. Fixed context aspect ratio decimals

## v0.9.9.9 r2 - March 25, 2020

1. Can now view all public scripts in Script Downloader and sort columns by ascending/descending order
1. Added an option in game scripts to launch each game instance as a different user (Nucleus will create temporary accounts "nucluesplayerx" and then delete them at the very end) [thanks to @napserious for his base code]
1. Added an option in game scripts to run Game.ExecutableName in addition to Game.LauncherExe (if used)
1. Added an option in game scripts to specify an amount of time to wait after lauching game but before grabbing the game's process
1. Added an option in game scripts to specify if starting arguments should be inside the executable path when using Game.CMDLaunch
1. Added an option in game scripts/utility to use a EAC bypass dll
1. Added an option in game scripts to kill mutexes at the end, when using Game.ProcessChangsAtEnd
1. Added an option in game scripts to add gamepad cursors [thanks to @Ilyaki]
1. Added additional logic to SetWindowHook and SetWindowHook hooks, now they should work better
1. New context options available to get monitor or a specific player window's height/width/aspectratio
1. Game.CreateSingleDeviceFile will now also hook CreateFileA for ANSI calls
1. Whenever registry keys are being edited or deleted by a script, Nucleus will now backup the current registry keys and restore them upon exit
1. When hard copying game files, you can now specify exclusions
1. When hard copying game files, Nucleus will wait for each instance to hard copy before continuing
1. Updated Goldberg emulator to latest git build (sha 5c41ba020c4ffc46d0adbeb3b82c9ae623d14ef2)
1. Fixed not all lines using right encoding when specified, when modifying lines to files (replacelines, removelines, etc)
1. Fixed Script Downloader not working for Window 7 users
1. Fix for raw input filter / reregister raw input not working [thanks to @Ilyaki]
1. Fixed some objects weren't being properly disposed

## v0.9.9.9 f1 - March 15, 2020

1. Added an option in game scripts to create only one file for HID devices per instance (the assigned HID device). This is a workaround for Unity games that use default input
1. Added an option in game scripts to enable the minimize, and restore of game window at the end (now off by default, only few games are known to need it atm)
1. Device handle zero support [thanks to @Ilyaki]
1. Added a delay during start up hooks for better performance (would cause issues on lower end PCs), and fixed hang when it failed its 5 attempts
1. Fix ResetWindows not resizing or repositioning if DontResize or DontReposition is on
1. Only copy from AppData folder to Nucleus Environment if Nucleus Environment is being used
1. Re-enabled debug log header
1. Minor tweaks/bug fixes

## v0.9.9.9 - March 7, 2020

1. Nucleus now supports multiple Mice and Keyboards [thanks to @Ilyaki]
1. Added the ability to search for scripts (handlers) and download them directly from the UI [thanks to @r-mach]
1. You can now edit scripts while Nucleus is open and changes will take effect (no need to restart Nucleus each time anymore)
1. Added a new option in game scripts to do resizing, repositioning and post-launch hooking of ALL instances at the very end (after all have been opened)
1. Note: This method will not work for every option to-date
1. Added an option in game scripts to re-register raw input in game process, a replacement for forwarding raw input [thanks to @Ilyaki]
1. Added a new utility, Flawless Widescreen. Can be setup by calling it in game script
1. Added an option in game scripts to change your IP per instance (NOTE: Highly experimental), a drop-down has been added to settings to select which network to change IP for
1. Added logic to delete certain files that Nucleus adds to original game folder (when not linking or copying). For example, some that get deleted: Nucleus custom dlls, x360ce, xinput plus, custom utils
1. Added an option in game scripts to kill mutexes in launchers
1. Added an option in game scripts to kill mutexes in the last instance (normally last is ignored)
1. Added an option in game scripts to rename, move or delete files in instance folders
1. Added an option in game scripts to prompt the user after launching each instance, but before the grab process is grabbed
1. Added an option in game scripts to delete file(s) from user profile config or save path, a prompt asking you to confirm will typically show, but this can be turned off in script too
1. Added an option in game scripts to rename the steam api for Goldberg experimental (default will now not rename, you must set it to do so)
1. Added an option in game scripts to kill additional processes upon exiting Nucleus or stopping a session
1. Added an option in game scripts to ignore PreventWindowDeactivation if player is using keyboard
1. Added an option in game scripts to copy (not symlink) all files within a given folder
1. Added options in game scripts to disable Nucleus resizing, repositioning or making windows top most
1. Added an option in game scripts to specify custom window styles (extended window styles as well)
1. Increased max nickname length to 9 characters
1. Exposed IsKeyboardPlayer to Context (will be True or False), can now be called in Game.Play
1. Launchers will now also be killed upon exiting Nucleus or stopping a session (if any remain open throughout session), in addition to game windows
1. Added logic to KeepAspectRatio & KeepMonitorAspectRatio if new width is to be determined (previously only new height)
1. Added logic so that mutexes of different kinds can be killed. In Game.KillMutex(Launcher), simply begin the string with "Type:Mutant|" following by the name of the mutex to kill. Can replace mutant with any mutex type
1. Added logic to check if borders are removed at very end (some games bring them back), and if they aren't, remove them again
1. Added Steam language in UI settings, and language gets updated automatically for SSE now as well
1. Updated logic when launching games with start up hooks. Will check if process for that instance is already running, as well as try to grab the correct process if it is detected to be wrong
1. Updated logic for Goldberg to set the settings folder in Nucleus Environment if environment and no local save are enabled
1. Updated Goldberg emulator to latest git build (sha a0b66407bf2b8da686a708802cbc412f9cd386ca)
1. Updated Context.LocalIP to better identify user's local IP when there are multiple IPs
1. Updated method to capture current environment's user profile if different than their username [thanks to @Ilyaki]
1. Fixed bug with x360ce placing files in wrong directory
1. Fixed some duplicate operations happening if game was not linked or copied
1. Fixed instanced folders trying to be deleted if game was not linked or copied
1. Fixed a bug with PreventWindowDeactivation causing input to not work for some users
1. Fixed some bugs with HexEditFileAddress and HexEditExeAddress
1. Fixed architecture displayed in UI script details

## v0.9.9.1 - February 5, 2020

1. Updated logic for LauncherExe. The file name in this field will now be launched via Nucleus but ExecutableName will be used to resize, reposition and hook. Launchers will no longer be looked for when grabbing process to manipulate. LauncherExe will be used for hex editing exes and change exes.
1. LauncherExe can now also accept a full path to the launcher exe. This is if the launcher is outside of the game folder; NOTE: This file/folder contents will NOT be symlinked/copied, only the game root folder of ExecutableName. This means that hex edits and changing exe using a full path in LauncherExe WILL overwrite the original file!
1. Added an option in game scripts to specify which instances get starting hooks, post-launch hooks and fake focus messages sent to (including a specific option for keyboard player)
1. Added an option in game scripts to provide a relative config and save path from user profile for games (for use with Nucleus environment)
1. Added an option in context menu/game options in UI to open or delete relative config and save paths (for system user profile and Nucleus)
1. Added logic if the game is running in Nucleus environment and a user profile config and/or save path have been provided, files and folders will be copied over from the system user profile path (+ added an option to force it)
1. Added an option in game scripts to manually enter steam IDs to be used
1. Added an option in game scripts for games to try and keep monitor aspect ratio when being resized
1. Added an option in game scripts to replace bytes in the exe or file at a specific offset
1. Added new field in Context that can be used to grab the local IP address of the computer
1. Added new fields in Context for Nucleus environment paths and relative game config/save paths
1. If using Nucleus environment and goldberg, set the goldberg settings folder to the Nucleus appdata path
1. Updated Goldberg emulator to latest git build (sha b4205535fbee455bee925ab3aa90780e00eead27)
1. Hopefully, final fixes for aspect ratios being preserved via KeepAspectRatio and the new KeepMonitorAspectRatio options
1. Fixed some issues with x360ce
1. Fixed a bug that would cause infinite waiting if process data had already been assigned by the time it got checked
1. Fixes for CMDBatchBefore/After (CMDOptions no longer required and will only use if there is a line for that instance, CMDBatchAfter now working)
1. Fixed a bug that caused Nucleus to crash when using x360ce
1. Fixed SmartSteamEmu not grabbing game process to resize, reposition and hook. SSE launches can now also use process picker
1. Fixed bug preventing nickname from reverting back to default after removing a nickname
1. Fixed bug that would sometimes cause an error and weird nicknames in UI, when disconnecting, reconnecting controllers
1. Made some changes/improvements to context/game options menu and logging
1. Game.HookInitDelay and Game.HookFocusInstances have been removed

## v0.9.9 - January 22, 2020

1. Added an option in game scripts to use Goldberg Lobby Connect (automatic)
1. Thanks to Ilyaki, added an option in game scripts to use a custom environment (located at C:\Users\<your username>\NucleusCoop)
1. Added an option in game scripts to specify if the game is launched outside of Nucleus (Nucleus will then not launch it, but continue to do everything else, hook, resize, repos etc)
1. Added a last fail-safe if process is still not found; a new window will open asking user to manually select the process
1. Added an option in game scripts to manually pick the process to be manipulated for things such as resizing, repositioning and used for post-launch hooks
1. Added an option in game scripts to run other commands in command prompt before or after game launches when using CMDLaunch (or UseForceBindIP), also set some environment variables that can be used in commands
1. Added DInput support for XInputPlus
1. Added an option in game scripts to have Goldberg not create a local_save.txt file (use default save location)
1. Added an option to specify a different x360ce dll name be used, if needed
1. Added the ability to copy entire folders (and all its contents) in CopyCustomUtils
1. Added an option in context menu/game options in UI to delete content folder
1. Added an option in game scripts to completely ignore linking or copying a folder and all its contents
1. Updated Goldberg emulator to latest git build (sha 3f44827326eff6d9cc385c27f0bded89ee7642ea)
1. Replaced a function (GetDpiForWindow - used for GUI) that required Windows 10, with an alternative method, so older versions of Windows are now better supported
1. Made auto goldberg case insensitive when replacing steam api dlls
1. Fixed bug that would stop script notes from appearing in UI after stopping a session
1. Fixed bug when KeepSymLinkOnExit = true and # of players increase when there are more than 4 players
1. Fixed LauncherExe being required in script if process was not found, or if script uses CMDLaunch or UseForceBindIP
1. Fixed "Process is not running" crashes when using start up hooks
1. Fixed KeepAspectRatio not resizing correctly, windows will now be horizontally centered as well (to player bounds)
1. Fixed crash when SymlinkGame is false

## v0.9.8.2 - December 8, 2019

1. Hook code has been cleaned up and some lingering issues with Easyhook in the past have been resolved *Thanks to @Ilyaki
1. Completely reworked Autosearch. Fixed bug requiring admin rights, custom paths are now allowed, user can choose which found games to add, and so much more
1. Instance folder will no longer be symlinked if SymlinkFolders is true
1. Integrated and added an option in game scripts to use DirectX 9, Direct 3D wrapper (d3d9.dll) to try and run DirectX 9 games in windowed mode
1. Added an option in game scripts to force steam stub DRM patcher to either use x64 or x86 architecture dll
1. Added an option in game scripts to force symlink every time
1. Added an option in game scripts to set processor affinity per instance
1. Changed logic of setting processor affinity
1. Nicknames assigned in Nucleus are no longer exclusive to Goldberg
1. The keyboard player can now be assigned a nickname in Settings
1. Fixed large files sometimes breaking the symlink process
1. Fixed files not being symlinked with different amount of players under the new rule: files will only be copied or symlinked once, if needed
1. Fixed Nicknames not always being updated if changed
1. Fixed the black window from Hide Desktop not closing when stopping a game session (must press stop or stop session hotkey)
1. Added an option in game scripts to set processor affinity per instance
1. Changed logic of setting processor affinity

## v0.9.8.1 - November 4, 2019

1. Reverted folder and file exclusion logic to the way it was done pre-0.9.8 (but still kept improvements made to them)
1. DirSymlinkExclusion will force hardcopy of the folder (if it is to be symlinked), FileSymlinkExclusion will completely ignore the file (no link/copy), FileSymlinkCopyInstead will continue to just create hardcopy of file instead of symlinking it
1. Fixed xinput plus controller mappings when keyboard player was any player except last
1. Prompt between instances can now be delayed if PauseBetweenStarts has a value

## v0.9.8 - November 1, 2019

1. Nucleus no longer starts with administrative privileges and will prompt if it is needed, games will not be launched elevated now either
1. Improved and changed logic on how files are copied/symlinked, much faster now and done all it once at the beginning
  !WARNING!: DirSymlinkExclusions did not work properly in original Nucleus Alpha 8 and now does. All files and subfolders of a DirSymlinkExclusion will be ignored no matter what the file is. Check your scripts!
1. Files that get added to the instance folders (goldberg, xinput plus, etc) will no longer be copied over to the original game path and files (as long as SymlinkFolders is false)
1. If SymlinkFolders is true, files WILL be copied over to the original file path
1. Files will only be copied or symlinked once, if needed; i.e. if Instance0 path exists with at least one file & KeepSymLinkOnExit is true, Nucleus will skip copying/linking
1. Added an option in game scripts to prevent window deactivation
1. Added an option in game scripts to hardlink files instead of symlink
1. Added an option in game scripts to symlink folders (needed for some games)
1. Added option in game scripts to use the experimental Goldberg branch
1. Added an option in game scripts to ignore creating a steam_appid file (needed for some games)
1. Added an option in game scripts to use Steam Stub DRM Patcher
1. Added an option in game scripts to set the foreground window to something other than the games, needed for some games to balance out FPS
1. Added an option in game scripts to create a steam_appid.txt file in the same folder as the game executable (needed for some games)
1. Added game option to open the original executable directory
1. Added number of players each game has in the UI game list, under the title
1. Added option to right click on layout selection to go through the different layouts in reverse
1. Added/tweaked some additional logging info
1. Added more information to be displayed when selecting the Details option in UI
1. Updated Goldberg emulator to latest git build (sha 2986b01d0cf34cd900f772cf4294ad387c104cf4)
1. XInput plus and X360ce will now leave controller slots blank if player is using a keyboard (no controller input should work on keyboard instance)
1. Scripts will now open in Notepad++ by default if installed, when using the Open Script option in UI
1. Fixed Script Author notes in UI so that long notes will not overlap when placing controllers
1. Fixed NeedsSteamEmulation option and added SSE support for x64 games
1. Fixed IdInWindowTitle and ForceWindowTitle bug that would prevent them from working
1. Fixed bug preventing mutexes being killed if there were two or more
1. Fixed process with ID of 0 being killed
1. Removed DotNetZip nuget package as it is no longer being used (replaced zip extractions with direct file operations)
1. If a game script has an issue and game doesn't appear, when you fix the issue and launch Nucleus again, you no longer need to re-add the game (game will simply reappear)
1. If a file is unable to be deleted after 10 tries during clean-up, do not throw an exception, just carry-on (details are logged)
1. PromptBetweenInstances will now show the last prompt (to install hooks) if there are hooks to install OR if FakeFocus is set to true
1. Renamed the Data folder to now be "content" (needed as some games mistake the Nucleus Data folder in the path as game files)
1. Renamed the games folder to now be "scripts"

## v0.9.7.2 - October 15, 2019

1. Fixed crash that would happen if ForceFocusWindowName was left blank

## v0.9.7.1 - October 14, 2019

1. When placing controllers, you can now resize any player's screen to be full vertically or horizontally on any layout (custom layouts too!)
1. Added a new option in game scripts to reset the previous window's size, position and borders as each new instance opens up
1. Added an option in game scripts to provide a description, that will appear in the UI when the user selects the script's game
1. Added an option to manually specify what language Goldberg should use
1. Experimental: UI will now scale based on monitor DPI and Font size
1. Improved closing procedures; better chance of finding and killing any lingering game processes
1. Only one copy of Nucleus Coop can be open now
1. Fixed typo preventing start up hooks from working on x64 when using the delay method
1. Fixed Nucleus not launching when there is an error with a game script; will now prompt the user about the error and not show the game in the game list
1. Fixed a bug when stopping a session would sometimes trigger an error message
1. Fixed a bug where third party files were being placed in wrong directory when working folder was set to something in game script
1. Fixed not all the files in the instance folders not being deleted sometimes
1. If Nucleus crashes, the error message is now logged in the debug log in addition to the error file generated in the Data folder, plus taskbar will now show if it was hidden
1. Changed the "Unknown Game" error message to be clearer on what the issue is
1. Instead of denying the user to add a game if the game's executable is already in the game list, it will now prompt the user if they would like to add the game anyway or not
1. Removed pointless horizontal scroll bar on game list
1. Other Minor UI changes

## v0.9.7.0 - October 7, 2019

1. Goldberg Emu is now built-in to Nucleus, 3 new options in game scripts that will fully automate the process
1. Added a customizable custom layout slot when placing controllers/devices. Create your own layout in Settings!
1. Fixed issue of mutexes with certain special characters not being killed or renamed properly
1. Removed SmartSteamEmu embedded in Nucleus.Gaming.dll, greatly reducing the dll's file size. SmartSteamEmu is now accessed and available externally through the "utils" folder (also updated SSE to the latest version, 1.4.3)
1. Added an option in game scripts to copy custom files if needed
1. Added two options in game scripts to be able to do text value replacements in binary files
1. Added two additional options in game scripts to be able to do text value replacements in a file you specify
1. Added new util, ForceBindIP and an option in game scripts to set it up automatically
1. Added new util, XInputPlus and an option in game scripts to set it up automatically
1. Added new util, Devreorder and an option in game scripts to set it up automatically
1. Added an option in game scripts to block raw input devices in game
1. Updated cmd launch to now work with every instance (previously only worked on second instance)
1. User can now right click on game list to access different options for that game
1. Added an option in the UI to open the game script in notepad
1. Added an option in the UI to open the game data folder
1. Added an option in the UI to change the game icon
1. Added an option to enable a debug log to be created
1. Game.ExecutableName is no longer case sensitive when trying to add a game in the UI
1. Fixed an empty error log text file from being created in instance folder whenever a hook was used
1. Fixed x64 focus hooks and setwindow hook
1. Added a SetWindowStart hook that can be applied as each game opens up, instead of later on with SetWindowHook
1. Made game windows not be the top most when using Game.PromptBetweenInstances, until after the last prompt (keeps prompt on top and easier to switch-to)
1. Improved x360ce so that the xinput dll file gets copied along with the executable so x360ce doesn't prompt to create one
1. Exposed the raw gamepad guid of each player's controller in game scripts (changed the name of the x360ce formatted one for clarity)

## v0.9.6.1a - September 22, 2019

1. Fixed 6 and 8 player layouts
1. Reverted mutex searching back to exact matches by default as this broke some games, but left the option to do partial searches if needed (some games do require this)

## v0.9.6.0a - September 20, 2019

1. Added an option in game scripts to rename mutexes instead of killing them
1. Tweaked method of finding mutexes to be more inclusive. You can now provide a partial name of mutex to kill (you must still provide full name for renaming)
1. Upgraded custom xinput dll to Alpha 10s' and added x64 custom dll support (alpha 10 custom dlls are now the default, but you can revert back to alpha 8 with a line in game script for compatability)
1. Added 6 and 8 player support
1. Added an option to delete games from directly within the NucleusCoop UI
1. Added the ability to assign "nicknames" to controllers in settings, used to identify specific controllers
1. Added the option to allow you to use those newly created nicknames in-game, instead of default "Player1", "Player2" etc (this is for games using Goldberg only)
1. Added the ability to see which controller is which by simply holding down a button on a controller. The corresponding controller icon will light up
1. Added an option in game scripts to keep the game's aspect ratio when resizing
1. Added an option in game scripts to manually set width, height, as well as position of each game window
1. Added an option in game scripts to launch and set up x360ce before launching games (make sure you aren't using custom dlls)
1. Added an option in game scripts to have the user decide when to open the next instance, instead of each being opened automatically
1. Added an option in game scripts to hide desktop background
1. Added an option in game scripts to hide taskbar when launching games
1. Added an option in game scripts to hide the mouse cursor
1. Added an option in game scripts to add the process ID to the end of game windows (helpful for creating unique/different window titles)
1. Added an option in game scripts to use different exe names for each instance
1. Added an option in game scripts to Hardcopy game files instead of Symlinking
1. Added an option in game scripts to resize and reposition the previous window after a new window opens (some games need this)
1. Fixed a bug preventing x64 focus and setwindow hooks from working
1. Fixed a bug preventing Nucleus from opening when multiple scripts use the same Game.ExecutableName value
1. WIP: rudimentary support for DInput alongside XInput (more recognized in Nucleus, can be assigned to a screen like any instance - should work in game). *Most dinput devices will still need to be set up manually.
1. WIP: rudimentary Keyboard support

## v0.9.5.3a - August 30, 2019

1. Fixed a bug that stopped Game.FakeFocus from working

## v0.9.5.2a - August 30, 2019

1. Fixed Halo not working correctly with more than 2 instances

## v0.9.5.1a - August 29, 2019

1. Added an option in game scripts to prevent games from resizing on their own
1. Added an option in game scripts to copy files from the game directory to the instanced folder
1. Added a parameter to the registry manipulation methods, you can now specify a base key to work from (local machine or current user)
1. Created a custom solution in order for Halo Custom Edition to work in Nucleus
1. Tweaked EditRegKey so it now takes an object data type as the data and a custom RegType to specify the registry type (DWord, QWord, Binary, etc), opens alot more possibilities
1. Reworked launching games using command prompt, much better results now. CMDOptions can now be specified per game instance (first instance is ignored for now)
1. Fixed an issue with focus hooks that would sometimes crash the game
1. Fixed launching games that don't require mutexes be killed but still have initial hooks
1. Fixed some other minor bugs

## v0.9.5 - August 21, 2019

1. Added an option in game scripts to hook into game functions to trick the game into thinking it has focus
1. Added an option in game scripts to hook into game functions that try and prevent multiple instances from running
1. Added @gymmer's "FocusFaker' method to further trick the game into thinking it has focus
1. Added an option in game scripts to set the window priority of each game instance. Can be set to either "AboveNormal", "High" or "RealTime"
1. Added an option in game scripts to assign which processor games run on. Can be either an ideal processor (safer, no gaurantee) or a fixed one despite availability
1. Added the ability to edit registry keys. Required for some games
1. Added an option in game scripts to work-around ForceFocusWindowName having to match 1:1 with game window title for resizing, positioning and focus. Needed for some games that change their window title (does not effect game launchers)
1. Added an option to force the title of game windows to be whatever is specific in Game.Hook.ForceFocusWindowName
1. Added an option to specify individual files to Symlink
1. Added an option in game scripts to launch a game using cmd with specified options
1. There is a new Settings window (see cog icon to right corner of Nucleus)
1. You can now customize the Nucleus hotkeys to be whatever you'd like
1. Fixed a bug that Nucleus did not work properly when game windows were closed manually
1. Updated method to toggle game windows being top most. It is now more efficient and doesn't rely on game script
1. Minor improvement in performance
1. Cleaned up some code

## v0.9.4.1a - August 7, 2019

1. Fixed bug that broke the generic file manipulation methods

## v0.9.4a - August 6, 2019

1. Added two new methods for game scripts: RemoveLineInTextFile and FindLineNumberInTextFile
1. All four of the generic file manipulation methods (Find, Remove, Replace, ReplacePartial) now include an overload method to specify the kind of encoding to use
1. Added a new variable to be used in Remove & Find Line methods: Nucleus.SearchType. You can specify either "Contains", "Full" or "StartsWith" to get more accurate results. Now with the four file manipulation methods, you should now be able to edit ANY game text file for ANY game
1. Fixed bug in original Nucleus that didn't allow 1 player to be able to start/play a game
1. Fixed "one time use" bug in original Nucleus; Nucleus can now be used multiple times in one session by simply pressing the STOP Button in the app OR pressing the newly created hotkey for it: Ctrl+S
1. Added a toggle to make the game windows not the top most windows. Useful if you want to use other programs in the background. Hotkey is Ctrl+A. Pressing it will either disable or enable the windows being top most, depending on its current state. This requires Game.GameName in Nucleus game scripts to work.

## v0.9a - August 1, 2019

1. Initial release
