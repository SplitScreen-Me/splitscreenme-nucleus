using Nucleus.Gaming.Platform.PCSpecs;
using System.Threading;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    /// <summary>
    /// Starts the gamepad shortcuts thread (session shortcuts). Moved from the WinForms Core_Interface so any host can
    /// start it without touching UI controls. The legacy "gamepad control of the Nucleus window" thread (left stick moved
    /// the mouse cursor, buttons clicked) is gone: the web UI is navigated with gamepads natively (couch navigation in the
    /// page), and both acting on the same press clicked twice.
    /// </summary>
    public static class GamepadThreads
    {
        /// <summary>
        /// Starts the thread (once). Enabled only for Windows versions with default support for xinput1_4.dll:
        /// returns false on Windows 7 and Vista, where nothing is started.
        /// </summary>
        public static bool Start()
        {
            string windowsVersion = MachineSpecs.GetPCspecs();

            if (windowsVersion.Contains("Windows 7") || windowsVersion.Contains("Windows Vista"))
            {
                return false;
            }

            if (GamepadShortcuts.GamepadShortcutsThread == null)
            {
                GamepadShortcuts.GamepadShortcutsThread = new Thread(GamepadShortcuts.GamepadShortcutsUpdate);
                GamepadShortcuts.GamepadShortcutsThread.Start();
                GamepadShortcuts.UpdateShortcutsValue();
            }

            return true;
        }
    }
}
