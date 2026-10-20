using Nucleus.Gaming.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Nucleus.Gaming.Controls
{
    public partial class CustomNumericUpDown : UserControl, IDynamicSized
    {
        private Rectangle upBounds;
        private Rectangle downBounds;

        public int MaxValue = 999;
        public int MinValue = 0;
        public bool InvalidParent;

        private int _value = 0;
        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                val.Text = _value.ToString();
            }
        }

        private Color mUpdown = Color.Gray;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Updown Button BackColor")]
        public Color UpdownBackColor
        {
            get { return mUpdown; }
            set
            {
                if (mUpdown != value)
                {
                    mUpdown = value;
                    Invalidate();
                }
            }
        }

        public CustomNumericUpDown()
        {
            InitializeComponent();
            BackColor = Color.Transparent;
            up.BackColor = Color.Transparent;
            down.BackColor = Color.Transparent;
            val.BackColor = Color.Transparent;
            DPIManager.Register(this);
        }

        private void up_Click(object sender, EventArgs e)
        {
            if (_value == MaxValue)
            {
                return;
            }

            _value++;
            val.Text = _value.ToString();

            if (InvalidParent)
                Parent.Invalidate();
        }

        private void down_Click(object sender, EventArgs e)
        {
            if (_value == MinValue)
            {
                return;
            }

            _value--;
            val.Text = _value.ToString();

            if (InvalidParent)
                Parent.Invalidate();
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            Font = new Font(Font.FontFamily, 9.25f * scale, Font.Style, GraphicsUnit.Pixel, 0);
            upBounds = up.Bounds;
            downBounds = down.Bounds;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (upBounds == null || downBounds == null || upBounds == RectangleF.Empty || downBounds == Rectangle.Empty)
            {
                return;
            }

            Cursor = upBounds.Contains(e.Location) || downBounds.Contains(e.Location) ? Cursor = Theme_Settings.Hand_Cursor : Theme_Settings.Default_Cursor;
        }

        private void val_TextChanged(object sender, EventArgs e)
        {
            if (Parent != null)
                if (InvalidParent)
                    Parent.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            
            Pen pen = new Pen(Color.White);

            Rectangle rec = new Rectangle(down.Left, up.Top, down.Width, down.Bottom);
            Rectangle border = new Rectangle(0, 0, down.Right-1 , down.Bottom);

            SolidBrush br = new SolidBrush(Color.FromArgb(255, 31, 34, 35));
            e.Graphics.FillRectangle(br, border);

            br = new SolidBrush(UpdownBackColor);
            e.Graphics.FillRectangle(br,rec);

            e.Graphics.DrawRectangle(pen, border);

            br.Dispose();
            pen.Dispose();
        }
    }
}
