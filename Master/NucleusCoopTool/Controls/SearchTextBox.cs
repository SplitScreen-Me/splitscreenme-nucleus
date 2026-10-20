using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public partial class SearchTextBox : UserControl, IDynamicSized
    {
        public string ImagePath { get; set; }
        public FlatTextBox SearchText { get; private set; }

        public Action<bool> Toggle_Visiblility;
        public string Hint => SearchText.Hint;
        private float _scale;
        private float scale
        {
            get => _scale;
            set
            {
                if(value < 1)
                {
                    _scale = 1;
                }
                _scale = value < 1 ? 1 : value;

                On_Resize(null, null);
            }
        }

        private PictureBox imageBox;

        public SearchTextBox()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |ControlStyles.UserPaint, true);

            BackColor = Color.FromArgb(255, 41, 44,45);
            AutoSize = true;
            Resize += SearchText_Resize;
            Click += Focus_TextBox;
            ControlAdded += Control_Added;
            VisibleChanged += On_VisibleChanged;
            MouseDown += SearchText_MouseDown;
            Resize += On_Resize;
            CreateControls();

            DPIManager.Register(this);
        }

        public void VirtualClick()
        {
            OnClick(EventArgs.Empty);
            SearchText.ForeColor = Color.White;
            SearchText.Text = "";
        }

        private void CreateControls()
        {
            imageBox = new PictureBox();
            Controls.Add(imageBox);

            imageBox.Image = ImageCache.GetImage(Globals.ThemeFolder + "magnifier.png");
            imageBox.SizeMode = PictureBoxSizeMode.StretchImage;
            imageBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            imageBox.Margin = new Padding(0, 0, 0, 0);
            imageBox.BackColor = BackColor;
            imageBox.MouseDown += SearchText_MouseDown;
            SearchText = new FlatTextBox();
            Controls.Add(SearchText);

            SearchText.Font = Font;
            SearchText.ForeColor = Color.Gray;
            SearchText.BackColor =  BackColor;
            SearchText.BorderStyle = BorderStyle.None;
            SearchText.Hint = "Search By Name";
            SearchText.Text = SearchText.Hint;
            SearchText.Font = new Font(SearchText.Font.FontFamily,  8f , SearchText.Font.Style);
            SearchText.MouseDown += SearchText_MouseDown;
            SearchText.LostFocus += SearchText_LostFocus;
            SearchText.TextChanged += SearchText_TextChanged;
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            FlatTextBox textbox = sender as FlatTextBox;
            if(textbox.Text == textbox.Hint)
            SearchText.ForeColor = Color.Gray;
        }

        private void SearchText_LostFocus(object sender,EventArgs e)
        {
            SearchText.ForeColor = Color.Gray;
            SearchText.Text = SearchText.Hint;
        }

        private void SearchText_MouseDown(object sender ,MouseEventArgs e)
        {
            SearchText.ForeColor = Color.White;
            SearchText.Text = "";
        }

        private void SearchText_Resize(object sender, EventArgs e)
        { 
            imageBox.Size = new Size(Height - 3 , Height -3);
            imageBox.Location = new Point(2, 1);
            SearchText.Size =  new Size((Width - imageBox.Width) - 20, Height);
            SearchText.Location = new Point(imageBox.Right + 10,5);
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            this.scale = scale;
            SearchText.Font = new Font(SearchText.Font.FontFamily, 8f * scale, SearchText.Font.Style);
        }

        private void On_Resize(object sender, EventArgs e)
        {
            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, this.Width, this.Height, (int)(15 * scale), (int)(15 * scale));
        }

        private void Focus_TextBox(object sender, EventArgs e)
        {
            SearchText.Focus();
        }

        private void Control_Added(object sender, ControlEventArgs e)
        {
            if(e.Control != SearchText)
               e.Control.Click += Focus_TextBox;
        }

        private void On_VisibleChanged(object sender, object e)
        {
            Toggle_Visiblility?.Invoke(Visible);
        }
    }

}

