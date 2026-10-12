using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Session;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Session;

public class SessionSummaryTests
{
    private static SetupSnapshot Setup()
    {
        SetupSnapshot snapshot = new SetupSnapshot { GameId = "game", GameName = "Game" };
        snapshot.Devices.Add(new SetupDevice { Id = "xinput:0", Kind = DeviceKind.XInput, Name = "Xbox Controller" });
        snapshot.Devices.Add(new SetupDevice { Id = "xinput:1", Kind = DeviceKind.XInput, Name = "Second pad" });
        snapshot.Devices.Add(new SetupDevice { Id = "rawkb:a", Kind = DeviceKind.RawKeyboard, Name = "Keyboard" });
        snapshot.Devices.Add(new SetupDevice { Id = "rawmouse:a", Kind = DeviceKind.RawMouse, Name = "Mouse" });
        snapshot.Devices.Add(new SetupDevice { Id = "xinput:2", Kind = DeviceKind.XInput, Name = "Unplaced pad" });

        snapshot.Players.Add(new SetupPlayer { Id = "rawkb:a", Number = 2, ScreenIndex = 1, DeviceIds = { "rawkb:a", "rawmouse:a" }, Nickname = " " });
        snapshot.Players.Add(new SetupPlayer
        {
            Id = "xinput:0",
            Number = 1,
            Nickname = "Léa",
            DeviceIds = { "xinput:0" },
            GuestDeviceIds = { "xinput:1", "gone" },
        });

        return snapshot;
    }

    [Fact]
    public void Players_come_in_player_order_with_their_named_devices_and_guests_last()
    {
        List<SessionSummaryPlayer> players = SessionSummaryBuilder.Players(Setup());

        Assert.Equal(new[] { 1, 2 }, players.Select(p => p.Number));
        Assert.Equal("Léa", players[0].Nickname);
        Assert.Null(players[1].Nickname);
        Assert.Equal(1, players[1].ScreenIndex);

        Assert.Equal(new[] { ("Xbox Controller", false), ("Second pad", true) }, players[0].Devices.Select(d => (d.Name, d.Guest)));
        Assert.Equal(new[] { DeviceKind.RawKeyboard, DeviceKind.RawMouse }, players[1].Devices.Select(d => d.Kind));
    }

    [Fact]
    public void The_summary_keeps_the_error_lines_of_the_log()
    {
        DateTime start = new DateTime(2026, 10, 12, 20, 0, 0, DateTimeKind.Utc);
        List<SessionLogEntry> log = Enumerable.Range(0, SessionSummaryBuilder.MaxErrors + 5)
            .Select(i => new SessionLogEntry { Seq = i, Level = i % 2 == 0 ? SessionLogLevel.Error : SessionLogLevel.Info, Text = "line " + i })
            .ToList();

        SessionSummary summary = SessionSummaryBuilder.Build("game", "Game", start, start.AddMinutes(42), true, SessionSummaryBuilder.Players(Setup()), " ", log);

        Assert.Equal(TimeSpan.FromMinutes(42), summary.EndedAt - summary.StartedAt);
        Assert.True(summary.Launched);
        Assert.Null(summary.Error);
        Assert.Equal(2, summary.Players.Count);
        Assert.Equal(13, summary.ErrorCount);
        Assert.Equal(13, summary.Errors.Count);
        Assert.All(summary.Errors, e => Assert.Equal(SessionLogLevel.Error, e.Level));
        Assert.Equal("line 0", summary.Errors[0].Text);
    }

    [Fact]
    public void A_failed_launch_keeps_its_error_and_never_ends_before_it_started()
    {
        DateTime start = new DateTime(2026, 10, 12, 20, 0, 0, DateTimeKind.Utc);
        List<SessionLogEntry> log = Enumerable.Range(0, SessionSummaryBuilder.MaxErrors + 3)
            .Select(i => new SessionLogEntry { Seq = i, Level = SessionLogLevel.Error, Text = "error " + i })
            .ToList();

        SessionSummary summary = SessionSummaryBuilder.Build("game", "Game", start, start.AddSeconds(-1), false, new List<SessionSummaryPlayer>(), "The game exe was not found.", log);

        Assert.Equal(summary.StartedAt, summary.EndedAt);
        Assert.False(summary.Launched);
        Assert.Equal("The game exe was not found.", summary.Error);
        Assert.Equal(SessionSummaryBuilder.MaxErrors, summary.Errors.Count);
        Assert.Equal(SessionSummaryBuilder.MaxErrors + 3, summary.ErrorCount);
    }
}
