using Nucleus.Gaming.Cache;
using Nucleus.Gaming.DPI;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    public partial class CustomMessageBox : Form
    {
        private string message;
        private bool sized;
        private Font font;

        public CustomMessageBox(string title, string message, bool format)
        {
            string[] rgb_MouseOverColor = Globals.ThemeConfigFile.IniReadValue("Colors", "MouseOver").Split(',');

            InitializeComponent();

            Cursor = Theme_Settings.Default_Cursor;
            TopMost = true;
            Text = title;

            this.message = format ? FormatText(message) : message;

            closeBtn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
            closeBtn.Cursor = Theme_Settings.Hand_Cursor;

            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(int.Parse(rgb_MouseOverColor[0]), int.Parse(rgb_MouseOverColor[1]), int.Parse(rgb_MouseOverColor[2]), int.Parse(rgb_MouseOverColor[3]));
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            Opacity = 0;//invisible until resized

            CreateHandle();
        }

        private string FormatText(string message)
        {
            string removedNewLineJump = message;
            string[] words = removedNewLineJump.Split(' ');
            string formated = string.Empty;

            int wordsBeforeNewLine = 10;
            int start = wordsBeforeNewLine;

            for (int i = 0; i < words.Length; i++)
            {
                if (start == i)
                {
                    formated += words[i] + '\n';
                    start += wordsBeforeNewLine;
                }
                else
                {
                    if (words[i].EndsWith("."))
                    {
                        formated += words[i] + '\n';
                        start += wordsBeforeNewLine;
                    }
                    else
                    {
                        formated += words[i] + ' ';
                    }
                }
            }

            return formated;
        }

        private void CustomMessaBox_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            float scale = DPIManager.Scale;
            font = new Font(Font.FontFamily, Font.Size * scale);
            
            SizeF newSize = g.MeasureString(message, font);

            if (!sized)
            {
                Size = new Size((int)newSize.Width + 30, (int)newSize.Height + 30 + closeBtn.Bottom + 5);
                MaximumSize = Size;
               
                if (Theme_Settings.WindowRoundedCorners)
                {
                    FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 20, 20);
                }

                CenterToScreen();
                Opacity = 1.0F;

                sized = true;
            }

            g.DrawString(message, font, Brushes.White, 10, closeBtn.Bottom + 5);
            g.Dispose();
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void CustomMessageBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }
    }
}
