using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Gaming.Tools.UserDriveInfo
{
    public static class UserDriveInfo
    {
        public static DriveInfo[] UserDrives => DriveInfo.GetDrives();

        public static bool IsExFat(string fullPath,bool startupCheck)//Addd the check for Nucleus install too just in case(Check on startup).
        {
            try
            {
                string driveName = fullPath[0].ToString();

                foreach (DriveInfo drive in UserDrives)
                {
                    if (driveName == drive.Name[0].ToString())
                    {
                        if (drive.DriveFormat == "exFAT")//exFat do not supports symlink, that's bad for us.
                        {
                            if (startupCheck)
                            {
                                MessageBox.Show("\"exFat\" is not a supported drive format.\n\n" +
                                   "Move your Nucleus install folder to a NTFS formated drive, or format the drive to NTFS format.\n\n" +
                                   "Reasons:\n\n" +
                                   "exFat does not support symlink functions.", "Incompatible drive format!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                MessageBox.Show("\"exFat\" is not a supported drive format.\n\n" +
                                    "Move your game(s) to a NTFS formated drive, or format the drive to NTFS format.\n\n" +
                                    "Reasons:\n\n" +
                                    "exFat does not support symlink functions.", "Incompatible drive format!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                                MessageBox.Show("The game has not been added to your Nucleus game collection.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                string message = "Error: " + ex.Message;
                MessageBox.Show(message + "Something went wrong during drives compatibility check!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static void PrintDrivesInfo()//list
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                Console.WriteLine("Drive {0}", d.Name);
                Console.WriteLine("  Drive type: {0}", d.DriveType);
                if (d.IsReady)
                {
                    Console.WriteLine("  Volume label: {0}", d.VolumeLabel);
                    Console.WriteLine("  File system: {0}", d.DriveFormat);
                    Console.WriteLine(
                        "  Available space to current user:{0, 15} bytes",
                        d.AvailableFreeSpace);

                    Console.WriteLine(
                        "  Total available space:          {0, 15} bytes",
                        d.TotalFreeSpace);

                    Console.WriteLine(
                        "  Total size of drive:            {0, 15} bytes ",
                        d.TotalSize);
                }
            }
        }
    }
}
