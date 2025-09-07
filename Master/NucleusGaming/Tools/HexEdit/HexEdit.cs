using Nucleus.Gaming.App.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Nucleus.Gaming.Tools.HexEdit
{
    public static class HexEdit
    {
        private static void Log(string logMessage)
        {
            if (App_Misc.DebugLog)
            {
                using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]NemirtingasGalaxyEmu: {logMessage}");
                    writer.Close();
                }
            }
        }

        public static void HexEditFile(int i, string linkFolder)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string[] splitValues = handlerInstance.CurrentGameInfo.HexEditFile[i].Split('|');
            if (splitValues.Length == 3)
            {
                string filePath = splitValues[0];
                string fullPath = Path.Combine(Path.Combine(linkFolder, filePath));
                string fullFileName = Path.GetFileName(filePath);
                string strToSearch = splitValues[1];
                string replacedStr = splitValues[2];
                Log(string.Format("HexEditFile - Patching file: {0}", filePath));

                bool origExists = false;
                if (File.Exists(Path.GetDirectoryName(fullPath) + "\\" + Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)))
                {
                    origExists = true;
                }

                if (origExists)
                {
                    Log(string.Format("Temporarily renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                    File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                    Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                    handlerInstance.Context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-TEMP" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                    Log(string.Format("Deleting temporary file {0}", Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                    File.Delete(Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                }
                else
                {
                    Log(string.Format("Renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-ORIG" + Path.GetExtension(filePath)));
                    File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)));
                    Log(string.Format("Created patched file {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                    handlerInstance.Context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-ORIG" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                }
            }
            else
            {
                Log("Invalid # of parameters provided for: " + handlerInstance.CurrentGameInfo.HexEditFile[i] + ", skipping");
            }

            Log("Patching executable complete");
        }


        public static void HexEditAllFiles(int i, string linkFolder)
        {
            var handlerInstance = GenericGameHandler.Instance;

            foreach (string asciiValues in handlerInstance.CurrentGameInfo.HexEditAllFiles)
            {
                string[] splitValues = asciiValues.Split('|');
                if (splitValues.Length == 3)
                {
                    string filePath = splitValues[0];
                    string fullPath = Path.Combine(Path.Combine(linkFolder, filePath));
                    string fullFileName = Path.GetFileName(filePath);
                    string strToSearch = splitValues[1];
                    string replacedStr = splitValues[2];
                    Log(string.Format("HexEditAllFiles - Patching file: {0}", filePath));

                    bool origExists = false;
                    if (File.Exists(Path.GetDirectoryName(fullPath) + "\\" + Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)))
                    {
                        origExists = true;
                    }

                    if (origExists)
                    {
                        Log(string.Format("Temporarily renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                        File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                        handlerInstance.Context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-TEMP" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                        Log(string.Format("Deleting temporary file {0}", Path.GetFileNameWithoutExtension(fullFileName) + "-TEMP" + Path.GetExtension(filePath)));
                        File.Delete(Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-TEMP" + Path.GetExtension(filePath)));
                    }
                    else
                    {
                        Log(string.Format("Renaming original file {0} to {1}", fullFileName, Path.GetFileNameWithoutExtension(fullFileName) + "-ORIG" + Path.GetExtension(filePath)));
                        File.Move(fullPath, Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "-ORIG" + Path.GetExtension(filePath)));
                        Log(string.Format("Created patched file {0} where the text string '{1}' has been replaced with '{2}'", fullFileName, strToSearch, replacedStr));
                        handlerInstance.Context.PatchFile(fullPath.Substring(0, fullPath.Length - 4) + "-ORIG" + Path.GetExtension(filePath), fullPath, splitValues[1], splitValues[2]);
                    }
                }
                else
                {
                    Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                }
            }
            Log("Patching executable complete");
        }

        public static void HexEditExe(int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            Log("HexEditExe - Patching individual executable");
            bool origExists = false;

            if (File.Exists(Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-ORIG.exe")))
            {
                origExists = true;
            }

            if (origExists)
            {
                string[] splitValues = handlerInstance.CurrentGameInfo.HexEditExe[i].Split('|');
                if (splitValues.Length == 2)
                {
                    Log(string.Format("Temporarily renaming original executable {0} to {1}", handlerInstance.CurrentGameInfo.ExecutableName, Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-TEMP.exe"));
                    File.Move(handlerInstance.exePath, Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-TEMP.exe"));
                    Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", handlerInstance.CurrentGameInfo.ExecutableName, splitValues[0], splitValues[1]));
                    handlerInstance.Context.PatchFile(handlerInstance.exePath.Substring(0, handlerInstance.exePath.Length - 4) + "-TEMP.exe", handlerInstance.exePath, splitValues[0], splitValues[1]);
                    Log(string.Format("Deleting temporary executable {0}", Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-TEMP.exe"));
                    File.Delete(Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-TEMP.exe"));
                }
                else
                {
                    if (string.IsNullOrEmpty(handlerInstance.CurrentGameInfo.HexEditExe[i]))
                    {
                        Log("Nothing to change for this instance's executable");
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + handlerInstance.CurrentGameInfo.HexEditFile[i] + ", skipping");
                    }
                }
            }
            else
            {
                string[] splitValues = handlerInstance.CurrentGameInfo.HexEditExe[i].Split('|');
                if (splitValues.Length == 2)
                {
                    Log(string.Format("Renaming original executable {0} to {1}", handlerInstance.CurrentGameInfo.ExecutableName, Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-ORIG.exe"));
                    File.Move(handlerInstance.exePath, Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-ORIG.exe"));
                    Log(string.Format("Created patched executable {0} where {1} has been replaced with {2}", handlerInstance.CurrentGameInfo.ExecutableName, splitValues[0], splitValues[1]));
                    handlerInstance.Context.PatchFile(handlerInstance.exePath.Substring(0, handlerInstance.exePath.Length - 4) + "-ORIG.exe", handlerInstance.exePath, splitValues[0], splitValues[1]);
                }
                else
                {
                    if (string.IsNullOrEmpty(handlerInstance.CurrentGameInfo.HexEditExe[i]))
                    {
                        Log("Nothing to change for this instance's executable");
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + handlerInstance.CurrentGameInfo.HexEditFile[i] + ", skipping");
                    }
                }
            }
            Log("Patching executable complete");
        }

        public static void HexEditAllExes(int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            Log("HexEditAllExes - Patching executable");

            bool origExists = false;
            if (File.Exists(Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-ORIG.exe")))
            {
                origExists = true;
            }

            foreach (string asciiValues in handlerInstance.CurrentGameInfo.HexEditAllExes)
            {
                if (origExists)
                {
                    string[] splitValues = asciiValues.Split('|');
                    if (splitValues.Length == 2)
                    {
                        Log(string.Format("Temporarily renaming original executable {0} to {1}", handlerInstance.CurrentGameInfo.ExecutableName, Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-TEMP.exe"));
                        File.Move(handlerInstance.exePath, Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-TEMP.exe"));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", handlerInstance.CurrentGameInfo.ExecutableName, splitValues[0], splitValues[1]));
                        handlerInstance.Context.PatchFile(handlerInstance.exePath.Substring(0, handlerInstance.exePath.Length - 4) + "-TEMP.exe", handlerInstance.exePath, splitValues[0], splitValues[1]);
                        Log(string.Format("Deleting temporary executable {0}", Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-TEMP.exe"));
                        File.Delete(Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-TEMP.exe"));
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                    }
                }
                else
                {
                    string[] splitValues = asciiValues.Split('|');
                    if (splitValues.Length == 2)
                    {
                        Log(string.Format("Renaming original executable {0} to {1}", handlerInstance.CurrentGameInfo.ExecutableName, Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName) + "-ORIG.exe"));
                        File.Move(handlerInstance.exePath, Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "-ORIG.exe"));
                        Log(string.Format("Created patched executable {0} where the text string '{1}' has been replaced with '{2}'", handlerInstance.CurrentGameInfo.ExecutableName, splitValues[0], splitValues[1]));
                        handlerInstance.Context.PatchFile(handlerInstance.exePath.Substring(0, handlerInstance.exePath.Length - 4) + "-ORIG.exe", handlerInstance.exePath, splitValues[0], splitValues[1]);
                    }
                    else
                    {
                        Log("Invalid # of parameters provided for: " + asciiValues + ", skipping");
                    }
                }
            }

            Log("Patching executable complete");
        }

        public static void HexEditExeAddress(int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.CurrentGameInfo.SymlinkExe)
            {
                Log("Skipping HexEditExeAddress, " + Path.GetFileName(handlerInstance.exePath) + " is symlinked");
                return;
            }

            Log("HexEditExeAddress - Patching executable, " + Path.GetFileName(handlerInstance.exePath) + ", in instance folder");

            if (!handlerInstance.CurrentGameInfo.SymlinkGame && !handlerInstance.CurrentGameInfo.HardlinkGame && !handlerInstance.CurrentGameInfo.HardcopyGame)
            {
                string fileBackup = Path.Combine(Path.GetDirectoryName(handlerInstance.exePath), Path.GetFileNameWithoutExtension(handlerInstance.exePath) + "_NUCLEUS_BACKUP.exe");
                if (File.Exists(handlerInstance.exePath) && !File.Exists(fileBackup))
                {
                    try
                    {
                        File.Copy(handlerInstance.exePath, fileBackup);
                        handlerInstance.backupFiles.Add(fileBackup);
                        Log($"Backing up file {Path.GetFileName(handlerInstance.exePath)} as {Path.GetFileName(fileBackup)}");
                    }
                    catch
                    { }
                }
            }

            foreach (string hexSplitLine in handlerInstance.CurrentGameInfo.HexEditExeAddress)
            {
                string[] hexSplit = hexSplitLine.Split('|');
                int indexOffset = 1;
                if (hexSplit.Length == 3)
                {
                    if (int.Parse(hexSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (hexSplit.Length == 1)
                {
                    Log("Invalid # of parameters provided for: " + hexSplitLine + ", skipping");
                    continue;
                }

                Log(string.Format("Bytes at address '{0}' to be replaced with '{1}'", hexSplit[1 - indexOffset], hexSplit[2 - indexOffset]));
                List<byte> bytesConv = new List<byte>();
                for (int s = 0; s < hexSplit[2 - indexOffset].Length; s += 2)
                {
                    bytesConv.Add(Convert.ToByte(hexSplit[2 - indexOffset].Substring(s, 2), 16));
                }
                byte[] bArray = bytesConv.ToArray();

                using (Stream stream = File.Open(handlerInstance.exePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Position = long.Parse(hexSplit[1 - indexOffset], NumberStyles.HexNumber);
                    stream.Write(bArray, 0, bArray.Length);
                }
            }
        }

        public static void HexEditFileAddress(int i, string linkFolder)
        {
            var handlerInstance = GenericGameHandler.Instance;

            foreach (string hexSplitLine in handlerInstance.CurrentGameInfo.HexEditFileAddress)
            {
                string[] hexSplit = hexSplitLine.Split('|');
                int indexOffset = 1;
                if (hexSplit.Length == 4)
                {
                    if (int.Parse(hexSplit[0]) != (i + 1))
                    {
                        continue;
                    }
                    indexOffset = 0;
                }
                else if (hexSplit.Length <= 2)
                {
                    Log("Invalid # of parameters provided for: " + hexSplitLine + ", skipping hex edit file address");
                    continue;
                }

                string fullFilePath = Path.Combine(linkFolder, hexSplit[1 - indexOffset]);
                FileInfo pathInfo = new FileInfo(fullFilePath);
                if (pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    Log("Skipping HexEditFileAddress, " + Path.GetFileName(hexSplit[1 - indexOffset]) + " is symlinked");
                    continue;
                }

                if (File.Exists(fullFilePath))
                {
                    if (!handlerInstance.CurrentGameInfo.SymlinkGame && !handlerInstance.CurrentGameInfo.HardlinkGame && !handlerInstance.CurrentGameInfo.HardcopyGame)
                    {
                        string fileBackup = Path.Combine(Path.GetDirectoryName(fullFilePath), Path.GetFileNameWithoutExtension(fullFilePath) + "_NUCLEUS_BACKUP" + Path.GetExtension(fullFilePath));
                        if (File.Exists(fullFilePath) && !File.Exists(fileBackup))
                        {
                            try
                            {
                                File.Copy(fullFilePath, fileBackup);
                                handlerInstance.backupFiles.Add(fileBackup);
                                Log($"Backing up file {Path.GetFileName(fullFilePath)} as {Path.GetFileName(fileBackup)}");
                            }
                            catch
                            { }
                        }
                    }

                    Log("HexEditFileAddress - Patching file, " + Path.GetFileName(fullFilePath) + ", in instance folder");
                    Log(string.Format("Bytes at address '{0}' to be replaced with '{1}'", hexSplit[2 - indexOffset], hexSplit[3 - indexOffset]));
                    List<byte> bytesConv = new List<byte>();
                    for (int s = 0; s < hexSplit[3 - indexOffset].Length; s += 2)
                    {
                        bytesConv.Add(Convert.ToByte(hexSplit[3 - indexOffset].Substring(s, 2), 16));
                    }
                    byte[] bArray = bytesConv.ToArray();

                    using (Stream stream = File.Open(fullFilePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        stream.Position = long.Parse(hexSplit[2 - indexOffset], NumberStyles.HexNumber);
                        stream.Write(bArray, 0, bArray.Length);
                    }
                }
                else
                {
                    Log("ERROR - Could not find file: " + fullFilePath + " to patch");
                }
            }
        }
    }
}
