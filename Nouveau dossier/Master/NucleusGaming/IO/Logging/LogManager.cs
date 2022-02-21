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

        private static readonly IniFile ini = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

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

            logStream = new FileStream(GetLogPath(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
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
            string local = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return Path.Combine(local, "content");
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

            if (ini.IniReadValue("Misc", "DebugLog") == "True")
            {
                if (str.StartsWith("Found game info"))
                {
                    return;
                }
                using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]LOGMANAGER: {str}");
                    writer.Close();
                }
            }
        }

        public void LogExceptionFile(Exception ex)
        {
            Log("ERROR - " + ex.Message + " | Stacktrace: " + ex.StackTrace);

            string local = GetAppDataPath();
            DateTime now = DateTime.Now;
            string file = string.Format("{0}{1}{2}_{3}{4}{5}", now.Day.ToString("00"), now.Month.ToString("00"), now.Year.ToString("0000"), now.Hour.ToString("00"), now.Minute.ToString("00"), now.Second.ToString("00")) + ".log";
            MessageBox.Show($"Nucleus has crashed unexpectedly. An attempt to clean up will be made.\n\n[Type]\n{ex.GetType().Name}\n\n[Message]\n{ex.Message}\n\n[Stacktrace]\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Log("Attempting shut-down procedures in order to clean-up");

            string[] regFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);
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

                    File.Delete(regFilePath);
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
                    writer.WriteLine("Nucleus Coop Alpha v" + Globals.Version);
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

            Windows.User32Util.ShowTaskBar();

            Log("High-level error log generated at content/" + file);

            Application.Exit();
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
