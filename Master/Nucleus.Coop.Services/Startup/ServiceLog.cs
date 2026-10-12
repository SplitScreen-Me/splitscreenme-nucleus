using System;
using Nucleus.Gaming;

namespace Nucleus.Coop.Services.Startup;

/// <summary>Writes the library, handlers and settings services' lines to the app log (<c>content\app.log</c>).</summary>
public static class ServiceLog
{
    /// <summary>Where lines go. Defaults to <see cref="LogManager"/>; tests replace it.</summary>
    internal static Action<string> Sink { get; set; } = line => LogManager.Log(line);

    /// <summary>Normal operation.</summary>
    public static void Info(string message) => Write("INFO", message, null);

    /// <summary>Something unexpected that the service recovered from.</summary>
    public static void Warn(string message, Exception? exception = null) => Write("WARN", message, exception);

    /// <summary>A failure.</summary>
    public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [services] {level} {message}";
            if (exception != null)
            {
                line += " | " + exception.GetType().Name + ": " + exception.Message;
            }

            Sink(line);
        }
        catch
        {
            // Logging never breaks a service call.
        }
    }
}
