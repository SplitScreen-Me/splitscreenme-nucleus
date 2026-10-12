using System;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class WebView2TransportTests
{
    [Theory]
    [InlineData("https://ui.nucleus.example/index.html#/library", true)]
    [InlineData("https://UI.nucleus.example/", true)]
    [InlineData("https://ui.nucleus.example:443/", true)]
    [InlineData("http://ui.nucleus.example/", false)]
    [InlineData("https://ui.nucleus.example.evil.com/", false)]
    [InlineData("https://assets.nucleus.example/", false)]
    [InlineData("about:blank", false)]
    [InlineData(null, false)]
    public void Accepts_only_the_ui_origin(string? source, bool expected)
    {
        Assert.Equal(expected, Origin.Matches(source, Origin.Of("https://ui.nucleus.example")!));
    }

    [Fact]
    public void Dev_server_origins_include_the_port()
    {
        var origin = Origin.Of("http://localhost:5173/")!;

        Assert.True(Origin.Matches("http://localhost:5173/src/main.tsx", origin));
        Assert.False(Origin.Matches("http://localhost:5174/", origin));
    }

    [Fact]
    public void Refuses_a_host_without_synchronization_context()
    {
        var error = Assert.Throws<ArgumentException>(() => new WebView2Transport(null!, new BridgeHost(contract: TestHost.Contract), "https://ui.nucleus.example"));

        Assert.Contains("SynchronizationContext", error.Message);
    }

    [Fact]
    public void Bridge_json_settings_are_a_copy()
    {
        BridgeJson.Settings.Converters.Clear();

        Assert.NotEmpty(BridgeJson.Settings.Converters);
    }
}
