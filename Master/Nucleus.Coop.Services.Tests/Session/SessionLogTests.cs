using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Session;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Session;

public class SessionLogTests
{
    private sealed class FakeEvents : IBridgeEvents
    {
        public List<object> Emitted { get; } = new List<object>();

        public void Emit<TEvent>(TEvent payload) where TEvent : class
        {
            lock (Emitted)
            {
                Emitted.Add(payload);
            }
        }
    }

    [Theory]
    [InlineData("Exception while copying files", SessionLogLevel.Error)]
    [InlineData("Could not find window, retrying", SessionLogLevel.Warning)]
    [InlineData("Instance 2 launched", SessionLogLevel.Success)]
    [InlineData("Setting window position", SessionLogLevel.Info)]
    public void Levels_are_guessed_from_the_words(string text, SessionLogLevel level)
    {
        Assert.Equal(level, SessionLog.Parse(text).Level);
    }

    [Theory]
    [InlineData("Player 3 Nickname set", 3)]
    [InlineData("Starting instance #12", 12)]
    [InlineData("Global setup", null)]
    public void Player_numbers_are_found(string text, int? player)
    {
        Assert.Equal(player, SessionLog.Parse(text).PlayerNumber);
    }

    [Fact]
    public void The_buffer_keeps_the_last_lines_with_increasing_sequence_numbers()
    {
        SessionLog log = new SessionLog(new FakeEvents());

        for (int i = 0; i < SessionLog.Capacity + 100; i++)
        {
            log.Append("line " + i);
        }

        List<SessionLogEntry> entries = log.Entries();

        Assert.Equal(SessionLog.Capacity, entries.Count);
        Assert.Equal("line 100", entries[0].Text);
        Assert.True(entries.Zip(entries.Skip(1), (a, b) => b.Seq == a.Seq + 1).All(x => x));
    }

    [Fact]
    public void New_lines_are_pushed_in_batches()
    {
        FakeEvents events = new FakeEvents();
        SessionLog log = new SessionLog(events);

        log.Append("one");
        log.Append("two");
        log.Append("   ");
        Thread.Sleep(600);

        SessionLogAppended batch = Assert.IsType<SessionLogAppended>(Assert.Single(events.Emitted));
        Assert.Equal(new[] { "one", "two" }, batch.Entries.Select(e => e.Text));
    }
}
