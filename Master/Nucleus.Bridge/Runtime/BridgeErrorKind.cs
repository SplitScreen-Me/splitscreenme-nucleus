namespace Nucleus.Bridge;

/// <summary>
/// Kind of a bridge error. The numeric value of each member is its JSON-RPC error code; the UI receives the kind
/// as a camelCase string (<c>notFound</c>).
/// </summary>
public enum BridgeErrorKind
{
    /// <summary>The message is not valid JSON.</summary>
    Parse = -32700,

    /// <summary>The message is JSON but not a valid JSON-RPC 2.0 request.</summary>
    InvalidRequest = -32600,

    /// <summary>The method does not exist, or no implementation is registered for its service.</summary>
    MethodNotFound = -32601,

    /// <summary>The params do not bind to the method's request type, or fail validation.</summary>
    InvalidParams = -32602,

    /// <summary>Unexpected failure. The message sent to the UI is generic; details are in the log.</summary>
    Internal = -32603,

    /// <summary>The UI cancelled the request (<c>$/cancel</c>, page reload).</summary>
    Cancelled = -32800,

    /// <summary>A dependency is not available: hub offline, device missing…</summary>
    Unavailable = -32001,

    /// <summary>The request conflicts with the current state, for example a session is already running.</summary>
    Conflict = -32002,

    /// <summary>Unknown game, handler, profile…</summary>
    NotFound = -32004,
}
