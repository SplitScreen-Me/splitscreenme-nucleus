using Microsoft.Win32;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Tools.UserDriveInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    internal static class StartChecks
    {
        static bool isRunning = false;

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
            }
        }

        public static bool IsAlreadyRunning()
        {
            Thread.Sleep(1000);//Put this here for the theme switch option.
            if (Process.GetProcessesByName("NucleusCoop").Length > 1)
            {
                MessageBox.Show("Nucleus Co-op is already running, if you don't see the Nucleus Co-op window it might be running in the background, close the process using task manager.", "Nucleus Co-op is already running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isRunning = true;
            }

            return isRunning;
        }

        public static bool IsInvalidDriveFormat()
        {
            return UserDriveInfo.IsExFat(Globals.NucleusInstallRoot, true);       
        }       
                   
        public static void CheckFilesIntegrity()
        {
            string[] ncFiles = {
                "Ionic.Zip.Reduced.dll",
                "EasyHook.dll",
                "EasyHook32.dll",
                "EasyHook32Svc.exe",
                "EasyHook64.dll",
                "EasyHook64Svc.exe",
                "EasyHookSvc.exe",
                "Jint.dll",
                "NAudio.dll",
                "Newtonsoft.Json.dll",
                "Nucleus.Gaming.dll",
                "Nucleus.Hook32.dll",
                "Nucleus.Hook64.dll",
                "Nucleus.IJx64.exe",
                "Nucleus.IJx86.exe",
                "Nucleus.SHook32.dll",
                "Nucleus.SHook64.dll",
                "openxinput1_3.dll",
                "ProtoInputHooks32.dll",
                "ProtoInputHooks64.dll",
                "ProtoInputHost.exe",
                "ProtoInputIJ32.exe",
                "ProtoInputIJ64.exe",
                "ProtoInputIJP32.dll",
                "ProtoInputIJP64.dll",
                "ProtoInputLoader32.dll",
                "ProtoInputLoader64.dll",
                "ProtoInputUtilDynamic32.dll",
                "ProtoInputUtilDynamic64.dll",
                "SharpDX.DirectInput.dll",
                "SharpDX.dll",
                "SharpDX.XInput.dll",
                "StartGame.exe",
                "WindowScrape.dll"
            };

            foreach (string file in ncFiles)
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, file)))
                {
                    if (MessageBox.Show(file + " is missing from your Nucleus Co-op installation folder. Check that your antivirus program is not deleting or blocking any Nucleus Co-op files. Add the Nucleus Co-op folder to your antivirus exceptions list and extract it again. Click \"OK\" for more information", "Missing file(s)", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Process.Start("https://www.splitscreen.me/docs/faq/#7--nucleus-co-op-doesnt-launchcrashes-how-do-i-fix-it");
                        Process nc = Process.GetCurrentProcess();
                        nc.Kill();
                    }
                    else
                    {
                        Process nc = Process.GetCurrentProcess();
                        nc.Kill();
                    }
                }
            }

            if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\covers")))
            {
                Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\covers")));
            }

            if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots")))
            {
                Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\screenshots")));
            }

            try
            {
                if (Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\descriptions")))//Not used anymore
                {
                    Directory.Delete(Path.Combine(Application.StartupPath, $"gui\\descriptions"), true);
                }
            }
            catch { }
        }
        
        public static void CheckUserEnvironment()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                    string mydocPath = dkey.GetValue("Personal").ToString();

                    if (mydocPath.Contains("NucleusCoop"))
                    {
                        string[] environmentRegFileBackup = Directory.GetFiles(Path.Combine(Globals.NucleusInstallRoot, "utils\\backup"), "*.reg", SearchOption.AllDirectories);

                        if (environmentRegFileBackup.Length > 0)
                        {
                            foreach (string environmentRegFilePathBackup in environmentRegFileBackup)
                            {
                                if (environmentRegFilePathBackup.Contains("User Shell Folders"))
                                {
                                    Process regproc = new Process();

                                    try
                                    {
                                        regproc.StartInfo.FileName = "reg.exe";
                                        regproc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                        regproc.StartInfo.CreateNoWindow = true;
                                        regproc.StartInfo.UseShellExecute = false;

                                        string command = "import \"" + environmentRegFilePathBackup + "\"";
                                        regproc.StartInfo.Arguments = command;
                                        regproc.Start();

                                        regproc.WaitForExit();

                                    }
                                    catch (Exception)
                                    {
                                        regproc.Dispose();
                                    }
                                }
                            }
                            //Console.WriteLine("Registry has been restored");
                        }
                    }
                    else if (!File.Exists(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg")))
                    {
                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                    }
                    else
                    {
                        if (!Directory.Exists(Path.Combine(Application.StartupPath, @"utils\backup\Temp")))
                        {
                            Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"utils\backup\Temp")));
                        }

                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"));

                        FileStream currentEnvPathBackup = new FileStream(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"), FileMode.Open);
                        FileStream TempEnvPathBackup = new FileStream(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"), FileMode.Open);

                        if (currentEnvPathBackup.Length == TempEnvPathBackup.Length)
                        {
                            TempEnvPathBackup.Dispose();
                            currentEnvPathBackup.Dispose();

                            File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"));
                            Directory.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp"));
                            //Console.WriteLine("Registry backup is up-to-date");
                        }
                        else
                        {
                            TempEnvPathBackup.Dispose();
                            currentEnvPathBackup.Dispose();
                            File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                            File.Move(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"), Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                            File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"));
                            Directory.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp"));
                            //Console.WriteLine("Registry has been updated");
                        }
                    }
                }
                catch (Exception)
                {

                }
            });
        }

        public static bool StartCheck(bool warningMessage)
        {
            return CheckInstallFolder(warningMessage);
        }

        private static bool CheckInstallFolder(bool warningMessage)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower();

            bool problematic = exePath.StartsWith(@"C:\Program Files\".ToLower()) ||
                               exePath.StartsWith(@"C:\Program Files (x86)\".ToLower()) ||
                               exePath.StartsWith(@"C:\Users\".ToLower()) ||
                               exePath.StartsWith(@"C:\Windows\".ToLower());


            bool OnDriveEnabled = Globals.UserDocumentsRoot.Contains("OneDrive");

            if (OnDriveEnabled)
            {
                string message = "Using OneDrive as default documents path will break most handlers because Nucleus can't access its storage." +
                                 "To change your documents path log out from the OneDrive app and right click your Documents folder in file explorer, go to properties, " +
                                 "select path or location and set it to the default Windows."; /*one when done delete \"User Shell Folders.reg\" from \"utils\\backup\" if the file exists.";*/

                MessageBox.Show(message, "OneDrive must be disabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if(File.Exists(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg")))
                {
                    File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                }

            }

            if (problematic)
            {
                string message = "Nucleus Co-Op should not be installed here.\n\n" +
                                "Do NOT install in any of these folders:\n" +
                                "- A folder containing any game files\n" +
                                "- C:\\Program Files or C:\\Program Files (x86)\n" +
                                "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                                "- Any folder with security settings like C:\\Windows\n" +
                                "\n" +
                                "A good place is C:\\Nucleus\\NucleusCoop.exe";

                if (warningMessage)
                {
                    MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static void Check_VCRVersion()
        {
            string downloadPath = Application.StartupPath + @"\Temp";
            string x86FileName = "vc_redist.x86.exe";
            string x64FileName = "vc_redist.x64.exe";

            try
            {
                const string subkeyX86 = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86";
                const string subkeyX64 = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";

                bool validVCRx86;
                bool validVCRx64;

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkeyX86))
                {
                    validVCRx86 = (int)ndpKey.GetValue("Bld") >= 31103;
                }

                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkeyX64))
                {
                    validVCRx64 = (int)ndpKey.GetValue("Bld") >= 31103;
                }

                DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                DeleteMVCDownloadFolder(downloadPath);

                if (!validVCRx86)//!validVCRx86
                {
                    DialogResult dialogResultX86 = MessageBox.Show("Please install Microsoft Visual C++ 2015 - 2022 Redistributable (both x86 and x64)\n" +
                                   "Do you want to download and install Microsoft Visual C++ 2015 - 2022 Redistributable x86 version now.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX86 == DialogResult.Yes)
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        ServicePointManager.DefaultConnectionLimit = 9999;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://aka.ms/vs/17/release/vc_redist.x86.exe");
                        request.Timeout = 4000;
                        request.UserAgent = "request";

                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (var fileStream = new FileStream(Path.Combine(downloadPath, x86FileName), FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }

                        while (!File.Exists(Path.Combine(downloadPath, x86FileName)))
                        {
                            //Console.WriteLine("File not created yet");
                        }

                        ProcessStartInfo vcX86Installer = new ProcessStartInfo(Path.Combine(downloadPath, x86FileName));
                        vcX86Installer.UseShellExecute = true;
                        Process.Start(vcX86Installer);

                        while (Process.GetProcessesByName("vc_redist.x86").Length > 0)
                        {
                            //Console.WriteLine("Installer is running");
                        }

                        DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                        DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                        DeleteMVCDownloadFolder(downloadPath);
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }

                //check if file exist here in case installation process is aborted by user 
                DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                DeleteMVCDownloadFolder(downloadPath);

                if (!validVCRx64)//!validVCRx64
                {
                    DialogResult dialogResultX64 = MessageBox.Show("Do you want to download and install Microsoft Visual C++ 2015 - 2022 Redistributable x64 version now.", "Microsoft Visual C++ 2015 - 2022 Redistributable x64", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX64 == DialogResult.Yes)
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        ServicePointManager.DefaultConnectionLimit = 9999;

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://aka.ms/vs/17/release/vc_redist.x64.exe");
                        request.Timeout = 4000;
                        request.UserAgent = "request";

                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (var fileStream = new FileStream(Path.Combine(downloadPath, x64FileName), FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }

                        while (!File.Exists(Path.Combine(downloadPath, x64FileName)))
                        {
                            //Console.WriteLine("File not created yet");
                        }

                        ProcessStartInfo vcX64Installer = new ProcessStartInfo(Path.Combine(downloadPath, x64FileName));
                        vcX64Installer.UseShellExecute = true;
                        Process.Start(vcX64Installer);

                        while (Process.GetProcessesByName("vc_redist.x64").Length > 0)
                        {
                            //Console.WriteLine("Installer is running");
                        }

                        DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                        DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                        DeleteMVCDownloadFolder(downloadPath);
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }

                    //check if file exist here in case installation process is aborted by user 
                    DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                    DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                    DeleteMVCDownloadFolder(downloadPath);
                }
            }
            catch (Exception ex)
            {
                string mvcInstallerWarning = string.Empty;
                if (Process.GetProcessesByName("vc_redist.x64").Length > 0 || Process.GetProcessesByName("vc_redist.x86").Length > 0)
                {
                    mvcInstallerWarning = "Close Microsoft Visual C++ 2015 - 2022 Redistributable installer and try again.\n\n";
                }

                DeleteMVCFile(Path.Combine(downloadPath, x86FileName));
                DeleteMVCFile(Path.Combine(downloadPath, x64FileName));
                DeleteMVCDownloadFolder(downloadPath);

                MessageBox.Show("Something went wrong during Microsoft Visual C++ 2015 - 2022 Redistributable sanity checks.\n\n"
                    + mvcInstallerWarning
                    + ex.Message, "Microsoft Visual C++ sanity checks failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DeleteMVCFile(string file)
        {
            string processName = file.Split('\\').Last().Replace(".exe", "");

            if (File.Exists(file) && Process.GetProcessesByName(processName).Length == 0)
            {
                File.Delete(file);
            }
        }

        private static void DeleteMVCDownloadFolder(string folder)
        {
            if (Directory.Exists(folder) &&
                !File.Exists(Path.Combine(folder, "vc_redist.x86.exe")) &&
                !File.Exists(Path.Combine(folder, "vc_redist.x64.exe")))
            {
                Directory.Delete(folder);
            }
        }

        public static bool CheckHubResponse()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://hub.splitscreen.me/");
            request.Timeout = 4000;
            request.Method = "HEAD";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void CheckAppUpdate()
        {
            if (File.Exists(Path.Combine(Application.StartupPath, "Updater.exe")))
                Process.Start(Path.Combine(Application.StartupPath, "Updater.exe"));
        }

        public static void CheckDebugLogSize()
        {
           string logPath = Path.Combine(Application.StartupPath, "debug-log.txt");

            if (File.Exists(logPath))
            {
                FileStream debugLog = File.OpenRead(logPath);

                long LogSize = debugLog.Length;

                debugLog.Close();

                if (LogSize >= 150000)//150ko
                {
                    File.Delete(logPath);
                    App_Misc.DebugLog = false;
                }
            }
        }

        public static void CleanLogs()
        {
            string logsDirectory = Path.Combine(Globals.NucleusInstallRoot, "content");

            if(!Directory.Exists(logsDirectory))
            {
                return;
            }

            IEnumerable<string> logs = Directory.EnumerateFiles(logsDirectory).Where(l => Path.GetExtension(l) == ".log");

            foreach (string log in logs)
            {
                if (File.Exists(log))
                {
                    DateTime now = DateTime.Now;
                    DateTime creation = File.GetLastWriteTime(log);
                   
                    if (now.Month == creation.Month)
                    {
                        if (now.Day >= creation.Day + 7)
                        {
                            File.Delete(log);
                            continue;
                        }
                    }

                    if (now.Month > creation.Month)
                    {
                        File.Delete(log);
                        continue;
                    }

                    if (now.Month < creation.Month)//new year since last update
                    {
                        if ((now.Day >= 7 && now.Month == 1) || now.Month > 1)
                        {
                            File.Delete(log);
                            continue;
                        }
                    }
                }
            }
        }
    }
}
