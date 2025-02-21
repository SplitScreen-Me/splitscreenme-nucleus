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
        //always a square 
        private PictureBox imageBox;
        private Color backColor = Theme_Settings.BackgroundGradientColor;
        public SearchTextBox()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |ControlStyles.UserPaint, true);


            BackColor = Color.FromArgb(255, 31, 34, 35); //Color.FromArgb(255, backColor.R, backColor.G, backColor.B); //
            AutoSize = true;
            Resize += SearchText_Resize;
            Paint += _Paint;
            Click += Focus_TextBox;
            ControlAdded += Control_Added;

            CreateControls();

            DPIManager.Register(this);
        }

        private void CreateControls()
        {
            imageBox = new PictureBox();
            Controls.Add(imageBox);

            imageBox.Image = ImageCache.GetImage(Globals.ThemeFolder + "magnifier.png"); //temp new Bitmap(ImagePath);
            imageBox.SizeMode = PictureBoxSizeMode.StretchImage;
            imageBox.Anchor = AnchorStyles.Top | AnchorStyles.Left/*| AnchorStyles.Right*/;
            imageBox.Margin = new Padding(0, 0, 0, 0);
            imageBox.BackColor = Color.FromArgb(255, 31, 34, 35); //Color.FromArgb(255, backColor.R, backColor.G, backColor.B);
            SearchText = new FlatTextBox();
            Controls.Add(SearchText);

            SearchText.Font = Font;// new Font(Font.FontFamily, 12f * Interface.ScalingRatio, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            SearchText.ForeColor = Color.White;
            SearchText.BackColor = Color.FromArgb(255, 31, 34, 35); //Color.FromArgb(255, backColor.R, backColor.G, backColor.B);
            SearchText.BorderStyle = BorderStyle.None;
            SearchText.Paint += SearchText_Paint;
            SearchText.Hint = "Search By Name";
        }

        private void sr(object sender, EventArgs e)
        {
           // Invalidate();
        }

        private void SearchText_Resize(object sender, EventArgs e)
        {
            //FlatTextBox search = sender as FlatTextBox;
            imageBox.Size = new Size(Height, Height);
            imageBox.Location = new Point(0, 0);
            SearchText.Size = new Size((Width - imageBox.Width) - 40, 20);
            SearchText.Location = new Point(imageBox.Right + 10, imageBox.Height/2 - SearchText.Height/2);
            //FormGraphicsUtil.CreateRoundedControlRegion(SearchText, 1, 1, SearchText.Width - 1, SearchText.Height - 1, 10, 10);
            //SearchText.Invalidate();
            //Height = SearchText.Height;
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            SearchText.Font = new Font(SearchText.Font.FontFamily, 9 * scale, SearchText.Font.Style);
        }

        private void SearchText_Paint(object sender, PaintEventArgs e)
        {
            FlatTextBox search = sender as FlatTextBox;

        }

        private void _Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //Color color = Color.FromArgb(255, 31, 34, 35);
            //Rectangle selectedBorder = new Rectangle(Left, SearchText.Top,
            //               Right, Height);

            //SolidBrush border = new SolidBrush(Color.FromArgb(255, color.R, color.G, color.B));

            //GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(selectedBorder, 15, 15, true, true, true, true);

            //e.Graphics.FillPath(border, backGp);

            //border.Dispose();
            //backGp.Dispose();
        }

        private void Focus_TextBox(object sender, EventArgs e)
        {
            SearchText.Focus();
        }

        private void Control_Added(object sender, ControlEventArgs e)
        {
            if(e.Control != SearchText)
               e.Control.Click += Focus_TextBox;
                  // c.GotFocus += Focus_TextBox;

        }

    }

}

