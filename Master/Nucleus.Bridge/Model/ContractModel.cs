using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nucleus.Bridge.Model;

/// <summary>
/// The canonical description of a bridge contract: services and their methods, DTOs, enums and events, read by
/// reflection from <see cref="BridgeServiceAttribute"/> interfaces and <see cref="BridgeEventAttribute"/> classes.
/// <see cref="BridgeHost"/> dispatches with it and Nucleus.Bridge.Gen generates the TypeScript contract from it,
/// so both always agree.
/// </summary>
public sealed class ContractModel
{
    private static readonly Lazy<ContractModel> NucleusContract = new(() => Build(typeof(ContractModel).Assembly));

    internal ContractModel(IReadOnlyList<ServiceModel> services, IReadOnlyList<TypeModel> types, IReadOnlyList<EventModel> events)
    {
        Services = services;
        Types = types;
        Events = events;
        Hash = ComputeHash(ToJson(includeDocs: false));
    }

    /// <summary>The Nucleus Co-op contract (namespace <c>Nucleus.Bridge.Contracts</c>), built once, without docs.</summary>
    public static ContractModel Default => NucleusContract.Value;

    /// <summary>Services, sorted by name.</summary>
    public IReadOnlyList<ServiceModel> Services { get; }

    /// <summary>DTOs and enums reachable from the services and events, sorted by name.</summary>
    public IReadOnlyList<TypeModel> Types { get; }

    /// <summary>Events, sorted by name.</summary>
    public IReadOnlyList<EventModel> Events { get; }

    /// <summary>Every method of every service, in service then method order.</summary>
    public IEnumerable<MethodModel> Methods => Services.SelectMany(s => s.Methods);

    /// <summary>
    /// First 16 hex characters of the SHA-256 of the canonical JSON (<see cref="ToJson"/> without docs).
    /// It changes when anything visible on the wire changes, and only then: editing a comment keeps it.
    /// </summary>
    public string Hash { get; }

    /// <summary>Builds the model of the contract declared in <paramref name="assembly"/>.</summary>
    /// <param name="assembly">Assembly whose public types contain the service interfaces and event classes.</param>
    /// <param name="docs">XML documentation of the assembly, to fill the <c>Doc</c> properties. Optional.</param>
    /// <exception cref="ContractModelException">The contract breaks the conventions.</exception>
    public static ContractModel Build(Assembly assembly, XmlDocs? docs = null) => Build(assembly.GetExportedTypes(), docs);

    /// <summary>Builds the model of the contract declared by <paramref name="types"/>.</summary>
    /// <param name="types">
    /// Service interfaces and event classes. Other types are ignored; DTOs and enums are discovered from them.
    /// </param>
    /// <param name="docs">XML documentation of the assembly, to fill the <c>Doc</c> properties. Optional.</param>
    /// <exception cref="ContractModelException">The contract breaks the conventions.</exception>
    public static ContractModel Build(IEnumerable<Type> types, XmlDocs? docs = null) => new ContractModelBuilder().Build(types, docs);

    /// <summary>
    /// The canonical JSON form of the model: compact, deterministic, wire names and types only (plus docs on request).
    /// Diff two dumps to see why <see cref="Hash"/> changed.
    /// </summary>
    /// <param name="includeDocs">Include the <c>Doc</c> properties.</param>
    public string ToJson(bool includeDocs = false)
    {
        JObject Node(params (string Name, object? Value)[] properties)
        {
            var node = new JObject();
            foreach (var (name, value) in properties)
            {
                if (value != null)
                {
                    node.Add(name, value as JToken ?? JToken.FromObject(value));
                }
            }

            return node;
        }

        string? Doc(string? doc) => includeDocs ? doc : null;

        var root = Node(
            ("services", new JArray(Services.Select(s => Node(
                ("name", s.Name),
                ("doc", Doc(s.Doc)),
                ("methods", new JArray(s.Methods.Select(m => Node(
                    ("name", m.Name),
                    ("doc", Doc(m.Doc)),
                    ("params", m.Params?.ToString()),
                    ("result", m.Result?.ToString()))))))))),
            ("types", new JArray(Types.Select(t => t switch
            {
                ObjectTypeModel dto => Node(
                    ("name", t.Name),
                    ("doc", Doc(t.Doc)),
                    ("properties", new JArray(dto.Properties.Select(p => Node(
                        ("name", p.Name),
                        ("doc", Doc(p.Doc)),
                        ("type", p.Type.ToString())))))),
                EnumTypeModel enumType => Node(
                    ("name", t.Name),
                    ("doc", Doc(t.Doc)),
                    ("values", new JArray(enumType.Members.Select(m => Node(
                        ("value", m.Value),
                        ("doc", Doc(m.Doc))))))),
                _ => throw new InvalidOperationException(t.GetType().Name),
            }))),
            ("events", new JArray(Events.Select(e => Node(
                ("name", e.Name),
                ("doc", Doc(e.Doc)),
                ("payload", e.PayloadName),
                ("invalidates", new JArray(e.Invalidates)))))));
        return root.ToString(Formatting.None);
    }

    private static string ComputeHash(string canonicalJson)
    {
        using var sha = SHA256.Create();
        var digest = sha.ComputeHash(Encoding.UTF8.GetBytes(canonicalJson));
        return string.Concat(digest.Take(8).Select(b => b.ToString("x2")));
    }
}
