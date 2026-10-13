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
using static Nucleus.Gaming.Localization.I18n;

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
            Thread.Sleep(1000);// Lets a restarting instance (restart as administrator) exit first.
            if (Process.GetProcessesByName("NucleusCoop").Length > 1)
            {
                MessageBox.Show(TPlain("engine.startChecks.alreadyRunning.message"), TPlain("engine.startChecks.alreadyRunning.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                isRunning = true;
            }

            return isRunning;
        }

        public static bool IsInvalidDriveFormat()
        {
            return UserDriveInfo.IsExFat(Globals.NucleusInstallRoot);
        }       
                   
        public static void CheckFilesIntegrity()
        {
            // Relative to the install root. Managed assemblies are in bin\ (probing privatePath of the exe configs);
            // natives and helper exes stay at the root. tools\build-native.ps1 parses this list.
            string[] ncFiles = {
                @"bin\EasyHook.dll",
                @"bin\Ionic.Zip.Reduced.dll",
                @"bin\Jint.dll",
                @"bin\NAudio.dll",
                @"bin\Newtonsoft.Json.dll",
                @"bin\Nucleus.Bridge.dll",
                @"bin\Nucleus.Coop.Services.dll",
                @"bin\Nucleus.Gaming.dll",
                @"bin\SharpDX.DirectInput.dll",
                @"bin\SharpDX.dll",
                @"bin\SharpDX.XInput.dll",
                @"bin\WindowScrape.dll",
                "EasyHook32.dll",
                "EasyHook64.dll",
                "Nucleus.Hook32.dll",
                "Nucleus.Hook64.dll",
                "Nucleus.IJx64.exe",
                "Nucleus.IJx64.exe.config",
                "Nucleus.IJx86.exe",
                "Nucleus.IJx86.exe.config",
                "Nucleus.SHook32.dll",
                "Nucleus.SHook64.dll",
                "openxinput1_3.dll",
                "ProtoInputHooks32.dll",
                "ProtoInputHooks64.dll",
                "ProtoInputIJ32.exe",
                "ProtoInputIJ64.exe",
                "ProtoInputIJP32.dll",
                "ProtoInputIJP64.dll",
                "ProtoInputLoader32.dll",
                "ProtoInputLoader64.dll",
                "ProtoInputUtilDynamic32.dll",
                "ProtoInputUtilDynamic64.dll",
                "StartGame.exe",
                "StartGame.exe.config"
            };

            try
            {
                foreach (string file in ncFiles)
                {
                    if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, file)))
                    {
                        if (MessageBox.Show(TPlain("engine.startChecks.missingFile.message", new { file }), TPlain("engine.startChecks.missingFile.title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
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

            if (!Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, @"gui\covers")))
            {
                Directory.CreateDirectory((Path.Combine(Globals.NucleusInstallRoot, @"gui\covers")));
            }

            if (!Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, @"gui\screenshots")))
            {
                Directory.CreateDirectory((Path.Combine(Globals.NucleusInstallRoot, @"gui\screenshots")));
            }
         
                if (Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, $"gui\\descriptions")))//Not used anymore
                {
                    Directory.Delete(Path.Combine(Globals.NucleusInstallRoot, $"gui\\descriptions"), true);
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
                    else if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg")))
                    {
                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                    }
                    else
                    {
                        if (!Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp")))
                        {
                            Directory.CreateDirectory((Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp")));
                        }

                        ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp\User Shell Folders.reg"));

                        FileStream currentEnvPathBackup = new FileStream(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"), FileMode.Open);
                        FileStream TempEnvPathBackup = new FileStream(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp\User Shell Folders.reg"), FileMode.Open);

                        if (currentEnvPathBackup.Length == TempEnvPathBackup.Length)
                        {
                            TempEnvPathBackup.Dispose();
                            currentEnvPathBackup.Dispose();

                            File.Delete(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp\User Shell Folders.reg"));
                            Directory.Delete(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp"));
                            //Console.WriteLine("Registry backup is up-to-date");
                        }
                        else
                        {
                            TempEnvPathBackup.Dispose();
                            currentEnvPathBackup.Dispose();
                            File.Delete(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                            File.Move(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp\User Shell Folders.reg"), Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                            File.Delete(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp\User Shell Folders.reg"));
                            Directory.Delete(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\Temp"));
                            //Console.WriteLine("Registry has been updated");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, TPlain("engine.startChecks.environmentError.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        public static bool StartCheck(bool warningMessage)
        {
            return CheckInstallFolder(warningMessage);
        }

        private static bool CheckInstallFolder(bool warningMessage)
        {
            // Install root with a trailing separator (was the exe path), compared with the folder prefixes below.
            string installPath = InstallPaths.Root.TrimEnd('\\').ToLower() + "\\";

            if(installPath == null)
            {
                MessageBox.Show(TPlain("engine.startChecks.installPathUnknown.message"), TPlain("engine.titles.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool problematic = installPath.StartsWith(@"C:\Program Files\".ToLower()) ||
                               installPath.StartsWith(@"C:\Program Files (x86)\".ToLower()) ||
                               installPath.StartsWith(@"C:\Users\".ToLower()) ||
                               installPath.StartsWith(@"C:\Windows\".ToLower());



            if (Globals.IsOneDriveEnabled)
            {
                MessageBox.Show(TPlain("engine.startChecks.oneDrive.message"), TPlain("engine.startChecks.oneDrive.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (problematic)
            {
                if (warningMessage)
                {
                    string message = TPlain("engine.startChecks.installFolder.message", new
                    {
                        programFiles = @"C:\Program Files",
                        programFilesX86 = @"C:\Program Files (x86)",
                        users = @"C:\Users",
                        windows = @"C:\Windows",
                        example = @"C:\Nucleus\NucleusCoop.exe",
                    });

                    MessageBox.Show(message, TPlain("engine.titles.warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    DialogResult dialogResultX86 = MessageBox.Show(TPlain("engine.startChecks.vcRedist.message", new { architecture = "x86" }), TPlain("engine.titles.warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX86 == DialogResult.Yes)
                    {
                        DependenciesDownloader dldp = new DependenciesDownloader("https://aka.ms/vs/17/release/vc_redist.x86.exe", TPlain("engine.startChecks.vcRedist.downloading", new { architecture = "x86" }), "vc_redist.x86.exe", "vc_redist.x86");
                        dldp.ShowDialog();//to keep it synchronous                       
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }

                if (!validVCRx64)//!validVCRx64
                {
                    DialogResult dialogResultX64 = MessageBox.Show(TPlain("engine.startChecks.vcRedist.message", new { architecture = "x64" }), TPlain("engine.titles.warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResultX64 == DialogResult.Yes)
                    {
                        DependenciesDownloader dldp = new DependenciesDownloader("https://aka.ms/vs/17/release/vc_redist.x64.exe", TPlain("engine.startChecks.vcRedist.downloading", new { architecture = "x64" }), "vc_redist.x64.exe", "vc_redist.x64");
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

            string webViewVer = null;

            try
            {
                webViewVer = CoreWebView2Environment.GetAvailableBrowserVersionString(null);
            }
            catch(WebView2RuntimeNotFoundException)
            {         
            }

            if (webViewVer == null)
            {
                DialogResult dialogResultX64 = MessageBox.Show(TPlain("engine.startChecks.webView2.message"), TPlain("engine.startChecks.webView2.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResultX64 == DialogResult.Yes)
                {
                    DependenciesDownloader dldp = new DependenciesDownloader(directDownloadLinkX64, TPlain("engine.startChecks.webView2.downloading"), x64FileName, "MicrosoftEdgeWebView2RuntimeInstallerX64");
                    dldp.ShowDialog();           
                }
            }
        }
        
        public static void CheckAppUpdate()
        {
            try
            {
                if (File.Exists(Path.Combine(Globals.NucleusInstallRoot, "Updater.exe")))
                    Process.Start(Path.Combine(Globals.NucleusInstallRoot, "Updater.exe"));
            }
            catch 
            {
            }
        }

        public static void CheckDebugLogSize()
        {
           string logPath = Path.Combine(Globals.NucleusInstallRoot, DebugLogFile.FileName);

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
