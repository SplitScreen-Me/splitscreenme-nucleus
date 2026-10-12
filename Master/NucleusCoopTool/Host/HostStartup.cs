using System;
using System.Linq;

namespace Nucleus.Coop.Host
{
    /// <summary>Command-line options of NucleusCoop.exe, parsed once in <c>Program.RunApplication</c>.</summary>
    public static class HostStartup
    {
        /// <summary>Opens the previous WinForms interface (MainForm) instead of the web UI.</summary>
        public const string LegacyUiSwitch = "--legacy-ui";

        /// <summary>
        /// Game to preselect: the first command-line argument when it is not a switch (a handler GUID, as used by
        /// desktop shortcuts). Null when absent. Services read it until it is exposed through the contract.
        /// </summary>
        public static string GameId { get; private set; }

        /// <summary>Whether <see cref="LegacyUiSwitch"/> was passed.</summary>
        public static bool LegacyUi { get; private set; }

        /// <summary>The arguments without the host switches, in their original order (what MainForm expects).</summary>
        internal static string[] ForwardedArgs { get; private set; } = new string[0];

        internal static void Parse(string[] args)
        {
            args = args ?? new string[0];

            LegacyUi = args.Any(a => string.Equals(a, LegacyUiSwitch, StringComparison.OrdinalIgnoreCase));
            ForwardedArgs = args.Where(a => !string.Equals(a, LegacyUiSwitch, StringComparison.OrdinalIgnoreCase)).ToArray();

            string first = ForwardedArgs.FirstOrDefault();
            GameId = string.IsNullOrWhiteSpace(first) || first.StartsWith("--", StringComparison.Ordinal) ? null : first;
        }
    }
}
