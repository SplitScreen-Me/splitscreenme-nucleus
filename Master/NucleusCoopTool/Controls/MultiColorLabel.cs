using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public class MultiColorLabel
    {
        private DoubleBufferPanel container;
        private List<Label> labels;

        private Point location;
        public Point Location
        {
            get { return location; }
            set
            {
                location = value;
                container.Location = value;
            }
        }

        public Size Size
        {
            get { return container.Size; }
            set { container.Size = value; }
        }

        public int Width
        {
            get { return container.Width; }
            set { container.Width = value; }
        }   

        public int Height
        {
            get { return container.Height; }
            set { container.Height = value; }
        }

        private (string, Color)[] content = new (string, Color)[] { };
        public (string, Color)[] Content => content;

        public MultiColorLabel(Control parent,Point location)
        {
            container = new DoubleBufferPanel();
            container.BackColor = Color.Transparent;
            container.AutoSize = true;
            container.Location = location;
            parent.Controls.Add(container);
        }

        public void SetText((string, Color)[] words)
        {
            for (int i = 0; i < container.Controls.Count; i++)
            {
                var con = container.Controls[i];
                container.Controls.Remove(con);
                con.Dispose();
            }

            container.Controls.Clear();

            labels = new List<Label>();

            for (int i = 0; i < words.Length; i++)
            {
                Label lbl = new Label();
                lbl.Font = new Font(Theme_Settings.CustomFont, 14, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                lbl.Text = words[i].Item1;
                lbl.ForeColor = words[i].Item2;
                lbl.AutoSize = true;
                lbl.Padding = new Padding(0, 0, 0, 0);
                lbl.Margin = new Padding(0, 0, 0, 0);
                lbl.Resize += ArrangeEvent;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.BackColor = Color.Transparent;

                if (i == 0)
                {
                    lbl.Location = new Point(0, 0);
                }

                labels.Add(lbl);
                container.Controls.Add(lbl);
            }

            content = words;
        }

        private void ArrangeEvent(object sender, EventArgs e)
        {
            for (int i = 0; i < container.Controls.Count; i++)
            {
                var lbl = container.Controls[i];

                if (i == 0)
                {
                    lbl.Location = new Point(0, 0);
                }
                else
                {
                    lbl.Location = new Point(labels[i - 1].Right - 3, 0);
                }
            }

            container.Width = container.Controls[container.Controls.Count - 1].Right;
            container.Height = container.Controls[container.Controls.Count - 1].Height;
        }
    }
}
