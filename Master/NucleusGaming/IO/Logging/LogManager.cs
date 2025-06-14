using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class LogManager
    {
        public static readonly long MaxSize = 1024 * 1024 * 1024; // 1mb

        private static LogManager instance;
        public static LogManager Instance
        {
            get
            {
                if (instance == null)
                {
                    new LogManager();
                }
                return instance;
            }
        }

        private string logPath;
        private Stream logStream;
        private StreamWriter writer;
        private object locker;
        private List<ILogNode> logCallbacks;

        public LogManager()
        {
            locker = new object();

            instance = this;
            logPath = GetLogPath();

            logCallbacks = new List<ILogNode>();

            logStream = new FileStream(GetLogPath(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            logStream.Position = logStream.Length; // keep writing from where we left

            writer = new StreamWriter(logStream);
        }

        public static void RegisterForLogCallback(ILogNode node)
        {
            instance.logCallbacks.Add(node);
        }

        public static void UnregisterForLogCallback(ILogNode node)
        {
            instance.logCallbacks.Remove(node);
        }

        private static string GetAppDataPath()
        {
#if ALPHA
            return Path.Combine(Globals.NucleusInstallRoot, "content");
#else
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Nucleus Coop");
#endif
        }

        protected static string GetLogPath()
        {
            return Path.Combine(GetAppDataPath(), "app.log");
        }

        public void PLog(string str)
        {
            Console.WriteLine(str);
            ThreadPool.QueueUserWorkItem(doLog, str);
        }

        private void doLog(object s)
        {
            lock (locker)
            {
                string str = (string)s;
                writer.WriteLine(str);
                writer.Flush();

                if (logStream.Position > MaxSize)
                {
                    logStream.Position = 0;// write on top
                }
            }
        }

        public static void Log(string str)
        {
            Instance.PLog(str);
        }

        public void LogExceptionFile(Exception ex)
        {
            Log("ERROR - " + ex.Message + " | Stacktrace: " + ex.StackTrace);
            string version = "Nucleus v " + Globals.Version;
            string local = GetAppDataPath();
            DateTime now = DateTime.Now;
            string file = string.Format("{0}{1}{2}_{3}{4}{5}", now.Day.ToString("00"), now.Month.ToString("00"), now.Year.ToString("0000"), now.Hour.ToString("00"), now.Minute.ToString("00"), now.Second.ToString("00")) + ".log";
            string help = string.Empty;

            // if(Regex.IsMatch(ex.Message, ".*(.cfg|.ini|.xml|.conf|.scr|.json|.kgs).") && !Regex.IsMatch(ex.Message, ".*(XinputPlus.ini).") && ex.GetType().Name != "UnauthorizedAccessException")FileNotFoundExeption
            if (ex.GetType().Name == "FileNotFoundException")
            {
                help = "Nucleus Co-op can't find the game's configuration file, make you sure you ran your main game at least once without Nucleus and that you changed some graphic settings and applied them, that way you make sure the proper configuration files got generated.\n" +
                       "If you are still getting this error after doing that, select the game in the Nucleus Co-op user interface, click on Game Options and select Delete UserProfile Config Path for all players. You can also try deleting Nucleus Co-op content folder and adding the game again.";
            }
            else if (ex.GetType().Name == "UnauthorizedAccessException" || ex.GetType().Name == "IOException")
            {
                help = "Nucleus Co-op doesn't have the necessary permissions or is trying to access a file that is already in use,\n" +
                       "a file could be missing(For helpers: in case of \"IOException\" check if all required files from \"Game.Play = function()\" exists.)\n" +
                       "please make sure Nucleus Co-op is outside any game files or protected folders,\n" +
                       "placing Nucleus Co-op app folder in the root of your main drive is recommended to avoid permission issues: C:\\NucleusCo-op.\n" +
                       "Make sure other apps are not using any of the files Nucleus is trying to access. If all else fails run Nucleus Co-op with admin rights.";
            }
#if RELEASE

            string error = $"{version}\n\nNucleus failed setting up the handler. An attempt to clean up will be made.\n\n[Type]\n\n{ex.GetType().Name}\n\n[Message]\n\n{ex.Message}\n\n{help}";
#else

            string error = $"{version}\n\nNucleus failed setting up the handler. An attempt to clean up will be made.\n\n[Type]\n\n{ex.GetType().Name}\n\n[Message]\n\n{ex.Message}\n{help}\n\n[Stacktrace]\n\n{ex.StackTrace}";
#endif

            NucleusMessageBox.Show("Something went wrong :(", error, true);

            Log("Attempting clean-up procedures...");

            string[] regFiles = Directory.GetFiles(Path.Combine(Globals.NucleusInstallRoot, "utils\\backup"), "*.reg", SearchOption.AllDirectories);
            if (regFiles.Length > 0)
            {
                LogManager.Log("Restoring backed up registry files - method 2");
                foreach (string regFilePath in regFiles)
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();

                    try
                    {
                        proc.StartInfo.FileName = "reg.exe";
                        proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.UseShellExecute = false;

                        string command = "import \"" + regFilePath + "\"";
                        proc.StartInfo.Arguments = command;
                        proc.Start();

                        proc.WaitForExit();
                        LogManager.Log($"Imported {Path.GetFileName(regFilePath)}");
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

            GenericGameHandler.Instance.End(false);

            while (!GenericGameHandler.Instance.HasEnded)
            {
                Thread.Sleep(1000);
            }

            Thread.Sleep(1000);

            string path = Path.Combine(local, file);

            using (Stream stream = File.OpenWrite(path))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine("[Header]");
                    writer.WriteLine(now.ToLongDateString());
                    writer.WriteLine(now.ToLongTimeString());
                    writer.WriteLine("Nucleus Co-op v" + Globals.Version);
                    writer.WriteLine("[Message]");
                    writer.WriteLine(ex.Message);
                    writer.WriteLine("[Stacktrace]");
                    writer.WriteLine(ex.StackTrace);

                    for (int i = 0; i < logCallbacks.Count; i++)
                    {
                        ILogNode node = logCallbacks[i];
                        try
                        {
                            node.Log(writer);
                        }
                        catch
                        {
                            writer.WriteLine("LogNode failed to log: " + node.ToString());
                        }
                    }
                }
            }

            Log("High-level error log generated at content/" + file);

#if RELEASE
            bool EnableBtn = false;

            if (!App_Misc.DebugLog &&
                App_Misc.EnableLogAnswer == "")
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show($"Click \"Yes\" if you want to enable debug logging. Start the game once again to generate a debug log.\nClick \"No\" to disable this prompt.", "Enable debug log", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (dialogResult == DialogResult.Yes)
                {
                    App_Misc.DebugLog = true;
                    EnableBtn = true;
                }
                else
                {
                    App_Misc.EnableLogAnswer = "No";
                }
            }

            if (EnableBtn)
            {
                //Globals.Btn_debuglog.Parent.Invoke((MethodInvoker)delegate ()
                //{
                //    Globals.Btn_debuglog.Visible = true;
                //});
            }
#endif
        }

        public static void Log(string str, object par1)
        {
            Instance.PLog(string.Format(str, par1));
        }
        public static void Log(string str, object par1, object par2)
        {
            Instance.PLog(string.Format(str, par1, par2));
        }
        public static void Log(string str, object par1, object par2, object par3)
        {
            Instance.PLog(string.Format(str, par1, par2, par3));
        }
        public static void Log(string str, params object[] pars)
        {
            Instance.PLog(string.Format(str, pars));
        }
    }
}
