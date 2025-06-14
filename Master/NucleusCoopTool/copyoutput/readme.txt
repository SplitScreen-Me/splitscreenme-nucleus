GameNucleus Co-op - version 2.3.2

Nucleus Co-op is a free and open source tool for Windows that allows split-screen play on many games that do not initially support it, the app purpose is to make it as easy as possible for the average user to play games locally using only one PC and one game copy.

https://github.com/SplitScreen-Me/splitscreenme-nucleus

https://www.splitscreen.me/docs/faq

This is a new and improved official version of the Nucleus Co-op application and is part of the SplitScreen.Me github organization, it includes the following:

1. New and improved options in game handlers:

Note: If you want to create your own handler also read the MasterHandler.js in Nucleus Co-op handlers folder alongside this info.

#################### Game Info ####################

Game.ExecutableName = "Game.exe";                       //The name of the game executable with the extension. This will be used by Nucleus to add games, to run the game and as process to follow for positioning and resizing. 
Game.ExecutableToLaunch = "Game.exe";		        //If the game is to launch a different executable, other than what Game.ExecutableName is.
Game.LauncherExe = "Launcher.exe";                      //If the game needs to go through a launcher before opening Nucleus will use this exe to start the game and it will still use the Game.ExecutableName process for positioning and resizing. 
Game.GUID = "Game Name";                                //The name of the folder that will be created inside the Nucleus content folder (just use letters not symbols). In this folder Nucleus will store the symlinked or copied game files for each instance of the game.
Game.GameName = "Game Name";                            //Title of the game that will be shown in Nucleus.
Game.LauncherTitle = "Launcher Window Title";           //The name of the launcher's window title. Some games need to go through a launcher to open. This is needed or else the application will lose the game's window.
Game.MaxPlayersOneMonitor = 4;                          //This is just info. It will not limit the players number.
Game.MaxPlayers = 16;                                   //This is just the max players info that shows under the handler name in Nucleus UI. Usually we write the max number of players the game supports. (PC, should support 16 max connected input devices).

#################### Mutex ####################

Game.KillMutexType = "Mutant"; 				//Specify the type of mutex/handle to kill, for example "Mutant", "Event" or "Semaphore" | default: "Mutant".
Game.KillMutexDelay = 10; 				//# of seconds to wait before killing the mutex | Only works with killing not renaming | default: 0.
Game.RenameNotKillMutex = true;				//Instead of killing the mutex or handle, rename it on startup to something else | Requires Game.KillMutex to contain the EXACT name of the handles for this to work (no partial).
Game.PartialMutexSearch = false;			//When killing handles, should a partial search be done with the handle name? | Renaming handles requires an exact match | default: false.
Game.KillLastInstanceMutex = false;			//Kill the handles, specified in Game.KillMutex, in the last instance (normally last is ignored).
Game.KillMutexAtEnd = false;				//When using ProcessChangesAtEnd, should handles also be killed at the end?.
Game.KillMutexProcess = ["Mutexes","To","Close"];	//When using Game.MutexProcessExe, here you can specify the handles that need to be killed.
Game.PartialMutexSearchProcess = false;			//When using Game.MutexProcessExe, here you can specify if you want to do a partial search for handles.
Game.KillMutexTypeProcess = "Mutant";			//When using Game.MutexProcessExe, here you can specify the type of handles to kill.
Game.MutexProcessExe = "Process.exe";			//Specify another executable to kill handles for.
Game.PauseBeforeMutexKilling = 1000;			//Wait for X number of seconds before proceeding with mutex killing.
Game.KillMutexDelayProcess = 1000;			//When using Game.MutexProcessExe, wait for X number of seconds before beginning its mutex killing.

#################### File System ####################

Game.SymlinkGame = true;                                //If we should symbolic link the game's files to a temporary directory (Nucleus instances folders in its content folder). If not will launch straight from the installation directory.
Game.SymlinkExe = false;                                //If SymlinkGame is enabled, if we should copy or symlink the game executable. 
Game.SymlinkFolders = false;				//Folders by default are hardcopied, with this enabled, folders will be symlinked instead | warning files placed in symlink folders will appear in the original game files too.
Game.KeepSymLinkOnExit = true; 				//Enable or disable symlink files from being deleted when Nucleus is closed | default: false.
Game.SymlinkFiles = ["game.exe","settings.cfg"];	//Symlink individual files from the game directory to the instanced folder.
Game.CopyFiles = ["game.exe","settings.cfg"];		//Copy files from the game directory to the instanced folder.
Game.HardcopyGame = true;				//Instead of Symlinking, create hard copies of the games files (copy/paste) | Be careful of storage, takes a LONG time to start a game when copying.
Game.HardlinkGame = false;				//Hardlink files instead of Symlink (or hard copy) | Directories will still be symlinked but files will be hardlinked.
Game.ForceSymlink = false;				//Force game to be symlinked each time it is ran.
Game.DirSymlinkExclusions = ["folder1", "folder2"];     //Array with the name of the folders you don't want Nucleus Co-op to symlink, only the folders placed here get hardcopied not the files.
Game.FileSymlinkExclusions = ["file1.txt", "file2.txt"]; //Array with the name of the files you don't want Nucleus Co-op to symlink, useful if you want to replace files or add external files.  
Game.FileSymlinkCopyInstead = ["file1.txt", "file2.txt"]; //Array with the name of the files you want Nucleus Co-op to make full copies of, in some cases games need certain files to be full copies or they won't run. 
Game.DirSymlinkCopyInstead = [ "folder1", "folder2" ];	//Copy (not symlink) all files within a given folder | Folder name is relative from root game folder.
Game.DirSymlinkCopyInsteadIncludeSubFolders = false;	//When specifying folder(s) to copy all its contents instead of linking, should subfolders and files be included as well?
Game.SymlinkFoldersTo = ["folderToMove|whereToMoveIt"]; //Symlink folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.HardlinkFoldersTo = ["folderToMove|whereToMoveIt"];//Hardlink folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.ChangeExe = true;					//Will rename the game's executable to "<exe name> - Player X", x being the instance/player #.
Game.CopyCustomUtils = [ "d3d9.dll" ];			//Copy the a file or folder you specify between the quotes to each instance folder, if the file/folder is located in Nucleus folder\utils\User. This function also accepts two additional parameters, a relative path from the game root folder if the file needs to be placed somewhere else within instance folder and one parameter to indicate which instances to copy the file to if it only needs to be in some. Use of parameters is separated by a | character. So it would look something like this [ "filename.ini|\\bin|1,2" ]. This example would copy filename.ini from Nucleus\utils\User to Instance0\bin and Instance1\bin. If you don't specify which instances, it will do them all by default. If you don't specify a path then root instance folder is used. If you want instances but root folder you would just do [ "filename.ini||1,2" ] | If copying multiple files, use comma seperated strings
Game.HexEditAllExes = [ "before|afters" ];		//Will do a text value replacement in a file for every instance, accepts two parameters, one on what to look for, and what it should be replaced with (seperated with a "|") | Works with multiple values | Works in conjunction with HexEditExe (this will trigger first).
Game.HexEditExe = [ "before|afters" ];			//Will do a text value replacement in a file for a specific instance, accepts two parameters, one on what to look for, and what it should be replaced with (seperated with a "|") | Works with multiple values, each comma seperated string is the order of the instances | Works in conjunction with HexEditAllExe (this will trigger second).
Game.HexEditFile = [ "filePath|before|afters" ]; 	//Works same as HexEditExe function but with a file you specify as an extra parameter (first one) | filePath is relative path from the game root folder.
Game.HexEditAllFiles = [ "filePath|before|afters" ]; 	//Works same as HexEditAllExes function but with a file you specify as an extra parameter (first one) | filePath is relative path from the game root folder.
Game.DirExclusions = ["dir1"];				//Folders (+ all its contents) listed here will not be linked or copied over to Nucleus game content folder, the instance folders.
Game.CreateSteamAppIdByExe = false;			//Create a steam_appid.txt file where the game executable is.
Game.LauncherExeIgnoreFileCheck = false;		//Do not check if Launcher Exe exists in game folder | you will need to provide a relative filepath from game root folder.
Game.CopyEnvFoldersToNucleusAccounts = ["Documents", "AppData"];	//Copy subfolders of current user profile to Nucleus user accounts.
Game.CopyFoldersTo = ["folderToMove|whereToMoveIt"]; 	//Copy folders in game root folder to another spot in game folder | Relative paths from root of game folder.
Game.HexEditExeAddress = [ "1|address|bytes" ];		//Use this to replace bytes in a file at a specified address, can be for specific instances with optional 3rd argument | 1st arg: instance # (optional), 2nd arg: hex address offset, 3rd arg: new bytes.
Game.HexEditFileAddress = [ "1|relativePath|address|bytes" ];	//Same as HexEditExeAddress but for a file other than exe | Need to provide relative path (from game root folder) + filename as 1st or 2nd arg if not specifying an instance.
Game.IgnoreDeleteFilesPrompt = false;			//Do not display the warning message about a file being deleted .
Game.RenameAndOrMoveFiles = [ "1|before.dat|after.dat" ];//Specify files to either rename or move | can accept relative path from root | optional first parameter to specify a specific instance to apply to, omit to do them all.
Game.DeleteFiles = [ "1|delete.dis" ];			//Specify files to be deleted from instanced folder | can accept relative path from root | optional first parameter to specify a specific instance to apply to, omit to do them all.
Game.RunLauncherAndExe = false;				//When using Game.LauncherExe, should ExecutableName also be launched?
Game.ForceLauncherExeIgnoreFileCheck = false;           //Forces LauncherExeIgnoreFileCheck when game isn't symlinked.
Game.BackupFiles = ["file1.txt", "file2.txt"];          //Back up files from the instances folders to Nucleus Co-op environment folder on handler stop, they will be restored on subsequent runs. Works per player/nickname. 
Game.BackupFolders = ["folder1", "folder2"];            //Back up folders from the instances folders to Nucleus Co-op environment folder on handler stop, they will be restored on subsequent runs. Works per player/nickname. 
Game.BinariesFolderPathFix = true;                      //Fixes binaries folder path not being correctly parsed if the binaries folder name exists multiple times in the binaries folder full path.

#################### Nucleus Co-op Environment ####################

Game.UseNucleusEnvironment = false;			//Use custom environment variables for games that use them, replaces some common paths (e.g. AppData) with C:\Users\<your username>\NucleusCoop.
Game.UserProfileConfigPath = "AppData\\Local\\Game\\Config"; //Relative path from user profile (e.g. C:\Users\ZeroFox) to game's config path | Used to provide some extra functionality (open/delete/copy over to Nucleus Environment).
Game.UserProfileSavePath = "AppData\\Local\\Game\\Saves"; //Relative path from user profile (e.g. C:\Users\ZeroFox) to game's save path | Used to provide some extra functionality (open/delete/copy over to Nucleus Environment).
Game.ForceUserProfileConfigCopy = false;		//Force the games config files in UserProfileConfigPath to copy over from system user profile to Nucleus environment.
Game.ForceUserProfileSaveCopy = false;			//Force the games save files in UserProfileSavePath to copy over from system user profile to Nucleus environment.
Game.DeleteFilesInConfigPath = [ "file.del", "me.too" ];//Specify files to delete in Nucleus environment config path (UserProfileConfigPath).
Game.DeleteFilesInSavePath = [ "file.del", "me.too" ];	//Specify files to delete in Nucleus environment save path (UserProfileSavePath).
Game.UserProfileConfigPathNoCopy = false;		//Do not copy files from original UserProfileConfigPath if using Nucleus Environment.
Game.UserProfileSavePathNoCopy = false;			//Do not copy files from original UserProfileSavePath if using Nucleus Environment.
Game.UseCurrentUserEnvironment = false;			//Force the game to use the current user's environment (useful for some games that may require different Window user accounts).
Game.DocumentsConfigPath = "Path\\Here";		//Similar to UserProfileConfigPath, use this when the game uses Documents to store game files.
Game.DocumentsSavePath = "Path\\Here";		        //Similar to UserProfileSavePath, use this when the game uses Documents to store game files.
Game.ForceDocumentsConfigCopy = false;			//When using DocumentsConfigPath, forces a file copy from original location to Nucleus Documents.
Game.ForceDocumentsSaveCopy = false;			//When using DocumentsSavePath, forces a file copy from original location to Nucleus Documents.
Game.DocumentsConfigPathNoCopy = false;			//When using DocumentsConfigPath, do not let Nucleus copy from original location to Nucleus Documents.
Game.DocumentsSavePathNoCopy = false;			//When using DocumentsSavePath, do not let Nucleus copy from original location to Nucleus Documents.
Game.ForceEnvironmentUse = true;                        //Forces use of custom environment variable when Game.ThirdPartyLaunch = true;

#################### Focus ####################

Game.FakeFocus = true;					//Enable or disable the sending of focus messages to each game window at a regular interval | default: false.
Game.HookFocus = true;					//Enable or disable hooks to trick the game into thinking it has focus | default: false.
Game.HookInit = true;					//Enable or disable hooks of functions some games may try and use to prevent multiple instances from running | default: false.
Game.PreventWindowDeactivation = false;			//Blocks the processing of all the windows messages that get sent when the window loses focus.
Game.HasDynamicWindowTitle = false;			//Work-around for ForceFocusWindowName having to match 1:1 with game window title for resizing, positioning and focus | default: false.
Game.ForceWindowTitle = false;				//Forces the game window title to be whatever is specified in Game.Hook.ForceFocusWindowName (triggers once, after all instances have started) | default: false.
Game.IdInWindowTitle = true;				//Adds the process ID to the end of the window title.
Game.SetForegroundWindowElsewhere = false;		//Set the foreground window to be something other than game windows.
Game.StartHookInstances = "1,2,3,4";			//If you only want specific instances to have starting hooks, specify them in a comma seperated string.
Game.PostHookInstances = "1,2,3,4";			//If you only want specific instances to have post launch hooks, specify them in a comma seperated string.
Game.FakeFocusInstances = "1,2,3,4";			//If you only want specific instances to have fake focus messages sent to, specify them in a comma seperated string.
Game.KeyboardPlayerSkipFakeFocus = false;		//Should the keyboard player instance be skipped when fake focus messages are being sent to.
Game.KeyboardPlayerSkipPreventWindowDeactivate = false; //Ignore PreventWindowDeactivation if player is using keyboard.
Game.FakeFocusSendActivate = true;			//Should WM_ACTIVATE message be sent to each instance? | default: true.
Game.PreventGameFocus = false;				//Makes sure all the game windows are unfocused so nothing received double input from Windows.
Game.FakeFocusInterval = 1000; 				//The milliseconds between sending fake focus messages. Default at 1000, some rare games need this to be very low.
Game.EnableWindows = false;				//Enable each game window at the end (useful if became disabled, or for some games that require this Window function to be called to display properly, after Nucleus setup).
Game.ProcessChangesAtEnd = false;			//Do the resizing, repositioning and post-launch hooking of all game instances at the very end | will not work with every option ran normally.
Game.PromptProcessChangesAtEnd = false;			//If ProcessChangesAtEnd = true, pause and show a prompt, before making changes to processes.
Game.PromptBetweenInstancesEnd = false;			//If ProcessChangesAtEnd = true, show a prompt between each instance being changed.

#################### Window manipulation ####################

Game.SetWindowHook = true;				//Prevent games from resizing their windows on their own | Hooks after all instances have been opened (see Game.SetWindowHookStart for an alternative).
Game.SetWindowHookStart = false;			//Prevent games from resizing window their windows on their own | Hooks upon game starting up (see Game.SetWindowHook for an alternative).
Game.Hook.FixResolution = false;			//Should the custom dll do the resizing? | Only works with Alpha 10 custom dll | default: false.
Game.Hook.FixPosition = false;				//Should the custom dll do the repositioning? | Only works with Alpha 10 custom dll | default: false.
Game.Hook.WindowX = 0;				        //If manual positioning, what is the window's X coordinate | If both X and Y value > 0, window will be positioned manually.
Game.Hook.WindowY = 0;				        //If manual positioning, what is the window's Y coordinate | If both X and Y value > 0, window will be positioned manually.
Game.Hook.ResWidth = 1280; 				//If manual resizing, what is the window's width | If both ResWidth and ResHeight value > 0, window will be positioned manually.
Game.Hook.ResHeight = 720;				//If manual resizing, what is the window's height | If both ResWidth and ResHeight value > 0, window will be positioned manually.
Game.KeepAspectRatio = false;				//Should the game window try and keep it's aspect ratio when being resized? | default: false.
Game.ResetWindows = false;				//After each new instance opens, resize, reposition and remove borders of the previous instance.
Game.KeepMonitorAspectRatio = false;			//Try and resize game window within player bounds to the aspect ratio of the monitor.
Game.DontResize = false;				//Should Nucleus not resize the game windows?.
Game.DontReposition = false;				//Should Nucleus not reposition the game windows?.
Game.NotTopMost = false;				//Should Nucleus not make the game windows top most (appear above everything else).
Game.WindowStyleValues = [ "~0xC00000", "0x8000000" ];	//Override Nucleus' default of removing borders and specify a custom window style | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.ExtWindowStyleValues = [ "~0x200", "0x200000" ];	//Override Nucleus' default of removing borders and specify a custom extended window style | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.RefreshWindowAfterStart = false;			//Should each game window be minimized and restored once all instances are opened?.
Game.WindowStyleEndChanges = [ "~0xC00000", "0x8000000" ];	//Override Nucleus' default of removing borders and specify a custom window style during end processing | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.ExtWindowStyleEndChanges = [ "~0xC00000", "0x8000000" ]; 	//Override Nucleus' default of removing borders and specify a custom window style during end processing | Start string with ~ to remove a style, or don't use it to add one | See https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles for values that can be used.
Game.IgnoreWindowBordercheck = false;			//Ignore logic at end to check if any game window still has a border.
Game.DontRemoveBorders = false;				//Prevents Nucleus from removing game window borders.
Game.SetTopMostAtEnd = true;                            //Set the game windows to top most at the very end.
Game.ToggleUnfocusOnInputsLock = true;                  //Automatically unfocus the game windows (gives the focus to the Nucleus window). Works with "Game.LockInputAtStart = true;" too.

#################### Input ####################

Game.SupportsKeyboard = true;				//Enable/disable use of one keyboard/mouse player.
Game.XInputPlusDll = ["xinput1_3.dll"];		        //Set up XInputPlus | If multiple dlls required, use comma seperated strings.
Game.KeyboardPlayerFirst = true;			//If the keyboard player should be processed first.
Game.UseX360ce = true;					//Before launching any games, Nucleus Co-op will open x360ce and let the user set up their controllers before continuing | close x360ce to continue | Don't use with custom dlls.
Game.Hook.UseAlpha8CustomDll = false;			//Use the xinput custom dll from Alpha 8 instead of Alpha 10 | Will still force alpha 10 custom dll if game is x64.
Game.PlayersPerInstance = 2;				//If using XInputPlus or X360ce and there are multiple players playing the same instance, set the # per instance here.
Game.UseDevReorder = true;				//Set up Devreorder.
Game.XboxOneControllerFix = false;			//When using x360ce, this will set certain hooktype that may work for xbox one controllers if the normal method does not work.
Game.BlockRawInput = false;				//Disable raw input devices in game | default: false.
Game.X360ceDll = [ "xinput1_3.dll" ];			//If x360ce dll should be named something OTHER than xinput1_3.dll | requires Game.Usex360ce to be set to true.
Game.CreateSingleDeviceFile = false;			//Create only one file for HID devices per instance (the assigned HID device).
Game.Hook.EnableMKBInput = false;			//Enable Mouse/Keyboard input for instances when using Alpha 10 custom xinput dll (normally MKB is restricted).	
Game.UseDInputBlocker = false;				//Setup wizark952's dinput blocker (block dinput for the game).
Game.XInputPlusNoIni = false;				//Do not copy XInputPlus' ini when using Game.XInputPlusDll.
Game.XInputPlusOldDll = false;				//When using Game.XInputPlusDll, you can specify to use the previous version instead of latest (needed for some games).

#################### Goldberg Emulator ####################

Game.UseGoldberg = false;				//Use the built-in Goldberg features in Nucleus | default: false.
Game.GoldbergExperimental = false;			//Use the experimental branch of Goldberg | Requires `Game.UseGoldberg = true` | default: false.
Game.GoldbergExperimentalSteamClient = false;		//Automatically setup Goldberg's Experimental Steam Client | Requires Game.UseGoldberg and the original steam_api.dll.
Game.GoldbergLobbyConnect = false;			//Should Goldberg Lobby Connect be used to connect the instances.
Game.GoldbergNoLocalSave = false;			//Do not create a local_save.txt file for Goldberg, saves are to use default game save location.
Game.GoldbergNeedSteamInterface = false;		//Some older games require a steam_interfaces.txt file for Goldberg to work | Will first search orig game path and nucleus games path, if not found then tries to create one with the GoldbergNeedSteamInterface command.
Game.GoldbergLanguage = "english";			//Manually specify what language you want Goldberg to use for the game | by default, Goldberg will use the language you chose in Steam.
Game.OrigSteamDllPath = "C:\full path\steam_api.dll";	//If steam_interface.txt is required, provide full path here to the original steam_api(64).dll and Nucleus will create one if it can't find an existing copy.
Game.GoldbergIgnoreSteamAppId = false;			//When set to true, Goldberg will not create a steam_appid.txt file.
Game.PlayerSteamIDs = ["76561198134585131","76561198134585132"]; //A list of steam IDs to be used instead of the pre-defined ones Nucleuses uses | IDs will be used in order they are placed, i.e. instance 1 will be first non-empty string in array.
Game.GoldbergExperimentalRename = false;		//Set to true to have Goldberg Experimental rename instanced steam_api(64).dll to cracksteam_api(64).dll.
Game.GoldbergWriteSteamIDAndAccount = false;		//Force Goldberg to write account_name.txt and user_steam_id.txt | Requires Game.UseGoldberg;
Game.GoldbergNoWarning = true;                          //Nucleus will not prompt if the Goldberg dlls are missing if the handler supports different platforms.
Game.UseHandlerSteamIds = true;                         //Player Steam ids from a handler will have priority over the default or customized settings player Steam ids.
Game.UseGoldbergNoOGSteamDlls = true;                   //Use this instead of Game.UseGoldberg if the main game files do not have any steam_api dlls.
Game.CustomSteamApiDllPath = ["steam_api64.dll", "steam_api.dll"]; //Specify the path from the root of the instance folder where you want goldberg to apply, use this with Game.UseGoldbergNoOGSteamDlls.

#################### Smart Steam Emulator ###################

Game.NeedsSteamEmulation = false;                       //If the game needs a fake Steam wrapper (SSE) to run multiple instances (this is not needed if you are using the new use Goldberg emulator line).
Game.SSEAdditionalLines = ["Section|Key=Value"];	//When using Game.NeedsSteamEmulation, here you can provide additional lines to write to the SSE ini file.

#################### Nemirtingas Epic Emulator ####################

Game.UseNemirtingasEpicEmu = false;			//Automatically set up Nemirtinga's Epic Emulator in Nucleus
Game.EpicEmuArgs = false;				//When using Nemirtinga's Epic Emulator, use pre-defined parameters -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal -epicusername=\"" + <Player Nickname here> + "\" -epicuserid=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA -epiclocale=en"
Game.AltEpicEmuArgs = false;                            //Optional. When using Nemirtinga's Epic Emulator, use pre-defined parameters + Set NickName as epic id, only to use with games that do not use epic id to start or connect(Set clever save names if the game use the epic id to name saves ex: Tiny Tina's Assault On Dragon Keep)" -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal -epicusername=" + <Player Nickname here> + " -epicuserid="+ <Player Nickname here> + "-epiclocale=" + EpicLang".

NemirtingasEpicEmu.json edition example:

    //If you don't need to edit a line do not add it here, the emu will automatically write it with default parameters. More info here: https://gitlab.com/Nemirtingas/nemirtingas_epic_emu/-/blob/master/README.md
    //Available debug parameters,  should be "off" by default. Only required to debug the nermintingas eos emulator.
    //TRACE: Very verbose, will log DEBUG + All functions enter
    //DEBUG: Very verbose, will log INFO  + Debug infos like function parameters
    //INFO : verbose     , will log WARN  + some informations about code execution and TODOs
    //WARN : not verbose , will log ERR   + some warnings about code execution
    //ERR  : not verbose , will log FATAL + errors about code execution
    //FATAL: not verbose , will log only Fatal errors like unimplemented steam_api versions
    //OFF  : no logs     , saves cpu usage when running the debug versions 
    //In case of using custom start arguments => -epicusername == same username as in the.json => -epicuserid == same epicid as in the.json >
	
    var jsonPath = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\nepice_settings\\NemirtingasEpicEmu.json";// + "\\NemirtingasEpicEmus.json" for stable older epic emu version.
    var params = [
    '{',
    '  "appid": "InvalidAppid",',
    '  "disable_online_networking": false,',
    '  "enable_lan": true,',
    '  "enable_overlay": true,',
    //'  "epicid": "3808a45790894253344fec21026bbf80",', //better to let the emu automatically add this line.
    '  "language":' + '"' + Context.EpicLang + '"' + ',',
    '  "log_level": "off",',
    //'  "productuserid": "ab65359ffde1b5cc41e81afee8e32c33",', //better to let the emu automatically add this line.
    '  "savepath": "appdata",',
    '  "signaling_servers": [],',
    '  "unlock_dlcs": true,',
    '  "username": ' + '"' + Context.Nickname + '"', //must always be added if you edit the json and must be the last line else the emulator will reset all parameters (there is no coma at the end of this line in the json).
    '}'
    ] ;
    Context.WriteTextFile(jsonPath,params);	

#################### Nemirtingas Galaxy GoG Emulator ####################

Game.UseNemirtingasGalaxyEmu = false;			//Automatically set up Nemirtinga's Galaxy Emulator in Nucleus.

NemirtingasGalxyEmu.json edition example:

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

#################### Additional Tools ####################

Game.SteamlessPatch = ["false", "--quiet --keepbind", "2500"]; //Use atom0s' Steamless app to remove Steam Stub DRM from a protected executable. string[patch launcher(only), steamless arguments, patch timing].
Game.UseSteamStubDRMPatcher = false;			//Use UberPsyX's Steam Stub DRM Patcher to remove Steam Stub DRM from a protected executable.
Game.SteamStubDRMPatcherArch = "64";			//Force Steam Stub DRM Patcher to use either the x64 or x86 dll | Values: "64" or "86".
Game.UseEACBypass = false;				//Replace any EasyAntiCheat_(x86)(x64).dll with a bypass dll.
Game.FlawlessWidescreen = "FWGameName";			//Use Flawless Widescreen application | value must be the name of the game plugin folder in PluginCache\FWS_Plugins\Modules.
Game.FlawlessWidescreenOverrideDisplay = false; 	//(undocumented)
Game.FlawlessWidescreenPluginPath 			//(undocumented)

#################### Network ####################

Game.UseForceBindIP = false;				//Set up game instances with ForceBindIP; each instance having their own IP.
Game.ForceBindIPNoDummy = false;                        //ForceBindIP will be used without the "dummy" launch argument, the argument prevents crashes but it causes issues in other games.
Game.ChangeIPPerInstance = false;			//Highly experimental feature, will change your existing network to a static IP for each instance | option in settings to choose your network.
Game.ChangeIPPerInstanceAlt;				//An alternative method to changing IP per instance | this method will create loopback adapters for each player and assign them a static ip on the same subnet mask as your main network interface.

#################### Extra ####################

Game.PauseBetweenProcessGrab = 30;			//How many seconds to wait after launching game (or launcher) but before grabbing game process.
Game.PauseBetweenContextAndLaunch = 0;			//Number of seconds to wait after running additional files but before continuing with player setup.
Game.ThirdPartyLaunch = false;				//Use if the game is launched outside of Nucleus | NOTE: You will not be able to use start up hooks or CMDLaunch with this option.
Game.IgnoreThirdPartyPrompt = false;			//Ignore the prompt that appears when using Game.ThirdPartyLaunch;
Game.ForceProcessPick = false;				//Manually select the process that will be used for process manipulation, such as resizing, repositioning and used for post-launch hooks.
Game.PromptBetweenInstances = true;			//Prompt the user with a messagebox to let the user decide when to open the next instance | default: false, PauseBetweenStarts STILL required.
Game.PromptAfterFirstInstance = false;			//Show a prompt that user must click on ok to continue, after the first instance is setup.
Game.GamePlayAfterLaunch;				//Call the Game.Play function after the call has launched.
Game.GamePlayBeforeGameSetup = false;			//Execute Game.Play function (context) before the majority of the game is setup.
Game.LaunchAsDifferentUsers = false;			//Launch each instance from a different user account | must run Nucleus as admin | will temporary create user accounts "nucleusplayerx" and delete them when closing Nucleus.
Game.LaunchAsDifferentUsersAlt = false;			//An alternative method to launch each instance from a different user account | must run Nucleus as admin | will temporary create user accounts "nucleusplayerx" and delete them when closing Nucleus.
Game.TransferNucleusUserAccountProfiles = false;	//Will backup and restore Nucleus user account user profile's on windows between sessions (when user accounts are not kept).
Game.ForceProcessSearch = false;			//Force Nucleus to search for the game process.
Game.RequiresAdmin = false;				//Game requires Nucleus to be run as administrator (this option will check and advise if detected not running Nucleus as admin).
Game.WriteToProcessMemory 				//(undocumented)
Game.ProcessorPriorityClass = "Normal";			//Sets the overall priority category for the associated process. Can be "AboveNormal", "High" or "RealTime" | default: "Normal".
Game.UseProcessor = "1,2,3,4";				//Sets the processors on which the game's threads can run on (provide the # of the cores in a comma-seperated list in quotations) | default: system delegation.
Game.UseProcessorsPerInstance = [ "1,2","3,4" ];	//Sets the processors on which an instances game's threads can run on (provide the # of the cores in a comma-seperated list in quotations) | default: system delegation.
Game.IdealProcessor = 2;				//Sets the preferred processor for the game's threads, used when the system schedules threads, to determine which processor to run the thread on | default: system delegation.
Game.HideCursor = true;					//Hide the mouse cursor in game instances.
Game.HideDesktop = false;				//Hide the desktop background with a solid black background.
Game.HideTaskbar = true;				//Most games hide the taskbar when placed on-top but for games that don't you can use this.
Game.Description = "Hello World";			//Display a message to the end-user, that will appear when user selects the game of a handler| useful if there is anything the end-user needs to know before-hand | Only first two or three sentences will appear in UI, but full message can be viewed if user right clicks on the game in the list.
Game.KillProcessesOnClose = [ "kill", "me" ];		//List of additional processes that need to be killed (other than executable and launcher).
Game.DeleteOnClose = ["DeleteThis.exe"]; 		//Delete a file upon ending game session | Relative paths from root of game folder.
Game.UseDirectX9Wrapper = false;			//Use a Direct X wrapper to try and force DirectX 9 games to run windowed.
Game.CMDLaunch = false;					//Launch a game using command prompt.
Game.CMDOptions = ["ops1","ops2"];			//Specify command line options if game is launched using command prompt | requires CMDLaunch to be true, each element is for a different instance.
Game.CMDBatchBefore = [ "0|ops1", "1|ops2" ];		//When using CMDLaunch (or UseForceBindIP), specify command lines to run BEFORE the game launches in same cmd session, syntax is instance and | symbol as a delimiter and the line you want that instance to run. If you want same line to run all instances, leave out the # and | (only write the line in quotes).
Game.CMDBatchAfter = [ "0|ops1", "1|ops2" ]; 		//When using CMDLaunch (or UseForceBindIP), specify command lines to run AFTER the game launches in same cmd session, syntax is instance and | symbol as a delimiter and the line you want that instance to run. If you want same line to run all instances, leave out the # and | (only write the line in quotes).
Game.PauseCMDBatchBefore = 10;				//Wait for X number of seconds before proceeding with CMDBatchBefore.
Game.PauseCMDBatchAfter = 10;				//Wait for X number of seconds before proceeding with CMDBatchAfter.
Game.CMDBatchClose ["cmd1", "cmd2"];			//Run command lines upon exiting Nucleus.
Game.CMDStartArgsInside = false;			//When using CMDLaunch, should the game's starting arguments be inside the same quotations as the game path?
Game.ForceGameArch = "x86" (or "x64");			//Force Nucleus to treat the game as 32 or 64-bit architecture.
Game.SplitDivCompatibility = false;                     //Explicitly disable splitscreen divisons if the game is known to be imcompatible with it. Does not require to be true for compatible games. Default = true.
Game.CustomHotkeys = ["Ctrl|K","Alt|F5","Ctrl|X"]       //All available keys that can be passed after Alt, Ctrl and Shift.

  Game.OnCustomHotKey_1 = function () {
  var outPut = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\OnCustomHotKey_1.txt";
  var data = ["Created with custom hotkey 1"];
  Context.WriteTextFile(outPut, data);
  }
  Game.OnCustomHotKey_2 = function () {
  var outPut = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\OnCustomHotKey_2.txt";
  var data = ["Created with custom hotkey 2"];
  Context.WriteTextFile(outPut, data);
  }
  Game.OnCustomHotKey_3 = function () {
  var outPut = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\OnCustomHotKey_3.txt";
  var data = ["Created with custom hotkey 3"];
  Context.WriteTextFile(outPut, data);
  }

~ Custom prompts - Prompt user for input, which can be then used in handlers logic

Game.CustomUserGeneralPrompts = ["Enter ROM name", "Enter filename"];	//This prompts user one time and applies to ALL players, unless a value text file already exists and saving is on.
Game.SaveCustomUserGeneralValues = false;
Game.SaveAndEditCustomUserGeneralValues = false;

Game.CustomUserPlayerPrompts = ["Enter network Adapter name", "Enter character name"];	//This will prompt each player, unless a value text file already exists for that player and saving for players is on.
Game.SaveCustomUserPlayerValues = false;
Game.SaveAndEditCustomUserPlayerValues = false;

Game.CustomUserInstancePrompts = ["Enter network Adapter name"];	//This will prompt each instance, unless a value text file already exists for that instance and save is on. In case it is not player specific but different values are needed for instances.
Game.SaveCustomUserInstanceValues = false;
Game.SaveAndEditCustomUserInstanceValues = false;
Access the user input values via Context.CustomUser(General/Player/Instance)Values[index]

2. Support for multiple mice and keyboards (These are deprecated, see the new Proto Input guide: https://www.splitscreen.me/docs/proto)

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
Game.LockInputToggleKey = 0x23;				//See https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
Game.HookReRegisterRawInput = false;			//Re-register raw input from directly within game process | Recommended to disable forwarding input while using this
Game.HookReRegisterRawInputMouse = true;		
Game.HookReRegisterRawInputKeyboard = true;
Game.UpdateFakeMouseWithInternalInput = false;

3. New methods to be used in game handlers -

#################### Path variables ####################

Context.RootInstallFolder                                   //Path to the source game root folder.
Context.RootFolder                                          //Path to the Nucleus Co-op instances folders.
Context.NucleusFolder                                       //Path to the Nucleus Co-op root folder (Nucleus-Coop\).
Context.ScriptFolder					    //Path to the game handler folder (Nucleus-Coop\handlers\handler_folder).
Game.Folder                                                 //Path to the game handler folder (Nucleus-Coop\handlers\handler_folder).
Context.GetFolder(Nucleus.Folder.InstancedGameFolder)       //Path to the Nucleus Co-op current instance root folder (Nucleus-Coop\content\GUID\Instance#).
Context.EnvironmentPlayer				    //Path to current players Nucleus environment.
Context.EnvironmentRoot					    //Path to Nucleus environment root folder.
Context.UserProfileConfigPath				    //Relative path from user profile to game's config path | requires Game.UserProfileConfigPath be set.
Context.UserProfileSavePath				    //Relative path from user profile to game's save path | requires Game.UserProfileSavePath be set.
Context.DocumentsPlayer                                     //Path to current players Nucleus documents environment.
Context.DocumentsRoot                                       //Path to Nucleus documents environment root folder.
Context.DocumentsConfigPath                                 //Relative path from user profile to game's config path | requires Game.DocumentsConfigPath be set.
Context.DocumentsSavePath                                   //Relative path from user profile to game's save path | requires Game.DocumentsSavePath be set.
Context.NucleusUserRoot                                     //Path to current players Nucleus Windows User root folder.
Context.HandlersFolder                                      //Path to the Nucleus Co-op handlers root folder (Nucleus-Coop\handlers).
Context.UserName                                            //Current Windows username without the path.

Context.ChangeXmlNodeInnerTextValue(string path, string nodeName, string newValue)		//Edit an XML element (previously only nodes and attributes).
Context.ReplaceLinesInTextFile(string path, string[] lineNumAndnewValues)			//Replace an entire line; for string array use the format: "lineNum|newValue", the | is required.
Context.ReplacePartialLinesInTextFile(string path, string[] lineNumRegPtrnAndNewValues)		//Partially replace a line; for string array use the format: "lineNum|Regex pattern|newValue", the | is required.
Context.RemoveLineInTextFile(string path, int lineNum)						//Removes a given line number completely.
Context.RemoveLineInTextFile(string path, string txtInLine, SearchType type)			//Removes a given line number completely.
Context.FindLineNumberInTextFile(string path, string searchValue, SearchType type)		//Returns a line number (int), utilizes a newly created enum SearchType.
Each of the above methods, also have an overload method so you can specify a kind of encoding to use (enter string of encoding as last parameter, e.g. "utf-8", "utf-16", "us-ascii").
- SearchTypes include: "Contains", "Full" and "StartsWith", use like so: Nucleus.SearchType.StartsWith.

Context.CreateRegKey(string baseKey, string sKey, string subKey)				//Create a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER".
Context.DeleteRegKey(string baseKey, string sKey, string subKey)				//Delete a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER".
Context.EditRegKey(string baseKey, string sKey, string name, object data, RegType type)	//Edit a registry key for current user, baseKey can either be "HKEY_LOCAL_MACHINE" or "HKEY_CURRENT_USER".
	EditRegKey uses a custom registry data type to use, by using Nucleus.RegType.DWord for example. The last word can be of the same name of RegistryValueKind enum.
Context.EditRegKeyNoBackup                                                                      //Edit a registry key for current user without Nucleus creating a backup of the registry or before it creates one. Must be placed before any "Context.EditRegKey" line in handler for it to work.
Context.Nickname										//Use this in a game handler to get the player's nickname
Context.EpicLang                                                                                //Can be use to edit NemirtingasEpicEmu.json  //Can be use in start argument to setup user Epic language parameter //ex: Context.StartArguments = ' -AlwaysFocus -nosplash -nomoviestartup -nomouse' + Context.EpicLang; (if Epic Language is set to "en" return => "-epiclocale=en", Should not be necessary in most cases)
Context.GamepadGuid                                                                             //Get the raw gamepad guid
Context.GogLang                                                                                 //Can be use to edit NemirtingasGalaxyEmu.json from handlers
Context.GamepadId										//Useful to make sure that controllers are correctly assigned to the player they are meant to be assigned.
Context.x360ceGamepadGuid									//Get the x360ce formatted gamepad guid.
Context.LocalIP											//Local IP address of the computer.
Context.KillProcessesMatchingProcessName(string name)						//Kill processes matching a given process name during preperation.
Context.KillProcessesMatchingWindowName(string name)						//Kill processes matching a given window name during preperation.
Context.IsKeyboardPlayer									//True if current player is using keyboard, useful for logic that is needed for keyboard only player.
Context.OrigHeight										//Player monitor's height.
Context.OrigWidth										//Player monitor's width.
Context.OrigAspectRatio										//Player monitor's aspect ratio (e.g. 16:9).
Context.OrigAspectRatioDecimal									//Player monitor's aspect ratio in decimal (e.g. 1.777777).
Context.AspectRatio										//Player's aspect ratio (e.g. 16:9).
Context.AspectRatioDecimal									//Player's aspect ratio in decimal (e.g. 1777777).
Context.FindFiles(string rootFolder, string fileName)						//Returns a string array of filenames (and their paths) found that match a pattern you specify.
Context.CreatedDate(string file, int year, int month, int day)					//Change the creation date of a file.
Context.ModifiedDate(string file, int year, int month, int day)					//Change the last modified date of a file.
Context.RunAdditionalFiles(string[] filePaths, bool changeWorkingDir, string customText, int secondsToPauseInbetween, bool showFilePath, bool runAsAdmin, bool promptBetween,bool confirm)	//Specify additional files to run before launching game. By default will run each additional file once but can specify to run during specific player's instances by prefixing the filepath with #|. Replace # with player number. Can also specify to run files for each player by prefixing filepath with "all|". "bool confirm" will only run the file after clicking "Ok" in the prompt.
Context.ReadRegKey(string baseKey, string sKey, string subKey)					//Returns the value of a provided key as a string.
Context.HandlerGUID                                                                             //Returns Game.GUID.
Context.StartArguments = "";                                                                    //Adds whatever you put into the field as starting parameters for the game's executable in context. For example, in most cases '-windowed' will force windowed mode. Parameters can be chained.
Context.ProceedSymlink();                                                                       //To use with Game.SymlinkFiles = ["file"]; Can now symlink files inside "Game.Play = function() {}".
Context.HideDesktop();                                                                          //Same as "Game.HideDesktop = true;" but usable inside "Game.Play = function() {}".
Context.HideTaskBar();                                                                          //Same as "Game.HideTaskbar = true;" but usable inside "Game.Play = function() {}".
Context.Wait(int milliseconds);                                                                 //Time to wait between two functions inside "Game.Play = function() {}".
Context.NumberOfPlayers                                                                         //Returns the amount of players (players to proceed). 
Context.HexEdit(string fileToEdit, string address, byte[] newBytes)                             //Hex edit the file specified at fileToEdit (must be the full path, it's not relative to any Nucleus folder, can be anywhere).
Context.PatchFileFindAll(string originalFile, string patchedFileDestination, byte[] patchFind, byte[] patchReplace) //Replace all patchFind bytes found in the file stream by patchReplace bytes (must be full paths, it's not relative to any Nucleus folder).                                                                       
Context.MoveFolder(string sourceDirName, string destDirName)                                    //Move a folder to another location (both instanced game folder relative).
Context.CopyScriptFolder(string DestinationPath)                                                //Copy the whole handler folder inside Nucleus handlers folder to a specific path you choose, destination can be anywhere.
Context.RandomInt(int min, int max)                                                             //Generate a random int between min and max (min/max included).
Context.RandomString(int size, bool lowerCase = false)                                          //Generate a random string of the specified lenght.                                                                
Context.ConvertToInt(value)                                                                     //Convert a string, float, long to an integer.
Context.ConvertToString(object str)                                                             //Convert the object param (float, int, long, etc.) to a string.
Context.ConvertToBytes(value)                                                                   //Convert an int, float or string to a byte array.
Context.GCD(int a, int b)                                                                       //Returns the greatest common divisor (GCD), int.
Context.Arch                                                                                    //Returns the game default architecture (x86/x64) or the Game.ForceGameArch value (string).
Context.PosX                                                                                    //Returns current processed player X window location (int). 
Context.PosY                                                                                    //Returns current processed player Y window location (int). 
Context.MonitorWidth                                                                            //Returns current processed player Monitor Width (int). 
Context.MonitorHeight                                                                           //Returns current processed player Monitor Height (int). 
Context.Log(value)                                                                              //Write custom message to debug-log.txt => Context.Log(string msg); Context.Log(string msg, string val); Context.Log(string msg, int val); Context.Log(string msg, double val); Context.Log(string msg, string[] vals);
Context.ProcessID                                                                               //Returns current processed player attached process id (int). 
Context.HasKeyboardPlayer                                                                       //True if the current setup has at least one k&m player (bool).              
Context.StartProcess(string exePath, string startArgs, bool asAdmin);                           //Start the exe at exePath.                                                         
Context.EditTextFile(string filePath, string[] refLines, string[] newLines, string encoder);    //refLines are partial or full strings to look for. NewLines are new strings (full strings) that will replace refLines. Adding "Delete" as newLine will delete the corresponding reflLine from the file. Should work with any type of text files (xml, ini, txt). 
Context.PlayerSteamID                                                                           //Can get players steam ids from Game.Play.
Context.FindFilePartialName (string sourceFolder, string[] partialNames);                       //Find files in "sourceFolder" using partial file name. Useful if a full file name is not predictable but has some static patterns. 
Context.GetFileName(string fullFilePath);                                                       //Get the name of a file from its full path;
Context.BackupFiles = ["file1.txt", "file2.txt"];                                               //Back ups files on handler stop. They will be restored on subsequent runs.
Context.BackupFolders = ["folder1", "folder2"];                                                 //Back ups folders on handler stop. They will be restored on subsequent runs.
Context.ToUpperCase                                                                             //Convert string to uppercase. 
Context.ToLowerCase                                                                             //Convert string to lowercase. 
Context.CopyFolder(string sourcePath,string destinationPath)                                    //Copy a full folder, example => Context.CopyFolder(Context.GetFolder(Nucleus.Folder.InstancedGameFolder) +"\\Engine", Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\Citadel\\Engine");
Context.DeleteRegKeyValues(string baseKey, string sKey, string[] values);                       //Where values is a string array of the value(s) you want to delete from a register key.
Context.ExtractZip(string sourceZip, string contentDestination,string password);
Context.EditZipFile(string sourceZip, string password, string savePath, string[] itemsToAdd, string[] entriesToRemove); //itemsToAdd => ["path where to copy in the zip|path of file/folder to add"];
                                                                                                                        //Always add "\\" add the end off the path if the content to remove is a folder;
Example :

   var zipSourcePath = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\MyZip.zip";
   var zipDestinationPath = Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\MyNewZip.zip";
   var zipPassword = "";
   var addToZip = [ "New folder1\\Engine|" + Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\Engine", 
                    "New folder2|" + Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\Manifest_NonUFSFiles_Win64.txt"];//can a be folder(recursive) or a single file
   var deleteFromZip =  ["folder\\sub1.txt","folder\\sub2.txt","2.txt","folder\\"];

   Context.EditZipFile(zipSourcePath ,zipPassword ,zipDestinationPath , addToZip, deleteFromZip);

Context.SearchSubFolder(string parentDirectory, string searchPattern);                           //Returns the full path of the found directory matching the search pattern.
Context.SearchSubFolders(string parentDirectory, string searchPattern);                          //Returns an array of all found directories matching the search pattern
Context.NoDPIHandlingWidth                                                                       //Returns game window size with no DPI calculation.
Context.NoDPIHandlingHeight                                                                      //Returns game window size with no DPI calculation.
Context.SteamLang                                                                                //Returns user Steam language settings choice.
Context.PlayerGameFilesBackupFolder                                                              //Returns C:\Users\UserName\NucleusCoop\_Game Files Backup_\GameGUID\PlayerNickname\.

#################### Other useful lines ####################

System.IO.File.Delete("FileToDelete");                      //Delete the specified file.
System.IO.File.Copy("SourceFile", "DestinationFile", true); //Copy the specified "SourceFile" to the "DestinationFile" path. You can uses a different name on the DestinationFile to rename the file. The true in the end specify to overwrite, if found, the existing file with the same name.
System.IO.File.Move("SourceFile", "DestinationFile", true); //Move the specified "SourceFile" to the "DestinationFile" path. You can uses a different name on the DestinationFile to rename the file. The true in the end specify to overwrite, if found, the existing file with the same name.
var OneFolderUP = System.IO.Path.Combine(Path goes here, ".."); // The variable OneFolderUP will return a folder UP of the specified path. Example: var OneFolderUP = System.IO.Path.Combine(Game.Folder, "..");
var TwoFolderUP = System.IO.Path.Combine(Path goes here, "..", ".."); // The variable TwoFolderUP will return a folder UP of the specified path. Example: var OneFolderUP = System.IO.Path.Combine(Game.Folder, "..", "..");
var OneFolderUP = System.IO.Directory.GetParent("SourceDirectory"); // The variable OneFolderUP will return a folder UP of the specified path ("SourceDirectory").

4. CMD Launch Environment Variables (used with CMDBatchBefore and CMDBatchAfter)
%NUCLEUS_EXE% 			= Exe filename (e.g. Halo.exe).
%NUCLEUS_INST_EXE_FOLDER% 	= Path the instance exe resides in (e.g. C:\Nucleus\content\Halo\Instance0\Binaries).
%NUCLEUS_INST_FOLDER% 		= Path of the instance folder (e.g. C:\Nucleus\content\Halo\Instance0\).
%NUCLEUS_FOLDER%		= Path where Nucleus Coop is being ran from (e.g. C:\Nucleus).
%NUCLEUS_ORIG_EXE_FOLDER%	= Path the original exe resides in (e.g. C:\Program Files\Halo\Binaries).
%NUCLEUS_ORIG_FOLDER%		= Path of the "root" original folder (e.g. C:\Proggram Files\Halo).

5. New Player variables
Player.Nickname
Player.HIDDeviceID
Player.RawHID
Player.SID
Player.Adapter
Player.UserProfile

--------------------------------------------------------------------------------------
Known Issues:

- Force feedback does not work with the classic Nucleus xinput custom dlls.
- PreventWindowDeactivation will prevent mouse and keyboard input on instances using this hook (and may have other adverse effects).

--------------------------------------------------------------------------------------
Changelog: 

v2.3.2 - November 12, 2024

-Fixed audio routing feature not working on Windows 10.
-Fixed osd messages not disappearing for some users.
-Fixed a bug where handler data was not correctly refreshed after a handler update and the game was selected.
-Added Context.PlayerGameFilesBackupFolder returns C:\Users\UserName\NucleusCoop\_Game Files Backup_\GameGUID\PlayerNickname\.
-Added Game.UseGoldbergNoOGSteamDlls = true; and Game.CustomSteamApiDllPath = ["steam_api64.dll", "steam_api.dll"];
-Fixed Context.WriteTextFile adding an extra empty string at the end of the file, making some files unusable by games in some cases.
-Fixed a silent crash when the app can't get the OS version.
-Added a check to settings and profile settings to not allow duplicated Player Steam Ids.
-The Player Steam Ids will now be assigned dynamically while on the setup screen like the nicknames.
-The default Player Steam Ids will now populate the text fields in the players tab in Nucleus Co-op settings.
-Many scaling fixes in settings and profile settings.
-Many UI changes in settings and profile settings.
-Fixed custom cursors scaling at +100 scaling factor.
-Fixed more typos.

v2.3.1 - October 22, 2024

-Fixed a graphic glitch when the webview opens (missing bit on the left for a second).
-The downloader will now open external links in the user's browser.
-Fixed the osd not disappearing for some users when a handler uses `Game.ToggleUnfocusOnInputLock`.
-Fixed the game content folder not deleting on Ctrl+Q.
-Fixed clickable link error in a handler options page.
-Adjusted some graphics details to make the UI a bit smoother when selecting a game.
-Fixed error when using custom scaling factors.
-Fixed backup folders game options menu not showing.
-Fixed Nucleus game backgrounds stretching.

v2.3.0 - October 15, 2024

 -Added a Webview in place of the downloader so we can now browse and download handlers directly from the hub website. Could require https://developer.microsoft.com/en-us/microsoft-edge/webview2/consumer/?form=MA13LH if using non default Windows "debloated versions".
 -Disabled using xinput indexes by default for gamepad assignment (broke controller support in some handlers that rely on device id). Now the user will have to press a button on each gamepad for the controller icons to show in every handler, same as the multiple keyboards/mice icons work.
 -Fixed Automatic Steam language detection (deprecated code).
 -Fixed gamepad UI navigation virtual mouse icon appearing on the setup screen.
 -Fixed Nucleus Co-op windows location being restored on non available screen area (disconnected screens or Windows screen layout has changed).
 -Fixed high cpu usage if there are no gamepads connected.
 -Fixed bug where hotkey fields were detected as empty so hotkeys were not customizable.
 -Nucleus Co-op will now warn users if their documents path is in the OneDrive environment (not tested).
 -Nucleus Co-op will now offer the user to download Microsoft Visual C++ Redistributable directly instead of just redirecting to the Microsoft dedicated download page.
 -Right clicking on a game in the list will now open the option menu without selecting the game.
 -Added "Open Backup Folder" and "Delete Backup Folder" to the game options menu (per player).
 -Added a game options menu entry to access game assets directly from the UI.  
 -Now the KeepSymlink option is hidden in the game options menu when `KeepSymlinkOnExit` is enabled in the handler.  
 -Now hotkeys will be registered on handler startup and unregistered on stop, so they can't interfere with other software shortcuts. 
 -Now if a `Steam` game based handler is downloaded (ONLY STEAM GAMES AND NOT ALL) Nucleus will directly open the search exe window at the exe root folder for an easier selection and addition.
 -Improved automatic nickname swapping and populating in Nucleus profiles settings.
 -Favorites game button will now only update the list if at least one game is set as favorite instead of showing an empty list.
 -Removed click sound.
 -Now the profile list is scrollable.
 -Removed the 20 max profiles limit.
 -Removed broken auto search game function (might come back).
 -Added a prompt after handler download so the user can choose to download the game assets (cover and backgrounds) right away.
 -The game manager will now delete game assets if the game exe is not present anymore or if the handler has been manually removed from the handler directory.
 -The game manager will now delete the game from the user profile if the `.js file` has been manually deleted.
 -Now Nucleus deletes the game content folder and not only the instances folders.
 -It's not possible to add the same game twice anymore (same exe path).
 -Added a function to clean old app logs.
 -Now Nucleus Co-op will try to kill remaining game processes before trying to delete the game content folder (on game selection).
 -Added an option to game profile to disable gamepad navigation on handler startup. 
 -Added a popup so user can restart Nucleus Co-op as admin if the handler requires admin rights.
 -Added a "shortcuts reminder" toggleable with `Alt+R` for keyboard and `guide button` for controller (long press), a window showing Nucleus gamepad shortcuts and keyboard hotkeys will show.
 -Added a new window merger so we can now use Lossless Scaling with Nucleus Co-op.
 -Updated `BackupFile(string filePaths, bool overwrite)` function, now it supports multiple files and works per player too.
 -Fixed a bug where `KeepSymlinkOnExit` was ignored and instances content deleted.
 -Changed steamless lines to => `Game.SteamlessPatch = ["false", "--quiet --keepbind", "2500"];`  string[patch launcher(only), steamless arguments, patch timing] so now even launchers can be patched.
 -Fixed a bug where game files were re-symlinked even if the game option "keep instances content folder" was enabled and files existed.
 -Updated `Game.BackupFiles;`  `Game.BackupFolders;` `Context.BackupFiles;`  `Context.BackupFolders;`, so they work per player.
 -Added `Context.SearchSubFolder(string parentDirectory,  string searchPattern);` return the full path of the found directory matching the search pattern. 
 -Added `Context.SearchSubFolders(string parentDirectory,  string searchPattern);` return an array of all found directories matching the search pattern.
 -Added `Context.NoDPIHandlingWidth` and `Context.NoDPIHandlingHeight`, returns game window size with no DPI calculation.
 -Added `Context.SteamLang` returns user steam language settings choice.
 -Added path length check to the unlock og game files function, throws an error if the path length is too long.
 -Fixed audio routing for Windows10/11 version 21h2 and up.
 -Now a clickable link can be added to a handler UI option description (one per option).
 -Added `Game.BinariesFolderPathFix` (boolean), fixes binaries folder path not being correctly parsed if the binaries folder name exists multiple times in the binaries folder full path.
 -Added clickable link to all custom handler prompts and RunAdditionalFile prompts (one per window).
 -Added `Game.UseHandlerSteamIds` so player Steam ids from a handler will have priority over the default or customized settings player Steam ids.
 -And more. For the complete changelog check the Nucleus Co-op releases github page: https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases

v2.2.1 - January 06, 2024

 -Fixed a bug affecting handlers using Devreorder (e.g. Star Wars: Battlefront II Classic).
 -Fixed a bug deleting a player if his attached controller has been unplugged. Fixes Nucleus Co-op crashing when disconnecting a controller during a handler launch.
 -Fixed typos.
 -Fixed the updater. 

v2.2.0 - January 01, 2024

 - Huge code clean up and refactoring.
 - New per game profile system (20 max per game) and new settings. The new profile system allows specific configurations per game, can be disabled in Nucleus Co-op settings.
 - New UI changes and QOL improvements (crash windows, credits, prompt windows, handler downloader and more).
 - New shortcuts: switch player layout (2 players only), toggle cutscene mode, reset windows. If using 2 or more monitors in cutscene mode all screens will have one instance unmuted.
 - Added basic gamepad UI navigation, can be enabled/disabled in settings. 
 - Added gamepad support to all Nucleus shortcuts.
 - Fixed UI highlighting the wrong controller in the UI when polling and controller sometimes being wrongly assigned in-game.
 - Instances launch order changed from left to right/top to bottom to the order that input devices icons are dropped on each screen (same for single monitor). P1,P2,P3 etc. on the setup screen represent the launch order now.
 - Removed the splashscreen.
 - Better sorting of the game list.
 - Better userprofile.json indentation.
 - Added Goldberg emulator updater.
 - Now Nucleus will try to unlock all the original game files (read only files) before symlinking, hard copying etc.
 - Added new information messages that give feedback on Nucleus actions and shortcuts calls (keyboard and controller).
 - Added Microsoft Visual C++ Redistributable version check and redirection to Microsoft website.
 - Fixed "Download or update game covers" crash if no game handler has been selected/downloaded.
 - Fixed game options menu at higher scaling factors.
 - Fixed a long standing bug where it was possible to drop an input device in an expanded player area.
 - Fixed a long standing bug (and not documented) where it was not possible to expand player bounds on negative monitors.
 - Nucleus will now save its windows size and position.
 - Fixed infinite loop when cleaning game content containing read only file(s) on session end or errors.
 - Added debug log size check (max size 150kb) if max size is reached the log is deleted and logging is disabled.
 - Fixed an issue with Steamless not applying when the game executable is inside a bin folder.
 - Nucleus Co-op will not get stuck if the hub is down now (will still take ~ 4 seconds waiting for hub response).
 - Added placeholder text for Game.Options:  Game.AddOption("Title", "Description", "Option Name" , [leave empty]); // var option = Context.Options["Option Name"];  to retrieve the typed text. It works like the regular one but with an empty string array.
 - Added Context.StartProcess(Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\an-executable.exe");
 - Added per-game KeepSymlink option in game menu.
 - Added Context.EditTextFile(string filePath, string[] refLines, string[] newLines, string encoder); //refLines are partial or full strings to look for. newLines are new strings (full strings) that will replace refLines. Adding "Delete" as newLine will delete the corresponding reflLine from the file. Should work with any type of text files (xml, ini, txt). 
 - Added Game.GoldbergNoWarning = true; so Nucleus will not prompt if the Goldberg dlls are missing if the handler supports different platforms.
 - Added Context.PlayerSteamID so we can get players steam ids from Game.Play.
 - Added Context.FindFilePartialName (string sourceFolder, string[] partialNames); 
 - Added Context.GetFileName(string fullFilePath); //get the name of a file from its full path;
 - Added hide taskbar parameter to Context.HideDesktop(); function, Context.HideDesktop(bool hideTaskbar) so no need to add the hide taskbar line.
 - Added Game.BackupFiles = ["file1.txt", "file2.txt"]; and Context.BackupFiles = ["file1.txt", "file2.txt"]; both can be used in the same handler. 
 - Added Game.BackupFolders = ["folder1", "folder2"];  and Context.BackupFolders = ["folder1", "folder2"];
 - Added Context.ToUpperCase to convert string to uppercase. 
 - Added Context.ToLowerCase to convert string to lowercase. 
 - Added a prompt on error so user can enable logging from it.
 - Added debug log button when enabled in settings (opens debug-log.txt and a Nucleus install folder explorer window).
 - Added Context.CopyFolder(string sourcePath,string destinationPath) example => Context.CopyFolder(Context.GetFolder(Nucleus.Folder.InstancedGameFolder) +"\\Engine", Context.GetFolder(Nucleus.Folder.InstancedGameFolder) + "\\Citadel\\Engine");
 - Added Context.DeleteRegKeyValues(string baseKey, string sKey, string[] values); //where values is a string array of the value(s) you want to delete;
 - Added Context.ExtractZip(string sourceZip, string contentDestination,string password);
 - Added Context.EditZipFile(string sourceZip, string password, string savePath, string[] itemsToAdd, string[] entriesToRemove); //itemsToAdd => ["path where to copy in the zip|path of file/folder to add"]; Always add "\\" at the end off the path if the content to remove is a folder;

v2.1.2 - September 26, 2022

 - Fixed (for good) goldberg user_steam_id.txt and account_name.txt being edited when goldberg is not used.
 - Fixed the process picker list, it was not fully visible/scrollable.
 - New patching methods using patterns, see MasterHandler.js inside Nucleus Co-op handlers folder.
 - Reworked the arrows and play button highlighting (buttons will be highlighted now accordingly to the current config step).
 - Sizable/movable main Nucleus/downloader windows.
 - Set the settings window a little bit bigger, it was a bit cluttered after the last additions.
 - Added "Game.ForceBindIPNoDummy" => default false. ForceBindIP will start without the "dummy" launch argument.
 - Added player steam id format check in settings, can now only type numbers and a message pop up shows if the player steam id length is too short.
 - Delete cover/screenshots/description/icon path (icons.ini) when a game is deleted from the game list.
 - Cover and background images will now be updated (on screen) when a game has been selected before downloading the assets instead of keeping the default cover and background until another game is selected.
 - New "Unfocus" hotkey combo "Ctrl+H".
 - Added shortcut on the setup screen to access the player settings faster (player steam ids & nicknames).
 - Added a message when each instance starts showing game name, player nickname, and player number (id+1). Will show for a few seconds and automatically close.
 - The nickname tab in settings will now be populated with the default nicknames (player1, players2 etc).
 - Added app version in the crash report window + error report enhancements.
 - Added new line "Game.ToggleUnfocusOnInputsLock = true;" to automatically unfocus the game windows (gives focus to the Nucleus window), works with "Game.LockInputAtStart = true;" too.
 - Merged "split division" background window and the "Hide desktop" window will now be minimized/restored when using the CTRL+T shortcut (Toggle top most hotkey) accordingly to the game windows.
 - Added new line "Context.HideDesktop();" same as "Game.HideDesktop = true;" but usable in "Game.Play = function() {}". The PlayerID is checked in the function so no need to worry about it in the handler.
 - Added Context.HideTaskBar(); same as the regular one but usable in "Game.Play = function() {}".
 - Nucleus will now delete the game content folder on game selection (if `Game.KeepSymLinkOnExit` is false or not present in the handler) to avoid crashing after a previous crash, it will also check if any file has been locked (set to read only) and try to unlock it, if it fails a prompt will show and explain what to do to fix it.
 - New handler update method. Handlers can now be updated from the game options menu without having to re-select the game exe path. 
 - Moved the connection state check (hub) earlier so the app will take fewer seconds to appear on screen.
 - Some UI changes.
 - Added Favorite Games feature.
 - Added classic and macstyle themes, select between them in Nucleus Co-op settings theme option and click save, macstyle theme by V and classic theme by Vortex and Talos91 with cover by Altissimo. 
 - Unfocus the Nucleus Co-op "Stop" button when a handler starts to avoid stopping the session accidentally.
 - Disabled lock key until a game instance starts.
 - Changed input dropping order to reflect the instances (players) launch order.
 - Changed the input icons sorting in the setup screen (now dynamic & by input type).
 - Fixed the screen "priority" issue when starting a handler using multiple screens setup. There's a number at top right of each screen to show the startup order. The index on the screen will only show if there is more than one connected screen. 
 - Some scaling improvements (auto search game window, settings). 
 - Added Context.Wait(int milliseconds); time to wait between two functions inside "Game.Play = function() {}".
 - Fixed Nucleus Co-op not closing correctly with Ctrl+Q in some cases.
 - Added Context.UserName, gets the current Windows username. 
 - Added a list of the default Player Steam IDs used by Nucleus to the settings "Players" tab.
 - New customizable cursors.
 - New unsafe path warning for handlers that use Game.LaunchAsDifferentUsers = true; will show in the setup screen, same for the admin rights warning now.

v2.1.1 - May 24, 2022

 - Added Steamless command line version support: "Game.UseSteamless = true;", "Game.SteamlessArgs = "";", "Game.SteamlessTiming = 2500;". 
 - Fixed nicknames not working when using "Game.GoldbergExperimentalSteamClient = true;".
 - Fixed Player Steam IDs setting to 0 when using "Game.PlayerSteamIDs = [];".
 - Added game descriptions, they get downloaded to gui\descriptions.
 - Added new line Context.EditRegKeyNoBackup, will not create a backup of the registry when editing.
 - Fixed an unknown bug breaking the Nucleus window shape in some cases (maximizing without using the app maximizing button).
 - Fixed changing the default text editor in Settings.ini not working.
 - Added blur to background images, the blur can be disabled by setting Blur = 0 in Settings.ini.
 - Other minor UI improvements and changes.

v2.1 - May 5, 2022
 - Added Context.HandlersFolder (path to the root of Nucleus Co-op handlers folder: NucleusCo-op\handlers). 
 - Fixed app crash when a handler throws an error (sometimes on app close). 
 - Fixed random crashes while clicking on the game list. 
 - All monitors in use should be correctly scaled to 100% when a game starts now.
 - Added UI option to enable/disable the auto setting of the desktop scale to 100% (enabled by default).
 - All UI elements (pictures) can be customized now (see the default theme folder). 
 - Splashscreen fixes, you can skip it now by clicking on it if it shows for too long.
 - Added UI options in settings to disable/enable the splashscreen and click sound in the settings tab and moved the "mouse click" setting to the setting.ini instead of theme.ini. 
 - Other UI related details.
 - Some UI code optimizations.
 - New and improved Nucleus Co-op themes.
 - Added theme switch option in settings.
 - Links can be clicked in handler notes now.
 - Added option to use custom text in Context.RunAdditionalFiles prompt(s) + a boolean to show or not the file path. See readme.txt.
 - New Documents path registry key backup/restoration handling, should fully fix Nucleus changing the location of the Documents folder sometimes after a crash.
 - Added custom virtual devices icons.
 - First attempt to fix Turkish "Ä±" bug, requires to be tested in real conditions.
 - Fixed account_name.txt being edited while UseGoldberg is not used.
 - Added new input device detection in setup screen, keyboards and mice icons will only show in the UI if a key is pressed or a mouse moved now.
 - Added an option in theme.ini to round the Nucleus Co-op window corners (enabled by default). 
 - Added multiple Nucleus Co-op instances check (can be disabled in settings.ini).
 - Added the possibility to choose the app font in theme.ini (size can be adjusted).
 - Fixed a crash that occurred when custom icon pictures were deleted.
 - Added new "icons" folder inside the Nucleus Co-op "gui" folder, custom icon paths are now saved in the "icons.ini" inside that folder instead of being saved in settings.ini. 
 - Fixed crash that occurred when an user had a custom Documents folder in the root of his drive and clicked game options in the UI.
 - Fixed "Game.SymlinkFiles = [""];" and updated so that it can work under "Game.Play = function() {" using "Context.ProceedSymlink();".
 - Help gif updated.
 - Fixed Nucleus Co-op reporting the incorrect line number when a handler has an error, can still show the number with an offset of +1 if the line number returned is a float. 
 - Fixed a Nucleus Co-op silent crash that happened when controllers got disconnected and reconnected multiple times.
 - Added Game.SetTopMostAtEnd = true; Sets the game windows to top most at the very end.
 - Added .ini option to hide the Nucleus Co-op offline icon.
 - Added handler notes magnifier option.
 - Added new supported inputs UI icons, display what input devices a handler supports.
 - Added Player Steam IDs fields to the Nucleus Nicknames settings tab (now named Players), you can change the instances Player Steam IDs when a handler uses goldberg or SSE via the UI now.
 - Added new Nicknames/Player Steam IDs switcher, you can quickly switch the order of the nicknames and Player Steam IDs you set up.
 - Fixed minor UI glitch.
 - Last hooks prompt will show now when only using `Game.PromptBetweenInstances=true; `with` Game.SetTopMostAtEnd = true;`
 - Added option in Settings.ini to change the default text editor.
 - Selection not working and scaling issues fixed for Nucleus UI options that use images.

v2.0 - February 25, 2022
 - New overhauled and customizable user interface with support for themes, game covers and screenshots.
 - Fixed ui scaling issues at more than 100% desktop scale (and all other issues/bugs related to it).
 - Fixed multi-monitor vertical setup drawing to not overlap input device list.
 - Quality of life improvements such as but not limited to: new discord invite link, Nucleus Co-op github release link and much more. 
 - Added Nermintingas Galaxy emulator support.
 - SplitCalculator(Josivan88) integration.
 - Renamed scripts to handlers. 
 - Added new handler callbacks.
 - New player nickname assignation.
 - New player and input order processing.
 - New optional splitscreen division(visualy similar to a native splitscreen game).

v1.1.3 - September 28, 2021
 - Fixed only 4 Xinput controllers showing
 - Fixed controller index being inconsistent with the Nucleus UI
 - Fixed duplicate controller icons (and similar bugs)
 - Fixed Ctrl+T working unreliably when Proto Input was injected
 - Removed xinput1_4 dependency to fix crashing on Windows 7
 - Added more script callbacks

v1.1.2 - September 1, 2021
- Fixed fake cursor not showing in some games

v1.1.1 - August 30, 2021
 - Added script updater
 - Fixed the Proto Input controller hooks
 - Fixed incorrect controller icons being displayed 
 - Fixed the fake cursor not hiding when it should in some games

v1.1.0 - August 18, 2021
- Integrated Proto Input (github.com/ilyaki/protoinput) hooks
- Greatly improved keyboard/mouse input
- Complete rewrite of all hooks, plus new injection method support
- Fixed most bugs relating to keyboard/mouse input, including CursorVisibilityHook crashing
- New in-game GUI to change hooks and input settings while the game is running
- Improved fake focus, including a message filter system for all windows messages
- Smarter scripting so you can use a combination of keyboards/mice and controllers smoothly
- Custom mouse cursor support
- Added support for more than 4 Xinput controllers with OpenXinput
- Added DirectInput controller redirection
- Rewrite of the input locking system - no more laggy cursor, and Windows UI elements won't randomly open
- A bunch of misc changes and bug fixes

v1.0.2 R5 FINAL - January 2, 2020
- Fixed bug that would cause the incorrect document path to be used for subsequent players when using Nucleus environment and start up hooks
- Document path in registry will now only be changed if it needs to (only if playing a game that uses Documents for game files)
- Some fixes for device layout screen
- Updated Goldberg emulator to latest git build

v1.0.1 R5 FINAL - December 30, 2020
- Fixed app not opening for some users
- Fixed bug with expanding single keyboard vertically
- Added NucluesUserRoot to context (get userprofile paths for a player's respective nucleusplayer Windows account)
- Other minor bug fixes/tweaks

v1.0 R5 FINAL - December 24, 2020
- Audio routing per instance; specify each game to run through different audio sources
- Added support for Nemirtingas Epic Emulator
- Added support for games that use Documents for game files (and support for users with custom document folders)
- Nucleus user accounts on windows can now retain save data between sessions (including Halo MCC, your game saves will remain intact)
- Nucleus on close will now remove files it created when using original game folders (keeping your original game folder untouched)
- Registry edits done by Nucleus now only persist the game session (Nucleus backs up and restores, as to note touch your original game settings)
- Added alot of new scripting features that greatly increases game compatibility list
- New function, custom prompts. Scripts can prompt users for input that can be used in scripts for logic
- New xinput hooks (alot more effective way of hooking and handling xinput messages) (Thanks to @Ilyaki)
- Fixes for custom layouts and stretching KMB inputs on layouts
- Fixes for status window, performs alot better (but still every once and awhile may not work)
- Updated alpha 10 custom xinput dlls to enable mouse and keyboard input for instances
- XinputPlus assign gamepad based on player's gamepad index NOT player's index
- Added support for wizark952's dinput8 blocker 
- Added some support for Goldberg's steam client loader 
- Added options to set window styles at very end, ignore window border check, and option to enable windows at end
- Added function Game.IgnoreThirdPartyPrompt; when launching via third party, ignore the prompt to press ok when game is launched
- Changes to LaunchAsDifferentUsers - added option to keep user accounts and ability to transfer user account data (if not keeping)
- Made some back-end changes to Nucleus environment 
- Revamped settings window, tabs are now used to seperate different sub-settings
- Changes to Context values, functions and bug fixes 
- Updated Goldberg emulator to latest git build
- Added prelimary and experimental function to write to process memory; Game.WriteToProcessMemory
- LOTS of miscellaneous changes/bug-fixes

v0.9.9.9 r4 - April 12, 2020
- Improved script downloader. Handlers are cached and viewable up to a specified # of results at a time (via "pages"). User can also pre-sort and use drop-down as an alternative way to sort when searching
- Replaced LaunchAsDifferentUsers with a new and improved method, which will now also utilize current user's Nucleus environment. The previous method has been retained as LaunchAsDifferentUsersAlt
- Added an optional status window to appear when launching and closing Nucleus, to show what Nucleus is doing
- Added an alternative method of changing IP per instance. Alt method will create temporary loopback adapters for each player (however, it will not change any metric)
- Added the ability to extract existing script handlers (.nc files)
- Added a new option in game scripts to call Game.Play after the game instance has launched
- Added a new option in game scripts to not copy files from UserProfile<Config><Save>Path when using Nucleus environment
- Added a new option in game scripts to ignore checking if launcher exe exists in game folder
- Added a new Context function, ReadRegKey. Will return a string value for a specified key
- Added new Context function, RunAdditionalFiles. User can specify files that need to be run before launcher/game
- Added functions to Context so you can now get a list of all found files and their paths that match a given pattern. Can also change a creation date and modified date of a file
- Added an option for DirSymlinkCopyInstead to copy all subfolders and files in addition
- Added new values for Player, SID, Adapter and UserProfile (for use with some of the new functions in this update)
- Improved editing and deleting of registry keys, in addition keys in HKEY_USERS are now accessible
- Changed prompts from message boxes to Forms (more control, shold now be on top most of the time)
- Migrated from SlimDX to SharpDX for reading controllers
- Updated rename mutexes to work with "Type:|" prefixes introduced in a recent update
- Updated external libraries to latest versions (Jint, DotNetZip, Newtonsoft Json)
- Potential fix for bug that was eating CPU resources when using Direct Input/Xinput Reroute
- Fixed XInputPlus bug that would only recognize dinput dll if it was all lowercase
- Fixed bugs when using DrawFakeMouseCursor, added option DrawFakeMouseForControllers [thanks to @Ilyaki]
- Fixed some bugs in script downloader
- Fixed backed up registry table not being restored
- Fixed DirSymlinkCopyInstead not copying folder itself
- Fixed ChangeExe not working when KeepSymLinkOnExit is being used
- Fixed Nucleus post hook dlls being injected too early when using ProcessChangesAtEnd
- Number of other bug fixes
- Minor tweaks/changes

v0.9.9.9 r3 - March 26, 2020
- Fixes and improvements to Game.LaunchAsDifferentUsers
- Fixed error message on the controller layout screen when using dinput / xinput reroute
- Fixed context aspect ratio decimals

v0.9.9.9 r2 - March 25, 2020
- Can now view all public scripts in Script Downloader and sort columns by ascending/descending order
- Added an option in game scripts to launch each game instance as a different user (Nucleus will create temporary accounts "nucluesplayerx" and then delete them at the very end) [thanks to @napserious for his base code]
- Added an option in game scripts to run Game.ExecutableName in addition to Game.LauncherExe (if used)
- Added an option in game scripts to specify an amount of time to wait after lauching game but before grabbing the game's process
- Added an option in game scripts to specify if starting arguments should be inside the executable path when using Game.CMDLaunch
- Added an option in game scripts/utility to use a EAC bypass dll
- Added an option in game scripts to kill mutexes at the end, when using Game.ProcessChangsAtEnd
- Added an option in game scripts to add gamepad cursors [thanks to @Ilyaki]
- Added additional logic to SetWindowHook and SetWindowHook hooks, now they should work better
- New context options available to get monitor or a specific player window's height/width/aspectratio
- Game.CreateSingleDeviceFile will now also hook CreateFileA for ANSI calls
- Whenever registry keys are being edited or deleted by a script, Nucleus will now backup the current registry keys and restore them upon exit
- When hard copying game files, you can now specify exclusions
- When hard copying game files, Nucleus will wait for each instance to hard copy before continuing
- Updated Goldberg emulator to latest git build (sha 5c41ba020c4ffc46d0adbeb3b82c9ae623d14ef2)
- Fixed not all lines using right encoding when specified, when modifying lines to files (replacelines, removelines, etc)
- Fixed Script Downloader not working for Window 7 users
- Fix for raw input filter / reregister raw input not working [thanks to @Ilyaki]
- Fixed some objects weren't being properly disposed

v0.9.9.9 f1 - March 15, 2020
- Added an option in game scripts to create only one file for HID devices per instance (the assigned HID device). This is a workaround for Unity games that use default input
- Added an option in game scripts to enable the minimize, and restore of game window at the end (now off by default, only few games are known to need it atm)
- Device handle zero support [thanks to @Ilyaki]
- Added a delay during start up hooks for better performance (would cause issues on lower end PCs), and fixed hang when it failed its 5 attempts
- Fix ResetWindows not resizing or repositioning if DontResize or DontReposition is on
- Only copy from AppData folder to Nucleus Environment if Nucleus Environment is being used
- Re-enabled debug log header
- Minor tweaks/bug fixes

v0.9.9.9 - March 7, 2020
- Nucleus now supports multiple Mice and Keyboards [thanks to @Ilyaki]
- Added the ability to search for scripts (handlers) and download them directly from the UI [thanks to @r-mach]
- You can now edit scripts while Nucleus is open and changes will take effect (no need to restart Nucleus each time anymore)
- Added a new option in game scripts to do resizing, repositioning and post-launch hooking of ALL instances at the very end (after all have been opened)
	- Note: This method will not work for every option to-date
- Added an option in game scripts to re-register raw input in game process, a replacement for forwarding raw input [thanks to @Ilyaki]
- Added a new utility, Flawless Widescreen. Can be setup by calling it in game script
- Added an option in game scripts to change your IP per instance (NOTE: Highly experimental), a drop-down has been added to settings to select which network to change IP for
- Added logic to delete certain files that Nucleus adds to original game folder (when not linking or copying). For example, some that get deleted: Nucleus custom dlls, x360ce, xinput plus, custom utils
- Added an option in game scripts to kill mutexes in launchers	
- Added an option in game scripts to kill mutexes in the last instance (normally last is ignored)
- Added an option in game scripts to rename, move or delete files in instance folders
- Added an option in game scripts to prompt the user after launching each instance, but before the grab process is grabbed
- Added an option in game scripts to delete file(s) from user profile config or save path, a prompt asking you to confirm will typically show, but this can be turned off in script too
- Added an option in game scripts to rename the steam api for Goldberg experimental (default will now not rename, you must set it to do so)
- Added an option in game scripts to kill additional processes upon exiting Nucleus or stopping a session
- Added an option in game scripts to ignore PreventWindowDeactivation if player is using keyboard
- Added an option in game scripts to copy (not symlink) all files within a given folder
- Added options in game scripts to disable Nucleus resizing, repositioning or making windows top most
- Added an option in game scripts to specify custom window styles (extended window styles as well)
- Increased max nickname length to 9 characters
- Exposed IsKeyboardPlayer to Context (will be True or False), can now be called in Game.Play
- Launchers will now also be killed upon exiting Nucleus or stopping a session (if any remain open throughout session), in addition to game windows
- Added logic to KeepAspectRatio & KeepMonitorAspectRatio if new width is to be determined (previously only new height)
- Added logic so that mutexes of different kinds can be killed. In Game.KillMutex(Launcher), simply begin the string with "Type:Mutant|" following by the name of the mutex to kill. Can replace mutant with any mutex type
- Added logic to check if borders are removed at very end (some games bring them back), and if they aren't, remove them again
- Added Steam language in UI settings, and language gets updated automatically for SSE now as well
- Updated logic when launching games with start up hooks. Will check if process for that instance is already running, as well as try to grab the correct process if it is detected to be wrong
- Updated logic for Goldberg to set the settings folder in Nucleus Environment if environment and no local save are enabled
- Updated Goldberg emulator to latest git build (sha a0b66407bf2b8da686a708802cbc412f9cd386ca)
- Updated Context.LocalIP to better identify user's local IP when there are multiple IPs
- Updated method to capture current environment's user profile if different than their username [thanks to @Ilyaki]
- Fixed bug with x360ce placing files in wrong directory
- Fixed some duplicate operations happening if game was not linked or copied
- Fixed instanced folders trying to be deleted if game was not linked or copied
- Fixed a bug with PreventWindowDeactivation causing input to not work for some users
- Fixed some bugs with HexEditFileAddress and HexEditExeAddress
- Fixed architecture displayed in UI script details

v0.9.9.1 - February 5, 2020
- Updated logic for LauncherExe. The file name in this field will now be launched via Nucleus but ExecutableName will be used to resize, reposition and hook. Launchers will no longer be looked for when grabbing process to manipulate. LauncherExe will be used for hex editing exes and change exes.
- LauncherExe can now also accept a full path to the launcher exe. This is if the launcher is outside of the game folder; NOTE: This file/folder contents will NOT be symlinked/copied, only the game root folder of ExecutableName. This means that hex edits and changing exe using a full path in LauncherExe WILL overwrite the original file!
- Added an option in game scripts to specify which instances get starting hooks, post-launch hooks and fake focus messages sent to (including a specific option for keyboard player)
- Added an option in game scripts to provide a relative config and save path from user profile for games (for use with Nucleus environment)
- Added an option in context menu/game options in UI to open or delete relative config and save paths (for system user profile and Nucleus)
- Added logic if the game is running in Nucleus environment and a user profile config and/or save path have been provided, files and folders will be copied over from the system user profile path (+ added an option to force it)
- Added an option in game scripts to manually enter steam IDs to be used
- Added an option in game scripts for games to try and keep monitor aspect ratio when being resized
- Added an option in game scripts to replace bytes in the exe or file at a specific offset
- Added new field in Context that can be used to grab the local IP address of the computer
- Added new fields in Context for Nucleus environment paths and relative game config/save paths
- If using Nucleus environment and goldberg, set the goldberg settings folder to the Nucleus appdata path
- Updated Goldberg emulator to latest git build (sha b4205535fbee455bee925ab3aa90780e00eead27)
- Hopefully, final fixes for aspect ratios being preserved via KeepAspectRatio and the new KeepMonitorAspectRatio options
- Fixed some issues with x360ce
- Fixed a bug that would cause infinite waiting if process data had already been assigned by the time it got checked
- Fixes for CMDBatchBefore/After (CMDOptions no longer required and will only use if there is a line for that instance, CMDBatchAfter now working)
- Fixed a bug that caused Nucleus to crash when using x360ce
- Fixed SmartSteamEmu not grabbing game process to resize, reposition and hook. SSE launches can now also use process picker
- Fixed bug preventing nickname from reverting back to default after removing a nickname
- Fixed bug that would sometimes cause an error and weird nicknames in UI, when disconnecting, reconnecting controllers
- Made some changes/improvements to context/game options menu and logging
- Game.HookInitDelay and Game.HookFocusInstances have been removed

v0.9.9 - January 22, 2020
- Added an option in game scripts to use Goldberg Lobby Connect (automatic)
- Thanks to Ilyaki, added an option in game scripts to use a custom environment (located at C:\Users\<your username>\NucleusCoop)
- Added an option in game scripts to specify if the game is launched outside of Nucleus (Nucleus will then not launch it, but continue to do everything else, hook, resize, repos etc)
- Added a last fail-safe if process is still not found; a new window will open asking user to manually select the process
- Added an option in game scripts to manually pick the process to be manipulated for things such as resizing, repositioning and used for post-launch hooks
- Added an option in game scripts to run other commands in command prompt before or after game launches when using CMDLaunch (or UseForceBindIP), also set some environment variables that can be used in commands
- Added DInput support for XInputPlus
- Added an option in game scripts to have Goldberg not create a local_save.txt file (use default save location)
- Added an option to specify a different x360ce dll name be used, if needed
- Added the ability to copy entire folders (and all its contents) in CopyCustomUtils
- Added an option in context menu/game options in UI to delete content folder
- Added an option in game scripts to completely ignore linking or copying a folder and all its contents
- Updated Goldberg emulator to latest git build (sha 3f44827326eff6d9cc385c27f0bded89ee7642ea)
- Replaced a function (GetDpiForWindow - used for GUI) that required Windows 10, with an alternative method, so older versions of Windows are now better supported
- Made auto goldberg case insensitive when replacing steam api dlls
- Fixed bug that would stop script notes from appearing in UI after stopping a session
- Fixed bug when KeepSymLinkOnExit = true and # of players increase when there are more than 4 players
- Fixed LauncherExe being required in script if process was not found, or if script uses CMDLaunch or UseForceBindIP
- Fixed "Process is not running" crashes when using start up hooks
- Fixed KeepAspectRatio not resizing correctly, windows will now be horizontally centered as well (to player bounds)
- Fixed crash when SymlinkGame is false

v0.9.8.2 - December 8, 2019
- Hook code has been cleaned up and some lingering issues with Easyhook in the past have been resolved *Thanks to @Ilyaki
- Completely reworked Autosearch. Fixed bug requiring admin rights, custom paths are now allowed, user can choose which found games to add, and so much more
- Instance folder will no longer be symlinked if SymlinkFolders is true
- Integrated and added an option in game scripts to use DirectX 9, Direct 3D wrapper (d3d9.dll) to try and run DirectX 9 games in windowed mode
- Added an option in game scripts to force steam stub DRM patcher to either use x64 or x86 architecture dll
- Added an option in game scripts to force symlink every time
- Added an option in game scripts to set processor affinity per instance
- Changed logic of setting processor affinity
- Nicknames assigned in Nucleus are no longer exclusive to Goldberg
- The keyboard player can now be assigned a nickname in Settings
- Fixed large files sometimes breaking the symlink process
- Fixed files not being symlinked with different amount of players under the new rule: files will only be copied or symlinked once, if needed
- Fixed Nicknames not always being updated if changed
- Fixed the black window from Hide Desktop not closing when stopping a game session (must press stop or stop session hotkey)
- Added an option in game scripts to set processor affinity per instance
- Changed logic of setting processor affinity

v0.9.8.1 - November 4, 2019
- Reverted folder and file exclusion logic to the way it was done pre-0.9.8 (but still kept improvements made to them)
	- DirSymlinkExclusion will force hardcopy of the folder (if it is to be symlinked), FileSymlinkExclusion will completely ignore the file (no link/copy), FileSymlinkCopyInstead will continue to just create hardcopy of file instead of symlinking it
- Fixed xinput plus controller mappings when keyboard player was any player except last
- Prompt between instances can now be delayed if PauseBetweenStarts has a value

v0.9.8 - November 1, 2019
- Nucleus no longer starts with administrative privileges and will prompt if it is needed, games will not be launched elevated now either
- Improved and changed logic on how files are copied/symlinked, much faster now and done all it once at the beginning
	!WARNING!: DirSymlinkExclusions did not work properly in original Nucleus Alpha 8 and now does. All files and subfolders of a DirSymlinkExclusion will be ignored no matter what the file is. Check your scripts!
- Files that get added to the instance folders (goldberg, xinput plus, etc) will no longer be copied over to the original game path and files (as long as SymlinkFolders is false)
	- If SymlinkFolders is true, files WILL be copied over to the original file path
- Files will only be copied or symlinked once, if needed; i.e. if Instance0 path exists with at least one file & KeepSymLinkOnExit is true, Nucleus will skip copying/linking
- Added an option in game scripts to prevent window deactivation
- Added an option in game scripts to hardlink files instead of symlink
- Added an option in game scripts to symlink folders (needed for some games)
- Added option in game scripts to use the experimental Goldberg branch
- Added an option in game scripts to ignore creating a steam_appid file (needed for some games)
- Added an option in game scripts to use Steam Stub DRM Patcher
- Added an option in game scripts to set the foreground window to something other than the games, needed for some games to balance out FPS
- Added an option in game scripts to create a steam_appid.txt file in the same folder as the game executable (needed for some games)
- Added game option to open the original executable directory
- Added number of players each game has in the UI game list, under the title
- Added option to right click on layout selection to go through the different layouts in reverse
- Added/tweaked some additional logging info
- Added more information to be displayed when selec	ting the Details option in UI
- Updated Goldberg emulator to latest git build (sha 2986b01d0cf34cd900f772cf4294ad387c104cf4)
- XInput plus and X360ce will now leave controller slots blank if player is using a keyboard (no controller input should work on keyboard instance)
- Scripts will now open in Notepad++ by default if installed, when using the Open Script option in UI
- Fixed Script Author notes in UI so that long notes will not overlap when placing controllers
- Fixed NeedsSteamEmulation option and added SSE support for x64 games
- Fixed IdInWindowTitle and ForceWindowTitle bug that would prevent them from working
- Fixed bug preventing mutexes being killed if there were two or more
- Fixed process with ID of 0 being killed
- Removed DotNetZip nuget package as it is no longer being used (replaced zip extractions with direct file operations)
- If a game script has an issue and game doesn't appear, when you fix the issue and launch Nucleus again, you no longer need to re-add the game (game will simply reappear)
- If a file is unable to be deleted after 10 tries during clean-up, do not throw an exception, just carry-on (details are logged)
- PromptBetweenInstances will now show the last prompt (to install hooks) if there are hooks to install OR if FakeFocus is set to true
- Renamed the Data folder to now be "content" (needed as some games mistake the Nucleus Data folder in the path as game files)
- Renamed the games folder to now be "scripts"

v0.9.7.2 - October 15, 2019
- Fixed crash that would happen if ForceFocusWindowName was left blank

v0.9.7.1 - October 14, 2019
- When placing controllers, you can now resize any player's screen to be full vertically or horizontally on any layout (custom layouts too!)
- Added a new option in game scripts to reset the previous window's size, position and borders as each new instance opens up 
- Added an option in game scripts to provide a description, that will appear in the UI when the user selects the script's game
- Added an option to manually specify what language Goldberg should use
- Experimental: UI will now scale based on monitor DPI and Font size
- Improved closing procedures; better chance of finding and killing any lingering game processes
- Only one copy of Nucleus Coop can be open now
- Fixed typo preventing start up hooks from working on x64 when using the delay method
- Fixed Nucleus not launching when there is an error with a game script; will now prompt the user about the error and not show the game in the game list
- Fixed a bug when stopping a session would sometimes trigger an error message
- Fixed a bug where third party files were being placed in wrong directory when working folder was set to something in game script
- Fixed not all the files in the instance folders not being deleted sometimes
- If Nucleus crashes, the error message is now logged in the debug log in addition to the error file generated in the Data folder, plus taskbar will now show if it was hidden
- Changed the "Unknown Game" error message to be clearer on what the issue is
- Instead of denying the user to add a game if the game's executable is already in the game list, it will now prompt the user if they would like to add the game anyway or not
- Removed pointless horizontal scroll bar on game list
- Other Minor UI changes

v0.9.7.0 - October 7, 2019
- Goldberg Emu is now built-in to Nucleus, 3 new options in game scripts that will fully automate the process
- Added a customizable custom layout slot when placing controllers/devices. Create your own layout in Settings!
- Fixed issue of mutexes with certain special characters not being killed or renamed properly
- Removed SmartSteamEmu embedded in Nucleus.Gaming.dll, greatly reducing the dll's file size. SmartSteamEmu is now accessed and available externally through the "utils" folder (also updated SSE to the latest version, 1.4.3)
- Added an option in game scripts to copy custom files if needed
- Added two options in game scripts to be able to do text value replacements in binary files
- Added two additional options in game scripts to be able to do text value replacements in a file you specify
- Added new util, ForceBindIP and an option in game scripts to set it up automatically
- Added new util, XInputPlus and an option in game scripts to set it up automatically
- Added new util, Devreorder and an option in game scripts to set it up automatically
- Added an option in game scripts to block raw input devices in game
- Updated cmd launch to now work with every instance (previously only worked on second instance)
- User can now right click on game list to access different options for that game
- Added an option in the UI to open the game script in notepad
- Added an option in the UI to open the game data folder
- Added an option in the UI to change the game icon
- Added an option to enable a debug log to be created
- Game.ExecutableName is no longer case sensitive when trying to add a game in the UI
- Fixed an empty error log text file from being created in instance folder whenever a hook was used
- Fixed x64 focus hooks and setwindow hook
- Added a SetWindowStart hook that can be applied as each game opens up, instead of later on with SetWindowHook
- Made game windows not be the top most when using Game.PromptBetweenInstances, until after the last prompt (keeps prompt on top and easier to switch-to)
- Improved x360ce so that the xinput dll file gets copied along with the executable so x360ce doesn't prompt to create one
- Exposed the raw gamepad guid of each player's controller in game scripts (changed the name of the x360ce formatted one for clarity)

v0.9.6.1a - September 22, 2019
- Fixed 6 and 8 player layouts
- Reverted mutex searching back to exact matches by default as this broke some games, but left the option to do partial searches if needed (some games do require this)

v0.9.6.0a - September 20, 2019
- Added an option in game scripts to rename mutexes instead of killing them
- Tweaked method of finding mutexes to be more inclusive. You can now provide a partial name of mutex to kill (you must still provide full name for renaming)
- Upgraded custom xinput dll to Alpha 10s' and added x64 custom dll support (alpha 10 custom dlls are now the default, but you can revert back to alpha 8 with a line in game script for compatability)
- Added 6 and 8 player support
- Added an option to delete games from directly within the NucleusCoop UI
- Added the ability to assign "nicknames" to controllers in settings, used to identify specific controllers
- Added the option to allow you to use those newly created nicknames in-game, instead of default "Player1", "Player2" etc (this is for games using Goldberg only)
- Added the ability to see which controller is which by simply holding down a button on a controller. The corresponding controller icon will light up
- Added an option in game scripts to keep the game's aspect ratio when resizing
- Added an option in game scripts to manually set width, height, as well as position of each game window
- Added an option in game scripts to launch and set up x360ce before launching games (make sure you aren't using custom dlls)
- Added an option in game scripts to have the user decide when to open the next instance, instead of each being opened automatically
- Added an option in game scripts to hide desktop background
- Added an option in game scripts to hide taskbar when launching games
- Added an option in game scripts to hide the mouse cursor
- Added an option in game scripts to add the process ID to the end of game windows (helpful for creating unique/different window titles)
- Added an option in game scripts to use different exe names for each instance
- Added an option in game scripts to Hardcopy game files instead of Symlinking
- Added an option in game scripts to resize and reposition the previous window after a new window opens (some games need this)
- Fixed a bug preventing x64 focus and setwindow hooks from working
- Fixed a bug preventing Nucleus from opening when multiple scripts use the same Game.ExecutableName value
- WIP: rudimentary support for DInput alongside XInput (more recognized in Nucleus, can be assigned to a screen like any instance - should work in game). *Most dinput devices will still need to be set up manually.
- WIP: rudimentary Keyboard support

v0.9.5.3a - August 30, 2019
- Fixed a bug that stopped Game.FakeFocus from working

v0.9.5.2a - August 30, 2019
- Fixed Halo not working correctly with more than 2 instances

v0.9.5.1a - August 29, 2019
- Added an option in game scripts to prevent games from resizing on their own
- Added an option in game scripts to copy files from the game directory to the instanced folder
- Added a parameter to the registry manipulation methods, you can now specify a base key to work from (local machine or current user)
- Created a custom solution in order for Halo Custom Edition to work in Nucleus
- Tweaked EditRegKey so it now takes an object data type as the data and a custom RegType to specify the registry type (DWord, QWord, Binary, etc), opens alot more possibilities
- Reworked launching games using command prompt, much better results now. CMDOptions can now be specified per game instance (first instance is ignored for now)
- Fixed an issue with focus hooks that would sometimes crash the game
- Fixed launching games that don't require mutexes be killed but still have initial hooks
- Fixed some other minor bugs

v0.9.5 - August 21, 2019
- Added an option in game scripts to hook into game functions to trick the game into thinking it has focus
- Added an option in game scripts to hook into game functions that try and prevent multiple instances from running
- Added @gymmer's "FocusFaker' method to further trick the game into thinking it has focus
- Added an option in game scripts to set the window priority of each game instance. Can be set to either "AboveNormal", "High" or "RealTime"
- Added an option in game scripts to assign which processor games run on. Can be either an ideal processor (safer, no gaurantee) or a fixed one despite availability
- Added the ability to edit registry keys. Required for some games
- Added an option in game scripts to work-around ForceFocusWindowName having to match 1:1 with game window title for resizing, positioning and focus. Needed for some games that change their window title (does not effect game launchers)
- Added an option to force the title of game windows to be whatever is specific in Game.Hook.ForceFocusWindowName
- Added an option to specify individual files to Symlink
- Added an option in game scripts to launch a game using cmd with specified options
- There is a new Settings window (see cog icon to right corner of Nucleus)
- You can now customize the Nucleus hotkeys to be whatever you'd like
- Fixed a bug that Nucleus did not work properly when game windows were closed manually
- Updated method to toggle game windows being top most. It is now more efficient and doesn't rely on game script
- Minor improvement in performance
- Cleaned up some code

v0.9.4.1a - August 7, 2019
- Fixed bug that broke the generic file manipulation methods

v0.9.4a - August 6, 2019
- Added two new methods for game scripts: RemoveLineInTextFile and FindLineNumberInTextFile
- All four of the generic file manipulation methods (Find, Remove, Replace, ReplacePartial) now include an overload method to specify the kind of encoding to use
- Added a new variable to be used in Remove & Find Line methods: Nucleus.SearchType. You can specify either "Contains", "Full" or "StartsWith" to get more accurate results. Now with the four file manipulation methods, you should now be able to edit ANY game text file for ANY game
- Fixed bug in original Nucleus that didn't allow 1 player to be able to start/play a game
- Fixed "one time use" bug in original Nucleus; Nucleus can now be used multiple times in one session by simply pressing the STOP Button in the app OR pressing the newly created hotkey for it: Ctrl+S
- Added a toggle to make the game windows not the top most windows. Useful if you want to use other programs in the background. Hotkey is Ctrl+A. Pressing it will either disable or enable the windows being top most, depending on its current state. This requires Game.GameName in Nucleus game scripts to work.

v0.9a - August 1, 2019
- Initial release

--------------------------------------------------------------------------------------
Credits:

Official Nucleus Co-op 2.0 and Up: Mikou27/nene27.
Original Nucleus Co-op Project: Lucas Assis (lucasassislar).
Nucleus Co-op Alpha 8 Mod : ZeroFox.
Proto Input, USS, multiple keyboards/mice & hooks: Ilyaki.
Website & handler hub API: r-mach.
Handlers development, Nucleus Co-op general testing, feedback and improvement: Talos91, PoundlandBacon, Pizzo, Maxine, Zensuu and many more.
Nucleus Co-op 2.0+ UI assets creation: Maxine, Mikou27, PoundlandBacon.

Additional credits to all the original developers of the third party utilities Nucleus Co-op uses:
Mr_Goldberg (Goldberg Emulator), syahmixp (SmartSteamEmu), atom0s (Steamless), EJocys (x360ce), 0dd14 Lab (Xinput Plus), r1ch (ForceBindIP), HaYDeN (Flawless Widescreen), briankendall (devreorder), VerGreeneyes (DirectXWrapper), wizark952 (dinput8 blocker), Nemirtingas (Epic\Galaxy Emulator & OpenXinput), Dark Reader extension (https://github.com/darkreader/darkreader?tab=readme-ov-file), Josivan88 (SplitCalculator).

Special thanks to the SplitScreenDreams discord community, this wouldn't have been possible without all your contributions.
 