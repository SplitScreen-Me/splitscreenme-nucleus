namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// The frameless host window. Dragging is done in CSS with <c>app-region: drag</c>.
    /// The native title stays "Nucleus Co-op" (other code finds the window by that title).
    /// </summary>
    [BridgeService("window")]
    public interface IWindowService
    {
        /// <summary>Current state of the window.</summary>
        WindowState GetState();

        /// <summary>Minimizes the window.</summary>
        void Minimize();

        /// <summary>Maximizes the window, or restores it when it is maximized.</summary>
        void ToggleMaximize();

        /// <summary>Closes the window and quits the app (a running session is stopped first).</summary>
        void Close();
    }

    /// <summary>State of the host window.</summary>
    public sealed class WindowState
    {
        /// <summary>Whether the window is maximized.</summary>
        public bool Maximized { get; set; }

        /// <summary>Whether the window is minimized.</summary>
        public bool Minimized { get; set; }

        /// <summary>Whether the window has keyboard focus.</summary>
        public bool Focused { get; set; }

        /// <summary>Scale factor of the monitor the window is on (1 = 96 DPI).</summary>
        public double DpiScale { get; set; }
    }

    /// <summary>The window was maximized, restored, minimized, focused or blurred, or moved to another monitor.</summary>
    [BridgeEvent("window.stateChanged")]
    public sealed class WindowStateChanged
    {
        /// <summary>New state.</summary>
        public WindowState State { get; set; } = new WindowState();
    }
}
