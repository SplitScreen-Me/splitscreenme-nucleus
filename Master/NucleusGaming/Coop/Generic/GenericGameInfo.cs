using Jint;
using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Generic.Step;
using Nucleus.Gaming.Tools.NemirtingasEpicEmu;
using Nucleus.Gaming.Tools.NemirtingasGalaxyEmu;
using Nucleus.Gaming.Tools.Steam;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static Nucleus.Gaming.GameDpiAwareness;

namespace Nucleus.Gaming
{
    public class GenericGameInfo
    {
        private Engine engine;
        private string js;

        public Hub Hub = new Hub();

        public GameHookInfo Hook = new GameHookInfo();
        public List<GameOption> Options = new List<GameOption>();
        private List<string> profilesPath = new List<string>();

        public SaveType SaveType;//Not used but could be present in some handlers
        public string SavePath;//Not used but could be present in some handlers
        public bool UpdateAvailable;
        public string[] DirSymlinkExclusions;
        public string[] FileSymlinkExclusions;
        public string[] FileSymlinkCopyInstead;
        public string[] DirSymlinkCopyInstead;
        public string[] DirExclusions;
        public string[] BackupFiles;
        public string[] BackupFolders;

        public bool KeepSymLinkOnExit;

        public double HandlerInterval;
        public bool Debug;
        public bool SupportsPositioning;
        public bool SymlinkExe;
        public bool SymlinkGame;
        public bool HardcopyGame;

        public bool SupportsKeyboard;
        public bool KeyboardPlayerFirst;

        public string[] ExecutableContext;
        public string ExecutableName;
        public string SteamID;
        public string GUID;
        public string GameName;
        public int MaxPlayers;
        public int MaxPlayersOneMonitor;
        public int PauseBetweenStarts;
        public DPIHandling DPIHandling = DPIHandling.True;
        public DpiAwarenessMode DpiAwarenessMode = DpiAwarenessMode.HighDpiAware; 
        public string StartArguments;
        public string BinariesFolder;
        public bool BinariesFolderPathFix;

        public bool FakeFocus;
        public int FakeFocusInterval = 1000;//TODO: high CPU usage with low value?
        public bool FakeFocusSendActivate = true;
        public bool SendFakeFocusMsg;
        public bool SplitDivCompatibility = true;
        public bool SetTopMostAtEnd;
        public bool Favorite;
       
        public void AddOption(string name, string desc, string key, object value, object defaultValue)
        {
            Options.Add(new GameOption(name, desc, key, value, defaultValue));
        }

        public void AddOption(string name, string desc, string key, object value)
        {
            Options.Add(new GameOption(name, desc, key, value));
        }

        public string HandlerId => Hub.Handler.Id;

        public bool IsUpdateAvailable(bool fetch)
        {
            return Hub.IsUpdateAvailable(fetch);
        }

        /// <summary>
        /// The relative path to where the games starts in
        /// </summary>
        public string WorkingFolder;
        public bool NeedsSteamEmulation;
        public bool NeedSteamClient;
        public string[] KillMutex;
        public string KillMutexType = "Mutant";
        public int KillMutexDelay;
        public string LauncherExe;
        public string LauncherTitle;
        public Action Play;
        public Action SetupSse;
        public Action OnStop;
        public Action OnFinishedSetup;
        public List<CustomStep> CustomSteps = new List<CustomStep>();
        public string JsFileName;
        public bool LockMouse;
        public string Folder;
        public bool HookFocus;
        public bool ForceWindowTitle;
        public int IdealProcessor;
        public string UseProcessor;
        public string ProcessorPriorityClass;
        public bool CMDLaunch;
        public string[] CMDOptions;
        public bool HasDynamicWindowTitle;
        public string[] SymlinkFiles;
        public bool HookInitDelay;
        public bool HookInit;
        public string[] CopyFiles;
        public bool SetWindowHook;
        public bool HideTaskbar;
        public bool PromptBetweenInstances;
        public bool HideCursor;
        public bool RenameNotKillMutex;
        public bool IdInWindowTitle;
        public bool ChangeExe;
        public string GamepadGuid;
        public bool UseX360ce;
        public string HookFocusInstances;
        public bool KeepAspectRatio;
        public bool HideDesktop;
        public bool ResetWindows;
        public bool PartialMutexSearch;
        public bool UseGoldberg;
        public bool GoldbergNoWarning = false;
        public string OrigSteamDllPath;
        public bool GoldbergNeedSteamInterface;
        //deprecated kept for backward compatibility => use SteamlessPatch insatead
        public bool UseSteamless = false;
        public string SteamlessArgs;
        public int SteamlessTiming = 2500;
        //
        public string[] SteamlessPatch;//bool apply to launcher,string Steamless Args,int wait for exe pacthing finished.
        public bool UseGoldbergNoOGSteamDlls;
        public string[] CustomSteamApiDllPath;
        public bool XboxOneControllerFix;
        public bool UseForceBindIP;
        public string[] XInputPlusDll;
        public string[] SDLPaths;//Relative to instanced game directory root 
        public string[] CopyCustomUtils;
        public int PlayersPerInstance;
        public bool UseDevReorder;
        public string[] HexEditAllExes;
        public string[] HexEditExe;
        public bool BlockRawInput;
        public string[] HexEditFile;
        public string[] HexEditAllFiles;
        public bool SetWindowHookStart;
        public string GoldbergLanguage;
        public string Description;
        public bool GoldbergExperimental;
        public bool GoldbergIgnoreSteamAppId;
        public bool UseSteamStubDRMPatcher;
        public bool HardlinkGame;
        public bool SetForegroundWindowElsewhere;
        public bool PreventWindowDeactivation;
        public bool SymlinkFolders;
        public bool CreateSteamAppIdByExe;
        public bool ForceSymlink;
        public string[] UseProcessorsPerInstance;
        public bool UseDirectX9Wrapper;
        public string SteamStubDRMPatcherArch;
        public bool GoldbergLobbyConnect;
        public string[] X360ceDll;
        public string[] CMDBatchBefore;
        public string[] CMDBatchAfter;
        public bool GoldbergNoLocalSave;
        public bool UseNucleusEnvironment;
        public bool ThirdPartyLaunch;
        public bool ForceProcessPick;
        public bool KeepMonitorAspectRatio;
        public string PostHookInstances;
        public string StartHookInstances;
        public string[] KillMutexLauncher;
        public string KillMutexTypeLauncher = "Mutant";
        public int KillMutexDelayLauncher;
        public bool PartialMutexSearchLauncher;
        public string FakeFocusInstances;
        public bool KeyboardPlayerSkipFakeFocus;
        public string UserProfileConfigPath;
        public string UserProfileSavePath;
        public string[] PlayerSteamIDs;
        public bool UseHandlerSteamIds;
        public string[] HexEditExeAddress;
        public string[] HexEditFileAddress;
        public bool ForceUserProfileConfigCopy;
        public bool ForceUserProfileSaveCopy;
        public bool PromptBeforeProcessGrab;
        public bool ProcessChangesAtEnd;
        public bool PromptProcessChangesAtEnd;
        public string[] DeleteFilesInConfigPath;
        public string[] DeleteFilesInSavePath;
        public bool PromptBetweenInstancesEnd;
        public bool IgnoreDeleteFilesPrompt;
        public bool ChangeIPPerInstance;
        public string FlawlessWidescreen;
        public string[] RenameAndOrMoveFiles;
        public string[] DeleteFiles;
        public bool GoldbergExperimentalRename;
        public string[] KillProcessesOnClose;
        public bool KeyboardPlayerSkipPreventWindowDeactivate;
        public bool DontResize;
        public bool DontReposition;
        public bool NotTopMost;
        public string[] WindowStyleValues;
        public string[] ExtWindowStyleValues;
        public bool KillLastInstanceMutex;
        public bool RefreshWindowAfterStart = false;
        public bool CreateSingleDeviceFile;
        public bool KillMutexAtEnd;
        public bool CMDStartArgsInside;
        public bool UseEACBypass;
        public bool LaunchAsDifferentUsers;
        public bool RunLauncherAndExe;
        public int PauseBetweenProcessGrab;
        public int PauseBetweenContextAndLaunch;
        public bool DirSymlinkCopyInsteadIncludeSubFolders;
        public bool LaunchAsDifferentUsersAlt;
        public bool ChangeIPPerInstanceAlt;
        public bool GamePlayAfterLaunch;
        public bool UserProfileConfigPathNoCopy;
        public bool UserProfileSavePathNoCopy;
        public bool LauncherExeIgnoreFileCheck;
        public bool ForceBindIPDelay;
        public bool ForceBindIPNoDummy = false;
        public string[] CustomUserGeneralPrompts;
        public bool SaveCustomUserGeneralValues;
        public string[] CustomUserPlayerPrompts;
        public bool SaveCustomUserPlayerValues;
        public string[] CustomUserInstancePrompts;
        public bool SaveCustomUserInstanceValues;
        public bool SaveAndEditCustomUserGeneralValues;
        public bool SaveAndEditCustomUserPlayerValues;
        public bool SaveAndEditCustomUserInstanceValues;
        public bool TransferNucleusUserAccountProfiles;
        public bool UseCurrentUserEnvironment = false;
        public bool EnableWindows;
        public string[] WindowStyleEndChanges;
        public string[] ExtWindowStyleEndChanges;
        public bool UseDInputBlocker;
        public bool IgnoreThirdPartyPrompt;
        public string ExecutableToLaunch;
        public bool GoldbergWriteSteamIDAndAccount;
        public bool ForceProcessSearch;
        public bool IgnoreWindowBorderCheck;
        public string WriteToProcessMemory;
        public bool UseNemirtingasEpicEmu;
        public bool UseNemirtingasGalaxyEmu;
        public bool EpicEmuArgs;
        public bool AltEpicEmuArgs;
        public bool PromptAfterFirstInstance;
        public bool FakeFocusSendActivateIgnoreKB;
        public string[] CopyEnvFoldersToNucleusAccounts;
        public string[] CopyFoldersTo;
        public string[] SymlinkFoldersTo;
        public string[] HardlinkFoldersTo;
        public bool GamePlayBeforeGameSetup;
        public bool RequiresAdmin;
        public int PauseCMDBatchBefore;
        public int PauseCMDBatchAfter;
        public bool DontRemoveBorders;
        public string[] KillMutexProcess;
        public string MutexProcessExe;
        public bool PartialMutexSearchProcess;
        public string KillMutexTypeProcess = "Mutant";
        public bool GoldbergExperimentalSteamClient;
        public int PauseBeforeMutexKilling;
        public int KillMutexDelayProcess;
        public bool XInputPlusNoIni;
        public string DocumentsConfigPath;
        public string DocumentsSavePath;
        public bool ForceDocumentsConfigCopy;
        public bool ForceDocumentsSaveCopy;
        public bool DocumentsConfigPathNoCopy;
        public bool DocumentsSavePathNoCopy;
        public string FlawlessWidescreenPluginPath;
        public bool XInputPlusOldDll;
        public bool FlawlessWidescreenOverrideDisplay;
        public string[] CMDBatchClose;
        public string ForceGameArch;
        public string[] SSEAdditionalLines;
        public string[] DeleteOnClose;
        public float[] DesktopScale;
        // -- From USS
        //Effectively a switch for all of USS features
        public bool SupportsMultipleKeyboardsAndMice;

        //Hooks
        public bool HookSetCursorPos = true;
        public bool HookGetCursorPos = true;
        public bool HookGetKeyState = true;
        public bool HookGetAsyncKeyState = true;
        public bool HookGetKeyboardState = true;
        public bool HookFilterRawInput;
        public bool HookFilterMouseMessages;
        public bool HookUseLegacyInput;
        public bool HookDontUpdateLegacyInMouseMsg;
        public bool HookMouseVisibility = true;
        public bool HookReRegisterRawInput = false;
        public bool HookReRegisterRawInputMouse = true;
        public bool HookReRegisterRawInputKeyboard = true;
        public bool InjectHookXinput = false;
        public bool InjectDinputToXinputTranslation = false;
        //Not hooks
        public bool SendNormalMouseInput = true;
        public bool SendNormalKeyboardInput = true;
        public bool SendScrollWheel = false;
        public bool ForwardRawKeyboardInput = false;
        public bool ForwardRawMouseInput = false;
        public bool DrawFakeMouseCursor = true;
        public bool DrawFakeMouseCursorForControllers = false;
        public bool UpdateFakeMouseWithInternalInput = false;
        public bool LockInputAtStart = false;
        public bool PreventGameFocus = false;
        public int LockInputToggleKey = 0x23;//End by default. Keys: https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        public bool ForceEnvironmentUse;
        public bool ForceLauncherExeIgnoreFileCheck;


        // Proto Input
        public ProtoInputOptions ProtoInput = new ProtoInputOptions();
        public bool LockInputSuspendsExplorer = false;
        public bool ToggleUnfocusOnInputsLock = false;//v.2.1.2 see RawInputProcessor & GenericGameHandler
        public string[] CustomHotkeys;

        public Action OnCustomHotKey_1;
        public Action OnCustomHotKey_2;
        public Action OnCustomHotKey_3;

        public Type HandlerType => typeof(GenericGameHandler);
        public GameMetaInfo MetaInfo = new GameMetaInfo();

        public GenericGameInfo(string fileName, string folderPath, Stream str, bool[] checkUpdate)//checkUpdate [0]=initial update check [1]=update was available before reloading the handler  
        {
            JsFileName = fileName;
            Folder = folderPath;

            js = "";
            using (StreamReader sr = new StreamReader(str))
            {
                while (!sr.EndOfStream)
                {

                    string line = sr.ReadLine();
                    {
                        js += "\r\n" + line + "\r\n";
                    }
                }
            }

            Assembly assembly = typeof(GameOption).Assembly;

            engine = new Engine(cfg => cfg.AllowClr(assembly));

            engine.SetValue("Game", this);
            engine.SetValue("Hub", Hub);
            engine.Execute("var Nucleus = importNamespace('Nucleus.Gaming');");

            try
            {
                engine.Execute(js); 
            }
            catch (Exception ex)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    string[] lineSplit = ex.Message.Split(':');
                    string splited = lineSplit?[0];
                    string[] getNum = splited.Split(' ');

                    bool lineIsNum = int.TryParse(getNum?[1], out int numLine);

                    string details = ex.Message;

                    if (lineIsNum)
                    {
                        int num = int.Parse(getNum?[1]);
                        details = $"Error at line {num/2} {lineSplit?[1]}";
                    }

                    string error = $"There is an error in the game handler {fileName}.\n" +
                    $"\nThe game this handler is for will not appear in the list. If the issue has been fixed,\n" +
                    $"please try re-adding the game.\n\nCommon errors include:\n- A syntax error (such as a \',\' \';\' or \']\' missing)\n" +
                    $"- Another handler has this GUID (must be unique!)\n- Code is not in the right place or format\n(for example: methods using Context must be within the Game.Play function)" +
                    $"\n\n{details}";

                    NucleusMessageBox.Show("Error in handler", error, false);              
                });

                return;
            }

            MetaInfo.LoadGameMetaInfo(this);

            if (MetaInfo.CheckUpdate)
            {
                if (checkUpdate[0])//workaround else handler update is checked before instances setup too,
                                   //see MainForm.cs Btn_Play_Click(object sender, EventArgs e) => gameManager.AddScript.
                {
                    // Run this in another thread to not block UI
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        UpdateAvailable = Hub.IsUpdateAvailable(true);
                    });
                }
                else
                {
                    UpdateAvailable = checkUpdate[1];
                }
            }

            engine.SetValue("Game", (object)null);
        }

        public CustomStep ShowOptionAsStep(string optionKey, bool required, string title)
        {
            GameOption option = Options.First(c => c.Key == optionKey);
            option.Hidden = true;

            CustomStep step = new CustomStep
            {
                Option = option,
                Required = required,
                Title = title
            };

            CustomSteps.Add(step);
            return step;
        }

        public void SetPlayerList(List<PlayerInfo> players)
        {
            engine.SetValue("PlayerList", players);
        }

        public void SetProtoInputValues()
        {
            engine.SetValue("ProtoInput", Coop.ProtoInput.ProtoInput.protoInput);
            //engine.SetValue("ProtoInputValues", Coop.ProtoInput.ProtoInput.exposedValues);
        }

        public void PrePlay(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            engine.SetValue("Context", handlerInstance.context);
            engine.SetValue("Handler", handlerInstance);
            engine.SetValue("Player", player);
            engine.SetValue("Game", this);
            engine.SetValue("Hub", Hub);

            Play?.Invoke();
        }

        /// <summary>
        /// Clones this Game Info into a new Generic Context
        /// </summary>
        /// <returns></returns>
        public GenericContext CreateContext(GameProfile profile, PlayerInfo info, bool hasKeyboardPlayer)
        {
            var handlerInstance = GenericGameHandler.Instance;

            GenericContext context = new GenericContext(profile, info, handlerInstance, hasKeyboardPlayer);

            Type t = GetType();
            PropertyInfo[] props = t.GetProperties();
            FieldInfo[] fields = t.GetFields();

            Type c = context.GetType();
            PropertyInfo[] cprops = c.GetProperties();
            FieldInfo[] cfields = c.GetFields();

            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo p = props[i];
                PropertyInfo d = cprops.FirstOrDefault(k => k.Name == p.Name);
                if (d == null)
                {
                    continue;
                }

                if (p.PropertyType != d.PropertyType ||
                    !d.CanWrite)
                {
                    continue;
                }

                object value = p.GetValue(this, null);
                d.SetValue(context, value, null);
            }

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo source = fields[i];
                FieldInfo dest = cfields.FirstOrDefault(k => k.Name == source.Name);
                if (dest == null)
                {
                    continue;
                }

                if (source.FieldType != dest.FieldType)
                {
                    continue;
                }

                object value = source.GetValue(this);
                dest.SetValue(context, value);
            }

            return context;
        }


        public List<string> Profiles_ConfigPath => GetProfConfigPath();
        private List<string> GetProfConfigPath()
        {
            UpdateProfilesPath();

            List<string> playersProfilePath = new List<string>();

            if (UserProfileConfigPath?.Length > 0)
            {
                if (profilesPath.Count > 0)
                {
                    try
                    {
                        foreach (string profilePath in profilesPath)
                        {
                            string currPath = Path.Combine(profilePath, UserProfileConfigPath);
                            if (Directory.Exists(currPath))
                            {
                                playersProfilePath.Add(profilePath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return playersProfilePath;
                    }
                }

                return playersProfilePath;
            }

            return playersProfilePath;
        }

        public List<string> Profiles_SavePath => GetProfSavePath();
        private List<string> GetProfSavePath()
        {
            UpdateProfilesPath();

            List<string> playersProfilePath = new List<string>();

            if (UserProfileSavePath?.Length > 0)
            {
                if (profilesPath.Count > 0)
                {
                    try
                    {
                        foreach (string profilePath in profilesPath)
                        {
                            string currPath = Path.Combine(profilePath, UserProfileSavePath);
                            if (Directory.Exists(currPath))
                            {
                                playersProfilePath.Add(profilePath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return playersProfilePath;
                    }
                }

                return playersProfilePath;
            }

            return playersProfilePath;
        }


        public List<string> Profiles_Documents_ConfigPath => GetProfDocConfigPath();
        private List<string> GetProfDocConfigPath()
        {
            UpdateProfilesPath();

            List<string> playersProfilePath = new List<string>();

            if (DocumentsConfigPath?.Length > 0)
            {
                if (profilesPath.Count > 0)
                {
                    try
                    {
                        foreach (string profilePath in profilesPath)
                        {
                            string currPath = Path.Combine(profilePath, DocumentsConfigPath);
                            if (Directory.Exists(currPath))
                            {
                                playersProfilePath.Add(profilePath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return playersProfilePath;
                    }
                }

                return playersProfilePath;
            }

            return playersProfilePath;
        }

        public List<string> Profiles_Documents_SavePath => GetProfDocSavePath();
        private List<string> GetProfDocSavePath()
        {
            UpdateProfilesPath();

            List<string> playersProfilePath = new List<string>();

            if (DocumentsSavePath?.Length > 0)
            {
                if (profilesPath.Count > 0)
                {
                    try
                    {
                        foreach (string profilePath in profilesPath)
                        {
                            string currPath = Path.Combine(profilePath, DocumentsSavePath);
                            if (Directory.Exists(currPath))
                            {
                                playersProfilePath.Add(profilePath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return playersProfilePath;
                    }
                }

                return playersProfilePath;
            }

            return playersProfilePath;
        }

        public List<string> Players_BackupPath => GetPlayersBackupPath();
        private List<string> GetPlayersBackupPath()
        {
            List<string> playersBackupPath = new List<string>();

            string backupsPath = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\_Game Files Backup_\{GUID}";

            if (Directory.Exists(backupsPath))
            {
                return Directory.GetDirectories(backupsPath, "*", SearchOption.TopDirectoryOnly).ToList();
            }

            return playersBackupPath;
        }


        public string Content_Folder => GetContentDir();
        private string GetContentDir()
        {
            GameManager gameManager = GameManager.Instance;
            string path = Path.Combine(gameManager.GetAppContentPath(), GUID);

            if (Directory.Exists(path))
            {
                return path;
            }

            return string.Empty;
        }

        private void UpdateProfilesPath()
        {
            profilesPath.Clear();
            profilesPath.Add(Environment.GetEnvironmentVariable("userprofile"));
            profilesPath.Add(Globals.UserDocumentsRoot);

            if (UseNucleusEnvironment)
            {
                string targetDirectory = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\";

                if (Directory.Exists(targetDirectory))
                {
                    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory, "*", SearchOption.TopDirectoryOnly);
                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        profilesPath.Add(subdirectory);
                        if ($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\" == targetDirectory)
                        {
                            profilesPath.Add(subdirectory + "\\Documents");
                        }
                    }
                }

                if ($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\" != targetDirectory)
                {
                    targetDirectory = $@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\";
                    if (Directory.Exists(targetDirectory))
                    {
                        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory, "*", SearchOption.TopDirectoryOnly);
                        foreach (string subdirectory in subdirectoryEntries)
                        {
                            profilesPath.Add(subdirectory + "\\Documents");
                        }
                    }
                }
            }
        }

        public string EpicLang => NemirtingasEpicEmu.GetEpicLanguage();

        public string GogLang => NemirtingasGalaxyEmu.GetGogLanguage();

        public string SteamLang => SteamFunctions.GetUserSteamLanguageChoice(this);
    }
}
