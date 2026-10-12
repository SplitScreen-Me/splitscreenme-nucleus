using System;

namespace Nucleus.Bridge;

/// <summary>
/// Marks a DTO class as a bridge event: the host sends it to the UI with
/// <see cref="IBridgeEvents.Emit{TEvent}"/> as a JSON-RPC notification named <see cref="Name"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BridgeEventAttribute : Attribute
{
    /// <summary>Marks a class as the bridge event <paramref name="name"/>.</summary>
    /// <param name="name">Event name on the wire, <c>&lt;service&gt;.&lt;camelCaseName&gt;</c>, for example <c>library.changed</c>.</param>
    public BridgeEventAttribute(string name) => Name = name;

    /// <summary>Event name on the wire, for example <c>library.changed</c>.</summary>
    public string Name { get; }

    /// <summary>
    /// Wire names of the methods whose cached results become stale when the event arrives, for example
    /// <c>library.list</c>. The UI refetches them automatically.
    /// </summary>
    public string[] Invalidates { get; set; } = Array.Empty<string>();
}
