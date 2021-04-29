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
        public enum wormMoveDirection
        {
            up,
            down,
            left,
            right
        }
        static void Main(string[] args)
        {
            Start:
            Console.Write("input level number");
            string levelFilePath = Directory.GetCurrentDirectory() + "\\levels\\" + Console.ReadLine() +".ini";

            if(args.Length != 0)
            {
                levelFilePath = args[0];
                if (!File.Exists(levelFilePath))
                    return;
            }
            Restart:
            string[] data = GetLevelData(levelFilePath);

            if (data.Length < 6)
                return;

            Point newHeadPos;
            Point wormHead;
            Point end = Helpers.StringToPoint(data[2]);

            List<Point> worm =      new List<Point> { };
            List<Point> apples =    new List<Point> { };
            List<Point> map =       new List<Point> { };
            List<Point> spikes =    new List<Point> { };
            List<Point> rocks =     new List<Point> { };

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
                apples.Add(Helpers.StringToPoint(p));
            }

            // get all spike positions
            foreach (string p in data[3].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                spikes.Add(Helpers.StringToPoint(p));
            }

            // get all block positions
            foreach (string p in data[4].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                map.Add(Helpers.StringToPoint(p));
            }

            // get all block positions
            foreach (string p in data[5].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) break;
                rocks.Add(Helpers.StringToPoint(p));
            }
            
            Size mapSize = new Size(25, 25);

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

            wormMoveDirection dir = wormMoveDirection.down;
            bool wormMoved = false;
            bool isDead = false;
            int fallTilDeath = mapSize.Height; // if the worm falls for 10 loops it dies
            int fallLoopCount = 0;


            Stopwatch moveLimitStopwatch = new Stopwatch();
            moveLimitStopwatch.Start();
            while (true)
            {
                Console.SetCursorPosition(0, 0);


                List<Point> colPoints = new List<Point> { };
                colPoints.AddRange(map);
                colPoints.AddRange(apples);
                colPoints.AddRange(rocks);
                colPoints.Add(end);

                fallLoopCount = 0;
                while (IsFalling(worm, colPoints))
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

                fallLoopCount = 0;
                int[] fallingRocks = GetFallingRocksIndex(rocks, Helpers.CombineList(colPoints, worm));
                while(fallingRocks.Length != 0)
                {
                    foreach (int i in fallingRocks)
                    {
                        rocks[i] = new Point(rocks[i].X, rocks[i].Y + 1);
                    }
                    
                    fallLoopCount++;

                    if (fallLoopCount >= fallTilDeath)
                    {
                        foreach (int i in fallingRocks)
                        {
                            rocks.RemoveAt(i);
                        }
                        break;
                    }
                    fallingRocks = GetFallingRocksIndex(rocks, Helpers.CombineList(colPoints, worm));
                }

                

                if (InSpikes(worm, spikes))
                {
                    isDead = true;
                }

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

                        if(rocks.Contains(new Point(x, y)))
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.Gray);
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

                        if(apples.Contains(new Point(x, y)))
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
                            dir = wormMoveDirection.right;
                            break;
                        case ConsoleKey.LeftArrow:
                            newHeadPos = new Point(wormHead.X - 1, wormHead.Y);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.X--;
                            wormMoved = true;
                            dir = wormMoveDirection.left;
                            break;
                        case ConsoleKey.DownArrow:
                            newHeadPos = new Point(wormHead.X, wormHead.Y + 1);
                            //if(rocks.Contains(newHeadPos))
                            //    newHeadPos = new Point(newHeadPos.X, newHeadPos.Y + 1);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.Y++;
                            wormMoved = true;
                            dir = wormMoveDirection.down;
                            break;
                        case ConsoleKey.UpArrow:
                            newHeadPos = new Point(wormHead.X, wormHead.Y - 1);
                            if (map.Contains(newHeadPos) || worm.Contains(newHeadPos))
                                break;

                            wormHead.Y--;
                            wormMoved = true;
                            dir = wormMoveDirection.up;
                            break;
                        case ConsoleKey.R:
                            goto Restart;
                    }

                    if (rocks.Contains(wormHead))
                    {
                        int index = rocks.IndexOf(wormHead);
                        switch (dir)
                        {
                            case wormMoveDirection.up:
                                newHeadPos = new Point(wormHead.X, wormHead.Y - 1);
                                if (map.Contains(newHeadPos) || worm.Contains(newHeadPos) || rocks.Contains(newHeadPos))
                                {
                                    wormHead.Y++;
                                    wormMoved = false;
                                    break;
                                }
                                rocks[index] = new Point(rocks[index].X, rocks[index].Y - 1);
                                break;
                            case wormMoveDirection.down:
                                newHeadPos = new Point(wormHead.X, wormHead.Y + 1);
                                if (map.Contains(newHeadPos) || worm.Contains(newHeadPos) || rocks.Contains(newHeadPos))
                                {
                                    wormHead.Y--;
                                    wormMoved = false;
                                    break;
                                }
                                rocks[index] = new Point(rocks[index].X, rocks[index].Y + 1);
                                break;
                            case wormMoveDirection.left:
                                newHeadPos = new Point(wormHead.X - 1, wormHead.Y);
                                if (map.Contains(newHeadPos) || worm.Contains(newHeadPos) || rocks.Contains(newHeadPos))
                                {
                                    wormHead.X++;
                                    wormMoved = false;
                                    break;
                                }
                                rocks[index] = new Point(rocks[index].X - 1, rocks[index].Y);
                                break;
                            case wormMoveDirection.right:
                                newHeadPos = new Point(wormHead.X + 1, wormHead.Y);
                                if (map.Contains(newHeadPos) || worm.Contains(newHeadPos) || rocks.Contains(newHeadPos))
                                {
                                    wormHead.X--;
                                    wormMoved = false;
                                    break;
                                }
                                rocks[index] = new Point(rocks[index].X + 1, rocks[index].Y);
                                break;
                        }
                    }

                    moveLimitStopwatch.Restart();
                }

                

                if (wormMoved)
                {
                    wormMoved = false;
                    worm.Add(wormHead);

                    if (wormHead == end)
                    {
                        // put green over the purple head to make it look like
                        // its head went into the white end point
                        Console.SetCursorPosition(worm[worm.Count - 2].X * 2, worm[worm.Count - 2].Y);
                        Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkGreen);

                        // go through and replace the green with nothing
                        // to make it look like its entering 
                        for (int i = 0; i < worm.Count - 1; i++)
                        {
                            Console.SetCursorPosition(worm[i].X * 2, worm[i].Y);
                            Console.WriteLine("  ");
                            Thread.Sleep(1000 / worm.Count);
                        }
                        Console.SetCursorPosition(0, mapSize.Height - 2);
                        Console.WriteLine("level complete");
                        goto Start;
                    }

                    if (apples.Count != 0)
                    {
                        if (apples.Contains(wormHead))
                            apples.Remove(wormHead);
                        else
                            worm.RemoveAt(0);
                    }
                    else
                    {
                        worm.RemoveAt(0);
                    }
                }
                

                Helpers.ClearConsoleInputBuffer();
            }

            Console.WriteLine("you died");
            Console.ReadKey();
            goto Restart;
        }

        private static int[] GetFallingRocksIndex(List<Point> rocks, List<Point> colision)
        {
            List<int> fallingRocks = new List<int> { };
            for (int i = 0; i <= rocks.Count-1;i++)
                if (!colision.Contains(new Point(rocks[i].X, rocks[i].Y + 1)))
                    fallingRocks.Add(i);

            return fallingRocks.ToArray();
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
            foreach (Point p in worm)
                // if any part of the worm is touching the ground
                // we know its not falling
                if (colision.Contains(new Point(p.X, p.Y + 1)))
                    return false;

            return true;
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
    }
}
