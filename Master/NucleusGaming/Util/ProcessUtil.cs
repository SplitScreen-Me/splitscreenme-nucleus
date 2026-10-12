using Microsoft.Win32;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Interop;
using Nucleus.Gaming.Tools.Steam;
using Nucleus.Gaming.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;

namespace Nucleus
{
    public static class ProcessUtil
    {
        private static string UserEnvironmentRoot = Globals.UserEnvironmentRoot;

        private static void Log(string logMessage) => DebugLogFile.Write("PROCESSUTIL", logMessage);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
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

        [Flags]

        public enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateProcess(
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            Int64 lpBaseAddress,
            [In, Out] Byte[] lpBuffer,
            UInt64 dwSize,
            out IntPtr lpNumberOfBytesWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
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
        public struct PROCESS_INFORMATION
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

        public static bool RunOrphanProcess(string path, /*bool runAdmin,*/ bool useNucleusEnvironment, string playerNickname, string arguments = "")
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            //cmd.StartInfo.RedirectStandardOutput = true;
            //cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            if (useNucleusEnvironment)
            {
                Log("Setting up Nucleus environment");
                //var username = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(@"C:\Users\", "");
                string username = Environment.UserName;
                try
                {
                    username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                }
                catch (Exception)
                {
                    Log("ERROR - getting current user's username, defaulting to using environment's username");
                }

                cmd.StandardInput.WriteLine($@"set APPDATA={UserEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Roaming");
                cmd.StandardInput.WriteLine($@"set LOCALAPPDATA={UserEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Local");
                cmd.StandardInput.WriteLine($@"set USERPROFILE={UserEnvironmentRoot}\NucleusCoop\{playerNickname}");
                cmd.StandardInput.WriteLine($@"set HOMEPATH=\Users\{username}\NucleusCoop\{playerNickname}");

                //Some games will crash if the directories don't exist
                Directory.CreateDirectory($@"{UserEnvironmentRoot}\NucleusCoop");
                Directory.CreateDirectory($@"{UserEnvironmentRoot}\NucleusCoop\{playerNickname}");
                Directory.CreateDirectory($@"{UserEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Roaming");
                Directory.CreateDirectory($@"{UserEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Local");

                //Directory.CreateDirectory($@"\Users\{username}\NucleusCoop\{playerNickname}");
                Directory.CreateDirectory($@"{UserEnvironmentRoot}\NucleusCoop\{playerNickname}\Documents");

                Directory.CreateDirectory(Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{playerNickname}\Documents");

                if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg")))
                {
                    //string mydocPath = key.GetValue("Personal").ToString();
                    RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                }

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                key.SetValue("Personal", Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{playerNickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
            }

            cmd.StandardInput.WriteLine("\"" + path + "\" " + arguments);

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            cmd.WaitForExit();

            return true;

            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.FileName = @"cmd.exe";
            //psi.Arguments = "/C \"" + path + "\" " + arguments;
            ////if (runAdmin)
            ////{
            ////    psi.Verb = "runas";
            ////}
            //return Process.Start(psi);
            ////ThreadPool.QueueUserWorkItem(KillParent, p);
        }


        private static void KillParent(object state)
        {
            Process p = (Process)state;
        }

        public static bool KillMutex(Process process, string mutexType, string mutexName, bool partial)
        {
            if (mutexName.ToLower().StartsWith("type:") && mutexName.Contains("|"))
            {
                mutexType = mutexName.Substring("type:".Length, mutexName.IndexOf('|') - "type:".Length);
                mutexName = mutexName.Substring(mutexName.IndexOf('|') + 1);
            }

            Log(string.Format("Attempting to kill mutex"));
            // 4 tries
            for (int i = 1; i < 4; i++)
            {
                Log("Attempt #" + i);
                Console.WriteLine("Loop " + i);

                if (partial)
                {
                    List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = Win32Processes.GetHandles(process, mutexType);
                    foreach (Win32API.SYSTEM_HANDLE_INFORMATION handle in handles)
                    {
                        string strObjectName = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));

                        if (!string.IsNullOrWhiteSpace(strObjectName) && strObjectName.Contains(mutexName))
                        {
                            Log(string.Format("Killing mutex located at '{0}'", strObjectName));
                            IntPtr ipHandle = IntPtr.Zero;
                            if (!Win32API.DuplicateHandle(Process.GetProcessById(handle.ProcessID).Handle, handle.Handle, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DUPLICATE_CLOSE_SOURCE))
                            {
                                Log(string.Format("DuplicateHandle() failed, error = {0}", Marshal.GetLastWin32Error()));
                                Console.WriteLine("DuplicateHandle() failed, error = {0}", Marshal.GetLastWin32Error());
                            }
                            else
                            {
                                Log("Mutex was killed successfully");
                                return true;
                            }
                        }
                        //Log("----------------END-----------------");
                    }
                    return true;
                }
                else
                {
                    List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = Win32Processes.GetHandles(process, mutexType, "", mutexName);

                    if (handles.Count == 0)
                    {
                        Log(string.Format("{0} not found in process handles", mutexName));
                        continue;
                    }

                    foreach (Win32API.SYSTEM_HANDLE_INFORMATION handle in handles)
                    {
                        string strObjectName = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));
                        Log(string.Format("Killing mutex {0}", strObjectName));
                        if (!string.IsNullOrWhiteSpace(strObjectName) && strObjectName.EndsWith(mutexName))
                        {
                            IntPtr ipHandle = IntPtr.Zero;
                            if (!Win32API.DuplicateHandle(Process.GetProcessById(handle.ProcessID).Handle, handle.Handle, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DUPLICATE_CLOSE_SOURCE))
                            {
                                Console.WriteLine("DuplicateHandle() failed, error = {0}", Marshal.GetLastWin32Error());
                            }
                            else
                            {
                                Log("Mutex was killed successfully");
                                return true;
                            }
                        }

                    }
                }
            }
            Log("Mutex was not killed");
            //Log("----------------END-----------------");
            return false;
        }

        public static bool MutexExists(Process process, string mutexType, string mutexName, bool partial)
        {
            if (mutexName.ToLower().StartsWith("type:") && mutexName.Contains("|"))
            {
                mutexType = mutexName.Substring("type:".Length, mutexName.IndexOf('|') - "type:".Length);
                mutexName = mutexName.Substring(mutexName.IndexOf('|') + 1);
            }


            Log(string.Format("Checking if mutex '{0}' of type '{1}' exists in process '{2} (pid {3})'", mutexName, mutexType, process.ProcessName, process.Id));
            // 4 tries
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = Win32Processes.GetHandles(process, mutexType, "", mutexName);

                    if (partial)
                    {
                        bool foundHandle = false;

                        foreach (Win32API.SYSTEM_HANDLE_INFORMATION handle in handles)
                        {
                            string strObjectName = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));
                            if (!string.IsNullOrWhiteSpace(strObjectName) && strObjectName.Contains(mutexName))
                            {
                                Log(string.Format("Found mutex that contains search criteria. Located at {0}", strObjectName));
                                if (!foundHandle)
                                {
                                    foundHandle = true;
                                }
                            }
                        }

                        return foundHandle;
                    }
                    else
                    {
                        if (handles.Count > 0)
                        {
                            Log("Found the following mutexes:");
                            foreach (Win32API.SYSTEM_HANDLE_INFORMATION handle in handles)
                            {
                                string strObjectName = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));
                                Log(strObjectName);
                            }

                            return true;
                        }
                    }


                }
                catch (IndexOutOfRangeException)
                {
                    Log(string.Format("The process name '{0}' is not currently running", process.MainWindowTitle));
                }
                catch (ArgumentException)
                {
                    Log(string.Format("The mutex '{0}' was not found in the process '{1}'", mutexName, process.MainWindowTitle));
                }
            }
            Log(string.Format("The mutex '{0}' was not found in the process '{1}'", mutexName, process.MainWindowTitle));
            return false;
        }

        public static List<int> GetChildrenProcesses(Process process)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                        "SELECT * " +
                        "FROM Win32_Process " +
                        "WHERE ParentProcessId=" + process.Id);
            ManagementObjectCollection collection = searcher.Get();

            List<int> ids = new List<int>();
            foreach (ManagementBaseObject item in collection)
            {
                uint ui = (uint)item["ProcessId"];

                ids.Add((int)ui);
            }

            return ids;
        }

        public static bool IsRunning(Process process)
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    return true;
                }
            }
            //try { Process.GetProcessById(process.Id); }
            //catch (InvalidOperationException) { return false; }
            //catch (ArgumentException) { return false; }
            return false;
        }

        public static void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    SteamFunctions.readToEnd = true;
                    return;
                }

                if (e.Data.Contains("+connect_lobby") && string.IsNullOrEmpty(SteamFunctions.lobbyConnectArg))
                {
                    string toFind1 = "+connect_lobby ";
                    int start = e.Data.IndexOf(toFind1);
                    string string2 = e.Data.Substring(start);
                    SteamFunctions.lobbyConnectArg = string2;
                    SteamFunctions.readToEnd = true;
                    return;
                }

            }
            catch (Exception ex)
            {
                UserPrompts.Report(PromptIcon.Error, "Error", "Goldberg Lobby Connect output data error. " + ex.Message);
            }
        }

        public static void WriteToProcessMemory(GenericGameInfo gen, Process proc)
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

        public static void SetProcessorPriorityClass(GenericGameInfo gen, Process proc)
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

        public static void SetIdealProcessor(GenericGameInfo gen, Process proc)
        {
            Log(string.Format("Setting ideal processor to {0}", gen.IdealProcessor));
            ProcessThreadCollection threads = proc.Threads;
            for (int t = 0; t < threads.Count; t++)
            {
                if (threads[t].ThreadState == ThreadState.Running)
                {
                    threads[t].IdealProcessor = (gen.IdealProcessor + 1);
                }
            }
        }

        public static void SetProcessorProcessorAffinity(GenericGameInfo gen, Process proc)
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

        public static void SetProcessorAffinityPerInstance(GenericGameInfo gen, Process proc, int i)
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

        public static void KillRemainingProcess()
        {
            // search for game instances left behind
            try
            {
                var handlerInstance = GenericGameHandler.Instance;
                Process[] procs = Process.GetProcesses();

                List<string> addtlProcsToKill = new List<string>();
                if (handlerInstance.CurrentGameInfo.KillProcessesOnClose?.Length > 0)
                {
                    addtlProcsToKill = handlerInstance.CurrentGameInfo.KillProcessesOnClose.ToList();
                }

                foreach (Process proc in procs)
                {
                    try
                    {
                        if ((handlerInstance.CurrentGameInfo.LauncherExe != null && !handlerInstance.CurrentGameInfo.LauncherExe.Contains("NucleusDefined") && proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe.ToLower())) || addtlProcsToKill.Contains(proc.ProcessName, StringComparer.OrdinalIgnoreCase) || proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName.ToLower()) || (proc.Id != 0 && handlerInstance.attachedIds.Contains(proc.Id)) || (handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName != "" && proc.MainWindowTitle == handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName))
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

    }
}