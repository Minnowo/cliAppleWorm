using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing;

namespace cliAppleWorm
{
    public class AppleWormLevel
    {
        public List<Point> Worm;
        public List<Point> Apples;
        public List<Point> Map;
        public List<Point> Spikes;
        public List<Point> Rocks;

        public Point WinningPortal;
        public Point WormHead;
        public Size MapSize;
        public string Name;
        public bool WormAlive = true;

        public int Width
        {
            get
            {
                if (this.MapSize == null)
                    return -1;
                return this.MapSize.Width;
            }
            set
            {
                if (this.MapSize == null)
                {
                    this.MapSize = new Size(value, 0);
                    return;
                }
                this.MapSize = new Size(value, this.MapSize.Height);
            }
        }

        public int Height
        {
            get
            {
                if (this.MapSize == null)
                    return -1;
                return this.MapSize.Height;
            }
            set
            {
                if (this.MapSize == null)
                {
                    this.MapSize = new Size(0, value);
                    return;
                }
                this.MapSize = new Size(this.MapSize.Width, value);
            }
        }

        public bool IsValidLevel = false;

        public AppleWormLevel()
        {
            this.Worm = new List<Point>();
            this.Apples = new List<Point>();
            this.Map = new List<Point>();
            this.Spikes = new List<Point>();
            this.Rocks = new List<Point>();

            this.MapSize = new Size(25, 25);
            this.WormHead = new Point(0, 0);
            this.WinningPortal = new Point(0, 1);
            this.Name = "";
        }

        public AppleWormLevel(string file) : this()
        {
            this.LoadLevel(file);
            this.IsValidLevel = true;
            this.Name = Path.GetFileNameWithoutExtension(file);
        }

        public AppleWormLevel(string file, bool suppressErrors) : this()
        {
            try
            {
                this.LoadLevel(file);
                this.IsValidLevel = true;
                this.Name = Path.GetFileNameWithoutExtension(file);
            }
            catch (InvalidLevelFile)
            {
                IsValidLevel = false;
            }
        }

        public AppleWormLevel Clone()
        {
            AppleWormLevel l = new AppleWormLevel();
            l.Map.AddRange(this.Map);
            l.Worm.AddRange(this.Worm);
            l.Spikes.AddRange(this.Spikes);
            l.Rocks.AddRange(this.Rocks);
            l.Apples.AddRange(this.Apples);
            l.WinningPortal = this.WinningPortal;
            l.WormHead = this.WormHead;
            l.MapSize = this.MapSize;
            l.WormAlive = this.WormAlive;
            l.Name = this.Name;
            return l;
        }

        public bool ValidRockMove(Point newRockPosition)
        {
            return (this.Map.Any(_ => _ == newRockPosition) || 
                this.Worm.Any(_ => _ == newRockPosition) || 
                this.Rocks.Any(_ => _ == newRockPosition) || 
                this.Apples.Any(_ => _ == newRockPosition));
        }

        public bool InvalidWormMove(System.Drawing.Point newHeadPosition)
        {
            return (this.Map.Any(_ => _ == newHeadPosition) || this.Worm.Any(_ => _ == newHeadPosition));
        }

        public bool WormInSpikes()
        {
            foreach (Point b in this.Worm)
            {
                if(this.Spikes.Any(s => s == b))
                {
                    return true;
                }
            }
            return false;
        }

        public bool WormFalling()
        {
            foreach(Point b in this.Worm)
            {
                if(this.WinningPortal.Y - 1 == b.Y && this.WinningPortal.X == b.X)
                {
                    return false;
                }

                if(this.Map.Any(m => m.Y - 1 == b.Y && m.X == b.X))
                {
                    return false;
                }

                if(this.Apples.Any(a => a.Y - 1 == b.Y && a.X == b.X))
                {
                    return false;
                }

                if(this.Rocks.Any(r => r.Y - 1 == b.Y && r.X == b.X))
                {
                    return false;
                }
            }

            return true;
        }

        public void ApplyGravity()
        {
            int fallLoopCount = 0;
            while (this.WormFalling())
            {
                for (int i = 0; i < this.Worm.Count; i++)
                {
                    this.Worm[i] = new Point(this.Worm[i].X, this.Worm[i].Y + 1);
                }

                this.WormHead.Y++;

                fallLoopCount++;

                if (fallLoopCount >= this.MapSize.Height && fallLoopCount > 5 || this.WormInSpikes())
                {
                    WormAlive = false;
                    return;
                }
            }

            // order the rocks to avoid stacked rocks from not falling
            this.Rocks.OrderBy(r => r.Y);

            for(int i = 0; i < this.Rocks.Count; i++)
            {
                Point r = this.Rocks[i];

                // if anything can be colide with rocks moved up 1 position hits the current rock
                // then we know its not falling, and can move on
                if (this.Map.Any(m => m.Y - 1 == r.Y && m.X == r.X))
                {
                    continue;
                }

                if(this.Worm.Any(w => w.Y - 1 == r.Y && w.X == r.X))
                {
                    continue;
                }

                if(this.Rocks.Any(w => w.Y - 1 == r.Y && w.X == r.X))
                {
                    continue;
                }

                if (this.Apples.Any(a => a.Y - 1 == r.Y && a.X == r.X))
                {
                    continue;
                }

                if (this.WinningPortal.Y - 1 == r.Y && this.WinningPortal.X == r.X)
                {
                    continue;
                }

                // make rock fall
                this.Rocks[i] = new Point(r.X, r.Y + 1);
                    
                // rock out of map
                if(this.Rocks[i].Y > this.MapSize.Height)
                {
                    this.Rocks.RemoveAt(i);
                }
                // check the same rock again
                i--;
            }
        }

        public void SetConsoleBuffers()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(this.Width * 2 + 1, this.Height + 1);
            Console.SetBufferSize(this.Width * 2 + 1, this.Height + 1);

            IntPtr handle = NativeMethods.GetConsoleWindow();
            IntPtr sysMenu = NativeMethods.GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_MINIMIZE, NativeConstants.MF_BYCOMMAND);
                NativeMethods.DeleteMenu(sysMenu, NativeConstants.SC_SIZE, NativeConstants.MF_BYCOMMAND);
            }

        }

        public void LoadLevel(string file)
        {
            string[] data = GetLevelData(file);

            if (data.Length < 6)
                throw new InvalidLevelFile("invalid data length, should be 6 lines or more", new FileInfo(file));

            // get all worm positions
            foreach (string p in data[0].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) 
                    break;

                this.Worm.Add(Helpers.StringToPoint(p));
            }

            if (this.Worm.Count != 0)
            {
                this.WormHead = this.Worm.Last();
            }
            else
            {
                throw new InvalidLevelFile("no worm found", new FileInfo(file));
            }

            // get all apple locations
            foreach (string p in data[1].Split('|'))
            {
                if (string.IsNullOrEmpty(p))
                    break;

                this.Apples.Add(Helpers.StringToPoint(p));
            }

            this.WinningPortal = Helpers.StringToPoint(data[2]);

            if(this.WinningPortal == this.WormHead)
            {
                throw new InvalidLevelFile("end position is equal to the head position", new FileInfo(file));
            }

            // get all spike positions
            foreach (string p in data[3].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) 
                    break;

                this.Spikes.Add(Helpers.StringToPoint(p));
            }

            // get all block positions
            foreach (string p in data[4].Split('|'))
            {
                if (string.IsNullOrEmpty(p)) 
                    break;

                this.Map.Add(Helpers.StringToPoint(p));
            }

            // get all block positions
            foreach (string p in data[5].Split('|'))
            {
                if (string.IsNullOrEmpty(p))
                    break;

                this.Rocks.Add(Helpers.StringToPoint(p));
            }

            // backwards compatibility for storing level size in the files
            if(data.Length < 7)
            {
                this.MapSize = new Size(25, 25);
                return;
            }
            Point pp = Helpers.StringToPoint(data[6]);
            this.MapSize = new Size(pp.X, pp.Y);
        }

        private static string[] GetLevelData(string path)
        {
            if (!File.Exists(path))
                return null;

            List<string> lines = new List<string>();

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
