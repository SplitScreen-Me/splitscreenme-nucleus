using Newtonsoft.Json;
using System.Drawing;

namespace Nucleus.Gaming.Coop
{
    public class UserGameInfo
    {
        private GenericGameInfo game;
        private string exePath;
        private string gameGuid = "";
        private string fileFullPath;

        [JsonIgnore]
        public GenericGameInfo Game
        {
            get
            {
                if (game == null && gameGuid != null)
                {
                    GameManager.Instance?.Games.TryGetValue(gameGuid, out game);
                }
                return game;
            }
        }

        [JsonIgnore]
        public Bitmap Icon
        {
            get;
            set;
        }

        public string GameGuid
        {
            get => gameGuid;
            set => gameGuid = value;
        }

        public string ExePath
        {
            get => exePath;
            set => exePath = value;
        }

        /// <summary>
        /// Full path of the handler script this game was added with (userprofile.json, same key as Nucleus 2.5). Lets the
        /// library keep a game whose script fails to load and bind it again once the script is fixed. Null for games
        /// added by older versions (they are matched by <see cref="GameGuid"/>).
        /// </summary>
        public string FileFullPath
        {
            get => fileFullPath;
            set => fileFullPath = value;
        }

        public UserGameInfo()
        {

        }

        public void InitializeDefault(GenericGameInfo game, string exePath)
        {
            this.game = game;
            gameGuid = game.GUID;
            this.exePath = exePath.Replace("I", "i");
            fileFullPath = game.FileFullPath ?? fileFullPath;
        }

        /// <summary>
        /// Binds this entry to a freshly parsed handler (hot reload) without touching the executable path. Null unbinds
        /// it: <see cref="Game"/> then looks the GUID up again in <see cref="GameManager.Games"/>.
        /// </summary>
        public void Rebind(GenericGameInfo game)
        {
            this.game = game;

            if (game != null)
            {
                gameGuid = game.GUID;
                fileFullPath = game.FileFullPath ?? fileFullPath;
            }
        }
    }
}
