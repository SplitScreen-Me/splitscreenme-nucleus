using System;

namespace Nucleus.Bridge;

/// <summary>
/// Marks an interface as a bridge service. Each method becomes a JSON-RPC method named
/// <c>&lt;name&gt;.&lt;camelCaseMethod&gt;</c> that the UI can call.
/// </summary>
/// <remarks>
/// A method takes no parameter or one request DTO, optionally followed by a
/// <see cref="System.Threading.CancellationToken"/>, and returns <c>void</c>, <c>T</c>,
/// <see cref="System.Threading.Tasks.Task"/> or <c>Task&lt;T&gt;</c>. Overloads are not allowed.
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class BridgeServiceAttribute : Attribute
{
    /// <summary>Marks an interface as the bridge service <paramref name="name"/>.</summary>
    /// <param name="name">Service name on the wire, in camelCase, for example <c>library</c>.</param>
    public BridgeServiceAttribute(string name) => Name = name;

    /// <summary>Service name on the wire, for example <c>library</c>.</summary>
    public string Name { get; }
}
