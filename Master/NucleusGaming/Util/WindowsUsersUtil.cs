using Microsoft.Win32;
using Nucleus.Gaming.Coop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Nucleus.Gaming.Util
{
    public static class WindowsUsersUtil
    {
        private static string adminLocalGroup;

        [DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
        static extern bool DeleteProfile(string sidString, string profilePath, string computerName);

        static void DeleteProfileFolder(string file)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C rd /S /Q  \"" + file + "\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        static string GetUserSID(string userName)
        {
            var principalContext = new PrincipalContext(ContextType.Machine);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, userName);
            if (userPrincipal != null)
            {
                var userSid = userPrincipal.Sid;
                return userSid.ToString();
            }

            return null;
        }

        static ArrayList GetUserGroups(string sUserName)
        {
            ArrayList myItems = new ArrayList();
            UserPrincipal oUserPrincipal = GetUser(sUserName);

            if (oUserPrincipal != null)
            {
                PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetGroups();

                if (oPrincipalSearchResult != null && oPrincipalSearchResult?.ToList().Count > 0)
                {
                    foreach (Principal oResult in oPrincipalSearchResult)
                    {
                        myItems.Add(oResult.Name);
                    }
                }
                else
                {
                    LogManager.Log("ERROR - Principal Search Result is null");
                }
            }
            else
            {
                LogManager.Log("ERROR - User Principal is null");
            }

            if (myItems.Count == 0)
            {
                LogManager.Log("Error grabbing user groups for user: " + sUserName);
            }

            return myItems;
        }

        static UserPrincipal GetUser(string sUserName)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext();

            UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
            return oUserPrincipal;
        }

        static PrincipalContext GetPrincipalContext()
        {
            PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Machine);
            return oPrincipalContext;
        }

        public static void CreateWindowsUser(PlayerInfo player, int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (i == 0)
            {
                handlerInstance.Log("Searching for administrators local group");
                ArrayList localGroups = WindowsUsersUtil.GetUserGroups(Environment.UserName);

                if (localGroups != null && localGroups?.Count > 0)
                {
                    if (localGroups.Contains("Administrators"))
                    {
                        adminLocalGroup = "Administrators";
                        handlerInstance.Log("Found local group " + adminLocalGroup + " for current user");
                    }
                    else
                    {
                        foreach (string localGroup in localGroups)
                        {
                            if (localGroup.ToLower().StartsWith("admin"))
                            {
                                handlerInstance.Log("Found local group " + localGroup + " for current user");
                                adminLocalGroup = localGroup;
                                break;
                            }
                        }
                        if (adminLocalGroup == null)
                        {
                            adminLocalGroup = localGroups[0].ToString();
                            handlerInstance.Log("Unable to find an admin local group for current user, using " + adminLocalGroup);
                        }
                    }
                }
                else
                {
                    handlerInstance.Log("Unable to find any user groups, defaulting user group to Administrators");
                    adminLocalGroup = "Administrators";
                }

                handlerInstance.Log("Checking if sufficient Nucleus user accounts already exist");
                string createUserBatPath = Path.Combine(Globals.NucleusInstallRoot, "utils\\LaunchUsers\\create_users.bat");

                if (File.Exists(createUserBatPath))
                {
                    File.Delete(createUserBatPath);
                }

                bool createNeeded = false;
                bool echoLineUsed = false;
                for (int pc = 1; pc <= handlerInstance.numPlayers; pc++)
                {
                    bool UserExists = false;
                    using (PrincipalContext princ = new PrincipalContext(ContextType.Machine))
                    {
                        UserPrincipal up = UserPrincipal.FindByIdentity(
                            princ,
                            IdentityType.SamAccountName,
                            $"nucleusplayer{pc}");

                        UserExists = (up != null);
                    }

                    handlerInstance.Log($"nucleusplayer{pc} exists: {UserExists}");
                    if (!UserExists)
                    {
                        createNeeded = true;
                        using (StreamWriter sw = new StreamWriter(createUserBatPath, true))
                        {
                            if (!echoLineUsed)
                            {
                                sw.WriteLine(@"@echo off");
                                echoLineUsed = true;
                            }
                            sw.WriteLine($@"net user nucleusplayer{pc} {handlerInstance.nucleusUserAccountsPassword} /add && net user nucleusplayer{pc} {handlerInstance.nucleusUserAccountsPassword} && net localgroup " + adminLocalGroup + $" nucleusplayer{pc} /add");
                        }
                    }
                }

                if (createNeeded)
                {
                    handlerInstance.Log("Some users need to be created; creating user accounts");
                    Process user = new Process();
                    user.StartInfo.FileName = createUserBatPath;
                    user.StartInfo.Verb = "runas";
                    user.StartInfo.UseShellExecute = true;
                    user.Start();

                    user.WaitForExit();
                }

                if (File.Exists(createUserBatPath))
                {
                    File.Delete(createUserBatPath);
                }

                for (int pc = 1; pc <= handlerInstance.numPlayers; pc++)
                {
                    GameProfile.Instance.DevicesList[pc - 1].SID = GetUserSID($"nucleusplayer{pc}");
                    GameProfile.Instance.DevicesList[pc - 1].UserProfile = $"nucleusplayer{pc}";
                }
            }

            ProcessUtil.STARTUPINFO startup = new ProcessUtil.STARTUPINFO();
            startup.cb = Marshal.SizeOf(startup);

            bool success = ProcessUtil.CreateProcessWithLogonW($"nucleusplayer{i + 1}", Environment.UserDomainName, handlerInstance.nucleusUserAccountsPassword, ProcessUtil.LogonFlags.LOGON_WITH_PROFILE, "cmd.exe", null, ProcessUtil.ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, 0, null, ref startup, out ProcessUtil.PROCESS_INFORMATION processInformation);
            handlerInstance.Log("Launching intermediary program (cmd)");

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                handlerInstance.Log(string.Format("ERROR {0} - CreateProcessWithLogonW failed", error));
            }

            handlerInstance.launchProc = Process.GetProcessById(processInformation.dwProcessId);

            if (handlerInstance.CurrentGameInfo.TransferNucleusUserAccountProfiles)
            {
                Thread.Sleep(1000);
                handlerInstance.Log("Transfer Nucleus user account profiles is enabled");

                List<string> SourcePath = new List<string>();
                string OrigSourcePath = Globals.UserEnvironmentRoot + $@"\NucleusCoop\UserAccounts\{player.UserProfile}";
                SourcePath.Add(OrigSourcePath);

                if (handlerInstance.CurrentGameInfo.CopyEnvFoldersToNucleusAccounts?.Length > 0)
                {
                    foreach (string folder in handlerInstance.CurrentGameInfo.CopyEnvFoldersToNucleusAccounts)
                    {
                        SourcePath.Add(Globals.UserEnvironmentRoot + "\\" + folder);
                    }
                }

                string DestinationPath = $@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{player.UserProfile}";
                try
                {
                    foreach (string folder in SourcePath)
                    {
                        if (folder != OrigSourcePath)
                        {
                            DestinationPath = $@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{player.UserProfile}" + folder.Substring(Globals.UserEnvironmentRoot.Length);
                        }
                        else
                        {
                            DestinationPath = $@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{player.UserProfile}";
                        }

                        if (Directory.Exists(folder))
                        {
                            handlerInstance.Log("Copying " + folder + " to " + DestinationPath);
                            Directory.CreateDirectory(DestinationPath);

                            string cmd = "xcopy \"" + folder + "\" \"" + DestinationPath + "\" /E/H/C/I/Y/R";
                            int exitCode = -6942069;
                            CmdUtil.ExecuteCommand(folder, out exitCode, cmd, true);
                            while (exitCode == -6942069)
                            {
                                Thread.Sleep(25);
                            }
                            handlerInstance.Log($"Command: {cmd}, exit code: {exitCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    handlerInstance.Log("ERROR - " + ex.Message);
                }
            }
        }

        public static void DeleteCreatedWindowsUser()
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Deleting temporary user accounts");

            foreach (string folder in handlerInstance.folderUsers)
            {
                string username = folder.Substring(Path.GetDirectoryName(Globals.UserEnvironmentRoot).Length + 1);
                if (username.StartsWith("nucleusplayer"))
                {
                    handlerInstance.nucUsers.Add(username);
                    try
                    {
                        PrincipalContext principalContext = new PrincipalContext(ContextType.Machine);
                        UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
                        if (userPrincipal != null)
                        {
                            SecurityIdentifier userSid = userPrincipal.Sid;
                            handlerInstance.nucSIDs.Add(userSid.ToString());
                            DeleteProfile(userSid.ToString(), null, null);
                            userPrincipal.Delete();
                            string keyName = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true))
                            {
                                key.DeleteSubKeyTree(userSid.ToString(), false);
                            }

                            Thread.Sleep(250);

                            using (RegistryKey key = Registry.Users)
                            {
                                key.DeleteSubKeyTree(userSid.ToString(), false);
                                key.DeleteSubKeyTree(userSid.ToString() + "_Classes", false);
                            }
                        }
                        else
                        {
                            //MessageBox.Show("ERROR! User: {0} not found!", username);
                        }
                    }
                    catch (Exception)
                    {
                        //MessageBox.Show(exception.Message);
                    }

                    Thread.Sleep(1000);
                }
            }

            string deleteUserBatPath = Path.Combine(Globals.NucleusInstallRoot, "utils\\LaunchUsers\\delete_users.bat");
            if (File.Exists(deleteUserBatPath))
            {
                File.Delete(deleteUserBatPath);
            }

            using (StreamWriter sw = new StreamWriter(deleteUserBatPath, true))
            {
                sw.WriteLine("@echo off");

                foreach (string nucUser in handlerInstance.nucUsers)
                {
                    sw.WriteLine($"net user {nucUser} /delete");
                }
            }

            Process user = new Process();
            user.StartInfo.FileName = deleteUserBatPath; //"utils\\LaunchUsers\\delete_users.bat";
            user.StartInfo.Verb = "runas";
            user.StartInfo.UseShellExecute = true;
            user.Start();
            user.WaitForExit();

            if (File.Exists(deleteUserBatPath))
            {
                File.Delete(deleteUserBatPath);
            }
        }

        public static void DeleteCreatedWindowsUserFolder()
        {
            var handlerInstance = GenericGameHandler.Instance;
            foreach (string nucUser in handlerInstance.nucUsers)
            {
                DeleteProfileFolder($@"{Path.GetDirectoryName(Globals.UserEnvironmentRoot)}\{nucUser}");
            }
        }
    }
}
