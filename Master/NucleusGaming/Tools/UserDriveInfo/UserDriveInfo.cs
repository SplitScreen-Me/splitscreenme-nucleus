using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;

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
                            UserPrompts.Report(PromptIcon.Warning, "Incompatible drive format!", "\"exFat\" is not a supported drive format.\n\n" +
                               "Move your Nucleus install folder to a NTFS formated drive, or format the drive to NTFS format.\n\n" +
                               "Reasons:\n\n" +
                               "exFat does not support symlink functions.");

                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                string message = "Error: " + ex.Message;
                UserPrompts.Report(PromptIcon.Error, "Error", message + "Something went wrong during drives compatibility check!");
                return false;
            }
        }

    }
}
