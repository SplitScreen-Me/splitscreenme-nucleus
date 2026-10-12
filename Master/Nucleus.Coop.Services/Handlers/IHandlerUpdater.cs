using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge.Contracts;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// Handler update logic for the other services of this assembly (not part of the bridge contract), for example the
/// setup's "handler up to date" check before a launch. Implemented by <see cref="HandlersService"/>; methods are
/// called on the UI thread.
/// </summary>
public interface IHandlerUpdater
{
    /// <summary>Last known state: a newer version was found on the hub (and update checks are on for this game). No network.</summary>
    bool IsUpdateAvailable(string gameId);

    /// <summary>Hub version found by the last check, or null.</summary>
    int? LatestVersion(string gameId);

    /// <summary>
    /// Asks the hub now. True when a newer version exists, false when up to date, null when it can't be told (hub
    /// offline, handler not from the hub, update checks off for the game).
    /// </summary>
    Task<bool?> CheckAsync(string gameId, CancellationToken cancellationToken);

    /// <summary>Downloads and installs the hub's current version (same as <c>handlers.update</c>, with progress events).</summary>
    Task<InstallResult> UpdateAsync(string gameId, CancellationToken cancellationToken);
}
