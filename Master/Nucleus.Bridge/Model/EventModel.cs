using System;
using System.Collections.Generic;

namespace Nucleus.Bridge.Model;

/// <summary>A <see cref="BridgeEventAttribute"/> class.</summary>
public sealed class EventModel
{
    internal EventModel(string name, Type payloadType, IReadOnlyList<string> invalidates)
    {
        Name = name;
        PayloadType = payloadType;
        Invalidates = invalidates;
    }

    /// <summary>Event name on the wire, for example <c>library.changed</c>.</summary>
    public string Name { get; }

    /// <summary>The event class. It is also a DTO of <see cref="ContractModel.Types"/>, with the same name.</summary>
    public Type PayloadType { get; }

    /// <summary>Name of the payload DTO in <see cref="ContractModel.Types"/>.</summary>
    public string PayloadName => PayloadType.Name;

    /// <summary>Wire names of the methods the UI refetches when the event arrives.</summary>
    public IReadOnlyList<string> Invalidates { get; }

    /// <summary>XML documentation of the event class rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}
