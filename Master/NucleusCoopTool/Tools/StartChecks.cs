using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Nucleus.Coop.Forms;
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

            try
            {
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
         
                if (Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\descriptions")))//Not used anymore
                {
                    Directory.Delete(Path.Combine(Application.StartupPath, $"gui\\descriptions"), true);
                }
            }
            catch 
            { 
            
            
            }
        }
        
        public static void CheckUserEnvironment()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (!Globals.IsOneDriveEnabled)
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error while checking the user environment.", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if(exePath == null)
            {
                MessageBox.Show($"Unable to check Nucleus installation path." ,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool problematic = exePath.StartsWith(@"C:\Program Files\".ToLower()) ||
                               exePath.StartsWith(@"C:\Program Files (x86)\".ToLower()) ||
                               exePath.StartsWith(@"C:\Users\".ToLower()) ||
                               exePath.StartsWith(@"C:\Windows\".ToLower());



            if (Globals.IsOneDriveEnabled)
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

                if (!validVCRx86)//!validVCRx86
                {
                    DialogResult dialogResultX86 = MessageBox.Show("Please install Microsoft Visual C++ 2015 - 2022 Redistributable X86\n\n" +
                                   "Do you want to download and install Microsoft Visual C++ 2015 - 2022 Redistributable x86 version now?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX86 == DialogResult.Yes)
                    {
                        DependenciesDownloader dldp = new DependenciesDownloader("https://aka.ms/vs/17/release/vc_redist.x86.exe", "Downloading Microsoft Visual C++ 2015 - 2022 Redistributable x86 version", "vc_redist.x86.exe", "vc_redist.x86");
                        dldp.ShowDialog();//to keep it synchronous                       
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }

                if (!validVCRx64)//!validVCRx64
                {
                    DialogResult dialogResultX64 = MessageBox.Show("Please install Microsoft Visual C++ 2015 - 2022 Redistributable x64\n\n" +
                                    "Do you want to download and install Microsoft Visual C++ 2015 - 2022 Redistributable x64 version now?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX64 == DialogResult.Yes)
                    {
                        DependenciesDownloader dldp = new DependenciesDownloader("https://aka.ms/vs/17/release/vc_redist.x64.exe", "Downloading Microsoft Visual C++ 2015 - 2022 Redistributable x64 version", "vc_redist.x64.exe", "vc_redist.x64");
                        dldp.ShowDialog();//to keep it synchronous
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
            catch
            {
            }
        }
        
        public static void CheckWebView2Runtime()
        {
            //https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution?tabs=dotnetcsharp#detect-if-a-webview2-runtime-is-already-installed
            //https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/win32/webview2-idl?view=webview2-1.0.3240.44#getavailablecorewebview2browserversionstring
            string directDownloadLinkX64 = "https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/6e1e9052-dcb3-41e3-9aec-a7880afb75b2/MicrosoftEdgeWebView2RuntimeInstallerX64.exe";
            string x64FileName = "MicrosoftEdgeWebView2RuntimeInstallerX64.exe";

            string webViewVer = CoreWebView2Environment.GetAvailableBrowserVersionString(null);

            if (webViewVer == null)
            {
                DialogResult dialogResultX64 = MessageBox.Show("Do you want to download and install Microsoft WebView2 Runtime now?\n\nWithout it you will be unable to download handlers from the application.", "Microsoft WebView2 Runtime is not installed.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResultX64 == DialogResult.Yes)
                {
                    DependenciesDownloader dldp = new DependenciesDownloader(directDownloadLinkX64, "Downloading Microsoft WebView2 Runtime", x64FileName, "MicrosoftEdgeWebView2RuntimeInstallerX64");
                    dldp.ShowDialog();           
                }
            }
        }
        
        public static bool CheckHubResponse()
        {
            try
            {
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.DefaultConnectionLimit = 9999;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://hub.splitscreen.me/");
                request.Timeout = 6000;
                request.Method = "HEAD";

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
            try
            {
                if (File.Exists(Path.Combine(Application.StartupPath, "Updater.exe")))
                    Process.Start(Path.Combine(Application.StartupPath, "Updater.exe"));
            }
            catch 
            {           
            }
        }

        public static void CheckDebugLogSize()
        {
           string logPath = Path.Combine(Application.StartupPath, "debug-log.txt");

            try
            {
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
            catch 
            {
            }
        }

        public static void CleanLogs()
        {
            string logsDirectory = Path.Combine(Globals.NucleusInstallRoot, "content");

            try
            {
                if (!Directory.Exists(logsDirectory))
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
            catch
            { 
            }
        }
    }
}
