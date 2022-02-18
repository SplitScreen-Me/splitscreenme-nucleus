using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class ControlListBox : UserControl
    {
        private int totalHeight;
        private int border = 1;

        public event Action<object, Control> SelectedChanged;
        public Size Offset { get; set; }
        public Control SelectedControl { get; protected set; }

        public int Border
        {
            get => border;
            set => border = value;
        }

        public ControlListBox()
        {
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            HorizontalScroll.Maximum = 0;
            VerticalScroll.Visible = false;
            AutoScroll = true;
            DoubleBuffered = true;
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        public override bool AutoScroll
        {
            get => base.AutoScroll;
            set
            {
                base.AutoScroll = value;
                if (!value)
                {
                    HorizontalScroll.Visible = false;
                    HorizontalScroll.Enabled = false;
                    VerticalScroll.Visible = false;
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateSizes();
        }

        private bool updatingSize;
        public void UpdateSizes()
        {
            if (updatingSize)
            {
                return;
            }
            VerticalScroll.Value = 0;//avoid weird glitchs if scrolled before maximizing the main window.
            updatingSize = true;

            totalHeight = 0;
            bool isVerticalVisible = VerticalScroll.Visible;
            int v = isVerticalVisible ? (1 + SystemInformation.VerticalScrollBarWidth) : 0;


            for (int i = 0; i < Controls.Count; i++)
            {
                Control con = Controls[i];
                con.Width = Width - v;

                con.Location = new Point(0, totalHeight);
                totalHeight += con.Height + border;

                con.Invalidate();
            }

            updatingSize = false;

            HorizontalScroll.Visible = false;
            VerticalScroll.Visible = totalHeight > Height;
            VerticalScroll.Value = 0;//avoid weird glitchs if scrolled before maximizing the main window.
            if (VerticalScroll.Visible != isVerticalVisible)
            {
                UpdateSizes(); // need to update again
                VerticalScroll.Value = 0;//avoid weird glitchs if scrolled before maximizing the main window.
            }

        }

        private void C_SizeChanged(object sender, EventArgs e)
        {
            Control con = (Control)sender;
            // this has the potential of being incredibly slow
            UpdateSizes();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (!DesignMode && e.Control != null)
            {
                Control c = e.Control;

                c.ControlAdded += C_ControlAdded;
                c.Click += c_Click;
                c.SizeChanged += C_SizeChanged;
                if (c is IRadioControl)
                {
                    c.MouseEnter += c_MouseEnter;
                    c.MouseLeave += c_MouseLeave;
                }

                int index = Controls.IndexOf(c);
                Size s = c.Size;

                c.Location = new Point(0, totalHeight);
                totalHeight += s.Height + border;
            }
        }



        private void C_ControlAdded(object sender, ControlEventArgs e)
        {
            Control c = e.Control;
            c.Click += c_Click;
            c.MouseEnter += c_MouseEnter;
            c.MouseLeave += c_MouseLeave;
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            UpdateSizes();
        }

        public void Deselect()
        {
            SelectedControl = null;
            c_Click(this, EventArgs.Empty);
        }

        private void c_MouseEnter(object sender, EventArgs e)
        {
            Control parent = (Control)sender;

            if (parent != SelectedControl && parent is IRadioControl)
            {
                IRadioControl high = (IRadioControl)parent;
                high.UserOver();
            }
        }

        private void c_MouseLeave(object sender, EventArgs e)
        {
            Control parent = (Control)sender;

            if (parent != SelectedControl && parent is IRadioControl)
            {
                IRadioControl high = (IRadioControl)parent;
                high.UserLeave();
            }
        }

        private void c_Click(object sender, EventArgs e)
        {
            Control parent = (Control)sender;

            for (int i = 0; i < Controls.Count; i++)
            {
                Control c = Controls[i];
                if (c is IRadioControl)
                {
                    IRadioControl high = (IRadioControl)c;
                    if (parent == c)
                    {
                        // highlight
                        high.RadioSelected();
                    }
                    else
                    {
                        high.RadioUnselected();
                    }
                }
            }

            if (parent != null &&
                parent != SelectedControl)
            {
                if (SelectedChanged != null)
                {
                    SelectedControl = parent;
                    SelectedChanged(SelectedControl, this);
                }
            }

            SelectedControl = parent;

            OnClick(e);
        }

    }
}
