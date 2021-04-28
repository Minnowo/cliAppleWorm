using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace cliAppleWorm
{
    class Program
    {
        static void Main(string[] args)
        {
            string levelFilePath = Directory.GetCurrentDirectory() + "\\levels\\21.ini";

            if(args.Length != 0)
            {
                levelFilePath = args[0];
                if (!File.Exists(levelFilePath))
                    return;
            }

            string[] data = GetLevelData(levelFilePath);

            if (data.Length < 5)
                return;

            Point newHeadPos;
            Point wormHead;
            Point end = Helpers.StringToPoint(data[2]);

            List<Point> worm =      new List<Point> { };
            List<Point> applePos =  new List<Point> { };
            List<Point> map =       new List<Point> { };
            List<Point> spikes =    new List<Point> { };

            // get all worm positions
            foreach(string p in data[0].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                worm.Add(Helpers.StringToPoint(p));
            }
            wormHead = worm.Last();

            // get all apple locations
            foreach (string p in data[1].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                applePos.Add(Helpers.StringToPoint(p));
            }

            // get all spike positions
            foreach (string p in data[3].Split('|'))
            {
                spikes.Add(Helpers.StringToPoint(p));
            }

            // get all block positions
            foreach (string p in data[4].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                map.Add(Helpers.StringToPoint(p));
            }

            Size mapSize = new Size(25, 10);

            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(mapSize.Width * 2 + 1, mapSize.Height + 1);
            Console.SetBufferSize(mapSize.Width * 2 + 1, mapSize.Height + 1);

            IntPtr handle = NativeMethods.GetConsoleWindow();
            IntPtr sysMenu = NativeMethods.GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_MINIMIZE, NativeConstants.MF_BYCOMMAND);
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_SIZE, NativeConstants.MF_BYCOMMAND);
            }


            bool wormMoved = false;
            bool isDead = false;
            int fallTilDeath = 10; // if the worm falls for 10 loops it dies
            int fallLoopCount = 0;


            Stopwatch moveLimitStopwatch = new Stopwatch();
            moveLimitStopwatch.Start();
            while (true)
            {
                Console.SetCursorPosition(0, 0);

                while (IsFalling(worm, Helpers.CombineList(map, applePos)))
                {
                    for (int i = 0; i < worm.Count; i++)
                    {
                        worm[i] = new Point(worm[i].X, worm[i].Y + 1);
                    }

                    wormHead.Y++;
                    fallLoopCount++;

                    if (fallLoopCount >= fallTilDeath)
                    {
                        isDead = true;
                        break;
                    }
                }

                if (InSpikes(worm, spikes))
                {
                    isDead = true;
                }

                fallLoopCount = 0;

                for (int y = 0; y < mapSize.Height; y++)
                {
                    for (int x = 0; x < mapSize.Width; x++)
                    {
                        if(map.Contains(new Point(x, y)))
                        {
                            //Helpers.WriteAsConsoleColor("[]", ConsoleColor.DarkYellow);
                            Console.Write("[]");
                            continue;
                        }

                        if(spikes.Contains(new Point(x, y)))
                        {
                            Console.Write("WW");
                            continue;
                        }

                        if(end.X == x && end.Y == y)
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.White);
                            continue;
                        }

                        if(applePos.Contains(new Point(x, y)))
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

                if (isDead) break;

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
                    wormMoved = false;

                    worm.Add(wormHead);

                    if (!applePos.Contains(wormHead))
                    {
                        worm.RemoveAt(0);
                    }
                    else
                    {
                        applePos.Remove(wormHead);
                    }
                }

                Helpers.ClearConsoleInputBuffer();
            }

            Console.WriteLine("you died");
            Console.ReadLine();
        }

        private static bool InSpikes(List<Point> worm, List<Point> spikes)
        {
            foreach(Point p in worm)
                if (spikes.Contains(p))
                    return true;

            return false;
        }

        private static bool IsFalling(List<Point> worm, List<Point> colision)
        {
            int bodyToGroundCount = 0;
            foreach (Point p in worm)
                if (!colision.Contains(new Point(p.X, p.Y + 1)))
                    bodyToGroundCount++;

            if(bodyToGroundCount == worm.Count)
                return true;
            
            return false;
        }

        private static string[] GetLevelData(string path)
        {
            if (!File.Exists(path)) return null;

            List<string> lines = new List<string> { };

            using (var fileStream = File.OpenRead(path))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
            {
                String line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    lines.Add(line.Substring(0, line.IndexOf(';') != -1 ? line.IndexOf(';') : line.Length).Trim()); 
                }
            }

            return lines.ToArray();
        }

        private static Point GetApplePosition(int width, int height)
        {
            Random r = new Random();
            return new Point(r.Next(1, width), r.Next(1, height));
        }
    }
}
