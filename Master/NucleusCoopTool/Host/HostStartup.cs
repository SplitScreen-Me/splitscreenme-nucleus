using System;
using System.Linq;

namespace Nucleus.Coop.Host
{
    /// <summary>Command-line options of NucleusCoop.exe, parsed once in <c>Program.RunApplication</c>.</summary>
    public static class HostStartup
    {
        /// <summary>
        /// Used to open the WinForms interface (MainForm), which has been removed. Still accepted so old shortcuts keep
        /// working: it is logged and ignored.
        /// </summary>
        public const string LegacyUiSwitch = "--legacy-ui";

        /// <summary>
        /// Game to preselect: the first command-line argument when it is not a switch (a handler GUID, as used by
        /// desktop shortcuts). Null when absent. Services read it until it is exposed through the contract.
        /// </summary>
        public static string GameId { get; private set; }

        /// <summary>The arguments without the host switches, in their original order (passed on by a restart as administrator).</summary>
        internal static string[] ForwardedArgs { get; private set; } = new string[0];

        internal static void Parse(string[] args)
        {
            args = args ?? new string[0];

            if (args.Any(IsLegacyUiSwitch))
            {
                HostLog.Info($"Ignoring {LegacyUiSwitch}: the previous interface has been removed, starting the web UI.");
            }

            ForwardedArgs = args.Where(a => !IsLegacyUiSwitch(a)).ToArray();

            string first = ForwardedArgs.FirstOrDefault();
            GameId = string.IsNullOrWhiteSpace(first) || first.StartsWith("--", StringComparison.Ordinal) ? null : first;
        }

        private static bool IsLegacyUiSwitch(string arg) => string.Equals(arg, LegacyUiSwitch, StringComparison.OrdinalIgnoreCase);
    }
}
