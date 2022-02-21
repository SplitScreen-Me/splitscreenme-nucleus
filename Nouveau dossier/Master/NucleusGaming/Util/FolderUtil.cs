using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nucleus
{
    public static class FolderUtil
    {
        public static void MkLink(string folderPath, string destinationPath, params string[] exclude)
        {
            DirectoryInfo source = new DirectoryInfo(folderPath);
            DirectoryInfo[] allDirs = source.GetDirectories();

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            for (int i = 0; i < allDirs.Length; i++)
            {
                DirectoryInfo dir = allDirs[i];
                string sourceFolder = dir.FullName;
                if (exclude.Contains(dir.Name.ToLower()))
                {
                    continue;
                }

                string destination = Path.Combine(destinationPath, dir.Name);

                int exitCode;
                CmdUtil.ExecuteCommand(destinationPath, out exitCode, "mklink /d \"" + destination + "\" \"" + sourceFolder + "\"");
            }

            FileInfo[] allFiles = source.GetFiles();
            for (int i = 0; i < allFiles.Length; i++)
            {
                FileInfo f = allFiles[i];
                string sourceFile = f.FullName;
                if (exclude.Contains(f.Name.ToLower()))
                {
                    continue;
                }

                string destination = Path.Combine(destinationPath, f.Name);

                int exitCode;
                CmdUtil.ExecuteCommand(destinationPath, out exitCode, "mklink \"" + destination + "\" \"" + sourceFile + "\"");
            }
        }
    }
}
