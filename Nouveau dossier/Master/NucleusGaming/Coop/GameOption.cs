using System.Collections;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// A custom game option that can be modified by the user
    /// </summary>
    public class GameOption
    {
        public object DefaultValue { get; set; }
        private string name;
        private string description;
        private object value;
        private string key;
        private IList list;

        /// <summary>
        /// The name of the variable
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The description of the variable
        /// </summary>
        public string Description => description;

        /// <summary>
        /// The value of the variable
        /// </summary>
        public object Value
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// The key to this variable
        /// </summary>
        public string Key => key;

        public IList List => list;
        public bool Hidden { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public GameOption(string name, string desc, string key, object value)
        {
            this.name = name;
            description = desc;
            this.value = value;
            this.key = key;
            if (value is IList)
            {
                list = (IList)value;
                this.value = 0;
            }
        }

        public GameOption(string name, string desc, string key, object value, object defaultValue)
        {
            DefaultValue = defaultValue;
            this.name = name;
            description = desc;
            this.value = value;
            this.key = key;
            if (value is IList)
            {
                list = (IList)value;
                this.value = 0;
            }
        }

        public GameOption Instantiate()
        {
            return new GameOption(Name, Description, Key, Value);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Key, Value);
        }
    }
}
