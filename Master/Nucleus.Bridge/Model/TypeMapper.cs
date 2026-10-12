using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Nucleus.Bridge.Model;

/// <summary>
/// Maps C# types to wire types and discovers the DTOs and enums they reference, transitively.
/// Problems are reported to the error list rather than thrown, so one build lists all of them.
/// </summary>
internal sealed class TypeMapper
{
    private static readonly HashSet<Type> NumberTypes = new()
    {
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
        typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
    };

    private static readonly HashSet<Type> ListDefinitions = new()
    {
        typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>),
        typeof(IReadOnlyList<>), typeof(IReadOnlyCollection<>), typeof(HashSet<>), typeof(ISet<>),
    };

    private static readonly HashSet<Type> MapDefinitions = new()
    {
        typeof(Dictionary<,>), typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>),
    };

    private readonly List<string> _errors;
    private readonly HashSet<Type> _discovered = new();
    private readonly Queue<Type> _pending = new();

    public TypeMapper(List<string> errors) => _errors = errors;

    /// <summary>Whether a C# type maps to a DTO (a JSON object with known properties).</summary>
    public static bool IsObjectType(Type type) =>
        (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum))
        && type != typeof(string)
        && System.Nullable.GetUnderlyingType(type) == null
        && !type.IsArray
        && !(type.IsGenericType && (ListDefinitions.Contains(type.GetGenericTypeDefinition()) || MapDefinitions.Contains(type.GetGenericTypeDefinition())))
        && !IsFrameworkType(type);

    /// <summary>Maps a type position. <paramref name="location"/> prefixes error messages.</summary>
    public TypeRef Map(NullabilityNode node, string location)
    {
        var type = node.Type;
        var nullable = node.State == NullState.Nullable;
        if (node.State == NullState.Unknown)
        {
            Error(location, $"the nullability of {Describe(type)} is unknown: compile the contract with <Nullable>enable</Nullable>");
        }

        if (type == typeof(string) || type == typeof(Guid))
        {
            return TypeRef.Primitive(TypeRefKind.String, nullable);
        }

        if (type == typeof(bool))
        {
            return TypeRef.Primitive(TypeRefKind.Boolean, nullable);
        }

        if (NumberTypes.Contains(type))
        {
            return TypeRef.Primitive(TypeRefKind.Number, nullable);
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return TypeRef.Primitive(TypeRefKind.DateTime, nullable);
        }

        if (type == typeof(byte[]))
        {
            return Unsupported(location, type, "byte[] is sent as base64; use a string or a list of numbers explicitly");
        }

        if (type.IsArray)
        {
            return type.GetArrayRank() == 1
                ? TypeRef.ArrayOf(Map(node.Children[0], location), nullable)
                : Unsupported(location, type, "use a jagged array or a list of lists");
        }

        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            if (ListDefinitions.Contains(definition))
            {
                return TypeRef.ArrayOf(Map(node.Children[0], location), nullable);
            }

            if (MapDefinitions.Contains(definition))
            {
                return node.Children[0].Type == typeof(string)
                    ? TypeRef.MapOf(Map(node.Children[1], location), nullable)
                    : Unsupported(location, type, "dictionary keys must be strings");
            }

            return Unsupported(location, type, "generic types other than lists and dictionaries are not supported; declare a concrete class");
        }

        if (type.IsEnum || IsObjectType(type))
        {
            return Discover(type, nullable, location);
        }

        return Unsupported(location, type, null);
    }

    /// <summary>Builds the models of every type discovered so far, including the ones they reference.</summary>
    public IReadOnlyList<TypeModel> BuildTypes()
    {
        var types = new List<TypeModel>();
        while (_pending.Count > 0)
        {
            var type = _pending.Dequeue();
            types.Add(type.IsEnum ? BuildEnum(type) : BuildObject(type));
        }

        foreach (var clash in types.GroupBy(t => t.Name).Where(g => g.Count() > 1))
        {
            _errors.Add($"Two contract types are named {clash.Key}: {string.Join(", ", clash.Select(t => t.ClrType.FullName))}. Type names must be unique.");
        }

        return types.OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
    }

    private TypeRef Discover(Type type, bool nullable, string location)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return Unsupported(location, type, "abstract classes and interfaces cannot be deserialized; use a concrete class");
        }

        if (_discovered.Add(type))
        {
            _pending.Enqueue(type);
        }

        return TypeRef.Named(type.Name, nullable);
    }

    private ObjectTypeModel BuildObject(Type type)
    {
        var properties = new List<(PropertyModel Model, int Depth, int Token)>();
        if (BridgeJson.ContractResolver.ResolveContract(type) is not JsonObjectContract contract)
        {
            _errors.Add($"{type.Name}: not serialized as a JSON object.");
            return new ObjectTypeModel(type.Name, type, Array.Empty<PropertyModel>());
        }

        if (contract.Converter != null)
        {
            _errors.Add($"{type.Name}: custom JSON converters are not supported in the contract.");
        }

        foreach (var property in contract.Properties.Where(p => !p.Ignored && p.Readable))
        {
            var member = FindMember(property);
            var location = $"{type.Name}.{property.UnderlyingName}";
            if (member == null || property.Converter != null)
            {
                _errors.Add($"{location}: only plain properties and fields are supported (no custom JSON converters).");
                continue;
            }

            var nullability = member is PropertyInfo info ? Nullability.Of(info) : Nullability.Of((FieldInfo)member);
            var model = new PropertyModel(property.PropertyName!, Map(nullability, location), member);
            properties.Add((model, Depth(member.DeclaringType!), member.MetadataToken));
        }

        var ordered = properties.OrderBy(p => p.Depth).ThenBy(p => p.Token).Select(p => p.Model).ToList();
        return new ObjectTypeModel(type.Name, type, ordered);
    }

    private EnumTypeModel BuildEnum(Type type)
    {
        if (type.IsDefined(typeof(FlagsAttribute), false))
        {
            _errors.Add($"{type.Name}: [Flags] enums are not supported; use a list of enum values.");
        }

        var members = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(f => f.MetadataToken)
            .Select(field =>
            {
                if (field.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.Serialization.EnumMemberAttribute"))
                {
                    _errors.Add($"{type.Name}.{field.Name}: [EnumMember] is not supported; the wire value is the camelCase member name.");
                }

                return new EnumMemberModel(BridgeJson.Naming.GetPropertyName(field.Name, false), field);
            })
            .ToList();
        return new EnumTypeModel(type.Name, type, members);
    }

    private static MemberInfo? FindMember(JsonProperty property)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var declaring = property.DeclaringType!;
        var name = property.UnderlyingName!;
        return (MemberInfo?)declaring.GetProperty(name, flags) ?? declaring.GetField(name, flags);
    }

    private static int Depth(Type type)
    {
        var depth = 0;
        for (var t = type.BaseType; t != null; t = t.BaseType)
        {
            depth++;
        }

        return depth;
    }

    private static bool IsFrameworkType(Type type) =>
        type == typeof(object) || type.Namespace == "System" || (type.Namespace?.StartsWith("System.", StringComparison.Ordinal) ?? false)
        || (type.Namespace?.StartsWith("Newtonsoft.", StringComparison.Ordinal) ?? false);

    private TypeRef Unsupported(string location, Type type, string? hint)
    {
        Error(location, $"{Describe(type)} is not supported" + (hint == null ? "." : $": {hint}."));
        return TypeRef.Primitive(TypeRefKind.String, false);
    }

    private void Error(string location, string message) => _errors.Add($"{location}: {message}");

    private static string Describe(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var name = type.Name.Substring(0, type.Name.IndexOf('`'));
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Describe))}>";
    }
}
