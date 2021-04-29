using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
                    Console.WriteLine("");
                    continue;
                }

                outPutFile[i] = outPutFile[i].Substring(0, outPutFile[i].Length - 1);
                Console.WriteLine(outPutFile[i]);
            }
            textBox1.Text = string.Join("\n", outPutFile);
        }
    }
}
