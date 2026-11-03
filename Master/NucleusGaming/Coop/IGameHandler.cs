using System;
using System.Threading;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// Implements a game's splitscreen handler
    /// </summary>
    public interface IGameHandler
    {
        /// <summary>
        /// The update interval to call Update(). Set to 0 or -1
        /// to disable 
        /// </summary>
        double TimerInterval { get; }

        int TotalPlayers { get; }

        bool Initialize(UserGameInfo game, GameProfile profile, IGameHandler handler);

        string Play();

        void Update(double delayMS, bool refresh);

        void End(bool fromStopButton);

        bool HasEnded { get; }
        event Action Ended;
        event Action InitializeEnd;

        Thread FakeFocus { get; }
    }
}
