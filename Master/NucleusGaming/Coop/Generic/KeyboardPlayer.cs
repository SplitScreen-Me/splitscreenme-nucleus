namespace Nucleus.Gaming.Coop
{
    public class KeyboardPlayer : GameOptionValue
    {
        public KeyboardPlayer(string nam, int val)
            : base(nam, val)
        {
        }

        private static KeyboardPlayer noKeyboardPlayer = new KeyboardPlayer("No Keyboard Player", -1);
        private static KeyboardPlayer player1 = new KeyboardPlayer("Player 1", 0);
        private static KeyboardPlayer player2 = new KeyboardPlayer("Player 2", 1);
        private static KeyboardPlayer player3 = new KeyboardPlayer("Player 3", 2);
        private static KeyboardPlayer player4 = new KeyboardPlayer("Player 4", 3);

        public static KeyboardPlayer NoKeyboardPlayer => noKeyboardPlayer;
        public static KeyboardPlayer Player1 => player1;
        public static KeyboardPlayer Player2 => player2;
        public static KeyboardPlayer Player3 => player3;
        public static KeyboardPlayer Player4 => player4;
    }
}