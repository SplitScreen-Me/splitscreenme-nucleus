using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.Search;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// <c>library.searchGames</c> without GameManager: runs a <see cref="GameAutoSearch"/> over the game folders, sends its
/// progress as <see cref="FolderSearchProgress"/> events (a few per second at most) and maps the candidates to the
/// contract. Callers pass snapshots of the handlers and the library taken on the UI thread.
/// </summary>
internal static class FolderSearch
{
    /// <summary>Longest search id accepted (the UI makes short ones).</summary>
    public const int MaxSearchIdLength = 64;

    /// <summary>
    /// Every handler of <paramref name="handlers"/>, or only <paramref name="single"/> stopping at its first executable.
    /// Throws <see cref="OperationCanceledException"/> when cancelled.
    /// </summary>
    public static async Task<FolderSearchResult> RunAsync(GameAutoSearch search, IBridgeEvents events, string searchId,
        IReadOnlyCollection<GenericGameInfo> handlers, IReadOnlyCollection<(string GameId, string ExePath)> library,
        GenericGameInfo? single, CancellationToken cancellationToken)
    {
        int folders = search.ExistingFolders.Count;
        var progress = new ThrottledProgress(events, searchId);
        List<AutoSearchCandidate> found;

        if (single != null)
        {
            AutoSearchCandidate? first = await search.FindAsync(single, progress, cancellationToken).ConfigureAwait(false);
            found = first == null ? new List<AutoSearchCandidate>() : new List<AutoSearchCandidate> { first };
            foreach (AutoSearchCandidate candidate in found)
            {
                candidate.InLibrary = library.Any(l => string.Equals(l.ExePath, candidate.ExecutablePath, StringComparison.OrdinalIgnoreCase)
                                                    || string.Equals(l.GameId, candidate.GameId, StringComparison.OrdinalIgnoreCase));
            }
        }
        else
        {
            found = await search.SearchAsync(handlers, library, progress, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        progress.Finish(found.Count);

        return new FolderSearchResult
        {
            Candidates = found.Select(ToDto).ToList(),
            FoldersSearched = folders,
        };
    }

    /// <summary>A candidate as the contract describes it.</summary>
    public static FolderSearchCandidate ToDto(AutoSearchCandidate c) => new FolderSearchCandidate
    {
        GameId = c.GameId,
        GameName = c.GameName,
        HandlerFile = c.HandlerFile,
        ExecutablePath = c.ExecutablePath,
        InstallFolder = c.InstallFolder,
        InLibrary = c.InLibrary,
    };

    /// <summary>
    /// Search progress as events: the engine reports every game folder, which can be hundreds per second; the UI gets a
    /// report when the percentage moves on and at most every <see cref="IntervalMs"/> otherwise, then one at the end.
    /// </summary>
    internal sealed class ThrottledProgress : IProgress<GameSearchProgress>
    {
        public const int IntervalMs = 150;

        private readonly IBridgeEvents events;
        private readonly string searchId;
        private readonly Func<DateTime> clock;
        private readonly object gate = new object();
        private DateTime last = DateTime.MinValue;
        private int lastPercent = -1;
        private bool finished;

        public ThrottledProgress(IBridgeEvents events, string searchId, Func<DateTime>? clock = null)
        {
            this.events = events;
            this.searchId = searchId;
            this.clock = clock ?? (() => DateTime.UtcNow);
        }

        public void Report(GameSearchProgress value)
        {
            if (value == null)
            {
                return;
            }

            lock (gate)
            {
                DateTime now = clock();
                int percent = Math.Max(0, Math.Min(99, value.Percent));//100 comes with Finish, with the final count
                if (finished || (percent == lastPercent && (now - last).TotalMilliseconds < IntervalMs))
                {
                    return;
                }

                last = now;
                lastPercent = percent;
                events.Emit(new FolderSearchProgress { SearchId = searchId, Percent = percent, CurrentFolder = value.CurrentFolder, Found = value.Found });
            }
        }

        /// <summary>The last report: 100 %, no folder, the number of candidates.</summary>
        public void Finish(int found)
        {
            lock (gate)
            {
                if (finished)
                {
                    return;
                }

                finished = true;
                events.Emit(new FolderSearchProgress { SearchId = searchId, Percent = 100, Found = found });
            }
        }
    }
}
