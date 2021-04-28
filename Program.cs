using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
namespace cliAppleWorm
{
    class Program
    {

       
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(101, 26);
            Console.SetBufferSize(101, 26);

            IntPtr handle = NativeMethods.GetConsoleWindow();
            IntPtr sysMenu = NativeMethods.GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_MINIMIZE, NativeConstants.MF_BYCOMMAND);
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_SIZE, NativeConstants.MF_BYCOMMAND);
            }

            Size mapSize = new Size(50, 25);

            Point newHeadPos;
            Point wormHead = new Point(6, 10);
            Point applePos = GetApplePosition(mapSize.Width - 1, mapSize.Height - 1);

            List<Point> worm = new List<Point> { new Point(4,10), new Point(5, 10), new Point(6, 10) };
            List<Point> map = new List<Point> { };

            for(int i = 0; i < mapSize.Width; i++)
            {
                map.Add(new Point(i, mapSize.Height-1));
            }
            for (int i = 0; i < mapSize.Width; i++)
            {
                map.Add(new Point(i, 0));
            }
            for (int i = 0; i < mapSize.Height; i++)
            {
                map.Add(new Point(0,i));
            }
            for (int i = 0; i < mapSize.Height; i++)
            {
                map.Add(new Point(mapSize.Width - 1, i));
            }

            bool skipInput = true;
            bool wormMoved = false;

            Stopwatch moveLimitStopwatch = new Stopwatch();
            moveLimitStopwatch.Start();
            while (true)
            {
                Console.SetCursorPosition(0, 0);

                wormMoved = false;

                for (int y = 0; y < mapSize.Height; y++)
                {
                    
                    for (int x = 0; x < mapSize.Width; x++)
                    {
                        if(map.Contains(new Point(x, y)))
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkGray);
                            continue;
                        }

                        if(applePos.X == x && applePos.Y == y)
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkRed);
                            continue;
                        }

                        if (worm.Contains(new Point(x, y)))
                        {
                            if (wormHead.X == x && wormHead.Y == y)
                            {
                                Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkMagenta);
                                continue;
                            }
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkGreen);
                            continue;
                        }

                        Console.Write("  ");
                    }
                    Console.Write("\n");
                }

                ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                if (moveLimitStopwatch.ElapsedMilliseconds > 25)
                {
                    switch (k.Key)
                    {
                        case ConsoleKey.RightArrow:
                            newHeadPos = new Point(wormHead.X + 1, wormHead.Y);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.X++;
                            wormMoved = true;
                            break;
                        case ConsoleKey.LeftArrow:
                            newHeadPos = new Point(wormHead.X - 1, wormHead.Y);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.X--;
                            wormMoved = true;
                            break;
                        case ConsoleKey.DownArrow:
                            newHeadPos = new Point(wormHead.X, wormHead.Y + 1);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.Y++;
                            wormMoved = true;
                            break;
                        case ConsoleKey.UpArrow:
                            newHeadPos = new Point(wormHead.X, wormHead.Y - 1);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.Y--;
                            wormMoved = true;
                            break;
                    }
                    moveLimitStopwatch.Restart();
                }

                if (wormMoved)
                {
                    worm.Add(wormHead);
                    if (wormHead != applePos)
                    {
                        worm.RemoveAt(0);
                    }
                    else
                    {
                        applePos = GetApplePosition(mapSize.Width - 1, mapSize.Height - 1);
                    }
                }

                Helpers.ClearConsoleInputBuffer();
            }
        }



        private static Point GetApplePosition(int width, int height)
        {
            Random r = new Random();
            return new Point(r.Next(1, width), r.Next(1, height));
        }
    }
}
