using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace Nucleus.Gaming.Tools.BackupDatas
{
    public static class BackupDatas
    {
        private static readonly string BackupDirectory = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\_Game Files Backup_";

        public static void StartFilesBackup(string[] filesToBackup)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string gameGUID = handlerInstance.CurrentGameInfo.GUID;

            string gameContentPath = Path.Combine(GameManager.Instance.GetAppContentPath(), gameGUID);//game content root        
            
            var players = handlerInstance.profile.DevicesList;

            try
            {
                if (Directory.Exists(gameContentPath))
                {
                    Log("Start processing Files backup");

                    string[] instances = Directory.GetDirectories(gameContentPath, "*", SearchOption.TopDirectoryOnly);

                    for (int i = 0; i < players.Count(); i++)
                    {
                        var player = players[i];

                        if (Directory.Exists(instances[i]))
                        {
                            string destPath = $"{BackupDirectory}\\{gameGUID}\\{player.Nickname}";

                            if (!Directory.Exists(destPath))
                            {
                                Directory.CreateDirectory(destPath);
                            }

                            foreach (string filePath in filesToBackup)
                            {
                                string fileName = filePath.Split('\\').Last();//Get file name by splitting the full file path
                                string fileDest = filePath.Remove(filePath.IndexOf(fileName), fileName.Length);//Create a copy of the file path without the file name
                                string fileCopy = $"{destPath}\\{fileDest}\\{fileName}";//Build file copy/backup destination path

                                if (!Directory.Exists(fileDest))
                                {
                                    Directory.CreateDirectory($"{destPath}\\{fileDest}");
                                }

                                if (File.Exists(fileCopy))
                                {
                                    File.Delete(fileCopy);
                                }

                                File.Copy($"{instances[i]}\\{filePath}", fileCopy);
                            }
                        }
                    }

                    Log("Files backup successfull");
                }

            }
            catch (Exception ex)
            {
                Log($"Files backup failed\n{ex.Message}");
            }
        }

        public static void StartFoldersBackup(string[] foldersToBackup)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string gameGUID = handlerInstance.CurrentGameInfo.GUID;

            string gameContentPath = Path.Combine(GameManager.Instance.GetAppContentPath(), gameGUID);//game content root

            var players = handlerInstance.profile.DevicesList;

            try
            {
                if (Directory.Exists(gameContentPath))
                {
                    Log("Start processing folders backup");

                    string[] instances = Directory.GetDirectories(gameContentPath, "*", SearchOption.TopDirectoryOnly);

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];

                        if (Directory.Exists(instances[i]))
                        {
                            string destPath = $"{BackupDirectory}\\{gameGUID}\\{player.Nickname}";

                            if (!Directory.Exists(destPath))
                            {
                                Log($"Create player backup directory {destPath}");
                                Directory.CreateDirectory(destPath);
                            }

                            foreach (string sourceFolder in foldersToBackup)
                            {
                                string sourcePath = $"{instances[i]}\\{sourceFolder}";

                                if(!Directory.Exists(sourcePath) && !File.Exists(sourcePath))
                                {
                                    Log($"No file or directory to backup at => {sourcePath}");
                                    Log($"/!\\ {sourcePath} not found and ignored /!\\");
                                    continue;
                                }

                                string[] sourceFiles = Directory.GetFileSystemEntries(sourcePath, "*", SearchOption.AllDirectories);

                                if (sourceFiles.Length == 0)
                                {
                                    Log($"No files to backup found in {sourcePath}");
                                }

                                foreach (string sourceFile in sourceFiles)
                                {
                                    if (File.Exists(sourceFile))
                                    {
                                        string filePath = sourceFile.Substring(sourceFile.IndexOf(sourceFolder));
                                        string fileName = filePath.Split('\\').Last();//Get file name by splitting the full file path

                                        string fileDest = filePath.Remove(filePath.IndexOf(fileName), fileName.Length);//Build file copy destination folder path

                                        if (!Directory.Exists($"{destPath}\\{fileDest}"))
                                        {
                                            Log($"Create directory {destPath}\\{fileDest}");
                                            Directory.CreateDirectory($"{destPath}\\{fileDest}");
                                        }
                                        else
                                        {
                                            Log($"{destPath}\\{fileDest} exists...");
                                        }

                                        if (File.Exists($"{destPath}\\{fileDest}\\{fileName}"))
                                        {
                                            Log($"Delete previous copy of {destPath}\\{fileDest}\\{fileName}");
                                            File.Delete($"{destPath}\\{fileDest}\\{fileName}");
                                        }

                                        if (File.Exists(sourceFile))
                                        {
                                            Log($"Copy {sourceFile}");
                                            Log($"at {destPath}\\{fileDest}\\{fileName}");
                                            File.Copy(sourceFile, $"{destPath}\\{fileDest}\\{fileName}", true);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Log($"Folders backup finished");
                }
            }
            catch (Exception ex)
            {
                Log($"Folders backup failed\n{ex.Message}");
            }
        }

        #region Restore folders backups

        public static void StartBackupsRestoration()
        {
            var handlerInstance = GenericGameHandler.Instance;
            string gameGUID = handlerInstance.CurrentGameInfo.GUID;

            string gameContentPath = Path.Combine(GameManager.Instance.GetAppContentPath(), gameGUID);//game content root
            
            var players = handlerInstance.profile.DevicesList;

            try
            {
                if (Directory.Exists(gameContentPath))
                {
                    string sourceContent = $"{BackupDirectory}\\{gameGUID}";

                    if (!Directory.Exists(sourceContent))
                    {
                        Log($"No backed up datas to restore.");
                        return;
                    }

                    Log("Start processing folders backups restoration");

                    for (int i = 0; i < players.Count; i++)
                    {
                        var player = players[i];

                        string sourceFolder = $"{sourceContent}\\{player.Nickname}";

                        if (Directory.Exists(sourceFolder))
                        {
                            string destInstance = $"{gameContentPath}\\Instance{i}";

                            if (Directory.Exists(destInstance))
                            {
                                string[] sourceFiles = Directory.GetFileSystemEntries(sourceFolder, "*", SearchOption.AllDirectories);

                                foreach (string sourceFile in sourceFiles)
                                {
                                    if (File.Exists(sourceFile))
                                    {
                                        string fileName = sourceFile.Split('\\').Last();
                                        string filePath = sourceFile.Substring(sourceFile.LastIndexOf(player.Nickname));//Using "LastIndexOf" in case player and Windows user name are the same.
                                        string destPath = filePath.Remove(filePath.IndexOf(player.Nickname), player.Nickname.Length);

                                        string destDirectoryBuild = $"{destInstance}{destPath}";
                                        string destDirectory = destDirectoryBuild.Remove(destDirectoryBuild.IndexOf(fileName, fileName.Length));

                                        if (!Directory.Exists(destDirectory))
                                        {
                                            Log($"Creare directory {destDirectory}");
                                            Directory.CreateDirectory(destDirectory);
                                        }

                                        string fileCopy = $"{destInstance}{destPath}";

                                        if (File.Exists(fileCopy))
                                        {
                                            Log($"Delete previous copy of {fileCopy}");
                                            File.Delete(fileCopy);
                                        }

                                        Log($"Restore{sourceFile} at {fileCopy} for player {player.Nickname}");
                                        File.Copy(sourceFile, fileCopy);
                                    }
                                }
                            }
                        }
                        else 
                        {
                            Log($"Directory {sourceFolder} not found");
                        }
                    }

                    Log("Folders restoration finished");
                }
                else
                {
                    Log($"Directory {gameContentPath} not found");
                }
            }
            catch (Exception ex)
            {
                Log($"Folders restoration failed\n{ex.Message}");
            }
        }

        #endregion

        #region Here we backup files/folders specified in the game handler
        public static void ProceedBackup()
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.CurrentGameInfo.BackupFiles != null)
            {
                //Game.FilesToBackup
                if (handlerInstance.CurrentGameInfo.BackupFiles.Length > 0)
                {
                    StartFilesBackup(handlerInstance.CurrentGameInfo.BackupFiles);
                }
            }

            if (handlerInstance.Context != null)
            {
                if (handlerInstance.Context.BackupFiles != null)
                {
                    //Context.FilesToBackup
                    if (handlerInstance.Context.BackupFiles.Length > 0)
                    {
                        StartFilesBackup(handlerInstance.Context.BackupFiles);
                    }
                }
            }

            if (handlerInstance.CurrentGameInfo.BackupFolders != null)
            {
                //Game.BackupFolders
                if (handlerInstance.CurrentGameInfo.BackupFolders.Length > 0)
                {
                    StartFoldersBackup(handlerInstance.CurrentGameInfo.BackupFolders);
                }
            }

            if (handlerInstance.Context != null)
            {
                if (handlerInstance.Context.BackupFolders != null)
                {
                    //Context.BackupFolders
                    if (handlerInstance.Context.BackupFolders.Length > 0)
                    {
                        StartFoldersBackup(handlerInstance.Context.BackupFolders);
                    }
                }
            }
        }

        #endregion

        private static void Log(string logMessage)
        {
            try
            {
                if (App_Misc.DebugLog)
                {
                    using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                    {
                        writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]HANDLER: {logMessage}");
                        writer.Close();
                    }
                }
            }
            catch { }
        }
    }
}