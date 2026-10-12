using Nucleus.Gaming;
using System;

namespace Nucleus.Coop.Host
{
    /// <summary>Writes host messages to Nucleus' app log (<c>content\app.log</c>). Never throws.</summary>
    internal static class HostLog
    {
        public static void Debug(string message) => Write("DEBUG", message);

        public static void Info(string message) => Write("INFO", message);

        public static void Warning(string message) => Write("WARN", message);

        public static void Error(string message) => Write("ERROR", message);

        public static void Error(string message, Exception ex) => Write("ERROR", message + " " + ex);

        public static void Write(string level, string message)
        {
            try
            {
                // The single-argument overload does not run string.Format, so braces in messages are safe.
                // LogManager creates the content folder itself, so this can run before any GameManager exists.
                LogManager.Log($"[{DateTime.Now:HH:mm:ss.fff}] [WebUI] [{level}] {message}");
            }
            catch
            {
                // Logging must never break the UI.
            }
        }
    }
}
