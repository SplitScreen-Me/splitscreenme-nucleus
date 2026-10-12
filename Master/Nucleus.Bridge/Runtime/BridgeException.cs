using System;

namespace Nucleus.Bridge;

/// <summary>
/// An expected failure that a service reports to the UI, for example an unknown game. The UI receives
/// <see cref="Kind"/>, the message and <see cref="ErrorData"/> as they are. Any other exception thrown by a service
/// becomes <see cref="BridgeErrorKind.Internal"/> with a generic message.
/// </summary>
/// <example><code>throw new BridgeException(BridgeErrorKind.NotFound, "Game not found", new { gameId });</code></example>
public class BridgeException : Exception
{
    /// <summary>Creates a bridge error.</summary>
    /// <param name="kind">What went wrong; decides the JSON-RPC error code.</param>
    /// <param name="message">Message for the user or the developer, shown as is by the UI.</param>
    /// <param name="data">Optional details, serialized with the bridge JSON settings as the JSON-RPC error <c>data</c>.</param>
    public BridgeException(BridgeErrorKind kind, string message, object? data = null)
        : base(message)
    {
        Kind = kind;
        ErrorData = data;
    }

    /// <summary>What went wrong.</summary>
    public BridgeErrorKind Kind { get; }

    /// <summary>JSON-RPC error code of <see cref="Kind"/>.</summary>
    public int Code => (int)Kind;

    /// <summary>Details sent as the JSON-RPC error <c>data</c>, or null.</summary>
    public object? ErrorData { get; }
}
