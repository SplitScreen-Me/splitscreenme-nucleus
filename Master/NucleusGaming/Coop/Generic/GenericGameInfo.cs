using Jint;
using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Generic.Step;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class GenericGameInfo
    {
        private Engine engine;
        private string js;

        private Hub Hub = new Hub();

        public GameHookInfo Hook = new GameHookInfo();
        public List<GameOption> Options = new List<GameOption>();

        public SaveType SaveType;
        public string SavePath;

        public string[] DirSymlinkExclusions;
        public string[] FileSymlinkExclusions;
        public string[] FileSymlinkCopyInstead;
        public string[] DirSymlinkCopyInstead;
        public string[] DirExclusions;
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

        public string StartArguments;
        public string BinariesFolder;

        public bool FakeFocus;
        public int FakeFocusInterval = 1000;//TODO: high CPU usage with low value?
        public bool FakeFocusSendActivate = true;
        public bool SendFakeFocusMsg;
        public bool SplitDivCompatibility = true;

        public void AddOption(string name, string desc, string key, object value, object defaultValue)
        {
            Options.Add(new GameOption(name, desc, key, value, defaultValue));
        }

        public void AddOption(string name, string desc, string key, object value)
        {
            Options.Add(new GameOption(name, desc, key, value));
        }

        /// <summary>
        /// The relative path to where the games starts in
        /// </summary>
        public string WorkingFolder;
        public bool NeedsSteamEmulation;
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
        public string OrigSteamDllPath;
        public bool GoldbergNeedSteamInterface;
        public bool XboxOneControllerFix;
        public bool UseForceBindIP;
        public string[] XInputPlusDll;
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
        //public bool RunAsAdmin;
        public bool SetForegroundWindowElsewhere;
        //public bool GoldbergLobbyConnect;
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
        //public string NetworkInterface = null;
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
        public bool GoldbergIgnoreFileCheck;
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

        public Type HandlerType => typeof(GenericGameHandler);

        public GenericGameInfo(string fileName, string folderPath, Stream str)
        {
            JsFileName = fileName;
            Folder = folderPath;

            //StreamReader reader = new StreamReader(str);
            //js = reader.ReadToEnd();
            js = "";
            using (StreamReader sr = new StreamReader(str))
            {
                while (!sr.EndOfStream)
                {
                    
                    string line = sr.ReadLine();
                    // if (line.StartsWith("Hub."))
                    // {
                    //     continue;
                    // }
                    // else
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
                MessageBox.Show(string.Format("There is an error in the game handler {0}. The game this handler is for will not appear in the list. If the issue has been fixed, please try re-adding the game.\n\nCommon errors include:\n- A syntax error (such as a \',\' \';\' or \']\' missing)\n- Another handler has this GUID (must be unique!)\n- Code is not in the right place or format (for example: methods using Context must be within the Game.Play function)\n\n{1}: {2}", fileName, ex.InnerException, ex.Message), "Error in handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Run this in another thread to not block UI
            System.Threading.Tasks.Task.Run(() =>
            {
                bool update = Hub.IsUpdateAvailable(true);

                //if (update)
                //{
                //    MessageBox.Show("Update is available for " + GameName);
                //}
            });

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

        public void PrePlay(GenericContext context, GenericGameHandler handler, PlayerInfo player)
        {
            engine.SetValue("Context", context);
            engine.SetValue("Handler", handler);
            engine.SetValue("Player", player);
            engine.SetValue("Game", this);
            engine.SetValue("Hub", Hub);

            Play?.Invoke();
        }

        /// <summary>
        /// Clones this Game Info into a new Generic Context
        /// </summary>
        /// <returns></returns>
        public GenericContext CreateContext(GameProfile profile, PlayerInfo info, GenericGameHandler handler, bool hasKeyboardPlayer)
        {
            GenericContext context = new GenericContext(profile, info, handler, hasKeyboardPlayer);

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

        //public string GetEpicLanguage()
        //{
        //    IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        //    string EpicLang = ini.IniReadValue("Misc", "EpicLang"); ;

        //    if (EpicLang == "" )
        //    {
        //        EpicLang = "en";
        //    }

        //    return EpicLang;
        //}
        private string EpicLang;
        
        public string GetEpicLanguage()
        {
            IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

            IDictionary<string, string> epiclangs = new Dictionary<string, string>();
                epiclangs.Add("Arabic", "ar");
                epiclangs.Add("Brazilian", "pt-BR");
                epiclangs.Add("Bulgarian", "bg");
                epiclangs.Add("Chinese", "zh");
                epiclangs.Add("Czech", "cs");
                epiclangs.Add("Danish", "da");
                epiclangs.Add("Dutch", "nl");
                epiclangs.Add("English", "en");
                epiclangs.Add("Finnish", "fi");
                epiclangs.Add("French", "fr");
                epiclangs.Add("German", "de");
                epiclangs.Add("Greek", "el");
                epiclangs.Add("Hungarian", "hu");
                epiclangs.Add("Italian", "it");
                epiclangs.Add("Japanese", "ja");
                epiclangs.Add("Koreana", "ko");
                epiclangs.Add("Norwegian", "no");
                epiclangs.Add("Polish", "pl");
                epiclangs.Add("Portuguese", "pt");
                epiclangs.Add("Romanian", "ro");
                epiclangs.Add("Russian", "ru");
                epiclangs.Add("Spanish", "es");
                epiclangs.Add("Swedish", "sv");
                epiclangs.Add("Thai", "th");
                epiclangs.Add("Turkish", "tr");
                epiclangs.Add("Ukrainian", "uk");
       
            foreach (KeyValuePair<string, string> lang in epiclangs)
            {
                if (lang.Key == ini.IniReadValue("Misc", "EpicLang"))
                {
                    EpicLang = lang.Value;
                    Console.WriteLine(EpicLang);
                }
            }
            return EpicLang;
        }

        private string GogLang;
        public string GetGogLanguage()
        {
            IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

            IDictionary<string, string> epiclangs = new Dictionary<string, string>();
            epiclangs.Add("Arabic", "ar");
            epiclangs.Add("Brazilian", "pt-BR");
            epiclangs.Add("Bulgarian", "bg");
            epiclangs.Add("Chinese", "zh");
            epiclangs.Add("Czech", "cs");
            epiclangs.Add("Danish", "da");
            epiclangs.Add("Dutch", "nl");
            epiclangs.Add("English", "en");
            epiclangs.Add("Finnish", "fi");
            epiclangs.Add("French", "fr");
            epiclangs.Add("German", "de");
            epiclangs.Add("Greek", "el");
            epiclangs.Add("Hungarian", "hu");
            epiclangs.Add("Italian", "it");
            epiclangs.Add("Japanese", "ja");
            epiclangs.Add("Koreana", "ko");
            epiclangs.Add("Norwegian", "no");
            epiclangs.Add("Polish", "pl");
            epiclangs.Add("Portuguese", "pt");
            epiclangs.Add("Romanian", "ro");
            epiclangs.Add("Russian", "ru");
            epiclangs.Add("Spanish", "es");
            epiclangs.Add("Swedish", "sv");
            epiclangs.Add("Thai", "th");
            epiclangs.Add("Turkish", "tr");
            epiclangs.Add("Ukrainian", "uk");

            foreach (KeyValuePair<string, string> lang in epiclangs)
            {
                if (lang.Key == ini.IniReadValue("Misc", "EpicLang"))
                {
                    GogLang = lang.Key;
                    Console.WriteLine(GogLang);
                }
            }
            return GogLang;
        }

        public string GetSteamLanguage()
        {
            string result;
            if (Environment.Is64BitOperatingSystem)
            {
                result = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "Language", "english");
            }
            else
            {
                result = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "Language", "english");
            }

            return result;
        }

        public bool IsUpdateAvailable(bool fetch)
        {
            return Hub.IsUpdateAvailable(fetch);
        }

        public string GetScreenshots()
        {
            return Hub.GetScreenshots();
        }

        public string GetHubId()
        {
            return Hub.Handler.Id;
        }
    }
}
