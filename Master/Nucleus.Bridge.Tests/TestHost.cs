using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Model;
using Nucleus.Bridge.Tests.TestContract;

namespace Nucleus.Bridge.Tests;

/// <summary>A <see cref="BridgeHost"/> serving the shelf contract, with its log and outgoing messages recorded.</summary>
internal sealed class TestHost
{
    public static readonly ContractModel Contract = ContractModel.Build(ShelfContract.Types);

    public TestHost(SynchronizationContext? context = null)
    {
        Host = new BridgeHost(context, Log.Add, Contract);
        Host.Register<IShelfService>(Shelf);
        Host.Outgoing += Outgoing.Add;
    }

    public BridgeHost Host { get; }

    public FakeShelfService Shelf { get; } = new();

    public List<BridgeLogEntry> Log { get; } = new();

    public List<string> Outgoing { get; } = new();

    /// <summary>Sends a request and parses the response.</summary>
    public async Task<JObject> CallAsync(string method, object? @params = null, int id = 1)
    {
        var response = await Host.HandleAsync(Request(method, @params, id));
        return Parse(response!);
    }

    /// <summary>Parses a message keeping ISO dates as strings, as they are on the wire.</summary>
    public static JObject Parse(string message) =>
        JObject.Load(new JsonTextReader(new StringReader(message)) { DateParseHandling = DateParseHandling.None });

    public static string Request(string method, object? @params = null, int id = 1)
    {
        var request = new JObject { ["jsonrpc"] = "2.0", ["id"] = id, ["method"] = method };
        if (@params != null)
        {
            request["params"] = JToken.FromObject(@params);
        }

        return request.ToString();
    }
}
