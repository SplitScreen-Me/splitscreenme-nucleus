using System.IO;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming
{
    public enum SymbolicLink
    {
        File = 0,
        Directory = 1
    }
    public static class FileUtil
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        public static void Write(byte[] data, string place)
        {
            using (MemoryStream str = new MemoryStream(data))
            {
                using (FileStream stream = new FileStream(place, FileMode.Create))
                {
                    str.CopyTo(stream);
                    stream.Flush();
                }
            }
        }

        public static void CopyDirectoryFiles(string rootFolder, string destination, out int exitCode, params string[] exclusions)
        {
            exitCode = 1;

            FileInfo[] files = new DirectoryInfo(rootFolder).GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                string lower = file.Name.ToLower();
                bool cont = false;

                for (int j = 0; j < exclusions.Length; j++)
                {
                    string exc = exclusions[j];
                    if (!string.IsNullOrEmpty(exc) && lower.Contains(exc))
                    {
                        // check if the file is i
                        cont = true;
                        break;
                    }
                }

                if (cont)
                {
                    continue;
                }

                string relative = file.FullName.Replace(rootFolder + @"\", "");
                string linkPath = Path.Combine(destination, relative);
                try
                {
                    File.Copy(file.FullName, linkPath, false);
                }
                catch { }
            }
        }

        public static void CopyDirectory(string root, DirectoryInfo currentDir, string destination, out int exitCode, string[] dirExclusions, string[] fileExclusions, bool firstRun = true)
        {
            exitCode = 1;
            bool skip = false;

            if (dirExclusions.Length > 0 && !string.IsNullOrEmpty(dirExclusions[0]))
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

                        break;
                    }
                }
            }

            if (!skip || firstRun)
            {

                Directory.CreateDirectory(destination);
                // copy all files
                CopyDirectoryFiles(currentDir.FullName, destination, out exitCode, fileExclusions);

                DirectoryInfo[] children = currentDir.GetDirectories();
                for (int i = 0; i < children.Length; i++)
                {
                    DirectoryInfo child = children[i];
                    CopyDirectory(root, child, Path.Combine(destination, child.Name), out exitCode, dirExclusions, fileExclusions, false);
                }

            }
        }
    }
}
