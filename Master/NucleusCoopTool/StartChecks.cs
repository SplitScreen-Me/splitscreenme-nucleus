using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
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

        public static bool IsAlredyRunning()
        {
            Thread.Sleep(1000);//Put this here for the theme switch option.
            if (Process.GetProcessesByName("NucleusCoop").Length > 1)
            {
                MessageBox.Show("Nucleus Co-op is already running, if you don't see the Nucleus Co-op window it might be running in the background, close the process using task manager.", "Nucleus Co-op is already running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isRunning = true;
            }

            return isRunning;
        }

        public static void CheckFilesIntegrity()
        {
            string[] ncFiles = { "DotNetZip.dll", "EasyHook.dll", "EasyHook32.dll", "EasyHook32Svc.exe", "EasyHook64.dll", "EasyHook64Svc.exe", "EasyHookSvc.exe", "Jint.dll", "NAudio.dll", "Newtonsoft.Json.dll", "Nucleus.Gaming.dll", "Nucleus.Hook32.dll", "Nucleus.Hook64.dll", "Nucleus.IJx64.exe", "Nucleus.IJx86.exe", "Nucleus.SHook32.dll", "Nucleus.SHook64.dll", "openxinput1_3.dll", "ProtoInputHooks32.dll", "ProtoInputHooks64.dll", "ProtoInputHooks64.dll", "ProtoInputHost.exe", "ProtoInputIJ32.exe", "ProtoInputIJ64.exe", "ProtoInputIJP32.dll", "ProtoInputIJP64.dll", "ProtoInputLoader32.dll", "ProtoInputLoader64.dll", "ProtoInputUtilDynamic32.dll", "ProtoInputUtilDynamic64.dll", "SharpDX.DirectInput.dll", "SharpDX.dll", "SharpDX.XInput.dll", "StartGame.exe", "WindowScrape.dll" };

            foreach (string file in ncFiles)
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, file)))
                {
                    if (MessageBox.Show(file + " is missing from your Nucleus Co-op installation folder. Check that your antivirus program is not deleting or blocking any Nucleus Co-op files. Add the Nucleus Co-op folder to your antivirus exceptions list and extract it again. Click \"OK\" for more information", "Missing file(s)", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        Process.Start("https://www.splitscreen.me/docs/faq/#7--nucleus-co-op-doesnt-launchcrashes-how-do-i-fix-it");
                        Process[] processes = Process.GetProcessesByName("NucleusCoop");

                        foreach (Process NucleusCoop in processes)
                        {
                            NucleusCoop.Kill();
                        }
                    }
                    else 
                    {
                        Process[] processes = Process.GetProcessesByName("NucleusCoop");

                        foreach (Process NucleusCoop in processes)
                        {
                            NucleusCoop.Kill();
                        }
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

            if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\descriptions")))
            {
                Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\descriptions")));
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
                        string[] environmentRegFileBackup = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(StartChecks)).Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);

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
                            Console.WriteLine("Registry has been restored");
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
                            Console.WriteLine("Registry backup is up-to-date");
                        }
                        else
                        {
                            TempEnvPathBackup.Dispose();
                            currentEnvPathBackup.Dispose();
                            File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                            File.Move(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"), Path.Combine(Application.StartupPath, @"utils\backup\User Shell Folders.reg"));
                            File.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp\User Shell Folders.reg"));
                            Directory.Delete(Path.Combine(Application.StartupPath, @"utils\backup\Temp"));
                            Console.WriteLine("Registry has been updated");
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

        public static void CheckForUpdate()
        {
            try
            {

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = Path.Combine(Application.StartupPath, @"utils\NC_Updater\NC_Updater.exe");
                    proc.StartInfo.UseShellExecute = false;
                    //proc.StartInfo.RedirectStandardOutput = true;
                   // proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.CreateNoWindow = false;
                    //proc.StartInfo.Arguments = "-quiet";
                    proc.Start();
                    Console.WriteLine(proc.ProcessName);
                    //string stdout = proc.StandardOutput.ReadToEnd();
                    //string stderr = proc.StandardError.ReadToEnd();
                    //proc.WaitForExit();
                }
                Console.WriteLine("Checking for update");

            }
            catch (Exception)
            { }

        }

        public static bool CheckNetCon()
        {
            bool connected;
            try
            {
                Ping myPing = new Ping();
                String host = "hub.splitscreen.me";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    connected = true;
                    myPing.Dispose();
                    return connected;
                }
                else
                {
                    connected = false;
                    myPing.Dispose();
                    return connected;
                }
            }
            catch (Exception)
            {
                connected = false;
                return connected;
            }
        }
    }
}
