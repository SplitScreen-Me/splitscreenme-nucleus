using Jint.Native;
using Microsoft.Win32;
using Nucleus.Coop.Forms;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.Generic.Cursor;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Platform.PCSpecs;
using Nucleus.Gaming.Tools;
using Nucleus.Gaming.Tools.AudioReroute;
using Nucleus.Gaming.Tools.BackupDatas;
using Nucleus.Gaming.Tools.DevReorder;
using Nucleus.Gaming.Tools.DInput8CoopLvlUnlocker;
using Nucleus.Gaming.Tools.DInputBlocker;
using Nucleus.Gaming.Tools.DirectX9Wrapper;
using Nucleus.Gaming.Tools.DllsInjector;
using Nucleus.Gaming.Tools.EACBypass;
using Nucleus.Gaming.Tools.FlawlessWidescreen;
using Nucleus.Gaming.Tools.GameStarter;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.Tools.HexEdit;
using Nucleus.Gaming.Tools.MonitorsDpiScaling;
using Nucleus.Gaming.Tools.NemirtingasEpicEmu;
using Nucleus.Gaming.Tools.NemirtingasGalaxyEmu;
using Nucleus.Gaming.Tools.Network;
using Nucleus.Gaming.Tools.NucleusUsers;
using Nucleus.Gaming.Tools.SDL2Dll;
using Nucleus.Gaming.Tools.Steam;
using Nucleus.Gaming.Tools.WindowFakeFocus;
using Nucleus.Gaming.Tools.X360ce;
using Nucleus.Gaming.Tools.XInputPlusDll;
using Nucleus.Gaming.Util;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WindowScrape.Constants;
using static Nucleus.Gaming.GameDpiAwareness;

namespace Nucleus.Gaming
{
    public class GenericGameHandler : IGameHandler, ILogNode
    {
        public  static bool IsRunning = false;
        public List<WPFDiv> splitForms = new List<WPFDiv>();
        public List<ShortcutsReminder> shortcutsReminders = new List<ShortcutsReminder>();
        public List<Form> AllRuntimeForms = new List<Form>();

        private string origExePath;
        private string NucleusFolderPath => Globals.NucleusInstallRoot;
        public string exePath;
        public string instanceExeFolder;
        public string garch;
        public string nucleusUserAccountsPassword = "12345";
        public string JsFilename;
        public string HandlerGUID;
        public static string ofdPath;
        public string ssePath = string.Empty;
        public string[] folderUsers;

        private int prevProcId = 0;
        public int plyrIndex = 0;
        public int kbi = 1;
        public int origWidth = 0;
        public int origHeight = 0;
        public int playerBoundsWidth = 0;
        public int playerBoundsHeight = 0;
        public int prevWindowWidth = 0;
        public int prevWindowHeight = 0;
        public int prevWindowX = 0;
        public int prevWindowY = 0;
        public int exited;
        public int[] procOrder;
        public int keyboardProcId;
        public int numPlayers = 0;
        protected int totalPlayers;
        public int TotalPlayers => totalPlayers;

        public double origRatio = 1;
        public double Timer { get; set; }
        protected double timerInterval = 1000;
        public double TimerInterval => timerInterval;

        public int HWndInterval = 10000;

        internal CursorModule _cursorModule { get; set; }
        public GameProfile profile;
        private GenericGameInfo gen;
        public GenericGameInfo CurrentGameInfo => gen;

        private GenericContext context;
        public GenericContext Context => context;
        
        private UserGameInfo userGame;
        public ProcessData prevProcessData;
        public Process launchProc;
        public UserScreen owner;

        public Thread FakeFocus => WindowFakeFocus.fakeFocus;

        public event Action Ended;

        private static GenericGameHandler instance;
        public static GenericGameHandler Instance => instance;

        public Dictionary<string, string> jsData;

        private List<Process> attachedLaunchers = new List<Process>();
        private List<int> attachedIdsLaunchers = new List<int>();
        private List<int> mutexProcs = new List<int>();
        public List<string> userBackedFiles = new List<string>();
        public List<Display> screensInUse = new List<Display>();
        public List<string> nucUsers = new List<string>();
        public List<string> nucSIDs = new List<string>();
        public List<string> addedFiles = new List<string>();
        public List<string> backupFiles = new List<string>();
        public List<Process> attached = new List<Process>();
        public List<int> attachedIds = new List<int>();

        private bool earlyExit;
        public bool processingExit = false;
        private bool useDocs;
        private bool symlinkNeeded;
        private bool isPrevent;
        private bool hasKeyboardPlayer = false;
        public bool hasEnded;
        public virtual bool HasEnded => hasEnded;
        public bool gameIs64 { get; set; }
        public bool UsingNucleusAccounts;
        public bool isDebug;
        public bool dllResize = false;
        public bool dllRepos = false;
        public string error;

        public bool Initialize(UserGameInfo game, GameProfile profile, IGameHandler handler)
        {
            instance = this;
            userGame = game;
            this.profile = profile;

            isDebug = App_Misc.DebugLog;
           
            Network.iniNetworkInterface = GameProfile.Network;

            List<PlayerInfo> players = profile.DevicesList;

            gen = game.Game;
            // see if we have any save game to backup
            if (gen == null)
            {
                // you fucked up
                return false;
            }

            RawInputProcessor.CurrentGameInfo = gen;

            try
            {
                for (int pl = 0; pl < players.Count; pl++)
                {
                    if (players[pl].IsKeyboardPlayer)
                    {
                        hasKeyboardPlayer = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

            if (gen.LockMouse)
            {
                _cursorModule = new CursorModule();
            }

            jsData = new Dictionary<string, string>
            {
                { Folder.Documents.ToString(), Globals.UserDocumentsRoot },
                { Folder.MainGameFolder.ToString(), Path.GetDirectoryName(game.ExePath) },
                { Folder.InstancedGameFolder.ToString(), Path.GetDirectoryName(game.ExePath) }
            };

            timerInterval = gen.HandlerInterval;

            LogManager.RegisterForLogCallback(this);

            JsFilename = gen.JsFileName;
            HandlerGUID = gen.GUID;

            UsingNucleusAccounts = gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt;
            
            if (GameProfile.HWndInterval > 0)
            {
                HWndInterval = GameProfile.HWndInterval;
                Log($"Set Windows Setup Timing to {HWndInterval} ms");
            }

            totalPlayers = profile.DevicesList.Count;

            error = null;

            Thread PlayThread = new Thread(delegate ()
            {
                StartPlay(handler);
                System.Windows.Threading.Dispatcher.Run();
            });

            PlayThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            PlayThread.Start();

            if (TimerInterval > 0)
            {
                StartUpdateTick(gen.HandlerInterval);
            }

            HotkeyListenerThread.StartHotkeyListener();

            return true;
        }

        private void StartPlay(object handler)
        {
            try
            {
                error = ((IGameHandler)handler).Play();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                TaskbarState.Show.Invoke();

                try
                {                   
                    RegistryUtil.RestoreRegistry("Error Restore from GenericGameHandler");
                    // try to save the exception
                    LogManager.Instance.LogExceptionFile(ex);
                }
                catch
                {
                    error = "We failed so hard we failed while trying to record the reason we failed initially. Sorry.";
                    return;
                }
            }
        }

        public string Play()
        {
            if (!App_Misc.IgnoreInputLockReminder)
            {
                MessageBox.Show("Some handlers will require you to press the End key to lock input. Remember to unlock input by pressing End again when you finish playing. You can disable this message in the Settings. ", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
            }

            if (GameProfile.Stop_UINav) { GamepadNavigation.StopUINavigation(); }

            if (gen.NeedSteamClient)
            {
                SteamFunctions.StartSteamClient();
            }

            TaskbarState.Hide.Invoke();

            garch = "x86";

            if (MachineSpecs.GetMachineArch(userGame.ExePath) == true)
            {
                gameIs64 = true;
                garch = "x64";
            }

            if (gen.ForceGameArch?.Length > 0)
            {
                if (gen.ForceGameArch == "x86")
                {
                    gameIs64 = false;
                    garch = "x86";
                }
                else if (gen.ForceGameArch == "x64")
                {
                    gameIs64 = true;
                    garch = "x64";
                }
            }

            if (isDebug)
            {
                Log($"-Nucleus Version: {Globals.Version}");
                Log($"-Nucleus install location: {Globals.NucleusInstallRoot}");
                Log($"-Original game executable location: {userGame.ExePath}");
                if (Globals.IsOneDriveEnabled)
                {
                    Log($@"/!\  USER DOCUMENTS PATH IS IN ONEDRIVE  /!\  -> {Globals.UserDocumentsRoot} ");
                }

                MachineSpecs.GetPCspecs();
                Log(string.Format("-Game: {0}, Arch: {1}, Executable: {2}, Launcher: {3}, SteamID: {4}, Handler: {5}, Content Folder: {6}", gen.GameName, garch, gen.ExecutableName, gen.LauncherExe, gen.SteamID, gen.JsFileName, gen.GUID));
                Log(string.Format("-Number of players: {0}", profile.DevicesList.Count) + "\n");
                Log("--------------------- START ---------------------");
            }

            ProcessUtil.KillRemainingProcess();

            List<PlayerInfo> players = profile.DevicesList;

            Log("Determining which monitors will be used by Nucleus");

            foreach (Display dp in ScreensUtil.AllScreensParams())
            {
                if (players.Any(p => p.Owner.DisplayIndex == dp.DisplayIndex))
                {
                    screensInUse.Add(dp);
                }
            }

            if (GameProfile.AutoDesktopScaling == true)
            {
                MonitorsDpiScaling.SetupMonitors();
            }
            else
            {
                Log("The Windows deskop scale will not be set to 100% because this option has been disabled in settings and/or game profile");
            }

            bool hasMerger = false;

            if (GameProfile.EnableWindowsMerger && !gen.MetaInfo.DisableProfiles && !App_Misc.DisableGameProfiles)
            {
                string[] mergerRes = GameProfile.MergerResolution.Split('X');
                WindowsMergerThread.StartWindowsMerger(new System.Windows.Size(int.Parse(mergerRes[0]), int.Parse(mergerRes[1])));
                hasMerger = true;
            }
            else if (App_Layouts.WindowsMerger && (gen.MetaInfo.DisableProfiles || App_Misc.DisableGameProfiles))
            {
                string[] mergerRes = App_Layouts.WindowsMergerRes.Split('X');
                WindowsMergerThread.StartWindowsMerger(new System.Windows.Size(int.Parse(mergerRes[0]), int.Parse(mergerRes[1])));
                hasMerger = true;
            }

            foreach (Display dp in screensInUse)
            {
                if (screensInUse.Contains(dp))
                {
                    if (!hasMerger)
                    {
                        if ((GameProfile.UseSplitDiv && gen.SplitDivCompatibility) || gen.HideDesktop)
                        {
                            WPFDivFormThread.StartBackgroundForm(gen, dp);
                        }
                    }

                    ReminderFormThread.StartReminderForms(dp.Bounds);
                }
            }

            gen.SetPlayerList(players);

            gen.SetProtoInputValues();

            UserScreen[] all = profile.Screens.ToArray();

            Log(string.Format("Display - DPIHandling: {0}, DPI Scale: {1}", gen.DPIHandling, DPIManager.Scale +"\n"));
            for (int x = 0; x < all.Length; x++)
            {
                Log(string.Format("Monitor {0} - Resolution: {1}", x, all[x].MonitorBounds.Width + "x" + all[x].MonitorBounds.Height));
            }

            string tempDir = GameManager.Instance.GempTempFolder(gen);
            string exeFolder = Path.GetDirectoryName(userGame.ExePath).ToLower();
            string rootFolder = exeFolder;
            string workingFolder = exeFolder;

            if (!string.IsNullOrEmpty(gen.BinariesFolder) && !gen.BinariesFolderPathFix)
            {
                rootFolder = StringUtil.ReplaceCaseInsensitive(exeFolder, gen.BinariesFolder.ToLower(), "");
            }
            else if (!string.IsNullOrEmpty(gen.BinariesFolder) && gen.BinariesFolderPathFix)
            {
                rootFolder = StringUtil.GetRootFromBinariesFolder(exeFolder, gen.BinariesFolder);
            }

            if (!string.IsNullOrEmpty(gen.WorkingFolder))
            {
                workingFolder = Path.Combine(exeFolder, gen.WorkingFolder.ToLower());
            }

            Log("Trying to unlock original game files." + "\n");
            if (!StartGameUtil.UnlockGameFiles(rootFolder))
            {
                End(false);
                return string.Empty;
            }

            gen.LockInputToggleKey = App_Hotkeys.LockKeyValue;

            RawInputManager.windows.Clear();
            Window nextWindowToInject = null;

            numPlayers = players.Count;

            if (isDebug)
            {     
                Log("########## START OF HANDLER ##########" + "\n");
                string line;

                StreamReader file = new StreamReader(Path.Combine(GameManager.Instance.GetJsScriptsPath(), gen.JsFileName));
                while ((line = file.ReadLine()) != null)
                {
                    Log(line);
                }

                file.Close();

                Log("########## END OF HANDLER ##########" + "\n");
            }

            if (App_Misc.NucleusAccountPassword != "12345" && App_Misc.NucleusAccountPassword != "")
            {
                nucleusUserAccountsPassword = App_Misc.NucleusAccountPassword;
            }

            for (int i = 0; i < players.Count; i++)
            {
                if (processingExit)
                {
                    return string.Empty;
                }

                IsRunning = true;
                Log($"********** Setting up player {i + 1} **********" + "\n");

                PlayerInfo player = players[i];

                player.PlayerID = i;

                plyrIndex = i;

                if (!GameProfile.UseNicknames)
                {
                    player.Nickname = $"Player{i + 1}";
                }

                ProcessData procData = player.ProcessData;
                bool hasSetted = procData != null && procData.Setted;

                if (gen.PauseBeforeMutexKilling > 0)
                {
                    Log(string.Format("Pausing for {0} seconds", gen.PauseBeforeMutexKilling));
                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBeforeMutexKilling));
                }

                if (i > 0 && (gen.KillMutexLauncher?.Length > 0))
                {
                    for (; ; )
                    {
                        if (exited > 0)
                        {
                            return "";
                        }

                        Thread.Sleep(1000);

                        if (gen.KillMutexLauncher != null)
                        {
                            if (gen.KillMutexLauncher.Length > 0)
                            {
                                if (gen.KillMutexDelayLauncher > 0)
                                {
                                    Thread.Sleep((gen.KillMutexDelayLauncher * 1000));
                                }

                                Process[] currentProcs = Process.GetProcesses();
                                Process launcher = null;
                                foreach (Process currentProc in currentProcs)
                                {
                                    if (currentProc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(gen.LauncherExe).ToLower())
                                    {
                                        if (!attachedIdsLaunchers.Contains(currentProc.Id))
                                        {
                                            launcher = currentProc;
                                            attachedIdsLaunchers.Add(currentProc.Id);
                                            attachedLaunchers.Add(currentProc);
                                        }
                                    }
                                }

                                if (launcher == null)
                                {
                                    Log("Could not find launcher process to kill mutexes");
                                    break;
                                }

                                if (StartGameUtil.MutexExists(launcher, gen.KillMutexTypeLauncher, gen.PartialMutexSearchLauncher, gen.KillMutexLauncher))
                                {
                                    // mutexes still exist, must kill
                                    StartGameUtil.KillMutex(launcher, gen.KillMutexTypeLauncher, gen.PartialMutexSearchLauncher, gen.KillMutexLauncher);
                                    break;
                                }
                                else
                                {
                                    // mutexes dont exist anymore
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                        if (processingExit)
                        {
                            break;
                        }
                    }
                }

                if (!gen.KillMutexAtEnd)
                {
                    if (!gen.RenameNotKillMutex && i > 0 && (gen.KillMutex?.Length > 0 || !hasSetted))
                    {
                        PlayerInfo before = players[i - 1];

                        for (; ; )
                        {
                            if (exited > 0)
                            {
                                return "";
                            }

                            Thread.Sleep(1000);

                            if (gen.KillMutex != null)
                            {
                                if (gen.KillMutex.Length > 0 && !before.ProcessData.KilledMutexes)
                                {
                                    // check for the existence of the mutexes
                                    // before invoking our StartGame app to kill them
                                    ProcessData pdata = before.ProcessData;

                                    if (gen.KillMutexDelay > 0)
                                    {
                                        Thread.Sleep((gen.KillMutexDelay * 1000));
                                    }

                                    if (StartGameUtil.MutexExists(pdata.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex))
                                    {
                                        // mutexes still exist, must kill
                                        StartGameUtil.KillMutex(pdata.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex);
                                        pdata.KilledMutexes = true;
                                        break;
                                    }
                                    else
                                    {
                                        // mutexes dont exist anymore
                                        break;
                                    }
                                }

                            }
                            else
                            {
                                break;
                            }

                            if (processingExit)
                            {
                                return string.Empty;
                            }
                        }
                    }
                }

                if (i > 0 && (gen.KillMutexProcess?.Length > 0))
                {
                    Log("A process has been provided for Nucleus to kill mutexes in");

                    for (; ; )
                    {
                        if (exited > 0)
                        {
                            return "";
                        }

                        Thread.Sleep(1000);

                        if (gen.KillMutexProcess != null)
                        {
                            if (gen.KillMutexProcess.Length > 0)
                            {
                                if (gen.KillMutexDelayProcess > 0)
                                {
                                    Thread.Sleep((gen.KillMutexDelayProcess * 1000));
                                }

                                Process[] currentProcs = Process.GetProcesses();
                                Process mProc = null;
                                foreach (Process currentProc in currentProcs)
                                {
                                    if (currentProc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(gen.MutexProcessExe).ToLower())
                                    {
                                        if (!mutexProcs.Contains(currentProc.Id))
                                        {
                                            Log($"Found process {currentProc} (pid {currentProc.Id}) to kill mutexes");
                                            mProc = currentProc;
                                            mutexProcs.Add(currentProc.Id);

                                        }
                                    }
                                }

                                if (mProc == null)
                                {
                                    Log("Could not find process to kill mutexes");
                                    break;
                                }

                                if (StartGameUtil.MutexExists(mProc, gen.KillMutexTypeProcess, gen.PartialMutexSearchProcess, gen.KillMutexProcess))
                                {
                                    // mutexes still exist, must kill
                                    StartGameUtil.KillMutex(mProc, gen.KillMutexTypeProcess, gen.PartialMutexSearchProcess, gen.KillMutexProcess);
                                    break;
                                }
                                else
                                {
                                    // mutexes dont exist anymore
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                        if (processingExit)
                        {
                            break;

                        }
                    }
                }

                if (i > 0 && !gen.ProcessChangesAtEnd && (gen.HookFocus || gen.SetWindowHook || gen.HideCursor || gen.PreventWindowDeactivation || gen.SupportsMultipleKeyboardsAndMice))
                {
                    if (gen.PreventWindowDeactivation && !isPrevent)
                    {
                        Log("PreventWindowDeactivation detected, setting flag");
                        isPrevent = true;
                    }

                    if (isPrevent)
                    {
                        if (players[i - 1].IsKeyboardPlayer && gen.KeyboardPlayerSkipPreventWindowDeactivate)
                        {
                            Log("Ignoring PreventWindowDeactivation for keyboard player");
                            gen.PreventWindowDeactivation = false;
                        }
                        else
                        {
                            Log("Keeping PreventWindowDeactivation on");
                            gen.PreventWindowDeactivation = true;
                        }
                    }

                    if (gen.PostHookInstances?.Length > 0)
                    {
                        string[] instancesToHook = gen.PostHookInstances.Split(',');
                        foreach (string instanceToHook in instancesToHook)
                        {
                            if (int.Parse(instanceToHook) == i)
                            {
                                Log("Injecting hook DLL for previous instance");
                                PlayerInfo before = players[i - 1];
                                Thread.Sleep(1000);
                                ProcessData pdata = before.ProcessData;
                                User32Interop.SetForegroundWindow(pdata.Process.NucleusGetMainWindowHandle());
                                DllsInjector.InjectDLLs(pdata.Process, nextWindowToInject, before);
                            }
                        }
                    }
                    else
                    {
                        Log("Injecting hook DLL for previous instance");
                        PlayerInfo before = players[i - 1];
                        Thread.Sleep(1000);
                        ProcessData pdata = before.ProcessData;
                        User32Interop.SetForegroundWindow(pdata.Process.NucleusGetMainWindowHandle());
                        DllsInjector.InjectDLLs(pdata.Process, nextWindowToInject, before);
                    }
                }

                Rectangle playerBounds = player.MonitorBounds;
                owner = player.Owner;

                int width = playerBounds.Width;
                int height = playerBounds.Height;
                Log($"Player monitor's resolution: {owner.MonitorBounds.Width} x {owner.MonitorBounds.Height}");
                bool isFullscreen = owner.Type == UserScreenType.FullScreen;

                string linkFolder;
                string linkBinFolder;
                string origRootFolder = "";

                if (gen.SymlinkGame || gen.HardcopyGame || gen.HardlinkGame)
                {
                    List<string> dirExclusions = new List<string>();
                    List<string> fileExclusions = new List<string>();
                    List<string> fileCopies = new List<string>();

                    // symlink the game folder (and not the bin folder, if we have one)
                    linkFolder = Path.Combine(tempDir, $"Instance{i}");

                    Log("Commencing file operations");

                    if ((i == 0 && (gen.SymlinkGame || gen.HardlinkGame)) || gen.HardcopyGame)
                    {
                        for (int f = 0; f < players.Count; f++)
                        {
                            string insFolder = Path.Combine(tempDir, "Instance" + f);
                            Log(string.Format("Creating instance folder {0}", insFolder.Substring(insFolder.IndexOf("content\\"))));
                            Directory.CreateDirectory(insFolder);
                        }
                    }

                    linkBinFolder = linkFolder;

                    if (!string.IsNullOrEmpty(gen.BinariesFolder))
                    {
                        linkBinFolder = Path.Combine(linkFolder, gen.BinariesFolder);
                    }

                    exePath = Path.Combine(linkBinFolder, userGame.Game.ExecutableName);

                    SetExeDpiAwareness(exePath);

                    origExePath = Path.Combine(linkBinFolder, userGame.Game.ExecutableName);

                    if ((i == 0 && (gen.SymlinkGame || gen.HardlinkGame)) || gen.HardcopyGame)
                    {
                        if (gen.CopyFiles != null)
                        {
                            Log($"Copying {gen.CopyFiles.Length} files in Game.CopyFiles");
                            string[] filesToCopy = gen.CopyFiles;
                            for (int c = 0; c < filesToCopy.Length; c++)
                            {
                                string s = filesToCopy[c].ToLower();
                                File.Copy(Path.Combine(rootFolder, s), Path.Combine(linkFolder, s), true);
                            }
                        }
                    }

                    origRootFolder = rootFolder;
                    instanceExeFolder = linkBinFolder;

                    if (!string.IsNullOrEmpty(gen.WorkingFolder))
                    {
                        linkBinFolder = Path.Combine(linkFolder, gen.WorkingFolder);
                        dirExclusions.Add(gen.WorkingFolder);
                    }

                    // some games have save files inside their game folder, so we need to access them inside the loop
                    jsData[Folder.InstancedGameFolder.ToString()] = linkFolder;

                    Thread.Sleep(1000);
                    if (i == 0 && (gen.LauncherExe?.Length > 0 && gen.LauncherExe.EndsWith("NucleusDefined")))
                    {
                        string exeName = null;
                        if (gen.LauncherExe.Contains('|'))
                        {
                            exeName = gen.LauncherExe.Split('|')[0];
                            Log($"User needs to select launcher exe ({exeName})");
                            Forms.Prompt prompt = new Forms.Prompt($"Press OK after selecting the game launcher executable ({exeName}), and you're ready to continue.", true, exeName);
                            prompt.ShowDialog();
                        }
                        else
                        {
                            Log($"User needs to select launcher exe");
                            Forms.Prompt prompt = new Forms.Prompt("Press OK after selecting the game launcher executable, and you're ready to continue.", true, exeName);
                            prompt.ShowDialog();
                        }

                        Thread.Sleep(1000);
                        gen.LauncherExe = ofdPath;

                        if (ofdPath != null && !string.IsNullOrEmpty(ofdPath))
                        {
                            Log("User selected " + ofdPath + " as the launcher exe, updating handler file");
                            string text = File.ReadAllText(Path.Combine(GameManager.Instance.GetJsScriptsPath(), gen.JsFileName));
                            //text = text.Replace("Game.LauncherExe = \"NucleusDefined\"", "Game.LauncherExe = \"" + ofdPath + "\"");
                            text = Regex.Replace(text, @"Game.LauncherExe = (.*)", $"Game.LauncherExe = \"{ofdPath}\";");
                            File.WriteAllText(Path.Combine(GameManager.Instance.GetJsScriptsPath(), gen.JsFileName), text);

                            Thread.Sleep(3000);
                        }
                        else
                        {
                            gen.LauncherExe = null;
                            Log("User did not select a file to be the launcher");
                        }
                    }

                    if ((i == 0 && (gen.SymlinkGame || gen.HardlinkGame)) || gen.HardcopyGame)
                    {
                        if (gen.Hook.CustomDllEnabled)
                        {
                            fileExclusions.Add("xinput1_3.dll");
                            fileExclusions.Add("ncoop.ini");
                        }

                        if (!gen.SymlinkExe)
                        {
                            Log($"Game executable ({gen.ExecutableName}) will be copied and not symlinked");
                            fileCopies.Add(gen.ExecutableName.ToLower());
                            if (gen.LauncherExe?.Length > 0 && !gen.LauncherExe.Contains(':'))
                            {
                                Log($"Launcher executable ({gen.LauncherExe}) will be copied and not symlinked");
                                fileCopies.Add(gen.LauncherExe.ToLower());
                            }
                        }

                        // additional ignored files by the generic info
                        if (gen.FileSymlinkExclusions != null)
                        {
                            Log($"{gen.FileSymlinkExclusions.Length} Files in Game.FileSymlinkExclusions will not be symlinked");
                            string[] symlinkExclusions = gen.FileSymlinkExclusions;
                            for (int k = 0; k < symlinkExclusions.Length; k++)
                            {
                                string s = symlinkExclusions[k];
                                // make sure it's lower case
                                fileExclusions.Add(s.ToLower());
                            }
                        }

                        if (gen.FileSymlinkCopyInstead != null)
                        {
                            Log($"{gen.FileSymlinkCopyInstead.Length} Files in Game.FileSymlinkCopyInstead will be copied instead of symlinked");
                            string[] fileSymlinkCopyInstead = gen.FileSymlinkCopyInstead;
                            for (int k = 0; k < fileSymlinkCopyInstead.Length; k++)
                            {
                                string s = fileSymlinkCopyInstead[k];
                                // make sure it's lower case
                                fileCopies.Add(s.ToLower());
                            }
                        }

                        if (gen.DirSymlinkExclusions != null)
                        {
                            Log($"{gen.DirSymlinkExclusions.Length} Directories in Game.DirSymlinkExclusions will be ignored");
                            string[] symlinkExclusions = gen.DirSymlinkExclusions;
                            for (int k = 0; k < symlinkExclusions.Length; k++)
                            {
                                string s = symlinkExclusions[k];
                                // make sure it's lower case
                                dirExclusions.Add(s.ToLower());
                            }
                        }

                        if (gen.DirExclusions != null)
                        {
                            Log($"{gen.DirExclusions.Length} Directories (and their contents) in Game.DirExclusions will be skipped");
                            string[] skipExclusions = gen.DirExclusions;
                            for (int k = 0; k < skipExclusions.Length; k++)
                            {
                                string s = skipExclusions[k];
                                // make sure it's lower case
                                dirExclusions.Add($"direxskip{s.ToLower()}");
                            }
                        }

                        if (gen.DirSymlinkCopyInstead != null)
                        {
                            Log($"{gen.DirSymlinkCopyInstead.Length} Directories and all its contents in Game.DirSymlinkCopyInstead will be copied instead of symlinked");
                            string[] dirSymlinkCopyInstead = gen.DirSymlinkCopyInstead;

                            SearchOption toSearch = SearchOption.TopDirectoryOnly;
                            if (gen.DirSymlinkCopyInsteadIncludeSubFolders)
                            {
                                toSearch = SearchOption.AllDirectories;
                            }

                            for (int k = 0; k < dirSymlinkCopyInstead.Length; k++)
                            {
                                if (Directory.Exists(Path.Combine(origRootFolder, dirSymlinkCopyInstead[k])))
                                {
                                    dirExclusions.Add(dirSymlinkCopyInstead[k].ToLower());

                                    if (gen.DirSymlinkCopyInsteadIncludeSubFolders)
                                    {
                                        foreach (string dir in Directory.GetDirectories(Path.Combine(rootFolder, dirSymlinkCopyInstead[k]), "*", SearchOption.AllDirectories))
                                        {
                                            int extraChar = 1;
                                            if (rootFolder.EndsWith("\\"))
                                            {
                                                extraChar = 0;
                                            }

                                            dirExclusions.Add(dir.Substring(rootFolder.Length + extraChar).ToLower());
                                        }
                                    }

                                    string[] files = Directory.GetFiles(Path.Combine(rootFolder, dirSymlinkCopyInstead[k]), "*", toSearch);

                                    foreach (string s in files)
                                    {
                                        fileCopies.Add(Path.GetFileName(s).ToLower());
                                    }
                                }
                                else
                                {
                                    Log($"The subfolder {dirSymlinkCopyInstead[k]} does not exist in the original game folder. Skipping.");
                                }
                            }
                        }

                        for (int p = 0; p < players.Count; p++)
                        {
                            string path = Path.Combine(tempDir, "Instance" + p);
                            if (!Directory.Exists(path) || Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any<string>())
                            {
                                symlinkNeeded = true;
                            }
                        }
                    }

                    string[] fileExclusionsArr = fileExclusions.ToArray();
                    string[] fileCopiesArr = fileCopies.ToArray();

                    bool skipped = false;
                    bool keepSymlink = gen.KeepSymLinkOnExit || userGame.Game.MetaInfo.KeepSymLink;

                    if (gen.ForceSymlink || !keepSymlink || symlinkNeeded)
                    {
                        if (gen.ForceSymlink)
                        {
                            Log("Forcing symlink/copy");
                        }

                        if (gen.HardcopyGame)
                        {
                            Log(string.Format("Copying game folder {0} to {1} ", rootFolder, linkFolder));
                            // copy the directory
                            FileUtil.CopyDirectory(rootFolder, new DirectoryInfo(rootFolder), linkFolder, out int exitCode, dirExclusions.ToArray(), fileExclusionsArr, true);

                            while (exitCode != 1)
                            {
                                if (processingExit)
                                {
                                    return string.Empty;
                                }

                                Thread.Sleep(25);
                            }
                        }
                        else if (gen.HardlinkGame)
                        {
                            if (i == 0)
                            {
                                Log(string.Format("Hardlinking game folder and files at {0} to {1}, for each instance", rootFolder, tempDir));
                                int exitCode;
                                while (!StartGameUtil.SymlinkGame(rootFolder, linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, fileCopiesArr, true, gen.SymlinkFolders, players.Count))
                                {
                                    if (processingExit)
                                    {
                                        return string.Empty;
                                    }

                                    Thread.Sleep(25);
                                }
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                Log(string.Format("Symlinking game folder and files at {0} to {1}, for each instance", rootFolder, tempDir));
                                int exitCode;
                                while (!StartGameUtil.SymlinkGame(rootFolder, linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, fileCopiesArr, false, gen.SymlinkFolders, players.Count))
                                {
                                    if (processingExit)
                                    {
                                        return string.Empty;
                                    }

                                    Thread.Sleep(25);
                                }
                            }
                        }
                    }
                    else
                    {
                        skipped = true;
                        Log("Skipping linking or copying files as it is not needed");
                    }

                    if (gen.LauncherExe?.Length > 0)
                    {
                        if (gen.LauncherExe.Contains(':') && gen.LauncherExe.Contains('\\'))
                        {
                            exePath = gen.LauncherExe;
                        }
                        else
                        {
                            if (!gen.LauncherExeIgnoreFileCheck)
                            {
                                string[] launcherFiles = Directory.GetFiles(linkFolder, gen.LauncherExe, SearchOption.AllDirectories);

                                if (launcherFiles.Length < 1)
                                {
                                    Log($"ERROR - Could not find {gen.LauncherExe} in instance folder, Game executable will be used instead; {exePath}");
                                }
                                else if (launcherFiles.Length == 1)
                                {
                                    exePath = launcherFiles[0];
                                    Log($"Found launcher exe at {exePath}. This will be used to launch the game");
                                }
                                else
                                {
                                    exePath = launcherFiles[0];
                                    Log($"Multiple {gen.LauncherExe}'s found in instance folder. Using {exePath} to launch the game");
                                }
                            }
                            else
                            {
                                Log($"Ignoring validation check of launcher exe. Will use filepath: {Path.Combine(linkFolder, gen.LauncherExe)}");
                                exePath = Path.Combine(linkFolder, gen.LauncherExe);
                            }
                        }
                    }

                    if (gen.CopyFoldersTo?.Length > 0)
                    {
                        foreach (string sourceFolder in gen.CopyFoldersTo)
                        {
                            string[] splitStr = sourceFolder.Split('|');
                            string sourcePath = Path.Combine(rootFolder, splitStr[0]);
                            string destinationPath = Path.Combine(linkFolder, splitStr[1]);
                            Log($"Copying folder {sourcePath} and all its contents to {destinationPath}");

                            Directory.CreateDirectory(destinationPath);

                            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                            }

                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
                            }
                        }
                    }

                    if (gen.SymlinkFoldersTo?.Length > 0)
                    {
                        foreach (string sourceFolder in gen.SymlinkFoldersTo)
                        {
                            string[] splitStr = sourceFolder.Split('|');

                            string sourcePath = Path.Combine(rootFolder, splitStr[0]);
                            string destinationPath = Path.Combine(linkFolder, splitStr[1]);
                            Log($"Symlinking folder {sourcePath} to {destinationPath}");

                            if (gen.SymlinkFolders)
                            {
                                Platform.Windows.Interop.Kernel32Interop.CreateSymbolicLink(destinationPath, sourcePath, Platform.Windows.Interop.SymbolicLink.Directory);
                            }
                            else
                            {
                                Directory.CreateDirectory(destinationPath);
                            }

                            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                            {
                                if (gen.SymlinkFolders)
                                {
                                    Platform.Windows.Interop.Kernel32Interop.CreateSymbolicLink(dirPath.Replace(sourcePath, destinationPath), dirPath, Platform.Windows.Interop.SymbolicLink.Directory);
                                }
                                else
                                {
                                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                                }
                            }

                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                                Platform.Windows.Interop.Kernel32Interop.CreateSymbolicLink(newPath.Replace(sourcePath, destinationPath), newPath, Platform.Windows.Interop.SymbolicLink.File);
                        }
                    }

                    if (gen.HardlinkFoldersTo?.Length > 0)
                    {
                        foreach (string sourceFolder in gen.HardlinkFoldersTo)
                        {
                            string[] splitStr = sourceFolder.Split('|');

                            string sourcePath = Path.Combine(rootFolder, splitStr[0]);
                            string destinationPath = Path.Combine(linkFolder, splitStr[1]);
                            Log($"Creating folders in {sourcePath} and hardlinking their contents to {destinationPath}");

                            Directory.CreateDirectory(destinationPath);

                            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                            }

                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                            {
                                Platform.Windows.Interop.Kernel32Interop.CreateHardLink(newPath.Replace(sourcePath, destinationPath), newPath, IntPtr.Zero);
                            }
                        }
                    }

                    if (!skipped)
                    {
                        Log("File operations complete");
                    }
                }
                else
                {
                    exePath = userGame.ExePath;

                    SetExeDpiAwareness(exePath);

                    origExePath = userGame.ExePath;

                    linkBinFolder = workingFolder;
                    linkFolder = rootFolder;

                    instanceExeFolder = linkBinFolder;
                    origRootFolder = rootFolder;

                    if (!gen.ForceLauncherExeIgnoreFileCheck)
                    {
                        if (gen.LauncherExe?.Length > 0)
                        {
                            if (gen.LauncherExe.Contains(':') && gen.LauncherExe.Contains('\\'))
                            {
                                exePath = gen.LauncherExe;
                                origExePath = gen.LauncherExe;
                            }
                            else
                            {
                                string[] launcherFiles = Directory.GetFiles(linkFolder, gen.LauncherExe, SearchOption.AllDirectories);
                                if (launcherFiles.Length < 1)
                                {
                                    Log($"ERROR - Could not find {gen.LauncherExe} in instance folder, Game executable will be used instead; {exePath}");
                                }
                                else if (launcherFiles.Length == 1)
                                {
                                    exePath = launcherFiles[0];
                                    origExePath = launcherFiles[0];
                                    Log($"Found launcher exe at {exePath}. This will be used to launch the game");
                                }
                                else
                                {
                                    exePath = launcherFiles[0];
                                    origExePath = launcherFiles[0];
                                    Log($"Multiple {gen.LauncherExe}'s found in instance folder. Using {exePath} to launch the game");
                                }
                            }
                        }
                    }
                }

                if (i == 0)
                {
                    BackupDatas.StartBackupsRestoration();
                }

                if (processingExit)
                {
                    return string.Empty;
                }

                if (gen.ChangeIPPerInstanceAlt)
                {
                    Network.ChangeIPPerInstanceAltCreateAdapter(player);
                }

                if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
                {
                    WindowsUsersUtil.CreateWindowsUser(player, i);
                }

                bool userConfigPathConverted = false;
                if (gen.UserProfileConfigPath?.Length > 0 && gen.UserProfileConfigPath.ToLower().StartsWith(@"documents\"))
                {
                    gen.DocumentsConfigPath = gen.UserProfileConfigPath.Substring(10);
                    gen.DocumentsConfigPathNoCopy = gen.UserProfileConfigPathNoCopy;
                    gen.ForceDocumentsConfigCopy = gen.ForceUserProfileConfigCopy;
                    userConfigPathConverted = true;
                }

                bool userSavePathConverted = false;
                if (gen.UserProfileSavePath?.Length > 0 && gen.UserProfileSavePath.ToLower().StartsWith(@"documents\"))
                {
                    gen.DocumentsSavePath = gen.UserProfileSavePath.Substring(10);
                    gen.DocumentsSavePathNoCopy = gen.UserProfileSavePathNoCopy;
                    gen.ForceDocumentsSaveCopy = gen.ForceUserProfileSaveCopy;
                    userSavePathConverted = true;
                }

                if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                {
                    useDocs = true;
                }

                if (!userConfigPathConverted && !gen.UserProfileConfigPathNoCopy && (gen.UserProfileConfigPath?.Length > 0 || gen.ForceUserProfileConfigCopy) && gen.UseNucleusEnvironment)
                {
                    NucleusUsers.UserProfileConfigCopy(player);
                }

                if (!userSavePathConverted && !gen.UserProfileSavePathNoCopy && (gen.UserProfileSavePath?.Length > 0 || gen.ForceUserProfileSaveCopy) && gen.UseNucleusEnvironment)
                {
                    NucleusUsers.UserProfileSaveCopy(player);
                }

                if (!gen.DocumentsConfigPathNoCopy && (gen.DocumentsConfigPath?.Length > 0 || gen.ForceDocumentsConfigCopy) && gen.UseNucleusEnvironment)
                {
                    NucleusUsers.DocumentsConfigCopy(player);
                }

                if (!gen.DocumentsSavePathNoCopy && (gen.DocumentsSavePath?.Length > 0 || gen.ForceDocumentsSaveCopy) && gen.UseNucleusEnvironment)
                {
                    NucleusUsers.DocumentsSaveCopy(player);
                }

                if (gen.DeleteFilesInConfigPath?.Length > 0)
                {
                    NucleusUsers.DeleteFilesInConfigPath(player);
                }

                if (gen.DeleteFilesInSavePath?.Length > 0)
                {
                    NucleusUsers.DeleteFilesInSavePath(player);
                }

                if (gen.ChangeExe)
                {
                    ExecutableUtil.ChangeExeName(userGame, instanceExeFolder, i);
                }

                if (gen.RenameAndOrMoveFiles?.Length > 0)
                {
                    FileUtil.RenameOrMoveFiles(linkFolder, i);
                }

                if (gen.DeleteFiles?.Length > 0)
                {
                    FileUtil.DeleteFiles(linkFolder, i);
                }

                context = gen.CreateContext(profile, player, hasKeyboardPlayer);
                context.PlayerID = player.PlayerID;
                context.IsFullscreen = isFullscreen;
                context.ExePath = exePath;
                context.RootInstallFolder = exeFolder;
                context.RootFolder = linkFolder;
                context.OrigRootFolder = rootFolder;
                context.UserProfileConfigPath = gen.UserProfileConfigPath;
                context.UserProfileSavePath = gen.UserProfileSavePath;

                if (gen.SymlinkFiles != null)
                {
                    Log($"Symlinking {gen.SymlinkFiles.Length} files in Game.SymlinkFiles");
                    context.SymlinkFiles = gen.SymlinkFiles;
                }

                if (gen.ForceLauncherExeIgnoreFileCheck)
                {
                    Log($"Force ignoring validation check of launcher exe. Will use filepath: {Path.GetDirectoryName(userGame.ExePath).ToLower()}");
                    linkFolder = Path.GetDirectoryName(userGame.ExePath).ToLower();
                    exePath = Path.Combine(Path.GetDirectoryName(userGame.ExePath).ToLower(), gen.LauncherExe);

                    if (gen.LauncherExe?.Length > 0)
                    {
                        if (gen.LauncherExe.Contains(':') && gen.LauncherExe.Contains('\\'))
                        {
                            exePath = gen.LauncherExe;
                            origExePath = gen.LauncherExe;
                        }
                        else
                        {
                            string[] launcherFiles = Directory.GetFiles(linkFolder, gen.LauncherExe, SearchOption.AllDirectories);
                            if (launcherFiles.Length < 1)
                            {
                                Log($"ERROR - Could not find {gen.LauncherExe} in instance folder, Game executable will be used instead; {exePath}");
                            }
                            else if (launcherFiles.Length == 1)
                            {
                                exePath = launcherFiles[0];
                                origExePath = launcherFiles[0];
                                Log($"Found launcher exe at {exePath}. This will be used to launch the game");
                            }
                            else
                            {
                                exePath = launcherFiles[0];
                                origExePath = launcherFiles[0];
                                Log($"Multiple {gen.LauncherExe}'s found in instance folder. Using {exePath} to launch the game");
                            }
                        }
                    }

                    gen.LauncherExe = exePath;
                }

                if (gen.LauncherExe?.Length > 0)
                {
                    context.LauncherFolder = Path.GetDirectoryName(gen.LauncherExe);
                }

                if (userConfigPathConverted || userSavePathConverted)
                {
                    context.UserProfileConvertedToDocuments = true;

                    if (userConfigPathConverted)
                    {
                        context.UserProfileConfigPath = gen.DocumentsConfigPath;
                    }

                    if (userSavePathConverted)
                    {
                        context.UserProfileSavePath = gen.DocumentsSavePath;
                    }
                }
                else
                {
                    context.UserProfileConvertedToDocuments = false;
                }

                context.DocumentsConfigPath = gen.DocumentsConfigPath;
                context.DocumentsSavePath = gen.DocumentsSavePath;

                if (i == 0)
                {
                    for (int plyri = 0; plyri < players.Count; plyri++)
                    {
                        if (players[plyri].IsKeyboardPlayer)
                        {
                            context.NumKeyboards++;
                        }
                        else
                        {
                            context.NumControllers++;
                        }
                    }
                }

                if (gen.CustomUserGeneralPrompts?.Length > 0)
                {
                    CustomPromptRuntime.CustomUserGeneralPrompts(player);
                }

                if (gen.CustomUserPlayerPrompts?.Length > 0)
                {
                    CustomPromptRuntime.CustomUserPlayerPrompts(player);
                }

                if (gen.CustomUserInstancePrompts?.Length > 0)
                {
                    CustomPromptRuntime.CustomUserInstancePrompts(player);
                }

                bool setupDll = true;

                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame && i > 0)
                {
                    setupDll = false;
                }

                if (gen.UseDI8CoopLvlUnlocker)
                {
                    DInput8CoopLvlUnlocker.SetupDInput8CoopLvlUnlocker(player, i, setupDll);
                }

                //backward compat for existing handlers using UseSteamless(old way of applying steamless)
                if (gen.UseSteamless)
                {
                    gen.SteamlessPatch = new string[] { "false", gen.SteamlessArgs, gen.SteamlessTiming.ToString() };
                }

                if (gen.SteamlessPatch != null)
                {
                    if (bool.Parse(gen.SteamlessPatch[0]))//patch game launcher
                    {
                        Log($"Apply Steamless patch for {gen.LauncherExe} Timing: {gen.SteamlessPatch[2]}ms");
                        SteamFunctions.SteamlessProc(linkBinFolder, gen.LauncherExe, gen.SteamlessPatch[1], int.Parse(gen.SteamlessPatch[2]));
                        Thread.Sleep(int.Parse(gen.SteamlessPatch[2]) + 2000);
                    }
                    else//patch game exe
                    {

                        Log($"Apply Steamless patch for {gen.ExecutableName} Timing: {gen.SteamlessPatch[2]}ms");
                        SteamFunctions.SteamlessProc(linkBinFolder, gen.ExecutableName, gen.SteamlessPatch[1], int.Parse(gen.SteamlessPatch[2]));
                        Thread.Sleep(int.Parse(gen.SteamlessPatch[2]) + 2000);
                    }
                }

                if (processingExit)
                {
                    return string.Empty;
                }

                if (gen.GamePlayBeforeGameSetup && !gen.GamePlayAfterLaunch)
                {
                    gen.PrePlay(player);
                    ProtoInputLauncher.SetProtoControllersIndex(player, gen);
                }

                if (gen.HexEditExeAddress?.Length > 0)
                {
                    HexEdit.HexEditExeAddress(i);
                }

                if (gen.HexEditFileAddress?.Length > 0)
                {
                    HexEdit.HexEditFileAddress(i, linkFolder);
                }

                if (gen.HexEditAllExes?.Length > 0)
                {
                    HexEdit.HexEditAllExes(i);
                }

                if (gen.HexEditExe?.Length > 0)
                {
                    HexEdit.HexEditExe(i);
                }

                if (gen.HexEditAllFiles?.Length > 0)
                {
                    HexEdit.HexEditAllFiles(i, linkFolder);
                }

                if (gen.HexEditFile?.Length > 0)
                {
                    HexEdit.HexEditFile(i, linkFolder);
                }

                if (gen.UseSteamStubDRMPatcher)
                {
                    SteamFunctions.UseSteamStubDRMPatcher(setupDll);
                }

                if (gen.UseEACBypass)
                {
                    EACBypass.UseEACBypass(linkFolder, setupDll);
                }

                if (gen.UseGoldberg)
                {
                    SteamFunctions.UseGoldberg(rootFolder, NucleusFolderPath, linkFolder, i, player, players, setupDll);
                }

                if (gen.UseGoldbergNoOGSteamDlls)
                {
                    SteamFunctions.UseGoldbergNoOGSteamDlls(NucleusFolderPath, linkFolder, i, player, players, setupDll);
                }

                if (gen.UseNemirtingasEpicEmu)
                {
                    context.StartArguments += "";
                    NemirtingasEpicEmu.UseNemirtingasEpicEmu(rootFolder, linkFolder, i, player, setupDll);
                }

                if (gen.UseNemirtingasGalaxyEmu)
                {
                    NemirtingasGalaxyEmu.UseNemirtingasGalaxyEmu(rootFolder, linkFolder, i, player, setupDll);
                }

                if (gen.CreateSteamAppIdByExe)
                {
                    SteamFunctions.CreateSteamAppIdByExe(setupDll);
                }

                if (gen.XInputPlusDll?.Length > 0 && !gen.ProcessChangesAtEnd)
                {
                    XInputPlusDll.SetupXInputPlusDll(player, i, setupDll);
                }

                if (gen.UseDevReorder && !gen.ProcessChangesAtEnd)
                {
                    DevReorder.UseDevReorder(player, i, setupDll);
                }

                if (gen.UseDInputBlocker)
                {
                    DInputBlocker.UseDInputBlocker(setupDll);
                }

                if (gen.UseX360ce && !gen.ProcessChangesAtEnd)
                {
                    X360ce.UseX360ce(i, player, setupDll);
                }

                if (gen.UseDirectX9Wrapper)
                {
                    DirectX9Wrapper.UseDirectX9Wrapper(setupDll);
                }

                if(gen.Hook.SDL2Enabled)
                {
                    SDL2Dll.SetupSDL2Dll(player, i, setupDll);
                }

                if (gen.CopyCustomUtils?.Length > 0)
                {
                    FileUtil.CopyCustomUtils(i, linkFolder, setupDll);
                }

                if (!string.IsNullOrEmpty(gen.FlawlessWidescreen))
                {
                    FlawlessWidescreen.UseFlawlessWidescreen(i);
                }

                if (!gen.GamePlayBeforeGameSetup && !gen.GamePlayAfterLaunch)
                {
                    gen.PrePlay(player);
                    ProtoInputLauncher.SetProtoControllersIndex(player, gen);
                }

                if (gen.AltEpicEmuArgs)
                {
                    Log("Using pre-defined epic emu params");
                    context.StartArguments += " ";
                    context.StartArguments += $" -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=Nucleus -epicenv=Prod -EpicPortal -epiclocale={gen.EpicLang} -epicusername={player.Nickname} -epicuserid={player.Nickname} ";
                }

                if (gen.EpicEmuArgs)
                {
                    context.StartArguments += " ";
                    Log("Using alternative pre-defined epic emu params");

                    if (!context.StartArguments.Contains("-AUTH_LOGIN"))
                    {
                        Log("Epic Emu parameters not found in arguments. Adding the necessary parameters to existing starting arguments");
                        //AUTH_LOGIN = unused - AUTH_PASSWORD = cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd - AUTH_TYPE = exchangecode - epicapp = CrabTest - epicenv = Prod - EpicPortal - epiclocale = en - epicusername <= same username than in the.json > -epicuserid <= same epicid than in the.json                     
                        context.StartArguments += $" -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=Nucleus -epicenv=Prod -EpicPortal -epiclocale={gen.EpicLang} -epicusername={player.Nickname} -epicuserid=0000000000000000000000000player{i + 1} ";
                    }
                }

                if (gen.PauseBetweenContextAndLaunch > 0)
                {
                    Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenContextAndLaunch));
                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenContextAndLaunch));
                }

                string startArgs = context.StartArguments;

                if (!string.IsNullOrEmpty(SteamFunctions.lobbyConnectArg) && i > 0)
                {
                    startArgs = SteamFunctions.lobbyConnectArg + " " + startArgs;
                    Log("Goldberg Lobby Connect: Will join lobby ID " + SteamFunctions.lobbyConnectArg.Substring(15));
                }

                if (context.Hook.CustomDllEnabled && !gen.ProcessChangesAtEnd)
                {
                    XInputPlusDll.CustomDllEnabled(player, playerBounds, i, setupDll);
                }

                if (gen.GoldbergWriteSteamIDAndAccount)
                {
                    SteamFunctions.GoldbergWriteSteamIDAndAccount(linkFolder, i, player);
                }

                if (gen.ChangeIPPerInstance && !gen.ProcessChangesAtEnd)
                {
                    Network.ChangeIPPerInstance(i);
                }

                Process proc = null;

                if (gen.LauncherExe?.Length > 0)
                {
                    //Force no starting arguments as a launcher is being used
                    if (gen.HookInit || gen.RenameNotKillMutex || gen.SetWindowHookStart || gen.BlockRawInput || gen.CreateSingleDeviceFile)
                    {
                        Log("Disabling start up hooks as a launcher is being used");
                        gen.HookInit = false;
                        gen.RenameNotKillMutex = false;
                        gen.SetWindowHookStart = false;
                        gen.BlockRawInput = false;
                        gen.CreateSingleDeviceFile = false;
                    }
                }

                if (processingExit)
                {
                    return string.Empty;
                }
                else
                {
                    Globals.MainOSD.Show(1200, $"Starting {gen.GameName} instance for {player.Nickname} as Player #{player.PlayerID + 1}");
                }

                if (context.NeedsSteamEmulation)
                {
                    SteamFunctions.SmartSteamEmu(player, i, linkFolder, startArgs, setupDll);
                    proc = null;//leave this here just in case for now
                    Thread.Sleep(5000);
                }
                else
                {
                    if (gen.ForceEnvironmentUse && gen.ThirdPartyLaunch)
                    {
                        Log("Force Nucleus environment use");
                        NucleusUsers.CreateUserEnvironment(player);
                    }

                    if (!gen.ThirdPartyLaunch)
                    {
                        if (gen.ExecutableToLaunch?.Length > 0)
                        {
                            Log("Different executable provided to launch");
                            exePath = Path.Combine(linkFolder, gen.ExecutableToLaunch);
                        }

                        if (gen.ProtoInput.InjectStartup)
                        {
                            Log("Starting game with ProtoInput");

                            IntPtr envPtr = IntPtr.Zero;

                            if (gen.UseNucleusEnvironment)
                            {
                                envPtr = NucleusUsers.CreateUserEnvironment(player);
                            }

                            ProtoInputLauncher.InjectStartup(exePath,
                                startArgs, 0, NucleusFolderPath, i + 1, gen, player, out uint pid, envPtr,
                                (player.IsRawMouse ? (int)player.RawMouseDeviceHandle : -1),
                                (player.IsRawKeyboard ? (int)player.RawKeyboardDeviceHandle : -1),
                                (gen.ProtoInput.MultipleProtoControllers ? (player.ProtoController1) : ((player.IsRawMouse || player.IsRawKeyboard) ? 0 : player.GamepadId + 1)),
                                (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController2 : 0),
                                (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController3 : 0),
                                (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController4 : 0)
                                );

                            try
                            {
                                proc = Process.GetProcessById((int)pid);
                            }
                            catch (Exception)
                            {
                                proc = null;
                                Log("Process By ID failed, setting process to null and continuing, will try and catch it later");
                            }

                        }
                        else if ((gen.HookInit || (gen.RenameNotKillMutex && context.KillMutex?.Length > 0) || gen.SetWindowHookStart || gen.BlockRawInput || gen.CreateSingleDeviceFile) && !gen.CMDLaunch && !gen.UseForceBindIP && !gen.LaunchAsDifferentUsers && !gen.LaunchAsDifferentUsersAlt) /*|| (gen.CMDLaunch && i==0))*/
                        {
                            string mu = "";
                            if (gen.RenameNotKillMutex && context.KillMutex?.Length > 0)
                            {
                                for (int m = 0; m < gen.KillMutex.Length; m++)
                                {
                                    mu += gen.KillMutex[m];

                                    if (m != gen.KillMutex.Length - 1)
                                    {
                                        mu += "|==|";
                                    }
                                }
                            }

                            bool startupHooksEnabled = true;
                            if (gen.StartHookInstances?.Length > 0)
                            {
                                string[] instancesToHook = gen.StartHookInstances.Split(',');
                                if (!instancesToHook.ToList().Contains((i + 1).ToString()))
                                {
                                    startupHooksEnabled = false;
                                }
                            }

                            Log(string.Format("Launching game located at {0} through StartGameUtil", exePath));

                            uint sguOutPID = StartGameUtil.StartGame(exePath, startArgs,
                                gen.HookInit, gen.HookInitDelay, gen.RenameNotKillMutex, mu, gen.SetWindowHookStart, isDebug, NucleusFolderPath, gen.BlockRawInput, gen.UseNucleusEnvironment, player.Nickname, startupHooksEnabled, gen.CreateSingleDeviceFile, player.RawHID, player.MonitorBounds.Width, player.MonitorBounds.Height, player.MonitorBounds.X
                                , player.MonitorBounds.Y, Globals.UserDocumentsRoot, useDocs);

                            try
                            {
                                proc = Process.GetProcessById((int)sguOutPID);
                            }
                            catch (Exception)
                            {
                                proc = null;
                                Log("Process By ID failed, setting process to null and continuing, will try and catch it later");
                            }

                        }
                        else
                        {
                            if (gen.LaunchAsDifferentUsersAlt)
                            {
                                //create users OR reset their password if they exists.
                                Thread.Sleep(1000);

                                Process cmd = new Process();
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.StartInfo.Verb = "runas";
                                string cmdLine;
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                                cmdLine = $"elevate /C runas /savecred /env /user:nucleusplayer{i + 1}" + " \"" + exePath + " " + startArgs + "\"";
                                cmd.StartInfo.Arguments = cmdLine;

                                string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];

                                if (gen.UseNucleusEnvironment)
                                {
                                    cmd.StartInfo.EnvironmentVariables["APPDATA"] = Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                    cmd.StartInfo.EnvironmentVariables["LOCALAPPDATA"] = Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Local";
                                    cmd.StartInfo.EnvironmentVariables["USERPROFILE"] = Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}";
                                    cmd.StartInfo.EnvironmentVariables["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath") + $@"\NucleusCoop\{player.Nickname}";

                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot + $@"\NucleusCoop");
                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}");
                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\Documents");
                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Local");

                                    Directory.CreateDirectory(Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                    if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                    {
                                        if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg")))
                                        {
                                            RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                                        }

                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);

                                        if (key.GetValue("Personal").ToString() != "%USERPROFILE%\\Documents")
                                        {
                                            key.SetValue("Personal", Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }
                                    }

                                }
                                else if (gen.UseCurrentUserEnvironment)
                                {
                                    cmd.StartInfo.EnvironmentVariables["APPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                    cmd.StartInfo.EnvironmentVariables["LOCALAPPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                                    cmd.StartInfo.EnvironmentVariables["USERPROFILE"] = Globals.UserEnvironmentRoot;
                                    cmd.StartInfo.EnvironmentVariables["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath");

                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot);
                                    Directory.CreateDirectory(Globals.UserDocumentsRoot);
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                                }

                                Log(string.Format("Launching game as user: nucleusplayer{0}, using command: {1}", (i + 1), cmdLine));

                                cmd.Start();
                                cmd.WaitForExit();

                                proc = null;
                            }
                            else if (gen.LaunchAsDifferentUsers)
                            {
                                IntPtr envPtr = IntPtr.Zero;
                                string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                var sb = new StringBuilder();
                                IDictionary envVars = Environment.GetEnvironmentVariables();

                                if (gen.UseNucleusEnvironment)
                                {
                                    envPtr = NucleusUsers.CreateUserEnvironment(player);
                                }
                                else if (gen.UseCurrentUserEnvironment)
                                {
                                    Log("Setting environment to current user");

                                    envVars["APPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //$@"C:\Users\{username}\AppData\Roaming";
                                    envVars["LOCALAPPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //$@"C:\Users\{username}\AppData\Local";
                                    envVars["USERPROFILE"] = Globals.UserEnvironmentRoot; //$@"C:\Users\{username}\";
                                    envVars["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath"); //$@"\Users\{username}\";

                                    Directory.CreateDirectory(Globals.UserEnvironmentRoot); //$@"C:\Users\{username}");
                                    Directory.CreateDirectory(Globals.UserDocumentsRoot);//$@"C:\Users\{username}\Documents");
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));//$@"C:\Users\{username}\AppData\Roaming");
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));//$@"C:\Users\{username}\AppData\Local");

                                    foreach (object envVarKey in envVars.Keys)
                                    {
                                        if (envVarKey != null)
                                        {
                                            string key = envVarKey.ToString();
                                            string value = envVars[envVarKey].ToString();

                                            sb.Append(key);
                                            sb.Append("=");
                                            sb.Append(value);
                                            sb.Append("\0");
                                        }
                                    }

                                    sb.Append("\0");

                                    byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());
                                    envPtr = Marshal.AllocHGlobal(envBytes.Length);
                                    Marshal.Copy(envBytes, 0, envPtr, envBytes.Length);
                                }

                                ProcessUtil.STARTUPINFO startup = new ProcessUtil.STARTUPINFO();
                                startup.cb = Marshal.SizeOf(startup);

                                bool success = ProcessUtil.CreateProcessWithLogonW($"nucleusplayer{i + 1}", Environment.UserDomainName, nucleusUserAccountsPassword, ProcessUtil.LogonFlags.LOGON_WITH_PROFILE, null, exePath + " " + startArgs, ProcessUtil.ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, (uint)envPtr, Path.GetDirectoryName(exePath), ref startup, out ProcessUtil.PROCESS_INFORMATION processInformation);
                                Log(string.Format("Launching game directly at {0} with args {1} as user: nucleusplayer{2}", exePath, startArgs, (i + 1)));

                                if (!success)
                                {
                                    int error = Marshal.GetLastWin32Error();
                                    Log(string.Format("ERROR {0} - CreateProcessWithLogonW failed", error));
                                }

                                try
                                {
                                    proc = Process.GetProcessById(processInformation.dwProcessId);
                                }
                                catch
                                {
                                    proc = null;
                                }

                            }
                            else if (gen.CMDLaunch || (gen.UseForceBindIP && i > 0))
                            {
                                string scriptPath = Path.GetTempFileName().Replace(".tmp", ".cmd");
                                StringBuilder scriptContent = new StringBuilder();
                                string test = Globals.UserEnvironmentRoot.Replace("koumi", "Mickàél");

                                string[] cmdOps = gen.CMDOptions;

                                if (gen.CMDLaunch)
                                {
                                    if (gen.UseNucleusEnvironment)
                                    {
                                        Log("Setting up Nucleus environment");

                                        string username = Environment.UserName;
                                        try
                                        {
                                            username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                        }
                                        catch (Exception)
                                        {
                                            Log("ERROR - getting current user's username, defaulting to using environment's username");
                                        }

                                        scriptContent.AppendLine("@echo off");
                                        scriptContent.AppendLine("chcp 65001 >nul"); // UTF-8

                                        scriptContent.AppendLine($"set APPDATA={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}\\AppData\\Roaming");
                                        scriptContent.AppendLine($"set LOCALAPPDATA={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}\\AppData\\Local");
                                        scriptContent.AppendLine($"set USERPROFILE={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}");
                                        scriptContent.AppendLine($"set HOMEPATH=\\Users\\{username}\\NucleusCoop\\{player.Nickname}");

                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\Documents");
                                        Directory.CreateDirectory(Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                        if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                        {
                                            string backupPath = Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg");
                                            if (!File.Exists(backupPath))
                                            {
                                                RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", backupPath);
                                            }

                                            RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                            dkey.SetValue("Personal", Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0 || gen.CMDBatchAfter?.Length > 0)
                                    {
                                        scriptContent.AppendLine($"set NUCLEUS_EXE={Path.GetFileName(exePath)}");
                                        scriptContent.AppendLine($"set NUCLEUS_INST_EXE_FOLDER={Path.GetDirectoryName(exePath)}");
                                        scriptContent.AppendLine($"set NUCLEUS_INST_FOLDR={linkFolder}");
                                        scriptContent.AppendLine($"set NUCLEUS_FOLDER={NucleusFolderPath}");
                                        scriptContent.AppendLine($"set NUCLEUS_ORIG_EXE_FOLDER={exeFolder}");
                                        scriptContent.AppendLine($"set NUCLEUS_ORIG_FOLDER={exeFolder.Substring(0, (exeFolder.Length - gen.BinariesFolder.Length))}");
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0)
                                    {
                                        foreach (var line in gen.CMDBatchBefore)
                                        {
                                            if (!line.Contains("|"))
                                            {
                                                Log("Running command line: " + line);
                                                scriptContent.AppendLine(line);
                                            }
                                            else
                                            {
                                                var clineSplit = line.Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    scriptContent.AppendLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    string cmdLine = $"\"{exePath}\" {startArgs}";
                                    if (!gen.CMDStartArgsInside)
                                        cmdLine = $"\"{exePath} {startArgs}\"";

                                    if (cmdOps?.Length > 0 && i < cmdOps.Length)
                                    {
                                        cmdLine = $"{cmdOps[i]} \"{exePath}\" {startArgs}";
                                        if (!gen.CMDStartArgsInside)
                                            cmdLine = $"{cmdOps[i]} \"{exePath} {startArgs}\"";
                                    }

                                    if (gen.PauseCMDBatchBefore > 0)
                                    {
                                        Log($"Pausing for {gen.PauseCMDBatchBefore} seconds");                                      
                                        scriptContent.AppendLine($" timeout / t {gen.PauseCMDBatchBefore} / nobreak > nul");
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchBefore));
                                    }

                                    Log("Launching game via command prompt with the following line: " + cmdLine);
                                    scriptContent.AppendLine(cmdLine);

                                    if (gen.CMDBatchAfter?.Length > 0)
                                    {
                                        foreach (var line in gen.CMDBatchAfter)
                                        {
                                            if (!line.Contains("|"))
                                            {
                                                Log("Running command line: " + line);
                                                scriptContent.AppendLine(line);
                                            }
                                            else
                                            {
                                                var clineSplit = line.Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    scriptContent.AppendLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    if (gen.PauseCMDBatchAfter > 0)
                                    {
                                        Log($"Pausing for {gen.PauseCMDBatchAfter} seconds");
                                        scriptContent.AppendLine($" timeout / t {gen.PauseCMDBatchAfter} / nobreak > nul");
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchAfter));
                                    }
                                }
                                else
                                {
                                    string forceBindexe = gameIs64 ? "ForceBindIP64.exe" : "ForceBindIP.exe";

                                    if (gen.UseNucleusEnvironment)
                                    {
                                        string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];

                                        scriptContent.AppendLine("@echo off");
                                        scriptContent.AppendLine("chcp 65001 >nul");

                                        scriptContent.AppendLine($"set APPDATA={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}\\AppData\\Roaming");
                                        scriptContent.AppendLine($"set LOCALAPPDATA={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}\\AppData\\Local");
                                        scriptContent.AppendLine($"set USERPROFILE={Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}");
                                        scriptContent.AppendLine($"set HOMEPATH=\\Users\\{Globals.UserEnvironmentRoot}\\NucleusCoop\\{player.Nickname}");

                                        //Some games will crash if the directories don't exist
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\Documents");
                                        Directory.CreateDirectory(Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                        if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                        {
                                            string backupPath = Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg");
                                            if (!File.Exists(backupPath))
                                            {
                                                RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", backupPath);
                                            }

                                            RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                            dkey.SetValue("Personal", Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }

                                        if (gen.CMDBatchBefore?.Length > 0 || gen.CMDBatchAfter?.Length > 0)
                                        {
                                            scriptContent.AppendLine($@"set NUCLEUS_EXE=" + Path.GetFileName(exePath));
                                            scriptContent.AppendLine($@"set NUCLEUS_INST_EXE_FOLDER=" + Path.GetDirectoryName(exePath));
                                            scriptContent.AppendLine($@"set NUCLEUS_INST_FOLDER=" + linkFolder);
                                            scriptContent.AppendLine($@"set NUCLEUS_FOLDER=" + NucleusFolderPath);
                                            scriptContent.AppendLine($@"set NUCLEUS_ORIG_EXE_FOLDER=" + exeFolder);
                                            scriptContent.AppendLine($@"set NUCLEUS_ORIG_FOLDER=" + exeFolder.Substring(0, (exeFolder.Length - gen.BinariesFolder.Length)));
                                        }
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0)
                                    {
                                        foreach (var line in gen.CMDBatchBefore)
                                        {
                                            if (!line.Contains("|"))
                                            {
                                                Log("Running command line: " + line);
                                                scriptContent.AppendLine(line);
                                            }
                                            else
                                            {
                                                var clineSplit = line.Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    scriptContent.AppendLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    if (gen.PauseCMDBatchBefore > 0)
                                    {
                                        Log($"Pausing for {gen.PauseCMDBatchBefore} seconds");
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchBefore));
                                    }

                                    string iParam = gen.ForceBindIPDelay ? "-i " : "";
                                    string dummy = gen.ForceBindIPNoDummy ? "" : " dummy";

                                    string forceBindPath = Path.Combine(GameManager.Instance.GetUtilsPath(), "ForceBindIP", forceBindexe);
                                    string ip = "127.0.0." + (i + 2);

                                    string cmdLine = $"\"{forceBindPath}\" {iParam}{ip} \"{exePath}\"{dummy} {startArgs}";

                                    Log("Launching game using ForceBindIP command line argument: " + cmdLine);
                                    scriptContent.AppendLine(cmdLine);

                                    if (gen.CMDBatchAfter?.Length > 0)
                                    {
                                        foreach (var line in gen.CMDBatchAfter)
                                        {
                                            if (!line.Contains("|"))
                                            {
                                                Log("Running command line: " + line);
                                                scriptContent.AppendLine(line);
                                            }
                                            else
                                            {
                                                var clineSplit = line.Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    scriptContent.AppendLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }
                                }

                                File.WriteAllText(scriptPath, scriptContent.ToString(), new UTF8Encoding(true));
                                Thread.Sleep(100);

                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.Arguments = $"/c \"{scriptPath}\"";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.RedirectStandardError = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.StartInfo.CreateNoWindow = true;

                                if (gen.UseForceBindIP)
                                {
                                    cmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                                }

                                cmd.Start();
                                cmd.WaitForExit();

                                if (gen.PauseCMDBatchAfter > 0)
                                {
                                    Log($"Pausing for {gen.PauseCMDBatchAfter} seconds");
                                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchAfter));
                                }

                            }
                            else
                            {
                                IntPtr envPtr = IntPtr.Zero;

                                if (gen.UseNucleusEnvironment)
                                {
                                    envPtr = NucleusUsers.CreateUserEnvironment(player);
                                }

                                ProcessUtil.STARTUPINFO startup = new ProcessUtil.STARTUPINFO();
                                startup.cb = Marshal.SizeOf(startup);

                                bool success = ProcessUtil.CreateProcess(null, exePath + " " + startArgs, IntPtr.Zero, IntPtr.Zero, false, (uint)ProcessUtil.ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, envPtr, Path.GetDirectoryName(exePath), ref startup, out ProcessUtil.PROCESS_INFORMATION processInformation);
                                Log(string.Format("Launching game directly at {0} with args {1}", exePath, startArgs));

                                if (!success)
                                {
                                    int error = Marshal.GetLastWin32Error();
                                    Log(string.Format("ERROR {0} - CreateProcess failed - startGamePath: {1}, startArgs: {2}, dirpath: {3}", error, exePath, startArgs, Path.GetDirectoryName(exePath)));
                                }

                                try
                                {
                                    proc = Process.GetProcessById(processInformation.dwProcessId);
                                }
                                catch
                                {
                                    proc = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        Log("Skipping launching of game via Nucleus for third party launch");

                        if (!gen.IgnoreThirdPartyPrompt)
                        {
                            Forms.Prompt prompt = new Forms.Prompt("Press OK when game has launched.");
                            prompt.ShowDialog();
                        }
                    }

                    if (gen.LauncherExe?.Length > 0)
                    {
                        //Force process to be null as we don't want launcher process
                        Log("Dropping process as it is the launcher");
                        proc = null;
                    }

                    if (proc != null && proc.ProcessName.ToLower() == "steamclient_loader")
                    {
                        Log("Dropping process as it is the Goldberg Steamclient Loader");
                        proc = null;
                    }

                    if (proc != null && !Process.GetProcesses().Any(x => x.Id == proc.Id))
                    {
                        Log("Process " + proc.Id + " is no longer running. Will search for process");
                        proc = null;
                    }
                }

                if (gen.GamePlayAfterLaunch && !gen.GamePlayBeforeGameSetup)
                {
                    gen.PrePlay(player);
                    ProtoInputLauncher.SetProtoControllersIndex(player, gen);
                }

                if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
                {
                    if (launchProc != null)
                    {
                        launchProc.Kill();
                        launchProc = null;
                    }
                    else
                    {
                        Log("Unable to find intermediary proc to kill");
                    }
                }

                if (GameProfile.PauseBetweenInstanceLaunch > 0)
                {
                    gen.PauseBetweenStarts = GameProfile.PauseBetweenInstanceLaunch;
                    Log("Set Pause Between Instances Startup to " + gen.PauseBetweenStarts + " s");
                }

                if (gen.ProcessChangesAtEnd)
                {
                    if (i == (players.Count - 1))
                    {
                        if (gen.PauseBetweenStarts > 0)
                        {
                            Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                            Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                        }

                        if (gen.PromptProcessChangesAtEnd)
                        {
                            Log("Prompted user before processing end changes");
                            Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to make changes to game processes.");
                            prompt.ShowDialog();
                        }

                        ProcessChangeAtEnd();

                        return string.Empty;
                    }
                    else
                    {
                        if (gen.PromptAfterFirstInstance && i == 0)
                        {
                            Log("Prompted user after first instance");
                            Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to launch the rest of the instances.");
                            prompt.ShowDialog();
                        }

                        if (gen.PromptBetweenInstances)
                        {
                            if (gen.PauseBetweenStarts > 0)
                            {
                                Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                                Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                            }
                            Log(string.Format("Prompted user for Instance {0}", (i + 2)));
                            Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to launch instance " + (i + 2) + ".");
                            prompt.ShowDialog();
                        }
                        else
                        {
                            if (gen.PauseBetweenStarts > 0)
                            {
                                if (!gen.PromptAfterFirstInstance || (gen.PromptAfterFirstInstance && i > 0))
                                {
                                    Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                                }
                            }
                        }

                        continue;
                    }
                }

                if (gen.PromptBeforeProcessGrab)
                {
                    Log("Prompted user before searching for game process");

                    Forms.Prompt prompt = new Forms.Prompt("Press OK when ready for Nucleus to search for game process.");
                    prompt.ShowDialog();
                }

                if (gen.PauseBetweenProcessGrab > 0)
                {
                    Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenProcessGrab));
                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenProcessGrab));
                }

                if (processingExit)
                {
                    return string.Empty;
                }

                if (gen.LauncherExe?.Length > 0 && gen.RunLauncherAndExe)
                {
                    Log("Launching exe " + origExePath);
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = origExePath;
                    proc = Process.Start(startInfo);

                    int counter = 0;
                    bool found = false;
                    if (gen.GameName == "Ghost Recon Wildlands" && i > 0)
                    {
                        Log("Launching exe again " + origExePath);
                        startInfo = new ProcessStartInfo();
                        startInfo.FileName = origExePath;
                        proc = Process.Start(startInfo);

                        Log("Waiting to find process by window title");

                        while (!found)
                        {
                            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gen.ExecutableName));
                            foreach (var process in processes)
                            {
                                if ((int)process.NucleusGetMainWindowHandle() > 0 && process.MainWindowTitle == gen.Hook.ForceFocusWindowName && (attachedIds.Count == 0 || (attachedIds.Count > 0 && !attachedIds.Contains(process.Id))))
                                {
                                    Log("Process found, " + process.ProcessName + " pid (" + process.Id + ") after " + counter + " seconds");
                                    proc = process;
                                    attached.Add(process);
                                    attachedIds.Add(process.Id);
                                    player.ProcessID = process.Id;
                                    found = true;
                                    Log(string.Format("Process details; Name: {0}, ID: {1}, MainWindowtitle: {2}, NucleusGetMainWindowHandle(): {3}", process.ProcessName, process.Id, process.MainWindowTitle, process.NucleusGetMainWindowHandle()));
                                    break;
                                }
                            }
                            counter++;
                            Thread.Sleep(1000);

                            if (processingExit)
                            {
                                return string.Empty;
                            }
                        }

                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Log("Waiting to find process by window title");

                        while (!found)
                        {
                            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gen.ExecutableName));
                            foreach (var process in processes)
                            {
                                if ((int)process.NucleusGetMainWindowHandle() > 0 && process.MainWindowTitle == gen.Hook.ForceFocusWindowName && (attachedIds.Count == 0 || (attachedIds.Count > 0 && !attachedIds.Contains(process.Id))))
                                {
                                    Log("Process found, " + process.ProcessName + " pid (" + process.Id + ") after " + counter + " seconds");
                                    proc = process;
                                    attached.Add(process);
                                    attachedIds.Add(process.Id);
                                    player.ProcessID = process.Id;
                                    found = true;
                                    Log(string.Format("Process details; Name: {0}, ID: {1}, MainWindowtitle: {2}, NucleusGetMainWindowHandle(): {3}", process.ProcessName, process.Id, process.MainWindowTitle, process.NucleusGetMainWindowHandle()));
                                    break;
                                }
                            }

                            counter++;
                            Thread.Sleep(1000);

                            if (processingExit)
                            {
                                return string.Empty;
                            }
                        }
                    }

                    Thread.Sleep(10000);
                }

                if ((proc != null && !Process.GetProcesses().Any(x => x.Id == proc.Id)) || gen.ForceProcessSearch || gen.NeedsSteamEmulation || gen.ForceProcessPick || proc == null || gen.CMDLaunch || gen.UseForceBindIP || gen.GameName == "Halo Custom Edition" || (proc != null && !ProcessUtil.IsRunning(proc)) /*|| gen.LauncherExe?.Length > 0*/)
                {
                    if (proc != null && !Process.GetProcesses().Any(x => x.Id == proc.Id))
                    {
                        Log("Process " + proc.Id + " is no longer running. Will search for process");
                    }

                    Log("Searching for game process");

                    if (gen.GameName == "Halo Custom Edition" || gen.GameName == "Ghost Recon Wildlands" /*|| gen.LauncherExe?.Length > 0*/)
                    {
                        //Halo CE and GRW seem to need to wait X additional seconds otherwise crashes...
                        Thread.Sleep(10000);
                    }

                    string ids = "";

                    foreach (int id in attachedIds)
                    {
                        ids += id + " ";
                    }

                    Log("PIDs stored " + ids);

                    if (!gen.ForceProcessPick)
                    {
                        //bool foundUnique = false;
                        for (int times = 0; times < 200; times++)
                        {
                            Thread.Sleep(50);

                            string proceName = Path.GetFileNameWithoutExtension(gen.ExecutableName).ToLower();
                            if (gen.ChangeExe)
                            {
                                proceName = Path.GetFileNameWithoutExtension(userGame.Game.ExecutableName) + " - Player " + (i + 1) + ".exe";
                            }

                            Process[] procs = Process.GetProcesses();
                            for (int j = 0; j < procs.Length; j++)
                            {
                                Process p = procs[j];

                                string lowerP = p.ProcessName.ToLower();

                                if (lowerP == proceName)
                                {
                                    if (!attachedIds.Contains(p.Id))
                                    {
                                        if (p.ProcessName == "javaw" || p.ProcessName == "GRW" || p.ProcessName == "steamclient_loader")
                                        {
                                            if ((int)p.NucleusGetMainWindowHandle() == 0)
                                            {
                                                continue;
                                            }
                                        }

                                        Log(string.Format("Found process {0} (pid {1})", p.ProcessName, p.Id));

                                        attached.Add(p);
                                        attachedIds.Add(p.Id);
                                        player.ProcessID = p.Id;
                                        if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                                        {
                                            keyboardProcId = p.Id;
                                        }

                                        proc = p;
                                        prevProcId = p.Id;

                                        break;
                                    }
                                }
                            }

                            if (proc != null)
                            {
                                break;
                            }
                        }
                    }

                    if (proc == null || gen.ForceProcessPick)
                    {
                        if(gen.UseForceBindIP)
                        {
                            Thread.Sleep(5000);
                        }

                        proc = ProcessPickerRuntime.LaunchProcessPick(player);
                    }

                }
                else
                {
                    Log(string.Format("Obtained process {0} (pid {1})", proc.ProcessName, proc.Id));
                    attached.Add(proc);
                    attachedIds.Add(proc.Id);
                    player.ProcessID = proc.Id;

                    if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                    {
                        keyboardProcId = proc.Id;
                    }
                }

                if (processingExit)
                {
                    return string.Empty;
                }

                if (!ProcessUtil.IsRunning(proc))
                {
                    Log("Process is no longer running. Attempting to find process by window title");
                    Process[] processes = Process.GetProcesses();
                    foreach (var process in processes)
                    {
                        if (process.MainWindowTitle == gen.Hook.ForceFocusWindowName && !attachedIds.Contains(process.Id))
                        {
                            Log("Process found, " + process.ProcessName + " pid (" + process.Id + ")");
                            proc = process;
                            attached.Add(proc);
                            attachedIds.Add(proc.Id);
                            player.ProcessID = proc.Id;
                            if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                            {
                                keyboardProcId = proc.Id;
                            }
                        }
                    }
                }
              
                Log(string.Format("Process details; Name: {0}, ID: {1}, MainWindowtitle: {2}, NucleusGetMainWindowHandle(): {3}", proc.ProcessName, proc.Id, proc.MainWindowTitle, proc.NucleusGetMainWindowHandle()));

                if (gen.WriteToProcessMemory?.Length > 0)
                {
                    if (gen.WriteToProcessMemory.Contains('|'))
                    {
                        ProcessUtil.WriteToProcessMemory(gen, proc);
                    }
                }

                if (gen.GoldbergLobbyConnect && i == 0)
                {
                    SteamFunctions.GoldbergLobbyConnect();
                }

                if (i > 0 && gen.ResetWindows && prevProcessData != null)
                {
                    GlobalWindowMethods.ResetWindows(prevProcessData, prevWindowX, prevWindowY, prevWindowWidth, prevWindowHeight, i);
                }

                Log("Setting process data to process " + proc.ProcessName + " (pid " + proc.Id + ")");
                ProcessData data = new ProcessData(proc);
                prevProcessData = data;

                playerBoundsWidth = playerBounds.Width;
                playerBoundsHeight = playerBounds.Height;

                if (context.Hook.WindowX > 0 && context.Hook.WindowY > 0)
                {
                    data.Position = new Point(context.Hook.WindowX, context.Hook.WindowY);
                    prevWindowX = context.Hook.WindowX;
                    prevWindowY = context.Hook.WindowY;
                }
                else
                {
                    data.Position = new Point(playerBounds.X, playerBounds.Y);
                    prevWindowX = playerBounds.X;
                    prevWindowY = playerBounds.Y;
                }

                if (context.Hook.ResWidth > 0 && context.Hook.ResHeight > 0)
                {
                    data.Size = new Size(context.Hook.ResWidth, context.Hook.ResHeight);
                    prevWindowWidth = context.Hook.ResWidth;
                    prevWindowHeight = context.Hook.ResHeight;
                }
                else
                {
                    data.Size = new Size(playerBounds.Width, playerBounds.Height);
                    prevWindowWidth = playerBounds.Width;
                    prevWindowHeight = playerBounds.Height;
                }

                data.KilledMutexes = context.KillMutex?.Length == 0;
                player.ProcessData = data;

               PlayerInfo profilePlayer = null;

                //Using static GameProfile 
                if (GameProfile.ProfilePlayersList.Count > 0)
                {
                    profilePlayer = GameProfile.ProfilePlayersList[i];
                }

                if (profilePlayer?.PriorityClass != "Normal" && profilePlayer?.PriorityClass != null)
                {
                    player.PriorityClass = profilePlayer.PriorityClass;
                    gen.ProcessorPriorityClass = profilePlayer.PriorityClass;
                    ProcessUtil.SetProcessorPriorityClass(gen, proc);
                }
                else if (gen.ProcessorPriorityClass?.Length > 0)
                {
                    ProcessUtil.SetProcessorPriorityClass(gen, proc);
                }

                if (profilePlayer?.IdealProcessor != "*" && profilePlayer?.IdealProcessor != null)
                {
                    gen.IdealProcessor = int.Parse(profilePlayer.IdealProcessor) - 1;
                    ProcessUtil.SetIdealProcessor(gen, proc);
                }
                else if (gen.IdealProcessor > 0)
                {
                    ProcessUtil.SetIdealProcessor(gen, proc);
                }

                if (profilePlayer?.Affinity != "" && profilePlayer?.Affinity != null)
                {
                    player.Affinity = profilePlayer.Affinity;
                    gen.UseProcessor = profilePlayer.Affinity;
                    ProcessUtil.SetProcessorProcessorAffinity(gen, proc);
                }
                else if ((gen.UseProcessor != null ? (gen.UseProcessor.Length > 0 ? 1 : 0) : 0) != 0)
                {
                    ProcessUtil.SetProcessorProcessorAffinity(gen, proc);
                }
                else
                {
                    ProcessUtil.SetProcessorAffinityPerInstance(gen, proc, i);
                }

                if (gen.IdInWindowTitle || !string.IsNullOrEmpty(gen.FlawlessWidescreen))
                {
                    if ((int)proc.NucleusGetMainWindowHandle() == 0)
                    {
                        for (int times = 0; times < 200; times++)
                        {
                            Thread.Sleep(50);
                            if ((int)proc.NucleusGetMainWindowHandle() > 0)
                            {
                                break;
                            }
                        }
                    }
                    if ((int)proc.NucleusGetMainWindowHandle() > 0)
                    {
                        string windowTitle = proc.MainWindowTitle + "(" + i + ")";
                        if (!string.IsNullOrEmpty(gen.FlawlessWidescreen))
                        {
                            windowTitle = "Nucleus Instance " + (i + 1) + "(" + gen.Hook.ForceFocusWindowName + ")";
                        }
                        Log(string.Format("Setting window text to {0}", windowTitle));
                        GlobalWindowMethods.SetWindowText(proc, windowTitle);
                    }
                    else
                    {
                        Log(string.Format("ERROR - IdInWindowTitle could not find main window handle for {0} (pid {1})", proc.ProcessName, proc.Id));
                        MessageBox.Show(string.Format("IdInWindowTitle: Could not find main window handle for {0} (pid:{1})", proc.ProcessName, proc.Id), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (!gen.ProtoInput.InjectStartup &&
                    (gen.ProtoInput.InjectRuntime_EasyHookMethod ||
                     gen.ProtoInput.InjectRuntime_EasyHookStealthMethod ||
                     gen.ProtoInput.InjectRuntime_RemoteLoadMethod))
                {
                    Log("Injecting ProtoInput at runtime into pid " + (uint)proc.Id);

                    ProtoInputLauncher.InjectRuntime(
                        gen.ProtoInput.InjectRuntime_EasyHookMethod,
                        gen.ProtoInput.InjectRuntime_EasyHookStealthMethod,
                        gen.ProtoInput.InjectRuntime_RemoteLoadMethod,
                        (uint)proc.Id,
                        NucleusFolderPath,
                        i + 1,
                        gen,
                        player,
                        (player.IsRawMouse ? (int)player.RawMouseDeviceHandle : -1),
                        (player.IsRawKeyboard ? (int)player.RawKeyboardDeviceHandle : -1),
                        (gen.ProtoInput.MultipleProtoControllers ? (player.ProtoController1) : ((player.IsRawMouse || player.IsRawKeyboard) ? 0 : player.GamepadId + 1)),
                        (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController2 : 0),
                        (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController3 : 0),
                        (gen.ProtoInput.MultipleProtoControllers ? player.ProtoController4 : 0)
                    );
                }

                if (gen.PromptAfterFirstInstance)
                {
                    if (i == 0)
                    {
                        Log(string.Format("Prompted user after first instance", (i + 2)));
                        Prompt prompt = new Prompt("Press OK when ready to launch the rest of the instances.");
                        prompt.ShowDialog();
                    }
                }

                if (gen.PromptBetweenInstances && i < (players.Count - 1))
                {
                    if (gen.PauseBetweenStarts > 0)
                    {
                        Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                    }

                    Log(string.Format("Prompted user for Instance {0}", (i + 2)));

                    Prompt prompt = new Prompt("Press OK when ready to launch instance " + (i + 2) + ".");
                    prompt.ShowDialog();
                }
                else if (gen.PromptBetweenInstances && i == players.Count - 1 && (gen.HookFocus || gen.FakeFocus || gen.SetWindowHook || gen.HideCursor || gen.PreventWindowDeactivation || gen.SetTopMostAtEnd))
                {
                    if (gen.PauseBetweenStarts > 0)
                    {
                        Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                    }

                    Log("Prompted user to install post hooks");

                    Prompt prompt = new Prompt("Press OK when ready to install hooks and/or start sending fake messages.");
                    prompt.ShowDialog();

                    foreach (Process aproc in attached)
                    {
                        IntPtr topMostFlag = new IntPtr(-1);

                        if (gen.NotTopMost)
                        {
                            topMostFlag = new IntPtr(-2);
                        }

                        User32Interop.SetWindowPos(aproc.NucleusGetMainWindowHandle(), topMostFlag, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_SHOWWINDOW));
                    }
                }
                else
                {
                    if (!gen.PromptAfterFirstInstance || (gen.PromptAfterFirstInstance && i > 0))
                    {
                        Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenStarts));
                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenStarts));
                    }
                }

                if (!ProcessUtil.IsRunning(proc))
                {
                    Log("Process is no longer running. Attempting to find process by window title");

                    Process[] processes = Process.GetProcesses();

                    foreach (var process in processes)
                    {
                        if (process.MainWindowTitle == gen.Hook.ForceFocusWindowName && !attachedIds.Contains(process.Id))
                        {
                            Log("Process found, " + process.ProcessName + " pid (" + process.Id + ")");
                            proc = process;
                            attached.Add(proc);
                            attachedIds.Add(proc.Id);
                            player.ProcessID = proc.Id;
                            if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                            {
                                keyboardProcId = proc.Id;
                            }

                            Log("Recreating player process data");
                            data = new ProcessData(proc);
                            prevProcessData = data;

                            playerBoundsWidth = playerBounds.Width;
                            playerBoundsHeight = playerBounds.Height;

                            if (context.Hook.WindowX > 0 && context.Hook.WindowY > 0)
                            {
                                data.Position = new Point(context.Hook.WindowX, context.Hook.WindowY);
                                prevWindowX = context.Hook.WindowX;
                                prevWindowY = context.Hook.WindowY;
                            }
                            else
                            {
                                data.Position = new Point(playerBounds.X, playerBounds.Y);
                                prevWindowX = playerBounds.X;
                                prevWindowY = playerBounds.Y;
                            }

                            if (context.Hook.ResWidth > 0 && context.Hook.ResHeight > 0)
                            {
                                data.Size = new Size(context.Hook.ResWidth, context.Hook.ResHeight);
                                prevWindowWidth = context.Hook.ResWidth;
                                prevWindowHeight = context.Hook.ResHeight;
                            }
                            else
                            {
                                data.Size = new Size(playerBounds.Width, playerBounds.Height);
                                prevWindowWidth = playerBounds.Width;
                                prevWindowHeight = playerBounds.Height;
                            }

                            data.KilledMutexes = context.KillMutex?.Length == 0;
                            player.ProcessData = data;

                            Log(string.Format("Process details; Name: {0}, ID: {1}, MainWindowtitle: {2}, NucleusGetMainWindowHandle(): {3}", proc.ProcessName, proc.Id, proc.MainWindowTitle, proc.NucleusGetMainWindowHandle()));
                        }
                    }
                }
             
                //Set up raw input window
                //if (player.IsRawKeyboard || player.IsRawMouse)
                {
                    var window = GlobalWindowMethods.CreateRawInputWindow(proc, player);
                    nextWindowToInject = window;
                }

                if (i == (players.Count - 1))
                {
                    if (processingExit)
                    {
                        return string.Empty;
                    }

                    Log("All instances accounted for, performing final preperations");

                    //Audio routing is never called if the handler use ProcessChangeAtEnd()
                    //Add try/catch we never know
                    if (GameProfile.AudioInstances.Count > 0)
                    {
                        for (int pi = 0; pi < players.Count; pi++)
                        {
                            if ((pi + 1) < GameProfile.AudioInstances.Count())
                            {
                                if (GameProfile.AudioCustomSettings && GameProfile.AudioInstances["AudioInstance" + (pi + 1)] != "Default")
                                {
                                    Log($"Attempting to switch audio endpoint for process {players[pi].ProcessData.Process.ProcessName} pid ({players[pi].ProcessID}) to DeviceID {GameProfile.AudioInstances["AudioInstance" + (pi + 1)]}");
                                    Thread.Sleep(1000);
                                    try
                                    {
                                        AudioReroute.SwitchProcessTo(GameProfile.AudioInstances["AudioInstance" + (pi + 1)], AudioReroute.ERole.ERole_enum_count, AudioReroute.EDataFlow.eRender, (uint)players[pi].ProcessID);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log(ex.StackTrace + "\n" + ex.Message);
                                    }
                                }
                            }
                        }
                    }

                    if (gen.KillLastInstanceMutex && !gen.RenameNotKillMutex)
                    {
                        for (; ; )
                        {
                            if (gen.KillMutex != null)
                            {
                                if (gen.KillMutex.Length > 0 && !players[i].ProcessData.KilledMutexes)
                                {
                                    if (gen.KillMutexDelay > 0)
                                    {
                                        Thread.Sleep((gen.KillMutexDelay * 1000));
                                    }

                                    if (StartGameUtil.MutexExists(players[i].ProcessData.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex))
                                    {
                                        // mutexes still exist, must kill
                                        StartGameUtil.KillMutex(players[i].ProcessData.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex);
                                        players[i].ProcessData.KilledMutexes = true;
                                        break;
                                    }
                                    else
                                    {
                                        // mutexes dont exist anymore
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }

                            if (processingExit)
                            {
                                return string.Empty;
                            }
                        }
                    }

                    Thread.Sleep(1000);

                    if (gen.ResetWindows)
                    {
                        GlobalWindowMethods.ResetWindows(data, prevWindowX, prevWindowY, prevWindowWidth, prevWindowHeight, i + 1);
                    }

                    if (gen.FakeFocus)
                    {
                        Log($"Start sending fake focus messages every {gen.FakeFocusInterval} ms");
                        WindowFakeFocus.Initialize();
                        WindowFakeFocus.fakeFocus = new Thread(WindowFakeFocus.SendFocusMsgs);
                        WindowFakeFocus.fakeFocus.Start();
                    }

                    if (gen.ForceWindowTitle)
                    {
                        foreach (Process aproc in attached)
                        {
                            if ((int)aproc.NucleusGetMainWindowHandle() == 0)
                            {
                                for (int times = 0; times < 200; times++)
                                {
                                    Thread.Sleep(50);
                                    if ((int)aproc.NucleusGetMainWindowHandle() > 0)
                                    {
                                        break;
                                    }
                                }
                            }
                            if ((int)aproc.NucleusGetMainWindowHandle() > 0)
                            {
                                Log(string.Format("Renaming window title {0} to {1} for pid {2}", aproc.NucleusGetMainWindowHandle(), gen.Hook.ForceFocusWindowName, aproc.Id));
                                GlobalWindowMethods.SetWindowText(aproc, gen.Hook.ForceFocusWindowName);
                            }
                            else
                            {
                                Log(string.Format("ERROR - ChangeWindowTitle could not find main window handle for {0} (pid:{1})", aproc.ProcessName, aproc.Id));
                                MessageBox.Show(string.Format("ChangeWindowTitle: Could not find main window handle for {0} (pid:{1})", aproc.ProcessName, aproc.Id), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }                 

                    if (!string.IsNullOrEmpty(gen.FlawlessWidescreen))
                    {
                        for (int fw = 0; fw < players.Count; fw++)
                        {
                            Process fwProc = Process.GetProcessById(players[fw].ProcessData.Process.Id);
                            string windowTitle = "Nucleus Instance " + (fw + 1) + "(" + gen.Hook.ForceFocusWindowName + ")";

                            if (fwProc.MainWindowTitle != windowTitle)
                            {
                                Log(string.Format("Resetting window text for pid {0} to {0}", fwProc.Id, windowTitle));
                                GlobalWindowMethods.SetWindowText(fwProc, windowTitle);
                            }
                        }
                    }

                    if ((i > 0 || players.Count == 1) && (gen.HookFocus || gen.SetWindowHook || gen.HideCursor || gen.PreventWindowDeactivation || gen.SupportsMultipleKeyboardsAndMice))
                    {
                        if (gen.PreventWindowDeactivation && !isPrevent)
                        {
                            Log("PreventWindowDeactivation detected, setting flag");
                            isPrevent = true;
                        }

                        if (isPrevent)
                        {
                            if (players[i].IsKeyboardPlayer && gen.KeyboardPlayerSkipPreventWindowDeactivate)
                            {
                                Log("Ignoring PreventWindowDeactivation for keyboard player");
                                gen.PreventWindowDeactivation = false;
                            }
                            else
                            {
                                Log("Keeping PreventWindowDeactivation on");
                                gen.PreventWindowDeactivation = true;
                            }
                        }

                        if (gen.PostHookInstances?.Length > 0)
                        {
                            string[] instancesToHook = gen.PostHookInstances.Split(',');
                            foreach (string instanceToHook in instancesToHook)
                            {
                                if (int.Parse(instanceToHook) == (i + 1))
                                {
                                    Log("Injecting hook DLL for last instance");
                                    User32Interop.SetForegroundWindow(data.Process.NucleusGetMainWindowHandle());
                                    DllsInjector.InjectDLLs(data.Process, nextWindowToInject, players[i]);
                                }
                            }
                        }
                        else
                        {
                            Log("Injecting hook DLL for last instance");
                            User32Interop.SetForegroundWindow(data.Process.NucleusGetMainWindowHandle());
                            DllsInjector.InjectDLLs(data.Process, nextWindowToInject, players[i]);
                        }
                    }
          
                    if (!gen.IgnoreWindowBorderCheck)
                    {
                        GlobalWindowMethods.RemoveBorder();
                    }

                    // Fake mouse cursors
                    if (gen.DrawFakeMouseCursor)
                    {
                        RawInputManager.CreateCursorsOnWindowThread(gen.UpdateFakeMouseWithInternalInput, gen.DrawFakeMouseCursorForControllers);
                    }

                    //Window setup
                    foreach (var window in RawInputManager.windows)
                    {
                        var hWnd = window.hWnd;

                        //Borderlands 2 (and some other games) requires WM_INPUT to be sent to a window named DIEmWin, not the main hWnd.
                        foreach (ProcessThread thread in Process.GetProcessById(window.pid).Threads)
                        {
                            int WindowEnum(IntPtr _hWnd, int lParam)
                            {
                                var threadId = WinApi.GetWindowThreadProcessId(_hWnd, out int pid);
                                if (threadId == lParam)
                                {
                                    string windowText = WinApi.GetWindowText(_hWnd);

                                    if (windowText != null && windowText.ToLower().Contains("DIEmWin".ToLower()))
                                    {
                                        window.DIEmWin_hWnd = _hWnd;
                                    }
                                }

                                return 1;
                            }

                            WinApi.EnumWindows(WindowEnum, thread.Id);
                        }
                    }

                    RawInputProcessor.Start();

                    if (gen.SetForegroundWindowElsewhere)
                    {
                        Log("Setting the foreground window to Nucleus");                    
                        GlobalWindowMethods.ChangeForegroundWindow();                                           
                    }

                    if (gen.SendFakeFocusMsg)
                    {
                        WindowFakeFocus.Initialize();
                        WindowFakeFocus.SendFakeFocusMsg();
                    }
                }   
            }

            if (gen.LockInputAtStart)
            {
                Thread.Sleep(5000);

                if (gen.ToggleUnfocusOnInputsLock)
                {
                    GlobalWindowMethods.ChangeForegroundWindow();
                }

                LockInputRuntime.Lock(gen.LockInputSuspendsExplorer, gen.ProtoInput.FreezeExternalInputWhenInputNotLocked, gen?.ProtoInput);

                Globals.MainOSD.Show(1600, "Inputs Locked");
            }

            // Call the input lock/unlock callbacks, just in case they haven't been called with the players fully setup
            if (LockInputRuntime.IsLocked)
            {
                gen.ProtoInput.OnInputLocked?.Invoke();
            }
            else
            {
                gen.ProtoInput.OnInputUnlocked?.Invoke();
            }

            if (gen.SetTopMostAtEnd && (!gen.PromptBetweenInstances && !gen.NotTopMost))
            {
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerInfo p = players[i];
                    ProcessData data = p.ProcessData;

                    Log("Set game window to top most");
                    data.HWnd.TopMost = true;
                }
            }

            if (gen.UseNucleusEnvironment)
            {
                RegistryUtil.RestoreUserEnvironmentRegistryPath();
            }

            if (!processingExit)
            {
                gen.MetaInfo.StartGameplayTimerThread();
                GamepadNavigation.StopUINavigation();
                GameProfile.SaveGameProfile(profile);         
            }

            gen.OnFinishedSetup?.Invoke();

            WindowsMerger.Instance?.InsertGameWindows();
          
            Log("All done!");

            return string.Empty;
        }

        private void ProcessChangeAtEnd()
        {
            List<PlayerInfo> players = profile.DevicesList;

            Log("Starting process end changes");

            for (int i = 0; i < players.Count; i++)
            {
                Log("Setting up player " + (i + 1));

                PlayerInfo player = players[i];
                Rectangle playerBounds = player.MonitorBounds;

                owner = player.Owner;
                playerBoundsWidth = playerBounds.Width;
                playerBoundsHeight = playerBounds.Height;

                if (gen.ChangeIPPerInstance)
                {
                    Network.ChangeIPPerInstance(i);
                }

                bool setupDll = true;
                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame && i > 0)
                {
                    setupDll = false;
                }

                if (gen.XInputPlusDll?.Length > 0)
                {
                    XInputPlusDll.SetupXInputPlusDll(player, i, setupDll);
                }

                if (gen.UseDevReorder)
                {
                    DevReorder.UseDevReorder(player, i, setupDll);
                }

                if (gen.UseX360ce)
                {
                    X360ce.UseX360ce(i, player, setupDll);
                }

                if (gen.Hook.SDL2Enabled)
                {
                    SDL2Dll.SetupSDL2Dll(player, i, setupDll);
                }

                if (gen.PromptBetweenInstancesEnd)
                {
                    Log(string.Format("Prompted user for Instance {0}", (i + 1)));

                    Prompt prompt = new Prompt("Press OK when game instance " + (i + 1) + " has been launched and/or you wish to continue.");
                    prompt.ShowDialog();
                }
                else
                {
                    Thread.Sleep(1000);
                }

                ProcessData pdata = null;

                if (player.ProcessData != null)
                {
                    Log(string.Format("Using player process data. Process: {0} (pid {1})", player.ProcessData.Process.ProcessName, player.ProcessData.Process.Id));
                    pdata = player.ProcessData;
                }
                else
                {
                    Process[] cProcs;

                    Log("Player process data was null. Attempting to find process by executable name and start time");

                    try
                    {
                        cProcs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gen.ExecutableName)).OrderBy(p => p.StartTime).ToArray();
                    }
                    catch
                    {
                        Log("Unable to get processes by executable name ordered by their start time, using just by exe name alone");
                        cProcs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(gen.ExecutableName));
                    }

                    foreach (Process p in cProcs)
                    {
                        if (p.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(gen.ExecutableName.ToLower()) && !attachedIds.Contains(p.Id))
                        {
                            Log(string.Format("Found process {0} (pid {1})", p.ProcessName, p.Id));
                            pdata = new ProcessData(p);
                            player.ProcessData = pdata;
                            attachedIds.Add(p.Id);
                            attached.Add(p);
                            player.ProcessID = p.Id;
                            break;
                        }
                    }
                }

                Thread.Sleep(1000);

                Process proc = player.ProcessData.Process;

                var window = GlobalWindowMethods.CreateRawInputWindow(proc, players[i]);

                Thread.Sleep(1000);

                if (gen.HookFocus || gen.SetWindowHook || gen.HideCursor || gen.PreventWindowDeactivation || gen.SupportsMultipleKeyboardsAndMice)
                {
                    Log("Injecting post-launch hooks for process " + proc.ProcessName + " (pid " + proc.Id + ")");

                    if (gen.PreventWindowDeactivation && !isPrevent)
                    {
                        Log("PreventWindowDeactivation detected, setting flag");
                        isPrevent = true;
                    }

                    if (isPrevent)
                    {
                        if (players[i].IsKeyboardPlayer && gen.KeyboardPlayerSkipPreventWindowDeactivate)
                        {
                            Log("Ignoring PreventWindowDeactivation for keyboard player");
                            gen.PreventWindowDeactivation = false;
                        }
                        else
                        {
                            Log("Keeping PreventWindowDeactivation on");
                            gen.PreventWindowDeactivation = true;
                        }
                    }

                    if (gen.PostHookInstances?.Length > 0)
                    {
                        string[] instancesToHook = gen.PostHookInstances.Split(',');
                        foreach (string instanceToHook in instancesToHook)
                        {
                            if (int.Parse(instanceToHook) == (i + 1))
                            {
                                User32Interop.SetForegroundWindow(proc.NucleusGetMainWindowHandle());
                                DllsInjector.InjectDLLs(proc, window, players[i]);
                            }
                        }
                    }
                    else
                    {
                        User32Interop.SetForegroundWindow(proc.NucleusGetMainWindowHandle());
                        DllsInjector.InjectDLLs(proc, window, players[i]);
                    }
                }

                Thread.Sleep(1000);

                GlobalWindowMethods.ChangeGameWindow(proc, players, i);

                Thread.Sleep(1000);

                if (gen.KillMutexAtEnd)
                {
                    for (; ; )
                    {
                        Thread.Sleep(1000);

                        if (gen.KillMutex != null)
                        {
                            Log("Beginning mutex killing");
                            if (gen.KillMutex.Length > 0)
                            {
                                if (gen.KillMutexDelay > 0)
                                {
                                    Thread.Sleep((gen.KillMutexDelay * 1000));
                                }

                                if (StartGameUtil.MutexExists(pdata.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex))
                                {
                                    // mutexes still exist, must kill
                                    StartGameUtil.KillMutex(pdata.Process, gen.KillMutexType, gen.PartialMutexSearch, gen.KillMutex);
                                    pdata.KilledMutexes = true;
                                    break;
                                }
                                else
                                {
                                    // mutexes dont exist anymore
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                        if (processingExit)
                        {
                            break;
                        }
                    }
                }

                if (gen.ResetWindows)
                {
                    Thread.Sleep(1000);
                    GlobalWindowMethods.ResetWindows(players[i].ProcessData, players[i].MonitorBounds.X, players[i].MonitorBounds.Y, players[i].MonitorBounds.Width, players[i].MonitorBounds.Height, i + 1);
                }

                Thread.Sleep(3000);

                if (i == (players.Count - 1))
                {
                    Log("End process changes - All done!");                  
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            if (gen.FakeFocus)
            {
                Log("Start sending fake focus messages every 1000 ms");
                WindowFakeFocus.Initialize();
                WindowFakeFocus.fakeFocus = new Thread(WindowFakeFocus.SendFocusMsgs);
                WindowFakeFocus.fakeFocus.Start();
            }

            if (gen.SetForegroundWindowElsewhere)
            {
                GlobalWindowMethods.ChangeForegroundWindow();
            }

            if (gen.UseNucleusEnvironment)
            {
                RegistryUtil.RestoreUserEnvironmentRegistryPath();
            }

            if (!processingExit)
            {
                gen.MetaInfo.StartGameplayTimerThread();
                GamepadNavigation.StopUINavigation();
                GameProfile.SaveGameProfile(profile);
            }

            gen.OnFinishedSetup?.Invoke();

            WindowsMerger.Instance?.InsertGameWindows();   
        }

        struct UpdateTickThread
        {
            public double Interval;
        }

        public void StartUpdateTick(double interval)
        {
            Thread updateTickThread = new Thread(StartUpdateTickThread);

            UpdateTickThread tick = new UpdateTickThread
            {
                Interval = interval
            };

            updateTickThread.Start(tick);
        }

        private void StartUpdateTickThread(object state)
        {
            UpdateTickThread updateTickThread = (UpdateTickThread)state;

            for (; ; )
            {
                if (!string.IsNullOrEmpty(error))
                {
                    End(false);
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(updateTickThread.Interval));
                Update(updateTickThread.Interval, false);

                if (hasEnded)
                {
                    break;
                }
            }
        }

        public void Update(double delayMS, bool refresh)
        {
            GlobalWindowMethods.UpdateAndRefreshGameWindows(delayMS, refresh);
        }

        public void End(bool fromStopButton)
        {
            if (fromStopButton && LockInputRuntime.IsLocked)
            {
                //TODO/: For some reason the Stop button is clicked during split screen. Temporary fix is to not end if input is locked.//Should be fixed now by unfocusing the stop button.
                Log("IGNORING SHUTDOWN BECAUSE INPUT LOCKED");
                return;
            }

            if (!processingExit)
            {
                processingExit = true;
            }
            else
            {
                Log("Already processing exit");
                return;
            }

            Log("----------------- SHUTTING DOWN -----------------");

            if(GlobalWindowMethods.CutsceneOn)
            {
                GlobalWindowMethods.ToggleCutScenesMode();
            }

            gen.MetaInfo.StopGameplayTimerThread();
            GamepadNavigation.StartUINavigation();

            GlobalWindowMethods.ResetBools();
           
            WindowsMerger.Instance?.Dispose();
           
            if (splitForms.Count > 0)
            {
                foreach (WPFDiv backgroundForm in splitForms)
                {
                    try
                    {
                        backgroundForm.Dispatcher.Invoke(new Action(() =>
                        {
                            backgroundForm.Close();
                        }));
                    }
                    catch
                    { }
                }

                splitForms.Clear();
            }

            if (shortcutsReminders.Count > 0)
            {
                foreach (ShortcutsReminder shortcutsReminder in shortcutsReminders)
                {
                    try
                    {
                        shortcutsReminder.Invoke(new Action(() =>
                        {
                            shortcutsReminder?.Close();
                        }));
                    }
                    catch
                    { }
                }

                shortcutsReminders.Clear();
            }

            if (AllRuntimeForms.Count > 0)
            {
                foreach (Form runtimeForm in AllRuntimeForms)
                {
                    try
                    {
                        runtimeForm?.Invoke(new Action(() =>
                        {
                            runtimeForm?.Close();
                        }));
                    }
                    catch
                    { }
                }

                AllRuntimeForms.Clear();
            }

            LockInputRuntime.Unlock(false, gen?.ProtoInput);

            gen.OnStop?.Invoke();

            ProcessUtil.KillRemainingProcess();

            Thread.Sleep(1000);

            if (gen.CMDBatchClose?.Length > 0)
            {
                Process cmd = new Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.UseShellExecute = false;

                cmd.Start();

                for (int x = 0; x < gen.CMDBatchClose.Length; x++)
                {
                    Log("Running command line: " + gen.CMDBatchClose[x]);
                    cmd.StandardInput.WriteLine(gen.CMDBatchClose[x]);
                }

                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
            }

            Thread.Sleep(1000);

            if (gen.NeedsSteamEmulation && !gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame && !string.IsNullOrEmpty(ssePath))
            {
                Log("Deleting SmartSteamEmu folder and files");
                Directory.Delete(ssePath, true);
                Thread.Sleep(1000);
            }

            if (gen.DeleteOnClose?.Length > 0)
            {
                string exeFolder = Path.GetDirectoryName(userGame.ExePath).ToLower();
                string rootFolder = exeFolder;

                if (!string.IsNullOrEmpty(gen.BinariesFolder) && !gen.BinariesFolderPathFix)
                {
                    rootFolder = StringUtil.ReplaceCaseInsensitive(exeFolder, gen.BinariesFolder.ToLower(), "");
                }
                else if (!string.IsNullOrEmpty(gen.BinariesFolder) && gen.BinariesFolderPathFix)
                {
                    rootFolder = StringUtil.GetRootFromBinariesFolder(exeFolder, gen.BinariesFolder);
                }

                foreach (string file in gen.DeleteOnClose)
                {
                    string fullFilePath = Path.Combine(rootFolder, file);
                    if (File.Exists(fullFilePath))
                    {
                        Log("DeleteOnClose: Deleting " + fullFilePath);
                        File.Delete(fullFilePath);
                    }
                }
            }

            if (userBackedFiles?.Count > 0)
            {
                for (int i = 0; i < userBackedFiles.Count; i++)
                {
                    string filePath = userBackedFiles[i];

                    string origFileName = filePath.Replace($"_NUCLEUS_BACKUP" + Path.GetExtension(filePath), Path.GetExtension(filePath));
                    Log($"Restoring {origFileName}");

                    if (File.Exists(origFileName) && File.Exists(filePath))
                    {
                        File.Delete(origFileName);
                    }

                    File.Move(filePath, origFileName);
                }
            }
            else
            {
                Log("No Nucleus backed up files found");
            }

            Thread.Sleep(1000);

            RegistryUtil.RestoreRegistry("Restore from GenericGameHandler");

            Thread.Sleep(1000);

            MonitorsDpiScaling.ResetMonitorsSettings();

            Thread.Sleep(1000);

            if (!earlyExit)
            {
                folderUsers = Directory.GetDirectories(Path.GetDirectoryName(Globals.UserEnvironmentRoot));

                if (gen.TransferNucleusUserAccountProfiles)
                {
                    NucleusUsers.TransferNucleusUserAccountProfiles(profile.DevicesList);
                }

                if (!App_Misc.KeepAccounts)
                {
                    if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
                    {
                        WindowsUsersUtil.DeleteCreatedWindowsUser();
                        Thread.Sleep(1000);
                    }
                }

                if (gen.ChangeIPPerInstanceAlt)
                {
                    Network.ChangeIPPerInstanceAltDeleteAdapter();
                    Thread.Sleep(1000);
                }

                if (gen.ChangeIPPerInstance)
                {
                    Network.ChangeIPPerInstanceRestoreIP();
                }

                if (gen.FlawlessWidescreen?.Length > 0)
                {
                    FlawlessWidescreen.KillFlawlessWidescreen();
                }
            }

            GameManager.Instance.ExecuteBackup(userGame.Game);

            LogManager.UnregisterForLogCallback(this);

            foreach (var window in RawInputManager.windows)
            {
                window.HookPipe?.Close();
            }

            Cursor.Clip = Rectangle.Empty; // guarantee were not clipping anymore

            _cursorModule?.Stop();

            Thread.Sleep(1000);

            RawInputManager.EndSplitScreen();

            BackupDatas.ProceedBackup();            
            
            // delete symlink folder and users accounts 
#if RELEASE
            if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
            {
                FileUtil.CleanOriginalgGameFolder();
            }

            if (!gen.KeepSymLinkOnExit && !CurrentGameInfo.MetaInfo.KeepSymLink)
            {
                CleanGameContent.CleanContentFolder(gen);
            }

            if  (!App_Misc.KeepAccounts)
            {
                WindowsUsersUtil.DeleteCreatedWindowsUserFolder();
            }
#endif
            Log("All done closing operations.");
           
            Ended?.Invoke();

            hasEnded = true;
            IsRunning = false;
        }

        public void Log(StreamWriter writer)
        {
        }

        public void Log(string logMessage)
        {
            try
            {
                if (App_Misc.DebugLog)
                {
                    using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                    {
                        writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]HANDLER: {logMessage}");
                        writer.Close();
                    }
                }
            }
            catch { }
        }
    }
}