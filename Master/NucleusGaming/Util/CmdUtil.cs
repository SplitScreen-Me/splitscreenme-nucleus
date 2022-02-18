using System.Diagnostics;
using System.IO;

namespace Nucleus.Gaming
{
    public static class CmdUtil
    {
        public static void ExecuteCommand(string workingDirectory, out int exitCode, params string[] commands)
        {
            exitCode = 0;
            for (int i = 0; i < commands.Length; i++)
            {
                ExecuteCommand(workingDirectory, out exitCode, commands[i]);
            }
        }

        public static void MkLinkDirectory(string fromDir, string toDir, out int exitCode)
        {
            string cmd = string.Format("mklink /d \"{0}\" \"{1}\"", toDir, fromDir);
            ExecuteCommand(fromDir, out exitCode, cmd);
        }
        public static void MkLinkFile(string fromFile, string toFile, out int exitCode)
        {
            string cmd = string.Format("mklink \"{0}\" \"{1}\"", toFile, fromFile);
            ExecuteCommand(fromFile, out exitCode, cmd);
        }

        public static void MkHardLinkFile(string fromFile, string toFile, out int exitCode)
        {
            string cmd = string.Format("mklink /h \"{0}\" \"{1}\"", toFile, fromFile);
            ExecuteCommand(fromFile, out exitCode, cmd);
        }

        public static void LinkDirectory(string root, DirectoryInfo currentDir, string destination, out int exitCode, string[] dirExclusions, string[] fileExclusions, string[] fileCopyInstead, bool isSymlink, bool overrideSpecial = false)
        {
            exitCode = 1;

            bool special = overrideSpecial;
            for (int j = 0; j < dirExclusions.Length; j++)
            {
                string exclusion = dirExclusions[j];
                string fullPath = Path.Combine(root, exclusion).ToLower();

                if (fullPath.Contains(currentDir.FullName.ToLower()))
                {
                    // special case, one of our subfolders is excluded
                    special = true;
                    break;
                }
            }

            if (special)
            {
                // this folder has a child that cant be symlinked
                Directory.CreateDirectory(destination);
                CmdUtil.LinkFiles(currentDir.FullName, destination, out exitCode, fileExclusions, fileCopyInstead, isSymlink);


                DirectoryInfo[] children = currentDir.GetDirectories();
                for (int i = 0; i < children.Length; i++)
                {
                    DirectoryInfo child = children[i];
                    LinkDirectory(root, child, Path.Combine(destination, child.Name), out exitCode, dirExclusions, fileExclusions, fileCopyInstead, isSymlink);
                }
            }
            else
            {
                // we symlink this directly
                CmdUtil.MkLinkDirectory(currentDir.FullName, destination, out exitCode);
            }
        }

        public static void LinkFiles(string rootFolder, string destination, out int exitCode, string[] exclusions, string[] copyInstead, bool isSymlink)
        {
            exitCode = 1;

            FileInfo[] files = new DirectoryInfo(rootFolder).GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string lower = file.Name.ToLower();
                bool exclude = false;
                for (int j = 0; j < exclusions.Length; j++)
                {
                    string exc = exclusions[j];
                    if (lower.Contains(exc))
                    {
                        // check if the file is i
                        exclude = true;
                        break;
                    }
                }

                if (exclude)
                {
                    continue;
                }

                for (int j = 0; j < copyInstead.Length; j++)
                {
                    string copy = copyInstead[j];
                    if (lower.Contains(copy))
                    {
                        exclude = true;
                        break;
                    }
                }

                string relative = file.FullName.Replace(rootFolder + @"\", "");
                string linkPath = Path.Combine(destination, relative);
                if (exclude)
                {
                    // should copy!
                    File.Copy(file.FullName, linkPath, true);
                }
                else
                {
                    if (isSymlink)
                    {
                        CmdUtil.MkLinkFile(file.FullName, linkPath, out exitCode);
                    }
                    else //hardlink
                    {
                        CmdUtil.MkHardLinkFile(file.FullName, linkPath, out exitCode);
                    }
                }
            }
        }



        public static void ExecuteCommand(string workingDirectory, out int exitCode, string command, bool runAsAdmin = true)
        {
            ProcessStartInfo processInfo;

            processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = workingDirectory,
                Arguments = "/c " + command,

                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,

                UseShellExecute = true
            };
            if (runAsAdmin)
            {
                processInfo.Verb = "runas";
            }

            Process process = new Process
            {
                StartInfo = processInfo
            };
            process.Start();
            process.WaitForExit();

            exitCode = process.ExitCode;
        }

    }
}
