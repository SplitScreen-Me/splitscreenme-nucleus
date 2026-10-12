using System;
using System.Reflection;

namespace Nucleus.Bridge.Model;

/// <summary>A method of a service.</summary>
public sealed class MethodModel
{
    internal MethodModel(string serviceName, string name, MethodInfo method, Type? paramsType, TypeRef? @params, bool acceptsCancellation, TypeRef? result)
    {
        ServiceName = serviceName;
        Name = name;
        Method = method;
        ParamsType = paramsType;
        Params = @params;
        AcceptsCancellation = acceptsCancellation;
        Result = result;
    }

    /// <summary>Name of the service that declares the method.</summary>
    public string ServiceName { get; }

    /// <summary>Method name on the wire, without the service: <c>setFavorite</c>.</summary>
    public string Name { get; }

    /// <summary>Full name on the wire: <c>library.setFavorite</c>.</summary>
    public string WireName => ServiceName + "." + Name;

    /// <summary>The C# interface method.</summary>
    public MethodInfo Method { get; }

    /// <summary>The request DTO type, or null when the method takes no parameter.</summary>
    public Type? ParamsType { get; }

    /// <summary>The request DTO as a wire type, or null when the method takes no parameter.</summary>
    public TypeRef? Params { get; }

    /// <summary>Whether the last C# parameter is a <see cref="System.Threading.CancellationToken"/>.</summary>
    public bool AcceptsCancellation { get; }

    /// <summary>The result as a wire type, or null for <c>void</c> and <see cref="System.Threading.Tasks.Task"/>.</summary>
    public TypeRef? Result { get; }

    /// <summary>XML documentation rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}
