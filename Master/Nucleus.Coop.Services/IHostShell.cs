using System;
using System.Threading;

namespace Nucleus.Coop.Services
{
    /// <summary>What the services need from the host window. Implemented by the WebView2 host form.</summary>
    public interface IHostShell
    {
        /// <summary>Handle of the main window (target of the kill hotkey's WM_CLOSE during a session).</summary>
        IntPtr WindowHandle { get; }

        /// <summary>Synchronization context of the UI thread. Every service runs its state changes on it.</summary>
        SynchronizationContext UiContext { get; }

        /// <summary>Minimizes the main window (when a session starts).</summary>
        void MinimizeWindow();

        /// <summary>Restores and activates the main window (when a session ends).</summary>
        void RestoreWindow();
    }
}
