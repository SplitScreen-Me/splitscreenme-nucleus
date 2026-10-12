using Nucleus.Coop.Services;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Coop.Host
{
    /// <summary>The host window as the services see it. Safe to call from any thread.</summary>
    internal sealed class HostShell : IHostShell
    {
        private readonly HostForm form;
        private IntPtr handle;

        /// <param name="form">Host window, created on the UI thread.</param>
        /// <param name="uiContext">Synchronization context of the UI thread.</param>
        public HostShell(HostForm form, SynchronizationContext uiContext)
        {
            this.form = form;
            UiContext = uiContext;

            if (form.IsHandleCreated)
            {
                handle = form.Handle;
            }

            form.HandleCreated += (s, e) => handle = form.Handle;
            form.HandleDestroyed += (s, e) => handle = IntPtr.Zero;
        }

        public IntPtr WindowHandle => handle;

        public SynchronizationContext UiContext { get; }

        public UiPageState PageState => form.PageState;

        public void MinimizeWindow()
        {
            Post(() => form.MinimizeWindow());
        }

        public void RestoreWindow()
        {
            Post(() =>
            {
                // While the app quits at the end of a session, the window stays hidden.
                if (HostApplication.IsExiting || !form.Visible)
                {
                    return;
                }

                if (HostNative.IsIconic(form.Handle))
                {
                    HostNative.ShowWindow(form.Handle, HostNative.SW_RESTORE); // back to maximized when it was
                }

                form.BringToForeground(); // in front and focused even when a game had the last input, so pads work at once
            });
        }

        private void Post(Action action)
        {
            UiContext.Post(_ =>
            {
                if (form.IsDisposed || !form.IsHandleCreated)
                {
                    return;
                }

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    HostLog.Error("Host shell call failed.", ex);
                }
            }, null);
        }
    }
}
