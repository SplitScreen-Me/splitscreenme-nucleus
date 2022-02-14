using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    [DefaultEvent("Click")]
    public partial class PictureButton : UserControl
    {
        public Image Image
        {
            get => button_Picture.Image;
            set => button_Picture.Image = value;
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => button_Btn.Text;
            set => button_Btn.Text = value;
        }

        public PictureButton()
        {
            InitializeComponent();
        }

        public Button PictureBtn => button_Picture;

        private void button2_Click(object sender, EventArgs e)
        {
            OnClick(null);
        }
    }
}
