using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// Checks the hub for newer handler versions of library games (what <c>Hub.CheckUpdateAvailable</c> does per game),
/// a few requests at a time, and publishes the results with <c>library.changed</c> instead of the legacy polling of
/// <c>GenericGameInfo.UpdateAvailable</c> by every list row.
/// </summary>
public sealed class HandlerUpdateChecker
{
    private readonly HubClient hub;
    private readonly HubStatus status;
    private readonly UiDispatcher ui;
    private readonly LibraryNotifier notifier;

    // Hub version seen by the last check of each game (UI thread).
    private readonly Dictionary<string, int> latestVersions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates the checker.</summary>
    public HandlerUpdateChecker(HubClient hub, HubStatus status, UiDispatcher ui, LibraryNotifier notifier)
    {
        this.hub = hub;
        this.status = status;
        this.ui = ui;
        this.notifier = notifier;
    }

    /// <summary>A handler to check.</summary>
    public sealed class Target
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Hub handler id.</summary>
        public string HandlerId { get; set; } = "";

        /// <summary>Installed version.</summary>
        public int Version { get; set; }
    }

    /// <summary>Hub version found by the last check of a game, or null. Call on the UI thread.</summary>
    public int? LatestVersion(string gameId) => latestVersions.TryGetValue(gameId, out int version) ? version : (int?)null;

    /// <summary>Records the hub version after an install or update. Call on the UI thread.</summary>
    public void SetLatestVersion(string gameId, int version)
    {
        if (version >= 0)
        {
            latestVersions[gameId] = version;
        }
    }

    /// <summary>Library games with update checks on and a hub handler. Call on the UI thread.</summary>
    public static List<Target> LibraryTargets(IEnumerable<string>? onlyGameIds = null)
    {
        var filter = onlyGameIds == null ? null : new HashSet<string>(onlyGameIds, StringComparer.OrdinalIgnoreCase);
        var targets = new List<Target>();
        foreach (UserGameInfo entry in GameManager.Instance.User.Games.ToList())
        {
            GenericGameInfo game = entry.Game;
            if (game == null || (filter != null && !filter.Contains(game.GUID)))
            {
                continue;
            }

            if (game.MetaInfo.CheckUpdate && game.Hub.Handler.Version >= 0 && HubClient.IsValidHubId(game.Hub.Handler.Id))
            {
                targets.Add(new Target { GameId = game.GUID, HandlerId = game.Hub.Handler.Id, Version = game.Hub.Handler.Version });
            }
        }

        return targets;
    }

    /// <summary>
    /// Checks the targets and applies the results on the UI thread. Returns the number of games whose state changed.
    /// Throws <see cref="HubUnavailableException"/> when every request failed and <paramref name="throwWhenOffline"/>.
    /// </summary>
    public async Task<int> CheckAsync(IReadOnlyList<Target> targets, bool throwWhenOffline, CancellationToken cancellationToken)
    {
        if (targets.Count == 0)
        {
            return 0;
        }

        var results = new Dictionary<Target, int>();
        Exception? lastError = null;
        using var slots = new SemaphoreSlim(4, 4);

        IEnumerable<Task> requests = targets.Select(async target =>
        {
            await slots.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                HubHandlerInfo? info = await hub.GetHandlerAsync(target.HandlerId, cancellationToken).ConfigureAwait(false);
                lock (results)
                {
                    results[target] = info?.Version ?? -1;
                }
            }
            catch (HubUnavailableException e)
            {
                lastError = e;
            }
            finally
            {
                slots.Release();
            }
        });

        await Task.WhenAll(requests).ConfigureAwait(false);

        if (results.Count == 0 && lastError != null)
        {
            status.Report(false);
            if (throwWhenOffline)
            {
                throw lastError;
            }

            return 0;
        }

        status.Report(true);
        List<string> changed = await ui.InvokeAsync(() => Apply(results)).ConfigureAwait(false);
        if (changed.Count > 0)
        {
            ServiceLog.Info("handler update state of " + string.Join(", ", changed));
            notifier.GamesChanged(changed);
        }

        return changed.Count;
    }

    private List<string> Apply(Dictionary<Target, int> results)
    {
        var changed = new List<string>();
        foreach (KeyValuePair<Target, int> result in results)
        {
            // The handler may have been replaced (update, reload) while the request ran.
            if (!GameManager.Instance.Games.TryGetValue(result.Key.GameId, out GenericGameInfo game)
                || game.Hub.Handler.Version != result.Key.Version
                || !game.MetaInfo.CheckUpdate)
            {
                continue;
            }

            int? previousLatest = LatestVersion(game.GUID);
            if (result.Value >= 0)
            {
                latestVersions[game.GUID] = result.Value;
            }

            bool available = result.Value > result.Key.Version;
            if (game.UpdateAvailable != available || previousLatest != LatestVersion(game.GUID))
            {
                game.UpdateAvailable = available;
                changed.Add(game.GUID);
            }
        }

        return changed;
    }
}
