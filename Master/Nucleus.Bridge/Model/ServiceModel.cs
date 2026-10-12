using System;
using System.Collections.Generic;

namespace Nucleus.Bridge.Model;

/// <summary>A <see cref="BridgeServiceAttribute"/> interface.</summary>
public sealed class ServiceModel
{
    internal ServiceModel(string name, Type @interface, IReadOnlyList<MethodModel> methods)
    {
        Name = name;
        Interface = @interface;
        Methods = methods;
    }

    /// <summary>Service name on the wire, for example <c>library</c>.</summary>
    public string Name { get; }

    /// <summary>The C# interface.</summary>
    public Type Interface { get; }

    /// <summary>Methods, sorted by name.</summary>
    public IReadOnlyList<MethodModel> Methods { get; }

    /// <summary>XML documentation rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}
