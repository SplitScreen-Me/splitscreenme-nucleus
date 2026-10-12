using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Nucleus.Bridge.Model;

/// <summary>Nullable reference type annotation of one type position.</summary>
internal enum NullState
{
    /// <summary>Compiled without nullable annotations.</summary>
    Unknown,
    NotNull,
    Nullable,
}

/// <summary>
/// A type with the nullability of each of its positions: the type itself, then its type arguments (or array element).
/// <c>Nullable&lt;T&gt;</c> is unwrapped: the node is <c>T</c> with <see cref="NullState.Nullable"/>.
/// </summary>
internal sealed class NullabilityNode
{
    public NullabilityNode(Type type, NullState state, IReadOnlyList<NullabilityNode> children)
    {
        Type = type;
        State = state;
        Children = children;
    }

    public Type Type { get; }

    public NullState State { get; }

    /// <summary>Type arguments in order, or the element type of an array.</summary>
    public IReadOnlyList<NullabilityNode> Children { get; }

    /// <summary>A node for a type used without annotations, such as an event class.</summary>
    public static NullabilityNode NotNull(Type type) => new(type, NullState.NotNull, Array.Empty<NullabilityNode>());
}

/// <summary>
/// Reads the nullable metadata the C# compiler emits (<c>NullableAttribute</c>, <c>NullableContextAttribute</c>).
/// .NET Framework has no <c>NullabilityInfoContext</c>, so this follows the encoding documented in
/// https://github.com/dotnet/roslyn/blob/main/docs/features/nullable-metadata.md: one byte per type position in
/// pre-order (0 oblivious, 1 not null, 2 nullable), value types take no byte, <c>Nullable&lt;T&gt;</c> is encoded as
/// <c>T</c>, and a missing attribute falls back to the closest <c>NullableContextAttribute</c>.
/// </summary>
internal static class Nullability
{
    private const string NullableAttribute = "System.Runtime.CompilerServices.NullableAttribute";
    private const string NullableContextAttribute = "System.Runtime.CompilerServices.NullableContextAttribute";

    public static NullabilityNode Of(PropertyInfo property) =>
        Read(property.PropertyType, property.GetCustomAttributesData(), property);

    public static NullabilityNode Of(FieldInfo field) =>
        Read(field.FieldType, field.GetCustomAttributesData(), field);

    /// <summary>A method parameter, or <see cref="MethodInfo.ReturnParameter"/>.</summary>
    public static NullabilityNode Of(ParameterInfo parameter) =>
        Read(parameter.ParameterType, parameter.GetCustomAttributesData(), parameter.Member);

    private static NullabilityNode Read(Type type, IEnumerable<CustomAttributeData> attributes, MemberInfo scope)
    {
        var flags = FindFlags(attributes) ?? new Flags(FindContext(scope));
        var index = 0;
        return Walk(type, flags, ref index);
    }

    private static NullabilityNode Walk(Type type, Flags flags, ref int index)
    {
        var underlying = System.Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            var inner = Walk(underlying, flags, ref index);
            return new NullabilityNode(underlying, NullState.Nullable, inner.Children);
        }

        // Value types take no byte of their own, only their type arguments do.
        var state = type.IsValueType ? NullState.NotNull : flags.StateAt(index++);
        var children = type.IsArray ? new[] { type.GetElementType()! } : type.GetGenericArguments();
        var nodes = new NullabilityNode[children.Length];
        for (var i = 0; i < children.Length; i++)
        {
            nodes[i] = Walk(children[i], flags, ref index);
        }

        return new NullabilityNode(type, state, nodes);
    }

    private static Flags? FindFlags(IEnumerable<CustomAttributeData> attributes)
    {
        var attribute = attributes.FirstOrDefault(a => a.AttributeType.FullName == NullableAttribute);
        if (attribute == null)
        {
            return null;
        }

        var value = attribute.ConstructorArguments[0].Value;
        return value is ReadOnlyCollection<CustomAttributeTypedArgument> bytes
            ? new Flags(bytes.Select(b => (byte)b.Value!).ToArray())
            : new Flags((byte)value!);
    }

    private static byte FindContext(MemberInfo? member)
    {
        for (; member != null; member = member.DeclaringType)
        {
            var context = member.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.FullName == NullableContextAttribute);
            if (context != null)
            {
                return (byte)context.ConstructorArguments[0].Value!;
            }
        }

        return 0;
    }

    /// <summary>The bytes of a <c>NullableAttribute</c>: one per position, or a single one for all positions.</summary>
    private sealed class Flags
    {
        private readonly byte[]? _bytes;
        private readonly byte _all;

        public Flags(byte all) => _all = all;

        public Flags(byte[] bytes) => _bytes = bytes;

        public NullState StateAt(int index)
        {
            var flag = _bytes == null ? _all : index < _bytes.Length ? _bytes[index] : (byte)0;
            return flag switch
            {
                1 => NullState.NotNull,
                2 => NullState.Nullable,
                _ => NullState.Unknown,
            };
        }
    }
}
