using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Nucleus.Bridge;

/// <summary>
/// The JSON conventions of the bridge, used in both directions: camelCase properties, enums as camelCase strings,
/// dates as ISO 8601 UTC, nulls omitted, no type names. The contract model reads property names from the same
/// settings, so the generated TypeScript always matches the wire.
/// </summary>
public static class BridgeJson
{
    // Declared before Serializer: static fields initialize in textual order.
    private static readonly DefaultContractResolver Resolver = new() { NamingStrategy = new CamelCaseNamingStrategy() };

    internal static readonly CamelCaseNamingStrategy Naming = new();

    internal static readonly JsonSerializer Serializer = JsonSerializer.Create(CreateSettings());

    /// <summary>
    /// A copy of the bridge settings, for code that must serialize exactly like the bridge.
    /// Each access returns a new instance, so changing it does not affect the bridge.
    /// </summary>
    public static JsonSerializerSettings Settings => CreateSettings();

    internal static IContractResolver ContractResolver => Resolver;

    private static JsonSerializerSettings CreateSettings() => new()
    {
        ContractResolver = Resolver,
        Converters = { new StringEnumConverter(Naming) { AllowIntegerValues = false } },
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        // Keep ISO strings as strings while parsing envelopes; they become DateTime only when a DTO asks for one.
        DateParseHandling = DateParseHandling.None,
        TypeNameHandling = TypeNameHandling.None,
        MaxDepth = 64,
    };
}
