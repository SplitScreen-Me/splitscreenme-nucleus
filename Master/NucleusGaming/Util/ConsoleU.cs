using System;

namespace Nucleus.Gaming
{
    public static class ConsoleU
    {
        public static void WriteLine(string line, ConsoleColor foreGroundColor)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = foreGroundColor;
            Console.WriteLine(line);
            Console.ForegroundColor = current;
        }
    }
}
