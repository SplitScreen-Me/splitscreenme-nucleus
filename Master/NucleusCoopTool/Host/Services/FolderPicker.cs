using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nucleus.Coop.Host.Services
{
    /// <summary>The Vista+ folder picker (IFileOpenDialog with FOS_PICKFOLDERS), which FolderBrowserDialog lacks on .NET Framework.</summary>
    internal static class FolderPicker
    {
        private const uint FOS_NOCHANGEDIR = 0x00000008;
        private const uint FOS_PICKFOLDERS = 0x00000020;
        private const uint FOS_FORCEFILESYSTEM = 0x00000040;
        private const uint FOS_PATHMUSTEXIST = 0x00000800;
        private const uint SIGDN_FILESYSPATH = 0x80058000;
        private const int ERROR_CANCELLED_HRESULT = unchecked((int)0x800704C7);

        /// <summary>Shows the picker, modal to <paramref name="owner"/>. Returns the folder, or null when cancelled.</summary>
        /// <exception cref="COMException">The dialog could not be created or shown.</exception>
        public static string Show(IntPtr owner, string title, string initialDirectory)
        {
            IFileDialog dialog = (IFileDialog)new FileOpenDialogCoClass();
            try
            {
                uint options;
                dialog.GetOptions(out options);
                dialog.SetOptions(options | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM | FOS_PATHMUSTEXIST | FOS_NOCHANGEDIR);

                if (!string.IsNullOrWhiteSpace(title))
                {
                    dialog.SetTitle(title);
                }

                if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
                {
                    IShellItem folder;
                    Guid shellItemGuid = typeof(IShellItem).GUID;
                    if (SHCreateItemFromParsingName(initialDirectory, IntPtr.Zero, ref shellItemGuid, out folder) == 0)
                    {
                        dialog.SetFolder(folder);
                        Marshal.ReleaseComObject(folder);
                    }
                }

                int hr = dialog.Show(owner);
                if (hr == ERROR_CANCELLED_HRESULT)
                {
                    return null;
                }

                Marshal.ThrowExceptionForHR(hr);

                IShellItem result;
                dialog.GetResult(out result);
                try
                {
                    string path;
                    result.GetDisplayName(SIGDN_FILESYSPATH, out path);
                    return path;
                }
                finally
                {
                    Marshal.ReleaseComObject(result);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(dialog);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(string path, IntPtr bindContext, ref Guid riid, out IShellItem item);

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        private class FileOpenDialogCoClass
        {
        }

        [ComImport]
        [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);

            void SetFileTypes(uint count, IntPtr filterSpec);

            void SetFileTypeIndex(uint index);

            void GetFileTypeIndex(out uint index);

            void Advise(IntPtr events, out uint cookie);

            void Unadvise(uint cookie);

            void SetOptions(uint options);

            void GetOptions(out uint options);

            void SetDefaultFolder(IShellItem item);

            void SetFolder(IShellItem item);

            void GetFolder(out IShellItem item);

            void GetCurrentSelection(out IShellItem item);

            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);

            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);

            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);

            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);

            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);

            void GetResult(out IShellItem item);

            void AddPlace(IShellItem item, int placement);

            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);

            void Close(int hr);

            void SetClientGuid(ref Guid guid);

            void ClearClientData();

            void SetFilter(IntPtr filter);
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr bindContext, ref Guid handler, ref Guid riid, out IntPtr result);

            void GetParent(out IShellItem parent);

            void GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string name);

            void GetAttributes(uint mask, out uint attributes);

            void Compare(IShellItem other, uint hint, out int order);
        }
    }
}
