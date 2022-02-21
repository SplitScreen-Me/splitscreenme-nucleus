using System.Diagnostics;

namespace Nucleus.Gaming
{
    public static class SteamUtil
    {
        public static readonly string SteamProcess = "steam";

        public static bool IsSteamRunning()
        {
            Process[] process = Process.GetProcesses();
            for (int i = 0; i < process.Length; i++)
            {
                Process proc = process[i];
                string name = proc.ProcessName.ToLower();
                if (name.StartsWith(SteamProcess))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
