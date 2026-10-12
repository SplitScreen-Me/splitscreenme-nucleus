using Nucleus.Gaming;
using System;
using System.IO;

namespace Nucleus.Coop.Host
{
    /// <summary>Writes host messages to Nucleus' app log (<c>content\app.log</c>). Never throws.</summary>
    internal static class HostLog
    {
        private static readonly object Gate = new object();
        private static bool ready;

        public static void Debug(string message) => Write("DEBUG", message);

        public static void Info(string message) => Write("INFO", message);

        public static void Warning(string message) => Write("WARN", message);

        public static void Error(string message) => Write("ERROR", message);

        public static void Error(string message, Exception ex) => Write("ERROR", message + " " + ex);

        public static void Write(string level, string message)
        {
            try
            {
                if (!EnsureLogFolder())
                {
                    return;
                }

                // The single-argument overload does not run string.Format, so braces in messages are safe.
                LogManager.Log($"[{DateTime.Now:HH:mm:ss.fff}] [WebUI] [{level}] {message}");
            }
            catch
            {
                // Logging must never break the UI.
            }
        }

        /// <summary>
        /// LogManager opens content\app.log in its constructor and is left half-built (and crashes a thread-pool thread
        /// on the next line) when the folder is missing. The legacy UI creates the folder through GameManager first;
        /// the web UI may log before any GameManager exists.
        /// </summary>
        private static bool EnsureLogFolder()
        {
            if (ready)
            {
                return true;
            }

            lock (Gate)
            {
                if (!ready)
                {
                    Directory.CreateDirectory(Path.Combine(Globals.NucleusInstallRoot, "content")); // GameManager.GetAppContentPath (ALPHA)
                    ready = true;
                }
            }

            return true;
        }
    }
}
