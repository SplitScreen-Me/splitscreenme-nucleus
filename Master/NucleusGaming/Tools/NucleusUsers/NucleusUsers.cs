using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Tools.NucleusUsers
{
    public static class NucleusUsers
    {
        public static void UserProfileConfigCopy(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string nucConfigPath = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\", handlerInstance.CurrentGameInfo.UserProfileConfigPath);
            string realConfigPath = Path.Combine(Globals.UserEnvironmentRoot, handlerInstance.CurrentGameInfo.UserProfileConfigPath);
            if ((!Directory.Exists(nucConfigPath) && Directory.Exists(realConfigPath)) || (Directory.Exists(realConfigPath) && handlerInstance.CurrentGameInfo.ForceUserProfileConfigCopy))
            {
                Directory.CreateDirectory(nucConfigPath);

                handlerInstance.Log("Config path " + handlerInstance.CurrentGameInfo.UserProfileConfigPath + " does not exist for Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucConfigPath, dir.Substring(realConfigPath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)));
                        handlerInstance.Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        handlerInstance.Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        public static void UserProfileSaveCopy(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string nucSavePath = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\", handlerInstance.CurrentGameInfo.UserProfileSavePath);
            string realSavePath = Path.Combine(Globals.UserEnvironmentRoot, handlerInstance.CurrentGameInfo.UserProfileSavePath);
            if ((!Directory.Exists(nucSavePath) && Directory.Exists(realSavePath)) || (Directory.Exists(realSavePath) && handlerInstance.CurrentGameInfo.ForceUserProfileSaveCopy))
            {
                Directory.CreateDirectory(nucSavePath);

                handlerInstance.Log("Save path " + handlerInstance.CurrentGameInfo.UserProfileConfigPath + " does not exist in Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realSavePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucSavePath, dir.Substring(realSavePath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realSavePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)));
                        handlerInstance.Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        //Log("ERROR - " + ex.Message);
                        handlerInstance.Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        public static void DocumentsConfigCopy(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string nucConfigPath = Path.Combine($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\{player.Nickname}\Documents\", handlerInstance.CurrentGameInfo.DocumentsConfigPath);
            string realConfigPath = Path.Combine(Globals.UserDocumentsRoot, handlerInstance.CurrentGameInfo.DocumentsConfigPath);
            if ((!Directory.Exists(nucConfigPath) && Directory.Exists(realConfigPath)) || (Directory.Exists(realConfigPath) && handlerInstance.CurrentGameInfo.ForceDocumentsConfigCopy))
            {
                Directory.CreateDirectory(nucConfigPath);

                handlerInstance.Log("Config path " + handlerInstance.CurrentGameInfo.DocumentsConfigPath + " does not exist for Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucConfigPath, dir.Substring(realConfigPath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realConfigPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)));
                        handlerInstance.Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        //Log("ERROR - " + ex.Message);
                        handlerInstance.Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucConfigPath, file_name.Substring(realConfigPath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        public static void DocumentsSaveCopy(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string nucSavePath = Path.Combine($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\{player.Nickname}\Documents\", handlerInstance.CurrentGameInfo.DocumentsSavePath);
            string realSavePath = Path.Combine(Globals.UserDocumentsRoot, handlerInstance.CurrentGameInfo.DocumentsSavePath);

            if ((!Directory.Exists(nucSavePath) && Directory.Exists(realSavePath)) || (Directory.Exists(realSavePath) && handlerInstance.CurrentGameInfo.ForceDocumentsSaveCopy))
            {
                Directory.CreateDirectory(nucSavePath);

                handlerInstance.Log("Save path " + handlerInstance.CurrentGameInfo.DocumentsConfigPath + " does not exist in Nucleus nickname " + player.Nickname + " environment. Copying folder and all its contents over");
                foreach (string dir in Directory.GetDirectories(realSavePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(Path.Combine(nucSavePath, dir.Substring(realSavePath.Length + 1)));
                }

                foreach (string file_name in Directory.GetFiles(realSavePath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Copy(file_name, Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)));
                        handlerInstance.Log("Copying file " + Path.GetFileName(file_name));
                    }
                    catch
                    {
                        handlerInstance.Log("Using alternative copy method for " + file_name);
                        CmdUtil.ExecuteCommand(Path.GetDirectoryName(Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1))), out int exitCode, "copy \"" + file_name + "\" \"" + Path.Combine(nucSavePath, file_name.Substring(realSavePath.Length + 1)) + "\"", false);
                    }
                }
            }
        }

        public static void DeleteFilesInConfigPath(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string path = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\", handlerInstance.CurrentGameInfo.UserProfileConfigPath);

            foreach (string fileName in handlerInstance.CurrentGameInfo.DeleteFilesInConfigPath)
            {
                string[] foundFiles = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
                foreach (string foundFile in foundFiles)
                {
                    if (!handlerInstance.CurrentGameInfo.IgnoreDeleteFilesPrompt)
                    {
                        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + foundFile + "'?", "Nucleus - Delete Files In Config Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dialogResult != DialogResult.Yes)
                        {
                            continue;
                        }
                    }

                    handlerInstance.Log(string.Format("Deleting file {0}", foundFile));
                    File.Delete(foundFile);
                }
            }
        }

        public static void DeleteFilesInSavePath(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string path = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\", handlerInstance.CurrentGameInfo.UserProfileConfigPath);

            foreach (string fileName in handlerInstance.CurrentGameInfo.DeleteFilesInSavePath)
            {
                string[] foundFiles = Directory.GetFiles(path, fileName, SearchOption.AllDirectories);
                foreach (string foundFile in foundFiles)
                {
                    if (!handlerInstance.CurrentGameInfo.IgnoreDeleteFilesPrompt)
                    {
                        DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + foundFile + "'?", "Nucleus - Delete Files In Save Path", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dialogResult != DialogResult.Yes)
                        {
                            continue;
                        }
                    }

                    handlerInstance.Log(string.Format("Deleting file {0}", foundFile));
                    File.Delete(foundFile);
                }
            }
        }

        public static void TransferNucleusUserAccountProfiles(List<PlayerInfo> data)
        {
            var handlerInstance = GenericGameHandler.Instance;
            handlerInstance.Log("Transfer Nucleus user account profiles is enabled");
            foreach (PlayerInfo player in data)
            {
                handlerInstance.Log("Backing up AppData and Documents from " + Path.Combine($@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{player.UserProfile}") + " to " + Path.Combine(Globals.UserEnvironmentRoot + $@"\NucleusCoop\UserAccounts"));
                string subFolder;
                for (int fol = 0; fol < 2; fol++)
                {
                    if (fol == 0)
                    {
                        subFolder = "AppData";
                    }
                    else
                    {
                        subFolder = "Documents";
                    }

                    string SourcePath = $@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{player.UserProfile}\{subFolder}";
                    string DestinationPath = Globals.UserEnvironmentRoot + $@"\NucleusCoop\UserAccounts\{player.UserProfile}\{subFolder}";

                    try
                    {
                        if (Directory.Exists(SourcePath))
                        {
                            Directory.CreateDirectory(DestinationPath);
                            string cmd = "xcopy \"" + SourcePath + "\" \"" + DestinationPath + "\" /E/H/C/I/Y/R";
                            CmdUtil.ExecuteCommand(SourcePath, out int exitCode, cmd, true);
                            handlerInstance.Log($"Command: {cmd}, exit code: {exitCode}");
                        }

                    }
                    catch (Exception ex)
                    {
                        handlerInstance.Log("ERROR - " + ex.Message);
                    }

                }
                Thread.Sleep(1000);
            }
        }

        public static IntPtr CreateUserEnvironment(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;
            IntPtr envPtr = IntPtr.Zero;

            handlerInstance.Log("Setting up Nucleus environment");
            var sb = new StringBuilder();
            System.Collections.IDictionary envVars = Environment.GetEnvironmentVariables();
            string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
            envVars["USERPROFILE"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}";
            envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
            envVars["APPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
            envVars["LOCALAPPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";
            envVars["TEMP"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local\Temp";
            envVars["TMP"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local\Temp";

            //Some games will crash if the directories don't exist
            Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
            Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
            Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
            Directory.CreateDirectory(envVars["APPDATA"].ToString());
            Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
            Directory.CreateDirectory(envVars["TEMP"].ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents");

            if (handlerInstance.CurrentGameInfo.DocumentsConfigPath?.Length > 0 || handlerInstance.CurrentGameInfo.DocumentsSavePath?.Length > 0)
            {
                if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg")))
                {
                    RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Shell Folders.reg"));
                }

                RegistryKey dkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                dkey.SetValue("Personal", Path.GetDirectoryName(Globals.UserDocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents", (RegistryValueKind)(int)RegType.ExpandString);
            }

            foreach (object envVarKey in envVars.Keys)
            {
                if (envVarKey != null)
                {
                    string key = envVarKey.ToString();
                    string value = envVars[envVarKey].ToString();

                    sb.Append(key);
                    sb.Append("=");
                    sb.Append(value);
                    sb.Append("\0");
                }
            }

            sb.Append("\0");
            byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());
            envPtr = Marshal.AllocHGlobal(envBytes.Length);
            Marshal.Copy(envBytes, 0, envPtr, envBytes.Length);
            return envPtr;
        }

    }
}
