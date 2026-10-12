using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Notices;
using Nucleus.Coop.Services.Settings;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Forms;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Notices;

/// <summary>
/// The engine prompt seam (<see cref="UserPrompts"/>) and its web UI handler (<see cref="WebUserPrompts"/>): out-of-game
/// messages become notices or session details, everything else stays a native window.
/// </summary>
[Collection(UserPromptsCollection.Name)]
public class PromptRoutingTests
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

    /// <summary>Records what would have been a native window.</summary>
    private sealed class FakeNative : IUserPromptHandler
    {
        public List<PromptRequest> Shown { get; } = new List<PromptRequest>();

        public PromptResult Answer { get; set; } = new PromptResult { Accepted = true };

        public PromptResult Ask(PromptRequest request)
        {
            Shown.Add(request);
            return Answer;
        }
    }

    private readonly FakeEvents events = new FakeEvents();
    private readonly FakeNative native = new FakeNative();
    private readonly NoticeBoard board;
    private UiPageState page = UiPageState.Ready;

    public PromptRoutingTests()
    {
        board = new NoticeBoard(events);
    }

    private WebUserPrompts Web() => new WebUserPrompts(board, () => page, native);

    private static PromptRequest Notice(PromptOrigin origin, PromptIcon icon = PromptIcon.Error, string title = "Error in handler") =>
        new PromptRequest { Kind = PromptKind.Notice, Origin = origin, Icon = icon, Title = title, Message = "There is an error in the game handler Broken.js.\n\nError at line 3" };

    [Theory]
    [InlineData(PromptOrigin.Handler, NoticeSource.Handler)]
    [InlineData(PromptOrigin.Setup, NoticeSource.Setup)]
    public void Out_of_game_messages_become_notices_while_a_page_is_ready_or_loading(PromptOrigin origin, NoticeSource source)
    {
        foreach (UiPageState state in new[] { UiPageState.Ready, UiPageState.Loading })
        {
            page = state;
            PromptResult result = Web().Ask(Notice(origin, title: "Title " + state));
            Assert.True(result.Accepted);
        }

        Assert.Empty(native.Shown);
        List<AppNotice> notices = board.List();
        Assert.Equal(2, notices.Count);
        Assert.All(notices, n => Assert.Equal(source, n.Source));
        Assert.All(notices, n => Assert.Equal(NoticeLevel.Error, n.Level));
        Assert.StartsWith("There is an error", notices[0].Message);
        Assert.Equal(2, events.Emitted.OfType<NoticesChanged>().Count());
    }

    [Fact]
    public void Messages_go_native_when_no_page_can_show_them()
    {
        page = UiPageState.Unavailable;

        Web().Ask(Notice(PromptOrigin.Handler));
        Web().Ask(new PromptRequest { Kind = PromptKind.Message, Origin = PromptOrigin.LaunchReminder, Message = "Press End" });

        Assert.Equal(2, native.Shown.Count);
        Assert.Empty(board.List());
    }

    [Theory]
    [InlineData(PromptKind.Message)]
    [InlineData(PromptKind.Notice)]
    [InlineData(PromptKind.WaitForOk)]
    [InlineData(PromptKind.YesNo)]
    [InlineData(PromptKind.TextInput)]
    [InlineData(PromptKind.PickProcess)]
    [InlineData(PromptKind.PickLauncher)]
    public void Session_prompts_stay_native(PromptKind kind)
    {
        native.Answer = new PromptResult { Accepted = true, Text = "typed" };

        PromptResult result = Web().Ask(new PromptRequest { Kind = kind, Origin = PromptOrigin.Session, Message = "Press OK when ready." });

        Assert.Same(native.Answer, result);
        Assert.Single(native.Shown);
        Assert.Empty(board.List());
    }

    [Fact]
    public void Questions_stay_native_even_out_of_game()
    {
        native.Answer = new PromptResult { Accepted = false };

        PromptResult result = Web().Ask(new PromptRequest { Kind = PromptKind.YesNo, Origin = PromptOrigin.Setup, Message = "Delete?" });

        Assert.False(result.Accepted);
        Assert.Single(native.Shown);
    }

    [Fact]
    public void The_input_lock_reminder_is_skipped_while_the_page_shows_it_before_launch()
    {
        PromptResult result = Web().Ask(new PromptRequest { Kind = PromptKind.Message, Origin = PromptOrigin.LaunchReminder, DesktopOnly = true, Message = "Press End" });

        Assert.True(result.Accepted);
        Assert.Empty(native.Shown);
        Assert.Empty(board.List());
    }

    [Fact]
    public void A_launch_crash_report_is_kept_for_the_session_and_shown_once()
    {
        Web().Ask(new PromptRequest { Kind = PromptKind.Notice, Origin = PromptOrigin.LaunchFailure, Title = "Something went wrong :(", Message = "[Type]\n\nIOException" });

        Assert.Empty(native.Shown);
        Assert.Empty(board.List());
        Assert.Equal("[Type]\n\nIOException", board.TakeLaunchFailure());
        Assert.Null(board.TakeLaunchFailure());
    }

    [Fact]
    public void A_launch_crash_report_is_kept_even_when_shown_natively()
    {
        page = UiPageState.Unavailable;

        Web().Ask(new PromptRequest { Kind = PromptKind.Notice, Origin = PromptOrigin.LaunchFailure, Message = "report" });

        Assert.Single(native.Shown);
        Assert.Equal("report", board.TakeLaunchFailure());
    }

    [Theory]
    [InlineData(PromptIcon.Error, NoticeLevel.Error)]
    [InlineData(PromptIcon.Warning, NoticeLevel.Warning)]
    [InlineData(PromptIcon.Info, NoticeLevel.Info)]
    [InlineData(PromptIcon.None, NoticeLevel.Info)]
    [InlineData(PromptIcon.Question, NoticeLevel.Info)]
    public void Icons_map_to_notice_levels(PromptIcon icon, NoticeLevel level)
    {
        Assert.Equal(level, WebUserPrompts.Level(icon));
    }

    [Fact]
    public void Untitled_messages_get_a_title()
    {
        Web().Ask(Notice(PromptOrigin.Setup, title: " "));

        Assert.Equal("Setup error", board.List().Single().Title);
    }

    [Fact]
    public void The_same_notice_is_not_added_twice_and_dismissing_removes_it()
    {
        Web().Ask(Notice(PromptOrigin.Handler));
        Web().Ask(Notice(PromptOrigin.Handler));
        AppNotice notice = board.List().Single();

        board.Dismiss(new DismissNoticeRequest { Id = notice.Id });
        board.Dismiss(new DismissNoticeRequest { Id = "unknown" });

        Assert.Empty(board.List());
        Assert.Equal(new[] { 1, 0 }, events.Emitted.OfType<NoticesChanged>().Select(e => e.Count));
    }

    [Fact]
    public void Only_the_newest_notices_are_kept()
    {
        for (int i = 0; i < NoticeBoard.MaxNotices + 5; i++)
        {
            board.Add(NoticeLevel.Warning, NoticeSource.App, "Notice " + i, "text");
        }

        List<AppNotice> notices = board.List();
        Assert.Equal(NoticeBoard.MaxNotices, notices.Count);
        Assert.Equal("Notice 5", notices[0].Title);
    }

    [Fact]
    public void The_facade_sends_each_call_to_the_handler()
    {
        native.Answer = new PromptResult { Accepted = true, Text = "answer", Path = @"C:\\Games\\launcher.exe", Choice = "42 - game" };
        UserPrompts.Handler = native;

        try
        {
            UserPrompts.Report(PromptIcon.Warning, "Title", "Message", PromptOrigin.Handler);
            UserPrompts.Notice("Risk of crash!", "Locked", false, PromptOrigin.Setup, PromptIcon.Warning);
            Assert.True(UserPrompts.YesNo("Delete", "Sure?", PromptIcon.Question));
            UserPrompts.WaitForOk("Press OK when ready.");
            Assert.Equal(@"C:\\Games\\launcher.exe", UserPrompts.PickLauncher("Pick it", "launcher.exe"));
            Assert.Equal("answer", UserPrompts.AskText("Name?", "previous").Text);
            Assert.Equal("42 - game", UserPrompts.PickProcess(() => new[] { "42 - game" }));
        }
        finally
        {
            UserPrompts.Handler = null;
        }

        Assert.Equal(new[] { PromptKind.Message, PromptKind.Notice, PromptKind.YesNo, PromptKind.WaitForOk, PromptKind.PickLauncher, PromptKind.TextInput, PromptKind.PickProcess },
            native.Shown.Select(r => r.Kind));
        Assert.Equal(new[] { PromptOrigin.Handler, PromptOrigin.Setup, PromptOrigin.Session, PromptOrigin.Session, PromptOrigin.Session, PromptOrigin.Session, PromptOrigin.Session },
            native.Shown.Select(r => r.Origin));
        Assert.Equal(PromptIcon.Warning, native.Shown[1].Icon);
        Assert.Equal("launcher.exe", native.Shown[4].FileName);
        Assert.Equal("previous", native.Shown[5].DefaultText);
        Assert.Equal(new[] { "42 - game" }, native.Shown[6].Choices!());
        Assert.Same(UserPrompts.Native, UserPrompts.Handler);
    }

    [Fact]
    public void The_input_lock_check_offers_to_turn_the_reminder_off()
    {
        SetupCheck check = SetupChecks.InputLockReminder();

        Assert.Equal(SetupChecks.InputLock, check.Id);
        Assert.Equal(CheckState.Info, check.State);
        Assert.StartsWith("Press ", check.Title);
        Assert.Equal("Don't show again", check.ActionLabel);
    }

    [Fact]
    public void Turning_off_keep_accounts_asks_for_confirmation()
    {
        SettingDefinition keep = SettingsCatalog.Create().Single(d => d.Field.Key == "misc.keepAccounts");

        Assert.NotNull(keep.Field.Confirm);
        Assert.Equal(new[] { "false" }, keep.Field.Confirm!.Values);
        Assert.Equal("Turn off", keep.Field.Confirm.ConfirmLabel);
        Assert.Contains("deletes every account", keep.Field.Confirm.Message);
        Assert.All(SettingsCatalog.Create().Where(d => d.Field.Key != "misc.keepAccounts"), d => Assert.Null(d.Field.Confirm));
    }
}

/// <summary>Tests that change <see cref="UserPrompts.Handler"/> (static) never run in parallel.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class UserPromptsCollection
{
    public const string Name = "UserPrompts handler";
}
