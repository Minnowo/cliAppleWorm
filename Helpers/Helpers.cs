using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace cliAppleWorm
{
    public static class Helpers
    {
        public static readonly ConsoleColor DEFAULTCONSOLEBACKCOLOR = Console.BackgroundColor;

        public static List<T> CombineList<T>(List<T> a, List<T> b)
        {
            b.AddRange(a);
            return b;
        }

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

        public static Point StringToPoint(string s)
        {
            if (string.IsNullOrEmpty(s)) return Point.Empty;

            string[] p = s.Split(' ');
            return new Point(int.Parse(p[0]), int.Parse(p[1]));
        }
    }
}
