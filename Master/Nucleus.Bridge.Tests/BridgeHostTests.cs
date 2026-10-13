using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Tests.TestContract;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class BridgeHostTests
{
    private readonly TestHost _test = new();

    [Fact]
    public async Task Routes_an_async_method_and_serializes_camel_case()
    {
        var response = await _test.CallAsync("shelf.list", id: 7);

        Assert.Equal("2.0", (string?)response["jsonrpc"]);
        Assert.Equal(7, (int)response["id"]!);
        var first = response["result"]![0]!;
        Assert.Equal("Apple", (string?)first["name"]);
        Assert.Equal("red", (string?)first["color"]);
        Assert.Null(response["result"]![2]!["color"]);
    }

    [Fact]
    public async Task Binds_the_request_dto()
    {
        var response = await _test.CallAsync("shelf.find", new { index = 1 });

        Assert.Equal("b", (string?)response["result"]!["id"]);
        Assert.Equal("skyBlue", (string?)response["result"]!["color"]);
    }

    [Fact]
    public async Task Writes_null_results_explicitly()
    {
        var notFound = await _test.CallAsync("shelf.find", new { index = 99 });
        var voidMethod = await _test.CallAsync("shelf.rename", new { itemId = "a", name = "Avocado" });

        Assert.Equal(JTokenType.Null, notFound.Property("result")!.Value.Type);
        Assert.Equal(JTokenType.Null, voidMethod.Property("result")!.Value.Type);
        Assert.Equal("Avocado", _test.Shelf.Items[0].Name);
    }

    [Fact]
    public async Task Returns_dictionaries_as_objects_with_keys_untouched()
    {
        _test.Shelf.Items.Add(new ShelfItem { Id = "Mixed_Case", Name = "Key" });

        var response = await _test.CallAsync("shelf.labels");

        Assert.Equal("Key", (string?)response["result"]!["Mixed_Case"]);
    }

    [Fact]
    public async Task Round_trips_dates_enums_and_omits_nulls()
    {
        var response = await _test.CallAsync("shelf.echo", new
        {
            text = "t",
            at = "2026-10-11T14:30:00Z",
            color = "skyBlue",
            names = new[] { "x" },
        });

        var result = response["result"]!;
        Assert.Equal("2026-10-11T14:30:00Z", (string?)result["at"]);
        Assert.Equal("skyBlue", (string?)result["color"]);
        Assert.Null(result["maybeText"]);
        Assert.Null(result["spare"]);
    }

    [Fact]
    public async Task Missing_params_are_invalid_params()
    {
        var response = await _test.CallAsync("shelf.find");

        AssertError(response, -32602);
        Assert.Contains("FindItemRequest", (string?)response["error"]!["message"]);
        Assert.Equal("badParams", (string?)response["error"]!["data"]!["reason"]);
    }

    [Fact]
    public async Task Params_that_do_not_bind_are_invalid_params()
    {
        var response = await _test.CallAsync("shelf.find", new { index = "first" });

        AssertError(response, -32602);
        Assert.Equal("badParams", (string?)response["error"]!["data"]!["reason"]);
    }

    [Fact]
    public async Task Bridge_exceptions_keep_their_kind_message_and_data()
    {
        var response = await _test.CallAsync("shelf.rename", new { itemId = "zz", name = "x" }, id: 3);

        AssertError(response, -32004);
        Assert.Equal(3, (int)response["id"]!);
        Assert.Equal("No such item.", (string?)response["error"]!["message"]);
        Assert.Equal("zz", (string?)response["error"]!["data"]!["itemId"]);
        Assert.Contains(_test.Log, e => e.Level == BridgeLogLevel.Warning && e.Message.Contains("notFound"));
    }

    [Fact]
    public async Task Unexpected_exceptions_are_internal_with_a_generic_message()
    {
        var response = await _test.CallAsync("shelf.fail");

        AssertError(response, -32603);
        Assert.DoesNotContain("secret", (string?)response["error"]!["message"]);
        Assert.Null(response["error"]!["data"]);
        var entry = Assert.Single(_test.Log, e => e.Level == BridgeLogLevel.Error);
        Assert.IsType<InvalidOperationException>(entry.Exception);
        Assert.Contains("shelf.fail #1", entry.Message);
    }

    [Fact]
    public async Task Unknown_methods_and_unregistered_services_are_method_not_found()
    {
        var unknown = await _test.CallAsync("shelf.nope");
        var unregistered = await _test.CallAsync("clock.now");

        AssertError(unknown, -32601);
        AssertError(unregistered, -32601);
        Assert.Contains("no 'clock' service is registered", (string?)unregistered["error"]!["message"]);
    }

    [Theory]
    [InlineData("{not json", -32700)]
    [InlineData("[1, 2]", -32600)]
    [InlineData("{\"id\": 1, \"method\": \"shelf.list\"}", -32600)]
    [InlineData("{\"jsonrpc\": \"2.0\", \"id\": 1}", -32600)]
    [InlineData("{\"jsonrpc\": {}, \"id\": 1, \"method\": \"shelf.list\"}", -32600)]
    [InlineData("{\"jsonrpc\": \"2.0\", \"id\": {}, \"method\": \"shelf.list\"}", -32600)]
    public async Task Malformed_messages_get_protocol_errors(string message, int code)
    {
        var response = TestHost.Parse((await _test.Host.HandleAsync(message))!);

        AssertError(response, code);
    }

    [Fact]
    public async Task Cancel_notification_cancels_the_request()
    {
        var pending = _test.Host.HandleAsync(TestHost.Request("shelf.wait", id: 5));
        Assert.False(pending.IsCompleted);

        var ack = await _test.Host.HandleAsync("{\"jsonrpc\":\"2.0\",\"method\":\"$/cancel\",\"params\":{\"id\":5}}");

        Assert.Null(ack);
        var response = TestHost.Parse((await pending)!);
        AssertError(response, -32800);
        Assert.Equal(5, (int)response["id"]!);
    }

    [Fact]
    public async Task Cancel_all_cancels_and_drops_responses_of_the_previous_page()
    {
        var oldPage = _test.Host.HandleAsync(TestHost.Request("shelf.wait", id: 1));

        _test.Host.CancelAll();
        var newPage = await _test.CallAsync("shelf.list", id: 1);

        Assert.Null(await oldPage);
        Assert.Equal("Apple", (string?)newPage["result"]![0]!["name"]);
        Assert.Contains(_test.Log, e => e.Level == BridgeLogLevel.Info && e.Message.Contains("cancelled 1"));
    }

    [Fact]
    public async Task A_second_request_with_an_id_in_flight_is_rejected()
    {
        var first = _test.Host.HandleAsync(TestHost.Request("shelf.wait", id: 9));

        var second = await _test.CallAsync("shelf.list", id: 9);

        AssertError(second, -32600);
        _test.Host.CancelAll();
        await first;
    }

    [Fact]
    public async Task Unknown_notifications_are_ignored_and_logged()
    {
        var response = await _test.Host.HandleAsync("{\"jsonrpc\":\"2.0\",\"method\":\"shelf.list\"}");

        Assert.Null(response);
        Assert.Contains(_test.Log, e => e.Level == BridgeLogLevel.Warning && e.Message.Contains("ignored notification"));
    }

    [Fact]
    public async Task Every_call_is_logged_with_its_duration()
    {
        await _test.CallAsync("shelf.labels", id: 4);

        var entry = Assert.Single(_test.Log);
        Assert.Equal(BridgeLogLevel.Debug, entry.Level);
        Assert.Matches(@"^shelf\.labels #4 ok \(\d+ ms\)$", entry.Message);
    }

    [Fact]
    public void Register_checks_the_service()
    {
        Assert.Throws<ArgumentException>(() => _test.Host.Register<IDisposable>(new System.IO.MemoryStream()));
        Assert.Throws<InvalidOperationException>(() => _test.Host.Register<IShelfService>(new FakeShelfService()));
    }

    [Fact]
    public void Exposes_the_contract_hash()
    {
        Assert.Equal(TestHost.Contract.Hash, _test.Host.ContractHash);
        Assert.Matches("^[0-9a-f]{16}$", _test.Host.ContractHash);
    }

    [Fact]
    public void Serves_the_nucleus_contract_by_default()
    {
        var host = new BridgeHost();

        Assert.Equal(Model.ContractModel.Default.Hash, host.ContractHash);
        Assert.Contains(host.Contract.Methods, m => m.WireName == "app.hello");
        Assert.Equal(1, BridgeHost.ProtocolVersion);
    }

    private static void AssertError(JObject response, int code)
    {
        Assert.Null(response["result"]);
        Assert.Equal(code, (int)response["error"]!["code"]!);
        Assert.False(string.IsNullOrEmpty((string?)response["error"]!["message"]));
    }
}
