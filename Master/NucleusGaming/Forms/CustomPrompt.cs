using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    public partial class CustomPrompt : Form
    {
        private int index;
        private object ini;
        private readonly IniFile custom = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        public CustomPrompt(string message, string prevAnswer, int i)
        {
            string ChoosenTheme = custom.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));
          
            BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\Theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            InitializeComponent();
            lbl_Desc.Text = message;
            txt_UserInput.Text = prevAnswer;

            index = i;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            GenericGameHandler.customValue[index] = txt_UserInput.Text;
            Close();
        }
    }
}
