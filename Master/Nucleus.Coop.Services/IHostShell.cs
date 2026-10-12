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

        /// <summary>Whether the web UI can show messages now. Any thread.</summary>
        UiPageState PageState { get; }
    }

    /// <summary>State of the web UI page, for messages that would otherwise be native windows.</summary>
    public enum UiPageState
    {
        /// <summary>The page is loading (startup, reload): it reads what is waiting for it once loaded.</summary>
        Loading,

        /// <summary>The page is loaded and talking to the bridge.</summary>
        Ready,

        /// <summary>No page will show anything soon (WebView failed or closing, renderer crashed): use native windows.</summary>
        Unavailable,
    }
}
