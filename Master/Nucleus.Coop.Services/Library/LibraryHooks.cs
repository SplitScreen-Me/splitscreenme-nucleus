using System;
using System.Collections.Generic;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Integration points between the library services and the setup/profiles/session services of the same assembly.
/// Events are raised on the UI thread.
/// </summary>
public static class LibraryHooks
{
    private static LibraryNotifier? notifier;

    /// <summary>
    /// A game menu toggle changed how a game's setup works (gamepad assignment by index / button press, profiles on
    /// or off). When it is the game being set up, the setup should go back to its first step and reload the profile
    /// list, as the legacy menu did with <c>GoToStep(0)</c>. Argument: game id.
    /// </summary>
    public static event Action<string>? SetupInvalidated;

    /// <summary>
    /// Handlers were reloaded or replaced (install, update, reload): the <c>GenericGameInfo</c> and
    /// <c>UserGameInfo</c> objects of these games are new instances. An empty list means every game.
    /// </summary>
    public static event Action<IReadOnlyList<string>>? HandlersReplaced;

    /// <summary>
    /// The global "Disable game profiles" setting changed (legacy: <c>Core_Interface.DisableGameProfiles</c>, which
    /// refreshed the game page).
    /// </summary>
    public static event Action<bool>? GameProfilesSettingChanged;

    /// <summary>Tells the library that something of a game changed outside these services (for example a session ended).</summary>
    public static void NotifyGameChanged(string gameId) => notifier?.GamesChanged(gameId);

    internal static void Attach(LibraryNotifier libraryNotifier) => notifier = libraryNotifier;

    internal static void RaiseSetupInvalidated(string gameId) => SetupInvalidated?.Invoke(gameId);

    internal static void RaiseHandlersReplaced(IReadOnlyList<string> gameIds) => HandlersReplaced?.Invoke(gameIds);

    internal static void RaiseGameProfilesSettingChanged(bool disabled) => GameProfilesSettingChanged?.Invoke(disabled);
}
