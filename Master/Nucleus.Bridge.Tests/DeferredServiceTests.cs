using System;
using System.Threading.Tasks;
using Nucleus.Bridge.Tests.TestContract;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class DeferredServiceTests
{
    private sealed class FixedClock : IClockService
    {
        public DateTime Now() => new DateTime(2026, 10, 12, 18, 0, 0, DateTimeKind.Utc);
    }

    [Fact]
    public async Task A_call_waits_for_the_deferred_service_then_runs()
    {
        var test = new TestHost();
        var ready = new TaskCompletionSource<IClockService>();
        test.Host.RegisterDeferred(ready.Task);

        Task<Newtonsoft.Json.Linq.JObject> call = test.CallAsync("clock.now");
        await Task.Delay(50);
        Assert.False(call.IsCompleted);

        ready.SetResult(new FixedClock());
        var response = await call;

        Assert.StartsWith("2026-10-12T18:00:00", (string?)response["result"]);
    }

    [Fact]
    public async Task Calls_after_the_service_started_run_directly()
    {
        var test = new TestHost();
        test.Host.RegisterDeferred(Task.FromResult<IClockService>(new FixedClock()));

        var response = await test.CallAsync("clock.now");

        Assert.NotNull(response["result"]);
    }

    [Fact]
    public async Task A_failed_start_answers_unavailable()
    {
        var test = new TestHost();
        test.Host.RegisterDeferred(Task.FromException<IClockService>(new InvalidOperationException("library failed")));

        var response = await test.CallAsync("clock.now");

        Assert.Equal(-32001, (int)response["error"]!["code"]!);
        Assert.Contains("library failed", (string?)response["error"]!["message"]);
        Assert.Equal("serviceFailed", (string?)response["error"]!["data"]!["reason"]);
        Assert.Equal("clock", (string?)response["error"]!["data"]!["service"]);
    }

    [Fact]
    public void Registering_twice_is_refused()
    {
        var test = new TestHost();
        test.Host.RegisterDeferred(new TaskCompletionSource<IClockService>().Task);

        Assert.Throws<InvalidOperationException>(() => test.Host.Register<IClockService>(new FixedClock()));
        Assert.Throws<InvalidOperationException>(() => test.Host.RegisterDeferred(Task.FromResult<IClockService>(new FixedClock())));
    }
}
