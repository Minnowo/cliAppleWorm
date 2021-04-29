using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;

namespace cliAppleWormMapDesigner
{
    public enum itemType
    {
        worm,
        wormhead,
        map,
        spike,
        endpoint,
        apple,
        none,
        rock
    }
    class ColorCheckBox : CheckBox
    {
        public Color backCol = Color.White;
        public bool drawX = true;
        public itemType it = itemType.none;
        public ColorCheckBox()
        {
            //Appearance = System.Windows.Forms.Appearance.Button;
            //FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            //TextAlign = ContentAlignment.MiddleRight;
            Text = "";
            FlatAppearance.BorderSize = 0;
            AutoSize = false;
            Height = 13;
            Width = 13;
            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 0, 0);
            //BackColor = Color.Red;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            //base.OnPaint(pevent);

            pevent.Graphics.Clear(BackColor);

            using (SolidBrush brush = new SolidBrush(ForeColor))
                pevent.Graphics.DrawString(Text, Font, brush, 27, 4);

            Rectangle rect = new Rectangle(new Point(0, 0), new Size(16, 16));

            using(SolidBrush b = new SolidBrush(backCol))
            pevent.Graphics.FillRectangle(b, rect);

            if (Checked && drawX)
            {
                using (SolidBrush brush = new SolidBrush(Color.Black))
                using (Font wing = new Font("consolas", 12f))
                    pevent.Graphics.DrawString("x", wing, brush, 0, -4);
            }
            pevent.Graphics.DrawRectangle(Pens.DarkSlateBlue, rect);

            Rectangle fRect = ClientRectangle;

            if (Focused)
            {
                fRect.Inflate(-1, -1);
                using (Pen pen = new Pen(Brushes.Gray))
                    pevent.Graphics.DrawRectangle(pen, fRect);
            }
        }
    }
}
