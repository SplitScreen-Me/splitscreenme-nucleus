namespace Nucleus.Bridge.Model;

/// <summary>Kind of a <see cref="TypeRef"/>.</summary>
public enum TypeRefKind
{
    /// <summary>A JSON string: C# <c>string</c>, <c>Guid</c>.</summary>
    String,

    /// <summary>A JSON number: C# integer and floating-point types.</summary>
    Number,

    /// <summary>A JSON boolean.</summary>
    Boolean,

    /// <summary>An ISO 8601 date string: C# <c>DateTime</c> (UTC), <c>DateTimeOffset</c>.</summary>
    DateTime,

    /// <summary>A JSON array of <see cref="TypeRef.Item"/>.</summary>
    Array,

    /// <summary>A JSON object with string keys and <see cref="TypeRef.Item"/> values.</summary>
    Map,

    /// <summary>A DTO or an enum of the contract, see <see cref="TypeRef.Name"/>.</summary>
    Named,
}

/// <summary>A type as it appears on the wire.</summary>
public sealed class TypeRef
{
    private TypeRef(TypeRefKind kind, string? name, TypeRef? item, bool nullable)
    {
        Kind = kind;
        Name = name;
        Item = item;
        Nullable = nullable;
    }

    /// <summary>What kind of JSON value it is.</summary>
    public TypeRefKind Kind { get; }

    /// <summary>For <see cref="TypeRefKind.Named"/>: the name of a type in <see cref="ContractModel.Types"/>.</summary>
    public string? Name { get; }

    /// <summary>For <see cref="TypeRefKind.Array"/> and <see cref="TypeRefKind.Map"/>: the item or value type.</summary>
    public TypeRef? Item { get; }

    /// <summary>
    /// Whether the value can be null. For a DTO property this means optional: nulls are omitted on the wire.
    /// </summary>
    public bool Nullable { get; }

    internal static TypeRef Primitive(TypeRefKind kind, bool nullable) => new(kind, null, null, nullable);

    internal static TypeRef ArrayOf(TypeRef item, bool nullable) => new(TypeRefKind.Array, null, item, nullable);

    internal static TypeRef MapOf(TypeRef value, bool nullable) => new(TypeRefKind.Map, null, value, nullable);

    internal static TypeRef Named(string name, bool nullable) => new(TypeRefKind.Named, name, null, nullable);

    /// <summary>The same type without nullability.</summary>
    public TypeRef NonNullable() => Nullable ? new TypeRef(Kind, Name, Item, false) : this;

    /// <summary>
    /// Language-neutral notation used in the canonical model and in error messages:
    /// <c>string</c>, <c>number</c>, <c>boolean</c>, <c>date</c>, <c>GameSummary</c>, <c>list&lt;T&gt;</c>,
    /// <c>map&lt;T&gt;</c>, with a <c>?</c> suffix when nullable.
    /// </summary>
    public override string ToString()
    {
        var core = Kind switch
        {
            TypeRefKind.String => "string",
            TypeRefKind.Number => "number",
            TypeRefKind.Boolean => "boolean",
            TypeRefKind.DateTime => "date",
            TypeRefKind.Array => $"list<{Item}>",
            TypeRefKind.Map => $"map<{Item}>",
            _ => Name!,
        };
        return Nullable ? core + "?" : core;
    }
}
