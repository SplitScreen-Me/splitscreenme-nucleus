using Ionic.Zip;
using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Tools.NemirtingasEpicEmu;
using Nucleus.Gaming.Tools.NemirtingasGalaxyEmu;
using Nucleus.Gaming.Tools.Network;
using Nucleus.Gaming.Tools.Steam;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        public string PlayerSteamID => pInfo.SteamID.ToString();
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
        /// <summary
        /// "DPIHandling" is deprecated since v2.3.3, keep it for old handlers using it but has no effect.
        /// </summary>
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
        public bool bHasKeyboardPlayer;
        public bool KeepAspectRatio;
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
        public string[] BackupFiles;
        public string[] BackupFolders;

        private List<string> regKeyPaths = new List<string>();

        public string NucleusEnvironmentRoot = Globals.UserEnvironmentRoot;

        public string NucleusDocumentsRoot = Globals.UserDocumentsRoot;

        public string UtilFolder = Globals.NucleusInstallRoot + "\\utils";

        public string PlayerGameFilesBackupFolder => $@"{Globals.UserEnvironmentRoot}\NucleusCoop\_Game Files Backup_\{HandlerGUID}\{Nickname}";

        public Type HandlerType => typeof(GenericGameHandler);

        public Dictionary<string, object> Options => profile.Options;

        public string Arch => GenericGameHandler.Instance.garch;

        public int Width => pInfo.MonitorBounds.Width;
        public int Height => pInfo.MonitorBounds.Height;

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

        public string NucleusFolder => Globals.NucleusInstallRoot;

        public int NumberOfPlayers => parent.numPlayers;

        public int ProcessID => pInfo.ProcessID;

        public bool HasKeyboardPlayer => bHasKeyboardPlayer;

        public string GetFolder(Folder folder)
        {
            return FileUtil.GetFolder(parent, folder);
        }

        public string x360ceGamepadGuid => "IG_" + pInfo.GamepadGuid.ToString().Replace("-", string.Empty);

        public string GamepadGuid => pInfo.GamepadGuid.ToString();
        public string RawHID => pInfo.RawHID.ToString();

        public bool IsKeyboardPlayer => pInfo.IsKeyboardPlayer;

        public int GamepadId => pInfo.GamepadId + 1;

        public float OrigAspectRatioDecimal => (float)profile.Screens[pInfo.PlayerID].MonitorBounds.Width / profile.Screens[pInfo.PlayerID].MonitorBounds.Height;

        public string OrigAspectRatio
        {
            get
            {
                int width = profile.Screens[pInfo.PlayerID].MonitorBounds.Width;
                int height = profile.Screens[pInfo.PlayerID].MonitorBounds.Height;
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

        public bool StartProcess(string path, string args, bool asAdmin)
        {
            if (File.Exists(path))
            {
                ProcessStartInfo sc = new ProcessStartInfo(path);
                sc.UseShellExecute = true;
                sc.WorkingDirectory = Path.GetDirectoryName(path);
                sc.ErrorDialog = true;
                sc.Arguments = args;

                if (asAdmin)
                {
                    sc.Verb = "runas";
                }

                Process.Start(sc);
            }

            return true;
        }

        public bool ProceedSymlink()
        {
            string[] filesToSymlink = SymlinkFiles;
            for (int f = 0; f < filesToSymlink.Length; f++)
            {
                string s = filesToSymlink[f].ToLower();
                // make sure it's lower case
                CmdUtil.MkLinkFile(Path.Combine(OrigRootFolder, s), Path.Combine(RootFolder, s), out int exitCode);
            }

            return true;
        }

        public string SearchSubFolder(string parentDirectory, string searchPattern)
        {
            if (!Directory.Exists(parentDirectory) || searchPattern == null || searchPattern == "")
            {
                return null;
            }

            string[] subFolders = Directory.GetDirectories(parentDirectory);
            string searchPatternResult = subFolders.Where(sub => sub.Contains(searchPattern)).FirstOrDefault();

            if (searchPatternResult != null)
            {
                return searchPatternResult;
            }

            return null;
        }

        public string[] SearchSubFolders(string parentDirectory, string searchPattern)
        {
            if (!Directory.Exists(parentDirectory) || searchPattern == null || searchPattern == "")
            {
                return null;
            }

            string[] subFolders = Directory.GetDirectories(parentDirectory);
            string[] searchPatternResults = subFolders.Where(sub => sub.Contains(searchPattern)).ToArray();      

            return searchPatternResults;
        }

        public string EpicLang => NemirtingasEpicEmu.GetEpicLanguage();

        public string GogLang => NemirtingasGalaxyEmu.GetGogLanguage();

        public string SteamLang => SteamFunctions.GetUserSteamLanguageChoice(parent.CurrentGameInfo);

        public string UserName => Environment.UserName.Trim();

        public string HandlersFolder => Path.Combine(GameManager.Instance.GetJsScriptsPath());

        public string Nickname => pInfo.Nickname;

        public string LocalIP => Network.GetLocalIP();

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

        public string DocumentsPlayer => $@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\{Nickname}\Documents\";

        public string DocumentsRoot => $@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\";

        public string UserProfileConfigPath
        {
            get; 
            set;
        }

        public string UserProfileSavePath
        {
            get; 
            set;
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
            if (GameProfile.UseSplitDiv || PlayerID > 0)
            {
                return;
            }

            foreach (Display dp in parent.screensInUse)
            {
                WPFDivFormThread.StartBackgroundForm(parent.CurrentGameInfo, dp);
            }
        }

        public bool CopyFolder(string source, string destination)
        {
            FileUtil.CopyDirectory(source, new DirectoryInfo(source), destination, out int exitCode, new string[0], new string[0], true);
            return true;
        }

        public bool EditZipFile(string sourceZip, string password, string savePath, string[] itemsToAdd, string[] entriesToRemove)//itemsToAdd can be file or directory(recursive). https://documentation.help/DotNetZip/7d020d58-d917-63a8-6b5c-c5519767063d.htm
        {
            bool isValidZip = ZipFile.CheckZip(sourceZip);

            if (isValidZip)
            {
                ZipFile zip = new ZipFile(sourceZip);
                zip.Password = password;

                List<ZipEntry> remove = new List<ZipEntry>();

                foreach (ZipEntry e in zip)
                {
                    string root = e.FileName.Split('/')[0];

                    if (entriesToRemove.Contains(root + "\\") || entriesToRemove.Any(fn => fn.Replace('\\', '/') == e.FileName))//skip folder || skip file
                    {
                        remove.Add(e);
                        continue;
                    }
                }

                zip.RemoveEntries(remove);

                for (int i = 0; i < itemsToAdd.Length; i++)
                {
                    zip.AddItem(itemsToAdd[i].Split('|')[1].Replace('\\', '/'), itemsToAdd[i].Split('|')[0]);
                }

                zip.Save(savePath);
                zip.Dispose();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Zip file doesn't exist,is corrupted or in an unsupported format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        public bool ExtractZip(string sourceZip, string contentDestination, string password)
        {
            bool isValidZip = ZipFile.CheckZip(sourceZip);

            if (isValidZip)
            {
                ZipFile zip = new ZipFile(sourceZip);

                zip.Password = password;

                if (!Directory.Exists(contentDestination))
                {
                    Directory.CreateDirectory(contentDestination);
                }

                zip.ExtractAll(contentDestination);
                zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                zip.Dispose();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Zip file doesn't exist,is corrupted or in an unsupported format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        public void HideDesktop(bool hideTaskbar)
        {
            if (GameProfile.UseSplitDiv || PlayerID > 0)
            {
                return;
            }

            if (hideTaskbar)
            {
                HideTaskBar();
            }

            foreach (Display dp in parent.screensInUse)
            {
                WPFDivFormThread.StartBackgroundForm(parent.CurrentGameInfo, dp);
            }
        }

        public void HideTaskBar()
        {
            if (PlayerID > 0)
            {
                return;
            }

            User32Util.HideTaskbar();
        }

        public bool BackupFile(string[] filePaths, bool overwrite)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i];

                if (File.Exists(filePath))
                {
                    Log($"Backing up {filePath}");
                    string backupFileName = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + $"_NUCLEUS_BACKUP" + Path.GetExtension(filePath);
                    File.Copy(filePath, backupFileName, overwrite);
                    parent.userBackedFiles.Add(backupFileName);
                }
                else
                {
                    Log($"Unable to backup {filePath}, file does not exist");
                }
            }

            return true;
        }

        public bool BackupFile(string filePath, bool overwrite)//backward compatibility
        {
            if (pInfo.PlayerID == 0)
            {
                if (File.Exists(filePath))
                {
                    Log($"Backing up {filePath}");
                    string backupFileName = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_NUCLEUS_BACKUP" + Path.GetExtension(filePath);
                    File.Copy(filePath, backupFileName, overwrite);
                    parent.userBackedFiles.Add(backupFileName);
                }
                else
                {
                    Log($"Unable to backup {filePath}, file does not exist");
                }
            }

            return true;
        }

        public bool CopyScriptFolder(string DestinationPath)
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
                return false;
            }

            return true;
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

        public string ToUpperCase(object str)
        {
            return str.ToString().ToUpper();
        }

        public string ToLowerCase(object str)
        {
            return str.ToString().ToLower();
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
                                Forms.Prompt prompt = new Forms.Prompt(customText + " File path: " + filePaths[fileIndex]);
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
                                Forms.Prompt prompt = new Forms.Prompt(customText + " File path: " + filePaths[fileIndex]);
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
                                Forms.Prompt prompt = new Forms.Prompt(customText + " File path: " + filePaths[fileIndex]);
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


        public string GetFileName(string fileFullPath)
        {
            //Log("File name = " + fileFullPath.Split('\\').Last());
            return fileFullPath.Split('\\').Last();//Get file name by splitting the full file path
        }

        public string[] FindFilePartialName(string rootFolder, string[] partialFileNames/*, bool firstOnly*/)
        {
            Log("Looking for files by partial name in => " + rootFolder);

            string[] files = Directory.GetFileSystemEntries(rootFolder, "*", SearchOption.AllDirectories);

            List<string> filesFound = new List<string>();

            foreach (string file in files)
            {
                foreach (string name in partialFileNames)
                {
                    if (file.Contains(name))
                    {
                        filesFound.Add(file);
                        Log("File found by partial name  => " + file);
                    }
                }
            }

            return filesFound.ToArray();
        }

        //https://stackoverflow.com/questions/11689337/net-file-writealllines-leaves-empty-line-at-the-end-of-file //v.2.3.2
        public bool WriteTextFile(string path, params string[] lines)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var stream = File.OpenWrite(path))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        writer.WriteLine(lines[i]);
                    }
                    writer.Write(lines[lines.Length - 1]);
                }
            }

            return true;
        }

        public int FindLineNumberInTextFile(string path, string searchValue, SearchType type)
        {
            string[] lines;
            //if (!File.Exists(path))
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return -1;
            //}

            lines = File.ReadAllLines(path);

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
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path);
            if (lineNum < 0 || lineNum > lines.Length)
            {
                return;
            }
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            lines = lines.Where(w => w != lines[lineNum - 1]).ToArray();

            File.WriteAllLines(path, lines);
        }

        public void RemoveLineInTextFile(string path, int lineNum, string encoder)
        {
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            if (lineNum < 0 || lineNum > lines.Length)
            {
                return;
            }

            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            lines = lines.Where(w => w != lines[lineNum - 1]).ToArray();

            File.WriteAllLines(path, lines, Encoding.GetEncoding(encoder));
        }

        public bool RemoveLineInTextFile(string path, string searchValue, SearchType type)
        {
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path);
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool RemoveLineInTextFile(string path, string searchValue, SearchType type, string encoder)
        {
            string[] lines;

            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool ReplaceLinesInTextFile(string path, string[] newLines)
        {
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path);
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool ReplaceLinesInTextFile(string path, string[] newLines, string encoder)
        {
            string[] lines;

            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool ReplacePartialLinesInTextFile(string path, string[] newLines)
        {
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path);
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool ReplacePartialLinesInTextFile(string path, string[] newLines, string encoder)
        {
            string[] lines;
            //if (File.Exists(path))
            //{
            lines = File.ReadAllLines(path, Encoding.GetEncoding(encoder));
            File.Delete(path);
            //}
            //else
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool EditTextFile(string path, string[] refLines, string[] newLines, string encoder)
        {
            //if (!File.Exists(path))
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            Log("Edit " + newLines.Length + " line(s) " + " in " + path);

            string[] fileContent = File.ReadAllLines(path, Encoding.GetEncoding(encoder));

            List<string> refLinesList = refLines.ToList();
            List<string> newLinesList = newLines.ToList();
            List<string> newFileContent = new List<string>();

            for (int index = 0; index < fileContent.Length; index++)
            {
                newFileContent.Add(fileContent[index]);

                foreach (string refLine in refLinesList)
                {
                    if (fileContent[index].Contains(refLine))
                    {
                        if (newLinesList[refLinesList.IndexOf(refLine)] != "Delete")
                        {
                            newFileContent.Remove(fileContent[index]);
                            newFileContent.Add(newLinesList[refLinesList.IndexOf(refLine)]);
                        }
                        else
                        {
                            newFileContent.Remove(fileContent[index]);
                        }
                    }
                }
            }

            File.WriteAllLines(path, newFileContent, Encoding.GetEncoding(encoder));

            return true;
        }

        //public void EditTextFile(string sourceFilePath, string destinationPath, string[] newLinesArray, string encoder)//require the file to be copied and not symlinked or to be added in the symslink exclusion list in some cases
        //{
        //    if (!File.Exists(sourceFilePath))
        //    {
        //        Log(sourceFilePath + " not Found!");
        //        MessageBox.Show($"File not found at : \n{sourceFilePath}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    Log("Creating a copy of " + sourceFilePath + " to " + destinationPath + " with " + newLinesArray.Length + " new line(s)");

        //    string[] fileContent = File.ReadAllLines(sourceFilePath, Encoding.GetEncoding(encoder));

        //    List<string> newLinesList = newLinesArray.ToList();
        //    List<string> newFileContent = new List<string>();

        //    for (int index = 0; index < fileContent.Length; index++)
        //    {
        //        newFileContent.Add(fileContent[index]);
        //        string replace = ".";
        //        for (int i = 0; i < newLinesArray.Count(); i++)
        //        {
        //            if (newLinesList[i].Contains("insert below|") || newLinesList[i].Contains("insert above|"))
        //            {
        //                string[] splitted = newLinesList[i].Split('|');
        //                int lineIndex = Convert.ToInt32(splitted[1]);

        //                if (newLinesList[i].Contains("insert below|"))
        //                {
        //                    if (index + 1 == lineIndex)
        //                    {
        //                        newFileContent.Add(splitted[2]);
        //                    }
        //                }
        //                else
        //                {
        //                    if (index == 0)
        //                    {
        //                        if (index + 1 == lineIndex)
        //                        {
        //                            newFileContent.Clear();
        //                            newFileContent.Add(splitted[2]);
        //                            newFileContent.Add(fileContent[index]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (index + 1 == lineIndex)
        //                        {
        //                            newFileContent.Add(splitted[2]);
        //                        }
        //                    }
        //                }
        //            }
        //            else if (newLinesList[i].Contains("replace|"))
        //            {
        //                string[] splitted = newLinesList[i].Split('|');
        //                int lineIndex = Convert.ToInt32(splitted[1]);

        //                if (index + 1 == lineIndex)
        //                {
        //                    replace = fileContent[index];
        //                    newFileContent.Remove(fileContent[index]);
        //                    newFileContent.Add(splitted[2]);
        //                }
        //            }

        //            string[] deleteSplit = newLinesList[i].Split('|');
        //            int deleteIndex = Convert.ToInt32(deleteSplit[1]);

        //            if (newLinesList[i].Contains("delete|"))
        //            {
        //                newFileContent.Remove(fileContent[deleteIndex - 1]);
        //            }
        //        }
        //    }

        //    File.WriteAllLines(destinationPath, newFileContent, Encoding.GetEncoding(encoder));
        //}

        public SaveInfo NewSaveInfo(string section, string key, string value)
        {
            return new SaveInfo(section, key, value);
        }

        public bool ModifySaveFile(string installSavePath, string saveFullPath, SaveType type, params SaveInfo[] info)
        {
            //if (!File.Exists(installSavePath))
            //{
            //    Log(installSavePath + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{installSavePath}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }

        public bool HexEdit(string fileToEdit, string address, byte[] newBytes)
        {
            //if (!File.Exists(fileToEdit))
            //{
            //    Log(fileToEdit + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{fileToEdit}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            using (Stream stream = File.Open(fileToEdit, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = long.Parse(address, NumberStyles.HexNumber);
                stream.Write(newBytes, 0, newBytes.Length);
            }

            return true;
        }

        public bool PatchFileFindAll(string originalFile, string patchedFile, byte[] patchFind, byte[] patchReplace)
        {
            //if (!File.Exists(originalFile))
            //{
            //    Log(originalFile + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{originalFile}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

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

                for (int w = 0; w < patchReplace.Length; w++)
                {
                    fileContent[p + w] = patchReplace[w];
                }
            }

            // Save it to another location.
            File.WriteAllBytes(patchedFile, fileContent);

            return true;
        }

        public bool PatchFileFindPattern(string originalFile, string patchedFile, string origPattern, string patchPattern, bool patchall) // 2.1.2
        {
            //if (!File.Exists(originalFile))
            //{
            //    Log(originalFile + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{originalFile}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            Log($"Patching {patchedFile}");
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

            return true;
        }

        public bool PatchFileFindPattern(string originalFile, string patchedFile, string origPattern, int insert, int offset, bool patchall) // 2.1.2
        {
            //if (!File.Exists(originalFile))
            //{
            //    Log(originalFile + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{originalFile}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            Log($"Patching {patchedFile}");

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
            byte[] formatedInsert = new byte[] { insertdef[0], insertdef[1] };

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

            return true;
        }


        public bool PatchFile(string originalFile, string patchedFile, byte[] patchFind, byte[] patchReplace)
        {
            //if (!File.Exists(originalFile))
            //{
            //    Log(originalFile + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{originalFile}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
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

            return true;
        }

        public bool PatchFile(string originalFile, string patchedFile, string spatchFind, string spatchReplace)
        {
            //if (!File.Exists(originalFile))
            //{
            //    Log(originalFile + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{originalFile}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
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

            return true;
        }

        //XPath syntax
        //https://www.w3schools.com/xml/xpath_syntax.asp
        public bool ChangeXmlAttributeValue(string path, string xpath, string attributeName, string attributeValue)
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

            return true;
        }

        public bool ChangeXmlNodeValue(string path, string xpath, string nodeValue)
        {
            //if (!File.Exists(path))
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            path = Environment.ExpandEnvironmentVariables(path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                node.Value = nodeValue;
            }
            doc.Save(path);
            return true;
        }

        public bool ChangeXmlNodeInnerTextValue(string path, string xpath, string innerTextValue)
        {
            //if (!File.Exists(path))
            //{
            //    Log(path + " not Found!");
            //    //MessageBox.Show($"File not found at : \n{path}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            path = Environment.ExpandEnvironmentVariables(path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
            {
                node.InnerText = innerTextValue;
            }
            doc.Save(path);
            return true;
        }

        public bool CreateRegKey(string baseKey, string sKey, string subKey)
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

            return true;
        }

        private bool ExportRegistry(string strKey, string filepath)
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

            return true;
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

        public void DeleteRegKeyValues(string baseKey, string sKey, string[] values)
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

            foreach (var value in values)
            {
                key.DeleteValue(value);
            }

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

            Log("Registry key : " + baseKey + "\\" + sKey + "\\" + subKey + " -Value= " + value + " -RegType= " + regType + " will not be deleted from registry on Nucleus Co-op close");

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

        public bool KillProcessesMatchingWindowName(string name)
        {
            foreach (Process p in System.Diagnostics.Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains(name.ToLower())).ToArray())
            {
                p.Kill();
            }

            return true;
        }

        public void Wait(int wait)
        {
            Log(string.Format("Pausing for " + (double)wait / (double)1000 + " seconds"));
            Thread.Sleep(wait);
        }

        public bool KillProcessesMatchingProcessName(string name)
        {
            foreach (Process p in System.Diagnostics.Process.GetProcesses().Where(x => x.ProcessName.ToLower().Contains(name.ToLower())).ToArray())
            {
                p.Kill();
            }

            return true;
        }

        public bool MoveFolder(string sourceDirName, string destDirName)
        {
            //if (!Directory.Exists(sourceDirName))
            //{
            //    MessageBox.Show($"Directory not found at : \n{sourceDirName}\nStart the game out of Nucleus Co-op once in order to create the required file(s). ", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

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

            return true;
        }
    }
}
