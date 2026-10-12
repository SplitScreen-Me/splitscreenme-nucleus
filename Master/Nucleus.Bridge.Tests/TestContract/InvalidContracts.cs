using System;
using System.Collections.Generic;

namespace Nucleus.Bridge.Tests.TestContract;

/// <summary>Contracts that break the conventions, one problem each. Built one by one, never with the assembly.</summary>
public static class InvalidContracts
{
    [BridgeService("overloads")]
    public interface IOverloads
    {
        void Do();

        void Do(FindItemRequest request);
    }

    public sealed class Box<T>
    {
        public T Value { get; set; } = default!;
    }

    [BridgeService("generic")]
    public interface IGenericDto
    {
        Box<string> Get();
    }

    public sealed class Unsupported
    {
        public object Anything { get; set; } = new();

        public TimeSpan Duration { get; set; }

        public byte[] Bytes { get; set; } = Array.Empty<byte>();

        public Dictionary<int, string> ByNumber { get; set; } = new();
    }

    [BridgeService("unsupported")]
    public interface IUnsupportedTypes
    {
        Unsupported Get();
    }

    [BridgeService("twoParams")]
    public interface ITwoParams
    {
        void Do(FindItemRequest first, FindItemRequest second);
    }

    [BridgeService("primitiveParam")]
    public interface IPrimitiveParam
    {
        void Do(string name);
    }

    [BridgeService("nullableParam")]
    public interface INullableParam
    {
        void Do(FindItemRequest? request);
    }

    [BridgeService("Bad-Name")]
    public interface IBadServiceName
    {
        void Do();
    }

    [BridgeEvent("changed")]
    public sealed class BadEventName
    {
    }

    [BridgeEvent("shelf.updated", Invalidates = new[] { "shelf.nope" })]
    public sealed class UnknownInvalidation
    {
    }

    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
    }

    [BridgeService("flags")]
    public interface IFlagsEnum
    {
        Permissions Get();
    }

    [BridgeService("property")]
    public interface IHasProperty
    {
        int Count { get; }
    }

#nullable disable
    public sealed class Oblivious
    {
        public string Name { get; set; }
    }
#nullable restore

    [BridgeService("oblivious")]
    public interface IObliviousDto
    {
        Oblivious Get();
    }
}
