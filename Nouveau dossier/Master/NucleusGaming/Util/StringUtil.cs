using System;
using System.Drawing;
using System.Linq;

namespace Nucleus.Gaming
{
    public static class StringUtil
    {
        public static readonly char[] Numbers = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static bool IsNumber(char c)
        {
            // We COULD use Int.TryParse, but this looks way cleaner
            return Numbers.Contains(c);
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        /// <summary>
        /// This method can be made better
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="str"></param>
        /// <param name="graphics"></param>
        /// <param name="font"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string WrapString(float maxWidth, string str, Graphics graphics, Font font, out SizeF size)
        {
            size = graphics.MeasureString(str, font);

            string[] sep = str.Split(' ');
            if (sep.Length == 0)
            {
                return str;
            }

            float spaceSize = graphics.MeasureString(" ", font).Width;

            string result = sep[0];

            float currentWidth = graphics.MeasureString(result, font).Width;
            float maxUsedWidth = 0;
            int lines = 1;
            string currentLine = result;

            for (int i = 1; i < sep.Length; i++)
            {
                string word = sep[i];
                string spaceWord = " " + word;
                SizeF wordSize = graphics.MeasureString(spaceWord, font);
                currentWidth += wordSize.Width;

                if (currentWidth > maxWidth)
                {
                    currentWidth = wordSize.Width;
                    maxUsedWidth = Math.Max(maxUsedWidth, graphics.MeasureString(currentLine, font).Width);
                    result += "\n" + word;
                    currentLine = "";
                    lines++;
                }
                else
                {
                    result += spaceWord;
                    currentLine += spaceWord;
                }
            }

            if (maxUsedWidth == 0)
            {
                maxUsedWidth = Math.Max(maxUsedWidth, graphics.MeasureString(currentLine, font).Width);
            }
            size = new SizeF(maxUsedWidth, lines * font.GetHeight(graphics));

            return result;
        }
    }
}
