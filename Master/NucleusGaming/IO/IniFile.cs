using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, uint size, string filePath);

        private string path;

        public string Path => path;

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Section name
        /// <PARAM name="Key"></PARAM>
        /// Key Name
        /// <PARAM name="Value"></PARAM>
        /// Value Name
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            255, path);
            return temp.ToString();

        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public void IniAddKeyValueToNewSection(Dictionary<string, string[]> newSectionValues)
        {
            if (File.Exists(path))
            {
                // string[] rawIniContent = File.ReadAllLines(filePath);

                List<string> iniContent = new List<string>(File.ReadAllLines(path));

                List<string> newContent = new List<string>();

                foreach (KeyValuePair<string, string[]> kv in newSectionValues)
                {
                    string sectionHeader = "[" + kv.Key + "]";

                    if (!iniContent.Contains(sectionHeader))
                    {
                        newContent.Add("[" + kv.Key + "]");
                        foreach (string value in kv.Value)
                        {
                            newContent.Add(value);
                        }
                    }

                }

                iniContent.AddRange(newContent);
                File.WriteAllLines(path, iniContent);
            }

        }

        public void IniAddKeyValuesToSection(string section, string[] newValues)
        {
            if (File.Exists(path))
            {
                List<string> iniContent = new List<string>(File.ReadAllLines(path));

                List<string> newContent = new List<string>();

                for (int i = 0; i < iniContent.Count; i++)
                {
                    string lineCheck = iniContent[i];

                    if (lineCheck == $"[{section}]")
                    {
                       
                        for (int j = 0; j < newValues.Length; j++)
                        {
                            string value = newValues[j];

                            var t = string.IsNullOrEmpty(IniReadValue(section, value));

                            if (t)
                            {
                                iniContent.Insert(i + (j + 1), value);
                            }
                                
                        }
                    }
                }

                File.WriteAllLines(path, iniContent);
            }
        }
    }
}