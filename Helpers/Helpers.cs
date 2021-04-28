using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cliAppleWorm
{
    public static class Helpers
    {
        public static readonly ConsoleColor DEFAULTCONSOLEBACKCOLOR = Console.BackgroundColor;

        public static void WriteAsConsoleColor(string output, ConsoleColor col)
        {
            Console.BackgroundColor = col;
            Console.Write(output);
            Console.BackgroundColor = DEFAULTCONSOLEBACKCOLOR;
        }

        public static void ClearConsoleInputBuffer()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(false);
        }
    }
}
