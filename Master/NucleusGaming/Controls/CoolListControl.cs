using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nucleus.Gaming;

namespace SplitTool.Controls
{
    public class CoolListControl : UserControl
    {
        private Label titleLabel;
        protected Label descLabel;

        protected int defaultHeight = 72;
        protected int expandedHeight = 156;

        /* public Font TitleFont
        {
            get { return titleLabel.Font; }
            set { titleLabel.Font = value; }
        }
        public Font DetailsFont
        {
            get { return descLabel.Font; }
            set { descLabel.Font = value; }
        } */

        public string Title
        {
            get { return titleLabel.Text; }
            set { this.titleLabel.Text = value; }
        }

        public string Details
        {
            get { return descLabel.Text; }
            set { descLabel.Text = value; }
        }

        public bool EnableHighlighting { get; private set; }
        public object Data { get; set; }
        public event Action<object> OnSelected;

       public CoolListControl(bool enableHightlighting)
        {
            EnableHighlighting = enableHightlighting;

            Size = new Size(400, 120);
            BackColor = Color.FromArgb(30, 30, 30);

            titleLabel = new Label();
            titleLabel.Location = new Point(10, 10);
            titleLabel.AutoSize = true;
            Controls.Add(titleLabel);

            descLabel = new Label();
            descLabel.Location = new Point(10, 50);
            descLabel.AutoSize = true;
            Controls.Add(descLabel);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            Control c = e.Control;
            c.Click += C_Click;

            if (EnableHighlighting)
            {
                c.MouseEnter += C_MouseEnter;
                c.MouseLeave += C_MouseLeave;
            }
        }

        private void C_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void C_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void C_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            BackColor = Color.FromArgb(30, 30, 30);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!ContainsFocus)
            {
                BackColor = Color.FromArgb(60, 60, 60);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!ContainsFocus)
            {
                BackColor = Color.FromArgb(30, 30, 30);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            BackColor = Color.FromArgb(80, 80, 80);
            if (OnSelected != null)
            {
                OnSelected(Data);
            }
        }
    }
}
