using System;
using System.Collections.Generic;
using System.Threading;
using Nucleus.Bridge.Tests.TestContract;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class BridgeEventsTests
{
    [Fact]
    public void Emit_sends_a_notification_named_after_the_event()
    {
        var test = new TestHost();

        test.Host.Events.Emit(new ShelfChanged { ItemIds = { "a" } });

        Assert.Equal("{\"jsonrpc\":\"2.0\",\"method\":\"shelf.changed\",\"params\":{\"itemIds\":[\"a\"]}}", Assert.Single(test.Outgoing));
        Assert.Contains(test.Log, e => e.Message == "event shelf.changed");
    }

    [Fact]
    public void Emit_posts_to_the_synchronization_context_with_the_state_at_emit_time()
    {
        var context = new QueueContext();
        var test = new TestHost(context);
        var payload = new ShelfChanged { ItemIds = { "a" } };

        test.Host.Events.Emit(payload);
        payload.ItemIds.Add("b");

        Assert.Empty(test.Outgoing);
        context.RunAll();
        Assert.Equal(new[] { "a" }, TestHost.Parse(Assert.Single(test.Outgoing))["params"]!["itemIds"]!.ToObject<string[]>());
    }

    [Fact]
    public void Emit_serializes_dates_as_iso_utc()
    {
        var test = new TestHost();

        test.Host.Events.Emit(new ShelfSeen { At = new DateTime(2026, 10, 11, 14, 30, 0, DateTimeKind.Utc) });

        Assert.Contains("\"at\":\"2026-10-11T14:30:00Z\"", Assert.Single(test.Outgoing));
    }

    [Fact]
    public void Emit_rejects_types_that_are_not_events()
    {
        var test = new TestHost();

        var error = Assert.Throws<ArgumentException>(() => test.Host.Events.Emit(new ShelfItem()));
        Assert.Contains("[BridgeEvent", error.Message);
    }

    /// <summary>A synchronization context that queues posted work until <see cref="RunAll"/>.</summary>
    private sealed class QueueContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object? State)> _queue = new();

        public override void Post(SendOrPostCallback d, object? state) => _queue.Enqueue((d, state));

        public void RunAll()
        {
            while (_queue.Count > 0)
            {
                var (callback, state) = _queue.Dequeue();
                callback(state);
            }
        }
    }
}
