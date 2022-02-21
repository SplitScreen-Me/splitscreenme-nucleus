namespace Nucleus.Gaming.Coop
{
    public abstract class GameOptionValue
    {
        private string name;
        private int value;

        public string Name => name;

        public int Value => value;

        public GameOptionValue(string nam, int val)
        {
            name = nam;
            value = val;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
