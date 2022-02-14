using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{
    // From http://stackoverflow.com/questions/3166244/how-to-increase-the-size-of-checkbox-in-winforms
    public class SizeableCheckbox : CheckBox
    {
        public SizeableCheckbox()
        {
            TextAlign = ContentAlignment.MiddleRight;
        }
        public override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = false;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int h = ClientSize.Height - 2;
            Rectangle rc = new Rectangle(new Point(0, 1), new Size(h, h));
            ControlPaint.DrawCheckBox(e.Graphics, rc,
                Checked ? ButtonState.Checked : ButtonState.Normal);
        }
    }
}