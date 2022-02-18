using System.Collections.Generic;

namespace Nucleus.Gaming.Coop
{
    public class UserProfile
    {
        private List<UserGameInfo> games;

        public List<UserGameInfo> Games
        {
            get => games;
            set => games = value;
        }

        public UserProfile()
        {
        }

        public void InitializeDefault()
        {
            games = new List<UserGameInfo>();
        }
    }
}
