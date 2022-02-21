using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Windows.Interop
{
    internal static class ShcoreInterop
    {
        [DllImport("shcore.dll")]
        internal static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);
    }
}
