using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Session
{
    /// <summary>
    /// The pure rules of <c>session.restartInstance</c> and of the restart state the session snapshot shows (Nucleus 2.5
    /// restarts crashed instances, <c>GlobalWindowMethods.RequestInstanceRestart</c> restarts one on request).
    /// </summary>
    internal static class InstanceRestart
    {
        /// <summary>
        /// The bridge error for a refusal reason of <c>GlobalWindowMethods.RequestInstanceRestart</c> (or of the service):
        /// <c>conflict</c> for <c>noSession</c>, <c>notReady</c>, <c>busy</c> and <c>notSupported</c>, <c>notFound</c> for
        /// <c>unknownPlayer</c>, <c>unavailable</c> for anything else. Messages in the language in use, <c>data.reason</c>
        /// for the UI's logic.
        /// </summary>
        public static BridgeException Refusal(string? reason)
        {
            switch (reason)
            {
                case "noSession":
                    return SetupSession.Conflict(reason, I18n.T("host.session.restart.noSession"));
                case "notReady":
                    return SetupSession.Conflict(reason, I18n.T("host.session.restart.notReady"));
                case "busy":
                    return SetupSession.Conflict(reason, I18n.T("host.session.restart.busy"));
                case "notSupported":
                    return SetupSession.Conflict(reason, I18n.T("host.session.restart.notSupported"));
                case "unknownPlayer":
                    return new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.session.restart.unknownPlayer"), new { reason });
                default:
                    return new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.session.restart.failed"), new { reason = reason ?? "unknown" });
            }
        }

        /// <summary>
        /// Why a copy can't be started again now (null: it can), in the order of <c>RequestInstanceRestart</c>: no session
        /// (<c>noSession</c>), windows not all set up yet (<c>GameProfile.Saved</c> false: <c>notReady</c>), a restart
        /// running (<c>busy</c>), a handler that can't (<c>ProcessChangesAtEnd</c>, or <c>CantRestartInstances</c> without
        /// <c>GameReLaunchByItself</c>: <c>notSupported</c>).
        /// </summary>
        public static string? RefusalNow(bool sessionLive, bool setupFinished, bool restartRunning, bool processChangesAtEnd, bool cantRestartInstances, bool reLaunchByItself)
        {
            if (!sessionLive)
            {
                return "noSession";
            }

            if (!setupFinished)
            {
                return "notReady";
            }

            if (restartRunning)
            {
                return "busy";
            }

            return processChangesAtEnd || (cantRestartInstances && !reLaunchByItself) ? "notSupported" : null;
        }

        /// <summary>The stage shown for a copy: <see cref="InstanceStage.Restarting"/> while Nucleus starts it again, else the stage its process shows.</summary>
        public static InstanceStage Stage(InstanceStage observed, bool restarting) => restarting ? InstanceStage.Restarting : observed;

        /// <summary>Whether a copy reads as crashed: its process exited with an error, and Nucleus is not starting it again.</summary>
        public static bool Crashed(InstanceStage stage, bool exitedWithError) => stage == InstanceStage.Exited && exitedWithError;
    }
}
