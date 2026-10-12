using System;

namespace Nucleus.Bridge;

/// <summary>Severity of a <see cref="BridgeLogEntry"/>.</summary>
public enum BridgeLogLevel
{
    /// <summary>Every call with its duration, every event.</summary>
    Debug,

    /// <summary>Lifecycle: page reloads, cancellations.</summary>
    Info,

    /// <summary>Expected failures (<see cref="BridgeException"/>) and ignored messages.</summary>
    Warning,

    /// <summary>Unexpected exceptions thrown by services.</summary>
    Error,
}

/// <summary>A line of the bridge log, passed to the logging callback given to <see cref="BridgeHost"/>.</summary>
public sealed class BridgeLogEntry
{
    internal BridgeLogEntry(BridgeLogLevel level, string message, Exception? exception = null)
    {
        Level = level;
        Message = message;
        Exception = exception;
    }

    /// <summary>Severity.</summary>
    public BridgeLogLevel Level { get; }

    /// <summary>One line of text, for example <c>library.setFavorite #7 ok (3 ms)</c>.</summary>
    public string Message { get; }

    /// <summary>The exception behind an <see cref="BridgeLogLevel.Error"/> entry, or null.</summary>
    public Exception? Exception { get; }

    /// <summary>The message, followed by the exception when there is one.</summary>
    public override string ToString() => Exception == null ? Message : Message + Environment.NewLine + Exception;
}
