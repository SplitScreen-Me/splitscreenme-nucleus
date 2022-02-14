using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{
    public class NButton : Button
    {
        protected Color defaultForeColor = Color.Black;
        protected Color disabledForeColor = Color.Black;
        public Color DefaultForeColor
        {
            get => defaultForeColor;
            set => defaultForeColor = value;
        }
        public Color DisabledForeColor
        {
            get => disabledForeColor;
            set => disabledForeColor = value;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (base.Enabled)
            {
                base.OnPaint(pevent);
                base.ForeColor = defaultForeColor;
            }
            else
            {
                base.OnPaint(pevent);
                SizeF sf = pevent.Graphics.MeasureString(Text, Font, Width);
                Point ThePoint = new Point
                {
                    X = (int)((Width / 2) - (sf.Width / 2)),
                    Y = (int)((Height / 2) - (sf.Height / 2))
                };
                Brush brush = new SolidBrush(disabledForeColor);
                pevent.Graphics.DrawString(Text, Font, brush, ThePoint);
                brush.Dispose();
            }
        }
    }
}
