using Microsoft.Win32;
using Nucleus.Gaming;
using Nucleus.Gaming.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Nucleus
{
    public static class ProcessUtil
    {
        private static readonly IniFile ini = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        private static string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string DocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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

                cmd.StandardInput.WriteLine($@"set APPDATA={NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Roaming");
                cmd.StandardInput.WriteLine($@"set LOCALAPPDATA={NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Local");
                cmd.StandardInput.WriteLine($@"set USERPROFILE={NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}");
                cmd.StandardInput.WriteLine($@"set HOMEPATH=\Users\{username}\NucleusCoop\{playerNickname}");

                //Some games will crash if the directories don't exist
                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop");
                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}");
                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Roaming");
                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}\AppData\Local");

                //Directory.CreateDirectory($@"\Users\{username}\NucleusCoop\{playerNickname}");
                Directory.CreateDirectory($@"{NucleusEnvironmentRoot}\NucleusCoop\{playerNickname}\Documents");

                Directory.CreateDirectory(Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{playerNickname}\Documents");

                if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg")))
                {
                    //string mydocPath = key.GetValue("Personal").ToString();
                    ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), @"utils\backup\User Shell Folders.reg"));
                }

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                key.SetValue("Personal", Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{playerNickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
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

        private static void ExportRegistry(string strKey, string filepath)
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
                // handle exception
            }
        }

        private static void KillParent(object state)
        {
            Process p = (Process)state;

        }
        private static void Log(string logMessage)
        {
            if (ini.IniReadValue("Misc", "DebugLog") == "True")
            {
                //StreamWriter w = new StreamWriter("debug-log.txt", true);
                //w.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]PROCESSUTIL: {logMessage}");
                //w.Flush();
                //w.Close();
                //w.Dispose();

                using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]PROCESSUTIL: {logMessage}");
                    writer.Close();
                }
            }
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


    }
}
//        [DllImport("ntdll.dll")]
//        public static extern uint NtQuerySystemInformation(int
//            SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength,
//            ref int returnLength);

//        [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory")]
//        static extern void CopyMemory(byte[] Destination, IntPtr Source, uint Length);

//        [StructLayout(LayoutKind.Sequential, Pack = 1)]
//        public struct SYSTEM_HANDLE_INFORMATION
//        { // Information Class 16
//            public int ProcessID;
//            public byte ObjectTypeNumber;
//            public byte Flags; // 0x01 = PROTECT_FROM_CLOSE, 0x02 = INHERIT
//            public ushort Handle;
//            public int Object_Pointer;
//            public UInt32 GrantedAccess;
//        }

//        const int CNST_SYSTEM_HANDLE_INFORMATION = 16;
//        const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;

//        public static List<SYSTEM_HANDLE_INFORMATION> GetHandles(Process process)
//        {
//            uint nStatus;
//            int nHandleInfoSize = 0x10000;
//            IntPtr ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
//            int nLength = 0;
//            IntPtr ipHandle = IntPtr.Zero;

//            while ((nStatus = NtQuerySystemInformation(CNST_SYSTEM_HANDLE_INFORMATION, ipHandlePointer, nHandleInfoSize, ref nLength)) == STATUS_INFO_LENGTH_MISMATCH)
//            {
//                nHandleInfoSize = nLength;
//                Marshal.FreeHGlobal(ipHandlePointer);
//                ipHandlePointer = Marshal.AllocHGlobal(nLength);
//            }

//            byte[] baTemp = new byte[nLength];
//            CopyMemory(baTemp, ipHandlePointer, (uint)nLength);

//            long lHandleCount = 0;
//            if (Is64Bits())
//            {
//                lHandleCount = Marshal.ReadInt64(ipHandlePointer);
//                ipHandle = new IntPtr(ipHandlePointer.ToInt64() + 8);
//            }
//            else
//            {
//                lHandleCount = Marshal.ReadInt32(ipHandlePointer);
//                ipHandle = new IntPtr(ipHandlePointer.ToInt32() + 4);
//            }

//            SYSTEM_HANDLE_INFORMATION shHandle;
//            List<SYSTEM_HANDLE_INFORMATION> lstHandles = new List<SYSTEM_HANDLE_INFORMATION>();

//            for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
//            {
//                shHandle = new SYSTEM_HANDLE_INFORMATION();
//                if (Is64Bits())
//                {
//                    shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
//                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle) + 8);
//                }
//                else
//                {
//                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle));
//                    shHandle = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
//                }
//                if (shHandle.ProcessID != process.Id) continue;
//                lstHandles.Add(shHandle);
//            }
//            return lstHandles;

//        }

//        static bool Is64Bits()
//        {
//            return Marshal.SizeOf(typeof(IntPtr)) == 8 ? true : false;
//        }
//    }
//}
