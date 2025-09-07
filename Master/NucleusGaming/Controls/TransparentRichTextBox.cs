using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
                Highlight(this);

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
                var cleanResult = sresult.Remove(sresult.LastIndexOf("}") + 1);

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

        private static Color defColor = Color.GreenYellow;

        public static void Highlight(RichTextBox textbox)
        {
            var text = textbox.Rtf;

            string pattern = @"\*\<([^\[\]\<\>]+)\>\*";
            var matches = Regex.Matches(text, pattern);

            Dictionary<string, Color> parsedDatas = new Dictionary<string, Color>();

            int index = 0;

            foreach (var match in matches)
            {
                var datas = ParseTextDatas(match.ToString());

                if (parsedDatas.Keys.Contains(datas.Item1))
                {
                    index++;
                    continue;
                }

                parsedDatas.Add(datas.Item1, datas.Item2);
                textbox.Rtf = textbox.Rtf.Replace(matches[index].ToString(), datas.Item1);
                index++;
            }

            foreach (KeyValuePair<string, Color> keyValue in parsedDatas)
            {
                int startIndex = 0;
                while (startIndex < textbox.TextLength)
                {
                    int foundIndex = textbox.Find(keyValue.Key, startIndex, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                    if (foundIndex == -1) break;

                    textbox.Select(foundIndex, keyValue.Key.Length);
                    textbox.SelectionColor = keyValue.Value;

                    startIndex = foundIndex + keyValue.Key.Length;
                }
            }
        }

        private static (string, Color) ParseTextDatas(string datas)
        {
            string[] splitted = datas.Split('|');

            List<string> cleaned = new List<string>();

            for (int i = 0; i < splitted.Length; i++)
            {
                var s = splitted[i];
                if (s.StartsWith("*<"))
                {
                    s = s.Remove(0, 2);
                }
                else
                {
                    s = s.Remove(s.Length - 2, 2);
                }

                cleaned.Add(s);
            }

            if (cleaned.Count == 1)
            {
                return (cleaned[0], defColor);
            }
            else if (splitted.Length == 2)
            {
                return (cleaned[0], ParseColor(cleaned[1]));
            }

            return ("", defColor);
        }

        private static Color ParseColor(string colorText)
        {
            var colorArray = colorText.Split(',');
            int r = int.Parse(colorArray[0]); int g = int.Parse(colorArray[1]); int b = int.Parse(colorArray[2]);

            if (colorArray.Length == 3)
            {
                return Color.FromArgb(255, r < 0 || r > 255 ? 255 : r,
                                           g < 0 || g > 255 ? 255 : g,
                                           b < 0 || b > 255 ? 255 : b);
            }
            else
            {
                return defColor;
            }
        }

    }

}
