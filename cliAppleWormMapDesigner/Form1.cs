using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cliAppleWormMapDesigner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            rbMap.Checked = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine(panel1.Size);
            for(int y = 0; y < panel1.Height/13; y++)
            {
                for(int x = 0; x < panel1.Width/13; x++)
                {
                    ColorCheckBox cb = new ColorCheckBox();
                    cb.AutoSize = false;
                    cb.Size = new Size(13, 13);
                    cb.Text = "";
                    cb.Margin = new Padding(0, 0, 0, 0);
                    cb.Padding = new Padding(0, 0, 0, 0);
                    cb.Location = new Point(x*13, y * 13);
                    cb.BackColor = Color.Red;
                    cb.drawX = false;
                    cb.CheckedChanged += checkboxchecked;
                    panel1.Controls.Add(cb);
                }
            }
        }

        private void checkboxchecked(object sender, EventArgs e)
        {
            ColorCheckBox cb = sender as ColorCheckBox;

            if (cb == null) { Console.WriteLine("null"); return; }

            if (!cb.Checked)
            {
                cb.backCol = Color.White;
                cb.it = itemType.none;
                cb.Invalidate();
                return;
            }

            if (rbMap.Checked)
            {
                cb.backCol = Color.FromArgb(74, 41, 21);
                cb.it = itemType.map;
                cb.Invalidate();
                return;
            }

            if (rbRock.Checked)
            {
                cb.backCol = Color.Gray;
                cb.it = itemType.rock;
                cb.Invalidate();
                return;
            }

            if (rbApple.Checked)
            {
                cb.backCol = Color.FromArgb(153, 0, 0);
                cb.it = itemType.apple;
                cb.Invalidate();
                return;
            }

            if (rbWorm.Checked)
            {
                cb.backCol = Color.FromArgb(0, 111, 0);
                cb.it = itemType.worm;
                cb.Invalidate();
                return;
            }

            if (rbSpike.Checked)
            {
                cb.backCol = Color.FromArgb(28, 32, 4);
                cb.it = itemType.spike;
                cb.Invalidate();
                return;
            }

            if (rbEndPoint.Checked)
            {
                cb.backCol = Color.Blue;
                cb.it = itemType.endpoint;
                cb.Invalidate();
                return;
            }

            if (radioButton7.Checked)
            {
                cb.backCol = Color.FromArgb(157, 0, 178);
                cb.it = itemType.wormhead;
                cb.Invalidate();
                return;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string[] outPutFile = new string[] { "", "", "", "", "", ""};
            string wormHead = "";
            foreach(ColorCheckBox cb in panel1.Controls.OfType<ColorCheckBox>())
            {
                switch (cb.it)
                {
                    case itemType.none:
                        break;

                    case itemType.worm:
                        outPutFile[0] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.wormhead:
                        wormHead += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.rock:
                        outPutFile[5] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.apple:
                        outPutFile[1] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.spike:
                        outPutFile[3] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.endpoint:
                        outPutFile[2] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;

                    case itemType.map:
                        outPutFile[4] += $"{cb.Location.X / 13} {cb.Location.Y / 13}|";
                        break;
                }
            }

            outPutFile[0] += wormHead;
            for(int i = 0; i<outPutFile.Length; i++)
            {
                if (string.IsNullOrEmpty(outPutFile[i]))
                {
                    Console.WriteLine(";");
                    outPutFile[i] = ";";
                    continue;
                }

                outPutFile[i] = outPutFile[i].Substring(0, outPutFile[i].Length - 1);
                outPutFile[i] += ";";
                Console.WriteLine(outPutFile[i]);
            }
            textBox1.Text = string.Join("\n", outPutFile);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ColorCheckBox cb in panel1.Controls.OfType<ColorCheckBox>())
            {
                cb.backCol = Color.White;
                cb.it = itemType.none;
                cb.Invalidate();
            }
            }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "apple worm level file (*.awlf)|*.awlf";

                saveFileDialog1.FileName = "0000.awlf";
                saveFileDialog1.Title = "Save An Apple Worm Level File";
                //saveFileDialog1.ShowDialog();

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    button6_Click(null, e);
                    try
                    {
                        System.IO.File.WriteAllText(saveFileDialog1.FileName, textBox1.Text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error saving {ex}");
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            GC.Collect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "apple worm level file (*.awlf)|*.awlf";
                ofd.Title = "Open An Apple Worm Level File";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string fileName = ofd.FileName;

                    string[] data = GetLevelData(fileName);

                    if (data.Length < 6)
                        return;

                    Point newHeadPos;
                    Point wormHead = Point.Empty;
                    Point end = StringToPoint(data[2]);

                    List<Point> worm = new List<Point> { };
                    List<Point> apples = new List<Point> { };
                    List<Point> map = new List<Point> { };
                    List<Point> spikes = new List<Point> { };
                    List<Point> rocks = new List<Point> { };

                    // get all worm positions
                    foreach (string p in data[0].Split('|'))
                    {
                        if (string.IsNullOrEmpty(p)) break;
                        worm.Add(StringToPoint(p));
                    }
                    if (worm.Count != 0)
                        wormHead = worm.Last();

                    // get all apple locations
                    foreach (string p in data[1].Split('|'))
                    {
                        if (string.IsNullOrEmpty(p)) break;
                        apples.Add(StringToPoint(p));
                    }

                    // get all spike positions
                    foreach (string p in data[3].Split('|'))
                    {
                        if (string.IsNullOrEmpty(p)) break;
                        spikes.Add(StringToPoint(p));
                    }

                    // get all block positions
                    foreach (string p in data[4].Split('|'))
                    {
                        if (string.IsNullOrEmpty(p)) break;
                        map.Add(StringToPoint(p));
                    }

                    // get all block positions
                    foreach (string p in data[5].Split('|'))
                    {
                        if (string.IsNullOrEmpty(p)) break;
                        rocks.Add(StringToPoint(p));
                    }

                    //button1_Click(null, e);

                    foreach (ColorCheckBox cb in panel1.Controls.OfType<ColorCheckBox>())
                    {
                        Point p = new Point(cb.Location.X / 13, cb.Location.Y / 13);

                        if (map.Contains(p))
                        {
                            cb.backCol = Color.FromArgb(74, 41, 21);
                            cb.it = itemType.map;
                            cb.Invalidate();
                            continue;
                        }

                        if (rocks.Contains(p))
                        {
                            cb.backCol = Color.Gray;
                            cb.it = itemType.rock;
                            cb.Invalidate();
                            continue;
                        }

                        if (spikes.Contains(p))
                        {
                            cb.backCol = Color.FromArgb(28, 32, 4);
                            cb.it = itemType.spike;
                            cb.Invalidate();
                            continue;
                        }

                        if (end.X == p.X && end.Y == p.Y)
                        {
                            cb.backCol = Color.Blue;
                            cb.it = itemType.endpoint;
                            cb.Invalidate();
                            continue;
                        }

                        if (apples.Contains(p))
                        {
                            cb.backCol = Color.FromArgb(153, 0, 0);
                            cb.it = itemType.apple;
                            cb.Invalidate();
                            continue;
                        }

                        if (worm.Contains(p))
                        {
                            if (wormHead.X == p.X && wormHead.Y == p.Y)
                            {
                                cb.backCol = Color.FromArgb(151, 0, 178);
                                cb.it = itemType.wormhead;
                                cb.Invalidate();
                                continue;
                            }
                            cb.backCol = Color.FromArgb(0, 111, 0);
                            cb.it = itemType.worm;
                            cb.Invalidate();
                            continue;
                        }

                        cb.backCol = Color.White;
                        cb.it = itemType.none;
                        cb.Invalidate();
                    }
                }
            }
        }
        public static Point StringToPoint(string s)
        {
            if (string.IsNullOrEmpty(s)) return Point.Empty;

            string[] p = s.Split(' ');
            return new Point(int.Parse(p[0]), int.Parse(p[1]));
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
