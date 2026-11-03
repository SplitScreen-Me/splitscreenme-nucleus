using System;
using System.Collections;
using System.IO;

namespace Nucleus.Gaming.Tools.NucleusUsers
{
    /// <summary>
    /// Folders of the per-player Nucleus environment (<c>Game.UseNucleusEnvironment</c>): every player gets
    /// <c>&lt;user profile&gt;\NucleusCoop\&lt;nickname&gt;</c> as USERPROFILE, with its own AppData and, since Nucleus 2.5,
    /// its own temporary folder (TEMP and TMP), so instances no longer share <c>AppData\Local\Temp</c>.
    /// </summary>
    public static class NucleusEnvironmentPaths
    {
        /// <summary><c>&lt;userEnvironmentRoot&gt;\NucleusCoop\&lt;nickname&gt;</c>: the player's USERPROFILE.</summary>
        public static string PlayerProfile(string userEnvironmentRoot, string nickname)
        {
            return $@"{userEnvironmentRoot}\NucleusCoop\{nickname}";
        }

        /// <summary>The player's LOCALAPPDATA: <c>&lt;player profile&gt;\AppData\Local</c>.</summary>
        public static string PlayerLocalAppData(string userEnvironmentRoot, string nickname)
        {
            return PlayerProfile(userEnvironmentRoot, nickname) + @"\AppData\Local";
        }

        /// <summary>The player's TEMP and TMP: <c>&lt;player profile&gt;\AppData\Local\Temp</c>.</summary>
        public static string PlayerTemp(string userEnvironmentRoot, string nickname)
        {
            return PlayerLocalAppData(userEnvironmentRoot, nickname) + @"\Temp";
        }

        /// <summary>
        /// TEMP and TMP for a process started without the Nucleus environment as another Windows user (Nucleus accounts):
        /// <c>&lt;LOCALAPPDATA&gt;\Temp</c> of that user, so it does not inherit the temporary folder of the user running Nucleus.
        /// </summary>
        public static string UserTemp(string localAppData)
        {
            return Path.Combine(localAppData, "Temp");
        }

        /// <summary>Sets TEMP and TMP to <paramref name="tempFolder"/> in an environment block (StringDictionary, Hashtable…).</summary>
        public static void SetTemp(IDictionary environment, string tempFolder)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            environment["TEMP"] = tempFolder;
            environment["TMP"] = tempFolder;
        }

        /// <summary>Sets TEMP and TMP to <paramref name="tempFolder"/> in a <see cref="System.Diagnostics.ProcessStartInfo"/> environment.</summary>
        public static void SetTemp(System.Collections.Specialized.StringDictionary environment, string tempFolder)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            environment["TEMP"] = tempFolder;
            environment["TMP"] = tempFolder;
        }
    }
}
