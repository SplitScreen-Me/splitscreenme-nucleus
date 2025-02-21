using Nucleus.Gaming;
using Nucleus.Gaming.Windows;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public class CustomCheckBox : CheckBox
    {
        private Color mBorderColor = Color.White;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Border Color")]
        public Color BorderColor
        {
            get { return mBorderColor; }
            set
            {
                if (mBorderColor != value)
                {
                    mBorderColor = value;
                    Invalidate();
                }
            }
        }

        private Color mCheckColor = Color.White;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Check Color")]
        public Color CheckColor
        {
            get { return mCheckColor; }
            set
            {
                if (mCheckColor != value)
                {
                    mCheckColor = value;
                    Invalidate();
                }
            }
        }

        private Color mSelectionColor = Color.Green;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Selection Color")]
        public Color SelectionColor
        {
            get { return mSelectionColor; }
            set
            {
                if (mSelectionColor != value)
                {
                    mSelectionColor = value;
                    Invalidate();
                }
            }
        }

        private float _scale => (float)User32Util.GetDpiForWindow(this.Handle) / (float)100;

        public CustomCheckBox() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = Color.DimGray;
            ForeColor = Color.White;

            CheckedChanged += Checked_Changed;
        }

        private void Checked_Changed(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            SolidBrush brush = new SolidBrush(Checked ? CheckColor : SelectionColor);

            SizeF size = e.Graphics.MeasureString(Text, Font);
            float locY = (((float)Height / 2) - (size.Height * _scale) / 2);

            float ratio = Height / (size.Height * _scale);
            RectangleF tic = new RectangleF(0, locY - ratio, size.Height * _scale, size.Height * _scale);

            Pen outline = new Pen(BorderColor);
            e.Graphics.FillRectangle(brush, tic);
            e.Graphics.DrawRectangles(outline, new RectangleF[] { tic });

            brush.Dispose();
            outline.Dispose();
        }

    }

}