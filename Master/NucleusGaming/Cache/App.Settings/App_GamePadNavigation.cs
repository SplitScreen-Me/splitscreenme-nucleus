using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Gaming.App.Settings
{
    /// <summary>
    /// The <c>[XUINav]</c> section of Settings.ini. Only <c>Type</c> is still used (button names and pictures of the
    /// gamepad shortcuts). The other keys belonged to the legacy "gamepad control of the Nucleus window" (mouse cursor on
    /// the left stick), replaced by the web UI's own gamepad navigation: they are ignored and left in the file, so an
    /// older Nucleus reading the same Settings.ini still finds them.
    /// </summary>
    public static class App_GamePadNavigation
    {
        /// <summary>Default <see cref="Type"/>.</summary>
        public const string DefaultType = "Xbox";

        private static string type = DefaultType;
        public static string Type
        {
            get => type;
            set
            {
                type = NormalizeType(value);
                Globals.ini.IniWriteValue("XUINav", "Type", type);
            }
        }

        /// <summary>"Xbox" or "Playstation" (the gamepad picture folders); anything else, a missing key included, is Xbox.</summary>
        public static string NormalizeType(string value)
        {
            return string.Equals(value?.Trim(), "Playstation", StringComparison.OrdinalIgnoreCase) ? "Playstation" : DefaultType;
        }

        /// <summary>Never throws: a missing or hand-edited value falls back to its default.</summary>
        public static bool LoadSettings()
        {
            try
            {
                type = NormalizeType(Globals.ini.IniReadValue("XUINav", "Type"));
            }
            catch (Exception)
            {
                type = DefaultType;
            }

            return true;
        }
    }
}
