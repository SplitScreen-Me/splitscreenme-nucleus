using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace AutoHotkey.Interop
{
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region Native Calls

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeLibraryHandle LoadLibrary(string lpFileName);

        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        private SafeLibraryHandle() : base(true) { }

        protected override bool ReleaseHandle()
        {
            return FreeLibrary(handle);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
