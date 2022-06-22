using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;

namespace cliAppleWorm
{
    public static class Helpers
    {
        public static readonly ConsoleColor DEFAULTCONSOLEBACKCOLOR = Console.BackgroundColor;

        public static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            Regex regex = new Regex(@"\d+", RegexOptions.Compiled);

            int maxDigits = items
                          .SelectMany(i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                          .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')), stringComparer ?? StringComparer.CurrentCulture);
        }

        public static List<T> CombineList<T>(List<T> a, List<T> b)
        {
            List<T> r = new List<T> { };
            r.AddRange(a);
            r.AddRange(b);
            return r;
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
            if (string.IsNullOrEmpty(s)) 
                return Point.Empty;

            string[] p = s.Split(' ');
            return new Point(int.Parse(p[0]), int.Parse(p[1]));
        }
    }
}
