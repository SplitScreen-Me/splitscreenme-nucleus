using System.Drawing;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming
{
    public class Shell32
    {
        private static object getIconState = new object();

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, out ShFileInfo psfi, uint cbfileInfo, ShgFi uFlags);

        /// <summary>
        /// Maximal Length of unmanaged Windows-Path-strings
        /// </summary>
        public const int MAX_PATH = 260;

        /// <summary>
        /// Maximal Length of unmanaged Typename
        /// </summary>
        public const int MAX_TYPE = 80;

        /// <summary>
        /// Get the associated Icon for a file or application, this method always returns
        /// an icon.  If the strPath is invalid or there is no idonc the default icon is returned
        /// </summary>
        /// <param name="strPath">full path to the file</param>
        /// <param name="bSmall">if true, the 16x16 icon is returned otherwise the 32x32</param>
        /// <returns></returns>
        public static Icon GetIcon(string strPath, bool bSmall)
        {
            lock (getIconState)
            {
                ShFileInfo info = new ShFileInfo(true);
                int cbFileInfo = Marshal.SizeOf(info);
                ShgFi flags;
                if (bSmall)
                {
                    flags = ShgFi.Icon | ShgFi.SmallIcon | ShgFi.UseFileAttributes;
                }
                else
                {
                    flags = ShgFi.Icon | ShgFi.LargeIcon | ShgFi.UseFileAttributes;
                }

                SHGetFileInfo(strPath, 256, out info, (uint)cbFileInfo, flags);
                return Icon.FromHandle(info.hIcon);
            }
        }
    }
}
