using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Host
{
    /// <summary>
    /// Runs the web UI: creates the host window and the composition root, and takes over the duties the old
    /// MainForm had besides its UI (main window handle, OSD, gamepad threads, ending a session on close).
    /// </summary>
    internal static class HostApplication
    {
        private static readonly TimeSpan SessionEndTimeout = TimeSpan.FromSeconds(90);

        private static int exiting;

        /// <summary>True once the app is quitting (the window may be hidden while a session ends).</summary>
        public static bool IsExiting => Volatile.Read(ref exiting) != 0;

        /// <summary>Called from <c>Program.RunApplication</c> after the startup checks, on the UI thread. Never returns.</summary>
        public static void Run()
        {
            // The rest of the process is per-monitor v1 aware (Program.cs); the host window is per-monitor v2 so the
            // WebView and the native dialogs follow monitor DPI changes.
            HostNative.TryEnablePerMonitorV2ForThread();

            HostEnvironment environment = HostEnvironment.Resolve();
            HostLog.Info($"Starting the web UI (debug={environment.IsDebug}, start={environment.StartUri}, game={HostStartup.GameId ?? "none"}).");

            using (HostForm form = new HostForm(environment))
            {
                form.FormClosing += OnFormClosing;
                form.Shown += OnFirstShown;

                using (new CompositionRoot(form, environment))
                {
                    Application.Run(form);
                }
            }

            Exit();
        }

        private static void OnFirstShown(object sender, EventArgs e)
        {
            HostForm form = (HostForm)sender;
            form.Shown -= OnFirstShown;

            // Same as MainWindowFunc.FadeInTick: the OSD is a WPF window owned by the UI thread.
            try
            {
                Globals.MainOSD = new WPF_OSD(Screen.FromHandle(form.Handle).Bounds);
            }
            catch (Exception ex)
            {
                HostLog.Error("Could not create the OSD.", ex);
            }

            // After the OSD: the shortcuts thread reports through Globals.MainOSD.
            StartGamepadThreads();
        }

        /// <summary>Core_Interface.InitializeGamepadThreads without the legacy Settings/UI_Interface controls.</summary>
        private static void StartGamepadThreads()
        {
            try
            {
                // Starts the thread once; nothing on Windows 7/Vista (no xinput1_4.dll).
                if (!GamepadThreads.Start())
                {
                    HostLog.Info("Gamepad shortcuts need Windows 8 or later.");
                }
            }
            catch (Exception ex)
            {
                HostLog.Error("Could not start the gamepad threads.", ex);
            }
        }

        /// <summary>
        /// Closing the window (title bar, Alt+F4, the kill hotkey's WM_CLOSE) while a session runs ends the session first,
        /// on a worker thread, then quits. Same as MainWindowFunc.MainForm_Closed, without blocking the UI thread.
        /// </summary>
        private static void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            IGameHandler handler = GameProfile.I_GameHandler;
            if (handler == null || handler.HasEnded)
            {
                return;
            }

            e.Cancel = true;

            if (Interlocked.Exchange(ref exiting, 1) != 0)
            {
                return; // already ending
            }

            HostLog.Info("Window closed during a session: ending the session, then quitting.");
            ((Form)sender).Hide();

            Task.Run(() =>
            {
                try
                {
                    handler.End(false);
                }
                catch (Exception ex)
                {
                    HostLog.Error("Ending the session failed.", ex);
                }

                Stopwatch elapsed = Stopwatch.StartNew();
                while (!handler.HasEnded && elapsed.Elapsed < SessionEndTimeout)
                {
                    Thread.Sleep(100);
                }

                if (!handler.HasEnded)
                {
                    HostLog.Warning("The session did not end in time; quitting anyway.");
                }

                Kill();
            });
        }

        /// <summary>
        /// Quits after the window closed. The game handler and gamepad threads are foreground threads that never stop,
        /// so the process is killed, as the legacy UI does (MainWindowFunc.cs).
        /// </summary>
        private static void Exit()
        {
            Interlocked.Exchange(ref exiting, 1);
            Kill();
        }

        private static void Kill()
        {
            // Give the asynchronous app log a moment to flush its last lines.
            Thread.Sleep(200);
            Process.GetCurrentProcess().Kill();
        }
    }
}
