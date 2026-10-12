using System;
using System.Collections.Generic;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// Messages Nucleus has for the user outside a game session: handler errors (at startup, after an install or a
    /// reload), setup and profile errors, locked game files. They wait until the user dismisses them, so a page that
    /// loads later (startup, reload) still shows them. In-game prompts stay native windows over the game.
    /// </summary>
    [BridgeService("notices")]
    public interface INoticesService
    {
        /// <summary>Notices waiting to be read, oldest first.</summary>
        List<AppNotice> List();

        /// <summary>Removes a notice once the user has read it. Unknown ids are ignored.</summary>
        void Dismiss(DismissNoticeRequest request);
    }

    /// <summary>Severity of a notice.</summary>
    public enum NoticeLevel
    {
        /// <summary>Information.</summary>
        Info,
        /// <summary>Something may go wrong.</summary>
        Warning,
        /// <summary>Something failed.</summary>
        Error,
    }

    /// <summary>What a notice is about.</summary>
    public enum NoticeSource
    {
        /// <summary>Loading or reloading a handler.</summary>
        Handler,
        /// <summary>The setup of a game: profiles, content folder.</summary>
        Setup,
        /// <summary>Anything else.</summary>
        App,
    }

    /// <summary>A message for the user.</summary>
    public sealed class AppNotice
    {
        /// <summary>Id, unique within the app's lifetime.</summary>
        public string Id { get; set; } = "";

        /// <summary>Severity.</summary>
        public NoticeLevel Level { get; set; }

        /// <summary>What it is about.</summary>
        public NoticeSource Source { get; set; }

        /// <summary>Short title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Message, plain text; line breaks are meaningful.</summary>
        public string Message { get; set; } = "";

        /// <summary>When it was raised (UTC).</summary>
        public DateTime At { get; set; }
    }

    /// <summary>Parameters of <see cref="INoticesService.Dismiss"/>.</summary>
    public sealed class DismissNoticeRequest
    {
        /// <summary>Notice id.</summary>
        public string Id { get; set; } = "";
    }

    /// <summary>A notice was added or dismissed.</summary>
    [BridgeEvent("notices.changed", Invalidates = new[] { "notices.list" })]
    public sealed class NoticesChanged
    {
        /// <summary>Number of notices waiting.</summary>
        public int Count { get; set; }
    }
}
