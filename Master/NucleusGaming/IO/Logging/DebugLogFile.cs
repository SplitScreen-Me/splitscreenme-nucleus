using Nucleus.Gaming.App.Settings;
using System;
using System.IO;
using System.Threading;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Writes <c>debug-log.txt</c>, the user-facing debug log enabled by <c>[Misc] DebugLog</c>.
    /// Line format: <c>[yyyy-MM-dd HH:mm:ss]PREFIX: message</c>.
    /// The path is relative to the current directory on purpose: Nucleus, StartGame and Nucleus.Inject each write
    /// it in their own working directory. Thread-safe within a process; never throws.
    /// </summary>
    public static class DebugLogFile
    {
        public const string FileName = "debug-log.txt";

        private const int Attempts = 3;
        private static readonly object WriteLock = new object();

        /// <summary>Appends a line when <see cref="App_Misc.DebugLog"/> is on.</summary>
        public static void Write(string prefix, string message)
        {
            bool enabled;
            try
            {
                enabled = App_Misc.DebugLog;
            }
            catch
            {
                return;
            }

            if (enabled)
            {
                Append(prefix, message);
            }
        }

        /// <summary>Appends a line without checking <see cref="App_Misc.DebugLog"/>, for a process that reads the setting itself.</summary>
        public static void Append(string prefix, string message)
        {
            try
            {
                string line = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]{prefix}: {message}";

                lock (WriteLock)
                {
                    for (int attempt = 1; ; attempt++)
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(FileName, true))
                            {
                                writer.WriteLine(line);
                            }

                            return;
                        }
                        catch (IOException) when (attempt < Attempts)
                        {
                            // Another process (Nucleus, StartGame, Nucleus.Inject) is appending to the same file.
                            Thread.Sleep(10);
                        }
                    }
                }
            }
            catch
            {
                // The debug log must never break the caller.
            }
        }
    }
}
