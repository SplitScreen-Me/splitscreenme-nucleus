using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{
    public partial class HandlerNotesZoom : DoubleBufferPanel, IDynamicSized
    {
        public TransparentRichTextBox Notes => TextBox;
        public Font DefaultNotesFont;
        private Pen linePen;

        public HandlerNotesZoom()
        {         
            InitializeComponent();

            ForeColor = Theme_Settings.HandlerNoteForeColor;

            BackColor = Theme_Settings.HandlerNoteBackColor;

            close_Btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
            close_Btn.BackColor = Color.Transparent;
            close_Btn.Cursor = new Cursor(Globals.ThemeFolder + "cursor_hand.ico");

            linePen = new Pen(TextBox.ForeColor, 1);

            TextBox.DetectUrls = true;
            TextBox.Width = Width - 10;

            MouseDown += HandlerNotesZoom_MouseDown;
            TextBox.Resize += OnResize;

            DPIManager.Register(this);
        }

        private void TextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(e.LinkText);
            }
            catch
            { }
        }

        private void Close_Btn_Click(object sender, EventArgs e)
        {
            Visible = false;
        }

        private void Close_Btn_MouseEnter(object sender, EventArgs e)
        {
            close_Btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        }

        private void Close_Btn_MouseLeave(object sender, EventArgs e)
        {
            close_Btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
        }

        public void UpdateSize(float scale)
        {
            close_Btn.Size = new Size((int)(20 * scale), (int)(20 * scale));
            close_Btn.Location = new Point(Width / 2 - (close_Btn.Width / 2), (Height - close_Btn.Height) - 10);
            Warning.Height = (int)(Warning.Height * scale);
            TextBox.Height -= Warning.Height + close_Btn.Height;
            TextBox.Location = new Point(0, Warning.Bottom + 10);
            TextBox.Font = new Font(Theme_Settings.CustomFont, 18f * scale, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            DefaultNotesFont = TextBox.Font;
        }

        private void OnResize(object sender, EventArgs e)
        {
            close_Btn.Location = new Point(Width / 2 - (close_Btn.Width / 2),TextBox.Bottom + 30);
            TextBox.Width = Width - 10;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void HandlerNotesZoom_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, "Nucleus Co-op");
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void HandlerNotesZoom_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            e.Graphics.DrawEllipse(linePen, close_Btn.Location.X - 6, close_Btn.Location.Y - 6, close_Btn.Width + 12, close_Btn.Height + 12);            
        }
    }
}
