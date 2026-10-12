using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using System;

namespace Nucleus.Coop.Host.Services
{
    /// <summary>Controls the frameless host window and reports its state changes.</summary>
    internal sealed class WindowService : IWindowService
    {
        private readonly HostForm form;

        /// <param name="form">Host window.</param>
        /// <param name="events">Bridge events, used for <c>window.stateChanged</c>.</param>
        public WindowService(HostForm form, IBridgeEvents events)
        {
            this.form = form;
            form.WindowStateChanged += (sender, e) => events.Emit(new WindowStateChanged { State = ToContract(form.GetSnapshot()) });
        }

        public WindowState GetState() => OnUiThread(() => ToContract(form.GetSnapshot()));

        public void Minimize() => OnUiThread(form.MinimizeWindow);

        public void ToggleMaximize() => OnUiThread(form.ToggleMaximizeWindow);

        /// <summary>Closing goes through FormClosing, which ends a running session before quitting (HostApplication).</summary>
        public void Close() => OnUiThread(form.CloseWindow);

        private static WindowState ToContract(HostWindowSnapshot snapshot)
        {
            return new WindowState
            {
                Maximized = snapshot.Maximized,
                Minimized = snapshot.Minimized,
                Focused = snapshot.Focused,
                DpiScale = snapshot.DpiScale,
            };
        }

        private void OnUiThread(Action action)
        {
            if (form.InvokeRequired)
            {
                form.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private T OnUiThread<T>(Func<T> func)
        {
            return form.InvokeRequired ? (T)form.Invoke(func) : func();
        }
    }
}
