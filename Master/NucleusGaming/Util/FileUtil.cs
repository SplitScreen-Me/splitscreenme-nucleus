using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

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

        public static void CopyCustomUtils(int i, string linkFolder, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (setupDll)
            {
                handlerInstance.Log("Copying custom files/folders");
                string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\User");
                foreach (string customUtil in handlerInstance.CurrentGameInfo.CopyCustomUtils)
                {
                    int numParams = customUtil.Count(x => x == '|') + 1;
                    string[] splitParams = customUtil.Split('|');
                    string utilName = splitParams[0];
                    string utilPath = string.Empty;
                    if (numParams == 2)
                    {
                        utilPath = splitParams[1];
                    }

                    if (numParams == 3)
                    {
                        string utilInstances = splitParams[2];
                        List<int> instances = new List<int>();
                        instances = utilInstances.Split(',').Select(int.Parse).ToList();
                        if (!instances.Contains(i))
                        {
                            continue;
                        }
                    }

                    string source_dir = Globals.NucleusInstallRoot + "\\utils\\User\\" + utilPath;
                    FileAttributes attr = File.GetAttributes(source_dir);

                    if (attr.HasFlag(FileAttributes.Directory)) //directory
                    {
                        string destination_dir = linkFolder.TrimEnd('\\') + '\\' + utilPath;

                        foreach (string dir in System.IO.Directory.GetDirectories(source_dir, "*", System.IO.SearchOption.AllDirectories))
                        {
                            Directory.CreateDirectory(System.IO.Path.Combine(destination_dir, dir.Substring(source_dir.Length + 1)));
                        }

                        handlerInstance.Log("Copying user folder " + utilPath + " and all its contents to " + "Instance" + i + "\\" + utilPath);
                        foreach (string file_name in System.IO.Directory.GetFiles(source_dir, "*", System.IO.SearchOption.AllDirectories))
                        {
                            File.Copy(file_name, System.IO.Path.Combine(destination_dir, file_name.Substring(source_dir.Length + 1)));
                        }
                    }
                    else //file
                    {
                        FileCheck(Path.Combine(linkFolder.TrimEnd('\\') + '\\' + utilPath, Path.GetFileName(utilName)));

                        handlerInstance.Log("Copying " + utilName + " to " + "Instance" + i + "\\" + utilPath);
                        File.Copy(Path.Combine(utilFolder, utilName), Path.Combine(linkFolder.TrimEnd('\\') + '\\' + utilPath, Path.GetFileName(utilName)), true);
                    }
                }

                handlerInstance.Log("Copying custom files complete");
            }
        }

        public static string GetFolder(GenericGameHandler genericGameHandler, Folder folder)
        {
            string str = folder.ToString();
            string output;
            if (genericGameHandler.jsData.TryGetValue(str, out output))
            {
                return output;
            }
            return "";
        }

        public static void DeleteFiles(string linkFolder, int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            foreach (string deleteLine in handlerInstance.CurrentGameInfo.DeleteFiles)
            {
                string[] deleteSplit = deleteLine.Split('|');
                int indexOffset = 1;
                if (deleteSplit.Length == 2)
                {
                    if (int.Parse(deleteSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }

                string fullFilePath = Path.Combine(linkFolder, deleteSplit[1 - indexOffset]);

                if (File.Exists(fullFilePath))
                {
                    if (!handlerInstance.CurrentGameInfo.IgnoreDeleteFilesPrompt)
                    {
                        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + fullFilePath + "'?", "Nucleus - Delete Files In Config Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dialogResult != DialogResult.Yes)
                        {
                            continue;
                        }
                    }
                    handlerInstance.Log(string.Format("Deleting file {0}", Path.GetFileName(fullFilePath), Path.Combine(linkFolder, deleteSplit[2 - indexOffset])));
                    File.Delete(fullFilePath);
                }
                else
                {
                    handlerInstance.Log("ERROR - Could not find file: " + fullFilePath + " to delete");
                }
            }
        }

        public static void RenameOrMoveFiles(string linkFolder, int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            foreach (string renameLine in handlerInstance.CurrentGameInfo.RenameAndOrMoveFiles)
            {
                string[] renameSplit = renameLine.Split('|');
                int indexOffset = 1;
                if (renameSplit.Length == 3)
                {
                    if (int.Parse(renameSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (renameSplit.Length <= 1)
                {
                    handlerInstance.Log("Invalid # of parameters provided for: " + renameLine + ", skipping renaming and or moving");
                    continue;
                }

                string fullFilePath = Path.Combine(linkFolder, renameSplit[1 - indexOffset]);

                if (File.Exists(fullFilePath))
                {
                    handlerInstance.Log(string.Format("Renaming and/or moving {0} to {1}", Path.GetFileName(fullFilePath), Path.Combine(linkFolder, renameSplit[2 - indexOffset])));
                    if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(linkFolder, renameSplit[2 - indexOffset]))))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(linkFolder, renameSplit[2 - indexOffset])));
                    }
                    File.Move(fullFilePath, Path.Combine(linkFolder, renameSplit[2 - indexOffset]));
                }
                else
                {
                    handlerInstance.Log("ERROR - Could not find file: " + fullFilePath + " to rename and/or move");
                }
            }
        }

        public static void FileCheck(string file)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (File.Exists(file))
            {
                if (!handlerInstance.CurrentGameInfo.SymlinkGame && !handlerInstance.CurrentGameInfo.HardlinkGame && !handlerInstance.CurrentGameInfo.HardcopyGame)
                {
                    string fileBackup = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_NUCLEUS_BACKUP" + Path.GetExtension(file));
                    if (!File.Exists(fileBackup))
                    {
                        try
                        {
                            File.Move(file, fileBackup);
                            handlerInstance.backupFiles.Add(fileBackup);
                            handlerInstance.Log($"Backing up file {Path.GetFileName(file)} as {Path.GetFileName(fileBackup)}");
                        }
                        catch
                        { }
                    }
                }
                else
                {
                    handlerInstance.Log($"Deleting {Path.GetFileName(file)}");
                    File.Delete(file);
                }
            }
            else
            {
                handlerInstance.Log($"{Path.GetFileName(file)} doesn't exist, will be deleted upon ending session");
                handlerInstance.addedFiles.Add(file);
            }
        }

        public static void CleanOriginalgGameFolder()
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Deleting any files Nucleus added to original game folder");
            foreach (string addedFilePath in handlerInstance.addedFiles)
            {
                File.Delete(addedFilePath);
            }

            Thread.Sleep(500);

            handlerInstance.Log("Restoring any backed up files");

            foreach (string backupFilePath in handlerInstance.backupFiles)
            {
                try
                {
                    if (File.Exists(backupFilePath))
                    {
                        string origFile = backupFilePath.Replace("_NUCLEUS_BACKUP", "");
                        File.Delete(origFile);
                        File.Move(backupFilePath, origFile);
                    }
                }
                catch { }
            }
        }

        public static void SendToRecycleBin(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                else if (File.Exists(path))
                {
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
            catch (Exception ex)
            {
                DialogResult dialogResult = MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            }
        }

        public static string ComputeSHA256(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (var sha = new SHA256Managed())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
