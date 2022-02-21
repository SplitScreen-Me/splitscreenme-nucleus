namespace Nucleus.Gaming
{
    public class IniSaveInfo : SaveInfo
    {
        public string Section;
        public string Key;
        public string Value;

        public IniSaveInfo(string section, string key, string value)
        {
            Section = section;
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return "[" + Section + "]" + Key + "=" + Value;
        }
    }
}
