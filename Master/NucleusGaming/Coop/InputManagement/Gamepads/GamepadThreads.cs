using Nucleus.Gaming.Platform.PCSpecs;
using System.Threading;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    /// <summary>
    /// Starts the gamepad shortcuts and gamepad UI navigation threads. Moved from the WinForms Core_Interface so any
    /// host (WinForms main form or WebView2 host) can start them without touching UI controls.
    /// </summary>
    public static class GamepadThreads
    {
        /// <summary>
        /// Starts both threads (once). Enabled only for Windows versions with default support for xinput1_4.dll:
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

            if (GamepadNavigation.GamepadNavigationThread == null)
            {
                GamepadNavigation.GamepadNavigationThread = new Thread(GamepadNavigation.GamepadNavigationUpdate);
                GamepadNavigation.GamepadNavigationThread.Start();
                GamepadNavigation.UpdateUINavSettings();
            }

            return true;
        }
    }
}
