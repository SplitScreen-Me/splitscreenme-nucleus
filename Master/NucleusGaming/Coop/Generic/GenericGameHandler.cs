using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.Generic.Cursor;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Logging;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Tools.GameStarter;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.Windows.Interop;
using Nucleus.Interop.User32;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using WindowScrape.Constants;
using WindowScrape.Types;

namespace Nucleus.Gaming
{
    public class GenericGameHandler : IGameHandler, ILogNode
    {
        private const float HWndInterval = 10000;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(UInt32 destAddr, out UInt32 bestIfIndex);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcessWithLogonW(
           String userName,
           String domain,
           String password,
           LogonFlags logonFlags,
           String applicationName,
           String commandLine,
           ProcessCreationFlags creationFlags,
           UInt32 environment,
           String currentDirectory,
           ref STARTUPINFO startupInfo,
           out PROCESS_INFORMATION processInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteProcessMemory(
            IntPtr hProcess,
            Int64 lpBaseAddress,
            [In, Out] Byte[] lpBuffer,
            UInt64 dwSize,
            out IntPtr lpNumberOfBytesWritten);

        [Flags]
        enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        public enum DMDO
        {
            DEFAULT = 0,
            D90 = 1,
            D180 = 2,
            D270 = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct DEVMODE
        {
            public const int DM_PELSWIDTH = 0x80000;
            public const int DM_PELSHEIGHT = 0x100000;
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public DMDO dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int ChangeDisplaySettings([In] ref DEVMODE lpDevMode, int dwFlags);

        enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT Rect);

        [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        public static extern bool DeleteProfile(string sidString, string profilePath, string omputerName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnableWindow(IntPtr hWnd, bool bEnable);


        private static GenericGameHandler instance;
        public static GenericGameHandler Instance { get { return instance; } }

        private string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); //Environment.GetEnvironmentVariable("userprofile"); //$@"C:\Users\{Environment.UserName}";
        private string DocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private bool gameIs64 = false;

        private int origWidth = 0;
        private int origHeight = 0;
        private double origRatio = 1;
        private int playerBoundsWidth = 0;
        private int playerBoundsHeight = 0;
        private int prevWindowWidth = 0;
        private int prevWindowHeight = 0;
        private int prevWindowX = 0;
        private int prevWindowY = 0;
        private ProcessData prevProcessData;

        private int prevProcId = 0;

        private int plyrIndex = 0;
        private int kbi = 1;

        private long random_steam_id = 76561199023125438;
        private string nucleusFolderPath;

        private UserGameInfo userGame;
        private GameProfile profile;
        private GenericGameInfo gen;
        private Dictionary<string, string> jsData;
        private GenericContext context;

        private double timer;
        private int exited;
        private List<Process> attached = new List<Process>();
        private List<int> attachedIds = new List<int>();

        private List<Process> attachedLaunchers = new List<Process>();
        private List<int> attachedIdsLaunchers = new List<int>();

        private List<int> mutexProcs = new List<int>();

        public List<string> userBackedFiles = new List<string>();

        public int[] procOrder;

        protected bool hasEnded;
        protected double timerInterval = 1000;

        public event Action Ended;
        private CursorModule _cursorModule;

        private bool hasKeyboardPlayer = false;
        private string keyboardInstance;
        private int keyboardProcId;

        private Thread fakeFocus;
        private Thread statusWinThread;
        private string logMsg;

        private bool symlinkNeeded;

        public int numPlayers = 0;

        private bool dllResize = false;
        private bool dllRepos = false;

        private string exePath;
        private string origExePath;

        private string instanceExeFolder;
        public string garch;

        private static string lobbyConnectArg;
        private static bool readToEnd = false;

        private string currentIPaddress;
        private string currentSubnetMask;
        private bool isDHCPenabled;
        private bool isDynamicDns;
        private string currentGateway;
        private int hostAddr = 169;
        private List<string> dnsAddresses = new List<string>();
        string dnsServersStr;
        private string iniNetworkInterface;
        private bool isPrevent;
        private string adminLocalGroup;
        private string nucleusUserAccountsPassword = "12345";

        public static string[] customValue;
        public static string ofdPath;
        public string ssePath = string.Empty;

        private bool earlyExit;
        private bool processingExit = false;

        private Process launchProc;

        private List<string> addedFiles = new List<string>();
        private List<string> backupFiles = new List<string>();

        public string JsFilename;
        public string HandlerGUID;
        public bool UsingNucleusAccounts;
        private bool useDocs;

        private string startingArgs;

        private readonly IniFile ini = new Gaming.IniFile(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "Settings.ini"));
        bool isDebug;

        UserScreen owner;

        private List<Display> screensInUse = new List<Display>();
        private List<Display> screensChanged = new List<Display>();

        public enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        }

        public static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(0x3c, SeekOrigin.Begin);
            Int32 peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            UInt32 peHead = br.ReadUInt32();

            if (peHead != 0x00004550) // "PE\0\0", little-endian
            {
                throw new Exception("Can't find PE header");
            }

            MachineType machineType = (MachineType)br.ReadUInt16();
            br.Close();
            fs.Close();
            return machineType;
        }

        // Returns true if the exe is 64-bit, false if 32-bit, and null if unknown
        public static bool? Is64Bit(string exePath)
        {
            switch (GetDllMachineType(exePath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        public Thread FakeFocus
        {
            get { return fakeFocus; }
        }

        private enum FocusMessages
        {
            WM_ACTIVATEAPP = 0x001C,
            WM_ACTIVATE = 0x0006,
            WM_NCACTIVATE = 0x0086,
            WM_SETFOCUS = 0x0007,
            WM_MOUSEACTIVATE = 0x0021,
        }

        public virtual bool HasEnded
        {
            get { return hasEnded; }
        }

        public double TimerInterval
        {
            get { return timerInterval; }
        }

        private void ForceFinish()
        {
            // search for game instances left behind
            try
            {
                Process[] procs = Process.GetProcesses();

                List<string> addtlProcsToKill = new List<string>();
                if (gen.KillProcessesOnClose?.Length > 0)
                {
                    addtlProcsToKill = gen.KillProcessesOnClose.ToList();
                }

                foreach (Process proc in procs)
                {
                    try
                    {
                        if ((gen.LauncherExe != null && !gen.LauncherExe.Contains("NucleusDefined") && proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(gen.LauncherExe.ToLower())) || addtlProcsToKill.Contains(proc.ProcessName, StringComparer.OrdinalIgnoreCase) || proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(gen.ExecutableName.ToLower()) || (proc.Id != 0 && attachedIds.Contains(proc.Id)) || (gen.Hook.ForceFocusWindowName != "" && proc.MainWindowTitle == gen.Hook.ForceFocusWindowName))
                        {
                            Log(string.Format("Killing process {0} (pid {1})", proc.ProcessName, proc.Id));
                            proc.Kill();
                        }

                    }
                    catch (Exception ex)
                    {
                        Log(ex.InnerException + " " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.InnerException + " " + ex.Message);
            }
        }

        private void DeleteProfileFolder(string file)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C rd /S /Q  \"" + file + "\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public List<string> GetNetAdapters()
        {
            List<String> values = new List<String>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                values.Add(nic.Name);
            }
            return values;
        }

        public void End(bool fromStopButton)
        {
            if (!processingExit)
            {
                processingExit = true;
            }
            else
            {
                Log("Already processing exit");
                return;
            }


            if (fromStopButton && LockInput.IsLocked)
            {
                //TODO: For some reason the Stop button is clicked during split screen. Temporary fix is to not end if input is locked.
                Log("IGNORING SHUTDOWN BECAUSE INPUT LOCKED");
                return;
            }

            if (ini.IniReadValue("Misc", "ShowStatus") == "True")
            {

                try
                {
                    if (statusWinThread != null && statusWinThread.IsAlive)
                    {
                        statusWinThread.Abort();
                        Thread.Sleep(50);
                    }

                    statusWinThread = new Thread(ShowStatus);
                    statusWinThread.Start();
                }
                catch { }
            }

            Log("----------------- SHUTTING DOWN -----------------");

            LockInput.Unlock(false, gen?.ProtoInput);

            gen.OnStop?.Invoke();

            ForceFinish();

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
                if (!string.IsNullOrEmpty(gen.BinariesFolder))
                {
                    rootFolder = ReplaceCaseInsensitive(exeFolder, gen.BinariesFolder.ToLower(), "");
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
                foreach (string filePath in userBackedFiles)
                {
                    string origFileName = filePath.Replace("_NUCLEUS_BACKUP" + Path.GetExtension(filePath), Path.GetExtension(filePath));
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

            string[] regFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);
            if (regFiles.Length > 0)
            {
                Log("Restoring backed up registry files");
                foreach (string regFilePath in regFiles)
                {
                    Process proc = new Process();

                    try
                    {
                        proc.StartInfo.FileName = "reg.exe";
                        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;

                        string command = "import \"" + regFilePath + "\"";
                        proc.StartInfo.Arguments = command;
                        proc.Start();

                        proc.WaitForExit();
                        Log($"Imported {Path.GetFileName(regFilePath)}");
                    }
                    catch (Exception)
                    {
                        proc.Dispose();
                    }
                    if (!regFilePath.Contains("User Shell Folders"))
                    {
                        File.Delete(regFilePath);
                    }

                }
            }

            Thread.Sleep(1000);

            if (screensChanged.Count > 0)
            {
                foreach (Display screen in screensChanged)
                {
                    Log($"Resetting resolution for {screen.MonitorID} to revert Dpi settings");
                    SetResolution(800, 600, screen.DeviceName);
                    SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);
                }
            }

            Thread.Sleep(1000);

            List<PlayerInfo> data = profile.PlayerData;
            foreach (PlayerInfo player in data)
            {
                if (player.DInputJoystick != null)
                {
                    player.DInputJoystick.Dispose();
                }
            }

            if (fakeFocus != null && fakeFocus.IsAlive)
            {
                Log("Aborting thread to send fake focus messages");
                fakeFocus.Abort();
            }

            List<string> nucUsers = new List<string>();
            List<string> nucSIDs = new List<string>();

            if (!earlyExit)
            {
                string[] folderUsers = Directory.GetDirectories(Path.GetDirectoryName(NucleusEnvironmentRoot));

                if (gen.TransferNucleusUserAccountProfiles)
                {
                    Log("Transfer Nucleus user account profiles is enabled");
                    foreach (PlayerInfo player in data)
                    {
                        Log("Backing up AppData and Documents from " + Path.Combine($@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{player.UserProfile}") + " to " + Path.Combine(NucleusEnvironmentRoot + $@"\NucleusCoop\UserAccounts"));
                        string subFolder;
                        for (int fol = 0; fol < 2; fol++)
                        {
                            if (fol == 0)
                            {
                                subFolder = "AppData";
                            }
                            else
                            {
                                subFolder = "Documents";
                            }

                            string SourcePath = $@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{player.UserProfile}\{subFolder}";
                            string DestinationPath = NucleusEnvironmentRoot + $@"\NucleusCoop\UserAccounts\{player.UserProfile}\{subFolder}";

                            try
                            {
                                if (Directory.Exists(SourcePath))
                                {
                                    Directory.CreateDirectory(DestinationPath);
                                    string cmd = "xcopy \"" + SourcePath + "\" \"" + DestinationPath + "\" /E/H/C/I/Y/R";
                                    CmdUtil.ExecuteCommand(SourcePath, out int exitCode, cmd, true);
                                    Log($"Command: {cmd}, exit code: {exitCode}");
                                }

                            }
                            catch (Exception ex)
                            {
                                Log("ERROR - " + ex.Message);
                            }

                        }
                        Thread.Sleep(1000);
                    }
                }

                if (ini.IniReadValue("Misc", "KeepAccounts") != "True")
                {
                    if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
                    {
                        Log("Deleting temporary user accounts");

                        foreach (string folder in folderUsers)
                        {
                            string username = folder.Substring(Path.GetDirectoryName(NucleusEnvironmentRoot).Length + 1);
                            if (username.StartsWith("nucleusplayer"))
                            {
                                nucUsers.Add(username);
                                try
                                {                                  
                                    PrincipalContext principalContext = new PrincipalContext(ContextType.Machine);
                                    UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
                                    if (userPrincipal != null)
                                    {
                                        SecurityIdentifier userSid = userPrincipal.Sid;
                                        nucSIDs.Add(userSid.ToString());
                                        DeleteProfile(userSid.ToString(), null, null);
                                        userPrincipal.Delete();
                                        string keyName = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
                                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
                                        {
                                            key.DeleteSubKeyTree(userSid.ToString(), false);
                                        }

                                        Thread.Sleep(250);

                                        using (RegistryKey key = Registry.Users)
                                        {
                                            key.DeleteSubKeyTree(userSid.ToString(), false);
                                            key.DeleteSubKeyTree(userSid.ToString() + "_Classes", false);
                                        }
                                    }
                                    else
                                    {
                                        //MessageBox.Show("ERROR! User: {0} not found!", username);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    //MessageBox.Show(exception.Message);
                                }

                                Thread.Sleep(1000);
                            }
                        }

                        string deleteUserBatPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\LaunchUsers\\delete_users.bat");
                        if (File.Exists(deleteUserBatPath))
                        {
                            File.Delete(deleteUserBatPath);
                        }


                        using (StreamWriter sw = new StreamWriter(deleteUserBatPath, true))
                        {
                            sw.WriteLine("@echo off");
                            //for (int pc = 1; pc <= numPlayers; pc++)
                            //{
                            //    sw.WriteLine($"net user nucleusplayer{pc} /delete");
                            //}

                            foreach (string nucUser in nucUsers)
                            {
                                sw.WriteLine($"net user {nucUser} /delete");
                            }
                        }

                        Process user = new Process();
                        user.StartInfo.FileName = deleteUserBatPath; //"utils\\LaunchUsers\\delete_users.bat";
                        user.StartInfo.Verb = "runas";
                        user.StartInfo.UseShellExecute = true;
                        user.Start();
                        user.WaitForExit();


                        if (File.Exists(deleteUserBatPath))
                        {
                            File.Delete(deleteUserBatPath);
                        }

                        Thread.Sleep(1000);
                    }
                }

                if (gen.ChangeIPPerInstanceAlt)
                {
                    Log("Uninstalling loopback adapters");
                    Process p = new Process();

                    //string devconPath = Path.Combine(Directory.GetCurrentDirectory(), "utils\\devcon\\devcon.exe");
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location) + "\\utils\\devcon";
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.Arguments = "/C devcon remove \"@ROOT\\NET\\*\"";

                    p.Start();

                    string stdOut = p.StandardOutput.ReadToEnd();
                    Log("ChangeIPPerInstanceAlt remove output " + stdOut);

                    p.WaitForExit();

                    Thread.Sleep(1000);
                }

                if (gen.ChangeIPPerInstance)
                {
                    Log("Reverting IP settings back to normal");
                    //MessageBox.Show("Reverting IP settings back to normal. You may receive another prompt to action it.", "Nucleus - Change IP Per Instance");
                    Forms.Prompt prompt = new Forms.Prompt("Reverting IP settings back to normal. You may receive another prompt to action it.");
                    prompt.ShowDialog();
                    if (isDHCPenabled)
                    {
                        SetDHCP(iniNetworkInterface);
                    }
                    else
                    {
                        SetIP(iniNetworkInterface, currentIPaddress, currentSubnetMask, currentGateway);
                    }

                    //if (isDynamicDns)
                    //{
                    //    SetDNS(iniNetworkInterface, true);
                    //}
                }

                if (gen.FlawlessWidescreen?.Length > 0)
                {
                    Process[] runnProcs = Process.GetProcesses();
                    foreach (Process proc in runnProcs)
                    {
                        if (proc.ProcessName.ToLower() == "flawlesswidescreen")
                        {
                            Log("Killing Flawless Widescreen app");
                            proc.Kill();
                        }
                    }

                    if (gen.FlawlessWidescreenOverrideDisplay)
                    {
                        Log("Restoring back up Flawless Widescreen settings file");
                        string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\FlawlessWidescreen\\" + garch);
                        string setPath = utilFolder + "\\settings.xml";
                        string backupPath = Path.GetDirectoryName(setPath) + "\\settings_NUCLEUS_BACKUP.xml";
                        if (File.Exists(backupPath))
                        {
                            if (File.Exists(setPath))
                            {
                                File.Delete(setPath);
                            }

                            File.Move(backupPath, setPath);
                        }
                    }
                }
            }

            if (gen.ProtoInput.AutoHideTaskbar || ini.IniReadValue("CustomLayout", "SplitDiv") == "True")
            {
                ProtoInput.protoInput.SetTaskbarAutohide(false);
            }

            User32Util.ShowTaskBar();

            hasEnded = true;
            GameManager.Instance.ExecuteBackup(userGame.Game);

            LogManager.UnregisterForLogCallback(this);

            foreach (var window in RawInputManager.windows)
            {
                window.HookPipe?.Close();
            }

            Cursor.Clip = Rectangle.Empty; // guarantee were not clipping anymore
            string backupDir = GameManager.Instance.GempTempFolder(userGame.Game);

            if (_cursorModule != null)
            {
                _cursorModule.Stop();
            }

            Thread.Sleep(1000);
            // delete symlink folder

            int tempIndex = 0;

            RawInputManager.EndSplitScreen();

#if RELEASE
            if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
            {
                Log("Deleting any files Nucleus added to original game folder");
                foreach (string addedFilePath in addedFiles)
                {
                    File.Delete(addedFilePath);
                }

                Thread.Sleep(500);

                Log("Restoring any backed up files");
                foreach (string backupFilePath in backupFiles)
                {
                    try
                    {
                        if (File.Exists(backupFilePath))
                        {
                            string origFile = backupFilePath.Replace("_NUCLEUS_BACKUP", "");
                            File.Delete(origFile);
                            File.Move(backupFilePath, origFile);
                        }
                    }
                    catch { }
                }
            }

            if (gen.KeepSymLinkOnExit == false)
            {
                if (gen.SymlinkGame || gen.HardlinkGame || gen.HardcopyGame)
                {
                    for (int i = 0; i < profile.PlayerData.Count; i++)
                    {
                        tempIndex = i;
                        string linkFolder = Path.Combine(backupDir, "Instance" + i);
                        Log(string.Format("Deleting folder {0} and all of its contents", linkFolder));
                        int retries = 0;
                        while (Directory.Exists(linkFolder))
                        {

                            try
                            {
                                Directory.Delete(linkFolder, true);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                Log("ERROR - UnauthorizedAccessException - " + ex.Message);
                                int pFrom = ex.Message.IndexOf("\'") + 1;
                                int pTo = ex.Message.LastIndexOf("\'");
                                string fileName = ex.Message.Substring(pFrom, pTo - pFrom);

                                //string linkFolder = Path.Combine(backupDir, "Instance" + tempIndex);
                                if (!fileName.Contains("\\"))
                                {
                                    string[] files = Directory.GetFiles(linkFolder, fileName, SearchOption.AllDirectories);
                                    foreach (string file in files)
                                    {
                                        FileInfo fi = new FileInfo(file);
                                        if (fi.IsReadOnly/*fi.Attributes == FileAttributes.ReadOnly*/)
                                        {
                                            Log(string.Format("File was read-only. Setting '{0}' to normal file attributes and deleting", fileName));
                                            File.SetAttributes(file, FileAttributes.Normal);
                                            File.Delete(file);
                                        }
                                        else
                                        {
                                            Log("ERROR - Unknown reason why file cannot be deleted. Skipping this file for now");
                                        }
                                    }
                                }

                                if (retries < 10)
                                {
                                    retries++;
                                    Log("Retrying to delete folder and all its contents");
                                }
                                else
                                {
                                    Log("Abandoning trying to delete folder and all its contents");
                                    //throw;
                                }

                            }
                            catch (Exception ex)
                            {
                                Log("ERROR - " + ex.Message);

                                if (retries < 10)
                                {
                                    retries++;
                                    Log("Retrying to delete folder and all its contents");
                                }
                                else
                                {
                                    Log("Abandoning trying to delete folder and all its contents");
                                    //throw;
                                }
                            }
                            if (Directory.Exists(linkFolder))
                            {
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                Log("Folder and all its contents have successfully been deleted");
                            }
                        }
                    }
                    Log("File deletion complete");
                }
            }

            if (ini.IniReadValue("Misc", "KeepAccounts") != "True")
            {
                foreach (string nucUser in nucUsers)
                {
                    DeleteProfileFolder($@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{nucUser}");
                }
            }

#endif
            Log("All done closing operations. Exiting Nucleus.");

            try
            {
                if (statusWinThread != null && statusWinThread.IsAlive)
                {
                    statusWinThread.Abort();
                }
            }
            catch { }

            Ended?.Invoke();
        }

        public string GetFolder(Folder folder)
        {
            string str = folder.ToString();
            string output;
            if (jsData.TryGetValue(str, out output))
            {
                return output;
            }
            return "";
        }

        public bool Initialize(UserGameInfo game, GameProfile profile)
        {

            instance = this;

            userGame = game;
            this.profile = profile;

            if (ini.IniReadValue("Misc", "DebugLog") == "True")
            {
                isDebug = true;
            }

            iniNetworkInterface = ini.IniReadValue("Misc", "Network");

            List<PlayerInfo> players = profile.PlayerData;

            gen = game.Game as GenericGameInfo;
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

                _cursorModule = new CursorModule();


            jsData = new Dictionary<string, string>();
            jsData.Add(Folder.Documents.ToString(), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            jsData.Add(Folder.MainGameFolder.ToString(), Path.GetDirectoryName(game.ExePath));
            jsData.Add(Folder.InstancedGameFolder.ToString(), Path.GetDirectoryName(game.ExePath));

            timerInterval = gen.HandlerInterval;

            LogManager.RegisterForLogCallback(this);

            JsFilename = gen.JsFileName;
            HandlerGUID = gen.GUID;

            if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
            {
                UsingNucleusAccounts = true;
            }

            return true;
        }


        public string ReplaceCaseInsensitive(string str, string toFind, string toReplace)
        {
            string lowerOriginal = str.ToLower();
            string lowerFind = toFind.ToLower();
            string lowerRep = toReplace.ToLower();

            int start = lowerOriginal.IndexOf(lowerFind);
            if (start == -1)
            {
                return str;
            }

            string end = str.Remove(start, toFind.Length);
            end = end.Insert(start, toReplace);

            return end;
        }


        private static string internalGetRelativePath(DirectoryInfo dirInfo, DirectoryInfo rootInfo, string str)
        {
            if (dirInfo == null || dirInfo.FullName == rootInfo.FullName)
            {
                return str;
            }

            if (!string.IsNullOrWhiteSpace(Path.GetExtension(dirInfo.Name)))
            {
                str = dirInfo.Name;
            }
            else
            {
                str = dirInfo.Name + "\\" + str;
            }

            dirInfo = dirInfo.Parent;
            return internalGetRelativePath(dirInfo, rootInfo, str);
        }

        public void ShowStatus()
        {
            try
            {

                string lblTxt = "Engine starting up";
                if (processingExit)
                {
                    lblTxt = "Starting shut down procedures";
                }

                Label statusLbl = new Label
                {
                    Text = lblTxt,
                    Width = 560,
                    Height = 93,
                    AutoSize = false,
                    Location = new Point(12, 9),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                //private Form statusForm;
                Form statusForm = new Form()
                {
                    Text = "Nucleus Coop - Status",
                    Width = 600,
                    Height = 150,

                    ControlBox = true,
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(0, 0),
                    MaximizeBox = false,
                    MinimizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    TopMost = true,
                    // Icon = Icon.FromHandle(Properties.Resources.icon.GetHicon()),
                    AutoScaleMode = AutoScaleMode.Font,
                    AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F)
                };

                statusForm.FormClosing += new FormClosingEventHandler(StatusForm_Closing);
                statusForm.Controls.Add(statusLbl);
                statusForm.Show();



                while (statusWinThread != null && statusWinThread.IsAlive)
                {
                    try
                    {
                        if (statusWinThread != null && statusWinThread.IsAlive)
                        {
                            Thread.Sleep(100);

                            statusForm.TopMost = true;
                            statusForm.TopMost = false;
                            statusForm.TopMost = true;
                            statusLbl.Text = logMsg;
                            Application.DoEvents();

                            WindowScrape.Static.HwndInterface.MakeTopMost(statusForm.Handle);

                            try
                            {
                                statusForm.Refresh();
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }

        }

        private static DISP_CHANGE SetResolution(int w, int h, string deviceName)
        {

            DEVMODE dm = new DEVMODE();

            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmPelsWidth = w;
            dm.dmPelsHeight = h;
            dm.dmFields = DEVMODE.DM_PELSWIDTH | DEVMODE.DM_PELSHEIGHT;
            DISP_CHANGE result = ChangeDisplaySettingsEx(deviceName, ref dm, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_RESET), IntPtr.Zero);
            ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);

            return result;
        }

        public static string EnumerateSupportedModes(Display displayDevice)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmDeviceName = displayDevice.DeviceName;

            int modeIndex = 0; // 0 = The first mode

            string size = "NULL";

            while (EnumDisplaySettings(null, modeIndex, ref dm) == true) // Mode found
            {
                if ((dm.dmPelsWidth > 0 && dm.dmPelsHeight > 0) && (dm.dmPelsWidth != displayDevice.Bounds.Width || dm.dmPelsHeight != displayDevice.Bounds.Height))
                {
                    size = dm.dmPelsWidth + "x" + dm.dmPelsHeight;
                    break;
                }
                else
                {
                    modeIndex++; // The next mode
                }
            }

            return size;
        }

        public string Play()
        {

            garch = "x86";
            if (Is64Bit(userGame.ExePath) == true)
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
                Log("--------------------- START ---------------------");

                Log(string.Format("Game: {0}, Arch: {1}, Executable: {2}, Launcher: {3}, SteamID: {4}, Handler: {5}, Content Folder: {6}", gen.GameName, garch, gen.ExecutableName, gen.LauncherExe, gen.SteamID, gen.JsFileName, gen.GUID));

                string pcSpecs = "PC Info - ";
                var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                            select x.GetPropertyValue("Caption")).FirstOrDefault();
                //Log(name != null ? "OS:" + name.ToString() : "Windows OS: Unknown");

                pcSpecs += name != null ? "OS: " + name.ToString() + ", " : "Windows OS: Unknown, ";

                const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        pcSpecs += $".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}";
                        //Log($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}");
                    }
                    else
                    {
                        pcSpecs += $".NET Framework Version: {Environment.Version}";
                        //Log($".NET Framework Version: {Environment.Version}");
                    }
                }

                Log(pcSpecs);
            }

            ForceFinish();

            //Merge raw keyboard/mouse players into one
            var groupWindows = profile.PlayerData.Where(x => x.IsRawKeyboard || x.IsRawMouse).GroupBy(x => x.MonitorBounds).ToList();
            foreach (var group in groupWindows)
            {
                var firstInGroup = group.First();
                firstInGroup.IsRawKeyboard = group.Count(x => x.IsRawKeyboard) > 0;
                firstInGroup.IsRawMouse = group.Count(x => x.IsRawMouse) > 0;

                if (firstInGroup.IsRawKeyboard) firstInGroup.RawKeyboardDeviceHandle = group.First(x => x.RawKeyboardDeviceHandle != (IntPtr)(-1)).RawKeyboardDeviceHandle;
                if (firstInGroup.IsRawMouse) firstInGroup.RawMouseDeviceHandle = group.First(x => x.RawMouseDeviceHandle != (IntPtr)(-1)).RawMouseDeviceHandle;

                foreach (var x in group)
                {
                    profile.PlayerData.Remove(x);
                }

                profile.PlayerData.Add(firstInGroup);
            }

            var reorderPlayers = profile.PlayerData.OrderBy(c => c.Owner.display.Y).ThenBy(c => c.Owner.display.X).ThenBy(c => c.MonitorBounds.Y).ThenBy(c => c.MonitorBounds.X);//MonitorBounds nothing else?
            List<PlayerInfo> reorderedPlyrs = new List<PlayerInfo>();

            foreach (var player in reorderPlayers)
            {
                reorderedPlyrs.Add(player);
            }

            if (ini.IniReadValue("CustomLayout", "SplitDiv") == "True" && gen.SplitDivCompatibility == true && reorderedPlyrs.Count != 1)
            {
                Log("Setup splitscreen division");
                for (int i = 0; i < reorderedPlyrs.Count; i++)
                {
                    //int SplitDivThickness = Convert.ToInt32(ini.IniReadValue("CustomLayout", "SplitDivThickness"));
                    var player = reorderedPlyrs[i];
                    Point XY = new Point(player.MonitorBounds.X + 1, player.MonitorBounds.Y + 1);
                    Size WH = new Size(player.MonitorBounds.Width - 2, player.MonitorBounds.Height - 2);
                    Rectangle bounds = new Rectangle(XY, WH);
                    player.MonitorBounds = bounds;
                }
            }

            List<PlayerInfo> players = reorderedPlyrs;

            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerID = i;

                Log("Determining which monitors will be used by Nucleus");
                foreach (Display dp in User32Util.GetDisplays())
                {
                    //if (players[i].ScreenIndex != (dp.DisplayIndex - 1) && !screensInUse.Contains(dp))
                    //{
                    //A voir pour split div  //dp.Bounds.Bottom/Top etc.
                    screensInUse.Add(dp);
                    // }
                }
            }

            gen.SetPlayerList(players);

            gen.SetProtoInputValues();

            if (ini.IniReadValue("Misc", "AutoDesktopScaling") == "True")
            {
                Log("Checking if any monitors to be used by Nucleus are using DPI scaling other than 100%");
                RegistryKey perMonKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\PerMonitorSettings", true);
                if (perMonKey != null)
                {
                    //Log("TEMP: PerMonitorSettings exist");
                    foreach (var v in perMonKey.GetSubKeyNames())
                    {
                        //Log("TEMP: Looping through GetSubKeyNames");
                        foreach (Display screen in screensInUse)
                        {
                            //Log("TEMP: Looping through screensInUse");
                            if (v.ToString().StartsWith(screen.MonitorID))
                            {
                                //Log("TEMP: Found matching monitorID");
                                RegistryKey monitorKey = perMonKey.OpenSubKey(v, true);

                                Point dpi = new Point();
                                bool result = User32Util.GetDPIForMonitor(screen, ref dpi);
                                int calc = dpi.X / 24; // 96 (100%) / 24 = 4
                                int diff = calc - 4;

                                UInt32 currentVal = unchecked((UInt32)((Int32)monitorKey.GetValue("DpiValue")));

                                int newVal = unchecked((int)(currentVal - (UInt32)diff));

                                if (diff != 0)
                                {
                                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), $@"utils\backup\{v.ToString()}.reg")))
                                    {
                                        Log($"Backing up monitor settings for {screen.MonitorID}");
                                        ExportRegistry($@"HKEY_CURRENT_USER\Control Panel\Desktop\PerMonitorSettings\{v.ToString()}", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), $@"utils\backup\{v.ToString()}.reg"));
                                    }

                                    Log($"Setting DpiValue for {screen.MonitorID} from {currentVal} to {newVal}");
                                    monitorKey.SetValue("DpiValue", newVal, RegistryValueKind.DWord);

                                    string modeOutput = EnumerateSupportedModes(screen);
                                    if (modeOutput != "NULL")
                                    {
                                        int width = Convert.ToInt32(modeOutput.Substring(0, modeOutput.IndexOf("x")));
                                        int height = Convert.ToInt32(modeOutput.Substring(modeOutput.IndexOf("x") + 1));
                                        SetResolution(width, height, screen.DeviceName);
                                    }
                                    else
                                    {
                                        SetResolution(800, 600, screen.DeviceName);
                                    }
                                    SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);

                                    screensChanged.Add(screen);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Log("PerMonitorSettings does not exist");
                    RegistryKey userCpDesktopKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
                    if (userCpDesktopKey != null)
                    {
                        //Log("TEMP: Using Control Panel\\Desktop\\ LogPixels and Win8DpiScaling");
                        string origPix = userCpDesktopKey.GetValue("LogPixels", string.Empty).ToString();
                        string origScale = userCpDesktopKey.GetValue("Win8DpiScaling", string.Empty).ToString();

                        if ((!string.IsNullOrEmpty(origPix) && origPix != "96") || (!string.IsNullOrEmpty(origScale) && origScale != "0") || (string.IsNullOrEmpty(origPix) && string.IsNullOrEmpty(origScale)))
                        {
                            Log($"Setting Windows DPI Scaling to 100% for the duration of Nucleus session - Original LogPixels:{origPix}, DpiScaling:{origScale}");
                            ExportRegistry(@"HKEY_CURRENT_USER\Control Panel\Desktop", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Control Panel Desktop.reg"));

                            userCpDesktopKey.SetValue("LogPixels", 96, RegistryValueKind.DWord);
                            userCpDesktopKey.SetValue("Win8DpiScaling", 0, RegistryValueKind.DWord);

                            foreach (Display screen in screensInUse)
                            {
                                string modeOutput = EnumerateSupportedModes(screen);
                                if (modeOutput != "NULL")
                                {
                                    int width = Convert.ToInt32(modeOutput.Substring(0, modeOutput.IndexOf("x")));
                                    int height = Convert.ToInt32(modeOutput.Substring(modeOutput.IndexOf("x") + 1));
                                    SetResolution(width, height, screen.DeviceName);
                                }
                                else
                                {
                                    SetResolution(800, 600, screen.DeviceName);
                                }
                                SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);

                                screensChanged.Add(screen);
                            }
                        }
                    }
                }
            }
            else
            {
                Log("The Windows deskop resolution will not be set to 100% on game start because this option has been disabled in the  Nucleus Co-op settings");
            }

            UserScreen[] all = ScreensUtil.AllScreens();

            Log(string.Format("Display - DPIHandling: {0}, DPI Scale: {1}", gen.DPIHandling, DPIManager.Scale));
            for (int x = 0; x < all.Length; x++)
            {
                Log(string.Format("Monitor {0} - Resolution: {1}", x, all[x].MonitorBounds.Width + "x" + all[x].MonitorBounds.Height));
            }

            string nucleusRootFolder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location);
            nucleusFolderPath = nucleusRootFolder;

            string tempDir = GameManager.Instance.GempTempFolder(gen);
            string exeFolder = Path.GetDirectoryName(userGame.ExePath).ToLower();
            string rootFolder = exeFolder;
            string workingFolder = exeFolder;

            if (!string.IsNullOrEmpty(gen.BinariesFolder))
            {
                rootFolder = ReplaceCaseInsensitive(exeFolder, gen.BinariesFolder.ToLower(), "");
            }
            if (!string.IsNullOrEmpty(gen.WorkingFolder))
            {
                workingFolder = Path.Combine(exeFolder, gen.WorkingFolder.ToLower());
            }

            bool first = true;
            bool keyboard = false;

            if (ini.IniReadValue("Hotkeys", "LockKey") != "Default(End key)")
            {
                IDictionary<string, int> lockKeys = new Dictionary<string, int>
                {
                    { "Home", 0x24 },
                    { "Delete", 0x2E },
                    { "Multiply", 0x6A },
                    { "F1", 0x70 },
                    { "F2", 0x71 },
                    { "F3", 0x72 },
                    { "F4", 0x73 },
                    { "F5", 0x74 },
                    { "F6", 0x75 },
                    { "F7", 0x76 },
                    { "F8", 0x77 },
                    { "F9", 0x78 },
                    { "F10", 0x79 },
                    { "F11", 0x7A },
                    { "F12", 0x7B },
                    { "+", 0xBB },
                    { "-", 0xBD },
                    { "Numpad 0", 0x60 },
                    { "Numpad 1", 0x61 },
                    { "Numpad 2", 0x62 },
                    { "Numpad 3", 0x63 },
                    { "Numpad 4", 0x64 },
                    { "Numpad 5", 0x65 },
                    { "Numpad 6", 0x66 },
                    { "Numpad 7", 0x67 },
                    { "Numpad 8", 0x68 },
                    { "Numpad 9", 0x69 }
                };

                foreach (KeyValuePair<string, int> key in lockKeys)
                {
                    if (key.Key == ini.IniReadValue("Hotkeys", "LockKey"))
                    {
                        gen.LockInputToggleKey = key.Value;
                    }

                }

                RawInputProcessor.ToggleLockInputKey = gen.LockInputToggleKey;
            }
            else
            {
                RawInputProcessor.ToggleLockInputKey = gen.LockInputToggleKey;
            }


            RawInputManager.windows.Clear();
            Window nextWindowToInject = null;

            numPlayers = players.Count;
            Log(string.Format("Number of players: {0}", numPlayers));

            if (ini.IniReadValue("Misc", "ShowStatus") == "True")
            {
                try
                {
                    if (statusWinThread != null && statusWinThread.IsAlive)
                    {
                        statusWinThread.Abort();
                        Thread.Sleep(50);
                    }

                    statusWinThread = new Thread(ShowStatus);
                    statusWinThread.Start();
                }
                catch
                { }

            }

            if (isDebug)
            {
                Log("Nucleus Co-op version: " + Globals.Version);

                Log("########## START OF HANDLER ##########");
                string line;

                StreamReader file = new StreamReader(Path.Combine(GameManager.Instance.GetJsScriptsPath(), gen.JsFileName));
                while ((line = file.ReadLine()) != null)
                {
                    Log(line);
                }

                file.Close();

                Log("########## END OF HANDLER ##########");
            }

            if (ini.IniReadValue("Misc", "NucleusAccountPassword") != "12345" && ini.IniReadValue("Misc", "NucleusAccountPassword") != "")
            {
                nucleusUserAccountsPassword = ini.IniReadValue("Misc", "NucleusAccountPassword");
            }

            for (int i = 0; i < players.Count; i++)
            {
                Log("********** Setting up player " + (i + 1) + " **********");
                PlayerInfo player = players[i];

                if (ini.IniReadValue("Misc", "UseNicksInGame") == "True")
                {
                    if (ini.IniReadValue("ControllerMapping", "Player_" + (i + 1)) == "")
                    {
                        player.Nickname = "Player" + (i + 1);
                    }
                    else
                    {
                        player.Nickname = ini.IniReadValue("ControllerMapping", "Player_" + (i + 1));
                    }
                }
                else
                {
                    player.Nickname = "Player" + (i + 1);
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
                                InjectDLLs(pdata.Process, nextWindowToInject, before);
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
                        InjectDLLs(pdata.Process, nextWindowToInject, before);
                    }
                }

                Rectangle playerBounds = player.MonitorBounds;
                owner = player.Owner;

                int width = playerBounds.Width;
                int height = playerBounds.Height;
                Log("Player monitor's resolution: " + owner.display.Width + "x" + owner.display.Height);
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
                    linkFolder = Path.Combine(tempDir, "Instance" + i);

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
                        //dirExclusions.Add(gen.BinariesFolder);
                    }

                    exePath = Path.Combine(linkBinFolder, userGame.Game.ExecutableName);
                    origExePath = Path.Combine(linkBinFolder, userGame.Game.ExecutableName);

                    if ((i == 0 && (gen.SymlinkGame || gen.HardlinkGame)) || gen.HardcopyGame)
                    {

                        if (gen.CopyFiles != null)
                        {
                            Log("Copying " + gen.CopyFiles.Length + " files in Game.CopyFiles");
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
                            Log("Game executable (" + gen.ExecutableName + ") will be copied and not symlinked");
                            fileCopies.Add(gen.ExecutableName.ToLower());
                            if (gen.LauncherExe?.Length > 0 && !gen.LauncherExe.Contains(':'))
                            {
                                Log("Launcher executable (" + gen.LauncherExe + ") will be copied and not symlinked");
                                fileCopies.Add(gen.LauncherExe.ToLower());
                            }
                        }

                        // additional ignored files by the generic info
                        if (gen.FileSymlinkExclusions != null)
                        {
                            Log(gen.FileSymlinkExclusions.Length + " Files in Game.FileSymlinkExclusions will not be symlinked");
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
                            Log(gen.FileSymlinkCopyInstead.Length + " Files in Game.FileSymlinkCopyInstead will be copied instead of symlinked");
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
                            Log(gen.DirSymlinkExclusions.Length + " Directories in Game.DirSymlinkExclusions will be ignored");
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
                            Log(gen.DirExclusions.Length + " Directories (and their contents) in Game.DirExclusions will be skipped");
                            string[] skipExclusions = gen.DirExclusions;
                            for (int k = 0; k < skipExclusions.Length; k++)
                            {
                                string s = skipExclusions[k];
                                // make sure it's lower case
                                dirExclusions.Add("direxskip" + s.ToLower());
                            }
                        }
                        if (gen.DirSymlinkCopyInstead != null)
                        {
                            Log(gen.DirSymlinkCopyInstead.Length + " Directories and all its contents in Game.DirSymlinkCopyInstead will be copied instead of symlinked");
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
                    if (gen.ForceSymlink || !gen.KeepSymLinkOnExit || symlinkNeeded)
                    {
                        if (gen.ForceSymlink)
                        {
                            Log("Forcing symlink/copy");
                        }

                        if (gen.HardcopyGame)
                        {
                            Log(string.Format("Copying game folder {0} to {1} ", rootFolder, linkFolder));
                            // copy the directory
                            int exitCode;
                            FileUtil.CopyDirectory(rootFolder, new DirectoryInfo(rootFolder), linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, true);
                            while (exitCode != 1)
                            {
                                Thread.Sleep(25);
                            }
                        }
                        else if (gen.HardlinkGame)
                        {
                            if (i == 0)
                            {
                                Log(string.Format("Hardlinking game folder and files at {0} to {1}, for each instance", rootFolder, tempDir));
                                int exitCode;
                                //CmdUtil.LinkDirectory(rootFolder, new DirectoryInfo(rootFolder), linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, fileCopiesArr, false, true);
                                //Nucleus.Gaming.Platform.Windows.IO.LinkDirectory(rootFolder, new DirectoryInfo(rootFolder), linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, fileCopiesArr, true);
                                while (!StartGameUtil.SymlinkGame(rootFolder, linkFolder, out exitCode, dirExclusions.ToArray(), fileExclusionsArr, fileCopiesArr, true, gen.SymlinkFolders, players.Count))
                                {
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
                                    Log("ERROR - Could not find " + gen.LauncherExe + " in instance folder, Game executable will be used instead; " + exePath);
                                }
                                else if (launcherFiles.Length == 1)
                                {
                                    exePath = launcherFiles[0];
                                    Log("Found launcher exe at " + exePath + ". This will be used to launch the game");
                                }
                                else
                                {
                                    exePath = launcherFiles[0];
                                    Log("Multiple " + gen.LauncherExe + "'s found in instance folder." + " Using " + exePath + " to launch the game");
                                }
                            }
                            else
                            {
                                Log("Ignoring validation check of launcher exe. Will use filepath: " + Path.Combine(linkFolder, gen.LauncherExe));
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

                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                                SearchOption.AllDirectories))
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

                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                                SearchOption.AllDirectories))
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
                                    Log("ERROR - Could not find " + gen.LauncherExe + " in instance folder, Game executable will be used instead; " + exePath);
                                }
                                else if (launcherFiles.Length == 1)
                                {
                                    exePath = launcherFiles[0];
                                    origExePath = launcherFiles[0];
                                    Log("Found launcher exe at " + exePath + ". This will be used to launch the game");
                                }
                                else
                                {
                                    exePath = launcherFiles[0];
                                    origExePath = launcherFiles[0];
                                    Log("Multiple " + gen.LauncherExe + "'s found in instance folder." + " Using " + exePath + " to launch the game");
                                }
                            }
                        }
                    }

                }


                if (processingExit)
                {
                    return string.Empty;
                }

                if (!gen.ForceProcessPick)
                {
                    ShowInstanceInfo(player, i);
                }

                if (gen.ChangeIPPerInstanceAlt)
                {
                    List<string> adptrs = GetNetAdapters();

                    Process p = new Process();

                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location) + "\\utils\\devcon";
                    p.StartInfo.CreateNoWindow = false;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.Arguments = "/C devcon install %WINDIR%\\Inf\\Netloop.inf *MSLOOP";

                    p.Start();

                    string stdOut = p.StandardOutput.ReadToEnd();
                    Log("ChangeIPPerInstanceAlt install output " + stdOut);

                    p.WaitForExit();

                    List<string> newAdptrs = GetNetAdapters();

                    foreach (string adptr in newAdptrs)
                    {
                        if (!adptrs.Contains(adptr))
                        {
                            players[i].Adapter = adptr;
                            break;
                        }
                    }

                    if (players[i].Adapter == null)
                    {
                        Log("Could not find new network adapter made for this player.");
                    }

                    Thread.Sleep(3000);

                    ChangeIPPerInstance(i, players[i].Adapter);
                }

                if (gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt)
                {
                    if (i == 0)
                    {
                        Log("Searching for administrators local group");
                        ArrayList localGroups = GetUserGroups(Environment.UserName);

                        if (localGroups != null && localGroups?.Count > 0)
                        {
                            if (localGroups.Contains("Administrators"))
                            {
                                adminLocalGroup = "Administrators";
                                Log("Found local group " + adminLocalGroup + " for current user");
                            }
                            else
                            {
                                foreach (string localGroup in localGroups)
                                {
                                    if (localGroup.ToLower().StartsWith("admin"))
                                    {
                                        Log("Found local group " + localGroup + " for current user");
                                        adminLocalGroup = localGroup;
                                        break;
                                    }
                                }
                                if (adminLocalGroup == null)
                                {
                                    adminLocalGroup = localGroups[0].ToString();
                                    Log("Unable to find an admin local group for current user, using " + adminLocalGroup);
                                }
                            }
                        }
                        else
                        {
                            Log("Unable to find any user groups, defaulting user group to Administrators");
                            adminLocalGroup = "Administrators";
                        }

                        Log("Checking if sufficient Nucleus user accounts already exist");
                        string createUserBatPath = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\LaunchUsers\\create_users.bat");

                        if (File.Exists(createUserBatPath))
                        {
                            File.Delete(createUserBatPath);
                        }

                        bool createNeeded = false;
                        bool echoLineUsed = false;
                        for (int pc = 1; pc <= numPlayers; pc++)
                        {
                            bool UserExists = false;
                            using (PrincipalContext princ = new PrincipalContext(ContextType.Machine))
                            {
                                UserPrincipal up = UserPrincipal.FindByIdentity(
                                    princ,
                                    IdentityType.SamAccountName,
                                    $"nucleusplayer{pc}");

                                UserExists = (up != null);
                            }
                            Log($"nucleusplayer{pc} exists: {UserExists}");
                            if (!UserExists)
                            {
                                createNeeded = true;
                                using (StreamWriter sw = new StreamWriter(createUserBatPath, true))
                                {
                                    if (!echoLineUsed)
                                    {
                                        sw.WriteLine(@"@echo off");
                                        echoLineUsed = true;
                                    }
                                    sw.WriteLine($@"net user nucleusplayer{pc} {nucleusUserAccountsPassword} /add && net user nucleusplayer{pc} {nucleusUserAccountsPassword} && net localgroup " + adminLocalGroup + $" nucleusplayer{pc} /add");
                                }
                            }
                        }

                        if (createNeeded)
                        {
                            Log("Some users need to be created; creating user accounts");
                            Process user = new Process();
                            user.StartInfo.FileName = createUserBatPath;
                            user.StartInfo.Verb = "runas";
                            user.StartInfo.UseShellExecute = true;
                            user.Start();

                            user.WaitForExit();
                        }

                        if (File.Exists(createUserBatPath))
                        {
                            File.Delete(createUserBatPath);
                        }

                        for (int pc = 1; pc <= numPlayers; pc++)
                        {
                            players[pc - 1].SID = GetUserSID($"nucleusplayer{pc}");
                            players[pc - 1].UserProfile = $"nucleusplayer{pc}";
                        }
                    }

                    STARTUPINFO startup = new STARTUPINFO();
                    startup.cb = Marshal.SizeOf(startup);

                    bool success = CreateProcessWithLogonW($"nucleusplayer{i + 1}", Environment.UserDomainName, nucleusUserAccountsPassword, LogonFlags.LOGON_WITH_PROFILE, "cmd.exe", null, ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, 0, null, ref startup, out PROCESS_INFORMATION processInformation);
                    Log("Launcing intermediary program (cmd)");

                    if (!success)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Log(string.Format("ERROR {0} - CreateProcessWithLogonW failed", error));
                    }

                    launchProc = Process.GetProcessById(processInformation.dwProcessId);

                    if (gen.TransferNucleusUserAccountProfiles)
                    {
                        Thread.Sleep(1000);
                        Log("Transfer Nucleus user account profiles is enabled");

                        List<string> SourcePath = new List<string>();
                        string OrigSourcePath = NucleusEnvironmentRoot + $@"\NucleusCoop\UserAccounts\{player.UserProfile}";
                        SourcePath.Add(OrigSourcePath);

                        if (gen.CopyEnvFoldersToNucleusAccounts?.Length > 0)
                        {
                            foreach (string folder in gen.CopyEnvFoldersToNucleusAccounts)
                            {
                                SourcePath.Add(NucleusEnvironmentRoot + "\\" + folder);
                            }
                        }

                        string DestinationPath = $@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{player.UserProfile}";
                        try
                        {
                            foreach (string folder in SourcePath)
                            {
                                if (folder != OrigSourcePath)
                                {
                                    DestinationPath = $@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{player.UserProfile}" + folder.Substring(NucleusEnvironmentRoot.Length);
                                }
                                else
                                {
                                    DestinationPath = $@"{Path.GetDirectoryName(NucleusEnvironmentRoot)}\{player.UserProfile}";
                                }

                                if (Directory.Exists(folder))
                                {
                                    Log("Copying " + folder + " to " + DestinationPath);
                                    Directory.CreateDirectory(DestinationPath);

                                    string cmd = "xcopy \"" + folder + "\" \"" + DestinationPath + "\" /E/H/C/I/Y/R";
                                    int exitCode = -6942069;
                                    CmdUtil.ExecuteCommand(folder, out exitCode, cmd, true);
                                    while (exitCode == -6942069)
                                    {
                                        Thread.Sleep(25);
                                    }
                                    Log($"Command: {cmd}, exit code: {exitCode}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR - " + ex.Message);
                        }
                    }
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
                    UserProfileConfigCopy(player);
                }

                if (!userSavePathConverted && !gen.UserProfileSavePathNoCopy && (gen.UserProfileSavePath?.Length > 0 || gen.ForceUserProfileSaveCopy) && gen.UseNucleusEnvironment)
                {
                    UserProfileSaveCopy(player);
                }

                if (!gen.DocumentsConfigPathNoCopy && (gen.DocumentsConfigPath?.Length > 0 || gen.ForceDocumentsConfigCopy) && gen.UseNucleusEnvironment)
                {
                    DocumentsConfigCopy(player);
                }

                if (!gen.DocumentsSavePathNoCopy && (gen.DocumentsSavePath?.Length > 0 || gen.ForceDocumentsSaveCopy) && gen.UseNucleusEnvironment)
                {
                    DocumentsSaveCopy(player);
                }

                if (gen.DeleteFilesInConfigPath?.Length > 0)
                {
                    string path = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\", gen.UserProfileConfigPath);
                    foreach (string fileName in gen.DeleteFilesInConfigPath)
                    {
                        string[] foundFiles = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
                        foreach (string foundFile in foundFiles)
                        {
                            if (!gen.IgnoreDeleteFilesPrompt)
                            {
                                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + foundFile + "'?", "Nucleus - Delete Files In Config Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dialogResult != DialogResult.Yes)
                                {
                                    continue;
                                }
                            }
                            Log(string.Format("Deleting file {0}", foundFile));
                            File.Delete(foundFile);
                        }
                    }
                }

                if (gen.DeleteFilesInSavePath?.Length > 0)
                {
                    string path = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\", gen.UserProfileConfigPath);
                    foreach (string fileName in gen.DeleteFilesInSavePath)
                    {
                        string[] foundFiles = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
                        foreach (string foundFile in foundFiles)
                        {
                            if (!gen.IgnoreDeleteFilesPrompt)
                            {
                                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + foundFile + "'?", "Nucleus - Delete Files In Save Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (dialogResult != DialogResult.Yes)
                                {
                                    continue;
                                }
                            }
                            Log(string.Format("Deleting file {0}", foundFile));
                            File.Delete(foundFile);
                        }
                    }
                }

                if (gen.ChangeExe)
                {
                    ChangeExe(i);
                }

                if (gen.RenameAndOrMoveFiles?.Length > 0)
                {
                    RenameOrMoveFiles(linkFolder, i);
                }

                if (gen.DeleteFiles?.Length > 0)
                {
                    DeleteFiles(linkFolder, i);
                }

                context = gen.CreateContext(profile, player, this, hasKeyboardPlayer);
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
                    Log("Symlinking " + gen.SymlinkFiles.Length + " files list from \"Game.SymlinkFiles\"");
                    context.SymlinkFiles = gen.SymlinkFiles;
                }

                if (gen.ForceLauncherExeIgnoreFileCheck)
                {
                    Log("Force ignoring validation check of launcher exe. Will use filepath: " + Path.GetDirectoryName(userGame.ExePath).ToLower());
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
                                Log("ERROR - Could not find " + gen.LauncherExe + " in instance folder, Game executable will be used instead; " + exePath);
                            }
                            else if (launcherFiles.Length == 1)
                            {
                                exePath = launcherFiles[0];
                                origExePath = launcherFiles[0];
                                Log("Found launcher exe at " + exePath + ". This will be used to launch the game");
                            }
                            else
                            {
                                exePath = launcherFiles[0];
                                origExePath = launcherFiles[0];
                                Log("Multiple " + gen.LauncherExe + "'s found in instance folder." + " Using " + exePath + " to launch the game");
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
                    if (context.CustomUserGeneralValues == null || context.CustomUserGeneralValues?.Length < 1)
                    {
                        context.CustomUserGeneralValues = new string[gen.CustomUserGeneralPrompts.Length];
                    }
                    if (customValue == null || customValue?.Length < 1)
                    {
                        customValue = new string[gen.CustomUserGeneralPrompts.Length];
                    }

                    for (int c = 0; c < context.CustomUserGeneralValues.Length; c++)
                    {
                        context.CustomUserGeneralValues[c] = null;
                    }

                    string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(gen.JsFileName) + "\\custom_gen_values.txt");

                    int counter = 0;
                    if (gen.SaveCustomUserGeneralValues || gen.SaveAndEditCustomUserGeneralValues || i > 0)
                    {
                        Log("Handler uses custom general values");
                        if (File.Exists(valueFile))
                        {
                            Log("custom_gen_values.txt already exists for this handler, setting values accordingly");

                            string line;

                            StreamReader file = new StreamReader(valueFile);
                            while ((line = file.ReadLine()) != null)
                            {
                                Log(string.Format("Custom value {0}: {1}", counter, line));
                                customValue[counter] = line;
                                context.CustomUserGeneralValues[counter] = line;
                                counter++;
                            }

                            file.Close();

                            if (counter != gen.CustomUserGeneralPrompts.Length)
                            {
                                Log("Number of lines in file do not match number of prompts. Overwriting file");
                            }
                        }
                        else if (i == 0)
                        {
                            Log("custom_gen_values.txt does not exist. Creating new file at " + valueFile);
                        }
                    }
                    else if (File.Exists(valueFile) && i == 0 && !gen.SaveCustomUserGeneralValues && !gen.SaveAndEditCustomUserGeneralValues)
                    {
                        Log("Deleting value file");
                        File.Delete(valueFile);
                    }

                    if (i == 0 && (!File.Exists(valueFile) || !gen.SaveCustomUserGeneralValues || gen.SaveAndEditCustomUserGeneralValues || (File.Exists(valueFile) && counter != gen.CustomUserGeneralPrompts.Length)))
                    {

                        if (i == 0 && ((File.Exists(valueFile) && !gen.SaveAndEditCustomUserGeneralValues && !gen.SaveCustomUserGeneralValues) || (File.Exists(valueFile) && counter != gen.CustomUserGeneralPrompts.Length)))
                        {
                            Log("Deleting value file");
                            File.Delete(valueFile);
                        }


                        if (!Directory.Exists(Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(gen.JsFileName))))
                        {
                            Directory.CreateDirectory(Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(gen.JsFileName)));
                        }

                        bool containsValue = false;
                        for (int d = 0; d < gen.CustomUserGeneralPrompts.Length; d++)
                        {
                            Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), gen.CustomUserGeneralPrompts.Length, gen.CustomUserGeneralPrompts[d]));
                            string prevAnswer = "";
                            if (d < customValue.Length && File.Exists(valueFile))
                            {
                                prevAnswer = customValue[d];
                            }
                            Forms.CustomPrompt prompt = new Forms.CustomPrompt(gen.CustomUserGeneralPrompts[d], prevAnswer, d);
                            prompt.ShowDialog();
                            if (customValue[d]?.Length > 0)
                            {
                                context.CustomUserGeneralValues[d] = customValue[d];
                                Log("User entered: " + customValue[d]);
                                if (!containsValue)
                                {
                                    containsValue = true;
                                }
                            }
                            else
                            {
                                Log("User did not enter a value for this prompt");
                                context.CustomUserGeneralValues[d] = null;
                            }
                        }

                        if (containsValue)
                        {
                            using (StreamWriter outputFile = new StreamWriter(valueFile))
                            {
                                foreach (string line in customValue)
                                {
                                    outputFile.WriteLine(line);
                                }
                            }
                        }
                    }
                }

                if (gen.CustomUserPlayerPrompts?.Length > 0)
                {
                    if (context.CustomUserPlayerValues == null || context.CustomUserPlayerValues?.Length < 1)
                    {
                        context.CustomUserPlayerValues = new string[gen.CustomUserPlayerPrompts.Length];
                    }
                    if (customValue == null || customValue?.Length < 1)
                    {
                        customValue = new string[gen.CustomUserPlayerPrompts.Length];
                    }

                    string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(gen.JsFileName) + "\\" + player.Nickname + "\\custom_plyr_values.txt");

                    int counter = 0;
                    if (gen.SaveCustomUserPlayerValues || gen.SaveAndEditCustomUserPlayerValues)
                    {
                        Log("Handler uses custom player values");
                        if (File.Exists(valueFile))
                        {
                            Log("custom_plyr_values.txt already exists for this player, setting values accordingly");

                            string line;

                            StreamReader file = new StreamReader(valueFile);
                            while ((line = file.ReadLine()) != null)
                            {
                                Log(string.Format("Custom value {0}: {1}", counter, line));
                                customValue[counter] = line;
                                context.CustomUserPlayerValues[counter] = line;
                                counter++;
                            }

                            file.Close();

                            if (counter != gen.CustomUserPlayerPrompts.Length)
                            {
                                Log("Number of lines in file do not match number of prompts. Overwriting file");
                            }
                        }
                        else
                        {
                            Log("custom_plyr_values.txt does not exist for player " + player.Nickname + ". Creating new file at " + valueFile);
                        }
                    }

                    if (!File.Exists(valueFile) || !gen.SaveCustomUserPlayerValues || gen.SaveAndEditCustomUserPlayerValues || (File.Exists(valueFile) && counter != gen.CustomUserPlayerPrompts.Length))
                    {

                        if ((File.Exists(valueFile) && !gen.SaveAndEditCustomUserPlayerValues && !gen.SaveCustomUserPlayerValues) || (File.Exists(valueFile) && counter != gen.CustomUserPlayerPrompts.Length))
                        {
                            Log("Deleting value file");
                            File.Delete(valueFile);
                        }

                        if (!Directory.Exists(Path.GetDirectoryName(valueFile)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(valueFile));
                        }

                        for (int d = 0; d < gen.CustomUserPlayerPrompts.Length; d++)
                        {
                            Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), gen.CustomUserPlayerPrompts.Length, gen.CustomUserPlayerPrompts[d]));
                            string prevAnswer = "";
                            if (d < customValue.Length && File.Exists(valueFile))
                            {
                                prevAnswer = customValue[d];
                            }
                            Forms.CustomPrompt prompt = new Forms.CustomPrompt(gen.CustomUserPlayerPrompts[d], prevAnswer, d);
                            prompt.ShowDialog();
                            context.CustomUserPlayerValues[d] = customValue[d];
                            Log("User entered: " + customValue[d]);
                        }

                        using (StreamWriter outputFile = new StreamWriter(valueFile))
                        {
                            foreach (string line in customValue)
                            {
                                outputFile.WriteLine(line);
                            }
                        }
                    }

                }

                if (gen.CustomUserInstancePrompts?.Length > 0)
                {

                    if (context.CustomUserInstanceValues == null || context.CustomUserInstanceValues?.Length < 1)
                    {
                        context.CustomUserInstanceValues = new string[gen.CustomUserInstancePrompts.Length];
                    }
                    if (customValue == null || customValue?.Length < 1)
                    {
                        customValue = new string[gen.CustomUserInstancePrompts.Length];
                    }

                    string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(gen.JsFileName) + "\\instance " + i + "\\custom_inst_values.txt");

                    int counter = 0;
                    if (gen.SaveCustomUserInstanceValues || gen.SaveAndEditCustomUserInstanceValues)
                    {
                        Log("Handler uses custom instance values");
                        if (File.Exists(valueFile))
                        {
                            Log("custom_inst_values.txt already exists for this player, setting values accordingly");

                            string line;

                            StreamReader file = new StreamReader(valueFile);
                            while ((line = file.ReadLine()) != null)
                            {
                                Log(string.Format("Custom value {0}: {1}", counter, line));
                                customValue[counter] = line;
                                context.CustomUserInstanceValues[counter] = line;
                                counter++;
                            }

                            file.Close();

                            if (counter != gen.CustomUserInstancePrompts.Length)
                            {
                                Log("Number of lines in file do not match number of prompts. Overwriting file");
                            }
                        }
                        else
                        {
                            Log("custom_inst_values.txt does not exist for player " + player.Nickname + ". Creating new file at " + valueFile);
                        }
                    }

                    if (!File.Exists(valueFile) || !gen.SaveCustomUserInstanceValues || gen.SaveAndEditCustomUserInstanceValues || (File.Exists(valueFile) && counter != gen.CustomUserInstancePrompts.Length))
                    {

                        if ((File.Exists(valueFile) && !gen.SaveAndEditCustomUserInstanceValues && !gen.SaveCustomUserInstanceValues) || (File.Exists(valueFile) && counter != gen.CustomUserInstancePrompts.Length))
                        {
                            Log("Deleting value file");
                            File.Delete(valueFile);
                        }
                        //}

                        if (!Directory.Exists(Path.GetDirectoryName(valueFile)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(valueFile));
                        }

                        for (int d = 0; d < gen.CustomUserInstancePrompts.Length; d++)
                        {
                            Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), gen.CustomUserInstancePrompts.Length, gen.CustomUserInstancePrompts[d]));
                            string prevAnswer = "";
                            if (d < customValue.Length && File.Exists(valueFile))
                            {
                                prevAnswer = customValue[d];
                            }
                            Forms.CustomPrompt prompt = new Forms.CustomPrompt(gen.CustomUserInstancePrompts[d], prevAnswer, d);
                            prompt.ShowDialog();
                            context.CustomUserInstanceValues[d] = customValue[d];
                            Log("User entered: " + customValue[d]);
                        }

                        using (StreamWriter outputFile = new StreamWriter(valueFile))
                        {
                            foreach (string line in customValue)
                            {
                                outputFile.WriteLine(line);
                            }
                        }
                    }

                }

                bool setupDll = true;

                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame && i > 0)
                {
                    setupDll = false;
                }

                if (gen.UseSteamless)
                {
                    Log("Apply Steamless patch for " + gen.ExecutableName);
                    Log("Steamless patch timing =  " + gen.SteamlessTiming + "ms");
                    Tools.Steamless.Steamless steamless = new Tools.Steamless.Steamless();
                    steamless.SteamlessProc(exePath, linkFolder, gen.ExecutableName, gen.SteamlessArgs, gen.SteamlessTiming);
                    Thread.Sleep(gen.SteamlessTiming + 2000);
                }

                if (processingExit)
                {
                    return string.Empty;
                }

                if (gen.GamePlayBeforeGameSetup && !gen.GamePlayAfterLaunch)
                {
                    gen.PrePlay(context, this, player);
                }

                if (gen.HexEditExeAddress?.Length > 0)
                {
                    HexEditExeAddress(exePath, i);
                }

                if (gen.HexEditFileAddress?.Length > 0)
                {
                    HexEditFileAddress(linkFolder, i);
                }

                if (gen.HexEditAllExes?.Length > 0)
                {
                    HexEditAllExes(context, i);
                }

                if (gen.HexEditExe?.Length > 0)
                {
                    HexEditExe(context, i);
                }

                if (gen.HexEditAllFiles?.Length > 0)
                {
                    HexEditAllFiles(context, linkFolder);
                }

                if (gen.HexEditFile?.Length > 0)
                {
                    HexEditFile(context, i, linkFolder);
                }

                if (gen.UseSteamStubDRMPatcher)
                {
                    UseSteamStubDRMPatcher(garch, setupDll);
                }

                if (gen.UseEACBypass)
                {
                    UseEACBypass(linkFolder, setupDll);
                }

                if (gen.UseGoldberg)
                {
                    UseGoldberg(rootFolder, nucleusRootFolder, linkFolder, i, player, players, setupDll);
                }

                if (gen.UseNemirtingasEpicEmu)
                {
                    context.StartArguments += "";
                    UseNemirtingasEpicEmu(rootFolder, linkFolder, gen.UseNemirtingasEpicEmu, i, player, gen.GetEpicLanguage(), setupDll);
                }

                if (gen.UseNemirtingasGalaxyEmu)
                {
                    UseNemirtingasGalaxyEmu(rootFolder, linkFolder, i, player, gen.GetGogLanguage(), setupDll);
                }

                if (gen.CreateSteamAppIdByExe)
                {
                    CreateSteamAppIdByExe(setupDll);
                }

                if (gen.XInputPlusDll?.Length > 0 && !gen.ProcessChangesAtEnd)
                {
                    SetupXInputPlusDll(garch, player, context, i, setupDll);
                }

                if (gen.UseDevReorder && !gen.ProcessChangesAtEnd)
                {
                    UseDevReorder(garch, player, players, i, setupDll);
                }

                if (gen.UseDInputBlocker)
                {
                    UseDInputBlocker(garch, setupDll);
                }

                if (gen.UseX360ce && !gen.ProcessChangesAtEnd)
                {
                    UseX360ce(i, players, player, context, setupDll);
                }

                if (gen.UseDirectX9Wrapper)
                {
                    UseDirectX9Wrapper(setupDll);
                }

                if (gen.CopyCustomUtils?.Length > 0)
                {
                    CopyCustomUtils(i, linkFolder, setupDll);
                }

                if (!string.IsNullOrEmpty(gen.FlawlessWidescreen))
                {
                    UseFlawlessWidescreen(i);
                }

                if (!gen.GamePlayBeforeGameSetup && !gen.GamePlayAfterLaunch)
                {
                    gen.PrePlay(context, this, player);
                }

                if (gen.PauseBetweenContextAndLaunch > 0)
                {
                    Log(string.Format("Pausing for {0} seconds", gen.PauseBetweenContextAndLaunch));
                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseBetweenContextAndLaunch));
                }
                if (gen.AltEpicEmuArgs)
                {
                    Log("Using pre-defined epic emu params");
                    context.StartArguments += " ";
                    context.StartArguments += " -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal " + " -epiclocale=" + gen.GetEpicLanguage() + " -epicusername=" + player.Nickname + " -epicuserid=" + player.Nickname + " ";
                }

                if (gen.EpicEmuArgs)
                {
                    context.StartArguments += " ";
                    Log("Using pre-defined epic emu params");

                    if (!context.StartArguments.Contains("-AUTH_LOGIN"))
                    {
                        Log("Epic Emu parameters not found in arguments. Adding the necessary parameters to existing starting arguments");
                        //AUTH_LOGIN = unused - AUTH_PASSWORD = cdcdcdcdcdcdcdcdcdcdcdcdcdcdcdcd - AUTH_TYPE = exchangecode - epicapp = CrabTest - epicenv = Prod - EpicPortal - epiclocale = en - epicusername =< same username as in the.json > -epicuserid =< same epicid as in the.json >                    
                        context.StartArguments += " -AUTH_LOGIN=unused -AUTH_PASSWORD=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb -AUTH_TYPE=exchangecode -epicapp=CrabTest -epicenv=Prod -EpicPortal" + " -epiclocale=" + gen.GetEpicLanguage() + " -epicusername=" + player.Nickname + " -epicuserid=0000000000000000000000000player" + (i + 1) + " ";
                        //Console.WriteLine(context.StartArguments);
                    }
                }



                string startArgs = context.StartArguments;

                if (!string.IsNullOrEmpty(lobbyConnectArg) && i > 0)
                {
                    startArgs = lobbyConnectArg + " " + startArgs;
                    Log("Goldberg Lobby Connect: Will join lobby ID " + lobbyConnectArg.Substring(15));
                }

                if (context.Hook.CustomDllEnabled && !gen.ProcessChangesAtEnd)
                {
                    CustomDllEnabled(context, player, playerBounds, i, setupDll);
                }

                if ((!gen.UseGoldberg && ini.IniReadValue("Misc", "UseNicksInGame") == "True"))
                {
                    long steamID = random_steam_id + i;

                    bool useSettingsSID = false;

                    if (ini.IniReadValue("SteamIDs", "Player_" + (i + 1)) != "")
                    {
                        Log("Using steam ID from Nucleus settings ");
                        steamID = Convert.ToInt64(ini.IniReadValue("SteamIDs", "Player_" + (i + 1)));//ToString?   
                        useSettingsSID = true;
                    }

                    if (gen.PlayerSteamIDs != null && !useSettingsSID)
                    {
                        if (i < gen.PlayerSteamIDs.Length && !string.IsNullOrEmpty(gen.PlayerSteamIDs[i]))
                        {
                            Log("Using steam ID from handler");
                            steamID = long.Parse(gen.PlayerSteamIDs[i]);
                        }

                    }
                }

                if (gen.GoldbergWriteSteamIDAndAccount)
                {
                    string[] saFiles = Directory.GetFiles(linkFolder, "account_name.txt", SearchOption.AllDirectories);
                    List<string> files = saFiles.ToList();

                    if (saFiles.Length > 0)
                    {
                        Log("account_name.txt file(s) were found in folder. Will update nicknames");
                    }

                    string goldbergNoLocal = $@"{NucleusEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\account_name.txt";


                    if (File.Exists(goldbergNoLocal))
                    {
                        files.Add(goldbergNoLocal);
                    }

                    if (gen.UseNucleusEnvironment)
                    {
                        goldbergNoLocal = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\account_name.txt";
                        if (File.Exists(goldbergNoLocal))
                        {
                            files.Add(goldbergNoLocal);
                        }
                    }

                    foreach (string nameFile in files)
                    {
                        if (!string.IsNullOrEmpty(player.Nickname))
                        {
                            Log(string.Format("Writing nickname {0} in account_name.txt", player.Nickname));
                            File.Delete(nameFile);
                            File.WriteAllText(nameFile, player.Nickname);
                        }
                        else
                        {
                            Log("Writing nickname {0} in account_name.txt " + "Player" + (i + 1));
                            File.Delete(nameFile);
                            File.WriteAllText(nameFile, "Player" + (i + 1));
                        }
                    }

                    saFiles = Directory.GetFiles(linkFolder, "user_steam_id.txt", SearchOption.AllDirectories);
                    files = saFiles.ToList();
                    if (saFiles.Length > 0)
                    {
                        Log("user_steam_id.txt file(s) were found in folder. Will update nicknames");
                    }

                    goldbergNoLocal = $@"{NucleusEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\user_steam_id.txt";
                    if (File.Exists(goldbergNoLocal))
                    {
                        files.Add(goldbergNoLocal);
                    }

                    if (gen.UseNucleusEnvironment)
                    {
                        goldbergNoLocal = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\user_steam_id.txt";
                        if (File.Exists(goldbergNoLocal))
                        {
                            files.Add(goldbergNoLocal);
                        }
                    }

                    foreach (string nameFile in files)
                    {
                        long steamID = random_steam_id + i;

                        bool useSettingsSID = false;

                        if (ini.IniReadValue("SteamIDs", "Player_" + (i + 1)) != "")
                        {
                            Log("Using steam ID from Nucleus settings ");
                            steamID = Convert.ToInt64(ini.IniReadValue("SteamIDs", "Player_" + (i + 1)));//ToString?   
                            useSettingsSID = true;
                        }

                        if (gen.PlayerSteamIDs != null && !useSettingsSID)
                        {
                            if (i < gen.PlayerSteamIDs.Length && !string.IsNullOrEmpty(gen.PlayerSteamIDs[i]))
                            {
                                Log("Using steam ID from handler");
                                steamID = long.Parse(gen.PlayerSteamIDs[i]);
                            }

                        }

                        Log("Generating user_steam_id.txt with user steam ID " + (steamID).ToString());
                        File.Delete(nameFile);
                        File.WriteAllText(nameFile, (steamID).ToString());
                    }
                }

                if (gen.ChangeIPPerInstance && !gen.ProcessChangesAtEnd)
                {
                    ChangeIPPerInstance(i);
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

                if (context.NeedsSteamEmulation)
                {
                    string steamEmu = Path.Combine(linkFolder, "SmartSteamLoader");
                    ssePath = steamEmu;
                    string sourcePath = Path.Combine(GameManager.Instance.GetUtilsPath(), "SmartSteamEmu");

                    if (setupDll)
                    {
                        Log("Setting up SmartSteamEmu");

                        Log(string.Format("Copying SmartSteamEmu files to {0}", steamEmu));
                        try
                        {
                            //Create all of the directories
                            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                                SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(dirPath.Replace(sourcePath, steamEmu));
                            }

                            //Copy all the files & Replaces any files with the same name
                            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(newPath, newPath.Replace(sourcePath, steamEmu), true);
                            }
                        }
                        catch
                        {
                            Log(string.Format("ERROR - Copying of SmartSteamEmu files failed!"));
                            return "Extraction of SmartSteamEmu failed!";
                        }
                    }

                    string sseLoader = string.Empty;
                    if (gameIs64)
                    {
                        sseLoader = "SmartSteamLoader_x64.exe";
                    }
                    else
                    {
                        sseLoader = "SmartSteamLoader.exe";
                    }

                    string emuExe = Path.Combine(steamEmu, sseLoader);
                    string emuIni = Path.Combine(steamEmu, "SmartSteamEmu.ini");
                    IniFile emu = new IniFile(emuIni);

                    Log("Writing SmartSteamEmu.ini");
                    emu.IniWriteValue("Launcher", "Target", exePath);
                    emu.IniWriteValue("Launcher", "StartIn", Path.GetDirectoryName(exePath));
                    emu.IniWriteValue("Launcher", "CommandLine", startArgs);
                    emu.IniWriteValue("Launcher", "SteamClientPath", Path.Combine(steamEmu, "SmartSteamEmu.dll"));
                    emu.IniWriteValue("Launcher", "SteamClientPath64", Path.Combine(steamEmu, "SmartSteamEmu64.dll"));
                    emu.IniWriteValue("Launcher", "InjectDll", "1");

                    emu.IniWriteValue("SmartSteamEmu", "AppId", context.SteamID);
                    emu.IniWriteValue("SmartSteamEmu", "SteamIdGeneration", "Manual");

                    if (ini.IniReadValue("SteamIDs", "Player_" + (i + 1)) != "")
                    {
                        emu.IniWriteValue("SmartSteamEmu", "ManualSteamId", ini.IniReadValue("SteamIDs", "Player_" + (i + 1)).ToString());//ToString?   
                    }
                    else
                    {
                        emu.IniWriteValue("SmartSteamEmu", "ManualSteamId", (random_steam_id + i).ToString());
                    }

                    string lang = "english";
                    if (ini.IniReadValue("Misc", "SteamLang") != "" && ini.IniReadValue("Misc", "SteamLang") != "Automatic")
                    {
                        gen.GoldbergLanguage = ini.IniReadValue("Misc", "SteamLang").ToLower();
                    }
                    if (gen.GoldbergLanguage?.Length > 0)
                    {
                        lang = gen.GoldbergLanguage;
                    }
                    else
                    {
                        lang = gen.GetSteamLanguage();
                    }
                    emu.IniWriteValue("SmartSteamEmu", "Language", lang);

                    if (ini.IniReadValue("Misc", "UseNicksInGame") == "True" && !string.IsNullOrEmpty(player.Nickname))
                    {
                        emu.IniWriteValue("SmartSteamEmu", "PersonaName", player.Nickname);
                    }
                    else
                    {
                        emu.IniWriteValue("SmartSteamEmu", "PersonaName", "Player" + (i + 1));
                    }

                    emu.IniWriteValue("SmartSteamEmu", "DisableOverlay", "1");
                    emu.IniWriteValue("SmartSteamEmu", "SeparateStorageByName", "1");

                    if (gen.SSEAdditionalLines?.Length > 0)
                    {
                        foreach (string line in gen.SSEAdditionalLines)
                        {
                            if (line.Contains("|") && line.Contains("="))
                            {
                                string[] lineSplit = line.Split('|', '=');
                                if (lineSplit?.Length == 3)
                                {
                                    string section = lineSplit[0];
                                    string key = lineSplit[1];
                                    string value = lineSplit[2];
                                    Log(string.Format("Writing custom line in SSE, section: {0}, key: {1}, value: {2}", section, key, value));
                                    emu.IniWriteValue(section, key, value);
                                }
                            }
                        }
                    }

                    if (gen.ForceEnvironmentUse && gen.ThirdPartyLaunch)
                    {
                        Log("Force Nucleus environment use");

                        IntPtr envPtr = IntPtr.Zero;

                        if (gen.UseNucleusEnvironment)
                        {
                            Log("Setting up Nucleus environment");
                            var sb = new StringBuilder();
                            IDictionary envVars = Environment.GetEnvironmentVariables();
                            string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                            envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                            envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                            envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                            envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                            //Some games will crash if the directories don't exist
                            Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                            Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                            Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                            Directory.CreateDirectory(envVars["APPDATA"].ToString());
                            Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
                            Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                            if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                            {
                                if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                {
                                    ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                }

                                RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                            }

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
                    }

                    if (!gen.ThirdPartyLaunch)
                    {
                        if (context.KillMutex?.Length > 0)
                        {
                            // to kill the mutexes we need to orphanize the process
                            while (!ProcessUtil.RunOrphanProcess(emuExe, gen.UseNucleusEnvironment, player.Nickname))
                            {
                                Thread.Sleep(25);
                            }
                            Log("Terminal session launched SmartSteamEmu as an orphan in order to kill mutexes in future");
                        }
                        else
                        {

                            IntPtr envPtr = IntPtr.Zero;

                            if (gen.UseNucleusEnvironment)
                            {
                                Log("Setting up Nucleus environment");
                                var sb = new StringBuilder();
                                IDictionary envVars = Environment.GetEnvironmentVariables();
                                string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                                envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                                envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                                //Some games will crash if the directories don't exist
                                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                                Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                                Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                                Directory.CreateDirectory(envVars["APPDATA"].ToString());
                                Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
                                Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                {
                                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                    {
                                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                    }

                                    RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                    dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                }

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

                            STARTUPINFO startup = new STARTUPINFO();
                            startup.cb = Marshal.SizeOf(startup);

                            bool success = CreateProcess(null, emuExe, IntPtr.Zero, IntPtr.Zero, false, (uint)ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, envPtr, Path.GetDirectoryName(exePath), ref startup, out PROCESS_INFORMATION processInformation);

                            if (!success)
                            {
                                int error = Marshal.GetLastWin32Error();
                                Log(string.Format("ERROR {0} - CreateProcess failed - startGamePath: {1}, startArgs: {2}, dirpath: {3}", error, exePath, startArgs, Path.GetDirectoryName(exePath)));
                            }

                            proc = Process.GetProcessById(processInformation.dwProcessId);
                            Log(string.Format("Started process {0} (pid {1})", proc.ProcessName, proc.Id));
                        }
                    }
                    else
                    {
                        Log("Skipping launching of game via Nucleus for third party launch");
                    }

                    Log("SmartSteamEmu setup complete");

                    proc = null;
                    Thread.Sleep(5000);
                }
                else
                {
                    if (gen.ForceEnvironmentUse && gen.ThirdPartyLaunch)
                    {
                        Log("Force Nucleus environment use");

                        IntPtr envPtr = IntPtr.Zero;

                        if (gen.UseNucleusEnvironment)
                        {
                            Log("Setting up Nucleus environment");
                            var sb = new StringBuilder();
                            IDictionary envVars = Environment.GetEnvironmentVariables();
                            string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                            envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                            envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                            envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                            envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                            //Some games will crash if the directories don't exist
                            Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                            Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                            Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                            Directory.CreateDirectory(envVars["APPDATA"].ToString());
                            Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
                            Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                            if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                            {
                                if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                {
                                    ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                }

                                RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                            }

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
                                Log("Setting up Nucleus environment");
                                StringBuilder sb = new StringBuilder();
                                IDictionary envVars = Environment.GetEnvironmentVariables();
                                string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                                envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                                envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                                //Some games will crash if the directories don't exist
                                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                                Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                                Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                                Directory.CreateDirectory(envVars["APPDATA"].ToString());
                                Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
                                Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                {
                                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                    {
                                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                    }

                                    RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                    dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                }

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

                            ProtoInputLauncher.InjectStartup(exePath,
                                startArgs, 0, nucleusRootFolder, i + 1, gen, player, out uint pid, envPtr,
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
                                gen.HookInit, gen.HookInitDelay, gen.RenameNotKillMutex, mu, gen.SetWindowHookStart, isDebug, nucleusRootFolder, gen.BlockRawInput, gen.UseNucleusEnvironment, player.Nickname, startupHooksEnabled, gen.CreateSingleDeviceFile, player.RawHID, player.MonitorBounds.Width, player.MonitorBounds.Height, player.MonitorBounds.X
                                , player.MonitorBounds.Y, DocumentsRoot, useDocs);

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

                                    cmd.StartInfo.EnvironmentVariables["APPDATA"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                    cmd.StartInfo.EnvironmentVariables["LOCALAPPDATA"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Local";
                                    cmd.StartInfo.EnvironmentVariables["USERPROFILE"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}";
                                    cmd.StartInfo.EnvironmentVariables["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath") + $@"\NucleusCoop\{player.Nickname}";

                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop");
                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}");
                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\Documents");
                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Local");

                                    Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                    if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                    {
                                        if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                        {
                                            ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                        }

                                        RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);

                                        if (key.GetValue("Personal").ToString() != "%USERPROFILE%\\Documents")
                                        {
                                            key.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }
                                    }

                                }
                                else if (gen.UseCurrentUserEnvironment)
                                {
                                    cmd.StartInfo.EnvironmentVariables["APPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                    cmd.StartInfo.EnvironmentVariables["LOCALAPPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                                    cmd.StartInfo.EnvironmentVariables["USERPROFILE"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                    cmd.StartInfo.EnvironmentVariables["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath");

                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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
                                    Log("Setting up Nucleus environment");
                                    envVars["USERPROFILE"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}";
                                    envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                                    envVars["APPDATA"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                    envVars["LOCALAPPDATA"] = NucleusEnvironmentRoot + $@"\NucleusCoop\{player.Nickname}\AppData\Local";

                                    //Some games will crash if the directories don't exist
                                    Directory.CreateDirectory(NucleusEnvironmentRoot + $@"\NucleusCoop");
                                    Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                                    Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                                    Directory.CreateDirectory(envVars["APPDATA"].ToString());
                                    Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
                                    Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                    if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                    {
                                        if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                        {
                                            ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                        }

                                        RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                        dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                    }

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
                                else if (gen.UseCurrentUserEnvironment)
                                {
                                    Log("Setting environment to current user");

                                    envVars["APPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); //$@"C:\Users\{username}\AppData\Roaming";
                                    envVars["LOCALAPPDATA"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //$@"C:\Users\{username}\AppData\Local";
                                    envVars["USERPROFILE"] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); //$@"C:\Users\{username}\";
                                    envVars["HOMEPATH"] = Environment.GetEnvironmentVariable("homepath"); //$@"\Users\{username}\";

                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)); //$@"C:\Users\{username}");
                                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));//$@"C:\Users\{username}\Documents");
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

                                STARTUPINFO startup = new STARTUPINFO();
                                startup.cb = Marshal.SizeOf(startup);

                                bool success = CreateProcessWithLogonW($"nucleusplayer{i + 1}", Environment.UserDomainName, nucleusUserAccountsPassword, LogonFlags.LOGON_WITH_PROFILE, null, exePath + " " + startArgs, ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, (uint)envPtr, Path.GetDirectoryName(exePath), ref startup, out PROCESS_INFORMATION processInformation);
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
                            else if (gen.CMDLaunch /*&& i >= 1*/ || (gen.UseForceBindIP && i > 0))
                            {
                                string[] cmdOps = gen.CMDOptions;
                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.UseShellExecute = false;

                                if (gen.UseForceBindIP)
                                {
                                    cmd.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                                }

                                cmd.Start();

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

                                        cmd.StandardInput.WriteLine($@"set APPDATA={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        cmd.StandardInput.WriteLine($@"set LOCALAPPDATA={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        cmd.StandardInput.WriteLine($@"set USERPROFILE={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        cmd.StandardInput.WriteLine($@"set HOMEPATH=\Users\{username}\NucleusCoop\{player.Nickname}");

                                        //Some games will crash if the directories don't exist
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        //Directory.CreateDirectory($@"\Users\{username}\NucleusCoop\{player.Nickname}");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\Documents");

                                        Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                        if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                        {
                                            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                            {
                                                ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                            }

                                            RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                            dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0 || gen.CMDBatchAfter?.Length > 0)
                                    {
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_EXE=" + Path.GetFileName(exePath));
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_INST_EXE_FOLDER=" + Path.GetDirectoryName(exePath));
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_INST_FOLDER=" + linkFolder);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_FOLDER=" + nucleusFolderPath);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_ORIG_EXE_FOLDER=" + exeFolder);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_ORIG_FOLDER=" + exeFolder.Substring(0, (exeFolder.Length - gen.BinariesFolder.Length)));
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0)
                                    {
                                        for (int x = 0; x < gen.CMDBatchBefore.Length; x++)
                                        {
                                            if (!gen.CMDBatchBefore[x].Contains("|"))
                                            {
                                                Log("Running command line: " + gen.CMDBatchBefore[x]);
                                                cmd.StandardInput.WriteLine(gen.CMDBatchBefore[x]);
                                            }
                                            else
                                            {
                                                string[] clineSplit = gen.CMDBatchBefore[x].Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    cmd.StandardInput.WriteLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    string cmdLine = "\"" + exePath + "\" " + startArgs;
                                    if (!gen.CMDStartArgsInside)
                                    {
                                        cmdLine = "\"" + exePath + " " + startArgs + "\"";
                                    }
                                    if (cmdOps?.Length > 0 && i < cmdOps.Length)
                                    {
                                        cmdLine = cmdOps[i] + " \"" + exePath + "\" " + startArgs;
                                        if (!gen.CMDStartArgsInside)
                                        {
                                            cmdLine = cmdOps[i] + " \"" + exePath + " " + startArgs + "\"";
                                        }
                                    }

                                    if (gen.PauseCMDBatchBefore > 0)
                                    {
                                        Log(string.Format("Pausing for {0} seconds", gen.PauseCMDBatchBefore));
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchBefore));
                                    }

                                    Log(string.Format("Launching game via command prompt with the following line: {0}", cmdLine));
                                    cmd.StandardInput.WriteLine(cmdLine);

                                    if (gen.CMDBatchAfter?.Length > 0)
                                    {
                                        for (int x = 0; x < gen.CMDBatchAfter.Length; x++)
                                        {
                                            if (!gen.CMDBatchAfter[x].Contains("|"))
                                            {
                                                Log("Running command line: " + gen.CMDBatchAfter[x]);
                                                cmd.StandardInput.WriteLine(gen.CMDBatchAfter[x]);
                                            }
                                            else
                                            {
                                                string[] clineSplit = gen.CMDBatchAfter[x].Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    cmd.StandardInput.WriteLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    if (gen.PauseCMDBatchAfter > 0)
                                    {
                                        Log(string.Format("Pausing for {0} seconds", gen.PauseCMDBatchAfter));
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchAfter));
                                    }
                                }
                                else
                                {
                                    string forceBindexe = string.Empty;

                                    if (gameIs64)
                                    {
                                        forceBindexe = "ForceBindIP64.exe";
                                    }
                                    else //if (Is64Bit(exePath) == false)
                                    {
                                        forceBindexe = "ForceBindIP.exe";
                                    }

                                    if (gen.UseNucleusEnvironment)
                                    {
                                        Log("Setting up Nucleus environment");
                                        //var username = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"C:\Users\", "");
                                        string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                        cmd.StandardInput.WriteLine($@"set APPDATA={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        cmd.StandardInput.WriteLine($@"set LOCALAPPDATA={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        cmd.StandardInput.WriteLine($@"set USERPROFILE={NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        cmd.StandardInput.WriteLine($@"set HOMEPATH=\Users\{username}\NucleusCoop\{player.Nickname}");

                                        //Some games will crash if the directories don't exist
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}");
                                        //Directory.CreateDirectory($@"\Users\{username}\NucleusCoop\{player.Nickname}");
                                        Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\Documents");

                                        Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                        if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                        {
                                            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                            {
                                                ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                            }

                                            RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                            dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                        }

                                    }

                                    if (gen.CMDBatchBefore?.Length > 0 || gen.CMDBatchAfter?.Length > 0)
                                    {
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_EXE=" + Path.GetFileName(exePath));
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_INST_EXE_FOLDER=" + Path.GetDirectoryName(exePath));
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_INST_FOLDER=" + linkFolder);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_FOLDER=" + nucleusFolderPath);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_ORIG_EXE_FOLDER=" + exeFolder);
                                        cmd.StandardInput.WriteLine($@"set NUCLEUS_ORIG_FOLDER=" + exeFolder.Substring(0, (exeFolder.Length - gen.BinariesFolder.Length)));
                                    }

                                    if (gen.CMDBatchBefore?.Length > 0)
                                    {

                                        for (int x = 0; x < gen.CMDBatchBefore.Length; x++)
                                        {
                                            if (!gen.CMDBatchBefore[x].Contains("|"))
                                            {
                                                Log("Running command line: " + gen.CMDBatchBefore[x]);
                                                cmd.StandardInput.WriteLine(gen.CMDBatchBefore[x]);
                                            }
                                            else
                                            {
                                                string[] clineSplit = gen.CMDBatchBefore[x].Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    cmd.StandardInput.WriteLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }

                                    if (gen.PauseCMDBatchBefore > 0)
                                    {
                                        Log(string.Format("Pausing for {0} seconds", gen.PauseCMDBatchBefore));
                                        Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchBefore));
                                    }

                                    string iParam = string.Empty;

                                    if (gen.ForceBindIPDelay)
                                    {
                                        iParam = "-i ";
                                    }

                                    string dummy = " dummy";

                                    if (gen.ForceBindIPNoDummy)
                                    {
                                        dummy = string.Empty;
                                    }

                                    string cmdLine = "\"" + Path.Combine(GameManager.Instance.GetUtilsPath(), "ForceBindIP\\" + forceBindexe) + "\" " + iParam + "127.0.0." + (i + 2) + " \"" + exePath + "\"" + dummy + startArgs;
                                    // string cmdLine = "\"" + Path.Combine(GameManager.Instance.GetUtilsPath(), "ForceBindIP\\" + forceBindexe) + "\" " + iParam + "127.0.0." + (i + 2) + " \"" + exePath + "\" dummy" + startArgs;
                                    Log(string.Format("Launching game using ForceBindIP command line argument: {0}", cmdLine));
                                    cmd.StandardInput.WriteLine(cmdLine);

                                    if (gen.CMDBatchAfter?.Length > 0)
                                    {
                                        for (int x = 0; x < gen.CMDBatchAfter.Length; x++)
                                        {
                                            if (!gen.CMDBatchAfter[x].Contains("|"))
                                            {
                                                Log("Running command line: " + gen.CMDBatchAfter[x]);
                                                cmd.StandardInput.WriteLine(gen.CMDBatchAfter[x]);
                                            }
                                            else
                                            {
                                                string[] clineSplit = gen.CMDBatchAfter[x].Split('|');
                                                if (clineSplit[0] == i.ToString())
                                                {
                                                    Log("Running command line: " + clineSplit[1]);
                                                    cmd.StandardInput.WriteLine(clineSplit[1]);
                                                }
                                            }
                                        }
                                    }
                                }

                                proc = null;
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();

                                if (gen.PauseCMDBatchAfter > 0)
                                {
                                    Log(string.Format("Pausing for {0} seconds", gen.PauseCMDBatchAfter));
                                    Thread.Sleep(TimeSpan.FromSeconds(gen.PauseCMDBatchAfter));
                                }
                            }
                            else
                            {
                                IntPtr envPtr = IntPtr.Zero;

                                if (gen.UseNucleusEnvironment)
                                {
                                    Log("Setting up Nucleus environment");
                                    var sb = new StringBuilder();
                                    IDictionary envVars = Environment.GetEnvironmentVariables();
                                    //var username = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"C:\Users\", "");
                                    string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                                    envVars["USERPROFILE"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                                    envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                                    envVars["APPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                                    envVars["LOCALAPPDATA"] = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                                    //Some games will crash if the directories don't exist
                                    Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                                    Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                                    Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                                    Directory.CreateDirectory(envVars["APPDATA"].ToString());
                                    Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());

                                    Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

                                    if (gen.DocumentsConfigPath?.Length > 0 || gen.DocumentsSavePath?.Length > 0)
                                    {
                                        if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                                        {
                                            ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                                        }

                                        RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                                        dkey.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
                                    }

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

                                STARTUPINFO startup = new STARTUPINFO();
                                startup.cb = Marshal.SizeOf(startup);

                                bool success = CreateProcess(null, exePath + " " + startArgs, IntPtr.Zero, IntPtr.Zero, false, (uint)ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, envPtr, Path.GetDirectoryName(exePath), ref startup, out PROCESS_INFORMATION processInformation);
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
                    gen.PrePlay(context, this, player);
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
                        ProcessEnd();

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
                            //MessageBox.Show("Press OK when ready to launch instance " + (i + 2) + ".", "Nucleus - Prompt Between Instances", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
                        }
                    }
                    Thread.Sleep(10000);
                }

                if ((proc != null && !Process.GetProcesses().Any(x => x.Id == proc.Id)) || gen.ForceProcessSearch || gen.NeedsSteamEmulation || gen.ForceProcessPick || proc == null || gen.CMDLaunch || gen.UseForceBindIP || gen.GameName == "Halo Custom Edition" || (proc != null && !IsRunning(proc)) /*|| gen.LauncherExe?.Length > 0*/)
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
                        proc = LaunchProcessPick(player);
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

                if (!IsRunning(proc))
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
                        string[] writeSplit = gen.WriteToProcessMemory.Split('|');
                        long baseAddr = long.Parse(writeSplit[0], NumberStyles.HexNumber);

                        List<byte> bytesConv = new List<byte>();
                        for (int s = 0; s < writeSplit[1].Length; s += 2)
                        {
                            bytesConv.Add(Convert.ToByte(writeSplit[1].Substring(s, 2), 16));
                        }
                        byte[] bArray = bytesConv.ToArray();

                        bool result = WriteProcessMemory(proc.NucleusGetMainWindowHandle(), baseAddr, bArray, (ulong)bArray.Length, out _);
                        Log(string.Format("WriteToProcessMemory - baseaddr: {0} result: {1}", baseAddr, result));
                    }
                }

                if (gen.GoldbergLobbyConnect && i == 0)
                {
                    GoldbergLobbyConnect();
                }

                if (i > 0 && gen.ResetWindows && prevProcessData != null)
                {
                    ResetWindows(prevProcessData, prevWindowX, prevWindowY, prevWindowWidth, prevWindowHeight, i);
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
                first = false;

                if (gen.ProcessorPriorityClass?.Length > 0)
                {
                    Log(string.Format("Setting process priority to {0}", gen.ProcessorPriorityClass));
                    switch (gen.ProcessorPriorityClass)
                    {
                        case "AboveNormal":
                            proc.PriorityClass = ProcessPriorityClass.AboveNormal;
                            break;
                        case "High":
                            proc.PriorityClass = ProcessPriorityClass.High;
                            break;
                        case "RealTime":
                            proc.PriorityClass = ProcessPriorityClass.RealTime;
                            break;
                        default:
                            proc.PriorityClass = ProcessPriorityClass.Normal;
                            break;
                    }
                }

                if (gen.IdealProcessor > 0)
                {
                    Log(string.Format("Setting ideal processor to {0}", gen.IdealProcessor));
                    ProcessThreadCollection threads = proc.Threads;
                    for (int t = 0; t < threads.Count; t++)
                    {
                        threads[t].IdealProcessor = (gen.IdealProcessor + 1);
                    }
                }

                if ((gen.UseProcessor != null ? (gen.UseProcessor.Length > 0 ? 1 : 0) : 0) != 0)
                {
                    Log(string.Format("Assigning processors {0}", gen.UseProcessor));
                    string[] strArray = gen.UseProcessor.Split(',');
                    int num2 = 0;
                    foreach (string s in strArray)
                    {
                        num2 |= 1 << int.Parse(s) - 1;
                    }

                    proc.ProcessorAffinity = (IntPtr)num2;
                }
                else
                {
                    string[] processorsPerInstance = gen.UseProcessorsPerInstance;
                    if ((processorsPerInstance != null ? ((uint)processorsPerInstance.Length > 0U ? 1 : 0) : 0) != 0)
                    {
                        foreach (string str7 in gen.UseProcessorsPerInstance)
                        {
                            int num2 = int.Parse(str7.Split('|')[0]);
                            string str8 = str7.Split('|')[1];
                            if (num2 == i + 1)
                            {
                                Log(string.Format("Assigning processors {0} for instance {1}", (object)str8, i));
                                string[] strArray = str8.Split(',');
                                int num3 = 0;
                                foreach (string s in strArray)
                                {
                                    num3 |= 1 << int.Parse(s) - 1;
                                }

                                proc.ProcessorAffinity = (IntPtr)num3;
                            }
                        }
                    }
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
                        SetWindowText(proc.NucleusGetMainWindowHandle(), windowTitle);
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
                        nucleusFolderPath,
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
                        Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to launch the rest of the instances.");
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
                    Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to launch instance " + (i + 2) + ".");
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
                    Forms.Prompt prompt = new Forms.Prompt("Press OK when ready to install hooks and/or start sending fake messages.");
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

                if (!IsRunning(proc))
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
                    var window = CreateRawInputWindow(proc, player);

                    nextWindowToInject = window;
                }

                if (i == (players.Count - 1))
                {
                    if (processingExit)
                    {
                        return string.Empty;
                    }

                    Log("All instances accounted for, performing final preperations");

                    for (int pi = 0; pi < players.Count; pi++)
                    {
                        if (ini.IniReadValue("Audio", "Custom") == "1" && ini.IniReadValue("Audio", "AudioInstance" + (pi + 1)) != "" && ini.IniReadValue("Audio", "AudioInstance" + (pi + 1)) != "Default")
                        {
                            Log($"Attempting to switch audio endpoint for process {players[pi].ProcessData.Process.ProcessName} pid ({players[pi].ProcessID}) to DeviceID {ini.IniReadValue("Audio", "AudioInstance" + (pi + 1))}");
                            Thread.Sleep(1000);
                            SwitchProcessTo(ini.IniReadValue("Audio", "AudioInstance" + (pi + 1)), ERole.ERole_enum_count, EDataFlow.eRender, (uint)players[pi].ProcessID);
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
                        }
                    }

                    Thread.Sleep(1000);

                    if (gen.ResetWindows)
                    {
                        ResetWindows(data, prevWindowX, prevWindowY, prevWindowWidth, prevWindowHeight, i + 1);
                    }

                    if (gen.FakeFocus)
                    {
                        Log("Start sending fake focus messages every 1000 ms");
                        fakeFocus = new Thread(SendFocusMsgs);
                        fakeFocus.Start();
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
                                SetWindowText(aproc.NucleusGetMainWindowHandle(), gen.Hook.ForceFocusWindowName);
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
                                SetWindowText(fwProc.NucleusGetMainWindowHandle(), windowTitle);
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
                                    InjectDLLs(data.Process, nextWindowToInject, players[i]);
                                }
                            }
                        }
                        else
                        {
                            Log("Injecting hook DLL for last instance");
                            User32Interop.SetForegroundWindow(data.Process.NucleusGetMainWindowHandle());
                            InjectDLLs(data.Process, nextWindowToInject, players[i]);
                        }
                    }

                    if (!gen.IgnoreWindowBorderCheck)
                    {
                        foreach (PlayerInfo plyr in players)
                        {
                            Thread.Sleep(1000);

                            Process plyrProc = plyr.ProcessData.Process;

                            if (!gen.DontRemoveBorders)
                            {
                                const int flip = 0x00C00000 | 0x00080000 | 0x00040000; //WS_BORDER | WS_SYSMENU

                                var x = (int)User32Interop.GetWindowLong(plyrProc.NucleusGetMainWindowHandle(), User32_WS.GWL_STYLE);
                                if ((x & flip) > 0)//has a border
                                {
                                    Log("Process id " + plyrProc.Id + ", still has or regained a border, trying to remove it");
                                    x &= (~flip);
                                    ResetWindows(plyr.ProcessData, plyr.ProcessData.Position.X, plyr.ProcessData.Position.Y, plyr.ProcessData.Size.Width, plyr.ProcessData.Size.Height, plyr.PlayerID + 1);
                                }
                            }

                            if (gen.WindowStyleEndChanges?.Length > 0 || gen.ExtWindowStyleEndChanges?.Length > 0)
                            {
                                Thread.Sleep(1000);
                                WindowStyleChanges(plyr.ProcessData, i);
                            }

                            if (gen.EnableWindows)
                            {
                                EnableWindow(plyr.ProcessData.HWnd.NativePtr, true);
                            }
                        }
                    }

                    // Fake mouse cursors
                    if (gen.DrawFakeMouseCursor)//&& gen.SupportsMultipleKeyboardsAndMice)
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
                        IntPtr nucHwnd = User32Interop.FindWindow(null, "Nucleus Co-op");

                        if (nucHwnd != IntPtr.Zero)
                        {
                            User32Interop.SetForegroundWindow(nucHwnd);
                        }
                        else
                        {
                            Log("ERROR - Could not obtain the window handle for Nucleus");
                        }
                    }

                    if (gen.SendFakeFocusMsg)
                    {
                        foreach (PlayerInfo plyr in players)
                        {
                            Thread.Sleep(1000);

                            Process plyrProc = plyr.ProcessData.Process;

                            if (gen.FakeFocusSendActivate)
                            {
                                User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATE, (IntPtr)0x00000002, IntPtr.Zero);
                            }

                            User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATEAPP, (IntPtr)1, IntPtr.Zero);
                            User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_NCACTIVATE, (IntPtr)0x00000001, IntPtr.Zero);
                            User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                            User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_MOUSEACTIVATE, (IntPtr)plyrProc.NucleusGetMainWindowHandle(), (IntPtr)1);
                        }
                    }

                }

            }

            if (gen.LockInputAtStart)
            {
                Thread.Sleep(5000);
                if (gen.ToggleUnfocusOnInputsLock)
                {
                    IntPtr nucHwnd = User32Interop.FindWindow(null, "Nucleus Co-op");
                    if (nucHwnd != IntPtr.Zero)
                    {
                        User32Interop.SetForegroundWindow(nucHwnd);
                    }
                }

                LockInput.Lock(gen.LockInputSuspendsExplorer, gen.ProtoInput.FreezeExternalInputWhenInputNotLocked, gen?.ProtoInput);
            }

            // Call the input lock/unlock callbacks, just in case they haven't been called with the players fully setup
            if (LockInput.IsLocked)
            {
                gen.ProtoInput.OnInputLocked?.Invoke();
            }
            else
            {
                gen.ProtoInput.OnInputUnlocked?.Invoke();
            }

            if (gen.SetTopMostAtEnd)
            {
                if (!gen.PromptBetweenInstances)
                {
                    if (!gen.NotTopMost)
                    {
                        for (int i = 0; i < players.Count; i++)
                        {
                            PlayerInfo p = players[i];
                            ProcessData data = p.ProcessData;


                            Log("Set game window to top most");
                            data.HWnd.TopMost = true;
                        }
                    }
                }
            }

            if (gen.UseNucleusEnvironment)
            {
                string[] environmentRegFile = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);

                if (environmentRegFile.Length > 0)
                {
                    Log("Restoring default user environment path");

                    foreach (string environmentRegFilePath in environmentRegFile)
                    {
                        if (environmentRegFilePath.Contains("User Shell Folders"))
                        {
                            Process proc = new Process();

                            try
                            {
                                proc.StartInfo.FileName = "reg.exe";
                                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                proc.StartInfo.CreateNoWindow = true;
                                proc.StartInfo.UseShellExecute = false;

                                string command = "import \"" + environmentRegFilePath + "\"";
                                proc.StartInfo.Arguments = command;
                                proc.Start();

                                proc.WaitForExit();
                                Log($"Imported {Path.GetFileName(environmentRegFilePath)}");
                            }
                            catch (Exception)
                            {
                                proc.Dispose();
                            }
                        }
                    }
                }

            }


            Log("All done!");
            try
            {
                if (statusWinThread != null && statusWinThread.IsAlive)
                {
                    Thread.Sleep(5000);
                    if (statusWinThread != null && statusWinThread.IsAlive)
                    {
                        statusWinThread.Abort();
                    }
                }
            }
            catch { }

            gen.OnFinishedSetup?.Invoke();

            return string.Empty;
        }

        private void ShowInstanceInfo(PlayerInfo player, int i)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                Form info = new Form();
                info.Size = new Size(1000, 90);
                info.StartPosition = FormStartPosition.CenterScreen;
                info.FormBorderStyle = FormBorderStyle.None;
                info.ShowInTaskbar = false;
                info.TopMost = true;
                info.BackColor = Color.Black;
                info.TransparencyKey = Color.Black;
                info.Opacity = 0.8D;

                TextBox playerInfo = new TextBox();
                playerInfo.Text = "Starting " + gen.GameName + " instance for " + player.Nickname + " as Player #" + (i + 1);
                playerInfo.Size = new Size(info.Width, info.Height);
                playerInfo.Location = new Point(info.Width / 2 - playerInfo.Width / 2, info.Height / 2 - playerInfo.Height / 2);
                playerInfo.TextAlign = HorizontalAlignment.Center;
                playerInfo.Font = new Font("Franklin Gothic", 16.0F, FontStyle.Regular);
                playerInfo.ForeColor = Color.GreenYellow;
                playerInfo.BackColor = Color.Black;
                playerInfo.BorderStyle = BorderStyle.None;
                playerInfo.SelectionStart = playerInfo.Text.Length;

                info.Controls.Add(playerInfo);
                info.Show();

                long timer = (long)8e+9;

                while (timer > 0)
                {
                    timer -= (long)60000;
                    info.BringToFront();
                    info.TopMost = true;
                }

                info.Dispose();
            });
        }

        private void WindowStyleChanges(ProcessData processData, int i)
        {
            try
            {
                Log("WindowStyleChanges called");
                if (gen.WindowStyleEndChanges?.Length > 0)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE);
                    Log("Using user custom window style");
                    foreach (string val in gen.WindowStyleEndChanges)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                    User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);
                }

                if (gen.ExtWindowStyleEndChanges?.Length > 0)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);
                    Log("Using user custom extended window style");
                    foreach (string val in gen.ExtWindowStyleEndChanges)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                    User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);
                }


                User32Interop.SetWindowPos(processData.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
            }
            catch (Exception ex)
            {
                Log("ERROR - Exception in WindowStyleChanges for instance " + i + ". " + ex.Message);
            }
        }

        private Window CreateRawInputWindow(Process proc, PlayerInfo player)
        {
            Log("Creating raw input window");
            var hWnd = WaitForProcWindowHandleNotZero(proc);
            var mouseHdev = player.IsRawKeyboard ? player.RawMouseDeviceHandle : (IntPtr)(-1);
            var keyboardHdev = player.IsRawMouse ? player.RawKeyboardDeviceHandle : (IntPtr)(-1);

            var window = new Window(hWnd)
            {
                CursorVisibility = player.IsRawMouse && !gen.HideCursor && gen.DrawFakeMouseCursor,
                KeyboardAttached = keyboardHdev,
                MouseAttached = mouseHdev
            };

            window.CreateHookPipe(gen);

            RawInputManager.windows.Add(window);
            return window;
        }

        private static bool IsRunning(Process process)
        {
            try { Process.GetProcessById(process.Id); }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
            return true;
        }

        private void DeleteFiles(string linkFolder, int i)
        {
            foreach (string deleteLine in gen.DeleteFiles)
            {
                string[] deleteSplit = deleteLine.Split('|');
                int indexOffset = 1;
                if (deleteSplit.Length == 2)
                {
                    if (int.Parse(deleteSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }

                string fullFilePath = Path.Combine(linkFolder, deleteSplit[1 - indexOffset]);

                if (File.Exists(fullFilePath))
                {
                    if (!gen.IgnoreDeleteFilesPrompt)
                    {
                        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + fullFilePath + "'?", "Nucleus - Delete Files In Config Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dialogResult != DialogResult.Yes)
                        {
                            continue;
                        }
                    }
                    Log(string.Format("Deleting file {0}", Path.GetFileName(fullFilePath), Path.Combine(linkFolder, deleteSplit[2 - indexOffset])));
                    File.Delete(fullFilePath);
                }
                else
                {
                    Log("ERROR - Could not find file: " + fullFilePath + " to delete");
                }
            }
        }

        private void RenameOrMoveFiles(string linkFolder, int i)
        {
            foreach (string renameLine in gen.RenameAndOrMoveFiles)
            {
                string[] renameSplit = renameLine.Split('|');
                int indexOffset = 1;
                if (renameSplit.Length == 3)
                {
                    if (int.Parse(renameSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (renameSplit.Length <= 1)
                {
                    Log("Invalid # of parameters provided for: " + renameLine + ", skipping renaming and or moving");
                    continue;
                }

                string fullFilePath = Path.Combine(linkFolder, renameSplit[1 - indexOffset]);

                if (File.Exists(fullFilePath))
                {
                    Log(string.Format("Renaming and/or moving {0} to {1}", Path.GetFileName(fullFilePath), Path.Combine(linkFolder, renameSplit[2 - indexOffset])));
                    if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(linkFolder, renameSplit[2 - indexOffset]))))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(linkFolder, renameSplit[2 - indexOffset])));
                    }
                    File.Move(fullFilePath, Path.Combine(linkFolder, renameSplit[2 - indexOffset]));
                }
                else
                {
                    Log("ERROR - Could not find file: " + fullFilePath + " to rename and/or move");
                }
            }
        }

        private void UseFlawlessWidescreen(int i)
        {
            Log("Setting up Flawless Widescreen");

            bool pcIs64 = Environment.Is64BitOperatingSystem;
            string pcArch = pcIs64 ? "x64" : "x86";
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\FlawlessWidescreen\\" + pcArch);

            if (gen.FlawlessWidescreenOverrideDisplay)
            {
                List<PlayerInfo> players = profile.PlayerData;

                string setPath = utilFolder + "\\settings.xml";
                string backupPath = Path.GetDirectoryName(setPath) + "\\settings_NUCLEUS_BACKUP.xml";
                if (!File.Exists(backupPath))
                {
                    File.Copy(setPath, backupPath);
                }

                string text = File.ReadAllText(setPath);
                text = text.Replace("1010_FirstUse", "Nucleus_FirstUse");
                File.WriteAllText(setPath, text);

                Log($"Enabling display detection override and setting width:{players[i].MonitorBounds.Width.ToString()}, height:{players[i].MonitorBounds.Height.ToString()}");
                var setDoc = new XmlDocument();
                setDoc.Load(setPath);
                var nodes = setDoc.SelectNodes("Configuration/DisplayDetectionSettings/CustomSettings");
                foreach (XmlNode node in nodes)
                {
                    node.Attributes["Enabled"].Value = "true";
                    node.Attributes["Width"].Value = players[i].MonitorBounds.Width.ToString();
                    node.Attributes["Height"].Value = players[i].MonitorBounds.Height.ToString();
                }
                setDoc.Save(setPath);

                text = File.ReadAllText(setPath);
                text = text.Replace("Nucleus_FirstUse", "1010_FirstUse");
                File.WriteAllText(setPath, text);
            }


            string fwGameFolder = Path.Combine(utilFolder, "PluginCache\\FWS_Plugins\\Modules\\" + gen.FlawlessWidescreen);
            if (gen.FlawlessWidescreenPluginPath?.Length > 0)
            {
                fwGameFolder = Path.Combine(utilFolder, gen.FlawlessWidescreenPluginPath + "\\" + gen.FlawlessWidescreen);
            }

            if (!Directory.Exists(fwGameFolder))
            {
                MessageBox.Show("Nucleus could not find an installed plugin for \"" + gen.FlawlessWidescreen + "\" in FlawlessWidescreen. FlawlessWidescreen will now open. Please make sure to install the plugin and make any required changes. When yo close FlawlessWidescreen, Nucleus will continue. Press OK to open FlawlessWidescreen", "Nucleus - Use Flawless Widescreen", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                //bool appRunning = false;
                Process[] runnProcs = Process.GetProcesses();
                foreach (Process proc in runnProcs)
                {
                    if (proc.ProcessName.ToLower() == "flawlesswidescreen")
                    {
                        proc.Kill();
                    }
                }

                Log("Starting Flawless Widescreen process");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = utilFolder;
                startInfo.FileName = Path.Combine(utilFolder, "FlawlessWidescreen.exe");
                Process util = Process.Start(startInfo);
                util.WaitForExit();
            }

            if (Directory.Exists(fwGameFolder))
            {
                if (File.Exists(Path.Combine(fwGameFolder, "Dependencies\\Scripts\\" + gen.FlawlessWidescreen + ".lua")))
                {
                    List<string> otextChanges = new List<string>();
                    string oscriptPath = Path.Combine(fwGameFolder, "Dependencies\\Scripts\\" + gen.FlawlessWidescreen + ".lua");

                    otextChanges.Add(context.FindLineNumberInTextFile(oscriptPath, "Process_WindowName = ", SearchType.StartsWith) + "|Process_WindowName = \"Removed\"");
                    context.ReplaceLinesInTextFile(oscriptPath, otextChanges.ToArray());
                }

                string newFwGameFolder = fwGameFolder + " - Nucleus Instance " + (i + 1);
                if (Directory.Exists(newFwGameFolder))
                {
                    Directory.Delete(newFwGameFolder, true);
                }
                Directory.CreateDirectory(newFwGameFolder);

                foreach (string dir in Directory.GetDirectories(fwGameFolder, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(newFwGameFolder, dir.Substring(fwGameFolder.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(fwGameFolder, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(newFwGameFolder, file_name.Substring(fwGameFolder.Length + 1)));
                        Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        //Log("ERROR - " + ex.Message);
                        Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(newFwGameFolder, file_name.Substring(fwGameFolder.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(newFwGameFolder, file_name.Substring(fwGameFolder.Length + 1)) + "\"", false);
                    }

                    if (file_name.EndsWith("Dependencies\\Scripts\\" + gen.FlawlessWidescreen + ".lua"))
                    {
                        File.Move(Path.Combine(newFwGameFolder + "\\Dependencies\\Scripts\\", gen.FlawlessWidescreen + ".lua"), Path.Combine(newFwGameFolder + "\\Dependencies\\Scripts\\", Path.GetFileNameWithoutExtension(file_name) + " - Nucleus Instance " + (i + 1) + ".lua"));
                    }
                }

                List<string> textChanges = new List<string>();
                string scriptPath = Path.Combine(newFwGameFolder, "Dependencies\\Scripts\\" + gen.FlawlessWidescreen + " - Nucleus Instance " + (i + 1) + ".lua");

                textChanges.Add(context.FindLineNumberInTextFile(scriptPath, "Process_WindowName = ", SearchType.StartsWith) + "|Process_WindowName = \"" + "Nucleus Instance " + (i + 1) + "(" + gen.Hook.ForceFocusWindowName.Replace("®", "%R") + ")\"");
                context.ReplaceLinesInTextFile(scriptPath, textChanges.ToArray());

                string path = Path.Combine(utilFolder, "Plugins\\FWS_Plugins.fws");
                path = Environment.ExpandEnvironmentVariables(path);

                var doc = new XmlDocument();
                doc.Load(path);
                var nodes = doc.SelectNodes("Plugin/Modules/Module");
                bool exists = false;
                XmlNode origNode = null;
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes["NameSpace"].Value == gen.FlawlessWidescreen)
                    {
                        origNode = node;
                    }
                    if (node.Attributes["NameSpace"].Value == gen.FlawlessWidescreen + " - Nucleus Instance " + (i + 1))
                    {
                        exists = true;
                    }
                    if (origNode != null && exists)
                    {
                        break;
                    }
                }

                if (!exists)
                {
                    // Create a new node with the name of your new server
                    XmlNode newNode = doc.CreateElement("Module");

                    // set the inner xml of a new node to inner xml of original node
                    newNode.InnerXml = origNode.InnerXml;

                    XmlAttribute attr = doc.CreateAttribute("NameSpace");
                    attr.Value = gen.FlawlessWidescreen + " - Nucleus Instance " + (i + 1);

                    newNode.Attributes.SetNamedItem(attr);
                    newNode["FriendlyName"].InnerText = newNode["FriendlyName"].InnerText + " - Nucleus Instance " + (i + 1);
                    doc.DocumentElement["Modules"].AppendChild(newNode);
                }

                doc.Save(path);

                if (i == (numPlayers - 1))
                {
                    //bool appRunning = false;
                    Process[] runnProcs = Process.GetProcesses();
                    foreach (Process proc in runnProcs)
                    {
                        if (proc.ProcessName.ToLower() == "flawlesswidescreen")
                        {
                            proc.Kill();
                        }
                    }

                    Log("Starting Flawless Widescreen process");
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = utilFolder;
                    startInfo.FileName = Path.Combine(utilFolder, "FlawlessWidescreen.exe");
                    Process.Start(startInfo);

                }

                Log("Flawless Widescreen setup complete");
            }
        }


        private Process LaunchProcessPick(PlayerInfo player)
        {
            ProcessPicker ppform = new ProcessPicker(); //using ProcessPicker.cs 
            Log("Launching process picker");

            ppform.pplistBox.DoubleClick += new EventHandler(SelBtn_Click);

            foreach (Control c in ppform.Controls)
            {
                if (c.Name == "refrshBtn")
                {
                    c.Click += new EventHandler(RefrshBtn_Click);
                }
            }

            foreach (Control c in ppform.Controls)
            {
                if (c.Name == "selBtn")
                {
                    c.Click += new EventHandler(SelBtn_Click);
                }
            }

            Process[] allProc = Process.GetProcesses();

            foreach (Process p in allProc)
            {
                if (p.Id == 0 || string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    continue;
                }
                if (attachedIds.Contains(p.Id))
                {
                    if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.ExecutableName).ToLower()) || (gen.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.LauncherExe).ToLower())))
                    {
                        ppform.pplistBox.Items.Insert(0, p.Id + " - (DO NOT USE - Already assigned in Nucleus) " + p.ProcessName);
                    }
                    else
                    {
                        ppform.pplistBox.Items.Add(p.Id + " - (DO NOT USE - Already assigned in Nucleus) " + p.ProcessName);
                    }
                }
                else
                {
                    if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.ExecutableName).ToLower()) || (gen.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.LauncherExe).ToLower())))
                    {
                        ppform.pplistBox.Items.Insert(0, p.Id + " - " + p.ProcessName);
                    }
                    else
                    {
                        ppform.pplistBox.Items.Add(p.Id + " - " + p.ProcessName);
                    }
                }

            }

            ppform.ShowDialog();
            WindowScrape.Static.HwndInterface.MakeTopMost(ppform.Handle);

            if (ppform.pplistBox.SelectedItem != null)
            {
                Process proc = Process.GetProcessById(int.Parse(ppform.pplistBox.SelectedItem.ToString().Split(' ')[0]));
                Log(string.Format("Obtained process {0} (pid {1}) via process picker", proc.ProcessName, proc.Id));
                attached.Add(proc);
                attachedIds.Add(proc.Id);
                player.ProcessID = proc.Id;
                if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                {
                    keyboardProcId = proc.Id;
                }
                return proc;
            }

            return null;
        }

        private void ProcessEnd()
        {
            List<PlayerInfo> players = profile.PlayerData;

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
                    ChangeIPPerInstance(i);
                }

                bool setupDll = true;
                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame && i > 0)
                {
                    setupDll = false;
                }

                if (gen.XInputPlusDll?.Length > 0)
                {
                    SetupXInputPlusDll(garch, player, context, i, setupDll);
                }

                if (gen.UseDevReorder)
                {
                    UseDevReorder(garch, player, players, i, setupDll);
                }

                if (gen.UseX360ce)
                {
                    UseX360ce(i, players, player, context, setupDll);
                }

                if (gen.PromptBetweenInstancesEnd)
                {
                    Log(string.Format("Prompted user for Instance {0}", (i + 1)));

                    Forms.Prompt prompt = new Forms.Prompt("Press OK when game instance " + (i + 1) + " has been launched and/or you wish to continue.");
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

                var window = CreateRawInputWindow(proc, players[i]);

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
                                InjectDLLs(proc, window, players[i]);
                            }
                        }
                    }
                    else
                    {
                        User32Interop.SetForegroundWindow(proc.NucleusGetMainWindowHandle());
                        InjectDLLs(proc, window, players[i]);
                    }
                }

                Thread.Sleep(1000);

                ChangeGameWindow(proc, players, i);

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
                    }
                }

                if (gen.ResetWindows)
                {
                    Thread.Sleep(1000);
                    ResetWindows(players[i].ProcessData, players[i].MonitorBounds.X, players[i].MonitorBounds.Y, players[i].MonitorBounds.Width, players[i].MonitorBounds.Height, i + 1);
                }

                Thread.Sleep(3000);

                if (gen.HideTaskbar && ini.IniReadValue("CustomLayout", "SplitDiv") != "True")
                {
                    User32Util.HideTaskbar();
                }

                if (gen.ProtoInput.AutoHideTaskbar && ini.IniReadValue("CustomLayout", "SplitDiv") != "False")
                {
                    ProtoInput.protoInput.SetTaskbarAutohide(false);
                }

                if (i == (players.Count - 1))
                {
                    Log("End process changes - All done!");
                    try
                    {
                        if (statusWinThread != null && statusWinThread.IsAlive)
                        {
                            Thread.Sleep(5000);
                            if (statusWinThread != null && statusWinThread.IsAlive)
                            {
                                statusWinThread.Abort();
                            }
                        }
                    }
                    catch { }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            if (gen.FakeFocus)
            {
                Log("Start sending fake focus messages every 1000 ms");
                fakeFocus = new Thread(SendFocusMsgs);
                fakeFocus.Start();
            }

            if (gen.SetForegroundWindowElsewhere)
            {
                Log("Setting the foreground window to Nucleus");
                IntPtr nucHwnd = User32Interop.FindWindow(null, "Nucleus Co-op");

                if (nucHwnd != IntPtr.Zero)
                {
                    Log("ERROR - Could not obtain the window handle for Nucleus");
                    User32Interop.SetForegroundWindow(nucHwnd);
                }
            }
        }

        private void ChangeGameWindow(Process proc, List<PlayerInfo> players, int playerIndex)
        {
            var hwnd = WaitForProcWindowHandleNotZero(proc);

            Point loc = new Point(players[playerIndex].MonitorBounds.X, players[playerIndex].MonitorBounds.Y);
            Size size = new Size(players[playerIndex].MonitorBounds.Width, players[playerIndex].MonitorBounds.Height);

            if (!gen.DontRemoveBorders)
            {
                Log($"Removing game window border for process {proc.ProcessName} (pid {proc.Id})");

                uint lStyle = User32Interop.GetWindowLong(hwnd, User32_WS.GWL_STYLE);
                if (gen.WindowStyleValues?.Length > 0)
                {
                    Log("Using user custom window style");
                    foreach (string val in gen.WindowStyleValues)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                }
                else
                {
                    lStyle &= ~User32_WS.WS_CAPTION;
                    lStyle &= ~User32_WS.WS_THICKFRAME;
                    lStyle &= ~User32_WS.WS_MINIMIZE;
                    lStyle &= ~User32_WS.WS_MAXIMIZE;
                    lStyle &= ~User32_WS.WS_SYSMENU;

                    lStyle &= ~User32_WS.WS_DLGFRAME;
                    lStyle &= ~User32_WS.WS_BORDER;
                }
                int resultCode = User32Interop.SetWindowLong(hwnd, User32_WS.GWL_STYLE, lStyle);

                lStyle = User32Interop.GetWindowLong(hwnd, User32_WS.GWL_EXSTYLE);
                if (gen.ExtWindowStyleValues?.Length > 0)
                {
                    Log("Using user custom extended window style");
                    foreach (string val in gen.ExtWindowStyleValues)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                }
                else
                {
                    lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                    lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                    lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                }

                int resultCode2 = User32Interop.SetWindowLong(hwnd, User32_WS.GWL_EXSTYLE, lStyle);
                User32Interop.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
            }

            if (gen.KeepAspectRatio || gen.KeepMonitorAspectRatio)
            {
                if (gen.KeepMonitorAspectRatio)
                {
                    origWidth = owner.MonitorBounds.Width;
                    origHeight = owner.MonitorBounds.Height;
                }
                else
                {
                    if (GetWindowRect(hwnd, out RECT Rect))
                    {
                        origWidth = Rect.Right - Rect.Left;
                        origHeight = Rect.Bottom - Rect.Top;
                    }
                }

                double newWidth = playerBoundsWidth;
                double newHeight = playerBoundsHeight;

                if (newWidth < origWidth)
                {
                    if (origHeight > 0 && origWidth > 0)
                    {
                        origRatio = (double)origWidth / origHeight;

                        newHeight = (newWidth / origRatio);

                        if (newHeight > playerBoundsWidth)
                        {
                            newHeight = playerBoundsWidth;
                        }
                    }
                }
                else
                {
                    if (origHeight > 0 && origWidth > 0)
                    {
                        origRatio = (double)origWidth / origHeight;

                        newWidth = (newHeight * origRatio);

                        if (newWidth > playerBoundsHeight)
                        {
                            newWidth = playerBoundsHeight;
                        }
                    }
                }
                size.Width = (int)newWidth;
                size.Height = (int)newHeight;

                if (newWidth < origWidth)
                {
                    int yOffset = Convert.ToInt32(loc.Y + ((playerBoundsHeight - newHeight) / 2));
                    loc.Y = yOffset;
                }
                if (newHeight < origHeight)
                {
                    int xOffset = Convert.ToInt32(loc.X + ((playerBoundsWidth - newWidth) / 2));
                    loc.X = xOffset;
                }
            }

            if (!gen.DontResize)
            {
                Log(string.Format("Resizing this game window and keeping aspect ratio. Values: width:{0}, height:{1}, aspectratio:{2}, origwidth:{3}, origheight:{4}, plyrboundwidth:{5}, plyrboundheight:{6}", size.Width, size.Height, origRatio, origWidth, origHeight, playerBoundsWidth, playerBoundsHeight));
                WindowScrape.Static.HwndInterface.SetHwndSize(hwnd, size.Width, size.Height);
            }

            if (!gen.DontReposition)
            {
                Log(string.Format("Repositioning this game window to coords x:{0},y:{1}", loc.X, loc.Y));
                WindowScrape.Static.HwndInterface.SetHwndPos(hwnd, loc.X, loc.Y);
            }

            if (!gen.NotTopMost)
            {
                Log("Setting this game window to top most");
                WindowScrape.Static.HwndInterface.MakeTopMost(hwnd);
            }
        }

        public string GetLocalIP()
        {
            var dadada = GetBestInterface(BitConverter.ToUInt32(IPAddress.Parse("8.8.8.8").GetAddressBytes(), 0), out uint interfaceIndex);
            IPAddress xxxd = NetworkInterface.GetAllNetworkInterfaces()
            .Where(netInterface => netInterface.GetIPProperties().GetIPv4Properties().Index == BitConverter.ToInt32(BitConverter.GetBytes(interfaceIndex), 0)).First().GetIPProperties().UnicastAddresses.Where(ipAdd => ipAdd.Address.AddressFamily == AddressFamily.InterNetwork).First().Address;

            return xxxd.ToString();
        }

        private void ChangeIPPerInstance(int i, string networkInterfaceOverride = null)
        {
            Log(string.Format("Changing IP for instance {0}", i + 1));
            if (i == 0)
            {
                if (string.IsNullOrEmpty(iniNetworkInterface) || iniNetworkInterface == "Automatic")
                {
                    string localIP = GetLocalIP();

                    Log("No network interface provided, attempting to automatically find it");
                    var ni = NetworkInterface.GetAllNetworkInterfaces();
                    bool foundNIC = false;
                    foreach (NetworkInterface item in ni)
                    {
                        if (item.OperationalStatus == OperationalStatus.Up)
                        {
                            foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    string ipAddress = ip.Address.ToString();
                                    if (ipAddress == localIP)
                                    {
                                        iniNetworkInterface = item.Name;
                                        Log("Found network interface: " + iniNetworkInterface);
                                        foundNIC = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (foundNIC)
                        {
                            break;
                        }
                    }
                }

                if (iniNetworkInterface == null)
                {
                    Log("ERROR - Unable to resolve network interface");
                    MessageBox.Show("Unable to resolve network interface", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == iniNetworkInterface);
                    var ipProperties = networkInterface.GetIPProperties();
                    var ipInfo = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);
                    currentIPaddress = ipInfo.Address.ToString();
                    currentSubnetMask = ipInfo.IPv4Mask.ToString();
                    currentGateway = ipProperties.GatewayAddresses?.FirstOrDefault(g => g.Address.AddressFamily.ToString() == "InterNetwork")?.Address.ToString();
                    isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;
                    isDynamicDns = ipProperties.IsDynamicDnsEnabled;
                    IPAddressCollection dnsServers = ipProperties.DnsAddresses;
                    foreach (IPAddress dns in dnsServers)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork)
                        {
                            dnsAddresses.Add(dns.ToString());
                            dnsServersStr += dns.ToString() + " ";
                        }
                    }
                    Log("Default IP settings for NetworkInterface: " + iniNetworkInterface + ", IP: " + currentIPaddress + " Subnet Mask: " + currentSubnetMask + " Default Gateway: " + currentGateway + " DHCP: " + isDHCPenabled + " Dnyamic DNS: " + isDynamicDns + " DNS: " + dnsServersStr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Obtaining default IP settings error. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (networkInterfaceOverride == null)
            {
                MessageBox.Show("WARNING: This feature is highly experimental!\n\nYour computers IP is about to be changed. You may receive a prompt immediately after to complete this action. Your IP settings will be reverted back to normal upon exiting Nucleus normally. However, if Nucleus crashes, it is not gauranteed that your settings will be set back.\n\nPress OK when ready to have your IP changed.\n\nOriginal Settings:\nNetworkInterface: " + iniNetworkInterface + "\nIP: " + currentIPaddress + "\nSubnet Mask: " + currentSubnetMask + "\nDefault Gateway: " + currentGateway + "\nDHCP: " + isDHCPenabled + "\nDynamic DNS: " + isDynamicDns + "\nDNS: " + dnsServersStr, "Nucleus - Change IP Per Instance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Thread.Sleep(5000);
            }
            string ipNetwork = currentIPaddress.Substring(0, currentIPaddress.LastIndexOf('.') + 1);

            Ping pingSender = new Ping();
            for (int a = 0; a < 10; a++)
            {
                PingReply reply = pingSender.Send(ipNetwork + (hostAddr + i).ToString(), 1000);

                if (reply.Status == IPStatus.Success)
                {
                    hostAddr++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            string shostAddr = (hostAddr + i).ToString();
            Log("Changing IP to: " + ipNetwork + shostAddr);

            if (networkInterfaceOverride != null)
            {
                SetIP(networkInterfaceOverride, ipNetwork + shostAddr, currentSubnetMask, currentGateway);
            }
            else
            {
                SetIP(iniNetworkInterface, ipNetwork + shostAddr, currentSubnetMask, currentGateway);
            }
        }

        private bool SetIP(string networkInterfaceName, string ipAddress, string subnetMask, string gateway = null)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == networkInterfaceName);
            var ipProperties = networkInterface.GetIPProperties();
            var ipInfo = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);
            var currentIPaddress = ipInfo.Address.ToString();
            var currentSubnetMask = ipInfo.IPv4Mask.ToString();
            var isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;

            if (!isDHCPenabled && currentIPaddress == ipAddress && currentSubnetMask == subnetMask)
            {
                return true;
            }

            Process netsh = new Process();
            netsh.StartInfo.FileName = "netsh";
            netsh.StartInfo.RedirectStandardInput = true;
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.Verb = "runas";
            netsh.Start();

            netsh.StandardInput.WriteLine($"interface ip set address \"{networkInterfaceName}\" static {ipAddress} {subnetMask} " + (string.IsNullOrWhiteSpace(gateway) ? "" : $"{gateway} 1"));

            if (dnsAddresses.Count > 0)
            {
                for (int i = 0; i < dnsAddresses.Count; i++)
                {
                    if (i == 0)
                    {
                        netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} static {dnsAddresses[i]} primary");
                    }
                    else
                    {
                        netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} static {dnsAddresses[i]}");
                    }
                }
            }

            netsh.StandardInput.Flush();
            netsh.StandardInput.Close();
            netsh.WaitForExit();
            var successful = netsh.ExitCode == 0;
            netsh.Dispose();
            return successful;
        }

        private bool SetDHCP(string networkInterfaceName)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == networkInterfaceName);
            var ipProperties = networkInterface.GetIPProperties();
            var isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;


            if (isDHCPenabled && ipProperties.DnsAddresses[0].ToString() == dnsAddresses[0])
            {
                return true;    // no change necessary
            }

            Process netsh = new Process();
            netsh.StartInfo.FileName = "netsh";
            netsh.StartInfo.RedirectStandardInput = true;
            //netsh.StartInfo.RedirectStandardOutput = true;
            //cmd.StartInfo.CreateNoWindow = true;
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.Verb = "runas";
            netsh.Start();

            netsh.StandardInput.WriteLine($"interface ip set address \"{networkInterfaceName}\" dhcp");
            netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} dhcp");

            netsh.StandardInput.Flush();
            netsh.StandardInput.Close();
            netsh.WaitForExit();
            var successful = netsh.ExitCode == 0;
            netsh.Dispose();
            return successful;

        }

        public void SelBtn_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            Form ppform = control.FindForm();

            if (control.GetType() == typeof(ListBox))
            {
                ListBox listBox = control as ListBox;
                if (listBox.SelectedItem == null)
                {
                    MessageBox.Show("No process has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (control.GetType() == typeof(Button))
            {

                foreach (Control c in ppform.Controls)
                {
                    if (c.GetType() == typeof(ListBox))
                    {
                        ListBox listBox = c as ListBox;
                        if (listBox.SelectedItem == null)
                        {
                            MessageBox.Show("No process has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            ppform.Close();
        }

        public void RefrshBtn_Click(object sender, EventArgs e)
        {
            Control control = (Button)sender;

            Form ppform = control.FindForm();
            foreach (Control l in ppform.Controls)
            {

                if (l.GetType() == typeof(ListBox))
                {
                    ListBox listBox = l as ListBox;
                    listBox.Items.Clear();

                    Process[] allProc = Process.GetProcesses();
                    foreach (Process p in allProc)
                    {
                        if (p.Id == 0 || string.IsNullOrEmpty(p.MainWindowTitle))
                        {
                            continue;
                        }
                        if (attachedIds.Contains(p.Id))
                        {
                            if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.ExecutableName).ToLower()) || (gen.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.LauncherExe).ToLower())))
                            {
                                listBox.Items.Insert(0, p.Id + " - (DO NOT USE - Already assigned in Nucleus)" + p.ProcessName);
                            }
                            else
                            {
                                listBox.Items.Add(p.Id + " - (DO NOT USE - Already assigned in Nucleus)" + p.ProcessName);
                            }
                        }
                        else
                        {
                            if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.ExecutableName).ToLower()) || (gen.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(gen.LauncherExe).ToLower())))
                            {
                                listBox.Items.Insert(0, p.Id + " - " + p.ProcessName);
                            }
                            else
                            {
                                listBox.Items.Add(p.Id + " - " + p.ProcessName);
                            }
                        }
                    }

                }

            }
        }

        public static void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    readToEnd = true;
                    return;
                }

                if (e.Data.Contains("+connect_lobby") && string.IsNullOrEmpty(lobbyConnectArg))
                {
                    string toFind1 = "+connect_lobby ";
                    int start = e.Data.IndexOf(toFind1);
                    string string2 = e.Data.Substring(start);
                    lobbyConnectArg = string2;
                    readToEnd = true;
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Goldberg Lobby Connect output data error. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void InjectDLLs(Process proc, Window window, PlayerInfo player)
        {
            Log("Injecting hooks DLL");

            WaitForProcWindowHandleNotZero(proc);

            bool is64 = EasyHook.RemoteHooking.IsX64Process(proc.Id);
            string currDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location); //Directory.GetCurrentDirectory();

            bool windowNull = (window == null);

            try
            {
                string injectorPath = Path.Combine(currDir, $"Nucleus.IJ{(is64 ? "x64" : "x86")}.exe");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = injectorPath;

                object[] args = new object[]
                {
                    1, // Tier. 0 == start up hook, 1 == runtime hook
		            proc.Id, // Target PID
		            0, // WakeUp Thread ID
		            0, // InInjectionOptions (EasyHook)
		            "Nucleus.Hook32.dll", // lib path x86. Inject32/64 will decide which one to use, so pass in both
		            "Nucleus.Hook64.dll", // lib path x64
		            proc.NucleusGetMainWindowHandle(), // Game hWnd
		            gen.HookFocus, // Hook GetForegroundWindow/etc
		            gen.HideCursor,
                    isDebug,
                    nucleusFolderPath, // Primarily for log output
		            gen.SetWindowHook, // SetWindow hook (prevents window from moving)
					gen.PreventWindowDeactivation,
                    player.MonitorBounds.Width,
                    player.MonitorBounds.Height,
                    player.MonitorBounds.X,
                    player.MonitorBounds.Y,
                    (player.IsRawMouse || player.IsRawKeyboard) ? 0 : (player.GamepadId+1),

                    //These options are enabled by default, but if the game isn't using these features the hooks are unwanted
					gen.SupportsMultipleKeyboardsAndMice && gen.HookSetCursorPos,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookGetCursorPos,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookGetKeyState,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookGetAsyncKeyState,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookGetKeyboardState,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookFilterRawInput,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookFilterMouseMessages,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookUseLegacyInput,
                    !gen.HookDontUpdateLegacyInMouseMsg,
                    gen.SupportsMultipleKeyboardsAndMice && gen.HookMouseVisibility,
                    gen.HookReRegisterRawInput,
                    gen.HookReRegisterRawInputMouse,
                    gen.HookReRegisterRawInputKeyboard,
                    gen.InjectHookXinput,
                    gen.InjectDinputToXinputTranslation,

                    windowNull ? "" : (window.HookPipe?.pipeNameWrite ?? ""),
                    windowNull ? "" : (window.HookPipe?.pipeNameRead ?? ""),
                    windowNull ? -1 : window.MouseAttached.ToInt32(),
                    windowNull ? -1 : window.KeyboardAttached.ToInt32()
                };

                var sbArgs = new StringBuilder();
                foreach (object arg in args)
                {
                    //Converting to base64 prevents characters like " or \ breaking the arguments
                    string arg64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(arg.ToString()));

                    sbArgs.Append(" \"");
                    sbArgs.Append(arg64);
                    sbArgs.Append("\"");
                }

                string arguments = sbArgs.ToString();
                startInfo.Arguments = arguments;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                Process injectProc = Process.Start(startInfo);
                injectProc.WaitForExit();
            }
            catch (Exception ex)
            {
                Log(string.Format("ERROR - {0}", ex.Message));
            }
        }

        private void SendFocusMsgs()
        {
            List<PlayerInfo> players = profile.PlayerData;
            if (players == null)
            {
                return;
            }

            List<Process> fakeFocusProcs = new List<Process>();
            var windows = RawInputManager.windows;
            string ffPIDs = "";

            if (gen.FakeFocusInstances?.Length > 0)
            {
                string[] fakeFocusInstances = gen.FakeFocusInstances.Split(',');
                for (int i = 0; i < fakeFocusInstances.Length; i++)
                {
                    if (int.Parse(fakeFocusInstances[i]) <= numPlayers)
                    {
                        fakeFocusProcs.Add(attached[int.Parse(fakeFocusInstances[i]) - 1]);
                    }
                }
            }
            else
            {
                fakeFocusProcs = attached;
            }

            if (gen.KeyboardPlayerSkipFakeFocus)
            {
                for (int i = 0; i < fakeFocusProcs.Count; i++)
                {
                    if (fakeFocusProcs[i].Id == keyboardProcId)
                    {
                        fakeFocusProcs.RemoveAt(i);
                    }
                }
            }

            foreach (Process p in fakeFocusProcs)
            {
                ffPIDs = ffPIDs + p.Id + " ";
            }

            try
            {
                while (true)
                {
                    Thread.Sleep(gen.FakeFocusInterval);

                    foreach (Process proc in attached)
                    {
                        //TODO: NCACTIVATE is a bad idea?
                        User32Interop.SendMessage(proc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATEAPP, (IntPtr)1, IntPtr.Zero);
                        User32Interop.SendMessage(proc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_NCACTIVATE, (IntPtr)0x00000001, IntPtr.Zero);
                        User32Interop.SendMessage(proc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                        User32Interop.SendMessage(proc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_MOUSEACTIVATE, (IntPtr)proc.NucleusGetMainWindowHandle(), (IntPtr)1);

                        //Deep Rock Galactic doesn't work with this message
                        if (gen.FakeFocusSendActivate)
                        {
                            try
                            {
                                for (int p = 0; p < numPlayers; p++)
                                {
                                    if (gen.FakeFocusSendActivateIgnoreKB && (players[p].IsRawKeyboard || players[p].IsKeyboardPlayer) && players[p].ProcessID == proc.Id)
                                    {
                                        continue;
                                    }
                                    else if (players[p].ProcessID == proc.Id)
                                    {
                                        User32Interop.SendMessage(proc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATE, (IntPtr)0x00000002, IntPtr.Zero);
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    if (gen.PreventGameFocus)
                    {
                        foreach (var window in windows)
                        {
                            window.HookPipe.SendPreventForegroundWindow();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine($"ThreadAbortException in FakeFocus. Exiting. Error: {e}");
            }
        }

        struct TickThread
        {
            public double Interval;
            public Action Function;
        }

        public void StartPlayTick(double interval, Action function)
        {
            Thread t = new Thread(PlayTickThread);

            TickThread tick = new TickThread();
            tick.Interval = interval;
            tick.Function = function;
            t.Start(tick);
        }

        private void PlayTickThread(object state)
        {
            TickThread t = (TickThread)state;

            for (; ; )
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(t.Interval));
                t.Function();

                if (hasEnded)
                {
                    break;
                }
            }
        }

        public void CenterCursor()
        {
            List<PlayerInfo> players = profile.PlayerData;
            if (players == null)
            {
                return;
            }

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInfo p = players[i];

                if (p.IsKeyboardPlayer && !p.IsRawKeyboard)
                {
                    ProcessData data = p.ProcessData;
                    if (data == null)
                    {
                        continue;
                    }

                    Rectangle r = p.MonitorBounds;
                    //Cursor.Clip = r;
                    if (data.HWnd != null)
                    {
                        User32Interop.SetForegroundWindow(data.HWnd.NativePtr);
                    }
                }
            }
        }

        public void Update(double delayMS)
        {
            if (profile == null)
            {
                return;
            }

            exited = 0;
            List<PlayerInfo> players = profile.PlayerData;
            timer += delayMS;

            bool updatedHwnd = false;
            if (timer > HWndInterval)
            {
                updatedHwnd = true;
                timer = 0;
            }

            Application.DoEvents();

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInfo p = players[i];
                ProcessData data = p.ProcessData;

                if (data == null)
                {
                    continue;
                }

                if (data.Finished)
                {
                    if (data.Process.HasExited)
                    {
                        exited++;
                    }
                    continue;
                }

                if (p.SteamEmu)
                {
                    List<int> children = ProcessUtil.GetChildrenProcesses(data.Process);

                    // catch the game process, that was spawned from Smart Steam Emu
                    if (children.Count > 0)
                    {
                        for (int j = 0; j < children.Count; j++)
                        {
                            int id = children[j];
                            Process child = Process.GetProcessById(id);
                            try
                            {
                                if (child.ProcessName.Contains("conhost"))
                                {
                                    continue;
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            data.AssignProcess(child);
                            p.SteamEmu = child.ProcessName.Contains("SmartSteamLoader") || child.ProcessName.Contains("cmd");
                        }
                    }
                }
                else
                {
                    if (updatedHwnd)
                    {
                        if (data.Setted)
                        {
                            if (data.Process.HasExited)
                            {
                                exited++;
                                continue;
                            }

                            if (!gen.PromptBetweenInstances)
                            {
                                if (!gen.NotTopMost)
                                {
                                    if (!gen.SetTopMostAtEnd)
                                    {
                                        Log("(Update) Setting game window to top most");
                                        data.HWnd.TopMost = true;
                                    }
                                }
                            }

                            if (data.Status == 2)//
                            {

                                if (!gen.DontRemoveBorders)
                                {
                                    Log("(Update) Removing game window border for pid " + data.Process.Id);
                                    uint lStyle = User32Interop.GetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_STYLE);
                                    if (gen.WindowStyleValues?.Length > 0)
                                    {
                                        Log("(Update) Using user custom window style");
                                        foreach (string val in gen.WindowStyleValues)
                                        {
                                            if (val.StartsWith("~"))
                                            {
                                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                                            }
                                            else
                                            {
                                                lStyle |= Convert.ToUInt32(val, 16);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lStyle &= ~User32_WS.WS_CAPTION;
                                        lStyle &= ~User32_WS.WS_THICKFRAME;
                                        lStyle &= ~User32_WS.WS_MINIMIZE;
                                        lStyle &= ~User32_WS.WS_MAXIMIZE;
                                        lStyle &= ~User32_WS.WS_SYSMENU;

                                        lStyle &= ~User32_WS.WS_DLGFRAME;
                                        lStyle &= ~User32_WS.WS_BORDER;
                                    }
                                    int resultCode = User32Interop.SetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);

                                    lStyle = User32Interop.GetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);

                                    if (gen.ExtWindowStyleValues?.Length > 0)
                                    {
                                        Log("(Update) Using user custom extended window style");
                                        foreach (string val in gen.ExtWindowStyleValues)
                                        {
                                            if (val.StartsWith("~"))
                                            {
                                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                                            }
                                            else
                                            {
                                                lStyle |= Convert.ToUInt32(val, 16);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                                        lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                                        lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                                    }

                                    int resultCode2 = User32Interop.SetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);

                                    User32Interop.SetWindowPos(data.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
                                }

                                if (gen.EnableWindows)
                                {
                                    EnableWindow(data.HWnd.NativePtr, true);
                                }

                                //Minimise and un-minimise the window. Fixes black borders in Minecraft, but causing stretching issues in games like Minecraft.
                                if (gen.RefreshWindowAfterStart)
                                {
                                    ShowWindow(data.HWnd.NativePtr, 6);
                                    ShowWindow(data.HWnd.NativePtr, 9);
                                }

                                if (gen.WindowStyleEndChanges?.Length > 0 || gen.ExtWindowStyleEndChanges?.Length > 0)
                                {
                                    Thread.Sleep(1000);
                                    WindowStyleChanges(data, i);
                                }

                                data.Finished = true;

                                Debug.WriteLine("State 2");
                                Log("Update State 2");

                                if (i == (players.Count - 1))
                                {
                                    if (gen.LockMouse)
                                    {
                                        _cursorModule.SetActiveWindow();
                                    }
                                }
                            }
                            else if (data.Status == 1)
                            {
                                if (!gen.KeepAspectRatio && !gen.KeepMonitorAspectRatio && !dllRepos && !gen.DontResize)
                                {
                                    if (data.Position.X != p.MonitorBounds.X || data.Position.Y != p.MonitorBounds.Y)
                                    {
                                        Log("(Update) Data position X or Y does not match player bounds for pid " + data.Process.Id + ", using player bound variables");
                                        Log(string.Format("(Update) Repostioning game window for pid {0} to coords x:{1},y:{2}", data.Process.Id, p.MonitorBounds.X, p.MonitorBounds.Y));
                                        //data.HWnd.Location = data.Position;
                                        data.HWnd.Location = new Point(p.MonitorBounds.X, p.MonitorBounds.Y);
                                    }
                                    else
                                    {
                                        Log(string.Format("(Update) Repostioning game window for pid {0} to coords x:{1},y:{2}", data.Process.Id, data.Position.X, data.Position.Y));
                                        data.HWnd.Location = data.Position;
                                    }
                                }

                                data.Status++;
                                Log("Update State 1");

                                if (gen.LockMouse)
                                {
                                    if (p.IsKeyboardPlayer && !p.IsRawKeyboard)
                                    {
                                        _cursorModule.Setup(data.Process, p.MonitorBounds);
                                    }
                                    else
                                    {
                                        _cursorModule.AddOtherGameHandle(data.Process.NucleusGetMainWindowHandle());
                                    }
                                }

                                if (i == (players.Count - 1) && !gen.ProcessChangesAtEnd)
                                {
                                    Log("(Update) All done!");
                                    try
                                    {
                                        if (statusWinThread != null && statusWinThread.IsAlive)
                                        {
                                            Thread.Sleep(5000);
                                            if (statusWinThread != null && statusWinThread.IsAlive)
                                            {
                                                statusWinThread.Abort();
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                            else if (data.Status == 0)
                            {
                                if (!dllResize && !gen.DontResize)
                                {
                                    if (gen.KeepAspectRatio || gen.KeepMonitorAspectRatio)
                                    {

                                        if (gen.KeepMonitorAspectRatio)
                                        {
                                            origWidth = owner.MonitorBounds.Width;
                                            origHeight = owner.MonitorBounds.Height;
                                        }
                                        else
                                        {
                                            if (GetWindowRect(data.Process.NucleusGetMainWindowHandle(), out RECT Rect))
                                            {
                                                origWidth = Rect.Right - Rect.Left;
                                                origHeight = Rect.Bottom - Rect.Top;
                                            }
                                        }

                                        double newWidth = playerBoundsWidth;
                                        double newHeight = playerBoundsHeight;

                                        if (newWidth < origWidth)
                                        {
                                            if (origHeight > 0 && origWidth > 0)
                                            {
                                                origRatio = (double)origWidth / origHeight;

                                                newHeight = (newWidth / origRatio);

                                                if (newHeight > playerBoundsHeight)
                                                {
                                                    newHeight = playerBoundsHeight;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (origHeight > 0 && origWidth > 0)
                                            {
                                                origRatio = (double)origWidth / origHeight;

                                                newWidth = (newHeight * origRatio);

                                                if (newWidth > playerBoundsWidth)
                                                {
                                                    newWidth = playerBoundsWidth;
                                                }
                                            }
                                        }

                                        Log(string.Format("(Update) Resizing game window for pid {0} and keeping aspect ratio. Values: width:{1}, height:{2}, aspectratio:{3}, origwidth:{4}, origheight:{5}, plyrboundwidth:{6}, plyrboundheight:{7}", data.Process.Id, (int)newWidth, (int)newHeight, (Math.Truncate(origRatio * 100) / 100), origWidth, origHeight, playerBoundsWidth, playerBoundsHeight));
                                        data.HWnd.Size = new Size(Convert.ToInt32(newWidth), Convert.ToInt32(newHeight));

                                        //x horizontal , y vertical
                                        if (newWidth < origWidth)
                                        {
                                            int yOffset = Convert.ToInt32(data.Position.Y + ((playerBoundsHeight - newHeight) / 2));
                                            data.Position.Y = yOffset;
                                        }
                                        if (newHeight < origHeight)
                                        {
                                            int xOffset = Convert.ToInt32(data.Position.X + ((playerBoundsWidth - newWidth) / 2));
                                            data.Position.X = xOffset;
                                        }

                                        Log(string.Format("(Update) Resizing game window (for horizontal centering), coords x:{1},y:{2}", data.Process.Id, data.Position.X, data.Position.Y));
                                        data.HWnd.Location = data.Position;
                                    }
                                    else
                                    {
                                        if (data.Size.Width == 0 || data.Size.Height == 0)
                                        {
                                            Log("(Update) Data size width or height is showing as 0 for pid " + data.Process.Id + ", using player bound variables");
                                            data.Size.Width = playerBoundsWidth;
                                            data.Size.Height = playerBoundsHeight;

                                            if (playerBoundsWidth == 0 || playerBoundsHeight == 0)
                                            {
                                                Log("(Update) Play bounds width or height is showing as 0 for pid " + data.Process.Id + ", using monitor bound variables");
                                                data.Size.Width = p.MonitorBounds.Width;
                                                data.Size.Height = p.MonitorBounds.Height;
                                            }
                                        }
                                        Log(string.Format("(Update) Resizing game window for pid {0} to the following width:{1}, height:{2}", data.Process.Id, data.Size.Width, data.Size.Height));
                                        data.HWnd.Size = data.Size;
                                    }
                                }


                                data.Status++;
                                Log("Update State 0");
                            }
                        }
                        else
                        {
                            data.Process.Refresh();

                            if (data.Process.HasExited)
                            {
                                if (p.GotLauncher)
                                {
                                    if (p.GotGame)
                                    {
                                        exited++;
                                    }
                                    else
                                    {
                                        List<int> children = ProcessUtil.GetChildrenProcesses(data.Process);
                                        if (children.Count > 0)
                                        {
                                            for (int j = 0; j < children.Count; j++)
                                            {
                                                int id = children[j];
                                                Process pro = Process.GetProcessById(id);

                                                if (!attached.Contains(pro))
                                                {
                                                    attached.Add(pro);
                                                    attachedIds.Add(pro.Id);
                                                    p.ProcessID = pro.Id;
                                                    data.HWnd = null;
                                                    p.GotGame = true;
                                                    data.AssignProcess(pro);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Steam showing a launcher, need to find our game process
                                    string launcher = gen.LauncherExe;
                                    if (!string.IsNullOrEmpty(launcher))
                                    {
                                        if (launcher.ToLower().EndsWith(".exe"))
                                        {
                                            launcher = launcher.Remove(launcher.Length - 4, 4);
                                        }

                                        Process[] procs = Process.GetProcessesByName(launcher);
                                        for (int j = 0; j < procs.Length; j++)
                                        {
                                            Process pro = procs[j];

                                            if (!attachedIds.Contains(pro.Id))
                                            {
                                                attached.Add(pro);
                                                attachedIds.Add(pro.Id);
                                                p.ProcessID = pro.Id;
                                                data.AssignProcess(pro);
                                                p.GotLauncher = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (data.HWNDRetry || data.HWnd == null || data.HWnd.NativePtr != data.Process.NucleusGetMainWindowHandle())
                                {
                                    Log("Update data process has not exited");
                                    data.HWnd = new HwndObject(data.Process.NucleusGetMainWindowHandle());
                                    Point pos = data.HWnd.Location;

                                    if (string.IsNullOrEmpty(data.HWnd.Title) ||
                                        pos.X == -32000 ||
                                        data.HWnd.Title.ToLower() == gen.LauncherTitle?.ToLower())
                                    {
                                        data.HWNDRetry = true;
                                    }
                                    else if (!string.IsNullOrEmpty(gen.Hook.ForceFocusWindowName) &&
                                        // TODO: this Levenshtein distance is being used to help us around Call of Duty Black Ops, as it uses a ® icon in the title bar
                                        //       there must be a better way
                                        StringUtil.ComputeLevenshteinDistance(data.HWnd.Title, gen.Hook.ForceFocusWindowName) > 2 && !gen.HasDynamicWindowTitle)
                                    {
                                        data.HWNDRetry = true;
                                    }
                                    else
                                    {
                                        Size s = data.Size;
                                        data.Setted = true;
                                    }
                                }
                                else
                                {
                                    Log("Assigning window handle");
                                    data.HWnd = new HwndObject(data.Process.NucleusGetMainWindowHandle());
                                    Point pos = data.HWnd.Location;

                                    Size s = data.Size;
                                    data.Setted = true;
                                }
                            }
                        }
                    }
                }
            }

            if (exited == players.Count)
            {
                if (!hasEnded)
                {
                    hasEnded = true;
                    Log("Update method calling Handler End function");
                    End(false);
                }
            }
        }

        public void Log(StreamWriter writer)
        {
        }

        private void Log(string logMessage)
        {
            try
            {
                logMsg = logMessage;

                if (ini.IniReadValue("Misc", "DebugLog") == "True")
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

        private IntPtr WaitForProcWindowHandleNotZero(Process proc)
        {
            try
            {
                if ((int)proc.NucleusGetMainWindowHandle() == 0)
                {
                    for (int times = 0; times < 200; times++)
                    {
                        Thread.Sleep(500);
                        if ((int)proc.NucleusGetMainWindowHandle() > 0)
                        {
                            break;
                        }

                        if (times == 199 && (int)proc.NucleusGetMainWindowHandle() == 0)
                        {
                            Log(string.Format("ERROR - WaitForProcWindowHandleNotZero could not find main window handle for {0} (pid {1})", proc.ProcessName, proc.Id));
                        }
                    }
                }

                return proc.NucleusGetMainWindowHandle();
            }
            catch
            {
                Log("ERROR - WaitForProcWindowHandleNotZero encountered an exception");
                return (IntPtr)(-1);
            }
        }

        private string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
            {
                return "4.8 or later";
            }

            if (releaseKey >= 461808)
            {
                return "4.7.2";
            }

            if (releaseKey >= 461308)
            {
                return "4.7.1";
            }

            if (releaseKey >= 460798)
            {
                return "4.7";
            }

            if (releaseKey >= 394802)
            {
                return "4.6.2";
            }

            if (releaseKey >= 394254)
            {
                return "4.6.1";
            }

            if (releaseKey >= 393295)
            {
                return "4.6";
            }

            if (releaseKey >= 379893)
            {
                return "4.5.2";
            }

            if (releaseKey >= 378675)
            {
                return "4.5.1";
            }

            if (releaseKey >= 378389)
            {
                return "4.5";
            }
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        private void GoldbergLobbyConnect()
        {
            Forms.Prompt prompt = new Forms.Prompt("Goldberg Lobby Connect: Press OK after you are hosting a game.");
            prompt.ShowDialog();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\GoldbergEmu\\lobby_connect\\lobby_connect.exe");

            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            Process p = Process.Start(startInfo);
            p.OutputDataReceived += proc_OutputDataReceived;
            p.BeginOutputReadLine();

            while (readToEnd == false)
            {
                Thread.Sleep(25);
            }
            try
            {
                Thread.Sleep(2500);
                p.Kill();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            if (!string.IsNullOrEmpty(lobbyConnectArg))
            {
                Log("Goldberg Lobby Connect: Setting lobby ID to " + lobbyConnectArg.Substring(15));
            }
            else
            {
                Log("Goldberg Lobby Connect: Could not find lobby ID");
                MessageBox.Show("Goldberg Lobby Connect: Could not find lobby ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ResetWindows(ProcessData processData, int x, int y, int w, int h, int i)
        {
            Log("Attempting to reposition, resize and strip borders for instance " + (i - 1) + $" - {processData.Process.ProcessName} (pid {processData.Process.Id})");
            try
            {
                if (!gen.DontRemoveBorders)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE);
                    if (gen.WindowStyleValues?.Length > 0)
                    {
                        Log("Using user custom window style");
                        foreach (string val in gen.WindowStyleValues)
                        {
                            if (val.StartsWith("~"))
                            {
                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                            }
                            else
                            {
                                lStyle |= Convert.ToUInt32(val, 16);
                            }
                        }
                    }
                    else
                    {
                        lStyle &= ~User32_WS.WS_CAPTION;
                        lStyle &= ~User32_WS.WS_THICKFRAME;
                        lStyle &= ~User32_WS.WS_MINIMIZE;
                        lStyle &= ~User32_WS.WS_MAXIMIZE;
                        lStyle &= ~User32_WS.WS_SYSMENU;

                        lStyle &= ~User32_WS.WS_DLGFRAME;
                        lStyle &= ~User32_WS.WS_BORDER;
                    }
                    int resultCode = User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);

                    lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);

                    if (gen.ExtWindowStyleValues?.Length > 0)
                    {
                        Log("Using user custom extended window style");
                        foreach (string val in gen.ExtWindowStyleValues)
                        {
                            if (val.StartsWith("~"))
                            {
                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                            }
                            else
                            {
                                lStyle |= Convert.ToUInt32(val, 16);
                            }
                        }
                    }
                    else
                    {
                        lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                        lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                        lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                    }


                    int resultCode2 = User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);
                    User32Interop.SetWindowPos(processData.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));

                }

                if (!gen.DontReposition)
                {
                    processData.HWnd.Location = new Point(x, y);
                }

                if (!gen.DontResize)
                {
                    processData.HWnd.Size = new Size(w, h);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR - Exception in ResetWindows for instance " + (i - 1) + ". " + ex.Message);
            }

            try
            {
                if ((processData.HWnd.Location != new Point(x, y) && !gen.DontReposition) || (processData.HWnd.Size != new Size(w, h) && !gen.DontResize))
                {
                    Log("ERROR - ResetWindows was unsuccessful for instance " + (i - 1));
                }
            }
            catch (Exception e)
            {
                Log("ERROR - Exception in ResetWindows for instance " + (i - 1) + ", error = " + e);
            }
        }

        private void CustomDllEnabled(GenericContext context, PlayerInfo player, Rectangle playerBounds, int i, bool setupDll)
        {
            if (setupDll)
            {
                Log(string.Format("Setting up Custom DLL, UseAlpha8CustomDll: {0}", gen.Hook.UseAlpha8CustomDll));
                byte[] xdata;
                if (gen.Hook.UseAlpha8CustomDll && !gameIs64)
                {
                    xdata = Properties.Resources.xinput1_3;
                }
                else
                {
                    if (gen.Hook.UseAlpha8CustomDll)
                    {
                        Log("Using Alpha 10 custom dll as there is no Alpha 8 x64 custom dll");
                    }

                    if (gameIs64)
                    {
                        xdata = Properties.Resources.xinput1_3_a10_x64;
                    }
                    else //if (Is64Bit(exePath) == false)
                    {
                        xdata = Properties.Resources.xinput1_3_a10;
                    }
                }

                if (context.Hook.XInputNames == null)
                {
                    string ogFile = Path.Combine(instanceExeFolder, "xinput1_3.dll");
                    FileCheck(ogFile);

                    Log(string.Format("Writing custom dll xinput1_3.dll to {0}", instanceExeFolder));
                    using (Stream str = File.OpenWrite(ogFile))
                    {
                        str.Write(xdata, 0, xdata.Length);
                    }

                }
                else
                {
                    string[] xinputs = context.Hook.XInputNames;
                    for (int z = 0; z < xinputs.Length; z++)
                    {
                        string xinputName = xinputs[z];
                        string ogFile = Path.Combine(instanceExeFolder, xinputName);

                        FileCheck(ogFile);

                        Log(string.Format("Writing custom dll {0} to {1}", xinputName, instanceExeFolder));
                        using (Stream str = File.OpenWrite(Path.Combine(instanceExeFolder, xinputName)))
                        {
                            str.Write(xdata, 0, xdata.Length);
                        }
                    }
                }
            }

            Log(string.Format("Writing ncoop.ini to {0} with Game.Hook values", instanceExeFolder));
            string ncoopIni = Path.Combine(instanceExeFolder, "ncoop.ini");
            using (Stream str = File.OpenWrite(ncoopIni))
            {
                byte[] ini = Properties.Resources.ncoop;
                str.Write(ini, 0, ini.Length);
            }

            FileCheck(Path.Combine(instanceExeFolder, "ncoop.ini"));

            IniFile x360 = new IniFile(ncoopIni);
            x360.IniWriteValue("Options", "Log", "0");
            x360.IniWriteValue("Options", "FileLog", "0");
            x360.IniWriteValue("Options", "ForceFocus", gen.Hook.ForceFocus.ToString(CultureInfo.InvariantCulture));
            if (!gen.Hook.UseAlpha8CustomDll)
            {
                x360.IniWriteValue("Options", "Version", "2");
                x360.IniWriteValue("Options", "ForceFocusWindowRegex", gen.Hook.ForceFocusWindowName.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                string windowTitle = gen.Hook.ForceFocusWindowName;
                if (gen.IdInWindowTitle || gen.FlawlessWidescreen?.Length > 0)
                {
                    windowTitle = gen.Hook.ForceFocusWindowName + "(" + i + ")";
                    if (!string.IsNullOrEmpty(gen.FlawlessWidescreen))
                    {
                        windowTitle = "Nucleus Instance " + (i + 1) + "(" + gen.Hook.ForceFocusWindowName + ")";
                    }
                }
                x360.IniWriteValue("Options", "ForceFocusWindowName", windowTitle.ToString(CultureInfo.InvariantCulture));
            }

            int wx;
            int wy;
            int rw;
            int rh;
            if (context.Hook.WindowX > 0 && context.Hook.WindowY > 0)
            {
                wx = context.Hook.WindowX;
                wy = context.Hook.WindowY;
                x360.IniWriteValue("Options", "WindowX", context.Hook.WindowX.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "WindowY", context.Hook.WindowY.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                wx = playerBounds.X;
                wy = playerBounds.Y;
                x360.IniWriteValue("Options", "WindowX", playerBounds.X.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "WindowY", playerBounds.Y.ToString(CultureInfo.InvariantCulture));
            }

            if (context.Hook.ResWidth > 0 && context.Hook.ResHeight > 0)
            {
                rw = context.Hook.ResWidth;
                rh = context.Hook.ResHeight;
                x360.IniWriteValue("Options", "ResWidth", context.Hook.ResWidth.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ResHeight", context.Hook.ResHeight.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                rw = context.Width;
                rh = context.Height;
                x360.IniWriteValue("Options", "ResWidth", context.Width.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ResHeight", context.Height.ToString(CultureInfo.InvariantCulture));
            }

            if (!gen.Hook.UseAlpha8CustomDll)
            {
                if (context.Hook.FixResolution)
                {
                    Log(string.Format("Custom DLL will be doing the resizing with values width:{0}, height:{1}", rw, rh));
                    dllResize = true;
                }
                if (context.Hook.FixPosition)
                {
                    Log(string.Format("Custom DLL will be doing the repositioning with values x:{0}, y:{1}", wx, wy));
                    dllRepos = true;
                }
                x360.IniWriteValue("Options", "FixResolution", context.Hook.FixResolution.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "FixPosition", context.Hook.FixPosition.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ClipMouse", player.IsKeyboardPlayer.ToString(CultureInfo.InvariantCulture)); //context.Hook.ClipMouse
            }

            x360.IniWriteValue("Options", "RerouteInput", context.Hook.XInputReroute.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "RerouteJoystickTemplate", JoystickDatabase.GetID(player.GamepadProductGuid.ToString()).ToString(CultureInfo.InvariantCulture));

            if (context.Hook.EnableMKBInput || player.IsKeyboardPlayer)
            {
                Log("Enabling MKB");
                x360.IniWriteValue("Options", "EnableMKBInput", "True".ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                x360.IniWriteValue("Options", "EnableMKBInput", "False".ToString(CultureInfo.InvariantCulture));
            }

            x360.IniWriteValue("Options", "IsKeyboardPlayer", player.IsKeyboardPlayer.ToString(CultureInfo.InvariantCulture));

            // windows events

            x360.IniWriteValue("Options", "BlockInputEvents", context.Hook.BlockInputEvents.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "BlockMouseEvents", context.Hook.BlockMouseEvents.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "BlockKeyboardEvents", context.Hook.BlockKeyboardEvents.ToString(CultureInfo.InvariantCulture));

            // xinput
            x360.IniWriteValue("Options", "XInputEnabled", context.Hook.XInputEnabled.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "XInputPlayerID", player.GamepadId.ToString(CultureInfo.InvariantCulture));

            // dinput
            x360.IniWriteValue("Options", "DInputEnabled", context.Hook.DInputEnabled.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "DInputGuid", player.GamepadGuid.ToString().ToUpper());
            x360.IniWriteValue("Options", "DInputForceDisable", context.Hook.DInputForceDisable.ToString());

            Log("Custom DLL setup complete");
        }

        private void CopyCustomUtils(int i, string linkFolder, bool setupDll)
        {
            if (setupDll)
            {
                Log("Copying custom files/folders");
                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\User");
                foreach (string customUtil in gen.CopyCustomUtils)
                {
                    int numParams = customUtil.Count(x => x == '|') + 1;
                    string[] splitParams = customUtil.Split('|');
                    string utilName = splitParams[0];
                    string utilPath = string.Empty;
                    if (numParams == 2)
                    {
                        utilPath = splitParams[1];
                    }

                    if (numParams == 3)
                    {
                        string utilInstances = splitParams[2];
                        List<int> instances = new List<int>();
                        instances = utilInstances.Split(',').Select(int.Parse).ToList();
                        if (!instances.Contains(i))
                        {
                            continue;
                        }
                    }

                    string source_dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location) + "\\utils\\User\\" + utilPath;
                    FileAttributes attr = File.GetAttributes(source_dir);

                    if (attr.HasFlag(FileAttributes.Directory)) //directory
                    {
                        string destination_dir = linkFolder.TrimEnd('\\') + '\\' + utilPath;

                        foreach (string dir in System.IO.Directory.GetDirectories(source_dir, "*", SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(System.IO.Path.Combine(destination_dir, dir.Substring(source_dir.Length + 1)));
                        }

                        Log("Copying user folder " + utilPath + " and all its contents to " + "Instance" + i + "\\" + utilPath);
                        foreach (string file_name in System.IO.Directory.GetFiles(source_dir, "*", SearchOption.AllDirectories))
                        {
                            File.Copy(file_name, System.IO.Path.Combine(destination_dir, file_name.Substring(source_dir.Length + 1)));
                        }
                    }
                    else //file
                    {
                        FileCheck(Path.Combine(linkFolder.TrimEnd('\\') + '\\' + utilPath, Path.GetFileName(utilName)));

                        Log("Copying " + utilName + " to " + "Instance" + i + "\\" + utilPath);
                        File.Copy(Path.Combine(utilFolder, utilName), Path.Combine(linkFolder.TrimEnd('\\') + '\\' + utilPath, Path.GetFileName(utilName)), true);
                    }
                }
                Log("Copying custom files complete");
            }
        }

        private void UseDirectX9Wrapper(bool setupDll)
        {
            if (setupDll)
            {
                Log("Copying over DirectX 9, Direct 3D Wrapper (d3d9.dll) to instance executable folder");
                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\DirectXWrapper");
                string ogFile = Path.Combine(instanceExeFolder, "d3d9.dll");

                FileCheck(ogFile);
                File.Copy(Path.Combine(utilFolder, "d3d9.dll"), ogFile, true);
            }

        }

        private void UseX360ce(int i, List<PlayerInfo> players, PlayerInfo player, GenericContext context, bool setupDll)
        {
            Log("Setting up x360ce");
            string x360exe = "";
            string x360dll = "";
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\x360ce");

            string[] x360cedlls = { "xinput1_3.dll" };
            if (gen.X360ceDll?.Length > 0)
            {
                x360cedlls = gen.X360ceDll;
            }

            if (setupDll)
            {
                foreach (string x360ceDllName in x360cedlls)
                {
                    if (i == 0)
                    {
                        if (gameIs64)
                        {
                            x360exe = "x360ce_x64.exe";
                            x360dll = "xinput1_3_x64.dll";
                        }

                        else //if (Is64Bit(exePath) == false)
                        {
                            x360exe = "x360ce.exe";
                            x360dll = "xinput1_3.dll";
                        }

                        if (x360ceDllName.ToLower().StartsWith("dinput"))
                        {
                            if (gameIs64)
                            {
                                x360dll = "dinput8_x64.dll";
                            }
                            else
                            {
                                x360dll = "dinput8.dll";
                            }
                        }

                        string ogFile = Path.Combine(instanceExeFolder, x360exe);
                        FileCheck(ogFile);

                        Log("Copying over " + x360exe);
                        try
                        {
                            File.Copy(Path.Combine(utilFolder, x360exe), Path.Combine(instanceExeFolder, x360exe), true);
                        }
                        catch
                        {
                            Log("Using alternative copy method");
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, x360exe) + "\" \"" + Path.Combine(instanceExeFolder, x360exe) + "\"");
                        }

                        ogFile = Path.Combine(instanceExeFolder, x360ceDllName);
                        FileCheck(ogFile);

                        if (x360dll != x360ceDllName)
                        {
                            Log("Copying over " + x360dll + " and renaming it to " + x360ceDllName);
                        }
                        else
                        {
                            Log("Copying over " + x360dll);
                        }
                        try
                        {
                            File.Copy(Path.Combine(utilFolder, x360dll), ogFile, true);
                        }
                        catch
                        {
                            Log("Using alternative copy method");
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, x360dll) + "\" \"" + ogFile + "\"");
                        }

                    }
                    else
                    {
                        if (gen.SymlinkGame || gen.HardlinkGame || gen.HardcopyGame)
                        {
                            Log("Carrying over " + x360ceDllName + " from Instance0");
                            FileCheck(Path.Combine(instanceExeFolder, x360ceDllName));

                            File.Copy(Path.Combine(instanceExeFolder.Substring(0, instanceExeFolder.LastIndexOf('\\') + 1) + "Instance0", x360ceDllName), Path.Combine(instanceExeFolder, x360ceDllName), true);
                        }
                    }
                }
            }

            if (i > 0 && (gen.SymlinkGame || gen.HardlinkGame || gen.HardcopyGame))
            {
                Log("Carrying over x360ce.ini from Instance0");

                FileCheck(Path.Combine(instanceExeFolder, "x360ce.ini"));
                File.Copy(Path.Combine(instanceExeFolder.Substring(0, instanceExeFolder.LastIndexOf('\\') + 1) + "Instance0", "x360ce.ini"), Path.Combine(instanceExeFolder, "x360ce.ini"), true);
            }
            else
            {
                Log("Starting x360ce process");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = instanceExeFolder;
                startInfo.FileName = Path.Combine(instanceExeFolder, x360exe);
                Process util = Process.Start(startInfo);
                Log("Waiting until x360ce process is exited");
                util.WaitForExit();
            }

            if (!File.Exists(Path.Combine(instanceExeFolder, "x360ce.ini")))
            {
                Log("x360ce.ini has not been generated. Copying default x360ce.ini from utils");
                try
                {
                    File.Copy(Path.Combine(utilFolder, "x360ce.ini"), Path.Combine(instanceExeFolder, "x360ce.ini"), true);
                }
                catch
                {
                    Log("Using alternative copy method");
                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x360ce.ini") + "\" \"" + Path.Combine(instanceExeFolder, "x360ce.ini") + "\"");
                }
            }

            Log("Making changes to x360ce.ini; PAD mapping to player");

            List<string> textChanges = new List<string>();

            if (!player.IsKeyboardPlayer)
            {
                Thread.Sleep(1000);
                if (gen.PlayersPerInstance > 1)
                {
                    for (int x = 1; x <= gen.PlayersPerInstance; x++)
                    {
                        textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD" + x + "=", SearchType.StartsWith) + "|PAD" + x + "=IG_" + players[x].GamepadGuid.ToString().Replace("-", string.Empty));
                    }
                    for (int x = gen.PlayersPerInstance + 1; x <= 4; x++)
                    {
                        textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD" + x + "=", SearchType.StartsWith) + "|PAD" + x + "=IG_" + players[x].GamepadGuid.ToString().Replace("-", string.Empty));
                    }
                    plyrIndex += gen.PlayersPerInstance;
                }
                else
                {
                    textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD1=", SearchType.StartsWith) + "|PAD1=" + context.x360ceGamepadGuid);
                    textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD2=", SearchType.StartsWith) + "|PAD2=");
                    textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD3=", SearchType.StartsWith) + "|PAD3=");
                    textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "PAD4=", SearchType.StartsWith) + "|PAD4=");
                }

                context.ReplaceLinesInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), textChanges.ToArray());
            }

            if (gen.XboxOneControllerFix)
            {
                Thread.Sleep(1000);
                Log("Implementing Xbox One controller fix");

                textChanges.Clear();
                textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), "HookMode=1", SearchType.Full) + "|" +
                    "HookLL=0\n" +
                    "HookCOM=1\n" +
                    "HookSA=0\n" +
                    "HookWT=0\n" +
                    "HOOKDI=1\n" +
                    "HOOKPIDVID=1\n" +
                    "HookName=0\n" +
                    "HookMode=0\n"
                );

                context.ReplaceLinesInTextFile(Path.Combine(instanceExeFolder, "x360ce.ini"), textChanges.ToArray());
            }

            if (File.Exists(Path.Combine(instanceExeFolder, "x360ce.ini")) && !gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
            {
                Log("x360ce.ini will be deleted upon ending session");
                addedFiles.Add(Path.Combine(instanceExeFolder, "x360ce.ini"));
            }
            Log("x360ce setup complete");
        }

        private void UseDInputBlocker(string garch, bool setupDll)
        {
            Log("Setting up DInput blocker");
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\dinput8.blocker");

            if (setupDll)
            {

                string ogFile = Path.Combine(instanceExeFolder, "dinput8.dll");
                FileCheck(ogFile);

                Log("Copying dinput8.dll");
                File.Copy(Path.Combine(utilFolder, garch + "\\dinput8.dll"), Path.Combine(instanceExeFolder, "dinput8.dll"), true);
            }
        }

        private void UseDevReorder(string garch, PlayerInfo player, List<PlayerInfo> players, int i, bool setupDll)
        {
            Log("Setting up Devreorder");
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\devreorder");

            if (setupDll)
            {
                string ogFile = Path.Combine(instanceExeFolder, "dinput8.dll");
                FileCheck(ogFile);

                Log("Copying dinput8.dll");
                File.Copy(Path.Combine(utilFolder, garch + "\\dinput8.dll"), Path.Combine(instanceExeFolder, "dinput8.dll"), true);
            }

            if (setupDll)
            {
                addedFiles.Add(Path.Combine(instanceExeFolder, "devreorder.ini"));
            }

            List<string> iniConfig = new List<string>();
            iniConfig.Add("[order]");
            iniConfig.Add("{" + player.GamepadGuid + "}");
            iniConfig.Add(string.Empty);
            iniConfig.Add("[hidden]");

            for (int p = 0; p < players.Count; p++)
            {
                if (p != i)
                {
                    iniConfig.Add("{" + players[p].GamepadGuid + "}");
                }
            }
            Log("Writing devreorder.ini with the only visible gamepad guid: " + player.GamepadGuid);
            File.WriteAllLines(Path.Combine(instanceExeFolder, "devreorder.ini"), iniConfig.ToArray());
            Log("devreorder setup complete");
        }

        private void SetupXInputPlusDll(string garch, PlayerInfo player, GenericContext context, int i, bool setupDll)
        {
            Log("Setting up XInput Plus");
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\XInputPlus");
            if (gen.XInputPlusOldDll)
            {
                utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\XInputPlus\\old");
            }

            if (setupDll)
            {
                foreach (string xinputDllName in gen.XInputPlusDll)
                {
                    string xinputDll = "xinput1_3.dl_";
                    FileCheck(Path.Combine(instanceExeFolder, xinputDllName));
                    try
                    {
                        if (xinputDllName.ToLower().StartsWith("dinput."))
                        {
                            xinputDll = "Dinput.dl_";
                        }
                        else if (xinputDllName.ToLower().StartsWith("dinput8."))
                        {
                            xinputDll = "Dinput8.dl_";
                        }
                        Log("Using " + xinputDll + " (" + garch + ") as base and naming it: " + xinputDllName);

                        File.Copy(Path.Combine(utilFolder, garch + "\\" + xinputDll), Path.Combine(instanceExeFolder, xinputDllName), true);
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR - " + ex.Message);
                        Log("Using alternative copy method for " + xinputDll);
                        CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, garch + "\\" + xinputDll) + "\" \"" + Path.Combine(instanceExeFolder, xinputDllName) + "\"");
                    }
                }
            }

            if (!gen.XInputPlusNoIni)
            {
                List<string> textChanges = new List<string>();
                if (!player.IsKeyboardPlayer || (player.IsKeyboardPlayer && gen.PlayersPerInstance <= 1))
                {
                    if (setupDll)
                    {
                        addedFiles.Add(Path.Combine(instanceExeFolder, "XInputPlus.ini"));
                    }

                    Log("Copying XInputPlus.ini");
                    File.Copy(Path.Combine(utilFolder, "XInputPlus.ini"), Path.Combine(instanceExeFolder, "XInputPlus.ini"), true);

                    Log("Making changes to the lines in XInputPlus.ini; FileVersion and Controller values");

                    gen.XInputPlusDll = Array.ConvertAll(gen.XInputPlusDll, x => x.ToLower());
                    if (gen.XInputPlusDll.ToList().Any(val => val.StartsWith("dinput") == true)) //(xinputDll.ToLower().StartsWith("dinput"))
                    {
                        Log("A Dinput dll has been detected, also enabling X2Dinput in XInputPlus.ini");
                        textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "XInputPlus.ini"), "EnableX2Dinput=", SearchType.StartsWith) + "|EnableX2Dinput=True");
                    }

                    textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "XInputPlus.ini"), "FileVersion=", SearchType.StartsWith) + "|FileVersion=" + garch);

                    if (!player.IsKeyboardPlayer)
                    {
                        if (gen.PlayersPerInstance > 1)
                        {
                            for (int x = 1; x <= gen.PlayersPerInstance; x++)
                            {
                                textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "XInputPlus.ini"), "Controller" + x + "=", SearchType.StartsWith) + "|Controller" + x + "=" + (x + plyrIndex));
                            }
                            plyrIndex += gen.PlayersPerInstance;
                        }
                        else
                        {
                            textChanges.Add(context.FindLineNumberInTextFile(Path.Combine(instanceExeFolder, "XInputPlus.ini"), "Controller1=", SearchType.StartsWith) + "|Controller1=" + (player.GamepadId + 1));
                        }
                    }
                    else
                    {
                        Log("Skipping setting controller value for this instance, as this player is using keyboard");
                        kbi = 0;
                    }

                    context.ReplaceLinesInTextFile(Path.Combine(instanceExeFolder, "XInputPlus.ini"), textChanges.ToArray());
                }
            }

            Log("XInput Plus setup complete");
        }

        private void CreateSteamAppIdByExe(bool setupDll)
        {
            Log("Creating steam_appid.txt with steam ID " + gen.SteamID + " at " + instanceExeFolder);
            if (File.Exists(Path.Combine(instanceExeFolder, "steam_appid.txt")))
            {
                File.Delete(Path.Combine(instanceExeFolder, "steam_appid.txt"));
            }
            File.WriteAllText(Path.Combine(instanceExeFolder, "steam_appid.txt"), gen.SteamID);
            if (setupDll)
            {
                addedFiles.Add(Path.Combine(instanceExeFolder, "steam_appid.txt"));
            }
        }
        private void UseEACBypass(string linkFolder, bool setupDll)
        {
            if (setupDll)
            {
                Log("Starting EAC Bypass setup");

                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\EAC Bypass");

                string[] eac64DllFiles = Directory.GetFiles(linkFolder, "EasyAntiCheat_x64.dll", SearchOption.AllDirectories);
                foreach (string nameFile in eac64DllFiles)
                {
                    Log("Found " + nameFile);
                    string dir = Path.GetDirectoryName(nameFile);

                    FileCheck(nameFile);

                    File.Copy(Path.Combine(utilFolder, "EasyAntiCheat_x64.dll"), Path.Combine(dir, "EasyAntiCheat_x64.dll"), true);
                }

                string[] eac86DllFiles = Directory.GetFiles(linkFolder, "EasyAntiCheat_x86.dll", SearchOption.AllDirectories);
                foreach (string nameFile in eac86DllFiles)
                {
                    Log("Found " + nameFile);
                    string dir = Path.GetDirectoryName(nameFile);

                    FileCheck(nameFile);

                    File.Copy(Path.Combine(utilFolder, "EasyAntiCheat_x86.dll"), Path.Combine(dir, "EasyAntiCheat_x86.dll"), true);
                }
            }
        }

        private void UseNemirtingasEpicEmu(string rootFolder, string linkFolder, bool EpicEmuArgs, int i, PlayerInfo player, string epiclang, bool setupDll)
        {
            if (setupDll)
            {
                Log("Starting Nemirtingas Epic Emu setup");
                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\NemirtingasEpicEmu");
                string x86dll = "EOSSDK-Win32-Shipping.dll";
                string x64dll = "EOSSDK-Win64-Shipping.dll";

                string dllrootFolder = string.Empty;
                string dllFolder = string.Empty;
                string instanceDllFolder = string.Empty;

                Log("Generating emulator settings folder");
                try
                {
                    if (!Directory.Exists(Path.Combine(instanceExeFolder, "nepice_settings")))
                    {
                        Directory.CreateDirectory((Path.Combine(instanceExeFolder, "nepice_settings")));
                        Log("Emulator settings folder generated");
                    }
                }
                catch (Exception ex)
                {
                    Log("Nucleus is unable to generate the required emulator settings folder");
                }

                try
                {

                    string log;
                    if (i == 0)
                    {
                        log = "TRACE";
                    }
                    else
                    {
                        log = "OFF";
                    }

                    JObject emuSettings;

                    if (gen.AltEpicEmuArgs)
                    {
                        emuSettings = new JObject(
                        new JProperty("enable_overlay", false),
                        new JProperty("epicid", player.Nickname),
                        new JProperty("disable_online_networking", false),
                        new JProperty("enable_lan", true),
                        //new JProperty("log_level", log),
                        new JProperty("savepath", "appdata"),
                        new JProperty("unlock_dlcs", true),
                        new JProperty("language", epiclang),
                        new JProperty("username", player.Nickname)
                        );
                    }
                    else
                    {
                        emuSettings = new JObject(
                        new JProperty("enable_overlay", false),
                        //new JProperty("gamename", gen.GameName.ToLower()),
                        new JProperty("epicid", "0000000000000000000000000player" + (i + 1)),
                        new JProperty("disable_online_networking", false),
                        new JProperty("enable_lan", true),
                        // new JProperty("log_level", log),
                        new JProperty("savepath", "appdata"),
                        new JProperty("unlock_dlcs", true),
                        new JProperty("language", epiclang),
                        new JProperty("username", player.Nickname)
                        );
                    }

                    string jsonPath = Path.Combine(instanceExeFolder, "nepice_settings\\NemirtingasEpicEmu.json");

                    string oldjsonPath = Path.Combine(instanceExeFolder, "NemirtingasEpicEmu.json");//for older eos emu version

                    Log("Writing emulator settings NemirtingasEpicEmu.json");

                    File.WriteAllText(jsonPath, emuSettings.ToString());

                    File.WriteAllText(oldjsonPath, emuSettings.ToString());//for older eos emu version

                    if (setupDll)
                    {
                        addedFiles.Add(jsonPath);
                        addedFiles.Add(oldjsonPath);
                    }
                }
                catch (Exception ex)
                {
                    Log("Nucleus is unable to write the required NemirtingasEpicEmu.json file");
                }

                string[] steamDllFiles = Directory.GetFiles(rootFolder, "EOSSDK-Win*.dll", SearchOption.AllDirectories);
                foreach (string nameFile in steamDllFiles)
                {
                    Log("Found " + nameFile);
                    dllrootFolder = Path.GetDirectoryName(nameFile);

                    string tempRootFolder = rootFolder;
                    if (tempRootFolder.EndsWith("\\"))
                    {
                        tempRootFolder = tempRootFolder.Substring(0, tempRootFolder.Length - 1);
                    }
                    dllFolder = dllrootFolder.Remove(0, (tempRootFolder.Length));

                    instanceDllFolder = linkFolder.TrimEnd('\\') + "\\" + dllFolder.TrimStart('\\');

                    if (nameFile.EndsWith(x64dll, true, null))
                    {
                        FileCheck(Path.Combine(instanceDllFolder, x64dll));
                        try
                        {
                            Log("Placing Epic Emu " + x64dll + " in instance dll folder " + instanceDllFolder);
                            File.Copy(Path.Combine(utilFolder, "x64\\" + x64dll), Path.Combine(instanceDllFolder, x64dll), true);
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR - " + ex.Message);
                            Log("Using alternative copy method for " + x64dll);
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x64\\" + x64dll) + "\" \"" + Path.Combine(instanceDllFolder, x64dll) + "\"");
                        }
                    }

                    if (nameFile.EndsWith(x86dll, true, null))
                    {
                        FileCheck(Path.Combine(instanceDllFolder, x86dll));
                        try
                        {
                            Log("Placing Epic Emu " + x86dll + " in instance steam dll folder " + instanceDllFolder);
                            File.Copy(Path.Combine(utilFolder, "x86\\" + x86dll), Path.Combine(instanceDllFolder, x86dll), true);
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR - " + ex.Message);
                            Log("Using alternative copy method for " + x86dll);
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x86\\" + x86dll) + "\" \"" + Path.Combine(instanceDllFolder, x86dll) + "\"");
                        }
                    }
                }
            }

            Log("Epic Emu setup complete");
        }
        private void UseNemirtingasGalaxyEmu(string rootFolder, string linkFolder, int i, PlayerInfo player, string gogLang, bool setupDll)
        {
            if (setupDll)
            {
                Log("Starting Nemirtingas Galaxy Emu setup");
                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\NemirtingasGalaxyEmu");
                string x86dll = "Galaxy.dll";
                string x64dll = "Galaxy64.dll";

                string dllrootFolder = string.Empty;
                string dllFolder = string.Empty;
                string instanceDllFolder = string.Empty;

                Log("Generating emulator settings folder");
                try
                {
                    if (!Directory.Exists(Path.Combine(instanceExeFolder, "ngalaxye_settings")))
                    {
                        Directory.CreateDirectory((Path.Combine(instanceExeFolder, "ngalaxye_settings")));
                        Log("Emulator settings folder generated");
                    }
                }
                catch (Exception ex)
                {
                    Log("Nucleus is unable to generate the required emulator settings folder");
                }

                try
                {

                    string log;
                    if (i == 0)
                    {
                        log = "TRACE";
                    }
                    else
                    {
                        log = "OFF";
                    }

                    JObject emuSettings;
                    emuSettings = new JObject(
                    new JProperty("api_version", "1.100.2.0"),
                    new JProperty("disable_online_networking", false),
                    new JProperty("enable_lan", true),
                    new JProperty("enable_overlay", false),
                    new JProperty("galaxyid", 14601386556348240 + (i + 1)),
                    new JProperty("language", gogLang.ToLower()),
                    // new JProperty("log_level", log),
                    new JProperty("productid", 2104387650),
                    new JProperty("savepath", "appdata"),
                    new JProperty("unlock_dlcs", true),
                    new JProperty("username", player.Nickname)
                    );


                    string jsonPath = Path.Combine(instanceExeFolder, "ngalaxye_settings\\NemirtingasGalaxyEmu.json");


                    Log("Writing emulator settings NemirtingasGalaxyEmu.json");

                    File.WriteAllText(jsonPath, emuSettings.ToString());

                    if (setupDll)
                    {
                        addedFiles.Add(jsonPath);
                    }
                }
                catch (Exception ex)
                {
                    Log("Nucleus is unable to write the required NemirtingasGalaxyEmu.json file");
                }

                string[] steamDllFiles = Directory.GetFiles(rootFolder, "Galaxy*.dll", SearchOption.AllDirectories);
                foreach (string nameFile in steamDllFiles)
                {
                    Log("Found " + nameFile);
                    dllrootFolder = Path.GetDirectoryName(nameFile);

                    string tempRootFolder = rootFolder;
                    if (tempRootFolder.EndsWith("\\"))
                    {
                        tempRootFolder = tempRootFolder.Substring(0, tempRootFolder.Length - 1);
                    }
                    dllFolder = dllrootFolder.Remove(0, (tempRootFolder.Length));

                    instanceDllFolder = linkFolder.TrimEnd('\\') + "\\" + dllFolder.TrimStart('\\');

                    if (nameFile.EndsWith(x64dll, true, null))
                    {
                        FileCheck(Path.Combine(instanceDllFolder, x64dll));
                        try
                        {
                            Log("Placing Galaxy Emu " + x64dll + " in instance dll folder " + instanceDllFolder);
                            File.Copy(Path.Combine(utilFolder, "x64\\" + x64dll), Path.Combine(instanceDllFolder, x64dll), true);
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR - " + ex.Message);
                            Log("Using alternative copy method for " + x64dll);
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x64\\" + x64dll) + "\" \"" + Path.Combine(instanceDllFolder, x64dll) + "\"");
                        }
                    }

                    if (nameFile.EndsWith(x86dll, true, null))
                    {
                        FileCheck(Path.Combine(instanceDllFolder, x86dll));
                        try
                        {
                            Log("Placing Galaxy Emu " + x86dll + " in instance steam dll folder " + instanceDllFolder);
                            File.Copy(Path.Combine(utilFolder, "x86\\" + x86dll), Path.Combine(instanceDllFolder, x86dll), true);
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR - " + ex.Message);
                            Log("Using alternative copy method for " + x86dll);
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x86\\" + x86dll) + "\" \"" + Path.Combine(instanceDllFolder, x86dll) + "\"");
                        }
                    }
                }
            }

            Log("Galaxy Emu setup complete");
        }

        private void UseGoldberg(string rootFolder, string nucleusRootFolder, string linkFolder, int i, PlayerInfo player, List<PlayerInfo> players, bool setupDll)
        {

            Log("Starting Goldberg setup");
            string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\GoldbergEmu");
            string steamDllrootFolder = string.Empty;
            string steamDllFolder = string.Empty;
            string instanceSteamDllFolder = string.Empty;
            string instanceSteamSettingsFolder = string.Empty;
            string prevSteamDllFilePath = string.Empty;


            string steam64Dll = string.Empty;
            string steamDll = string.Empty;
            if (gen.GoldbergExperimental)
            {
                Log("Using experimental Goldberg");
                steam64Dll += "experimental\\";
                steamDll += "experimental\\";
            }
            steam64Dll += "steam_api64.dll";
            steamDll += "steam_api.dll";

            if (gen.GoldbergExperimentalSteamClient)
            {
                Log("Using Goldberg Experimental Steam Client");
                utilFolder += "\\experimental_steamclient";

                string exeFolder = Path.GetDirectoryName(exePath);

                FileCheck(Path.Combine(exeFolder, "steamclient.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient.dll"), Path.Combine(exeFolder, "steamclient.dll"));

                FileCheck(Path.Combine(exeFolder, "steamclient64.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient64.dll"), Path.Combine(exeFolder, "steamclient64.dll"));

                FileCheck(Path.Combine(exeFolder, "steamclient_loader.exe"));
                File.Copy(Path.Combine(utilFolder, "steamclient_loader.exe"), Path.Combine(exeFolder, "steamclient_loader.exe"));

                gen.ExecutableToLaunch = Path.Combine(exeFolder, "steamclient_loader.exe");
                gen.ForceProcessSearch = true;
                gen.GoldbergWriteSteamIDAndAccount = true;

                if (i == 0)
                {
                    startingArgs = gen.StartArguments;
                }

                var sb = new StringBuilder();
                string gblines = sb.Append("#My own modified version of ColdClientLoader originally by Rat431")
                                .AppendLine()
                                .Append("[SteamClient]")
                                .AppendLine()
                                .Append($"Exe={gen.ExecutableName}")
                                .AppendLine()
                                .Append($"ExeRunDir=.")
                                .AppendLine()
                                .Append($"ExeCommandLine={startingArgs}")
                                .AppendLine()
                                .Append($"AppId={gen.SteamID}")
                                .AppendLine()
                                .AppendLine()
                                .Append("SteamClientDll=steamclient.dll")
                                .AppendLine()
                                .Append("SteamClient64Dll=steamclient64.dll")
                                .ToString();
                File.WriteAllText(Path.Combine(exeFolder, "ColdClientLoader.ini"), gblines);
                addedFiles.Add(Path.Combine(exeFolder, "ColdClientLoader.ini"));

                //swalloing the launch arguments for the game here as we will be launching steamclient loader
                gen.StartArguments = string.Empty;
                context.StartArguments = string.Empty;

                string settingsFolder = exeFolder + "\\settings";
                if (gen.GoldbergNoLocalSave)
                {
                    if (gen.UseNucleusEnvironment)
                    {
                        settingsFolder = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                    else
                    {
                        settingsFolder = $@"{NucleusEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                }
                else
                {
                    File.WriteAllText(Path.Combine(exeFolder, "local_save.txt"), "");
                    addedFiles.Add(Path.Combine(exeFolder, "local_save.txt"));
                }

                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }

                File.WriteAllText(Path.Combine(settingsFolder, "user_steam_id.txt"), "");
                addedFiles.Add(Path.Combine(settingsFolder, "user_steam_id.txt"));

                File.WriteAllText(Path.Combine(settingsFolder, "account_name.txt"), "");
                addedFiles.Add(Path.Combine(settingsFolder, "account_name.txt"));

                string lang = "english";

                if (ini.IniReadValue("Misc", "SteamLang") != "" && ini.IniReadValue("Misc", "SteamLang") != "Automatic")
                {
                    gen.GoldbergLanguage = ini.IniReadValue("Misc", "SteamLang").ToLower();
                }

                if (gen.GoldbergLanguage?.Length > 0)
                {
                    lang = gen.GoldbergLanguage;
                }
                else
                {
                    lang = gen.GetSteamLanguage();
                }
                File.WriteAllText(Path.Combine(settingsFolder, "language.txt"), lang);
                addedFiles.Add(Path.Combine(settingsFolder, "language.txt"));
            }
            else
            {
                string[] steamDllFiles = Directory.GetFiles(rootFolder, "steam_api*.dll", SearchOption.AllDirectories);

                foreach (string nameFile in steamDllFiles)
                {
                    Log("Found " + nameFile);
                    steamDllrootFolder = Path.GetDirectoryName(nameFile);

                    string tempRootFolder = rootFolder;
                    if (tempRootFolder.EndsWith("\\"))
                    {
                        tempRootFolder = tempRootFolder.Substring(0, tempRootFolder.Length - 1);
                    }
                    steamDllFolder = steamDllrootFolder.Remove(0, (tempRootFolder.Length));

                    instanceSteamDllFolder = linkFolder.TrimEnd('\\') + "\\" + steamDllFolder.TrimStart('\\');

                    if (gen.UseNucleusEnvironment && gen.GoldbergNoLocalSave)
                    {
                        instanceSteamSettingsFolder = $@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                    else
                    {
                        instanceSteamSettingsFolder = Path.Combine(instanceSteamDllFolder, "settings");
                    }

                    Directory.CreateDirectory(instanceSteamSettingsFolder);
                    //Directory.CreateDirectory(instanceSteam_SettingsFolder);

                    if (setupDll)
                    {
                        if (nameFile.EndsWith("steam_api64.dll", true, null))
                        {
                            try
                            {
                                if (File.Exists(Path.Combine(instanceSteamDllFolder, "steam_api64.dll")))
                                {
                                    if (gen.GoldbergExperimental && gen.GoldbergExperimentalRename)
                                    {
                                        FileCheck(Path.Combine(instanceSteamDllFolder, "cracksteam_api64.dll"));

                                        Log("Renaming steam_api64.dll to cracksteam_api64.dll");
                                        File.Move(Path.Combine(instanceSteamDllFolder, "steam_api64.dll"), Path.Combine(instanceSteamDllFolder, "cracksteam_api64.dll"));
                                    }
                                    else
                                    {
                                        FileCheck(Path.Combine(instanceSteamDllFolder, "steam_api64.dll"));
                                    }
                                }
                                Log("Placing Goldberg steam_api64.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, steam64Dll), Path.Combine(instanceSteamDllFolder, "steam_api64.dll"), true);
                            }
                            catch (Exception ex)
                            {
                                Log("ERROR - " + ex.Message);
                                Log("Using alternative copy method for steam_api64.dll");
                                CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steam64Dll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api64.dll") + "\"");
                            }
                        }

                        if (nameFile.EndsWith("steam_api.dll", true, null))
                        {
                            try
                            {
                                if (File.Exists(Path.Combine(instanceSteamDllFolder, "steam_api.dll")))
                                {
                                    if (gen.GoldbergExperimental && gen.GoldbergExperimentalRename)
                                    {
                                        FileCheck(Path.Combine(instanceSteamDllFolder, "cracksteam_api.dll"));

                                        Log("Renaming steam_api.dll to cracksteam_api.dll");
                                        File.Move(Path.Combine(instanceSteamDllFolder, "steam_api.dll"), Path.Combine(instanceSteamDllFolder, "cracksteam_api.dll"));
                                    }
                                    else
                                    {
                                        FileCheck(Path.Combine(instanceSteamDllFolder, "steam_api.dll"));
                                    }
                                }
                                Log("Placing Goldberg steam_api.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, steamDll), Path.Combine(instanceSteamDllFolder, "steam_api.dll"), true);
                            }
                            catch (Exception ex)
                            {
                                Log("ERROR - " + ex.Message);
                                Log("Using alternative copy method for steam_api.dll");
                                CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steamDll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api.dll") + "\"");
                            }
                        }

                        if (gen.GoldbergExperimental)
                        {
                            FileCheck(Path.Combine(instanceSteamDllFolder, "steamclient.dll"));

                            Log("Placing Goldberg steamclient.dll in instance steam dll folder");
                            File.Copy(Path.Combine(utilFolder, "experimental\\steamclient.dll"), Path.Combine(instanceSteamDllFolder, "steamclient.dll"), true);
                            FileCheck(Path.Combine(instanceSteamDllFolder, "steamclient64.dll"));
                            Log("Placing Goldberg steamclient64.dll in instance steam dll folder");
                            File.Copy(Path.Combine(utilFolder, "experimental\\steamclient64.dll"), Path.Combine(instanceSteamDllFolder, "steamclient64.dll"), true);
                        }
                    }

                    if (!string.IsNullOrEmpty(prevSteamDllFilePath))
                    {
                        if (prevSteamDllFilePath == Path.GetDirectoryName(nameFile))
                        {
                            continue;
                        }
                    }
                    Log("New steam api folder found");
                    prevSteamDllFilePath = Path.GetDirectoryName(nameFile);

                    if (ini.IniReadValue("Misc", "UseNicksInGame") == "True" && !string.IsNullOrEmpty(player.Nickname))
                    {
                        if (setupDll)
                        {
                            addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                        }

                        File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                        Log("Generating account_name.txt with nickname " + player.Nickname);
                    }
                    else
                    {
                        if (setupDll)
                        {
                            addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                        }

                        if (ini.IniReadValue("Misc", "UseNicksInGame") == "True" && ini.IniReadValue("ControllerMapping", "Player_" + (i + 1)) != "")
                        {
                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                            Log("Generating account_name.txt with nickname " + player.Nickname);
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), "Player" + (i + 1));
                            Log("Generating account_name.txt with nickname Player " + (i + 1));
                        }
                    }

                    long steamID = random_steam_id + i;

                    bool useSettingsSID = false;

                    if (ini.IniReadValue("SteamIDs", "Player_" + (i + 1)) != "")
                    {
                        Log("Using steam ID from Nucleus settings ");
                        steamID = Convert.ToInt64(ini.IniReadValue("SteamIDs", "Player_" + (i + 1)));//ToString?   
                        useSettingsSID = true;
                    }

                    if (gen.PlayerSteamIDs != null && !useSettingsSID)
                    {
                        if (i < gen.PlayerSteamIDs.Length && !string.IsNullOrEmpty(gen.PlayerSteamIDs[i]))
                        {
                            Log("Using steam ID from handler");
                            steamID = long.Parse(gen.PlayerSteamIDs[i]);
                        }
                    }

                    Log("Generating user_steam_id.txt with user steam ID " + (steamID).ToString());

                    if (setupDll)
                    {
                        addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"));
                    }

                    File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"), (steamID).ToString());

                    if (setupDll)
                    {
                        string lang = "english";
                        if (ini.IniReadValue("Misc", "SteamLang") != "" && ini.IniReadValue("Misc", "SteamLang") != "Automatic")
                        {
                            gen.GoldbergLanguage = ini.IniReadValue("Misc", "SteamLang").ToLower();
                        }
                        if (gen.GoldbergLanguage?.Length > 0)
                        {
                            lang = gen.GoldbergLanguage;
                        }
                        else
                        {
                            lang = gen.GetSteamLanguage();
                        }
                        Log("Generating language.txt with language set to " + lang);
                        addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "language.txt"));
                        File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "language.txt"), lang);

                        if (gen.GoldbergIgnoreSteamAppId)
                        {
                            Log("Skipping steam_appid.txt creation");
                        }
                        else
                        {
                            Log("Generating steam_appid.txt using game steam ID " + gen.SteamID);
                            addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"));
                            File.WriteAllText(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"), gen.SteamID);
                        }

                        addedFiles.Add(Path.Combine(instanceSteamDllFolder, "local_save.txt"));
                        if (!gen.GoldbergNoLocalSave)
                        {
                            File.WriteAllText(Path.Combine(instanceSteamDllFolder, "local_save.txt"), "");
                        }

                        if (gen.GoldbergNeedSteamInterface)
                        {
                            addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"));
                            if (File.Exists(Path.Combine(utilFolder, "tools\\steam_interfaces.txt")))
                            {
                                Log("Found generated steam_interfaces.txt file in Nucleus util folder, copying this file");

                                File.Copy(Path.Combine(utilFolder, "tools\\steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (File.Exists(Path.Combine(steamDllrootFolder, "steam_interfaces.txt")))
                            {
                                Log("Found steam_interfaces.txt in original game folder, copying this file");
                                File.Copy(Path.Combine(steamDllrootFolder, "steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (File.Exists(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + gen.JsFileName.Substring(0, gen.JsFileName.Length - 3) + "\\steam_interfaces.txt"))
                            {
                                Log("Found steam_interfaces.txt in Nucleus game folder");
                                File.Copy(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + gen.JsFileName.Substring(0, gen.JsFileName.Length - 3) + "\\steam_interfaces.txt", Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (gen.OrigSteamDllPath?.Length > 0 && File.Exists(gen.OrigSteamDllPath))
                            {
                                Log("Attempting to create steam_interfaces.txt with the steam api dll located at: " + gen.OrigSteamDllPath);
                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.StartInfo.WorkingDirectory = Path.Combine(utilFolder, "tools");
                                cmd.Start();

                                cmd.StandardInput.WriteLine("generate_interfaces_file.exe \"" + gen.OrigSteamDllPath + "\"");
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_intterfaces.txt"));
                                Log("Copying over generated steam_interfaces.txt");
                                File.Copy(Path.Combine(Path.Combine(utilFolder, "tools"), "steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);

                                if ((i + 1) == players.Count)
                                {
                                    Log("Deleting generated steam_interfaces.txt file");
                                    File.Delete(Path.Combine(utilFolder, "tools\\steam_interfaces.txt"));
                                }
                            }
                            else
                            {
                                Log("Unable to locate steam_interfaces.txt or create one, skipping this file");
                                if (i == 0)
                                {
                                    MessageBox.Show("Goldberg was unable to locate steam_interfaces.txt or create one. Process will continue without using this file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }

                if (steamDllFiles == null || steamDllFiles.Length < 1)
                {
                    Log("Unable to locate a steam_api(64).dll file, Goldberg will not be used");
                    MessageBox.Show("Goldberg was unable to locate a steam_api(64).dll file. The built-in Goldberg will not be used.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            Log("Goldberg setup complete");
        }

        private void UseSteamStubDRMPatcher(string garch, bool setupDll)
        {
            if (setupDll)
            {
                string utilFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\Steam Stub DRM Patcher");

                string archToUse = garch;
                if (gen.SteamStubDRMPatcherArch?.Length > 0)
                {
                    archToUse = "x" + gen.SteamStubDRMPatcherArch;
                }

                FileCheck(Path.Combine(instanceExeFolder, "winmm.dll"));
                try
                {
                    Log(string.Format("Copying over winmm.dll ({0})", archToUse));
                    File.Copy(Path.Combine(utilFolder, archToUse + "\\winmm.dll"), Path.Combine(instanceExeFolder, "winmm.dll"), true);
                }

                catch (Exception ex)
                {
                    Log("ERROR - " + ex.Message);
                    Log("Using alternative copy method for winmm.dll");
                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, archToUse + "\\winmm.dll") + "\" \"" + Path.Combine(instanceExeFolder, "winmm.dll") + "\"");
                }
            }
        }

        private void HexEditFile(GenericContext context, int i, string linkFolder)
        {
            string[] splitValues = gen.HexEditFile[i].Split('|');
            if (splitValues.Length == 3)
            {
                string filePath = splitValues[0];
                string fullPath = Path.Combine(Path.Combine(linkFolder, filePath));
                string fullFileName = Path.GetFileName(filePath);
                string strToSearch = splitValues[1];
                string replacedStr = splitValues[2];
                Log(string.Format("HexEditFile - Patching file: {0}", filePath));

                bool origExists = false;
                if (File.Exists(Path.GetDirectoryName(fullPath) + "\\" + Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)))
                {
                    origExists = true;
                }

                if (origExists)
                {
                    Log(string.Format("Temporarily renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                    File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                    Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                    context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-TEMP" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                    Log(string.Format("Deleting temporary file {0}", Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                    File.Delete(Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                }
                else
                {
                    Log(string.Format("Renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-ORIG" + Path.GetExtension(filePath)));
                    File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)));
                    Log(string.Format("Created patched file {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                    context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-ORIG" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                }
            }
            else
            {
                Log("Invalid # of parameters provided for: " + gen.HexEditFile[i] + ", skipping");
            }
            Log("Patching executable complete");
        }

        private void HexEditAllFiles(GenericContext context, string linkFolder)
        {
            foreach (string asciiValues in gen.HexEditAllFiles)
            {
                string[] splitValues = asciiValues.Split('|');
                if (splitValues.Length == 3)
                {
                    string filePath = splitValues[0];
                    string fullPath = Path.Combine(Path.Combine(linkFolder, filePath));
                    string fullFileName = Path.GetFileName(filePath);
                    string strToSearch = splitValues[1];
                    string replacedStr = splitValues[2];
                    Log(string.Format("HexEditAllFiles - Patching file: {0}", filePath));

                    bool origExists = false;
                    if (File.Exists(Path.GetDirectoryName(fullPath) + "\\" + Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)))
                    {
                        origExists = true;
                    }

                    if (origExists)
                    {
                        Log(string.Format("Temporarily renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                        File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                        context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-TEMP" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                        Log(string.Format("Deleting temporary file {0}", Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                        File.Delete(Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                    }
                    else
                    {
                        Log(string.Format("Renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-ORIG" + Path.GetExtension(filePath)));
                        File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)));
                        Log(string.Format("Created patched file {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                        context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-ORIG" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                    }
                }
                else
                {
                    Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                }
            }
            Log("Patching executable complete");
        }

        private void HexEditExe(GenericContext context, int i)
        {
            Log("HexEditExe - Patching individual executable");
            bool origExists = false;
            if (File.Exists(Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-ORIG.exe")))
            {
                origExists = true;
            }

            if (origExists)
            {
                string[] splitValues = gen.HexEditExe[i].Split('|');
                if (splitValues.Length == 2)
                {
                    Log(string.Format("Temporarily renaming original executable {0} to {1}", gen.ExecutableName, Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-TEMP.exe"));
                    File.Move(exePath, Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-TEMP.exe"));
                    Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", gen.ExecutableName, splitValues[0], splitValues[1]));
                    context.PatchFile(exePath.Substring(0, exePath.Length - 4) + "-TEMP.exe", exePath, splitValues[0], splitValues[1]);
                    Log(string.Format("Deleting temporary executable {0}", Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-TEMP.exe"));
                    File.Delete(Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-TEMP.exe"));
                }
                else
                {
                    if (string.IsNullOrEmpty(gen.HexEditExe[i]))
                    {
                        Log("Nothing to change for this instance's executable");
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + gen.HexEditFile[i] + ", skipping");
                    }
                }
            }
            else
            {
                string[] splitValues = gen.HexEditExe[i].Split('|');
                if (splitValues.Length == 2)
                {
                    Log(string.Format("Renaming original executable {0} to {1}", gen.ExecutableName, Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-ORIG.exe"));
                    File.Move(exePath, Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-ORIG.exe"));
                    Log(string.Format("Created patched executable {0} where {1} has been replaced with {2}", gen.ExecutableName, splitValues[0], splitValues[1]));
                    context.PatchFile(exePath.Substring(0, exePath.Length - 4) + "-ORIG.exe", exePath, splitValues[0], splitValues[1]);
                }
                else
                {
                    if (string.IsNullOrEmpty(gen.HexEditExe[i]))
                    {
                        Log("Nothing to change for this instance's executable");
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + gen.HexEditFile[i] + ", skipping");
                    }
                }
            }
            Log("Patching executable complete");
        }

        private void HexEditAllExes(GenericContext context, int i)
        {
            Log("HexEditAllExes - Patching executable");

            bool origExists = false;
            if (File.Exists(Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-ORIG.exe")))
            {
                origExists = true;
            }

            foreach (string asciiValues in gen.HexEditAllExes)
            {
                if (origExists)
                {
                    string[] splitValues = asciiValues.Split('|');
                    if (splitValues.Length == 2)
                    {
                        Log(string.Format("Temporarily renaming original executable {0} to {1}", gen.ExecutableName, Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-TEMP.exe"));
                        File.Move(exePath, Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-TEMP.exe"));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", gen.ExecutableName, splitValues[0], splitValues[1]));
                        context.PatchFile(exePath.Substring(0, exePath.Length - 4) + "-TEMP.exe", exePath, splitValues[0], splitValues[1]);
                        Log(string.Format("Deleting temporary executable {0}", Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-TEMP.exe"));
                        File.Delete(Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-TEMP.exe"));
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                    }
                }
                else
                {
                    string[] splitValues = asciiValues.Split('|');
                    if (splitValues.Length == 2)
                    {
                        Log(string.Format("Renaming original executable {0} to {1}", gen.ExecutableName, Path.GetFileNameWithoutExtension(gen.ExecutableName) + "-ORIG.exe"));
                        File.Move(exePath, Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "-ORIG.exe"));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", gen.ExecutableName, splitValues[0], splitValues[1]));
                        context.PatchFile(exePath.Substring(0, exePath.Length - 4) + "-ORIG.exe", exePath, splitValues[0], splitValues[1]);
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                    }
                }
            }
            Log("Patching executable complete");
        }

        private void HexEditExeAddress(string exePath, int i)
        {
            if (gen.SymlinkExe)
            {
                Log("Skipping HexEditExeAddress, " + Path.GetFileName(exePath) + " is symlinked");
                return;
            }

            Log("HexEditExeAddress - Patching executable, " + Path.GetFileName(exePath) + ", in instance folder");

            if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
            {
                string fileBackup = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileNameWithoutExtension(exePath) + "_NUCLEUS_BACKUP.exe");
                if (File.Exists(exePath) && !File.Exists(fileBackup))
                {
                    try
                    {
                        File.Copy(exePath, fileBackup);
                        backupFiles.Add(fileBackup);
                        Log($"Backing up file {Path.GetFileName(exePath)} as {Path.GetFileName(fileBackup)}");
                    }
                    catch
                    { }
                }
            }

            foreach (string hexSplitLine in gen.HexEditExeAddress)
            {
                string[] hexSplit = hexSplitLine.Split('|');
                int indexOffset = 1;
                if (hexSplit.Length == 3)
                {
                    if (int.Parse(hexSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (hexSplit.Length == 1)
                {
                    Log("Invalid # of parameters provided for: " + hexSplitLine + ", skipping");
                    continue;
                }

                Log(string.Format("Bytes at address '{0}' to be replaced with '{1}'", hexSplit[1 - indexOffset], hexSplit[2 - indexOffset]));
                List<byte> bytesConv = new List<byte>();
                for (int s = 0; s < hexSplit[2 - indexOffset].Length; s += 2)
                {
                    bytesConv.Add(Convert.ToByte(hexSplit[2 - indexOffset].Substring(s, 2), 16));
                }
                byte[] bArray = bytesConv.ToArray();

                using (Stream stream = File.Open(exePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Position = long.Parse(hexSplit[1 - indexOffset], NumberStyles.HexNumber);
                    stream.Write(bArray, 0, bArray.Length);
                }
            }
        }

        private void HexEditFileAddress(string linkFolder, int i)
        {
            foreach (string hexSplitLine in gen.HexEditFileAddress)
            {
                string[] hexSplit = hexSplitLine.Split('|');
                int indexOffset = 1;
                if (hexSplit.Length == 4)
                {
                    if (int.Parse(hexSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (hexSplit.Length <= 2)
                {
                    Log("Invalid # of parameters provided for: " + hexSplitLine + ", skipping hex edit file address");
                    continue;
                }

                string fullFilePath = Path.Combine(linkFolder, hexSplit[1 - indexOffset]);
                FileInfo pathInfo = new FileInfo(fullFilePath);
                if (pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    Log("Skipping HexEditFileAddress, " + Path.GetFileName(hexSplit[1 - indexOffset]) + " is symlinked");
                    continue;
                }

                if (File.Exists(fullFilePath))
                {
                    if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
                    {
                        string fileBackup = Path.Combine(Path.GetDirectoryName(fullFilePath), Path.GetFileNameWithoutExtension(fullFilePath) + "_NUCLEUS_BACKUP" + Path.GetExtension(fullFilePath));
                        if (File.Exists(fullFilePath) && !File.Exists(fileBackup))
                        {
                            try
                            {
                                File.Copy(fullFilePath, fileBackup);
                                backupFiles.Add(fileBackup);
                                Log($"Backing up file {Path.GetFileName(fullFilePath)} as {Path.GetFileName(fileBackup)}");
                            }
                            catch
                            { }
                        }
                    }

                    Log("HexEditFileAddress - Patching file, " + Path.GetFileName(fullFilePath) + ", in instance folder");
                    Log(string.Format("Bytes at address '{0}' to be replaced with '{1}'", hexSplit[2 - indexOffset], hexSplit[3 - indexOffset]));
                    List<byte> bytesConv = new List<byte>();
                    for (int s = 0; s < hexSplit[3 - indexOffset].Length; s += 2)
                    {
                        bytesConv.Add(Convert.ToByte(hexSplit[3 - indexOffset].Substring(s, 2), 16));
                    }
                    byte[] bArray = bytesConv.ToArray();

                    using (Stream stream = File.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        stream.Position = long.Parse(hexSplit[2 - indexOffset], NumberStyles.HexNumber);
                        stream.Write(bArray, 0, bArray.Length);
                    }
                }
                else
                {
                    Log("ERROR - Could not find file: " + fullFilePath + " to patch");
                }
            }
        }

        private void UserProfileConfigCopy(PlayerInfo player)
        {
            string nucConfigPath = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\", gen.UserProfileConfigPath);
            string realConfigPath = Path.Combine(NucleusEnvironmentRoot, gen.UserProfileConfigPath);
            if ((!Directory.Exists(nucConfigPath) && Directory.Exists(realConfigPath)) || (Directory.Exists(realConfigPath) && gen.ForceUserProfileConfigCopy))
            {
                Directory.CreateDirectory(nucConfigPath);

                Log("Config path " + gen.UserProfileConfigPath + " does not exist for Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucConfigPath, dir.Substring(realConfigPath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)));
                        Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        private void UserProfileSaveCopy(PlayerInfo player)
        {
            string nucSavePath = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{player.Nickname}\", gen.UserProfileSavePath);
            string realSavePath = Path.Combine(NucleusEnvironmentRoot, gen.UserProfileSavePath);
            if ((!Directory.Exists(nucSavePath) && Directory.Exists(realSavePath)) || (Directory.Exists(realSavePath) && gen.ForceUserProfileSaveCopy))
            {
                Directory.CreateDirectory(nucSavePath);

                Log("Save path " + gen.UserProfileConfigPath + " does not exist in Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realSavePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucSavePath, dir.Substring(realSavePath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realSavePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)));
                        Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        //Log("ERROR - " + ex.Message);
                        Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        private void DocumentsConfigCopy(PlayerInfo player)
        {
            string nucConfigPath = Path.Combine($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\{player.Nickname}\Documents\", gen.DocumentsConfigPath);
            string realConfigPath = Path.Combine(DocumentsRoot, gen.DocumentsConfigPath);
            if ((!Directory.Exists(nucConfigPath) && Directory.Exists(realConfigPath)) || (Directory.Exists(realConfigPath) && gen.ForceDocumentsConfigCopy))
            {
                Directory.CreateDirectory(nucConfigPath);

                Log("Config path " + gen.DocumentsConfigPath + " does not exist for Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucConfigPath, dir.Substring(realConfigPath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)));
                        Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        //Log("ERROR - " + ex.Message);
                        Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        private void DocumentsSaveCopy(PlayerInfo player)
        {
            string nucSavePath = Path.Combine($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\{player.Nickname}\Documents\", gen.DocumentsSavePath);
            string realSavePath = Path.Combine(DocumentsRoot, gen.DocumentsSavePath);
            if ((!Directory.Exists(nucSavePath) && Directory.Exists(realSavePath)) || (Directory.Exists(realSavePath) && gen.ForceDocumentsSaveCopy))
            {
                Directory.CreateDirectory(nucSavePath);

                Log("Save path " + gen.DocumentsConfigPath + " does not exist in Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realSavePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucSavePath, dir.Substring(realSavePath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realSavePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)));
                        Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        private void ChangeExe(int i)
        {
            string newExe = Path.GetFileNameWithoutExtension(userGame.Game.ExecutableName) + " - Player " + (i + 1) + ".exe";

            if (File.Exists(Path.Combine(instanceExeFolder, userGame.Game.ExecutableName)))
            {
                if (File.Exists(Path.Combine(instanceExeFolder, newExe)))
                {
                    File.Delete(Path.Combine(instanceExeFolder, newExe));
                }
                File.Copy(Path.Combine(instanceExeFolder, userGame.Game.ExecutableName), Path.Combine(instanceExeFolder, newExe));
                Log("Changed game executable from " + gen.ExecutableName + " to " + newExe);
            }

            if (File.Exists(Path.Combine(instanceExeFolder, newExe)))
            {
                exePath = Path.Combine(instanceExeFolder, newExe);
                Log("Using " + newExe + " as the game executable");

                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
                {
                    Log($"{newExe} will be deleted upon ending session");
                    addedFiles.Add(Path.Combine(instanceExeFolder, newExe));
                }
                else
                {
                    if (File.Exists(Path.Combine(instanceExeFolder, userGame.Game.ExecutableName)))
                    {
                        File.Delete(Path.Combine(instanceExeFolder, userGame.Game.ExecutableName));
                    }
                }
            }
        }

        public string GetUserSID(string userName)
        {
            var principalContext = new PrincipalContext(ContextType.Machine);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, userName);
            if (userPrincipal != null)
            {
                var userSid = userPrincipal.Sid;
                return userSid.ToString();
            }

            return null;
        }

        private static ArrayList GetUserGroups(string sUserName)
        {
            ArrayList myItems = new ArrayList();
            UserPrincipal oUserPrincipal = GetUser(sUserName);

            if (oUserPrincipal != null)
            {
                PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetGroups();

                if (oPrincipalSearchResult != null && oPrincipalSearchResult?.ToList().Count > 0)
                {
                    foreach (Principal oResult in oPrincipalSearchResult)
                    {
                        myItems.Add(oResult.Name);
                    }
                }
                else
                {
                    LogManager.Log("ERROR - Principal Search Result is null");
                }
            }
            else
            {
                LogManager.Log("ERROR - User Principal is null");
            }

            if (myItems.Count == 0)
            {
                LogManager.Log("Error grabbing user groups for user: " + sUserName);
            }

            return myItems;
        }

        private static UserPrincipal GetUser(string sUserName)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext();

            UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
            return oUserPrincipal;
        }
        private static PrincipalContext GetPrincipalContext()
        {
            PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Machine);
            return oPrincipalContext;
        }

        private void StatusForm_Closing(object sender, FormClosingEventArgs e)
        {
            if (!processingExit)
            {
                Thread.Sleep(5000);
            }
        }

        void ExportRegistry(string strKey, string filepath)
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
                    proc.Start();
                    string stdout = proc.StandardOutput.ReadToEnd();
                    string stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                }
            }
            catch (Exception)
            {
            }
        }

        private void FileCheck(string file)
        {
            if (File.Exists(file))
            {
                if (!gen.SymlinkGame && !gen.HardlinkGame && !gen.HardcopyGame)
                {
                    string fileBackup = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_NUCLEUS_BACKUP" + Path.GetExtension(file));
                    if (!File.Exists(fileBackup))
                    {
                        try
                        {
                            File.Move(file, fileBackup);
                            backupFiles.Add(fileBackup);
                            Log($"Backing up file {Path.GetFileName(file)} as {Path.GetFileName(fileBackup)}");
                        }
                        catch
                        { }
                    }
                }
                else
                {
                    Log($"Deleting {Path.GetFileName(file)}");
                    File.Delete(file);
                }
            }
            else
            {
                Log($"{Path.GetFileName(file)} doesn't exist, will be deleted upon ending session");
                addedFiles.Add(file);
            }
        }

        public void SwitchProcessTo(string deviceId, ERole role, EDataFlow flow, uint processId)
        {
            var roles = new[]
           {
                ERole.eConsole,
                ERole.eCommunications,
                ERole.eMultimedia
            };

            if (role != ERole.ERole_enum_count)
            {
                roles = new[]
                {
                    role
                };
            }

            ComThread.Invoke((() =>
            {
                ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId);
            }));
        }

        /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
        private BlockingCollection<Task> _tasks;

        internal sealed class ComTaskScheduler : TaskScheduler, IDisposable
        {
            /// <summary>The STA threads used by the scheduler.</summary>
            private readonly Thread _thread;

            /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
            private BlockingCollection<Task> _tasks;

            /// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
            public ComTaskScheduler()
            {
                // Initialize the tasks collection
                _tasks = new BlockingCollection<Task>();

                _thread = new Thread(() =>
                {
                    // Continually get the next task and try to execute it.
                    // This will continue until the scheduler is disposed and no more tasks remain.
                    foreach (var t in _tasks.GetConsumingEnumerable())
                    {
                        TryExecuteTask(t);
                    }

                    //lightweight pump of the thread
                    Thread.CurrentThread.Join(1);
                })
                { Name = "ComThread", IsBackground = true };

                _thread.SetApartmentState(ApartmentState.STA);

                // Start all of the threads
                _thread.Start();
            }

            public int ThreadId => _thread?.ManagedThreadId ?? -1;

            /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
            public override int MaximumConcurrencyLevel => 1;

            /// <summary>
            ///     Cleans up the scheduler by indicating that no more tasks will be queued.
            ///     This method blocks until all threads successfully shutdown.
            /// </summary>
            public void Dispose()
            {
                if (_tasks == null) return;

                // Indicate that no new tasks will be coming in
                _tasks.CompleteAdding();

                _thread.Join();

                // Cleanup
                _tasks.Dispose();
                _tasks = null;
            }

            /// <summary>Queues a Task to be executed by this scheduler.</summary>
            /// <param name="task">The task to be executed.</param>
            protected override void QueueTask(Task task)
            {
                // Push it into the blocking collection of tasks
                _tasks.Add(task);
            }

            /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
            /// <returns>An enumerable of all tasks currently scheduled.</returns>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                // Serialize the contents of the blocking collection of tasks for the debugger
                return _tasks.ToArray();
            }

            /// <summary>Determines whether a Task may be inlined.</summary>
            /// <param name="task">The task to be executed.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
            /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                //Never run inline, it HAS to be run on the COM thread
                return false;
            }
        }

        internal static class ComThread
        {
            private static bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != Scheduler.ThreadId;
            private static ComTaskScheduler Scheduler { get; } = new ComTaskScheduler();

            /// <summary>
            /// Asserts that the execution following this statement is running on the ComThreads
            /// <exception cref="InvalidThreadException">Thrown if the assertion fails</exception>
            /// </summary>
            public static void Assert()
            {
                if (InvokeRequired)
                    throw new Exception($"(InvalidThread)This operation must be run on the ComThread ThreadId: {Scheduler.ThreadId}");
            }

            public static void Invoke(Action action)
            {
                if (!InvokeRequired)
                {
                    action();
                    return;
                }

                BeginInvoke(action).Wait();
            }

            private static Task BeginInvoke(Action action)
            {
                return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, Scheduler);
            }

            public static T Invoke<T>(Func<T> func)
            {
                return !InvokeRequired ? func() : BeginInvoke(func).GetAwaiter().GetResult();
            }

            private static Task<T> BeginInvoke<T>(Func<T> func)
            {
                return Task<T>.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, Scheduler);
            }
        }

        private ExtendedPolicyClient _extendedPolicyClient;

        private ExtendedPolicyClient ExtendPolicyClient
        {
            get
            {
                if (_extendedPolicyClient != null)
                {
                    return _extendedPolicyClient;
                }

                return _extendedPolicyClient = ComThread.Invoke(() => new ExtendedPolicyClient());
            }
        }

        public enum ERole : uint
        {
            eConsole = 0,
            eMultimedia = (eConsole + 1),
            eCommunications = (eMultimedia + 1),
            ERole_enum_count = (eCommunications + 1)
        }

        public enum EDataFlow : uint
        {
            eRender = 0,
            eCapture = (eRender + 1),
            eAll = (eCapture + 1),
            EDataFlow_enum_count = (eAll + 1)
        }

        public enum HRESULT : uint
        {
            S_OK = 0x0,
            S_FALSE = 0x1,
            AUDCLNT_E_DEVICE_INVALIDATED = 0x88890004,
            AUDCLNT_S_NO_SINGLE_PROCESS = 0x889000d,
            ERROR_NOT_FOUND = 0x80070490,
        }

        [Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
        [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        public interface IAudioPolicyConfigFactory
        {
            int __incomplete__add_CtxVolumeChange();
            int __incomplete__remove_CtxVolumeChanged();
            int __incomplete__add_RingerVibrateStateChanged();
            int __incomplete__remove_RingerVibrateStateChange();
            int __incomplete__SetVolumeGroupGainForId();
            int __incomplete__GetVolumeGroupGainForId();
            int __incomplete__GetActiveVolumeGroupForEndpointId();
            int __incomplete__GetVolumeGroupsForEndpoint();
            int __incomplete__GetCurrentVolumeContext();
            int __incomplete__SetVolumeGroupMuteForId();
            int __incomplete__GetVolumeGroupMuteForId();
            int __incomplete__SetRingerVibrateState();
            int __incomplete__GetRingerVibrateState();
            int __incomplete__SetPreferredChatApplication();
            int __incomplete__ResetPreferredChatApplication();
            int __incomplete__GetPreferredChatApplication();
            int __incomplete__GetCurrentChatApplications();
            int __incomplete__add_ChatContextChanged();
            int __incomplete__remove_ChatContextChanged();
            [PreserveSig]
            HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
            [PreserveSig]
            HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
            [PreserveSig]
            HRESULT ClearAllPersistedApplicationDefaultEndpoints();
        }

        internal sealed class ComBase
        {
            [DllImport("combase.dll", PreserveSig = false)]
            public static extern void RoGetActivationFactory(
                [MarshalAs(UnmanagedType.HString)] string activatableClassId,
                [In] ref Guid iid,
                [Out, MarshalAs(UnmanagedType.IInspectable)] out object factory);

            [DllImport("combase.dll", PreserveSig = false)]
            public static extern void WindowsCreateString(
                [MarshalAs(UnmanagedType.LPWStr)] string src,
                [In] uint length,
                [Out] out IntPtr hstring);
        }

        internal sealed class AudioPolicyConfigFactory
        {
            public static IAudioPolicyConfigFactory Create()
            {
                var iid = typeof(IAudioPolicyConfigFactory).GUID;
                ComBase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
                return (IAudioPolicyConfigFactory)factory;
            }
        }

        internal class ExtendedPolicyClient
        {
            private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
            private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
            private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

            private IAudioPolicyConfigFactory _sharedPolicyConfig;
            private IAudioPolicyConfigFactory PolicyConfig
            {
                get
                {
                    if (_sharedPolicyConfig != null)
                    {
                        return _sharedPolicyConfig;
                    }

                    return _sharedPolicyConfig = AudioPolicyConfigFactory.Create();
                }
            }

            private static string GenerateDeviceId(string deviceId, EDataFlow flow)
            {
                return $"{MMDEVAPI_TOKEN}{deviceId}{(flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
            }

            private static string UnpackDeviceId(string deviceId)
            {
                if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
                if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
                if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);

                return deviceId;
            }

            internal class ErrorConst
            {
                //FROM: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                public const ushort COM_ERROR_NOT_FOUND = 1168;
                public const int COM_ERROR_MASK = 0xFFFF;
            }

            public void SetDefaultEndPoint(string deviceId, EDataFlow flow, IEnumerable<ERole> roles, uint processId)
            {
                LogManager.Log($"ExtendedPolicyClient SetDefaultEndPoint {deviceId} [{flow}] {processId}");
                try
                {
                    var stringPtrDeviceId = IntPtr.Zero;

                    if (!string.IsNullOrWhiteSpace(deviceId))
                    {
                        var str = GenerateDeviceId(deviceId, flow);
                        ComBase.WindowsCreateString(str, (uint)str.Length, out stringPtrDeviceId);
                    }

                    foreach (var eRole in roles)
                    {
                        PolicyConfig.SetPersistedDefaultAudioEndpoint(processId, flow, eRole, stringPtrDeviceId);
                    }
                }
                catch (COMException e) when ((e.ErrorCode & ErrorConst.COM_ERROR_MASK) == ErrorConst.COM_ERROR_NOT_FOUND)
                {
                    //throw new DeviceNotFoundException($"Can't set default as {deviceId}", e, deviceId);
                    LogManager.Log($"(DeviceNotFound) Can't set default as {deviceId} - {e.Message}");
                }
                catch (Exception ex)
                {
                    LogManager.Log($"{ex}");
                }
            }

            /// <summary>
            /// Get the deviceId of the current DefaultEndpoint
            /// </summary>
            public string GetDefaultEndPoint(EDataFlow flow, ERole role, uint processId)
            {
                try
                {
                    PolicyConfig.GetPersistedDefaultAudioEndpoint(processId, flow, role, out string deviceId);
                    return UnpackDeviceId(deviceId);
                }
                catch (Exception ex)
                {
                    LogManager.Log($"{ex}");
                }

                return null;
            }

            public void ResetAllSetEndpoint()
            {
                try
                {
                    PolicyConfig.ClearAllPersistedApplicationDefaultEndpoints();
                }
                catch (Exception ex)
                {
                    LogManager.Log($"{ex}");
                }
            }
        }
    }
}