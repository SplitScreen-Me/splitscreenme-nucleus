using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public partial class TextMessageBox : Form
    {
        public string UserText => textBox1.Text;
        private readonly IniFile ini = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        public TextMessageBox()
        {
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));
 
            InitializeComponent();
            BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\Theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }
    }
}
