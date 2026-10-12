using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nucleus.Bridge;

/// <summary>An incoming JSON-RPC 2.0 request or notification.</summary>
internal sealed class JsonRpcRequest
{
    private JsonRpcRequest(JToken? id, string method, JToken? @params)
    {
        Id = id;
        Method = method;
        Params = @params;
    }

    /// <summary>The request id (string or number), or null for a notification.</summary>
    public JToken? Id { get; }

    public string Method { get; }

    public JToken? Params { get; }

    public bool IsNotification => Id == null;

    /// <summary>The id as written on the wire, for logs and cancellation lookups: <c>7</c>, <c>"a"</c>.</summary>
    public string IdText => IdToText(Id);

    public static string IdToText(JToken? id) => id?.ToString(Formatting.None) ?? "null";

    /// <summary>Parses a message, or returns the error response to send back.</summary>
    public static JsonRpcRequest? Parse(string message, out string? errorResponse)
    {
        errorResponse = null;
        JToken root;
        try
        {
            using var reader = new JsonTextReader(new StringReader(message)) { DateParseHandling = DateParseHandling.None };
            root = JToken.ReadFrom(reader);
        }
        catch (JsonException e)
        {
            errorResponse = JsonRpcWriter.Error(null, BridgeErrorKind.Parse, "Parse error: " + e.Message, null);
            return null;
        }

        if (root is not JObject request)
        {
            errorResponse = JsonRpcWriter.Error(null, BridgeErrorKind.InvalidRequest, "Expected a JSON-RPC request object (batches are not supported).", null);
            return null;
        }

        var hasId = request.TryGetValue("id", out var id);
        if (hasId && id!.Type is not (JTokenType.Integer or JTokenType.String))
        {
            errorResponse = JsonRpcWriter.Error(null, BridgeErrorKind.InvalidRequest, "The id must be a string or an integer.", null);
            return null;
        }

        if ((request["jsonrpc"] as JValue)?.Value as string != "2.0" || request["method"]?.Type != JTokenType.String)
        {
            errorResponse = JsonRpcWriter.Error(id, BridgeErrorKind.InvalidRequest, "Expected \"jsonrpc\": \"2.0\" and a string \"method\".", null);
            return null;
        }

        return new JsonRpcRequest(hasId ? id : null, (string)request["method"]!, request["params"]);
    }
}

/// <summary>Writes outgoing JSON-RPC 2.0 messages with the bridge serializer.</summary>
internal static class JsonRpcWriter
{
    /// <summary>A success response. <paramref name="result"/> is written even when null (void methods).</summary>
    public static string Result(JToken? id, object? result) => Write(writer =>
    {
        WriteHeader(writer, id);
        writer.WritePropertyName("result");
        BridgeJson.Serializer.Serialize(writer, result);
    });

    public static string Error(JToken? id, BridgeErrorKind kind, string message, object? data) => Write(writer =>
    {
        WriteHeader(writer, id);
        writer.WritePropertyName("error");
        writer.WriteStartObject();
        writer.WritePropertyName("code");
        writer.WriteValue((int)kind);
        writer.WritePropertyName("message");
        writer.WriteValue(message);
        if (data != null)
        {
            writer.WritePropertyName("data");
            BridgeJson.Serializer.Serialize(writer, data);
        }

        writer.WriteEndObject();
    });

    public static string Notification(string method, object payload) => Write(writer =>
    {
        writer.WritePropertyName("jsonrpc");
        writer.WriteValue("2.0");
        writer.WritePropertyName("method");
        writer.WriteValue(method);
        writer.WritePropertyName("params");
        BridgeJson.Serializer.Serialize(writer, payload);
    });

    private static void WriteHeader(JsonWriter writer, JToken? id)
    {
        writer.WritePropertyName("jsonrpc");
        writer.WriteValue("2.0");
        writer.WritePropertyName("id");
        if (id == null)
        {
            writer.WriteNull();
        }
        else
        {
            id.WriteTo(writer);
        }
    }

    private static string Write(Action<JsonWriter> body)
    {
        using var text = new StringWriter(CultureInfo.InvariantCulture);
        using (var writer = new JsonTextWriter(text))
        {
            writer.WriteStartObject();
            body(writer);
            writer.WriteEndObject();
        }

        return text.ToString();
    }
}
