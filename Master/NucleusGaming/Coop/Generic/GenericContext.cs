using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Nucleus.Gaming
{
    // This class is a holder for the GenericGameInfo class. It doesn't implement the IGenericGameInfo
    // because some of the elements are implemented differently to work with the JS engine
    // Comments can be found on the original class if no specific feature is implemented here
    public class GenericContext
    {
        public GameHookInfo Hook = new GameHookInfo();
        public double HandlerInterval;
        public bool Debug;
        public string Error;
        public int Interval;
        public bool SymlinkExe;
        public bool SupportsKeyboard;
        public string[] ExecutableContext;
        public string ExecutableName;
        public string SteamID;
        public string GUID;
        public string GameName;
        public int MaxPlayers;
        public int MaxPlayersOneMonitor;
        public SaveType SaveType;
        public SearchType searchType;
        public string SavePath;
        public string StartArguments;
        public string BinariesFolder;
        public string WorkingFolder;
        public bool NeedsSteamEmulation;
        public string[] KillMutex;
        public string KillMutexType;
        public int KillMutexDelay;
        public bool KeepSymLinkOnExit;
        public string LauncherExe;
        public string LauncherTitle;
        public int PlayerID;
        public bool IsFullscreen;
        public UserInfo User = new UserInfo();
        public DPIHandling DPIHandling = DPIHandling.True;
        public bool FakeFocus;
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
        public bool UseX360ce;
        public string HookFocusInstances;
        //public bool UseAlpha8CustomDll;
        public bool bHasKeyboardPlayer;
        public bool KeepAspectRatio;
        //public bool HideDesktop;
        //public int FakeFocusInterval;
        public bool ResetWindows;
        public bool UseGoldberg;
        public string OrigSteamDllPath;
        public bool GoldbergNeedSteamInterface;
        public bool XboxOneControllerFix;
        public bool UseForceBindIP;
        public string[] XInputPlusDll;
        public bool XInputPlusOldDll;
        public string[] CopyCustomUtils;
        public int PlayersPerInstance;
        public bool UseDevReorder;
        public string[] CustomUserGeneralValues;
        public string[] CustomUserPlayerValues;
        public string[] CustomUserInstanceValues;
        public bool InjectHookXinput;
        public bool InjectDinputToXinputTranslation;
        public bool UseDInputBlocker;
        public bool BlockRawInput;
        public bool PreventWindowDeactivation;
        public string[] X360ceDll;
        public string PostHookInstances;
        public string StartHookInstances;
        public string FakeFocusInstances;
        public string[] CMDBatchBefore;
        public string[] CMDBatchAfter;
        public string[] CMDBatchClose;
        public bool CMDStartArgsInside;
        public string LauncherFolder;
        public string[] PlayerSteamIDs;
        public int NumControllers = 0;
        public int NumKeyboards = 0;
        public bool KeepEditedRegKeys;

        private List<string> regKeyPaths = new List<string>();

        public string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public string NucleusDocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string UtilFolder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location) + "\\utils";

        public Type HandlerType => typeof(GenericGameHandler);

        public Dictionary<string, object> Options => profile.Options;

        public string Arch => GenericGameHandler.Instance.garch;

        public int Width
        {
            get
            {
                switch (DPIHandling)
                {
                    case DPIHandling.Scaled:
                        return (int)((pInfo.MonitorBounds.Width * DPIManager.Scale) + 0.5);
                    case DPIHandling.InvScaled:
                        return (int)((pInfo.MonitorBounds.Width * (1 / DPIManager.Scale)) + 0.5);
                    case DPIHandling.True:
                    default:
                        return pInfo.MonitorBounds.Width;
                }
            }
        }

        public int Height
        {
            get
            {
                switch (DPIHandling)
                {
                    case DPIHandling.Scaled:
                        return (int)((pInfo.MonitorBounds.Height * DPIManager.Scale) + 0.5);
                    case DPIHandling.InvScaled:
                        return (int)((pInfo.MonitorBounds.Height * (1 / DPIManager.Scale)) + 0.5);
                    case DPIHandling.True:
                    default:
                        return pInfo.MonitorBounds.Height;
                }
            }
        }

        public int PosX => pInfo.MonitorBounds.X;

        public int PosY => pInfo.MonitorBounds.Y;

        public int MonitorWidth => pInfo.Display.Bounds.Width;

        public int MonitorHeight => pInfo.Display.Bounds.Height;

        [Dynamic(AutoHandles = true)]
        public string ExePath;
        [Dynamic(AutoHandles = true)]
        public string RootInstallFolder;
        [Dynamic(AutoHandles = true)]
        public string RootFolder;
        [Dynamic(AutoHandles = true)]
        public string OrigRootFolder;

        private GameProfile profile;
        private PlayerInfo pInfo;
        private GenericGameHandler parent;
        public GenericContext(GameProfile prof, PlayerInfo info, GenericGameHandler handler, bool hasKeyboard)
        {
            profile = prof;
            pInfo = info;
            parent = handler;

            bHasKeyboardPlayer = hasKeyboard;
        }

        public void Log(string msg)
        {
            LogManager.Log(msg);
        }

        public void Log(string msg, string val)
        {
            LogManager.Log(msg + ": " + val);
        }

        public void Log(string msg, int val)
        {
            LogManager.Log(msg + ": " + val.ToString());
        }

        public void Log(string msg, bool val)
        {
            LogManager.Log(msg + ": " + val.ToString());
        }

        public void Log(string msg, double val)
        {
            LogManager.Log(msg + ": " + val.ToString());
        }

        public void Log(string msg, string[] vals)
        {
            string val = string.Join(",", vals);
            LogManager.Log(msg + ": " + val.ToString());
        }

        public string HandlerGUID => parent.HandlerGUID;

        public string NucleusFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public int NumberOfPlayers => parent.numPlayers;

        public int ProcessID => pInfo.ProcessID;

        public bool HasKeyboardPlayer => bHasKeyboardPlayer;

        public string GetFolder(Folder folder)
        {
            return parent.GetFolder(folder);
        }

        public string x360ceGamepadGuid => "IG_" + pInfo.GamepadGuid.ToString().Replace("-", string.Empty);

        public string GamepadGuid => pInfo.GamepadGuid.ToString();

        public bool IsKeyboardPlayer => pInfo.IsKeyboardPlayer;
        public int GamepadId => pInfo.GamepadId + 1;
        public float OrigAspectRatioDecimal => (float)profile.Screens[pInfo.PlayerID].display.Width / profile.Screens[pInfo.PlayerID].display.Height;

        public string OrigAspectRatio
        {
            get
            {
                int width = profile.Screens[pInfo.PlayerID].display.Width;
                int height = profile.Screens[pInfo.PlayerID].display.Height;
                int gcd = GCD(width, height);
                return string.Format("{0}:{1}", width / gcd, height / gcd);
            }
        }

        public float AspectRatioDecimal => (float)Width / Height;

        public string AspectRatio
        {
            get
            {
                int gcd = GCD(Width, Height);
                return string.Format("{0}:{1}", Width / gcd, Height / gcd);
            }
        }

        public void ProceedSymlink()
        {
            string[] filesToSymlink = SymlinkFiles;
            for (int f = 0; f < filesToSymlink.Length; f++)
            {
                string s = filesToSymlink[f].ToLower();
                // make sure it's lower case
                CmdUtil.MkLinkFile(Path.Combine(OrigRootFolder, s), Path.Combine(RootFolder, s), out int exitCode);
                Console.WriteLine(OrigRootFolder + s + " => Instance folder " + RootFolder + s);
            }
        }

        public bool _hideTaskBar;
        public void HideTaskBar()
        {
            if (PlayerID > 0)
            {
                return;
            }

            User32Util.HideTaskbar();
            _hideTaskBar = true;
        }

        private string epicLang;
        public string EpicLang
        {
            get
            {
                IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

                IDictionary<string, string> epiclangs = new Dictionary<string, string>
                {
                    { "Arabic", "ar" },
                    { "Brazilian", "pt-BR" },
                    { "Bulgarian", "bg" },
                    { "Chinese", "zh" },
                    { "Czech", "cs" },
                    { "Danish", "da" },
                    { "Dutch", "nl" },
                    { "English", "en" },
                    { "Finnish", "fi" },
                    { "French", "fr" },
                    { "German", "de" },
                    { "Greek", "el" },
                    { "Hungarian", "hu" },
                    { "Italian", "it" },
                    { "Japanese", "ja" },
                    { "Koreana", "ko" },
                    { "Norwegian", "no" },
                    { "Polish", "pl" },
                    { "Portuguese", "pt" },
                    { "Romanian", "ro" },
                    { "Russian", "ru" },
                    { "Spanish", "es" },
                    { "Swedish", "sv" },
                    { "Thai", "th" },
                    { "Turkish", "tr" },
                    { "Ukrainian", "uk" }
                };


                foreach (KeyValuePair<string, string> lang in epiclangs)
                {
                    if (lang.Key == ini.IniReadValue("Misc", "EpicLang"))
                    {
                        epicLang = lang.Value;
                    }
                }
                return epicLang;
            }
        }

        public string UserName
        {
            get
            {
               Environment.UserName.Trim('\\').Last();
               return Environment.UserName.Trim();
            }
        }

        private string handlersFolder;
        public string HandlersFolder
        {
            get
            {
                handlersFolder = Path.Combine(GameManager.Instance.GetJsScriptsPath());
                return handlersFolder;
            }
        }

        private string gogLang;
        public string GogLang
        {
            get
            {
                IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

                IDictionary<string, string> epiclangs = new Dictionary<string, string>
                {
                    { "Arabic", "ar" },
                    { "Brazilian", "pt-BR" },
                    { "Bulgarian", "bg" },
                    { "Chinese", "zh" },
                    { "Czech", "cs" },
                    { "Danish", "da" },
                    { "Dutch", "nl" },
                    { "English", "en" },
                    { "Finnish", "fi" },
                    { "French", "fr" },
                    { "German", "de" },
                    { "Greek", "el" },
                    { "Hungarian", "hu" },
                    { "Italian", "it" },
                    { "Japanese", "ja" },
                    { "Koreana", "ko" },
                    { "Norwegian", "no" },
                    { "Polish", "pl" },
                    { "Portuguese", "pt" },
                    { "Romanian", "ro" },
                    { "Russian", "ru" },
                    { "Spanish", "es" },
                    { "Swedish", "sv" },
                    { "Thai", "th" },
                    { "Turkish", "tr" },
                    { "Ukrainian", "uk" }
                };


                foreach (KeyValuePair<string, string> lang in epiclangs)
                {
                    if (lang.Key == ini.IniReadValue("Misc", "EpicLang"))
                    {
                        gogLang = lang.Key.ToLower();
                    }
                }
                return gogLang;
            }
        }

        public string Nickname => pInfo.Nickname;

        public string LocalIP =>
                //string localIP;
                //using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                //{
                //    socket.Connect("8.8.8.8", 65530);
                //    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                //    localIP = endPoint.Address.ToString();
                //}

                //var dadada = GetBestInterface(BitConverter.ToUInt32(IPAddress.Parse("8.8.8.8").GetAddressBytes(), 0), out uint interfaceIndex);
                //IPAddress xxxd = NetworkInterface.GetAllNetworkInterfaces()
                //                .Where(netInterface => netInterface.GetIPProperties().GetIPv4Properties().Index == BitConverter.ToInt32(BitConverter.GetBytes(interfaceIndex), 0)).First().GetIPProperties().UnicastAddresses.Where(ipAdd => ipAdd.Address.AddressFamily == AddressFamily.InterNetwork).First().Address;

                //return xxxd.ToString();

                parent.GetLocalIP();

        public string NucleusUserRoot
        {
            get
            {
                if (parent.UsingNucleusAccounts)
                {
                    return Path.Combine(Path.GetDirectoryName(NucleusEnvironmentRoot), "nucleusplayer" + (pInfo.PlayerID + 1));
                }
                else
                {
                    return NucleusEnvironmentRoot;
                }
            }
        }

        public string EnvironmentPlayer
        {
            get
            {
                if (!UserProfileConvertedToDocuments)
                {
                    return $@"{NucleusEnvironmentRoot}\NucleusCoop\{Nickname}\";
                }
                else
                {
                    return DocumentsPlayer;
                }
            }
        }

        public string EnvironmentRoot
        {
            get
            {
                if (!UserProfileConvertedToDocuments)
                {
                    return $@"{NucleusEnvironmentRoot}\NucleusCoop\";
                }
                else
                {
                    return DocumentsRoot;
                }
            }
        }

        public string DocumentsPlayer =>
                //Log($"TEMP: NucleusDocumentsRoot={NucleusDocumentsRoot}, Nuclues.Folder.Documents={Folder.Documents}, GetFolderPath={Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}");
                $@"{Path.GetDirectoryName(NucleusDocumentsRoot)}\NucleusCoop\{Nickname}\Documents\";

        public string DocumentsRoot => $@"{Path.GetDirectoryName(NucleusDocumentsRoot)}\NucleusCoop\";

        public string UserProfileConfigPath
        {
            //get
            //{
            //    //return parent.UserProfileConfigPath;
            //}
            //set { }

            get; set;
        }

        public string UserProfileSavePath
        {
            //get
            //{
            //    //return parent.UserProfileSavePath;
            //}
            //set { }

            get; set;
        }

        public string DocumentsConfigPath
        {
            get; set;
        }

        public string DocumentsSavePath
        {
            get; set;
        }

        public bool UserProfileConvertedToDocuments
        {
            get; set;
        }

        public string ScriptFolder => GameManager.Instance.GetJsScriptsPath() + "\\" + Path.GetFileNameWithoutExtension(parent.JsFilename);

        public void SetProtoInputMultipleControllers(int controller1, int controller2, int controller3, int controller4)
        {
            pInfo.ProtoController1 = controller1;
            pInfo.ProtoController2 = controller2;
            pInfo.ProtoController3 = controller3;
            pInfo.ProtoController4 = controller4;
        }

        public void HideDesktop()
        {
            if (PlayerID > 0)
            {
                return;
            }

            System.Threading.Tasks.Task.Run(() =>
            {
                IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
                string splitDivisionBackColor = ini.IniReadValue("CustomLayout", "SplitDivColor");
                Color ChoosenColor = Color.Black;

                IDictionary<string, Color> splitColors = new Dictionary<string, Color>();

                splitColors.Add("Black", Color.Black);
                splitColors.Add("Gray", Color.DimGray);
                splitColors.Add("White", Color.White);
                splitColors.Add("Dark Blue", Color.DarkBlue);
                splitColors.Add("Blue", Color.Blue);
                splitColors.Add("Purple", Color.Purple);
                splitColors.Add("Pink", Color.Pink);
                splitColors.Add("Red", Color.Red);
                splitColors.Add("Orange", Color.Orange);
                splitColors.Add("Yellow", Color.Yellow);
                splitColors.Add("Green", Color.Green);

                foreach (KeyValuePair<string, Color> color in splitColors)
                {
                    if (color.Key == splitDivisionBackColor)
                    {
                        ChoosenColor = color.Value;
                    }
                }

                foreach (Screen screen in Screen.AllScreens)
                {
                    Form backgroundForm = new Form
                    {
                        Name = "SplitForm",
                        Text = "SplitForm",
                        BackColor = ChoosenColor,
                        Location = new Point(0, 0),
                        Width = screen.WorkingArea.Size.Width,
                        Height = screen.WorkingArea.Size.Height + 50,
                        FormBorderStyle = FormBorderStyle.None,
                        StartPosition = FormStartPosition.Manual
                    };

                    backgroundForm.ShowDialog();//Show() make it crash
                }
            });
        }

        public void BackupFile(string filePath, bool overwrite)
        {
            if (pInfo.PlayerID == 0)
            {
                if (File.Exists(filePath))
                {
                    Log($"Backing up {filePath}");
                    string backupFileName = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_NUCLEUS_BACKUP" + Path.GetExtension(filePath);
                    File.Copy(filePath, backupFileName, overwrite);
                    //BackupAndRestoreFiles.Add(backupFileName);
                    parent.userBackedFiles.Add(backupFileName);
                }
                else
                {
                    Log($"Unable to backup {filePath}, file does not exist");
                }
            }
        }

        public void CopyScriptFolder(string DestinationPath)
        {
            string SourcePath = ScriptFolder;
            try
            {
                Directory.CreateDirectory(DestinationPath);

                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                    SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
                }

                foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                    SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
                }
            }
            catch
            {
            }
        }

        public int RandomInt(int min, int max)
        {
            Random _random = new Random();
            return _random.Next(min, max);
        }

        public string RandomString(int size, bool lowerCase = false)
        {
            Random _random = new Random();
            StringBuilder builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (int i = 0; i < size; i++)
            {
                char @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        public int ConvertToInt(string num)
        {
            return Convert.ToInt32(num);
        }

        public int ConvertToInt(object num)
        {
            return Convert.ToInt32(num);
        }

        public string ConvertToString(object str)
        {
            return str.ToString();
        }

        public byte[] ConvertToBytes(float num)
        {
            return BitConverter.GetBytes(num);
        }
       
        public byte[] ConvertToBytes(int num)
        {
            return BitConverter.GetBytes(num);
        }

        public byte[] ConvertToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public void RunAdditionalFiles(string[] filePaths, bool changeWorkingDir, int secondsToPauseInbetween)
        {
            for (int fileIndex = 0; fileIndex < filePaths.Length; fileIndex++)
            {
                string fileName = filePaths[fileIndex];
                if (fileName.Contains('|'))
                {
                    string[] fileNameSplit = fileName.Split('|');
                    fileName = fileNameSplit[1];

                    if (fileNameSplit[0].ToLower() != "all")
                    {
                        if (int.Parse(fileNameSplit[0]) != (pInfo.PlayerID + 1))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (pInfo.PlayerID > 0)
                    {
                        continue;
                    }
                }
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false
                };
                if (changeWorkingDir)
                {
                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                }
                else
                {
                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                }

                Process.Start(psi);

                if (fileIndex < (filePaths.Length - 1) && secondsToPauseInbetween > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(secondsToPauseInbetween));
                }
            }
        }

       
        public void RunAdditionalFiles(string[] filePaths, bool changeWorkingDir, int secondsToPauseInbetween, bool runAsAdmin, bool promptBetween)
        {
            for (int fileIndex = 0; fileIndex < filePaths.Length; fileIndex++)
            {
                string fileName = filePaths[fileIndex];
                if (fileName.Contains('|'))
                {
                    string[] fileNameSplit = fileName.Split('|');
                    fileName = fileNameSplit[1];

                    if (fileNameSplit[0].ToLower() != "all")
                    {
                        if (int.Parse(fileNameSplit[0]) != (pInfo.PlayerID + 1))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (pInfo.PlayerID > 0)
                    {
                        continue;
                    }
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false
                };
                if (changeWorkingDir)
                {
                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                }
                else
                {
                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                }
                if (runAsAdmin)
                {
                    psi.UseShellExecute = true;
                    psi.Verb = "runas";
                }

                Process.Start(psi);

                if (promptBetween)
                {
                    if (fileIndex < (filePaths.Length - 1))
                    {
                        Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to launch " + filePaths[fileIndex]);
                        prompt.ShowDialog();
                    }
                    else
                    {
                        Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to proceed with launching the game instance " + filePaths[fileIndex]);
                        prompt.ShowDialog();
                    }

                    if (fileIndex < (filePaths.Length - 1) && secondsToPauseInbetween > 0)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(secondsToPauseInbetween));
                    }
                }
            }
        }

        public void RunAdditionalFiles(string[] filePaths, bool changeWorkingDir, string customText, int secondsToPauseInbetween, bool showFilePath, bool runAsAdmin, bool promptBetween, bool confirm)
        {
            for (int fileIndex = 0; fileIndex < filePaths.Length; fileIndex++)
            {
                string fileName = filePaths[fileIndex];
                if (fileName.Contains('|'))
                {
                    string[] fileNameSplit = fileName.Split('|');
                    fileName = fileNameSplit[1];

                    if (fileNameSplit[0].ToLower() != "all")
                    {
                        if (int.Parse(fileNameSplit[0]) != (pInfo.PlayerID + 1))
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    if (pInfo.PlayerID > 0)
                    {
                        continue;
                    }
                }

                if (!confirm)//no need to press ok
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = false
                    };

                    if (changeWorkingDir)
                    {
                        psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                    }
                    else
                    {
                        psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                    }

                    if (runAsAdmin)
                    {
                        psi.UseShellExecute = true;
                        psi.Verb = "runas";
                    }

                    Process.Start(psi);

                    if (promptBetween)
                    {
                        if (fileIndex < (filePaths.Length - 1))
                        {
                            if (showFilePath)
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText + filePaths[fileIndex]);
                                prompt.ShowDialog();
                            }
                            else
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText);
                                prompt.ShowDialog();
                            }
                        }
                        else
                        {
                            if (showFilePath)
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText + filePaths[fileIndex]);
                                prompt.ShowDialog();
                            }
                            else
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText);
                                prompt.ShowDialog();
                            }
                        }

                        if (fileIndex < (filePaths.Length - 1) && secondsToPauseInbetween > 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(secondsToPauseInbetween));
                        }
                    }
                }
                else//need to press ok
                {
                    if (promptBetween)
                    {
                        if (fileIndex < (filePaths.Length - 1))
                        {
                            if (showFilePath)
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText + filePaths[fileIndex]);
                                prompt.ShowDialog();

                                ProcessStartInfo psi = new ProcessStartInfo
                                {
                                    FileName = fileName,
                                    UseShellExecute = false
                                };

                                if (changeWorkingDir)
                                {
                                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                                }
                                else
                                {
                                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                                }

                                if (runAsAdmin)
                                {
                                    psi.UseShellExecute = true;
                                    psi.Verb = "runas";
                                }

                                Process.Start(psi);
                            }
                            else
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText);
                                prompt.ShowDialog();

                                ProcessStartInfo psi = new ProcessStartInfo
                                {
                                    FileName = fileName,
                                    UseShellExecute = false
                                };

                                if (changeWorkingDir)
                                {
                                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                                }
                                else
                                {
                                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                                }

                                if (runAsAdmin)
                                {
                                    psi.UseShellExecute = true;
                                    psi.Verb = "runas";
                                }

                                Process.Start(psi);
                            }
                        }
                        else
                        {
                            if (showFilePath)
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText + filePaths[fileIndex]);
                                prompt.ShowDialog();

                                ProcessStartInfo psi = new ProcessStartInfo
                                {
                                    FileName = fileName,
                                    UseShellExecute = false
                                };

                                if (changeWorkingDir)
                                {
                                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                                }
                                else
                                {
                                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                                }

                                if (runAsAdmin)
                                {
                                    psi.UseShellExecute = true;
                                    psi.Verb = "runas";
                                }

                                Process.Start(psi);
                            }
                            else
                            {
                                Forms.Prompt prompt = new Forms.Prompt(customText);
                                prompt.ShowDialog();

                                ProcessStartInfo psi = new ProcessStartInfo
                                {
                                    FileName = fileName,
                                    UseShellExecute = false
                                };

                                if (changeWorkingDir)
                                {
                                    psi.WorkingDirectory = GameManager.Instance.GetAppContentPath() + "\\AdditionalFiles";
                                }
                                else
                                {
                                    psi.WorkingDirectory = Path.GetDirectoryName(fileName);
                                }
                                
                                if (runAsAdmin)
                                {
                                    psi.UseShellExecute = true;
                                    psi.Verb = "runas";
                                }

                                Process.Start(psi);
                            }
                        }

                        if (fileIndex < (filePaths.Length - 1) && secondsToPauseInbetween > 0)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(secondsToPauseInbetween));
                        }
                    }


                }
            }
        }
        private static int GCD(int a, int b)
        {
            return b == 0 ? Math.Abs(a) : GCD(b, a % b);
        }

        public void ModifiedDate(string file, int year, int month, int day, int hour, int minute, int second)
        {
            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            File.SetLastWriteTime(file, dt);
        }

        public void ModifiedDate(string file, int year, int month, int day)
        {
            DateTime dt = new DateTime(year, month, day, 0, 0, 0);
            File.SetLastWriteTime(file, dt);
        }

        public void CreatedDate(string file, int year, int month, int day, int hour, int minute, int second)
        {
            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            File.SetCreationTime(file, dt);
        }

        public void CreatedDate(string file, int year, int month, int day)
        {
            DateTime dt = new DateTime(year, month, day, 0, 0, 0);
            File.SetCreationTime(file, dt);
        }

        public string[] FindFiles(string rootFolder, string fileName)
        {
            string[] files = Directory.GetFiles(rootFolder, fileName, SearchOption.TopDirectoryOnly);
            return files;
        }

        public string[] FindFiles(string rootFolder, string fileName, bool searchAll)
        {
            SearchOption searchOp = searchAll ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(rootFolder, fileName, searchOp);
            return files;
        }

        public void WriteTextFile(string path, string[] lines)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllLines(path, lines);
        }

        public int FindLineNumberInTextFile(string path, string searchValue, SearchType type)
        {
            string[] lines = File.ReadAllLines(path);
            if (!File.Exists(path))
            {
                return -1;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                switch (type)
                {
                    case SearchType.Contains:
                        {
                            if (lines[i].Contains(searchValue))
                            {
                                return i + 1;
                            }
                        }
                        break;
                    case SearchType.Full:
                        {
                            if (string.Equals(lines[i], searchValue))
                            {
                                return i + 1;
                            }
                        }
                        break;
                    case SearchType.StartsWith:
                        {
                            if (lines[i].StartsWith(searchValue))
                            {
                                return i + 1;
                            }
                        }
                        break;
                }
            }

            return -1;
        }

        public void RemoveLineInTextFile(string path, int lineNum)
        {
            string[] lines = File.ReadAllLines(path);
            if (lineNum < 0 || lineNum > lines.Length)
            {
                return;
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            lines = lines.Where(w => w != lines[lineNum - 1]).ToArray();

            File.WriteAllLines(path, lines);
        }

        public void RemoveLineInTextFile(string path, int lineNum, string encoder)
        {
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            if (lineNum < 0 || lineNum > lines.Length)
            {
                return;
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            lines = lines.Where(w => w != lines[lineNum - 1]).ToArray();

            File.WriteAllLines(path, lines, Encoding.GetEncoding(encoder));
        }

        public void RemoveLineInTextFile(string path, string searchValue, SearchType type)
        {
            string[] lines = File.ReadAllLines(path);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                switch (type)
                {
                    case SearchType.Contains:
                        {
                            if (lines[i].Contains(searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                    case SearchType.Full:
                        {
                            if (string.Equals(lines[i], searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                    case SearchType.StartsWith:
                        {
                            if (lines[i].StartsWith(searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                }
                //if (lines[i].Contains(searchValue))
                //{
                //    lines.Where(w => w != lines[i]).ToArray();
                //}
            }
            File.WriteAllLines(path, lines);
        }

        public void RemoveLineInTextFile(string path, string searchValue, SearchType type, string encoder)
        {
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                switch (type)
                {
                    case SearchType.Contains:
                        {
                            if (lines[i].Contains(searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                    case SearchType.Full:
                        {
                            if (string.Equals(lines[i], searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                    case SearchType.StartsWith:
                        {
                            if (lines[i].StartsWith(searchValue))
                            {
                                lines = lines.Where(w => w != lines[i]).ToArray();
                            }
                        }
                        break;
                }
                //if (lines[i].Contains(searchValue))
                //{
                //    lines.Where(w => w != lines[i]).ToArray();
                //}
            }
            File.WriteAllLines(path, lines, Encoding.GetEncoding(encoder));
        }

        public void ReplaceLinesInTextFile(string path, string[] newLines)
        {
            string[] lines = File.ReadAllLines(path);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            foreach (string line in newLines)
            {
                string[] splitString = line.Split('|');
                string lineNum = splitString[0];
                string newValue = splitString[1];

                if (int.Parse(lineNum) > 0 && int.Parse(lineNum) <= lines.Length)
                {
                    lines[int.Parse(lineNum) - 1] = newValue;
                }
            }

            File.WriteAllLines(path, lines);
        }

        public void ReplaceLinesInTextFile(string path, string[] newLines, string encoder)
        {
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            foreach (string line in newLines)
            {
                string[] splitString = line.Split('|');
                string lineNum = splitString[0];
                string newValue = splitString[1];
                if (int.Parse(lineNum) > 0 && int.Parse(lineNum) <= lines.Length)
                {
                    lines[int.Parse(lineNum) - 1] = newValue;
                }
            }

            File.WriteAllLines(path, lines, Encoding.GetEncoding(encoder));
        }

        public void ReplacePartialLinesInTextFile(string path, string[] newLines)
        {
            string[] lines = File.ReadAllLines(path);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            foreach (string item in newLines)
            {
                string[] data = item.Split('|');
                string lineNum = data[0];
                string regexPtrn = data[1];
                string newValue = data[2];

                if (int.Parse(lineNum) > 0 && int.Parse(lineNum) <= lines.Length)
                {
                    lines[int.Parse(lineNum) - 1] = Regex.Replace(lines[int.Parse(lineNum) - 1], regexPtrn, newValue);
                }
            }

            File.WriteAllLines(path, lines);
        }

        public void ReplacePartialLinesInTextFile(string path, string[] newLines, string encoder)
        {
            string[] lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                return;
            }

            foreach (string item in newLines)
            {
                string[] data = item.Split('|');
                string lineNum = data[0];
                string regexPtrn = data[1];
                string newValue = data[2];

                if (int.Parse(lineNum) > 0 && int.Parse(lineNum) <= lines.Length)
                {
                    lines[int.Parse(lineNum) - 1] = Regex.Replace(lines[int.Parse(lineNum) - 1], regexPtrn, newValue);
                }
            }

            File.WriteAllLines(path, lines, Encoding.GetEncoding(encoder));
        }

        public SaveInfo NewSaveInfo(string section, string key, string value)
        {
            return new SaveInfo(section, key, value);
        }

        public void ModifySaveFile(string installSavePath, string saveFullPath, SaveType type, params SaveInfo[] info)
        {
            // this needs to be dynamic someday
            switch (type)
            {
                case SaveType.CFG:
                    {
                        SourceCfgFile cfg = new SourceCfgFile(installSavePath);
                        for (int j = 0; j < info.Length; j++)
                        {
                            SaveInfo save = info[j];
                            if (save is CfgSaveInfo)
                            {
                                CfgSaveInfo option = (CfgSaveInfo)save;
                                cfg.ChangeProperty(option.Section, option.Key, option.Value);
                            }
                        }
                        cfg.Save(saveFullPath);
                    }
                    break;
                case SaveType.INI:
                    {
                        if (!installSavePath.Equals(saveFullPath))
                        {
                            File.Copy(installSavePath, saveFullPath);
                        }
                        IniFile file = new IniFile(saveFullPath);
                        for (int j = 0; j < info.Length; j++)
                        {
                            SaveInfo save = info[j];
                            if (save is IniSaveInfo)
                            {
                                IniSaveInfo ini = (IniSaveInfo)save;
                                file.IniWriteValue(ini.Section, ini.Key, ini.Value);
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void HexEdit(string fileToEdit, string address, byte[] newBytes)
        {
            using (Stream stream = File.Open(fileToEdit, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = long.Parse(address, NumberStyles.HexNumber);
                stream.Write(newBytes, 0, newBytes.Length);
            }
        }

      

        public void PatchFileFindAll(string originalFile, string patchedFile, byte[] patchFind, byte[] patchReplace)
        {
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

            //int patchCount = 0;
            // Detect and patch file.
            for (int p = 0; p < fileContent.Length; p++)
            {
                if (p + patchFind.Length > fileContent.Length)
                {
                    continue;
                }

                bool toContinue = false;
                for (int i = 0; i < patchFind.Length; i++)
                {
                    if (patchFind[i] != fileContent[p + i])
                    {
                        toContinue = true;
                        break;
                    }
                }
                if (toContinue)
                {
                    continue;
                }

                //patchCount++;
                //if (patchCount > 1)
                //{
                //    LogManager.Log("PatchFind pattern is not unique in " + originalFile);
                //}
                //else
                //{
                for (int w = 0; w < patchReplace.Length; w++)
                {
                    fileContent[p + w] = patchReplace[w];
                }
                //}
            }

            //if (patchCount == 0)
            //{
            //    LogManager.Log("PatchFind pattern was not found in " + originalFile);
            //}

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);
        }

        public void PatchFileFindPattern(string originalFile, string patchedFile, string origPattern, string patchPattern, bool patchall) // 2.1.2
        {
            Log("Patching file");
            //Format original hex pattern
            string formatedOrigPattern = origPattern.Replace(" ", "");

            byte[] origBytePattern = new byte[formatedOrigPattern.Length / 2];

            for (int i = 0, h = 0; h < formatedOrigPattern.Length; i++, h += 2)
            {
                origBytePattern[i] = (byte)Int32.Parse(formatedOrigPattern.Substring(h, 2), System.Globalization.NumberStyles.HexNumber);
            }

            //Format patch hex pattern
            string formatedPatchPattern = patchPattern.Replace(" ", "");

            byte[] patchBytePattern = new byte[formatedPatchPattern.Length / 2];

            for (int i = 0, h = 0; h < formatedPatchPattern.Length; i++, h += 2)
            {
                patchBytePattern[i] = (byte)Int32.Parse(formatedPatchPattern.Substring(h, 2), System.Globalization.NumberStyles.HexNumber);
            }
            
            string checkpatchBytePattern = BitConverter.ToString(patchBytePattern).Replace("-", " ");

            byte[] fileContent = File.ReadAllBytes(originalFile);

            for (int p = 0; p < fileContent.Length; p++)
            {

                if (p + origBytePattern.Length > fileContent.Length)
                {
                    continue;
                }

                bool toContinue = false;
                for (int i = 0; i < origBytePattern.Length; i++)
                {
                    if (origBytePattern[i] != fileContent[p + i])
                    {
                        toContinue = true;
                        break;
                    }
                }

                if (toContinue)
                {
                    continue;
                }

                for (int i = 0; i < origBytePattern.Length; i++)
                {
                    if (fileContent[p + i] == origBytePattern[i])
                    {
                        fileContent[p + i] = patchBytePattern[i];

                        if (!patchall)
                        {
                            break;
                        }
                    }
                }

            }

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);
            Log("Patching finished");
        }

       public void PatchFileFindPattern(string originalFile, string patchedFile, string origPattern, int insert, int offset, bool patchall) // 2.1.2
        {
            Log("Patching file");
            //Format original hex pattern
            string formatedOrigPattern = origPattern.Replace(" ", "");

            byte[] origBytePattern = new byte[formatedOrigPattern.Length / 2];

            for (int i = 0, h = 0; h < formatedOrigPattern.Length; i++, h += 2)
            {
                origBytePattern[i] = (byte)Int32.Parse(formatedOrigPattern.Substring(h, 2), System.Globalization.NumberStyles.HexNumber);
            }

            //Format patch hex pattern
            string formatedPatchPattern = origPattern.Replace(" ", "");

            byte[] patchBytePattern = new byte[formatedPatchPattern.Length / 2];

            for (int i = 0, h = 0; h < formatedPatchPattern.Length; i++, h += 2)
            {
                patchBytePattern[i] = (byte)Int32.Parse(formatedPatchPattern.Substring(h, 2), System.Globalization.NumberStyles.HexNumber);
            }

            byte[] insertdef = BitConverter.GetBytes(insert);
            byte[] formatedInsert = new byte[] {insertdef[0], insertdef[1]};
            
            string insertStr = BitConverter.ToString(formatedInsert).Replace("-", " ");

            int _offset = offset - 1;

            for (int i = 0; i < formatedInsert.Length; i++)
            {
                patchBytePattern[_offset] = formatedInsert[i];
                _offset++;
            }

            string checkpatchBytePattern = BitConverter.ToString(patchBytePattern).Replace("-", " ");
           
            byte[] fileContent = File.ReadAllBytes(originalFile);

            for (int p = 0; p < fileContent.Length; p++)
            {

                if (p + origBytePattern.Length > fileContent.Length)
                {
                    continue;
                }

                bool toContinue = false;
                for (int i = 0; i < origBytePattern.Length; i++)
                {
                    if (origBytePattern[i] != fileContent[p + i])
                    {
                        toContinue = true;
                        break;
                    }
                }

                if (toContinue)
                {
                    continue;
                }

                bool patchFirst = false;
                for (int i = 0; i < origBytePattern.Length; i++)
                {
                    if (fileContent[p + i] == origBytePattern[i])
                    {
                        fileContent[p + i] = patchBytePattern[i];

                        if (!patchall && i == origBytePattern.Length - 1)
                        {
                            patchFirst = true;
                            break;
                        }
                    }
                }

                if (patchFirst)
                {
                    break;
                }

            }

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);
            Log("Patching finished");
        }


        public void PatchFile(string originalFile, string patchedFile, byte[] patchFind, byte[] patchReplace)
        {
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

            int patchCount = 0;
            // Detect and patch file.
            for (int p = 0; p < fileContent.Length; p++)
            {
                if (p + patchFind.Length > fileContent.Length)
                {
                    continue;
                }

                bool toContinue = false;
                for (int i = 0; i < patchFind.Length; i++)
                {
                    if (patchFind[i] != fileContent[p + i])
                    {
                        toContinue = true;
                        break;
                    }
                }
                if (toContinue)
                {
                    continue;
                }

                patchCount++;
                if (patchCount > 1)
                {
                    LogManager.Log("PatchFind pattern is not unique in " + originalFile);
                }
                else
                {
                    for (int w = 0; w < patchReplace.Length; w++)
                    {
                        fileContent[p + w] = patchReplace[w];
                    }
                }
            }

            if (patchCount == 0)
            {
                LogManager.Log("PatchFind pattern was not found in " + originalFile);
            }

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);
        }

        public void PatchFile(string originalFile, string patchedFile, string spatchFind, string spatchReplace)
        {
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

            byte[] patchFind = Encoding.ASCII.GetBytes(spatchFind);
            byte[] patchReplace = Encoding.ASCII.GetBytes(spatchReplace);

            int patchCount = 0;
            // Detect and patch file.
            for (int p = 0; p < fileContent.Length; p++)
            {
                if (p + patchFind.Length > fileContent.Length)
                {
                    continue;
                }

                bool toContinue = false;
                for (int i = 0; i < patchFind.Length; i++)
                {
                    if (patchFind[i] != fileContent[p + i])
                    {
                        toContinue = true;
                        break;
                    }
                }
                if (toContinue)
                {
                    continue;
                }

                patchCount++;
                if (patchCount > 1)
                {
                    LogManager.Log("PatchFind pattern is not unique in " + originalFile);
                }
                else
                {
                    for (int w = 0; w < patchReplace.Length; w++)
                    {
                        fileContent[p + w] = patchReplace[w];
                    }
                }
            }

            if (patchCount == 0)
            {
                LogManager.Log("PatchFind pattern was not found in " + originalFile);
            }

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);
        }

        //XPath syntax
        //https://www.w3schools.com/xml/xpath_syntax.asp
        public void ChangeXmlAttributeValue(string path, string xpath, string attributeName, string attributeValue)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                node.Attributes[attributeName].Value = attributeValue;
            }
            doc.Save(path);
        }

        public void ChangeXmlNodeValue(string path, string xpath, string nodeValue)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                node.Value = nodeValue;
            }
            doc.Save(path);
        }

        public void ChangeXmlNodeInnerTextValue(string path, string xpath, string innerTextValue)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                node.InnerText = innerTextValue;
            }
            doc.Save(path);
        }

        public void CreateRegKey(string baseKey, string sKey, string subKey)
        {
            if (baseKey == "HKEY_LOCAL_MACHINE" || baseKey == "HKEY_CURRENT_USER")
            {
                if (baseKey == "HKEY_LOCAL_MACHINE")
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(sKey, true);
                    key.CreateSubKey(subKey);
                    key.Close();
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(sKey, true);
                    key.CreateSubKey(subKey);
                    key.Close();
                }
            }
        }

        private void ExportRegistry(string strKey, string filepath)
        {
            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "reg.exe";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.Arguments = "export \"" + strKey + "\" \"" + filepath + "\" /y";
                    Log("Export Registry command: " + proc.StartInfo.Arguments);
                    proc.Start();
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("ERROR: Unable to export {0}. {1}", Path.GetFileName(filepath), ex.Message));
            }
        }

        public void DeleteRegKey(string baseKey, string sKey, string subKey)
        {
            if (baseKey != "HKEY_LOCAL_MACHINE" && baseKey != "HKEY_CURRENT_USER" && baseKey != "HKEY_USERS")
            {
                return;
            }

            RegistryKey key = null;
            switch (baseKey)
            {
                case "HKEY_LOCAL_MACHINE":
                    key = Registry.LocalMachine.OpenSubKey(sKey, true);
                    break;
                case "HKEY_CURRENT_USER":
                    key = Registry.CurrentUser.OpenSubKey(sKey, true);
                    break;
                case "HKEY_USERS":
                    key = Registry.Users.OpenSubKey(sKey, true);
                    break;
            }

            string fullKeyPath = baseKey + "\\" + sKey;
            if (!regKeyPaths.Contains(fullKeyPath) && key != null)
            {
                string regPath = Directory.GetCurrentDirectory() + "\\utils\\backup\\" + sKey.Substring(sKey.LastIndexOf('\\') + 1) + ".reg";
                if (!File.Exists(regPath))
                {
                    Log(string.Format("{0} not found in backups, exporting registry now", sKey.Substring(sKey.LastIndexOf('\\') + 1) + ".reg"));
                    regKeyPaths.Add(fullKeyPath);
                    ExportRegistry(baseKey + "\\" + sKey, regPath);
                }
            }

            key.DeleteSubKey(subKey);
            key.Close();
        }

        public string ReadRegKey(string baseKey, string sKey, string subKey)
        {
            if (baseKey != "HKEY_LOCAL_MACHINE" && baseKey != "HKEY_CURRENT_USER" && baseKey != "HKEY_USERS")
            {
                return string.Empty;
            }

            RegistryKey key = null;
            switch (baseKey)
            {
                case "HKEY_LOCAL_MACHINE":
                    key = Registry.LocalMachine.OpenSubKey(sKey);
                    break;
                case "HKEY_CURRENT_USER":
                    key = Registry.CurrentUser.OpenSubKey(sKey);
                    break;
                case "HKEY_USERS":
                    key = Registry.Users.OpenSubKey(sKey);
                    break;
            }

            if (key != null)
            {
                return key.GetValue(subKey).ToString();
            }
            else
            {
                return null;
            }
        }

        public void EditRegKey(string baseKey, string sKey, string subKey, object value, RegType regType)
        {

            //Binary = 3,
            //DWord = 4,
            //ExpandString = 2,
            //MultiString = 7,
            //None = -1,
            //QWord = 11,
            //String = 1,
            //Unknown = 0

            if ((baseKey != "HKEY_LOCAL_MACHINE" && baseKey != "HKEY_CURRENT_USER" && baseKey != "HKEY_USERS") || value == null)
            {
                return;
            }

            string val = value.ToString();

            RegistryKey key = null;
            switch (baseKey)
            {
                case "HKEY_LOCAL_MACHINE":
                    key = Registry.LocalMachine.OpenSubKey(sKey, true);
                    break;
                case "HKEY_CURRENT_USER":
                    key = Registry.CurrentUser.OpenSubKey(sKey, true);
                    break;
                case "HKEY_USERS":
                    key = Registry.Users.OpenSubKey(sKey, true);
                    break;
            }

            
            string fullKeyPath = baseKey + "\\" + sKey;

            if (!regKeyPaths.Contains(fullKeyPath) && key != null)
            {
                string regPath = Directory.GetCurrentDirectory() + "\\utils\\backup\\" + sKey.Substring(sKey.LastIndexOf('\\') + 1) + ".reg";
                if (!File.Exists(regPath))
                {
                    Log(string.Format("{0} not found in backups, exporting registry now", sKey.Substring(sKey.LastIndexOf('\\') + 1) + ".reg"));
                    regKeyPaths.Add(fullKeyPath);
                    ExportRegistry(baseKey + "\\" + sKey, regPath);
                }
            }
           

            if (key == null)
            {
                switch (baseKey)
                {
                    case "HKEY_LOCAL_MACHINE":
                        key = Registry.LocalMachine.CreateSubKey(sKey, true);
                        break;
                    case "HKEY_CURRENT_USER":
                        key = Registry.CurrentUser.CreateSubKey(sKey, true);
                        break;
                    case "HKEY_USERS":
                        key = Registry.Users.CreateSubKey(sKey, true);
                        break;
                }
            }

            if (regType == RegType.Binary)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(val);
                key.SetValue(subKey, bytes, (RegistryValueKind)(int)regType);
            }
            else
            {
                key.SetValue(subKey, value, (RegistryValueKind)(int)regType);
            }
            key.Close();
        }

        public void EditRegKeyNoBackup(string baseKey, string sKey, string subKey, object value, RegType regType)
        {
            if ((baseKey != "HKEY_LOCAL_MACHINE" && baseKey != "HKEY_CURRENT_USER" && baseKey != "HKEY_USERS") || value == null)
            {
                return;
            }

            string val = value.ToString();

            RegistryKey key = null;
            switch (baseKey)
            {
                case "HKEY_LOCAL_MACHINE":
                    key = Registry.LocalMachine.OpenSubKey(sKey, true);
                    break;
                case "HKEY_CURRENT_USER":
                    key = Registry.CurrentUser.OpenSubKey(sKey, true);
                    break;
                case "HKEY_USERS":
                    key = Registry.Users.OpenSubKey(sKey, true);
                    break;
            }
  
            Log("Registry key : " + baseKey + "\\" + sKey + "\\" + subKey + " -Value= " + value + " -RegType= " + regType + " will not be deleted from registry  on Nucleus Co-op close");
          
            if (key == null)
            {
                switch (baseKey)
                {
                    case "HKEY_LOCAL_MACHINE":
                        key = Registry.LocalMachine.CreateSubKey(sKey, true);
                        break;
                    case "HKEY_CURRENT_USER":
                        key = Registry.CurrentUser.CreateSubKey(sKey, true);
                        break;
                    case "HKEY_USERS":
                        key = Registry.Users.CreateSubKey(sKey, true);
                        break;
                }
            }

            if (regType == RegType.Binary)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(val);
                key.SetValue(subKey, bytes, (RegistryValueKind)(int)regType);
            }
            else
            {
                key.SetValue(subKey, value, (RegistryValueKind)(int)regType);
            }

            key.Close();
        }

        public void KillProcessesMatchingWindowName(string name)
        {
            foreach (Process p in System.Diagnostics.Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains(name.ToLower())).ToArray())
            {
                p.Kill();
            }
        }

        public void Wait(int wait)
        {
            Log(string.Format("Pausing for "+ (double)wait/(double)1000 +" seconds"));
            Thread.Sleep(wait);
        }

        public void KillProcessesMatchingProcessName(string name)
        {
            foreach (Process p in System.Diagnostics.Process.GetProcesses().Where(x => x.ProcessName.ToLower().Contains(name.ToLower())).ToArray())
            {
                p.Kill();
            }
        }

        public void MoveFolder(string sourceDirName, string destDirName)
        {
            string source = Folder.InstancedGameFolder.ToString() + "\\" + sourceDirName;
            string dest = Folder.InstancedGameFolder.ToString() + "\\" + destDirName;

            if (!Directory.Exists(dest))
            {
                Directory.Move(source, dest);
            }
            else
            {
                foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(dest, dir.Substring(source.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(dest, file_name.Substring(source.Length + 1)));
                    }
                    catch
                    {
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(dest, file_name.Substring(source.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(dest, file_name.Substring(source.Length + 1)) + "\"", false);
                    }
                }
            }
        }
    }
}
