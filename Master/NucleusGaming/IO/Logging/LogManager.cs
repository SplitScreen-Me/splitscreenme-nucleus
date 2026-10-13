using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Localization;
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
        /// <summary>
        /// Past this size (10 MiB), app.log is written again from its start. It used to say "1mb" but was 1 GiB;
        /// 10 MiB leaves room for the host's per-call bridge lines in a long session.
        /// </summary>
        public static readonly long MaxSize = 10 * 1024 * 1024;

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

            // The content folder may not exist yet: the host and the services can log before GameManager creates it.
            Directory.CreateDirectory(GetAppDataPath());

            // FileMode.Create starts a new log at each launch.
            logStream = new FileStream(GetLogPath(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            logStream.Position = logStream.Length;

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
                try
                {
                    string str = (string)s;
                    writer.WriteLine(str);
                    writer.Flush();

                    if (logStream.Position > MaxSize)
                    {
                        logStream.Position = 0;// write on top
                    }
                }
                catch
                {
                    // A thread-pool callback must not throw: that would end the process.
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
           
            string local = GetAppDataPath();
            DateTime now = DateTime.Now;
            string file = string.Format("{0}{1}{2}_{3}{4}{5}", now.Day.ToString("00"), now.Month.ToString("00"), now.Year.ToString("0000"), now.Hour.ToString("00"), now.Minute.ToString("00"), now.Second.ToString("00")) + ".log";
            string help = string.Empty;

            // if(Regex.IsMatch(ex.Message, ".*(.cfg|.ini|.xml|.conf|.scr|.json|.kgs).") && !Regex.IsMatch(ex.Message, ".*(XinputPlus.ini).") && ex.GetType().Name != "UnauthorizedAccessException")FileNotFoundExeption
            if (ex.GetType().Name == "FileNotFoundException")
            {
                help = I18n.T("engine.crashReport.help.fileNotFound");
            }
            else if (ex.GetType().Name == "UnauthorizedAccessException" || ex.GetType().Name == "IOException")
            {
                help = I18n.T("engine.crashReport.help.access", new { suggestedFolder = @"C:\NucleusCo-op" });
            }
#if RELEASE

            string error = I18n.T("engine.crashReport.release", new
            {
                version = Globals.Version,
                installPath = Globals.NucleusInstallRoot,
                oneDrive = Globals.IsOneDriveEnabled,
                type = ex.GetType().Name,
                message = ex.Message,
                help,
            });
#else

            string error = I18n.T("engine.crashReport.debug", new
            {
                version = Globals.Version,
                type = ex.GetType().Name,
                message = ex.Message,
                help,
                stackTrace = ex.StackTrace,
            });
#endif

            //A host can show this report in its own UI once the session has ended.
            UserPrompts.Notice(I18n.T("engine.crashReport.title"), error, true, PromptOrigin.LaunchFailure, PromptIcon.Error);

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
                bool yes = UserPrompts.YesNo(I18n.T("engine.debugLog.enable.title"), I18n.T("engine.debugLog.enable.message"), PromptIcon.Question);
                
                if (yes)
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
