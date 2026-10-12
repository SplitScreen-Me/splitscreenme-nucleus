using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Documents;

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

        public static string GetRootFromBinariesFolder(string gameExePath, string binariesToRemove)
        {
            string[] exeSubStrings = gameExePath.Split('\\');
            string binariesStart = binariesToRemove.Split('\\')[0].ToLower();

            string end = string.Empty;

            for (int i = 0; i < exeSubStrings.Count(); i++)
            {
                string sub = exeSubStrings[i].ToLower();

                if (sub == binariesStart)
                {
                    break;
                }

                if (i < exeSubStrings.Count() - 1)
                {
                    end += sub + "\\";
                    continue;
                }

                end += sub;
            }

            if(end != string.Empty)
            {
                return end;
            }

            return gameExePath;
        }

        public static string ReplaceCaseInsensitive(string str, string toFind, string toReplace)
        {
            string lowerOriginal = str.ToLower();
            string lowerFind = toFind.ToLower();
            string lowerRep = toReplace.ToLower();

            int start = lowerOriginal.IndexOf(lowerFind);
            if (start == -1)
            {
                return str;
            }

            string end = str.Remove(start, toFind.Length);
            end = end.Insert(start, toReplace);

            return end;
        }


        public static string InternalGetRelativePath(DirectoryInfo dirInfo, DirectoryInfo rootInfo, string str)
        {
            if (dirInfo == null || dirInfo.FullName == rootInfo.FullName)
            {
                return str;
            }

            if (!string.IsNullOrWhiteSpace(Path.GetExtension(dirInfo.Name)))
            {
                str = dirInfo.Name;
            }
            else
            {
                str = dirInfo.Name + "\\" + str;
            }

            dirInfo = dirInfo.Parent;
            return InternalGetRelativePath(dirInfo, rootInfo, str);
        }
    }
}
