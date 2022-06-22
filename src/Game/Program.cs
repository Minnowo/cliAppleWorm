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

        public enum LevelCompletionResult
        {
            win,
            lose,
            restart,
            cancel
        }


        public static LevelCompletionResult PlayLevel(AppleWormLevel l)
        {
            if (!l.IsValidLevel)
                return LevelCompletionResult.cancel;

            //level.SetConsoleBuffers();

            AppleWormLevel level = l.Clone();

            Point newHeadPos = new Point();
            bool wormMoved = false;
            bool isDead = false;
            int fallTilDeath = level.Height; // if the worm falls for 10 loops it dies
            int fallLoopCount = 0;

            wormMoveDirection dir = wormMoveDirection.down;

            while (true)
            {
                // reset the console cursor 
                // makes the console draw from top left
                Console.SetCursorPosition(0, 0);

                level.ApplyGravity();

                if (level.WormAlive)
                {
                    level.WormInSpikes();
                }

                for (int y = 0; y < level.Height; y++)
                {
                    for (int x = 0; x < level.Width; x++)
                    {
                        if (level.Map.Any(_ => _ == new Point(x, y)))
                        {
                            Console.Write("[]");
                            continue;
                        }

                        if (level.Rocks.Any(_ => _ == new Point(x, y)))
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkGray);
                            continue;
                        }

                        if (level.Spikes.Any(_ => _ == new Point(x, y)))
                        {
                            Console.Write("XX");
                            continue;
                        }

                        if (level.WinningPortal.X == x && level.WinningPortal.Y == y)
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.White);
                            continue;
                        }

                        if (level.Apples.Any(_ => _ == new Point(x, y)))
                        {
                            Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkRed);
                            continue;
                        }

                        if (level.Worm.Any(_ => _ == new Point(x, y)))
                        {
                            if (level.WormHead.X == x && level.WormHead.Y == y)
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

                Console.Write(new string(' ', level.Width));
                Console.WriteLine(level.Name.PadRight(level.Width*2, ' '));

                if (!level.WormAlive || level.WormInSpikes())
                {
                    Console.Write(new string(' ', level.Width));
                    Console.WriteLine("YOU DIED".PadRight(level.Width*2, ' '));
                    Console.ReadKey();
                    return LevelCompletionResult.lose;
                }   

                ConsoleKeyInfo k = Console.ReadKey(intercept: true);

                switch (k.Key)
                {
                    case ConsoleKey.RightArrow:
                        newHeadPos = new Point(level.WormHead.X + 1, level.WormHead.Y);
                        if (level.InvalidWormMove(newHeadPos))
                            break;

                        level.WormHead.X++;
                        wormMoved = true;
                        dir = wormMoveDirection.right;
                        break;

                    case ConsoleKey.LeftArrow:
                        newHeadPos = new Point(level.WormHead.X - 1, level.WormHead.Y);
                        if (level.InvalidWormMove(newHeadPos))
                            break;

                        level.WormHead.X--;
                        wormMoved = true;
                        dir = wormMoveDirection.left;
                        break;
                    case ConsoleKey.DownArrow:
                        newHeadPos = new Point(level.WormHead.X, level.WormHead.Y + 1);
                        if (level.InvalidWormMove(newHeadPos))
                            break;

                        level.WormHead.Y++;
                        wormMoved = true;
                        dir = wormMoveDirection.down;
                        break;
                    case ConsoleKey.UpArrow:
                        newHeadPos = new Point(level.WormHead.X, level.WormHead.Y - 1);
                        if (level.InvalidWormMove(newHeadPos))
                                break;

                        level.WormHead.Y--;
                        wormMoved = true;
                        dir = wormMoveDirection.up;
                        break;

                    case ConsoleKey.R:
                        Console.Write(new string(' ', level.Width));
                        Console.WriteLine("Restarting...".PadRight(level.Width*2, ' '));
                        return LevelCompletionResult.restart;

                    case ConsoleKey.Escape:
                        return LevelCompletionResult.cancel;
                }

                if (level.Rocks.Any(_ => _ == level.WormHead))
                {
                    int index = level.Rocks.IndexOf(level.WormHead);
                    switch (dir)
                    {
                        case wormMoveDirection.up:
                            newHeadPos.Y--;
                            if (level.ValidRockMove(newHeadPos))
                            {
                                level.WormHead.Y++;
                                wormMoved = false;
                                break;
                            }
                            level.Rocks[index] = new Point(level.Rocks[index].X, level.Rocks[index].Y - 1);
                            break;

                        case wormMoveDirection.down:
                            newHeadPos.Y++;
                            if (level.ValidRockMove(newHeadPos))
                            {
                                level.WormHead.Y--;
                                wormMoved = false;
                                break;
                            }
                            level.Rocks[index] = new Point(level.Rocks[index].X, level.Rocks[index].Y + 1);
                            break;

                        case wormMoveDirection.left:
                            newHeadPos.X--;
                            if (level.ValidRockMove(newHeadPos))
                            {
                                level.WormHead.X++;
                                wormMoved = false;
                                break;
                            }
                            level.Rocks[index] = new Point(level.Rocks[index].X - 1, level.Rocks[index].Y);
                            break;

                        case wormMoveDirection.right:
                            newHeadPos.X++;
                            if (level.ValidRockMove(newHeadPos))
                            {
                                level.WormHead.X--;
                                wormMoved = false;
                                break;
                            }
                            level.Rocks[index] = new Point(level.Rocks[index].X + 1, level.Rocks[index].Y);
                            break;
                    }
                }

                if (wormMoved)
                {
                    wormMoved = false;
                    level.Worm.Add(level.WormHead);

                    if (level.WormHead == level.WinningPortal)
                    {
                        // put green over the purple head to make it look like
                        // its head went into the white end point
                        Console.SetCursorPosition(level.Worm[level.Worm.Count - 2].X * 2, level.Worm[level.Worm.Count - 2].Y);
                        Helpers.WriteAsConsoleColor("  ", ConsoleColor.DarkGreen);

                        // go through and replace the green with nothing
                        // to make it look like its entering 
                        for (int i = 0; i < level.Worm.Count - 1; i++)
                        {
                            if (level.Worm[i].X * 2 < 0 || level.Worm[i].Y < 0)
                               continue;
                            
                            Console.SetCursorPosition(level.Worm[i].X * 2, level.Worm[i].Y);
                            Console.WriteLine("  ");
                            Thread.Sleep(1000 / level.Worm.Count);
                        }
                        Console.SetCursorPosition(0, level.Height+1);
                        Console.Write(new string(' ', (level.Width / 2) - level.Name.Length));
                        Console.WriteLine("Level Complete!".PadRight(level.Width, ' '));
                        return LevelCompletionResult.win;
                    }

                    if (level.Apples.Count != 0)
                    {
                        if(!level.Apples.Remove(level.Apples.Find(_ => _ == level.WormHead)))
                        {
                            level.Worm.RemoveAt(0);
                        }
                    }
                    else
                    {
                        level.Worm.RemoveAt(0);
                    }
                }

                Helpers.ClearConsoleInputBuffer();
            }
        }

        public static string AskGetLevel()
        {
            string file;
            do
            {
                Console.WriteLine("input level name '|auto|' to do 0->X");
                file = Console.ReadLine();

                if (file == "|auto|")
                {
                    return "|auto|";
                }

                file = Directory.GetCurrentDirectory() + "\\levels\\" + file + ".awlf";
            }
            while (!File.Exists(file));

            return file;
        }

        public static List<string> levels = new List<string>();
        public static int levelIndex = 0;
        public const string levelDir = "\\levels\\";

        public static AppleWormLevel LoadLevel(string path, bool autolevel)
        {
            AppleWormLevel level;
            if (autolevel)
            {
                levels.Clear();
                foreach (string i in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + levelDir).OrderByNatural(e => e))
                {
                    levels.Add(i);
                }

                do
                {
                    if(levelIndex >= levels.Count)
                    {
                        Console.WriteLine("No more levels");
                        return null;
                    }

                    level = new AppleWormLevel(levels[levelIndex], true);
                    levelIndex++;
                }
                while (!level.IsValidLevel);

                return level;
            }

            level = new AppleWormLevel(path, true);

            if (!level.IsValidLevel)
            {
                return LoadLevel(AskGetLevel(), autolevel);
            }
            return level;
        }

        static void Main()
        {
            bool autoLevel = false;
            string level;
            while (true)
            {
            // i know i know, but goto is chad in this case
            start:

                level = "";
                if (!autoLevel)
                {
                    level = AskGetLevel();

                    if (level == "|auto|")
                    {
                        levelIndex = 0;
                        autoLevel = true;
                    }
                    Console.Clear();
                }

                AppleWormLevel l = LoadLevel(level, autoLevel);

                if(l == null)
                {
                    autoLevel = false;
                    continue;
                }

                while (true)
                {    
                    LevelCompletionResult result = PlayLevel(l);

                    switch (result)
                    {
                        case LevelCompletionResult.cancel:
                            autoLevel = false;
                            goto start;

                        case LevelCompletionResult.restart:
                            continue;

                        case LevelCompletionResult.win:
                            goto start;

                        case LevelCompletionResult.lose:
                            continue;
                    }
                }
            }
        }
    }
}
