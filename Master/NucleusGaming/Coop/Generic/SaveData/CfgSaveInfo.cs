namespace Nucleus.Gaming
{
    public class CfgSaveInfo : SaveInfo
    {
        public string Section;
        public string Key;
        public string Value;

        public CfgSaveInfo(string section, string key, string value)
        {
            Section = section;
            Key = key;
            Value = value;
        }
    }
}
