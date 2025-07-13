using Nucleus.Coop.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    public class HighlightNotesText
    {
        private static Color defColor = Color.GreenYellow;

        public static void Highlight(RichTextBox textbox)
        {
            var text = textbox.Text;

            string pattern = @"\[\<([^\[\]\<\>]+)\>\]";
            var matches = Regex.Matches(text, pattern);

            Dictionary<string,Color> parsedDatas = new Dictionary<string,Color>();

            int index = 0;

            foreach (var match in matches)
            {
                var datas = ParseTextDatas(match.ToString());

                if(parsedDatas.Keys.Contains(datas.Item1))
                {
                    index++;
                    continue;
                }

                parsedDatas.Add(datas.Item1,datas.Item2);
                textbox.Text = textbox.Text.Replace(matches[index].ToString(), datas.Item1);
                index++;
            }

            foreach (KeyValuePair<string,Color> keyValue in parsedDatas)
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

            for(int i = 0; i < splitted.Length ;i++)
            {
                var s = splitted[i];
                if (s.StartsWith("[<"))
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
 
