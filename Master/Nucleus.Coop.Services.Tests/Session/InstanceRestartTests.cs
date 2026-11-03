using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Session;
using Nucleus.Gaming.Localization;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Session;

/// <summary>
/// <c>session.restartInstance</c> (Nucleus 2.5 restarts crashed copies; <c>GlobalWindowMethods.RequestInstanceRestart</c>
/// restarts one on request): the refusal reasons of the engine as bridge errors, the restart state of the snapshot.
/// </summary>
public class InstanceRestartTests
{
    [Theory]
    [InlineData("noSession", BridgeErrorKind.Conflict, "host.session.restart.noSession")]
    [InlineData("notReady", BridgeErrorKind.Conflict, "host.session.restart.notReady")]
    [InlineData("busy", BridgeErrorKind.Conflict, "host.session.restart.busy")]
    [InlineData("notSupported", BridgeErrorKind.Conflict, "host.session.restart.notSupported")]
    [InlineData("unknownPlayer", BridgeErrorKind.NotFound, "host.session.restart.unknownPlayer")]
    public void Engine_refusals_become_bridge_errors_with_their_reason(string reason, BridgeErrorKind kind, string key)
    {
        BridgeException error = InstanceRestart.Refusal(reason);

        Assert.Equal(kind, error.Kind);
        Assert.Equal(reason, (string)error.ErrorData!.GetType().GetProperty("reason")!.GetValue(error.ErrorData)!);
        Assert.Equal(I18n.T(key), error.Message);
        Assert.NotEqual(key, error.Message);//the key has an English text
    }

    [Theory]
    [InlineData(null)]
    [InlineData("somethingNew")]
    public void An_unknown_refusal_is_unavailable_and_keeps_its_reason(string? reason)
    {
        BridgeException error = InstanceRestart.Refusal(reason);

        Assert.Equal(BridgeErrorKind.Unavailable, error.Kind);
        Assert.Equal(reason ?? "unknown", (string)error.ErrorData!.GetType().GetProperty("reason")!.GetValue(error.ErrorData)!);
        Assert.Equal(I18n.T("host.session.restart.failed"), error.Message);
    }

    [Theory]
    //live, setup finished, restart running, ProcessChangesAtEnd, CantRestartInstances, GameReLaunchByItself
    [InlineData(false, false, false, false, false, false, "noSession")]
    [InlineData(true, false, true, false, false, false, "notReady")]//RestartThreadIsRunning is also true while the copies are set up
    [InlineData(true, true, true, false, false, false, "busy")]
    [InlineData(true, true, false, true, false, false, "notSupported")]
    [InlineData(true, true, false, false, true, false, "notSupported")]
    [InlineData(true, true, false, false, true, true, null)]//re-hooked by itself (Battlefield 1942)
    [InlineData(true, true, false, false, false, false, null)]
    public void The_snapshot_says_why_a_copy_cant_be_restarted_now(bool live, bool finished, bool restarting, bool changesAtEnd, bool cantRestart, bool reLaunch, string? expected)
    {
        Assert.Equal(expected, InstanceRestart.RefusalNow(live, finished, restarting, changesAtEnd, cantRestart, reLaunch));
    }

    [Fact]
    public void A_copy_being_started_again_reads_as_restarting_not_crashed()
    {
        Assert.Equal(InstanceStage.Restarting, InstanceRestart.Stage(InstanceStage.Exited, restarting: true));
        Assert.Equal(InstanceStage.Exited, InstanceRestart.Stage(InstanceStage.Exited, restarting: false));
        Assert.Equal(InstanceStage.Ready, InstanceRestart.Stage(InstanceStage.Ready, restarting: false));

        Assert.True(InstanceRestart.Crashed(InstanceStage.Exited, exitedWithError: true));
        Assert.False(InstanceRestart.Crashed(InstanceStage.Exited, exitedWithError: false), "closed normally (exit code 0 or unknown)");
        Assert.False(InstanceRestart.Crashed(InstanceStage.Restarting, exitedWithError: true));
    }

    [Fact]
    public void The_engine_refuses_without_a_session_and_reports_no_restart()
    {
        Assert.False(GlobalWindowMethods.RequestInstanceRestart(0, out string reason));
        Assert.Equal("noSession", reason);
        Assert.Equal(-1, GlobalWindowMethods.RestartingPlayerId);
        Assert.False(GlobalWindowMethods.RestartGaveUp(0));
    }

    [Fact]
    public void Only_an_error_exit_code_counts_as_a_crash()
    {
        //The automatic restart rule the snapshot mirrors: a crash restarts, a normal close counts as closed.
        Assert.Equal(GlobalWindowMethods.InstanceQuitAction.Restart, GlobalWindowMethods.DecideQuitAction(false, false, -1073741819, 0));
        Assert.Equal(GlobalWindowMethods.InstanceQuitAction.CountAsExited, GlobalWindowMethods.DecideQuitAction(false, false, 0, 0));
        Assert.Equal(GlobalWindowMethods.InstanceQuitAction.CountAsExited, GlobalWindowMethods.DecideQuitAction(false, false, 1, GlobalWindowMethods.MaxAutoRestarts));
    }
}
