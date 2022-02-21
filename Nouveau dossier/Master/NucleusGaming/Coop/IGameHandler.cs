using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
//using System.Windows.Forms;

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

        ///bool Initialize(string gameFilename, List<PlayerInfo> players, Dictionary<string, GameOption> options, List<Control> addSteps, int titleHeight);

        bool Initialize(UserGameInfo game, GameProfile profile);


        string Play();
        void Update(double delayMS);

        void End(bool fromStopButton);

        bool HasEnded { get; }
        event Action Ended;

        Thread FakeFocus { get; }
    }
}
