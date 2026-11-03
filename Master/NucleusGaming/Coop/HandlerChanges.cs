using Nucleus.Gaming.Coop.Generic;
using System.Collections.Generic;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// Handler files found new, changed or deleted by <see cref="GameManager.ScanHandlerFiles"/>, already parsed. Built on
    /// any thread; <see cref="GameManager.ApplyHandlerScan"/> swaps it in.
    /// </summary>
    public sealed class HandlerScan
    {
        internal readonly List<HandlerInfo> parsed = new List<HandlerInfo>();
        internal readonly List<string> deleted = new List<string>();

        /// <summary>New or changed files, parsed (valid or not).</summary>
        public IReadOnlyList<HandlerInfo> Parsed => parsed;

        /// <summary>Known files (full paths) that are no longer on disk.</summary>
        public IReadOnlyList<string> Deleted => deleted;

        /// <summary>Every script was parsed again, changed or not (<see cref="GameManager.ScanHandlerFiles"/> with <c>all</c>).</summary>
        public bool Forced { get; internal set; }

        public bool IsEmpty => parsed.Count == 0 && deleted.Count == 0;
    }

    /// <summary>What <see cref="GameManager.ApplyHandlerScan"/> changed (<see cref="GameManager.HandlersChanged"/>).</summary>
    public sealed class HandlerChanges
    {
        internal readonly List<HandlerInfo> added = new List<HandlerInfo>();
        internal readonly List<HandlerInfo> updated = new List<HandlerInfo>();
        internal readonly List<HandlerInfo> fixedHandlers = new List<HandlerInfo>();
        internal readonly List<HandlerInfo> invalid = new List<HandlerInfo>();
        internal readonly List<HandlerInfo> removed = new List<HandlerInfo>();
        internal readonly List<string> gameIds = new List<string>();
        internal readonly List<UserGameInfo> rebound = new List<UserGameInfo>();
        internal readonly List<UserGameInfo> orphaned = new List<UserGameInfo>();

        /// <summary>New valid handler files (a handler dropped into the folder appears without a restart).</summary>
        public IReadOnlyList<HandlerInfo> Added => added;

        /// <summary>Valid handler files that changed and were parsed again.</summary>
        public IReadOnlyList<HandlerInfo> Updated => updated;

        /// <summary>Files that were invalid (startup or a previous edit) and parse again: their game comes back.</summary>
        public IReadOnlyList<HandlerInfo> Fixed => fixedHandlers;

        /// <summary>
        /// New or changed files that can't be used (<see cref="HandlerInfo.Error"/>). A handler that was valid keeps its
        /// previous version until the file is fixed. Each error is reported once (not again until the file changes into
        /// another error or is fixed).
        /// </summary>
        public IReadOnlyList<HandlerInfo> Invalid => invalid;

        /// <summary>Deleted handler files.</summary>
        public IReadOnlyList<HandlerInfo> Removed => removed;

        /// <summary>GUIDs whose handler in <see cref="GameManager.Games"/> changed, appeared or disappeared.</summary>
        public IReadOnlyList<string> GameIds => gameIds;

        /// <summary>Library entries bound to a new parse of their handler (<see cref="UserGameInfo.Rebind"/>).</summary>
        public IReadOnlyList<UserGameInfo> Rebound => rebound;

        /// <summary>
        /// Library entries whose handler file was deleted and no other file declares their GUID. They are still in
        /// <see cref="UserProfile.Games"/>: the host removes them (artwork, profile file) as for a removed game.
        /// </summary>
        public IReadOnlyList<UserGameInfo> Orphaned => orphaned;

        public bool Any => added.Count > 0 || updated.Count > 0 || fixedHandlers.Count > 0 || invalid.Count > 0 || removed.Count > 0;
    }
}
