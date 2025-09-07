using Microsoft.Win32;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Tools.Steam
{
    public static class SteamFunctions
    {
        private static string startingArgs;
        public static bool readToEnd = false;
        public static string lobbyConnectArg;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        private static string GetUserSteamClientLanguage()
        {
            string result;

            if (Environment.Is64BitOperatingSystem)
            {
                result = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam\steamglobal", "Language", "english");
            }
            else
            {
                result = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "Language", "english");
            }

            return result;
        }

        public static string GetUserSteamLanguageChoice(GenericGameInfo game)
        {
            string lang;

            if(game.MetaInfo.SteamLanguage != null && game.MetaInfo.SteamLanguage != "App Setting")
            {
                lang = game.MetaInfo.SteamLanguage;
            }
            else if (App_Misc.SteamLang != "" && App_Misc.SteamLang != "Automatic")
            {
                lang = App_Misc.SteamLang.ToLower();
            }
            else
            {
                lang = GetUserSteamClientLanguage();
            }

            if (game.GoldbergLanguage?.Length > 0)
            {
                lang = game.GoldbergLanguage;
            }

            if (lang == string.Empty)
            {
                lang = "english";
            }

            return lang;
        }

        public static void UseGoldbergNoOGSteamDlls(string nucleusRootFolder, string linkFolder, int i, PlayerInfo player, List<PlayerInfo> players, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Starting Goldberg setup");
            string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\GoldbergEmu");
            string steamDllrootFolder = string.Empty;
            string instanceSteamDllFolder = string.Empty;
            string instanceSteamSettingsFolder = string.Empty;
            string prevSteamDllFilePath = string.Empty;

            string steam64Dll = string.Empty;
            string steamDll = string.Empty;

            if (handlerInstance.CurrentGameInfo.GoldbergExperimental)
            {
                handlerInstance.Log("Using experimental Goldberg");
                steam64Dll += "experimental\\";
                steamDll += "experimental\\";
            }

            steam64Dll += "steam_api64.dll";
            steamDll += "steam_api.dll";

            if (handlerInstance.CurrentGameInfo.GoldbergExperimentalSteamClient)
            {
                handlerInstance.Log("Using Goldberg Experimental Steam Client");
                utilFolder += "\\experimental_steamclient";

                string exeFolder = Path.GetDirectoryName(handlerInstance.exePath);

                File.Copy(Path.Combine(utilFolder, "steamclient.dll"), Path.Combine(exeFolder, "steamclient.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient64.dll"), Path.Combine(exeFolder, "steamclient64.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient_loader.exe"), Path.Combine(exeFolder, "steamclient_loader.exe"));

                handlerInstance.CurrentGameInfo.ExecutableToLaunch = Path.Combine(exeFolder, "steamclient_loader.exe");
                handlerInstance.CurrentGameInfo.ForceProcessSearch = true;
                handlerInstance.CurrentGameInfo.GoldbergWriteSteamIDAndAccount = true;

                if (i == 0)
                {
                    startingArgs = handlerInstance.CurrentGameInfo.StartArguments;
                }

                var sb = new StringBuilder();
                string gblines = sb.Append("#My own modified version of ColdClientLoader originally by Rat431")
                                .AppendLine()
                                .Append("[SteamClient]")
                                .AppendLine()
                                .Append($"Exe={handlerInstance.CurrentGameInfo.ExecutableName}")
                                .AppendLine()
                                .Append($"ExeRunDir=.")
                                .AppendLine()
                                .Append($"ExeCommandLine={startingArgs}")
                                .AppendLine()
                                .Append($"AppId={handlerInstance.CurrentGameInfo.SteamID}")
                                .AppendLine()
                                .AppendLine()
                                .Append("SteamClientDll=steamclient.dll")
                                .AppendLine()
                                .Append("SteamClient64Dll=steamclient64.dll")
                                .ToString();
                File.WriteAllText(Path.Combine(exeFolder, "ColdClientLoader.ini"), gblines);
                handlerInstance.addedFiles.Add(Path.Combine(exeFolder, "ColdClientLoader.ini"));

                //swalloing the launch arguments for the game here as we will be launching steamclient loader
                handlerInstance.CurrentGameInfo.StartArguments = string.Empty;
                handlerInstance.Context.StartArguments = string.Empty;

                string settingsFolder = exeFolder + "\\settings";
                if (handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                {
                    if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
                    {
                        settingsFolder = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                    else
                    {
                        settingsFolder = $@"{Globals.UserEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                }
                else
                {
                    File.WriteAllText(Path.Combine(exeFolder, "local_save.txt"), "");
                    handlerInstance.addedFiles.Add(Path.Combine(exeFolder, "local_save.txt"));
                }

                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }

                File.WriteAllText(Path.Combine(settingsFolder, "user_steam_id.txt"), "");
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "user_steam_id.txt"));

                File.WriteAllText(Path.Combine(settingsFolder, "account_name.txt"), "");
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "account_name.txt"));

                string lang = GetUserSteamLanguageChoice(handlerInstance.CurrentGameInfo);

                File.WriteAllText(Path.Combine(settingsFolder, "language.txt"), lang);
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "language.txt"));
            }
            else
            {
                if (handlerInstance.CurrentGameInfo.CustomSteamApiDllPath?.Length > 0)
                {
                    foreach (string filePath in handlerInstance.CurrentGameInfo.CustomSteamApiDllPath)
                    {
                        string fileName = filePath.Split('\\').Last(); 

                        instanceSteamDllFolder = linkFolder.TrimEnd('\\') + "\\" + filePath.Remove(filePath.IndexOf(fileName));

                        if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment && handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                        {
                            instanceSteamSettingsFolder = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                        }
                        else
                        {
                            instanceSteamSettingsFolder = Path.Combine(instanceSteamDllFolder, "settings");
                        }

                        Directory.CreateDirectory(instanceSteamSettingsFolder);

                        if (setupDll)
                        {
                            if (filePath.EndsWith("steam_api64.dll", true, null))
                            {
                                try
                                {
                                    handlerInstance.Log("Placing Goldberg steam_api64.dll in instance steam dll folder");
                                    File.Copy(Path.Combine(utilFolder, steam64Dll), Path.Combine(instanceSteamDllFolder, "steam_api64.dll"), true);
                                }
                                catch (Exception ex)
                                {
                                    handlerInstance.Log("ERROR - " + ex.Message);
                                    handlerInstance.Log("Using alternative copy method for steam_api64.dll");
                                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steam64Dll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api64.dll") + "\"");
                                }
                            }

                            if (filePath.EndsWith("steam_api.dll", true, null))
                            {
                                try
                                {
                                    handlerInstance.Log("Placing Goldberg steam_api.dll in instance steam dll folder");
                                    File.Copy(Path.Combine(utilFolder, steamDll), Path.Combine(instanceSteamDllFolder, "steam_api.dll"), true);
                                }
                                catch (Exception ex)
                                {
                                    handlerInstance.Log("ERROR - " + ex.Message);
                                    handlerInstance.Log("Using alternative copy method for steam_api.dll");
                                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steamDll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api.dll") + "\"");
                                }
                            }

                            if (handlerInstance.CurrentGameInfo.GoldbergExperimental)
                            {
                                handlerInstance.Log("Placing Goldberg steamclient.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, "experimental\\steamclient.dll"), Path.Combine(instanceSteamDllFolder, "steamclient.dll"), true);

                                handlerInstance.Log("Placing Goldberg steamclient64.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, "experimental\\steamclient64.dll"), Path.Combine(instanceSteamDllFolder, "steamclient64.dll"), true);
                            }
                        }

                        if (!string.IsNullOrEmpty(prevSteamDllFilePath))
                        {
                            if (prevSteamDllFilePath == filePath)
                            {
                                continue;
                            }
                        }

                        handlerInstance.Log("New steam api folder found");
                        prevSteamDllFilePath = filePath;

                        if (App_Misc.UseNicksInGame && !string.IsNullOrEmpty(player.Nickname))
                        {
                            if (setupDll)
                            {
                                handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                            }

                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                            handlerInstance.Log("Generating account_name.txt with nickname " + player.Nickname);
                        }
                        else
                        {
                            if (setupDll)
                            {
                                handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                            }

                            if (App_Misc.UseNicksInGame && Cache.PlayersIdentityCache.GetNicknameAt(i) != "")
                            {
                                File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                                handlerInstance.Log("Generating account_name.txt with nickname " + player.Nickname);
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), "Player" + (i + 1));
                                handlerInstance.Log("Generating account_name.txt with nickname Player " + (i + 1));
                            }
                        }

                        long steamID = player.SteamID;

                        handlerInstance.Log("Generating user_steam_id.txt with user steam ID " + (steamID).ToString());

                        if (setupDll)
                        {
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"));
                        }

                        File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"), (steamID).ToString());

                        if (setupDll)
                        {
                            string lang = GetUserSteamLanguageChoice(handlerInstance.CurrentGameInfo);

                            handlerInstance.Log("Generating language.txt with language set to " + lang);
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "language.txt"));
                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "language.txt"), lang);

                            if (handlerInstance.CurrentGameInfo.GoldbergIgnoreSteamAppId)
                            {
                                handlerInstance.Log("Skipping steam_appid.txt creation");
                            }
                            else
                            {
                                handlerInstance.Log("Generating steam_appid.txt using game steam ID " + handlerInstance.CurrentGameInfo.SteamID);
                                handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"));
                                File.WriteAllText(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"), handlerInstance.CurrentGameInfo.SteamID);
                            }

                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "local_save.txt"));
                            if (!handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                            {
                                File.WriteAllText(Path.Combine(instanceSteamDllFolder, "local_save.txt"), "");
                            }

                            if (handlerInstance.CurrentGameInfo.GoldbergNeedSteamInterface)
                            {
                                handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"));

                                if (File.Exists(Path.Combine(utilFolder, "tools\\steam_interfaces.txt")))
                                {
                                    handlerInstance.Log("Found generated steam_interfaces.txt file in Nucleus util folder, copying this file");

                                    File.Copy(Path.Combine(utilFolder, "tools\\steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                                }
                                else if (File.Exists(Path.Combine(steamDllrootFolder, "steam_interfaces.txt")))
                                {
                                    handlerInstance.Log("Found steam_interfaces.txt in original game folder, copying this file");
                                    File.Copy(Path.Combine(steamDllrootFolder, "steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                                }
                                else if (File.Exists(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + handlerInstance.CurrentGameInfo.JsFileName.Substring(0, handlerInstance.CurrentGameInfo.JsFileName.Length - 3) + "\\steam_interfaces.txt"))
                                {
                                    handlerInstance.Log("Found steam_interfaces.txt in Nucleus game folder");
                                    File.Copy(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + handlerInstance.CurrentGameInfo.JsFileName.Substring(0, handlerInstance.CurrentGameInfo.JsFileName.Length - 3) + "\\steam_interfaces.txt", Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                                }
                                else
                                {
                                    handlerInstance.Log("Unable to locate steam_interfaces.txt or create one, skipping this file");

                                    if (i == 0)
                                    {
                                        MessageBox.Show("Goldberg was unable to locate steam_interfaces.txt or create one. Process will continue without using this file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        }
                    }
                }
                else 
                {
                    NucleusMessageBox.Show("Warning", "No file path has been added to \"Game.CustomSteamApiDllPath = [fullFilePath1,fullFilePath2]\"", false);
                }
            }

            handlerInstance.Log("Goldberg setup complete");
        }

        public static void UseGoldberg(string rootFolder, string nucleusRootFolder, string linkFolder, int i, PlayerInfo player, List<PlayerInfo> players, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Starting Goldberg setup");
            string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\GoldbergEmu");
            string steamDllrootFolder = string.Empty;
            string steamDllFolder = string.Empty;
            string instanceSteamDllFolder = string.Empty;
            string instanceSteamSettingsFolder = string.Empty;
            string prevSteamDllFilePath = string.Empty;


            string steam64Dll = string.Empty;
            string steamDll = string.Empty;

            if (handlerInstance.CurrentGameInfo.GoldbergExperimental)
            {
                handlerInstance.Log("Using experimental Goldberg");
                steam64Dll += "experimental\\";
                steamDll += "experimental\\";
            }

            steam64Dll += "steam_api64.dll";
            steamDll += "steam_api.dll";

            if (handlerInstance.CurrentGameInfo.GoldbergExperimentalSteamClient)
            {
                handlerInstance.Log("Using Goldberg Experimental Steam Client");
                utilFolder += "\\experimental_steamclient";

                string exeFolder = Path.GetDirectoryName(handlerInstance.exePath);

                FileUtil.FileCheck(Path.Combine(exeFolder, "steamclient.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient.dll"), Path.Combine(exeFolder, "steamclient.dll"));

                FileUtil.FileCheck(Path.Combine(exeFolder, "steamclient64.dll"));
                File.Copy(Path.Combine(utilFolder, "steamclient64.dll"), Path.Combine(exeFolder, "steamclient64.dll"));

                FileUtil.FileCheck(Path.Combine(exeFolder, "steamclient_loader.exe"));
                File.Copy(Path.Combine(utilFolder, "steamclient_loader.exe"), Path.Combine(exeFolder, "steamclient_loader.exe"));

                handlerInstance.CurrentGameInfo.ExecutableToLaunch = Path.Combine(exeFolder, "steamclient_loader.exe");
                handlerInstance.CurrentGameInfo.ForceProcessSearch = true;
                handlerInstance.CurrentGameInfo.GoldbergWriteSteamIDAndAccount = true;

                if (i == 0)
                {
                    startingArgs = handlerInstance.CurrentGameInfo.StartArguments;
                }

                var sb = new StringBuilder();
                string gblines = sb.Append("#My own modified version of ColdClientLoader originally by Rat431")
                                .AppendLine()
                                .Append("[SteamClient]")
                                .AppendLine()
                                .Append($"Exe={handlerInstance.CurrentGameInfo.ExecutableName}")
                                .AppendLine()
                                .Append($"ExeRunDir=.")
                                .AppendLine()
                                .Append($"ExeCommandLine={startingArgs}")
                                .AppendLine()
                                .Append($"AppId={handlerInstance.CurrentGameInfo.SteamID}")
                                .AppendLine()
                                .AppendLine()
                                .Append("SteamClientDll=steamclient.dll")
                                .AppendLine()
                                .Append("SteamClient64Dll=steamclient64.dll")
                                .ToString();
                File.WriteAllText(Path.Combine(exeFolder, "ColdClientLoader.ini"), gblines);
                handlerInstance.addedFiles.Add(Path.Combine(exeFolder, "ColdClientLoader.ini"));

                //swalloing the launch arguments for the game here as we will be launching steamclient loader
                handlerInstance.CurrentGameInfo.StartArguments = string.Empty;
                handlerInstance.Context.StartArguments = string.Empty;

                string settingsFolder = exeFolder + "\\settings";
                if (handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                {
                    if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
                    {
                        settingsFolder = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                    else
                    {
                        settingsFolder = $@"{Globals.UserEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                }
                else
                {
                    File.WriteAllText(Path.Combine(exeFolder, "local_save.txt"), "");
                    handlerInstance.addedFiles.Add(Path.Combine(exeFolder, "local_save.txt"));
                }

                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }

                File.WriteAllText(Path.Combine(settingsFolder, "user_steam_id.txt"), "");
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "user_steam_id.txt"));

                File.WriteAllText(Path.Combine(settingsFolder, "account_name.txt"), "");
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "account_name.txt"));

                string lang = GetUserSteamLanguageChoice(handlerInstance.CurrentGameInfo);

                File.WriteAllText(Path.Combine(settingsFolder, "language.txt"), lang);
                handlerInstance.addedFiles.Add(Path.Combine(settingsFolder, "language.txt"));
            }
            else
            {
                string[] steamDllFiles = Directory.GetFiles(rootFolder, "steam_api*.dll", SearchOption.AllDirectories);

                foreach (string nameFile in steamDllFiles)
                {
                    handlerInstance.Log("Found " + nameFile);
                    steamDllrootFolder = Path.GetDirectoryName(nameFile);

                    string tempRootFolder = rootFolder;
                    if (tempRootFolder.EndsWith("\\"))
                    {
                        tempRootFolder = tempRootFolder.Substring(0, tempRootFolder.Length - 1);
                    }

                    steamDllFolder = steamDllrootFolder.Remove(0, (tempRootFolder.Length));

                    instanceSteamDllFolder = linkFolder.TrimEnd('\\') + "\\" + steamDllFolder.TrimStart('\\');

                    if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment && handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                    {
                        instanceSteamSettingsFolder = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\";
                    }
                    else
                    {
                        instanceSteamSettingsFolder = Path.Combine(instanceSteamDllFolder, "settings");
                    }

                    Directory.CreateDirectory(instanceSteamSettingsFolder);
                    //Directory.CreateDirectory(instanceSteam_SettingsFolder);

                    if (setupDll)
                    {
                        if (nameFile.EndsWith("steam_api64.dll", true, null))
                        {
                            try
                            {
                                if (File.Exists(Path.Combine(instanceSteamDllFolder, "steam_api64.dll")))
                                {
                                    if (handlerInstance.CurrentGameInfo.GoldbergExperimental && handlerInstance.CurrentGameInfo.GoldbergExperimentalRename)
                                    {
                                        FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "cracksteam_api64.dll"));
                                        handlerInstance.Log("Renaming steam_api64.dll to cracksteam_api64.dll");
                                        File.Move(Path.Combine(instanceSteamDllFolder, "steam_api64.dll"), Path.Combine(instanceSteamDllFolder, "cracksteam_api64.dll"));
                                    }
                                    else
                                    {
                                        FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "steam_api64.dll"));
                                    }
                                }

                                handlerInstance.Log("Placing Goldberg steam_api64.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, steam64Dll), Path.Combine(instanceSteamDllFolder, "steam_api64.dll"), true);
                            }
                            catch (Exception ex)
                            {
                                handlerInstance.Log("ERROR - " + ex.Message);
                                handlerInstance.Log("Using alternative copy method for steam_api64.dll");
                                CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steam64Dll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api64.dll") + "\"");
                            }
                        }

                        if (nameFile.EndsWith("steam_api.dll", true, null))
                        {
                            try
                            {
                                if (File.Exists(Path.Combine(instanceSteamDllFolder, "steam_api.dll")))
                                {
                                    if (handlerInstance.CurrentGameInfo.GoldbergExperimental && handlerInstance.CurrentGameInfo.GoldbergExperimentalRename)
                                    {
                                        FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "cracksteam_api.dll"));
                                        handlerInstance.Log("Renaming steam_api.dll to cracksteam_api.dll");
                                        File.Move(Path.Combine(instanceSteamDllFolder, "steam_api.dll"), Path.Combine(instanceSteamDllFolder, "cracksteam_api.dll"));
                                    }
                                    else
                                    {
                                        FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "steam_api.dll"));
                                    }
                                }
                                handlerInstance.Log("Placing Goldberg steam_api.dll in instance steam dll folder");
                                File.Copy(Path.Combine(utilFolder, steamDll), Path.Combine(instanceSteamDllFolder, "steam_api.dll"), true);
                            }
                            catch (Exception ex)
                            {
                                handlerInstance.Log("ERROR - " + ex.Message);
                                handlerInstance.Log("Using alternative copy method for steam_api.dll");
                                CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, steamDll) + "\" \"" + Path.Combine(instanceSteamDllFolder, "steam_api.dll") + "\"");
                            }
                        }

                        if (handlerInstance.CurrentGameInfo.GoldbergExperimental)
                        {
                            FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "steamclient.dll"));
                            handlerInstance.Log("Placing Goldberg steamclient.dll in instance steam dll folder");
                            File.Copy(Path.Combine(utilFolder, "experimental\\steamclient.dll"), Path.Combine(instanceSteamDllFolder, "steamclient.dll"), true);

                            FileUtil.FileCheck(Path.Combine(instanceSteamDllFolder, "steamclient64.dll"));
                            handlerInstance.Log("Placing Goldberg steamclient64.dll in instance steam dll folder");
                            File.Copy(Path.Combine(utilFolder, "experimental\\steamclient64.dll"), Path.Combine(instanceSteamDllFolder, "steamclient64.dll"), true);
                        }
                    }

                    if (!string.IsNullOrEmpty(prevSteamDllFilePath))
                    {
                        if (prevSteamDllFilePath == Path.GetDirectoryName(nameFile))
                        {
                            continue;
                        }
                    }

                    handlerInstance.Log("New steam api folder found");
                    prevSteamDllFilePath = Path.GetDirectoryName(nameFile);

                    if (App_Misc.UseNicksInGame && !string.IsNullOrEmpty(player.Nickname))
                    {
                        if (setupDll)
                        {
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                        }

                        File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                        handlerInstance.Log("Generating account_name.txt with nickname " + player.Nickname);
                    }
                    else
                    {
                        if (setupDll)
                        {
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"));
                        }

                        if (App_Misc.UseNicksInGame && Cache.PlayersIdentityCache.GetNicknameAt(i) != "")
                        {
                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), player.Nickname);
                            handlerInstance.Log("Generating account_name.txt with nickname " + player.Nickname);
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "account_name.txt"), "Player" + (i + 1));
                            handlerInstance.Log("Generating account_name.txt with nickname Player " + (i + 1));
                        }
                    }

                    long steamID = player.SteamID;

                    handlerInstance.Log("Generating user_steam_id.txt with user steam ID " + (steamID).ToString());

                    if (setupDll)
                    {
                        handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"));
                    }

                    File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "user_steam_id.txt"), (steamID).ToString());

                    if (setupDll)
                    {
                        string lang = GetUserSteamLanguageChoice(handlerInstance.CurrentGameInfo);

                        handlerInstance.Log("Generating language.txt with language set to " + lang);
                        handlerInstance.addedFiles.Add(Path.Combine(instanceSteamSettingsFolder, "language.txt"));
                        File.WriteAllText(Path.Combine(instanceSteamSettingsFolder, "language.txt"), lang);

                        if (handlerInstance.CurrentGameInfo.GoldbergIgnoreSteamAppId)
                        {
                            handlerInstance.Log("Skipping steam_appid.txt creation");
                        }
                        else
                        {
                            handlerInstance.Log("Generating steam_appid.txt using game steam ID " + handlerInstance.CurrentGameInfo.SteamID);
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"));
                            File.WriteAllText(Path.Combine(instanceSteamDllFolder, "steam_appid.txt"), handlerInstance.CurrentGameInfo.SteamID);
                        }

                        handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "local_save.txt"));
                        if (!handlerInstance.CurrentGameInfo.GoldbergNoLocalSave)
                        {
                            File.WriteAllText(Path.Combine(instanceSteamDllFolder, "local_save.txt"), "");
                        }

                        if (handlerInstance.CurrentGameInfo.GoldbergNeedSteamInterface)
                        {
                            handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"));
                            if (File.Exists(Path.Combine(utilFolder, "tools\\steam_interfaces.txt")))
                            {
                                handlerInstance.Log("Found generated steam_interfaces.txt file in Nucleus util folder, copying this file");

                                File.Copy(Path.Combine(utilFolder, "tools\\steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (File.Exists(Path.Combine(steamDllrootFolder, "steam_interfaces.txt")))
                            {
                                handlerInstance.Log("Found steam_interfaces.txt in original game folder, copying this file");
                                File.Copy(Path.Combine(steamDllrootFolder, "steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (File.Exists(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + handlerInstance.CurrentGameInfo.JsFileName.Substring(0, handlerInstance.CurrentGameInfo.JsFileName.Length - 3) + "\\steam_interfaces.txt"))
                            {
                                handlerInstance.Log("Found steam_interfaces.txt in Nucleus game folder");
                                File.Copy(nucleusRootFolder.TrimEnd('\\') + "\\handlers\\" + handlerInstance.CurrentGameInfo.JsFileName.Substring(0, handlerInstance.CurrentGameInfo.JsFileName.Length - 3) + "\\steam_interfaces.txt", Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);
                            }
                            else if (handlerInstance.CurrentGameInfo.OrigSteamDllPath?.Length > 0 && File.Exists(handlerInstance.CurrentGameInfo.OrigSteamDllPath))
                            {
                                handlerInstance.Log("Attempting to create steam_interfaces.txt with the steam api dll located at: " + handlerInstance.CurrentGameInfo.OrigSteamDllPath);
                                Process cmd = new Process();
                                cmd.StartInfo.FileName = "cmd.exe";
                                cmd.StartInfo.RedirectStandardInput = true;
                                cmd.StartInfo.RedirectStandardOutput = true;
                                cmd.StartInfo.UseShellExecute = false;
                                cmd.StartInfo.WorkingDirectory = Path.Combine(utilFolder, "tools");
                                cmd.Start();

                                cmd.StandardInput.WriteLine("generate_interfaces_file.exe \"" + handlerInstance.CurrentGameInfo.OrigSteamDllPath + "\"");
                                cmd.StandardInput.Flush();
                                cmd.StandardInput.Close();
                                cmd.WaitForExit();
                                handlerInstance.addedFiles.Add(Path.Combine(instanceSteamDllFolder, "steam_intterfaces.txt"));
                                handlerInstance.Log("Copying over generated steam_interfaces.txt");
                                File.Copy(Path.Combine(Path.Combine(utilFolder, "tools"), "steam_interfaces.txt"), Path.Combine(instanceSteamDllFolder, "steam_interfaces.txt"), true);

                                if ((i + 1) == players.Count)
                                {
                                    handlerInstance.Log("Deleting generated steam_interfaces.txt file");
                                    File.Delete(Path.Combine(utilFolder, "tools\\steam_interfaces.txt"));
                                }
                            }
                            else
                            {
                                handlerInstance.Log("Unable to locate steam_interfaces.txt or create one, skipping this file");

                                if (i == 0)
                                {
                                    MessageBox.Show("Goldberg was unable to locate steam_interfaces.txt or create one. Process will continue without using this file.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }

                if (steamDllFiles == null || steamDllFiles.Length < 1)
                {
                    if (!handlerInstance.CurrentGameInfo.GoldbergNoWarning)
                    {
                        handlerInstance.Log("Unable to locate a steam_api(64).dll file, Goldberg will not be used");
                        NucleusMessageBox.Show("Warning", "Goldberg was unable to locate a steam_api(64).dll file.\nThe built-in Goldberg will not be used.", false);
                    }
                }
            }

            handlerInstance.Log("Goldberg setup complete");
        }

        public static void GoldbergWriteSteamIDAndAccount(string linkFolder, int i, PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string[] saFiles = Directory.GetFiles(linkFolder, "account_name.txt", SearchOption.AllDirectories);
            List<string> files = saFiles.ToList();

            if (saFiles.Length > 0)
            {
                handlerInstance.Log("account_name.txt file(s) were found in folder. Will update nicknames");
            }

            string goldbergNoLocal = $@"{Globals.UserEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\account_name.txt";

            if (File.Exists(goldbergNoLocal))
            {
                files.Add(goldbergNoLocal);
            }

            if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
            {
                goldbergNoLocal = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\account_name.txt";
                if (File.Exists(goldbergNoLocal))
                {
                    files.Add(goldbergNoLocal);
                }
            }

            foreach (string nameFile in files)
            {
                if (!string.IsNullOrEmpty(player.Nickname))
                {
                    handlerInstance.Log(string.Format("Writing nickname {0} in account_name.txt", player.Nickname));
                    File.Delete(nameFile);
                    File.WriteAllText(nameFile, player.Nickname);
                }
                else
                {
                    handlerInstance.Log("Writing nickname {0} in account_name.txt " + "Player" + (i + 1));
                    File.Delete(nameFile);
                    File.WriteAllText(nameFile, "Player" + (i + 1));
                }
            }

            saFiles = Directory.GetFiles(linkFolder, "user_steam_id.txt", SearchOption.AllDirectories);
            files = saFiles.ToList();

            if (saFiles.Length > 0)
            {
                handlerInstance.Log("user_steam_id.txt file(s) were found in folder. Will update nicknames");
            }

            goldbergNoLocal = $@"{Globals.UserEnvironmentRoot}\AppData\Roaming\Goldberg SteamEmu Saves\settings\user_steam_id.txt";

            if (File.Exists(goldbergNoLocal))
            {
                files.Add(goldbergNoLocal);
            }

            if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
            {
                goldbergNoLocal = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming\Goldberg SteamEmu Saves\settings\user_steam_id.txt";

                if (File.Exists(goldbergNoLocal))
                {
                    files.Add(goldbergNoLocal);
                }
            }

            foreach (string nameFile in files)
            {
                long steamID = player.SteamID;      
                handlerInstance.Log("Generating user_steam_id.txt with user steam ID " + (steamID).ToString());

                File.Delete(nameFile);
                File.WriteAllText(nameFile, (steamID).ToString());
            }
        }

        public static void GoldbergLobbyConnect()
        {
            var handlerInstance = GenericGameHandler.Instance;

            Forms.Prompt prompt = new Forms.Prompt("Goldberg Lobby Connect: Press OK after you are hosting a game.");
            prompt.ShowDialog();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(Globals.NucleusInstallRoot, "utils\\GoldbergEmu\\lobby_connect\\lobby_connect.exe");

            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            Process p = Process.Start(startInfo);
            p.OutputDataReceived += ProcessUtil.proc_OutputDataReceived;
            p.BeginOutputReadLine();

            while (readToEnd == false)
            {
                if (GenericGameHandler.Instance.HasEnded)
                {
                    break;
                }

                Thread.Sleep(25);
            }

            try
            {
                Thread.Sleep(2500);
                p.Kill();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            if (!string.IsNullOrEmpty(lobbyConnectArg))
            {
                handlerInstance.Log("Goldberg Lobby Connect: Setting lobby ID to " + lobbyConnectArg.Substring(15));
            }
            else
            {
                handlerInstance.Log("Goldberg Lobby Connect: Could not find lobby ID");
                MessageBox.Show("Goldberg Lobby Connect: Could not find lobby ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CreateSteamAppIdByExe(bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Creating steam_appid.txt with steam ID " + handlerInstance.CurrentGameInfo.SteamID + " at " + handlerInstance.instanceExeFolder);

            if (File.Exists(Path.Combine(handlerInstance.instanceExeFolder, "steam_appid.txt")))
            {
                File.Delete(Path.Combine(handlerInstance.instanceExeFolder, "steam_appid.txt"));
            }

            File.WriteAllText(Path.Combine(handlerInstance.instanceExeFolder, "steam_appid.txt"), handlerInstance.CurrentGameInfo.SteamID);

            if (setupDll)
            {
                handlerInstance.addedFiles.Add(Path.Combine(handlerInstance.instanceExeFolder, "steam_appid.txt"));
            }
        }

        public static void SmartSteamEmu(PlayerInfo player, int i, string linkFolder, string startArgs, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string steamEmu = Path.Combine(linkFolder, "SmartSteamLoader");
            handlerInstance.ssePath = steamEmu;
            string sourcePath = Path.Combine(GameManager.Instance.GetUtilsPath(), "SmartSteamEmu");

            if (setupDll)
            {
                handlerInstance.Log("Setting up SmartSteamEmu");
                handlerInstance.Log(string.Format("Copying SmartSteamEmu files to {0}", steamEmu));

                try
                {
                    //Create all of the directories
                    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                        SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, steamEmu));
                    }

                    //Copy all the files & Replaces any files with the same name
                    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(sourcePath, steamEmu), true);
                    }
                }
                catch
                {
                    handlerInstance.Log(string.Format("ERROR - Copying of SmartSteamEmu files failed!"));
                    //return "Extraction of SmartSteamEmu failed!";
                }
            }

            string sseLoader = string.Empty;
            if (handlerInstance.gameIs64)
            {
                sseLoader = "SmartSteamLoader_x64.exe";
            }
            else
            {
                sseLoader = "SmartSteamLoader.exe";
            }

            string emuExe = Path.Combine(steamEmu, sseLoader);
            string emuIni = Path.Combine(steamEmu, "SmartSteamEmu.ini");
            IniFile emu = new IniFile(emuIni);

            handlerInstance.Log("Writing SmartSteamEmu.ini");
            emu.IniWriteValue("Launcher", "Target", handlerInstance.exePath);
            emu.IniWriteValue("Launcher", "StartIn", Path.GetDirectoryName(handlerInstance.exePath));
            emu.IniWriteValue("Launcher", "CommandLine", startArgs);
            emu.IniWriteValue("Launcher", "SteamClientPath", Path.Combine(steamEmu, "SmartSteamEmu.dll"));
            emu.IniWriteValue("Launcher", "SteamClientPath64", Path.Combine(steamEmu, "SmartSteamEmu64.dll"));
            emu.IniWriteValue("Launcher", "InjectDll", "1");

            emu.IniWriteValue("SmartSteamEmu", "AppId", handlerInstance.Context.SteamID);
            emu.IniWriteValue("SmartSteamEmu", "SteamIdGeneration", "Manual");

            long steamID = player.SteamID;
            emu.IniWriteValue("SmartSteamEmu", "ManualSteamId", steamID.ToString());

            string lang = GetUserSteamLanguageChoice(handlerInstance.CurrentGameInfo);

            emu.IniWriteValue("SmartSteamEmu", "Language", lang);

            if (App_Misc.UseNicksInGame && !string.IsNullOrEmpty(player.Nickname))
            {
                emu.IniWriteValue("SmartSteamEmu", "PersonaName", player.Nickname);
            }
            else
            {
                emu.IniWriteValue("SmartSteamEmu", "PersonaName", "Player" + (i + 1));
            }

            emu.IniWriteValue("SmartSteamEmu", "DisableOverlay", "1");
            emu.IniWriteValue("SmartSteamEmu", "SeparateStorageByName", "1");

            if (handlerInstance.CurrentGameInfo.SSEAdditionalLines?.Length > 0)
            {
                foreach (string line in handlerInstance.CurrentGameInfo.SSEAdditionalLines)
                {
                    if (line.Contains("|") && line.Contains("="))
                    {
                        string[] lineSplit = line.Split('|', '=');
                        if (lineSplit?.Length == 3)
                        {
                            string section = lineSplit[0];
                            string key = lineSplit[1];
                            string value = lineSplit[2];
                            handlerInstance.Log(string.Format("Writing custom line in SSE, section: {0}, key: {1}, value: {2}", section, key, value));
                            emu.IniWriteValue(section, key, value);
                        }
                    }
                }
            }

            if (handlerInstance.CurrentGameInfo.ForceEnvironmentUse && handlerInstance.CurrentGameInfo.ThirdPartyLaunch)
            {
                handlerInstance.Log("Force Nucleus environment use");

                IntPtr envPtr = IntPtr.Zero;

                if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
                {
                    handlerInstance.Log("Setting up Nucleus environment");
                    var sb = new StringBuilder();
                    System.Collections.IDictionary envVars = Environment.GetEnvironmentVariables();
                    string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                    envVars["USERPROFILE"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                    envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                    envVars["APPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                    envVars["LOCALAPPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                    //Some games will crash if the directories don't exist
                    Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
                    Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                    Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                    Directory.CreateDirectory(envVars["APPDATA"].ToString());
                    Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
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
                }
            }

            if (!handlerInstance.CurrentGameInfo.ThirdPartyLaunch)
            {
                if (handlerInstance.Context.KillMutex?.Length > 0)
                {
                    // to kill the mutexes we need to orphanize the process
                    while (!ProcessUtil.RunOrphanProcess(emuExe, handlerInstance.CurrentGameInfo.UseNucleusEnvironment, player.Nickname))
                    {
                        if (GenericGameHandler.Instance.HasEnded)
                        {
                            break;
                        }

                        Thread.Sleep(25);
                    }
                    handlerInstance.Log("Terminal session launched SmartSteamEmu as an orphan in order to kill mutexes in future");
                }
                else
                {
                    IntPtr envPtr = IntPtr.Zero;

                    if (handlerInstance.CurrentGameInfo.UseNucleusEnvironment)
                    {
                        handlerInstance.Log("Setting up Nucleus environment");
                        var sb = new StringBuilder();
                        System.Collections.IDictionary envVars = Environment.GetEnvironmentVariables();
                        string username = WindowsIdentity.GetCurrent().Name.Split('\\')[1];
                        envVars["USERPROFILE"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}";
                        envVars["HOMEPATH"] = $@"\Users\{username}\NucleusCoop\{player.Nickname}";
                        envVars["APPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Roaming";
                        envVars["LOCALAPPDATA"] = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\{player.Nickname}\AppData\Local";

                        //Some games will crash if the directories don't exist
                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
                        Directory.CreateDirectory(envVars["USERPROFILE"].ToString());
                        Directory.CreateDirectory(Path.Combine(envVars["USERPROFILE"].ToString(), "Documents"));
                        Directory.CreateDirectory(envVars["APPDATA"].ToString());
                        Directory.CreateDirectory(envVars["LOCALAPPDATA"].ToString());
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

                    }

                    ProcessUtil.STARTUPINFO startup = new ProcessUtil.STARTUPINFO();
                    startup.cb = Marshal.SizeOf(startup);

                    bool success = ProcessUtil.CreateProcess(null, emuExe, IntPtr.Zero, IntPtr.Zero, false, (uint)ProcessUtil.ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT, envPtr, Path.GetDirectoryName(handlerInstance.exePath), ref startup, out ProcessUtil.PROCESS_INFORMATION processInformation);

                    if (!success)
                    {
                        int error = Marshal.GetLastWin32Error();
                        handlerInstance.Log(string.Format("ERROR {0} - CreateProcess failed - startGamePath: {1}, startArgs: {2}, dirpath: {3}", error, handlerInstance.exePath, startArgs, Path.GetDirectoryName(handlerInstance.exePath)));
                    }

                    Process proc = Process.GetProcessById(processInformation.dwProcessId);
                    handlerInstance.Log(string.Format("Started process {0} (pid {1})", proc.ProcessName, proc.Id));
                }
            }
            else
            {
                handlerInstance.Log("Skipping launching of game via Nucleus for third party launch");
            }

            handlerInstance.Log("SmartSteamEmu setup complete");
        }

        public static void UseSteamStubDRMPatcher(bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (setupDll)
            {
                string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\Steam Stub DRM Patcher");

                string archToUse = handlerInstance.garch;
                if (handlerInstance.CurrentGameInfo.SteamStubDRMPatcherArch?.Length > 0)
                {
                    archToUse = "x" + handlerInstance.CurrentGameInfo.SteamStubDRMPatcherArch;
                }

                FileUtil.FileCheck(Path.Combine(handlerInstance.instanceExeFolder, "winmm.dll"));
                try
                {
                    handlerInstance.Log(string.Format("Copying over winmm.dll ({0})", archToUse));
                    File.Copy(Path.Combine(utilFolder, archToUse + "\\winmm.dll"), Path.Combine(handlerInstance.instanceExeFolder, "winmm.dll"), true);
                }

                catch (Exception ex)
                {
                    handlerInstance.Log("ERROR - " + ex.Message);
                    handlerInstance.Log("Using alternative copy method for winmm.dll");
                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, archToUse + "\\winmm.dll") + "\" \"" + Path.Combine(handlerInstance.instanceExeFolder, "winmm.dll") + "\"");
                }
            }
        }

        public static void SteamlessProc(string linkBinFolder, string executableName, string args, int timing)
        {
            try
            {
                string steamlessExePath = Path.Combine($@"{Directory.GetCurrentDirectory()}\utils\Steamless\Steamless.CLI.exe");
                string steamlessArgs = $@"{args} {linkBinFolder} \ {executableName}";
                var batch = Path.Combine(linkBinFolder, "steamless.bat");
                string content = $"{steamlessExePath} {steamlessArgs} \"{executableName}\"";

                File.WriteAllText(batch, content);

                ProcessStartInfo proc = new ProcessStartInfo();

                proc.FileName = batch;
                proc.UseShellExecute = false;
                proc.WorkingDirectory = Path.GetDirectoryName(batch);
                proc.CreateNoWindow = true;

                Process.Start(proc);

                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (!File.Exists($@"{linkBinFolder}\{executableName}.unpacked.exe") && watch.Elapsed.Seconds <= timing / 1000)
                {
                    Thread.Sleep(1000);
                }

                watch.Stop();

                if (File.Exists($@"{linkBinFolder}\{executableName}.unpacked.exe"))
                {
                    GenericGameHandler.Instance.Log($"Steamless patch applied successfully.");

                    File.Delete($@"{linkBinFolder}\{executableName}");
                    File.Move($@"{linkBinFolder}\{executableName}.unpacked.exe", $@"{linkBinFolder}\{executableName}");

                    if (File.Exists($@"{linkBinFolder}\{executableName}.exe"))
                    {
                        GenericGameHandler.Instance.Log($"Original executable successfully replaced by the Steamless patched version.");
                    }
                }
                else
                {
                    NucleusMessageBox.Show("Error", "Steamless failed at patching the executable. Please try again.", false);
                    GenericGameHandler.Instance.End(false);
                }

                if (File.Exists(batch))
                {
                    File.Delete(batch);
                }

            }
            catch (Exception ex)
            {
                GenericGameHandler.Instance.Log($"Error:\n{ex.Message}\n" +
                                                $"StackTrace:/n{ex.StackTrace}");
            };
        }

        public static void StartSteamClient()
        {
            if (Process.GetProcessesByName("steam").Length == 0)
            {
                Console.WriteLine("Steam Client Not Initialized");

                string steamExePath = string.Empty;
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry32);

                localKey = localKey.OpenSubKey(@"Software\Valve\Steam");

                if (localKey != null)
                {
                    steamExePath = localKey.GetValue("SteamExe").ToString();
                    localKey.Close();
                }

                ProcessStartInfo sc = new ProcessStartInfo(steamExePath);
                sc.UseShellExecute = true;
                sc.Arguments = "-silent";

                Process.Start(sc);
            }

            while (Process.GetProcessesByName("steamwebhelper").Length < 6) { }

            Console.WriteLine("Steam Client Initialized");
        }


        public static string GetSteamPath()
        {
            string steamPath = string.Empty;
            RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry32);

            localKey = localKey.OpenSubKey(@"Software\Valve\Steam");

            if (localKey != null)
            {
                steamPath = localKey.GetValue("SteamPath").ToString();
                localKey.Close();
            }

            string formatDrive = steamPath[0].ToString().ToUpper();
            string formatSlash = steamPath.Replace("/", "\\");
            string formatRemoveDrive = formatSlash.Remove(0,1);
            string formatedPath = formatDrive + formatRemoveDrive;

            return formatedPath;
        }
    }
}
