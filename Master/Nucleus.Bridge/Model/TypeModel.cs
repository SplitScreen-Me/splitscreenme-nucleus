using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nucleus.Bridge.Model;

/// <summary>A named type of the contract: a DTO (<see cref="ObjectTypeModel"/>) or an enum (<see cref="EnumTypeModel"/>).</summary>
public abstract class TypeModel
{
    private protected TypeModel(string name, Type clrType)
    {
        Name = name;
        ClrType = clrType;
    }

    /// <summary>Name in the generated code: the C# type name.</summary>
    public string Name { get; }

    /// <summary>The C# type.</summary>
    public Type ClrType { get; }

    /// <summary>XML documentation rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}

/// <summary>A DTO: a JSON object with known properties.</summary>
public sealed class ObjectTypeModel : TypeModel
{
    internal ObjectTypeModel(string name, Type clrType, IReadOnlyList<PropertyModel> properties)
        : base(name, clrType) => Properties = properties;

    /// <summary>Serialized properties, base class first, then in declaration order.</summary>
    public IReadOnlyList<PropertyModel> Properties { get; }
}

/// <summary>A property of a DTO.</summary>
public sealed class PropertyModel
{
    internal PropertyModel(string name, TypeRef type, MemberInfo member)
    {
        Name = name;
        Type = type;
        Member = member;
    }

    /// <summary>Name on the wire (camelCase).</summary>
    public string Name { get; }

    /// <summary>Wire type. <see cref="TypeRef.Nullable"/> means the property is optional.</summary>
    public TypeRef Type { get; }

    /// <summary>The C# property or field.</summary>
    public MemberInfo Member { get; }

    /// <summary>XML documentation rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}

/// <summary>An enum, sent as camelCase strings.</summary>
public sealed class EnumTypeModel : TypeModel
{
    internal EnumTypeModel(string name, Type clrType, IReadOnlyList<EnumMemberModel> members)
        : base(name, clrType) => Members = members;

    /// <summary>Members in declaration order.</summary>
    public IReadOnlyList<EnumMemberModel> Members { get; }
}

/// <summary>A member of an enum.</summary>
public sealed class EnumMemberModel
{
    internal EnumMemberModel(string value, FieldInfo field)
    {
        Value = value;
        Field = field;
    }

    /// <summary>Value on the wire, for example <c>warning</c>.</summary>
    public string Value { get; }

    /// <summary>The C# enum field.</summary>
    public FieldInfo Field { get; }

    /// <summary>XML documentation rendered as Markdown, or null.</summary>
    public string? Doc { get; internal set; }
}
