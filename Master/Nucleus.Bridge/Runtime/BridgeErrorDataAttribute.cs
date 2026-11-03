using System;

namespace Nucleus.Bridge;

/// <summary>
/// Marks a DTO class that describes the <c>data</c> of a <see cref="BridgeException"/> some methods throw (for example
/// the handlers an executable was refused by). No method returns it, so without the attribute it would not be in the
/// contract: with it, the model and the generated TypeScript include it like any DTO, and the UI reads the error data
/// as that type. The throwing method's documentation names the error kind and the <c>reason</c> that come with it.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BridgeErrorDataAttribute : Attribute
{
}
