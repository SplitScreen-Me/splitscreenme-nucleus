# HANDLER API DOCUMENTATION

Long format version: <https://www.splitscreen.me/docs/handler-api>

Note: If you want to create your own [handler](https://www.splitscreen.me/docs/handlers) see this documentation, and the [Create your handler](https://www.splitscreen.me/docs/create-handlers) guide by Pizzo.

## Game Info

```js
Game.ExecutableName = "Game.exe"; //The name of the game executable with the extension. This will be used by Nucleus to add games, to run the game and as process to follow for positioning and resizing.
Game.ExecutableToLaunch = "Game.exe"; //If the game is to launch a different executable, other than what Game.ExecutableName is.
Game.LauncherExe = "Launcher.exe"; //If the game needs to go through a launcher before opening Nucleus will use this exe to start the game and it will still use the Game.ExecutableName process for positioning and resizing.
Game.GUID = "Game Name"; //The name of the folder that will be created inside the Nucleus content folder (just use letters not symbols). In this folder Nucleus will store the symlinked or copied game files for each instance of the game.
Game.GameName = "Game Name"; //Title of the game that will be shown in Nucleus.
Game.LauncherTitle = "Launcher Window Title"; //The name of the launcher's window title. Some games need to go through a launcher to open. This is needed or else the application will lose the game's window.
Game.MaxPlayersOneMonitor = 4; //This is just info. It will not limit the players number.
Game.MaxPlayers = 16; //This is just the max players info that shows under the handler name in Nucleus UI. Usually we write the max number of players the game supports. (PC, should support 16 max connected input devices).
```

## Mutex

```js
Game.KillMutexType = "Mutant"; //Specify the type of mutex/handle to kill, for example "Mutant", "Event" or "Semaphore" | default: "Mutant".
Game.KillMutexDelay = 10; //# of seconds to wait before killing the mutex | Only works with killing not renaming | default: 0.
Game.RenameNotKillMutex = true; //Instead of killing the mutex or handle, rename it on startup to something else | Requires Game.KillMutex to contain the EXACT name of the handles for this to work (no partial).
Game.PartialMutexSearch = false; //When killing handles, should a partial search be done with the handle name? | Renaming handles requires an exact match | default: false.
Game.KillLastInstanceMutex = false; //Kill the handles, specified in Game.KillMutex, in the last instance (normally last is ignored).
Game.KillMutexAtEnd = false; //When using ProcessChangesAtEnd, should handles also be killed at the end?.
Game.KillMutexProcess = ["Mutexes","To","Close"]; //When using Game.MutexProcessExe, here you can specify the handles that need to be killed.
Game.PartialMutexSearchProcess = false; //When using Game.MutexProcessExe, here you can specify if you want to do a partial search for handles.
Game.KillMutexTypeProcess = "Mutant"; //When using Game.MutexProcessExe, here you can specify the type of handles to kill.
Game.MutexProcessExe = "Process.exe"; //Specify another executable to kill handles for.
Game.PauseBeforeMutexKilling = 1000; //Wait for X number of seconds before proceeding with mutex killing.
Game.KillMutexDelayProcess = 1000; //When using Game.MutexProcessExe, wait for X number of seconds before beginning its mutex killing.
```

## File System

```js
Game.SymlinkGame = true; //If we should symbolic link the game's files to a temporary directory (Nucleus instances folders in its content folder). If not will launch straight from the installation directory.
Game.SymlinkExe = false; //If SymlinkGame is enabled, if we should copy or symlink the game executable.
Game.SymlinkFolders = false; //Folders by default are hardcopied, with this enabled, folders will be symlinked instead | warning files placed in symlink folders will appear in the original game files too.
Game.KeepSymLinkOnExit = true; //Enable or disable symlink files from being deleted when Nucleus is closed | default: false.
Game.SymlinkFiles = ["game.exe","settings.cfg"]; //Symlink individual files from the game directory to the instanced folder.
Game.CopyFiles = ["game.exe","settings.cfg"]; //Copy files from the game directory to the instanced folder.
Game.HardcopyGame = true; //Instead of Symlinking, create hard copies of the games files (copy/paste) | Be careful of storage, takes a LONG time to start a game when copying.
Game.HardlinkGame = false; //Hardlink files instead of Symlink (or hard copy) | Directories will still be symlinked but files will be hardlinked.
Game.ForceSymlink = false; //Force game to be symlinked each time it is ran.
Game.DirSymlinkExclusions = ["folder1", "folder2"]; //Array with the name of the folders you don't want Nucleus Co-op to symlink, only the folders placed here get hardcopied not the files.
Game.FileSymlinkExclusions = ["file1.txt", "file2.txt"]; //Array with the name of the files you don't want Nucleus Co-op to symlink, useful if you want to replace files or add external files.
Game.FileSymlinkCopyInstead = ["file1.txt", "file2.txt"]; //Array with the name of the files you want Nucleus Co-op to make full copies of, in some cases games need certain files to be full copies or they won't run.
Game.DirSymlinkCopyInstead = [ "folder1", "folder2" ]; //Copy (not symlink) all files within a given folder | Folder name is relative from root game folder.
Game.DirSymlinkCopyInsteadIncludeSubFolders = false; //When specifying folder(s) to copy all its contents instead of linking, should subfolders and files be included as well?
Game.SymlinkFoldersTo = ["folderToMove|whereToMoveIt"]; //Symlink folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.HardlinkFoldersTo = ["folderToMove|whereToMoveIt"];//Hardlink folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.ChangeExe = true; //Will rename the game's executable to "<exe name> - Player X", x being the instance/player #.
Game.CopyCustomUtils = [ "d3d9.dll" ]; //Copy the a file or folder you specify between the quotes to each instance folder, if the file/folder is located in Nucleus folder\utils\User. This function also accepts two additional parameters, a relative path from the game root folder if the file needs to be placed somewhere else within instance folder and one parameter to indicate which instances to copy the file to if it only needs to be in some. Use of parameters is separated by a | character. So it would look something like this [ "filename.ini|\\bin|1,2" ]. This example would copy filename.ini from Nucleus\utils\User to Instance0\bin and Instance1\bin. If you don't specify which instances, it will do them all by default. If you don't specify a path then root instance folder is used. If you want instances but root folder you would just do [ "filename.ini||1,2" ] | If copying multiple files, use comma seperated strings
Game.HexEditAllExes = [ "before|afters" ]; //Will do a text value replacement in a file for every instance, accepts two parameters, one on what to look for, and what it should be replaced with (seperated with a "|") | Works with multiple values | Works in conjunction with HexEditExe (this will trigger first).
Game.HexEditExe = [ "before|afters" ]; //Will do a text value replacement in a file for a specific instance, accepts two parameters, one on what to look for, and what it should be replaced with (seperated with a "|") | Works with multiple values, each comma seperated string is the order of the instances | Works in conjunction with HexEditAllExe (this will trigger second).
Game.HexEditFile = [ "filePath|before|afters" ]; //Works same as HexEditExe function but with a file you specify as an extra parameter (first one) | filePath is relative path from the game root folder.
Game.HexEditAllFiles = [ "filePath|before|afters" ]; //Works same as HexEditAllExes function but with a file you specify as an extra parameter (first one) | filePath is relative path from the game root folder.
Game.DirExclusions = ["dir1"]; //Folders (+ all its contents) listed here will not be linked or copied over to Nucleus game content folder, the instance folders.
Game.CreateSteamAppIdByExe = false; //Create a steam_appid.txt file where the game executable is.
Game.LauncherExeIgnoreFileCheck = false; //Do not check if Launcher Exe exists in game folder | you will need to provide a relative filepath from game root folder.
Game.CopyEnvFoldersToNucleusAccounts = ["Documents", "AppData"]; //Copy subfolders of current user profile to Nucleus user accounts.
Game.CopyFoldersTo = ["folderToMove|whereToMoveIt"]; //Copy folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.HexEditExeAddress = [ "1|address|bytes" ]; //Use this to replace bytes in a file at a specified address, can be for specific instances with optional 3rd argument | 1st arg: instance # (optional), 2nd arg: hex address offset, 3rd arg: new bytes.
Game.HexEditFileAddress = [ "1|relativePath|address|bytes" ]; //Same as HexEditExeAddress but for a file other than exe | Need to provide relative path (from game root folder) + filename as 1st or 2nd arg if not specifying an instance.
Game.IgnoreDeleteFilesPrompt = false; //Do not display the warning message about a file being deleted .
Game.RenameAndOrMoveFiles = [ "1|before.dat|after.dat" ];//Specify files to either rename or move | can accept relative path from root | optional first parameter to specify a specific instance to apply to, omit to do them all.
Game.DeleteFiles = [ "1|delete.dis" ]; //Specify files to be deleted from instanced folder | can accept relative path from root | optional first parameter to specify a specific instance to apply to, omit to do them all.
Game.RunLauncherAndExe = false; //When using Game.LauncherExe, should ExecutableName also be launched?
Game.ForceLauncherExeIgnoreFileCheck = false; //Forces LauncherExeIgnoreFileCheck when game isn't symlinked.
```

## Nucleus Co-op Environment

```js
Game.UseNucleusEnvironment = false; //Use custom environment variables for games that use them, replaces some common paths (e.g. AppData) with C:\Users\<your username>\NucleusCoop.
Game.UserProfileConfigPath = "AppData\\Local\\Game\\Config"; //Relative path from user profile (e.g. C:\Users\ZeroFox) to game's config path | Used to provide some extra functionality (open/delete/copy over to Nucleus Environment).
Game.UserProfileSavePath = "AppData\\Local\\Game\\Saves"; //Relative path from user profile (e.g. C:\Users\ZeroFox) to game's save path | Used to provide some extra functionality (open/delete/copy over to Nucleus Environment).
Game.ForceUserProfileConfigCopy = false; //Force the games config files in UserProfileConfigPath to copy over from system user profile to Nucleus environment.
Game.ForceUserProfileSaveCopy = false; //Force the games save files in UserProfileSavePath to copy over from system user profile to Nucleus environment.
Game.DeleteFilesInConfigPath = [ "file.del", "me.too" ];//Specify files to delete in Nucleus environment config path (UserProfileConfigPath).
Game.DeleteFilesInSavePath = [ "file.del", "me.too" ]; //Specify files to delete in Nucleus environment save path (UserProfileSavePath).
Game.UserProfileConfigPathNoCopy = false; //Do not copy files from original UserProfileConfigPath if using Nucleus Environment.
Game.UserProfileSavePathNoCopy = false; //Do not copy files from original UserProfileSavePath if using Nucleus Environment.
Game.UseCurrentUserEnvironment = false; //Force the game to use the current user's environment (useful for some games that may require different Window user accounts).
Game.DocumentsConfigPath = "Path\\Here"; //Similar to UserProfileConfigPath, use this when the game uses Documents to store game files.
Game.DocumentsSavePath = "Path\\Here"; //Similar to UserProfileSavePath, use this when the game uses Documents to store game files.
Game.ForceDocumentsConfigCopy = false; //When using DocumentsConfigPath, forces a file copy from original location to Nucleus Documents.
Game.ForceDocumentsSaveCopy = false; //When using DocumentsSavePath, forces a file copy from original location to Nucleus Documents.
Game.DocumentsConfigPathNoCopy = false; //When using DocumentsConfigPath, do not let Nucleus copy from original location to Nucleus Documents.
Game.DocumentsSavePathNoCopy = false; //When using DocumentsSavePath, do not let Nucleus copy from original location to Nucleus Documents.
Game.ForceEnvironmentUse = true; //Forces use of custom environment variable when Game.ThirdPartyLaunch = true;
```

## Focus

```js
Game.FakeFocus = true; //Enable or disable the sending of focus messages to each game window at a regular interval | default: false.
Game.HookFocus = true; //Enable or disable hooks to trick the game into thinking it has focus | default: false.
Game.HookInit = true; //Enable or disable hooks of functions some games may try and use to prevent multiple instances from running | default: false.
Game.PreventWindowDeactivation = false; //Blocks the processing of all the windows messages that get sent when the window loses focus.
Game.HasDynamicWindowTitle = false; //Work-around for ForceFocusWindowName having to match 1:1 with game window title for resizing, positioning and focus | default: false.
Game.ForceWindowTitle = false; //Forces the game window title to be whatever is specified in Game.Hook.ForceFocusWindowName (triggers once, after all instances have started) | default: false.
Game.IdInWindowTitle = true; //Adds the process ID to the end of the window title.
Game.SetForegroundWindowElsewhere = false; //Set the foreground window to be something other than game windows.
Game.StartHookInstances = "1,2,3,4"; //If you only want specific instances to have starting hooks, specify them in a comma seperated string.
Game.PostHookInstances = "1,2,3,4"; //If you only want specific instances to have post launch hooks, specify them in a comma seperated string.
Game.FakeFocusInstances = "1,2,3,4"; //If you only want specific instances to have fake focus messages sent to, specify them in a comma seperated string.
Game.KeyboardPlayerSkipFakeFocus = false; //Should the keyboard player instance be skipped when fake focus messages are being sent to.
Game.KeyboardPlayerSkipPreventWindowDeactivate = false; //Ignore PreventWindowDeactivation if player is using keyboard.
Game.FakeFocusSendActivate = true; //Should WM_ACTIVATE message be sent to each instance? | default: true.
Game.PreventGameFocus = false; //Makes sure all the game windows are unfocused so nothing received double input from Windows.
Game.FakeFocusInterval = 1000; //The milliseconds between sending fake focus messages. Default at 1000, some rare games need this to be very low.
Game.EnableWindows = false; //Enable each game window at the end (useful if became disabled, or for some games that require this Window function to be called to display properly, after Nucleus setup).
Game.ProcessChangesAtEnd = false; //Do the resizing, repositioning and post-launch hooking of all game instances at the very end | will not work with every option ran normally.
Game.PromptProcessChangesAtEnd = false; //If ProcessChangesAtEnd = true, pause and show a prompt, before making changes to processes.
Game.PromptBetweenInstancesEnd = false; //If ProcessChangesAtEnd = true, show a prompt between each instance being changed.
```

## Window manipulation

```js
Game.SetWindowHook = true; //Prevent games from resizing their windows on their own | Hooks after all instances have been opened (see Game.SetWindowHookStart for an alternative).
Game.SetWindowHookStart = false; //Prevent games from resizing window their windows on their own | Hooks upon game starting up (see Game.SetWindowHook for an alternative).
Game.Hook.FixResolution = false; //Should the custom dll do the resizing? | Only works with Alpha 10 custom dll | default: false.
Game.Hook.FixPosition = false; //Should the custom dll do the repositioning? | Only works with Alpha 10 custom dll | default: false.
Game.Hook.WindowX = 0; //If manual positioning, what is the window's X coordinate | If both X and Y value > 0, window will be positioned manually.
Game.Hook.WindowY = 0; //If manual positioning, what is the window's Y coordinate | If both X and Y value > 0, window will be positioned manually.
Game.Hook.ResWidth = 1280; //If manual resizing, what is the window's width | If both ResWidth and ResHeight value > 0, window will be positioned manually.
Game.Hook.ResHeight = 720; //If manual resizing, what is the window's height | If both ResWidth and ResHeight value > 0, window will be positioned manually.
Game.KeepAspectRatio = false; //Should the game window try and keep it's aspect ratio when being resized? | default: false.
Game.ResetWindows = false; //After each new instance opens, resize, reposition and remove borders of the previous instance.
Game.KeepMonitorAspectRatio = false; //Try and resize game window within player bounds to the aspect ratio of the monitor.
Game.DontResize = false; //Should Nucleus not resize the game windows?.
Game.DontReposition = false; //Should Nucleus not reposition the game windows?.
Game.NotTopMost = false; //Should Nucleus not make the game windows top most (appear above everything else).
Game.WindowStyleValues = [ "~0xC00000", "0x8000000" ]; //Override Nucleus' default of removing borders and specify a custom window style | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.ExtWindowStyleValues = [ "~0x200", "0x200000" ]; //Override Nucleus' default of removing borders and specify a custom extended window style | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.RefreshWindowAfterStart = false; //Should each game window be minimized and restored once all instances are opened?.
Game.WindowStyleEndChanges = [ "~0xC00000", "0x8000000" ]; //Override Nucleus' default of removing borders and specify a custom window style during end processing | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.ExtWindowStyleEndChanges = [ "~0xC00000", "0x8000000" ]; //Override Nucleus' default of removing borders and specify a custom window style during end processing | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.IgnoreWindowBordercheck = false; //Ignore logic at end to check if any game window still has a border.
Game.DontRemoveBorders = false; //Prevents Nucleus from removing game window borders.
Game.SetTopMostAtEnd = true; //Set the game windows to top most at the very end.
```

## Input

```js
Game.SupportsKeyboard = true; //Enable/disable use of one keyboard/mouse player.
Game.XInputPlusDll = ["xinput1_3.dll"]; //Set up XInputPlus | If multiple dlls required, use comma seperated strings.
Game.KeyboardPlayerFirst = true; //If the keyboard player should be processed first.
Game.UseX360ce = true; //Before launching any games, Nucleus Co-op will open x360ce and let the user set up their controllers before continuing | close x360ce to continue | Don't use with custom dlls.
Game.Hook.UseAlpha8CustomDll = false; //Use the xinput custom dll from Alpha 8 instead of Alpha 10 | Will still force alpha 10 custom dll if game is x64.
Game.PlayersPerInstance = 2; //If using XInputPlus or X360ce and there are multiple players playing the same instance, set the # per instance here.
Game.UseDevReorder = true; //Set up Devreorder.
Game.XboxOneControllerFix = false; //When using x360ce, this will set certain hooktype that may work for xbox one controllers if the normal method does not work.
Game.BlockRawInput = false; //Disable raw input devices in game | default: false.
Game.X360ceDll = [ "xinput1_3.dll" ]; //If x360ce dll should be named something OTHER than xinput1_3.dll | requires Game.Usex360ce to be set to true.
Game.CreateSingleDeviceFile = false; //Create only one file for HID devices per instance (the assigned HID device).
Game.Hook.EnableMKBInput = false; //Enable Mouse/Keyboard input for instances when using Alpha 10 custom xinput dll (normally MKB is restricted).
Game.UseDInputBlocker = false; //Setup wizark952's dinput blocker (block dinput for the game).
Game.XInputPlusNoIni = false; //Do not copy XInputPlus' ini when using Game.XInputPlusDll.
Game.XInputPlusOldDll = false; //When using Game.XInputPlusDll, you can specify to use the previous version instead of latest (needed for some games).
```

## Goldberg Emulator

```js
Game.UseGoldberg = false; //Use the built-in Goldberg features in Nucleus | default: false.
Game.GoldbergExperimental = false; //Use the experimental branch of Goldberg | Requires `Game.UseGoldberg = true` | default: false.
Game.GoldbergExperimentalSteamClient = false; //Automatically setup Goldberg's Experimental Steam Client | Requires Game.UseGoldberg and the original steam_api.dll.
Game.GoldbergLobbyConnect = false; //Should Goldberg Lobby Connect be used to connect the instances.
Game.GoldbergNoLocalSave = false; //Do not create a local_save.txt file for Goldberg, saves are to use default game save location.
Game.GoldbergNeedSteamInterface = false; //Some older games require a steam_interfaces.txt file for Goldberg to work | Will first search orig game path and nucleus games path, if not found then tries to create one with the GoldbergNeedSteamInterface command.
Game.GoldbergLanguage = "english"; //Manually specify what language you want Goldberg to use for the game | by default, Goldberg will use the language you chose in Steam.
Game.OrigSteamDllPath = "C:\full path\steam_api.dll"; //If steam_interface.txt is required, provide full path here to the original steam_api(64).dll and Nucleus will create one if it can't find an existing copy.
Game.GoldbergIgnoreSteamAppId = false; //When set to true, Goldberg will not create a steam_appid.txt file.
Game.PlayerSteamIDs = ["76561198134585131","76561198134585132"]; //A list of steam IDs to be used instead of the pre-defined ones Nucleuses uses | IDs will be used in order they are placed, i.e. instance 1 will be first non-empty string in array.
Game.GoldbergExperimentalRename = false; //Set to true to have Goldberg Experimental rename instanced steam_api(64).dll to cracksteam_api(64).dll.
Game.GoldbergWriteSteamIDAndAccount = false; //Force Goldberg to write account_name.txt and user_steam_id.txt | Requires Game.UseGoldberg;
```

## Smart Steam Emulator

```js
Game.NeedsSteamEmulation = false; //If the game needs a fake Steam wrapper (SSE) to run multiple instances (this is not needed if you are using the new use Goldberg emulator line).
Game.SSEAdditionalLines = ["Section|Key=Value"]; //When using Game.NeedsSteamEmulation, here you can provide additional lines to write to the SSE ini file.
```

## Nemirtingas Epic Emulator

```js
Game.UseNemirtingasEpicEmu = false; //Automatically set up Nemirtinga's Epic Emulator in Nucleus
Game.EpicEmuArgs = false; //When using Nemirtinga's Epic Emulator, use pre-defined parameters -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal -epicusername=\"" + <Player Nickname here> + "\" -epicuserid=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA -epiclocale=en"
Game.AltEpicEmuArgs = false; //Optional. When using Nemirtinga's Epic Emulator, use pre-defined parameters + Set NickName as epic id, only to use with games that do not use epic id to start or connect(Set clever save names if the game use the epic id to name saves ex: Tiny Tina's Assault On Dragon Keep)" -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal -epicusername=" + <Player Nickname here> + " -epicuserid="+ <Player Nickname here> + "-epiclocale=" + EpicLang".

/*
 * NemirtingasEpicEmu.json edition from a handler example:
 */

 // If you don't need to edit a line do not add it here, the emu will automatically write it with default parameters. More info here: https://gitlab.com/Nemirtingas/nemirtingas_epic_emu/-/blob/master/README.md
 // Available debug parameters,  should be "off" by default. Only required to debug the nermintingas eos emulator.
 // TRACE: Very verbose, will log DEBUG + All functions enter
 // DEBUG: Very verbose, will log INFO  + Debug infos like function parameters
 // INFO : verbose     , will log WARN  + some informations about code execution and TODOs
 // WARN : not verbose , will log ERR   + some warnings about code execution
 // ERR  : not verbose , will log FATAL + errors about code execution
 // FATAL: not verbose , will log only Fatal errors like unimplemented steam_api versions
 // OFF  : no logs     , saves cpu usage when running the debug versions
 // In case of using custom start arguments => -epicusername == same username as in the.json => -epicuserid == same epicid as in the.json >

    var jsonPath = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\nepice_settings\\NemirtingasEpicEmu.json";// + "\\NemirtingasEpicEmus.json" for stable older epic emu version.
    var params = [
      '{',
      '  "appid": "InvalidAppid",',
      '  "disable_online_networking": false,',
      '  "enable_lan": true,',
      '  "enable_overlay": true,',
      //'  "epicid": "3808a45790894253344fec21026bbf80",', //better to let the emu automaticaly add this line.
      '  "language":' + '"' + Context.EpicLang + '"' + ',',
      '  "log_level": "off",',
      //'  "productuserid": "ab65359ffde1b5cc41e81afee8e32c33",', //better to let the emu automaticaly add this line.
      '  "savepath": "appdata",',
      '  "signaling_servers": [],',
      '  "unlock_dlcs": true,',
      '  "username": ' + '"' + Context.Nickname + '"', //must always be added if you edit the json and must be the last line else the emulator will reset all parameters(there is no coma at the end of this line in the json).
      '}'
    ] ;
    Context.WriteTextFile(jsonPath,params);
```

## Nemirtingas Galaxy GoG Emulator

```js
Game.UseNemirtingasGalaxyEmu = false; //Automatically set up Nemirtinga's Galaxy Emulator in Nucleus.

NemirtingasGalxyEmu.json edition from a handler example:

    var idg = Context.PlayerID + 6;

    var jsonPath = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\ngalaxye_settings\\NemirtingasGalaxyEmu.json";
    var params = [
      '{',
      '  "api_version": "1.139.2.0",',
      '  "enable_overlay": false,',
      '  "galaxyid": 14549624462898294'+idg+',',
      '  "language": ' + '"' + Context.GogLang + '",',
      '  "productid": 2143654691,',
      '  "username": ' + '"' + Context.Nickname + '"', //must always be added if you edit the json and must be the last line else the emulator will reset all parameters (there is no coma at the end of this line in the json).
      '}'
     ];
    Context.WriteTextFile(jsonPath,params);
```

## Additional Tools

```js
Game.UseSteamless = true; //Use atom0s' Steamless app to remove Steam Stub DRM from a protected executable.
Game.SteamlessArgs = "--quiet --keepbind"; //Use this when using Game.UseSteamless = true; always, the command line version of Steamless allows for different launch arguments to be used.
Game.SteamlessTiming = 2500; //The time in milliseconds to give Steamless to patch the game .exe. 2500 is the default value and will be applied even if the timing line has not been added in a handler.
Game.UseSteamStubDRMPatcher = false; //Use UberPsyX's Steam Stub DRM Patcher to remove Steam Stub DRM from a protected executable.
Game.SteamStubDRMPatcherArch = "64"; //Force Steam Stub DRM Patcher to use either the x64 or x86 dll | Values: "64" or "86".
Game.UseEACBypass = false; //Replace any EasyAntiCheat_(x86)(x64).dll with a bypass dll.
Game.FlawlessWidescreen = "FWGameName"; //Use Flawless Widescreen application | value must be the name of the game plugin folder in PluginCache\FWS_Plugins\Modules.
Game.FlawlessWidescreenOverrideDisplay = false; //(undocumented)
Game.FlawlessWidescreenPluginPath //(undocumented)
```

## Network

```js
Game.UseForceBindIP = false; //Set up game instances with ForceBindIP; each instance having their own IP.
Game.ChangeIPPerInstance = false; //Highly experimental feature, will change your existing network to a static IP for each instance | option in settings to choose your network.
Game.ChangeIPPerInstanceAlt; //An alternative method to changing IP per instance | this method will create loopback adapters for each player and assign them a static ip on the same subnet mask as your main network interface.
```

## Extra

```js
Game.PauseBetweenProcessGrab = 30; //How many seconds to wait after launching game (or launcher) but before grabbing game process.
Game.PauseBetweenContextAndLaunch = 0; //Number of seconds to wait after running additional files but before continuing with player setup.
Game.ThirdPartyLaunch = false; //Use if the game is launched outside of Nucleus | NOTE: You will not be able to use start up hooks or CMDLaunch with this option.
Game.IgnoreThirdPartyPrompt = false; //Ignore the prompt that appears when using Game.ThirdPartyLaunch;
Game.ForceProcessPick = false; //Manually select the process that will be used for process manipulation, such as resizing, repositioning and used for post-launch hooks.
Game.PromptBetweenInstances = true; //Prompt the user with a messagebox to let the user decide when to open the next instance | default: false, PauseBetweenStarts STILL required.
Game.PromptAfterFirstInstance = false; //Show a prompt that user must click on ok to continue, after the first instance is setup.
Game.GamePlayAfterLaunch; //Call the Game.Play function after the call has launched.
Game.GamePlayBeforeGameSetup = false; //Execute Game.Play function (context) before the majority of the game is setup.
Game.LaunchAsDifferentUsers = false; //Launch each instance from a different user account | must run Nucleus as admin | will temporary create user accounts "nucleusplayerx" and delete them when closing Nucleus.
Game.LaunchAsDifferentUsersAlt = false; //An alternative method to launch each instance from a different user account | must run Nucleus as admin | will temporary create user accounts "nucleusplayerx" and delete them when closing Nucleus.
Game.TransferNucleusUserAccountProfiles = false; //Will backup and restore Nucleus user account user profile's on windows between sessions (when user accounts are not kept).
Game.ForceProcessSearch = false; //Force Nucleus to search for the game process.
Game.RequiresAdmin = false; //Game requires Nucleus to be run as administrator (this option will check and advise if detected not running Nucleus as admin).
Game.WriteToProcessMemory //(undocumented)
Game.ProcessorPriorityClass = "Normal"; //Sets the overall priority category for the associated process. Can be "AboveNormal", "High" or "RealTime" | default: "Normal".
Game.UseProcessor = "1,2,3,4"; //Sets the processors on which the game's threads can run on (provide the # of the cores in a comma-seperated list in quotations) | default: system delegation.
Game.UseProcessorsPerInstance = [ "1,2","3,4" ]; //Sets the processors on which an instances game's threads can run on (provide the # of the cores in a comma-seperated list in quotations) | default: system delegation.
Game.IdealProcessor = 2; //Sets the preferred processor for the game's threads, used when the system schedules threads, to determine which processor to run the thread on | default: system delegation.
Game.HideCursor = true; //Hide the mouse cursor in game instances.
Game.HideDesktop = false; //Hide the desktop background with a solid black background.
Game.HideTaskbar = true; //Most games hide the taskbar when placed on-top but for games that don't you can use this.
Game.Description = "Hello World"; //Display a message to the end-user, that will appear when user selects the game of a handler| useful if there is anything the end-user needs to know before-hand | Only first two or three sentences will appear in UI, but full message can be viewed if user right clicks on the game in the list.
Game.KillProcessesOnClose = [ "kill", "me" ]; //List of additional processes that need to be killed (other than executable and launcher).
Game.DeleteOnClose = ["DeleteThis.exe"]; //Delete a file upon ending game session | Relative paths from root of game folder.
Game.UseDirectX9Wrapper = false; //Use a Direct X wrapper to try and force DirectX 9 games to run windowed.
Game.CMDLaunch = false; //Launch a game using command prompt.
Game.CMDOptions = ["ops1","ops2"]; //Specify command line options if game is launched using command prompt | requires CMDLaunch to be true, each element is for a different instance.
Game.CMDBatchBefore = [ "0|ops1", "1|ops2" ]; //When using CMDLaunch (or UseForceBindIP), specify command lines to run BEFORE the game launches in same cmd session, syntax is instance and | symbol as a delimiter and the line you want that instance to run. If you want same line to run all instances, leave out the # and | (only write the line in quotes).
Game.CMDBatchAfter = [ "0|ops1", "1|ops2" ]; //When using CMDLaunch (or UseForceBindIP), specify command lines to run AFTER the game launches in same cmd session, syntax is instance and | symbol as a delimiter and the line you want that instance to run. If you want same line to run all instances, leave out the # and | (only write the line in quotes).
Game.PauseCMDBatchBefore = 10; //Wait for X number of seconds before proceeding with CMDBatchBefore.
Game.PauseCMDBatchAfter = 10; //Wait for X number of seconds before proceeding with CMDBatchAfter.
Game.CMDBatchClose ["cmd1", "cmd2"]; //Run command lines upon exiting Nucleus.
Game.CMDStartArgsInside = false; //When using CMDLaunch, should the game's starting arguments be inside the same quotations as the game path?
Game.ForceGameArch = "x86" (or "x64"); //Force Nucleus to treat the game as 32 or 64-bit architecture.
Game.SplitDivCompatibility = false; //Explicitly disable splitscreen divisons if the game is known to be imcompatible with it.(Does not require to be true for compatible game)Default = true.

/*
 * Custom prompts - Prompt user for input, which can be then used in handlers logic
 */

Game.CustomUserGeneralPrompts = ["Enter ROM name", "Enter filename"]; //This prompts user one time and applies to ALL players, unless a value text file already exists and saving is on.
Game.SaveCustomUserGeneralValues = false;
Game.SaveAndEditCustomUserGeneralValues = false;

Game.CustomUserPlayerPrompts = ["Enter network Adapter name", "Enter character name"]; //This will prompt each player, unless a value text file already exists for that player and saving for players is on.
Game.SaveCustomUserPlayerValues = false;
Game.SaveAndEditCustomUserPlayerValues = false;

Game.CustomUserInstancePrompts = ["Enter network Adapter name"]; //This will prompt each instance, unless a value text file already exists for that instance and save is on. In case it is not player specific but different values are needed for instances.
Game.SaveCustomUserInstanceValues = false;
Game.SaveAndEditCustomUserInstanceValues = false;
//Access the user input values via Context.CustomUser(General/Player/Instance)Values[index]

/*
 * 2. Support for multiple mice and keyboards (These options are deprecated, see the new Proto Input guide: https://www.splitscreen.me/docs/proto)
 */

Game.SupportsMultipleKeyboardsAndMice = true;
Game.SendNormalMouseInput = true;
Game.SendNormalKeyboardInput = true;
Game.ForwardRawKeyboardInput = false;
Game.ForwardRawMouseInput = false;
Game.SendScrollWheel = false;
Game.DrawFakeMouseCursor = true;
Game.DrawFakeMouseForControllers = false;
Game.HookFilterRawInput = false;
Game.HookFilterMouseMessages = false;
Game.HookGetCursorPos = true;
Game.HookSetCursorPos = true;
Game.HookUseLegacyInput = false;
Game.HookDontUpdateLegacyInMouseMsg = false;
Game.HookGetKeyState = false;
Game.HookGetAsyncKeyState = true;
Game.HookGetKeyboardState = false;
Game.HookMouseVisibility = false;
Game.LockInputAtStart = true;
Game.LockInputToggleKey = 0x23; //See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
Game.HookReRegisterRawInput = false; //Re-register raw input from directly within game process | Recommended to disable forwarding input while using this
Game.HookReRegisterRawInputMouse = true;
Game.HookReRegisterRawInputKeyboard = true;
Game.UpdateFakeMouseWithInternalInput = false;

/*
 * 3. New methods to be used in game handlers -
 */
```

## Path variables

```js
Context.RootInstallFolder; //Path to the source game root folder.
Context.NucleusFolder //Path to the Nucleus Co-op root folder (Nucleus-Coop\).
Context.ScriptFolder //Path to the Nucleus Co-op handlers root folder (Nucleus-Coop\handlers).
Game.Folder //Path to the game handler folder (Nucleus-Coop\handlers\handler_folder).
Context.GetFolder(Nucleus.Folder.InstancedGameFolder) //Path to the Nucleus Co-op current instance root folder (Nucleus-Coop\content\GUID\Instance#).
Context.EnvironmentPlayer //Path to current players Nucleus environment.
Context.EnvironmentRoot //Path to Nucleus environment root folder.
Context.UserProfileConfigPath //Relative path from user profile to game's config path | requires Game.UserProfileConfigPath be set.
Context.UserProfileSavePath //Relative path from user profile to game's save path | requires Game.UserProfileSavePath be set.
Context.DocumentsPlayer //Path to current players Nucleus documents environment.
Context.DocumentsRoot //Path to Nucleus documents environment root folder.
Context.DocumentsConfigPath //Relative path from user profile to game's config path | requires Game.DocumentsConfigPath be set.
Context.DocumentsSavePath //Relative path from user profile to game's save path | requires Game.DocumentsSavePath be set.
Context.NucleusUserRoot //Path to current players Nucleus environment Windows User root folder.
Context.HandlersFolder //return "NucleusCo-op\handlers" path.

Context.ChangeXmlNodeInnerTextValue(string path, string nodeName, string newValue) //Edit an XML element (previously only nodes and attributes).
Context.ReplaceLinesInTextFile(string path, string[] lineNumAndnewValues) //Replace an entire line; for string array use the format: "lineNum|newValue", the | is required.
Context.ReplacePartialLinesInTextFile(string path, string[] lineNumRegPtrnAndNewValues) //Partially replace a line; for string array use the format: "lineNum|Regex pattern|newValue", the | is required.
Context.RemoveLineInTextFile(string path, int lineNum) //Removes a given line number completely.
Context.RemoveLineInTextFile(string path, string txtInLine, SearchType type) //Removes a given line number completely.
Context.FindLineNumberInTextFile(string path, string searchValue, SearchType type) //Returns a line number (int), utilizes a newly created enum SearchType.
/*
 * Each of the above methods, also have an overload method so you can specify a kind of encoding to use (enter string of encoding as last parameter, e.g. "utf-8", "utf-16" "us-ascii").
 * - SearchTypes include: "Contains", "Full" and "StartsWith", use like so: Nucleus SearchType.StartsWith.
 */ 

Context.CreateRegKey(string baseKey, string sKey, string subKey) //Create a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER".
Context.DeleteRegKey(string baseKey, string sKey, string subKey) //Delete a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER".
Context.EditRegKey(string baseKey, string sKey, string name, object data, RegType type) //Edit a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER". EditRegKey uses a custom registry data type to use, by using Nucleus.RegType.DWord for example. The last word can be of the same name of RegistryValueKind enum.
Context.EditRegKeyNoBackup //Edit a registry key for current user without Nucleus creating a backup of the registry or before it creates one. Must be placed before any "Context.EditRegKey" line in handler for it to work.
Context.Nickname //Use this in a game handler to get the player's nickname
Context.EpicLang //Can be use to edit NemirtingasEpicEmu.json //Can be use in start argument to setup user Epic language parameter //ex: Context.StartArguments = ' -AlwaysFocus -nosplash -nomoviestartup -nomouse' + Context.EpicLang; (if Epic Language is set to "en" return => "-epiclocale=en", Should not be necessary in most cases)
Context.GamepadGuid //Get the raw gamepad guid
Context.GogLang //Can be use to edit NemirtingasGalaxyEmu.json from handlers
Context.GamepadId //Useful to make sure that controllers are correctly assigned to the player they are meant to be assigned.
Context.x360ceGamepadGuid //Get the x360ce formatted gamepad guid.
Context.LocalIP //Local IP address of the computer.
Context.KillProcessesMatchingProcessName(string name) //Kill processes matching a given process name during preperation.
Context.KillProcessesMatchingWindowName(string name) //Kill processes matching a given window name during preperation.
Context.IsKeyboardPlayer //True if current player is using keyboard, useful for logic that is needed for keyboard only player.
Context.OrigHeight //Player monitor's height.
Context.OrigWidth //Player monitor's width.
Context.OrigAspectRatio //Player monitor's aspect ratio (e.g. 16:9).
Context.OrigAspectRatioDecimal //Player monitor's aspect ratio in decimal (e.g. 1.777777).
Context.AspectRatio //Player's aspect ratio (e.g. 16:9).
Context.AspectRatioDecimal //Player's aspect ratio in decimal (e.g. 1777777).
Context.FindFiles(string rootFolder, string fileName) //Return a string array of filenames (and their paths) found that match a pattern you specify.
Context.CreatedDate(string file, int year, int month, int day) //Change the creation date of a file.
Context.ModifiedDate(string file, int year, int month, int day) //Change the last modified date of a file.
Context.RunAdditionalFiles(string[] filePaths, bool changeWorkingDir, string customText, int secondsToPauseInbetween, bool showFilePath, bool runAsAdmin, bool promptBetween,bool confirm) //Specify additional files to run before launching game. By default will run each additional file once but can specify to run during specific player's instances by prefixing the filepath with #|. Replace # with player number. Can also specify to run files for each player by prefixing filepath with "all|". "bool confirm" will only run the file after clicking "Ok" in the prompt.
Context.ReadRegKey(string baseKey, string sKey, string subKey) //Return the value of a provided key as a string.
Context.HandlerGUID
Context.StartArguments = ""; //Adds whatever you put into the field as starting parameters for the game's executable in context. For example, in most cases '-windowed' will force windowed mode. Parameters can be chained.

Context.NumberOfPlayers

Context.CopyScriptFolder
Context.HexEdit
Context.PatchFileFindAll
Context.MoveFolder

Context.CopyScriptFolder(string DestinationPath)
Context.RandomInt(int min, int max)
Context.RandomString(int size, bool lowerCase = false)

Context.ConvertToInt()
Context.ConvertToString()
Context.ConvertToBytes()
Context.GCD(int a, int b)
Context.Arch
Context.PosX
Context.PosY
Context.MonitorWidth
Context.MonitorHeight
Context.Log()
Context.ProcessID
Context.HasKeyboardPlayer
```

## Other useful lines

```js
System.IO.File.Delete("FileToDelete"); //Delete the specified file.
System.IO.File.Copy("SourceFile", "DestinationFile", true); //Copy the specified "SourceFile" to the "DestinationFile" path. You can uses a different name on the DestinationFile to rename the file. The true in the end specify to overwrite, if found, the existing file with the same name.
System.IO.File.Move("SourceFile", "DestinationFile", true); //Move the specified "SourceFile" to the "DestinationFile" path. You can uses a different name on the DestinationFile to rename the file. The true in the end specify to overwrite, if found, the existing file with the same name.
var OneFolderUP = System.IO.Path.Combine(Path goes here, ".."); // The variable OneFolderUP will return a folder UP of the specified path. Example: var OneFolderUP = System.IO.Path.Combine(Game.Folder, "..");
var TwoFolderUP = System.IO.Path.Combine(Path goes here, "..", ".."); // The variable TwoFolderUP will return a folder UP of the specified path. Example: var OneFolderUP = System.IO.Path.Combine(Game.Folder, "..", "..");
var OneFolderUP = System.IO.Directory.GetParent("SourceDirectory"); // The variable OneFolderUP will return a folder UP of the specified path ("SourceDirectory").

/*
 * 4. CMD Launch Environment Variables (used with CMDBatchBefore and CMDBatchAfter)
 */
%NUCLEUS_EXE% //Exe filename (e.g. Halo.exe).
%NUCLEUS_INST_EXE_FOLDER% //Path the instance exe resides in (e.g. C:\Nucleus\content\Halo\Instance0\Binaries).
%NUCLEUS_INST_FOLDER% //Path of the instance folder (e.g. C:\Nucleus\content\Halo\Instance0\).
%NUCLEUS_FOLDER% //Path where Nucleus Coop is being ran from (e.g. C:\Nucleus).
%NUCLEUS_ORIG_EXE_FOLDER% //Path the original exe resides in (e.g. C:\Program Files\Halo\Binaries).
%NUCLEUS_ORIG_FOLDER% //Path of the "root" original folder (e.g. C:\Proggram Files\Halo).

/*
 * 5. New Player variables
 */
Player.Nickname
Player.HIDDeviceID
Player.RawHID
Player.SID
Player.Adapter
Player.UserProfile
```
