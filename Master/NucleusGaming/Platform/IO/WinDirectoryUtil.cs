using Nucleus.Gaming.Platform.Windows.Interop;
using System;
using System.IO;

namespace Nucleus.Gaming.Platform.Windows.IO
{
    public static class WinDirectoryUtil
    {
        private static void Log(string logMessage) => DebugLogFile.Write("INJECT", logMessage);


        public static void LinkFiles(string rootFolder, string destination, out int exitCode, string[] exclusions, string[] copyInstead, bool hardLink)
        {
            exitCode = 1;

            FileInfo[] files = new DirectoryInfo(rootFolder).GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string lower = file.Name.ToLower();
                bool exclude = false;

                if (!string.IsNullOrEmpty(exclusions[0]))
                {
                    for (int j = 0; j < exclusions.Length; j++)
                    {
                        string exc = exclusions[j];
                        if (!string.IsNullOrEmpty(exc) && lower.Contains(exc))
                        {
                            // check if the file is i
                            exclude = true;
                            break;
                        }
                    }
                }

                if (exclude)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(copyInstead[0]))
                {
                    for (int j = 0; j < copyInstead.Length; j++)
                    {
                        string copy = copyInstead[j];
                        if (!string.IsNullOrEmpty(copy) && lower.Contains(copy))
                        {
                            exclude = true;
                            break;
                        }
                    }
                }

                string relative = file.FullName.Replace(rootFolder + @"\", "");
                string linkPath = Path.Combine(destination, relative);
                if (exclude)
                {
                    // should copy!
                    try
                    {
                        File.Copy(file.FullName, linkPath, true);
                    }
                    catch (Exception)
                    {
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(linkPath), out int exitCode2, "copy \"" + file.FullName + "\" \"" + linkPath + "\"");
                    }

                    //CopyFile(file.FullName, linkPath);

                }
                else
                {
                    //CmdUtil.MkLinkFile(file.FullName, linkPath, out exitCode);
                    if (hardLink)
                    {
                        Kernel32Interop.CreateHardLink(linkPath, file.FullName, IntPtr.Zero);
                    }
                    else
                    {
                        Kernel32Interop.CreateSymbolicLink(linkPath, file.FullName, Nucleus.Gaming.Platform.Windows.Interop.SymbolicLink.File);
                    }
                }
            }
        }

        public static void LinkDirectory(string root, DirectoryInfo currentDir, string destination, out int exitCode,
            string[] dirExclusions, string[] fileExclusions, string[] fileCopyInstead, bool hardLink, bool symFolders, bool firstRun = true)
        {
            Console.WriteLine($"Symlinking folder {root} to {destination}");

            exitCode = 1;

            //bool special = overrideSpecial;
            bool special = false;
            bool skip = false;


            if (!string.IsNullOrEmpty(dirExclusions[0]))
            {
                for (int j = 0; j < dirExclusions.Length; j++)
                {
                    string exclusion = dirExclusions[j];
                    string fullPath;
                    if (exclusion.StartsWith("direxskip"))
                    {
                        fullPath = Path.Combine(root, exclusion.Substring(9).ToLower());
                    }
                    else
                    {
                        fullPath = Path.Combine(root, exclusion).ToLower();
                    }

                    if (!string.IsNullOrEmpty(exclusion) && fullPath.Contains(currentDir.FullName.ToLower()))
                    {
                        if (exclusion.StartsWith("direxskip"))
                        {
                            skip = true;
                            break;
                        }

                        // special case, one of our subfolders is excluded
                        special = true;
                        break;
                    }
                }
            }

            // this folder has a child that cant be symlinked
            if (!skip || firstRun)
            {
                if (symFolders)
                {
                    if (!special && !firstRun)
                    {
                        Kernel32Interop.CreateSymbolicLink(destination, currentDir.FullName, Interop.SymbolicLink.Directory);
                    }
                    else
                    {
                        Directory.CreateDirectory(destination);
                    }
                }
                else
                {
                    Directory.CreateDirectory(destination);
                }

                //CmdUtil.LinkFiles(currentDir.FullName, destination, out exitCode, fileExclusions, fileCopyInstead, true);
                WinDirectoryUtil.LinkFiles(currentDir.FullName, destination, out exitCode, fileExclusions, fileCopyInstead, hardLink);

                DirectoryInfo[] children = currentDir.GetDirectories();
                for (int i = 0; i < children.Length; i++)
                {
                    DirectoryInfo child = children[i];
                    LinkDirectory(root, child, Path.Combine(destination, child.Name), out exitCode, dirExclusions, fileExclusions, fileCopyInstead, hardLink, symFolders, false);
                }
            }
        }
    }
}