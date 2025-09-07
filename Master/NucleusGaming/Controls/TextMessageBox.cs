using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public partial class TextMessageBox : Form
    {
        public string UserText => textBox1.Text;

        public TextMessageBox()
        {
            InitializeComponent();
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
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
