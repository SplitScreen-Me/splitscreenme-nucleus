using System.Diagnostics;

namespace Nucleus.Gaming.Coop.InputManagement.Logging
{
    internal static class Logger
    {
        public static void WriteLine(object message)
        {
            WriteLine(message.ToString());
        }

        public static void WriteLine(string message)
        {
#if DEBUG
            LogManager.Log(message);

            Debug.WriteLine(message);
#endif
        }
    }
}
