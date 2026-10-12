using Nucleus.Gaming.Coop.Generic;
using System;
using System.Collections.Generic;

namespace Nucleus.Gaming.Forms
{
    /// <summary>What a prompt shows, and so which native window shows it and what it returns.</summary>
    public enum PromptKind
    {
        /// <summary>A message with an OK button (a Windows message box).</summary>
        Message,
        /// <summary>A message in the Nucleus message box (topmost, text optionally reflowed).</summary>
        Notice,
        /// <summary>A Yes/No question: <see cref="PromptResult.Accepted"/> is true for Yes.</summary>
        YesNo,
        /// <summary>"Press OK when…": a topmost window that waits for the user.</summary>
        WaitForOk,
        /// <summary>Picks the game launcher executable: <see cref="PromptResult.Path"/>.</summary>
        PickLauncher,
        /// <summary>Free text input: <see cref="PromptResult.Text"/>.</summary>
        TextInput,
        /// <summary>Picks a running process among <see cref="PromptRequest.Choices"/>: <see cref="PromptResult.Choice"/>.</summary>
        PickProcess,
    }

    /// <summary>Icon of a <see cref="PromptKind.Message"/> or <see cref="PromptKind.YesNo"/> prompt.</summary>
    public enum PromptIcon
    {
        /// <summary>No icon.</summary>
        None,
        /// <summary>Information.</summary>
        Info,
        /// <summary>Warning.</summary>
        Warning,
        /// <summary>Error.</summary>
        Error,
        /// <summary>Question.</summary>
        Question,
    }

    /// <summary>
    /// Where a prompt comes from. A host with its own UI may show the out-of-game ones there; <see cref="Session"/>
    /// prompts stay native windows, because they must appear over the game windows.
    /// </summary>
    public enum PromptOrigin
    {
        /// <summary>A game launch or session (the default): over the game windows.</summary>
        Session,
        /// <summary>Loading or parsing a handler (startup, install, reload).</summary>
        Handler,
        /// <summary>The setup of a game before a launch: profiles, content folder.</summary>
        Setup,
        /// <summary>The "lock inputs with End" reminder shown when a launch starts.</summary>
        LaunchReminder,
        /// <summary>The crash report of a launch that failed; the session ends right after it.</summary>
        LaunchFailure,
    }

    /// <summary>A prompt for the user. See <see cref="UserPrompts"/>.</summary>
    public sealed class PromptRequest
    {
        /// <summary>What the prompt shows.</summary>
        public PromptKind Kind { get; set; }

        /// <summary>Where it comes from.</summary>
        public PromptOrigin Origin { get; set; }

        /// <summary>Icon (<see cref="PromptKind.Message"/>, <see cref="PromptKind.YesNo"/>; for <see cref="PromptKind.Notice"/>, the severity only).</summary>
        public PromptIcon Icon { get; set; }

        /// <summary>Title (window caption).</summary>
        public string Title { get; set; } = "";

        /// <summary>Message.</summary>
        public string Message { get; set; } = "";

        /// <summary><see cref="PromptKind.Notice"/>: reflow the text (about ten words per line).</summary>
        public bool FormatMessage { get; set; }

        /// <summary><see cref="PromptKind.Message"/>: show the box on the default desktop, above every window.</summary>
        public bool DesktopOnly { get; set; }

        /// <summary><see cref="PromptKind.TextInput"/>: initial text (the previous answer).</summary>
        public string DefaultText { get; set; }

        /// <summary><see cref="PromptKind.PickLauncher"/>: expected file name, used as the file filter (null: any .exe).</summary>
        public string FileName { get; set; }

        /// <summary><see cref="PromptKind.PickProcess"/>: lists the choices, called again on refresh.</summary>
        public Func<IList<string>> Choices { get; set; }
    }

    /// <summary>The user's answer to a <see cref="PromptRequest"/>.</summary>
    public sealed class PromptResult
    {
        /// <summary>OK or Yes was clicked (for a message: it was shown or handled).</summary>
        public bool Accepted { get; set; }

        /// <summary><see cref="PromptKind.TextInput"/>: the text entered.</summary>
        public string Text { get; set; }

        /// <summary><see cref="PromptKind.PickLauncher"/>: the file picked (backslashes doubled, as the handler file expects), or null.</summary>
        public string Path { get; set; }

        /// <summary><see cref="PromptKind.PickProcess"/>: the choice picked, or null.</summary>
        public string Choice { get; set; }
    }

    /// <summary>Shows prompts. Called on any thread, including thread-pool and game threads.</summary>
    public interface IUserPromptHandler
    {
        /// <summary>Shows <paramref name="request"/> and returns the answer. Blocks until answered when an answer is expected.</summary>
        PromptResult Ask(PromptRequest request);
    }

    /// <summary>
    /// Every message box and prompt of the engine goes through here. The default handler shows today's native windows;
    /// a host replaces <see cref="Handler"/> to show some of them in its own UI (see <see cref="PromptOrigin"/>).
    /// If a replacement handler throws, the native window is shown instead.
    /// </summary>
    public static class UserPrompts
    {
        private static IUserPromptHandler handler;

        /// <summary>The native windows: message boxes, <see cref="Prompt"/>, <see cref="CustomPrompt"/>, <see cref="ProcessPicker"/>, <see cref="NucleusMessageBox"/>.</summary>
        public static IUserPromptHandler Native { get; } = new NativeUserPrompts();

        /// <summary>Current handler. Setting null restores <see cref="Native"/>.</summary>
        public static IUserPromptHandler Handler
        {
            get => handler ?? Native;
            set => handler = value;
        }

        /// <summary>Shows a prompt with the current handler.</summary>
        public static PromptResult Ask(PromptRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IUserPromptHandler current = Handler;

            if (!ReferenceEquals(current, Native))
            {
                try
                {
                    return current.Ask(request) ?? new PromptResult();
                }
                catch (Exception ex)
                {
                    LogManager.Log($"Prompt handler failed ({request.Kind}, {request.Origin}), showing the native window: {ex}");
                }
            }

            return Native.Ask(request) ?? new PromptResult();
        }

        /// <summary>A message with an OK button.</summary>
        public static void Report(PromptIcon icon, string title, string message, PromptOrigin origin = PromptOrigin.Session)
        {
            Ask(new PromptRequest { Kind = PromptKind.Message, Icon = icon, Title = title ?? "", Message = message ?? "", Origin = origin });
        }

        /// <summary>A message in the Nucleus message box. The native box shows no icon; <paramref name="icon"/> tells a host the severity.</summary>
        public static void Notice(string title, string message, bool formatMessage = false, PromptOrigin origin = PromptOrigin.Session, PromptIcon icon = PromptIcon.None)
        {
            Ask(new PromptRequest { Kind = PromptKind.Notice, Icon = icon, Title = title ?? "", Message = message ?? "", FormatMessage = formatMessage, Origin = origin });
        }

        /// <summary>A Yes/No question. True for Yes.</summary>
        public static bool YesNo(string title, string message, PromptIcon icon, PromptOrigin origin = PromptOrigin.Session)
        {
            return Ask(new PromptRequest { Kind = PromptKind.YesNo, Icon = icon, Title = title ?? "", Message = message ?? "", Origin = origin }).Accepted;
        }

        /// <summary>"Press OK when…": waits until the user clicks OK.</summary>
        public static void WaitForOk(string message)
        {
            Ask(new PromptRequest { Kind = PromptKind.WaitForOk, Message = message ?? "" });
        }

        /// <summary>Asks for the game launcher executable. Returns its path, or null.</summary>
        public static string PickLauncher(string message, string fileName)
        {
            return Ask(new PromptRequest { Kind = PromptKind.PickLauncher, Message = message ?? "", FileName = fileName }).Path;
        }

        /// <summary>Asks for a text. <see cref="PromptResult.Accepted"/> is false when the window was closed without OK.</summary>
        public static PromptResult AskText(string message, string defaultText)
        {
            return Ask(new PromptRequest { Kind = PromptKind.TextInput, Message = message ?? "", DefaultText = defaultText ?? "" });
        }

        /// <summary>Asks for a running process among <paramref name="choices"/>. Returns the choice, or null.</summary>
        public static string PickProcess(Func<IList<string>> choices)
        {
            return Ask(new PromptRequest { Kind = PromptKind.PickProcess, Choices = choices }).Choice;
        }
    }
}
