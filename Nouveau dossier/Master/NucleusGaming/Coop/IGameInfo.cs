using System;

namespace Nucleus.Gaming.Coop
{
    public interface IGameInfo
    {
        /// <summary>
        /// If this game's implementation is unfinished (should not show up in a release build)
        /// </summary>
        bool Debug { get; }

        /// <summary>
        /// An array of types of IUserInputForm controls
        /// that will be used to get information from the user
        /// and start the game
        /// </summary>
        Type[] AdditionalSteps { get; }

        /// <summary>
        /// The game's executable name in lower case
        /// </summary>
        string ExecutableName { get; }

        /// <summary>
        /// This string must be on the game's executable path for it to be considered
        /// this game
        /// </summary>
        string[] ExecutableContext { get; }

        /// <summary>
        /// The game's name
        /// </summary>
        string GameName { get; }

        /// <summary>
        /// The class to instantiate to handle the game's
        /// initialization
        /// </summary>
        Type HandlerType { get; }

        /// <summary>
        /// If the user can change this game's windows positions
        /// </summary>
        bool SupportsPositioning { get; }

        /// <summary>
        /// The maximum number of players this game can handle
        /// </summary>
        int MaxPlayers { get; }

        /// <summary>
        /// The maximum number of players this game can handle in 1 monitor
        /// </summary>
        int MaxPlayersOneMonitor { get; }

        /// <summary>
        /// If the game supports keyboard and mouse gameplay
        /// </summary>
        bool SupportsKeyboard { get; }



        /// <summary>
        /// Custom options that the user can modify
        /// </summary>
        GameOption[] Options { get; }

        /// <summary>
        /// An unique GUID specific to the game
        /// </summary>
        string GUID { get; }

        bool SupportsXInput { get; }
        bool SupportsDirectInput { get; }
    }
}
