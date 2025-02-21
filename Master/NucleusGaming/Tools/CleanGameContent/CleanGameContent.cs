using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using System.Management.Instrumentation;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public static class CleanGameContent
    {

        public static void CleanContentFolder(GenericGameInfo currentGameInfo, bool askBefore)
        {
            string path = currentGameInfo.Content_Folder;
            bool contentExist = Directory.Exists(path);

            if (askBefore)
            {
                if (contentExist)
                {
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        CleanContentFolder(currentGameInfo,path);
                    }
                }
                else
                {
                    MessageBox.Show("No data in content folder to delete.");
                }
            }
            else
            {
                if(contentExist)
                CleanContentFolder(currentGameInfo,path);
            }
        }


        private static void CleanContentFolder(GenericGameInfo currentGameInfo, string path)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    KillRemainingGameProcess(currentGameInfo);
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

                        System.Threading.Tasks.Task.Run(() =>
                        {
                            NucleusMessageBox.Show("Risk of crash!",
                                $"One or more files from {path} are locked\n" +
                                $"by the system or used by an other program \n" +
                                $"and Nucleus failed to unlock them.You can try\n" +
                                $"to delete/unlock the file(s) manually or restart\n" +
                                $" your computer to unlock the file(s) because it\n" +
                                $"could lead to a crash on game startup.\n" +
                                $" You can ignore this message and risk a crash or unexpected behaviors.", false);
                        });
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
                            proc.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(currentGameInfo.ExecutableName.ToLower()) || (currentGameInfo.Hook.ForceFocusWindowName != "" && proc.MainWindowTitle == currentGameInfo.Hook.ForceFocusWindowName))
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
