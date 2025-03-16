using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{

    public partial class TransparentRichTextBox : RichTextBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                //This makes the control's background transparent
                CreateParams CP = base.CreateParams;
                CP.ExStyle |= 0x20;
                return CP;
            }
        }

        private string linkColor = $"\\red{Color.Aqua.R}\\green{Color.Aqua.G}\\blue{Color.Aqua.B};";

        public TransparentRichTextBox()
        {
            InitializeComponent();
            ContentsResized += OnTextChanged;
        }

        private void OnTextChanged(object sender, object e)
        {       
            try
            {
                string shorten = null;
                int shStart = Text.IndexOf('[');
                int shEnd = Text.IndexOf(']');

                if (shStart != -1 && shEnd != -1)
                {
                    shorten = Text.Substring(shStart + 1, (shEnd - shStart) - 1);
                }
                else
                {
                    Rtf = Rtf.Replace("\\red0\\green0\\blue255;", linkColor); //Go back to blue without this here (unreadable).
                    ScrollToCaret();//else it won't scroll anymore
                    return;
                }

                string link = null;

                //search and build the link from value
                int linkStart = Text.IndexOf('{');
                int linkEnd = Text.IndexOf('}');

                if (linkStart != -1 && linkEnd != -1)
                {
                    link = Text.Substring(linkStart + 1, (linkEnd - linkStart) - 1);
                }

                if (link != null)
                {
                    string rtfFormated = FormatRtf(link, shorten);

                    if (rtfFormated != Rtf)
                    {                       
                        Rtf = rtfFormated;
                    }
                }
            }
            catch
            { }
        }

        private string FormatRtf(string hyperlink, string shorten)
        {
            try
            {
                int defLenght = Rtf.Length;

                int sstart = Rtf.IndexOf("[");
                int slength = Rtf.LastIndexOf("}");

                var sresult = Rtf.Substring(sstart, slength - sstart);
                var cleanResult = sresult.Remove(sresult.LastIndexOf("}") + 1 );

                var replaceTemplate = "{\\field{\\*\\fldinst HYPERLINK \"" + hyperlink + "\"}{\\fldrslt " + shorten + " }}";

                string newRtf = Rtf.Replace(cleanResult, replaceTemplate);

                int diffLenght = defLenght - newRtf.Length;

                string newStr = string.Empty;

                int insertIndex = newRtf.LastIndexOf("\\");

                for (int i = 0; i < diffLenght; i++)//Need to recreate the og lenght or the link won't work when clicked.
                {
                     newStr += " ";            
                }

                newRtf = newRtf.Insert(insertIndex, newStr);

                return newRtf.Replace("\\red0\\green0\\blue255;", linkColor); 
            }
            catch
            {
                return Rtf;
            }
        }
     
    }
}
