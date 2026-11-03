using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Gaming
{
    public static class CleanGameContent
    {

        /// <summary>
        /// Kills the game's leftover processes, then deletes its content folder in the background. The task's result says
        /// whether the folder is gone. When files stay locked, <paramref name="reportLockedFiles"/> shows "Risk of crash!"
        /// (<see cref="UserPrompts.Notice"/>, origin <see cref="PromptOrigin.Setup"/>). With <paramref name="sendToRecycleBin"/>
        /// (a deletion the user asked for, Nucleus 2.5) the folder goes to the recycle bin; it is deleted for good only when
        /// its drive has no recycle bin. The automatic cleanups (setup opened, session ended) delete for good.
        /// </summary>
        public static Task<bool> CleanContentFolderAsync(GenericGameInfo currentGameInfo, bool reportLockedFiles = true, bool sendToRecycleBin = false)
        {
            string path = currentGameInfo.Content_Folder;
            return Directory.Exists(path) ? CleanContentFolder(currentGameInfo, path, reportLockedFiles, sendToRecycleBin) : Task.FromResult(true);
        }

        /// <summary>
        /// Removes a folder: to the recycle bin when asked and possible, else for good. Throws when the folder could not be
        /// removed (locked files), so the caller can unlock them and try again.
        /// </summary>
        private static void RemoveFolder(string path, bool sendToRecycleBin)
        {
            if (sendToRecycleBin)
            {
                if (FileUtil.SendToRecycleBin(path) || !Directory.Exists(path))
                {
                    return;
                }

                if (FileUtil.HasRecycleBin(path))
                {
                    throw new IOException($"Could not move {path} to the recycle bin");
                }

                LogManager.Log($"No recycle bin for {path}: deleting it for good");
            }

            Directory.Delete(path, true);
        }

        /// <summary>Text of the "Risk of crash!" message.</summary>
        public static string LockedFilesMessage(string path)
        {
            return T("engine.gameContent.lockedFiles.message", new { path });
        }

        private static Task<bool> CleanContentFolder(GenericGameInfo currentGameInfo, string path, bool reportLockedFiles, bool sendToRecycleBin)
        {
            return Task.Run(() =>
            {
                try
                {
                    KillRemainingGameProcess(currentGameInfo);
                    RemoveFolder(path, sendToRecycleBin);
                    return true;
                }
                catch
                {
                    LogManager.Log("Nucleus will try to unlock one or more files in order to cleanup game content.");

                    try
                    {
                        string[] instances = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                        foreach (string instance in instances)
                        {
                            if (Directory.Exists(instance))
                            {
                                string[] subs = Directory.GetFileSystemEntries(instance, "*", SearchOption.AllDirectories);

                                foreach (string locked in subs)
                                {
                                    File.SetAttributes(locked, FileAttributes.Normal);
                                }

                                if (!sendToRecycleBin)//one recycle bin entry for the whole folder, below
                                {
                                    Directory.Delete(instance, true);
                                    LogManager.Log("Game content cleaned.");
                                }
                            }
                        }

                        if (Directory.Exists(path))
                        {
                            RemoveFolder(path, sendToRecycleBin);
                        }

                        return true;
                    }
                    catch
                    {
                        LogManager.Log("Game content cleanup failed. One or more files can't be unlocked by Nucleus.");

                        if (reportLockedFiles)
                        {
                            string title = T("engine.gameContent.lockedFiles.title");
                            string message = LockedFilesMessage(path);
                            Task.Run(() => UserPrompts.Notice(title, message, false, PromptOrigin.Setup, PromptIcon.Warning));
                        }

                        return false;
                    }
                }
            });
        }


        public static void CleanContentFolder(GenericGameInfo currentGameInfo)
        {
            string path = Path.Combine(GameManager.Instance.GetAppContentPath(), currentGameInfo.GUID);

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    LogManager.Log("Nucleus will try to unlock one or more files in order to cleanup game content.");

                    try
                    {
                        string[] instances = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                        foreach (string instance in instances)
                        {
                            if (Directory.Exists(instance))
                            {
                                string[] subs = Directory.GetFileSystemEntries(instance, "*", SearchOption.AllDirectories);

                                foreach (string locked in subs)
                                {
                                    File.SetAttributes(locked, FileAttributes.Normal);
                                }

                                Directory.Delete(instance, true);
                                LogManager.Log("Game content cleaned.");
                            }
                        }

                        Directory.Delete(path, true);
                    }
                    catch
                    {
                        LogManager.Log("Game content cleanup failed. One or more files can't be unlocked by Nucleus.");
                    }
                }
            }
        }

        private static void KillRemainingGameProcess(GenericGameInfo currentGameInfo)
        {
            try
            {
                Process[] procs = Process.GetProcesses();

                List<string> addtlProcsToKill = new List<string>();
                if (currentGameInfo.KillProcessesOnClose?.Length > 0)
                {
                    addtlProcsToKill = currentGameInfo.KillProcessesOnClose.ToList();
                }

                foreach (Process proc in procs)
                {
                    try
                    {
                        if ((currentGameInfo.LauncherExe != null && !currentGameInfo.LauncherExe.Contains("NucleusDefined") && proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(currentGameInfo.LauncherExe.ToLower())) ||
                            addtlProcsToKill.Contains(proc.ProcessName, StringComparer.OrdinalIgnoreCase) || 
                            (currentGameInfo.ExecutableName != null && proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(currentGameInfo.ExecutableName.ToLower())) || (currentGameInfo.Hook.ForceFocusWindowName != "" && proc.MainWindowTitle == currentGameInfo.Hook.ForceFocusWindowName))
                        {
                            LogManager.Log(string.Format("Killing process {0} (pid {1})", proc.ProcessName, proc.Id));
                            proc.Kill();
                        }

                    }
                    catch (Exception ex)
                    {
                        LogManager.Log(ex.InnerException + " " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(ex.InnerException + " " + ex.Message);
            }
        }
    }
}
