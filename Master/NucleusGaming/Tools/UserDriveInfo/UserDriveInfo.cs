using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Gaming.Tools.UserDriveInfo
{
    public static class UserDriveInfo
    {
        /// <summary>Startup check of the Nucleus install folder: shows a message box and returns true on an exFAT drive.</summary>
        public static bool IsExFat(string fullPath)
        {
            try
            {
                DriveInfo[] UserDrives = DriveInfo.GetDrives();

                string driveName = fullPath[0].ToString();

                foreach (DriveInfo drive in UserDrives)
                {
                    if (driveName == drive.Name[0].ToString())
                    {
                        if (drive.DriveFormat == "exFAT")//exFat do not supports symlink, that's bad for us.
                        {
                            UserPrompts.Report(PromptIcon.Warning, T("engine.drives.exFat.title"), T("engine.drives.exFat.message"));

                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                UserPrompts.Report(PromptIcon.Error, T("engine.titles.error"), T("engine.drives.checkFailed", new { error = ex.Message }));
                return false;
            }
        }

    }
}
