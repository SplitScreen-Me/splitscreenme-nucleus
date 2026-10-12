using System;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming.Forms;

namespace Nucleus.Coop.Services.Notices;

/// <summary>
/// The engine's prompt handler while the web UI runs (<see cref="UserPrompts.Handler"/>). Out-of-game messages go to the
/// web UI: handler and setup errors become notices, the crash report of a failed launch goes to the session screen,
/// and the input-lock reminder is already in "Before you launch". Everything else stays native: in-session prompts
/// (they must appear over the game), questions that need an answer, and any message while no page can show it
/// (<see cref="UiPageState.Unavailable"/>). Called on any thread.
/// </summary>
internal sealed class WebUserPrompts : IUserPromptHandler
{
    private readonly NoticeBoard board;
    private readonly Func<UiPageState> pageState;
    private readonly IUserPromptHandler native;

    public WebUserPrompts(NoticeBoard board, Func<UiPageState> pageState, IUserPromptHandler native)
    {
        this.board = board;
        this.pageState = pageState;
        this.native = native;
    }

    public PromptResult Ask(PromptRequest request)
    {
        if (request.Origin == PromptOrigin.LaunchFailure)
        {
            //Kept even when shown natively: the session snapshot carries it either way.
            board.SetLaunchFailure(request.Message);
        }

        if (!ShowsInApp(request, pageState()))
        {
            return native.Ask(request);
        }

        switch (request.Origin)
        {
            case PromptOrigin.LaunchReminder:
                ServiceLog.Info("session: input lock reminder shown in \"Before you launch\" instead of a message box");
                break;

            case PromptOrigin.LaunchFailure:
                ServiceLog.Info("session: launch crash report kept for the session screen");
                break;

            default:
                board.Add(Level(request.Icon), request.Origin == PromptOrigin.Handler ? NoticeSource.Handler : NoticeSource.Setup,
                    string.IsNullOrWhiteSpace(request.Title) ? DefaultTitle(request) : request.Title, request.Message ?? "");
                break;
        }

        return new PromptResult { Accepted = true };
    }

    /// <summary>Whether <paramref name="request"/> is shown by the web UI rather than a native window.</summary>
    internal static bool ShowsInApp(PromptRequest request, UiPageState state)
    {
        bool message = request.Kind == PromptKind.Message || request.Kind == PromptKind.Notice;
        return message && request.Origin != PromptOrigin.Session && state != UiPageState.Unavailable;
    }

    internal static NoticeLevel Level(PromptIcon icon)
    {
        switch (icon)
        {
            case PromptIcon.Error: return NoticeLevel.Error;
            case PromptIcon.Warning: return NoticeLevel.Warning;
            default: return NoticeLevel.Info;
        }
    }

    private static string DefaultTitle(PromptRequest request) => request.Origin == PromptOrigin.Handler ? "Handler error" : "Setup error";
}
