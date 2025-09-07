using Nucleus.Gaming.Coop;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Nucleus.Gaming.Tools.X360ce
{
    public static class X360ce
    {
        public static void UseX360ce(int i, PlayerInfo player, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;
            var players = GameProfile.Instance.DevicesList;

            handlerInstance.Log("Setting up x360ce");
            string x360exe = "";
            string x360dll = "";
            string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\x360ce");

            string[] x360cedlls = { "xinput1_3.dll" };
            if (handlerInstance.CurrentGameInfo.X360ceDll?.Length > 0)
            {
                x360cedlls = handlerInstance.CurrentGameInfo.X360ceDll;
            }

            if (setupDll)
            {
                foreach (string x360ceDllName in x360cedlls)
                {
                    if (i == 0)
                    {
                        if (handlerInstance.gameIs64)
                        {
                            x360exe = "x360ce_x64.exe";
                            x360dll = "xinput1_3_x64.dll";
                        }

                        else //if (Is64Bit(exePath) == false)
                        {
                            x360exe = "x360ce.exe";
                            x360dll = "xinput1_3.dll";
                        }

                        if (x360ceDllName.ToLower().StartsWith("dinput"))
                        {
                            if (handlerInstance.gameIs64)
                            {
                                x360dll = "dinput8_x64.dll";
                            }
                            else
                            {
                                x360dll = "dinput8.dll";
                            }
                        }

                        string ogFile = Path.Combine(handlerInstance.instanceExeFolder, x360exe);
                        FileUtil.FileCheck(ogFile);

                        handlerInstance.Log("Copying over " + x360exe);

                        try
                        {
                            File.Copy(Path.Combine(utilFolder, x360exe), Path.Combine(handlerInstance.instanceExeFolder, x360exe), true);
                        }
                        catch
                        {
                            handlerInstance.Log("Using alternative copy method");
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, x360exe) + "\" \"" + Path.Combine(handlerInstance.instanceExeFolder, x360exe) + "\"");
                        }

                        ogFile = Path.Combine(handlerInstance.instanceExeFolder, x360ceDllName);
                        FileUtil.FileCheck(ogFile);

                        if (x360dll != x360ceDllName)
                        {
                            handlerInstance.Log("Copying over " + x360dll + " and renaming it to " + x360ceDllName);
                        }
                        else
                        {
                            handlerInstance.Log("Copying over " + x360dll);
                        }
                        try
                        {
                            File.Copy(Path.Combine(utilFolder, x360dll), ogFile, true);
                        }
                        catch
                        {
                            handlerInstance.Log("Using alternative copy method");
                            CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, x360dll) + "\" \"" + ogFile + "\"");
                        }

                    }
                    else
                    {
                        if (handlerInstance.CurrentGameInfo.SymlinkGame || handlerInstance.CurrentGameInfo.HardlinkGame || handlerInstance.CurrentGameInfo.HardcopyGame)
                        {
                            handlerInstance.Log("Carrying over " + x360ceDllName + " from Instance0");
                            FileUtil.FileCheck(Path.Combine(handlerInstance.instanceExeFolder, x360ceDllName));
                            File.Copy(Path.Combine(handlerInstance.instanceExeFolder.Substring(0, handlerInstance.instanceExeFolder.LastIndexOf('\\') + 1) + "Instance0", x360ceDllName), Path.Combine(handlerInstance.instanceExeFolder, x360ceDllName), true);
                        }
                    }
                }
            }

            if (i > 0 && (handlerInstance.CurrentGameInfo.SymlinkGame || handlerInstance.CurrentGameInfo.HardlinkGame || handlerInstance.CurrentGameInfo.HardcopyGame))
            {
                handlerInstance.Log("Carrying over x360ce.ini from Instance0");

                FileUtil.FileCheck(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"));
                File.Copy(Path.Combine(handlerInstance.instanceExeFolder.Substring(0, handlerInstance.instanceExeFolder.LastIndexOf('\\') + 1) + "Instance0", "x360ce.ini"), Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), true);
            }
            else
            {
                handlerInstance.Log("Starting x360ce process");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = handlerInstance.instanceExeFolder;
                startInfo.FileName = Path.Combine(handlerInstance.instanceExeFolder, x360exe);
                Process util = Process.Start(startInfo);
                handlerInstance.Log("Waiting until x360ce process is exited");
                util.WaitForExit();
            }

            if (!File.Exists(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini")))
            {
                handlerInstance.Log("x360ce.ini has not been generated. Copying default x360ce.ini from utils");
                try
                {
                    File.Copy(Path.Combine(utilFolder, "x360ce.ini"), Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), true);
                }
                catch
                {
                    handlerInstance.Log("Using alternative copy method");
                    CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, "x360ce.ini") + "\" \"" + Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini") + "\"");
                }
            }

            handlerInstance.Log("Making changes to x360ce.ini; PAD mapping to player");

            List<string> textChanges = new List<string>();

            if (!player.IsKeyboardPlayer)
            {
                Thread.Sleep(1000);
                if (handlerInstance.CurrentGameInfo.PlayersPerInstance > 1)
                {
                    for (int x = 1; x <= handlerInstance.CurrentGameInfo.PlayersPerInstance; x++)
                    {
                        textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD" + x + "=", SearchType.StartsWith) + "|PAD" + x + "=IG_" + players[x].GamepadGuid.ToString().Replace("-", string.Empty));
                    }
                    for (int x = handlerInstance.CurrentGameInfo.PlayersPerInstance + 1; x <= 4; x++)
                    {
                        textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD" + x + "=", SearchType.StartsWith) + "|PAD" + x + "=IG_" + players[x].GamepadGuid.ToString().Replace("-", string.Empty));
                    }

                    handlerInstance.plyrIndex += handlerInstance.CurrentGameInfo.PlayersPerInstance;
                }
                else
                {
                    textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD1=", SearchType.StartsWith) + "|PAD1=" + handlerInstance.Context.x360ceGamepadGuid);
                    textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD2=", SearchType.StartsWith) + "|PAD2=");
                    textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD3=", SearchType.StartsWith) + "|PAD3=");
                    textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "PAD4=", SearchType.StartsWith) + "|PAD4=");
                }

                handlerInstance.Context.ReplaceLinesInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), textChanges.ToArray());
            }

            if (handlerInstance.CurrentGameInfo.XboxOneControllerFix)
            {
                Thread.Sleep(1000);
                handlerInstance.Log("Implementing Xbox One controller fix");

                textChanges.Clear();
                textChanges.Add(handlerInstance.Context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), "HookMode=1", SearchType.Full) + "|" +
                    "HookLL=0\n" +
                    "HookCOM=1\n" +
                    "HookSA=0\n" +
                    "HookWT=0\n" +
                    "HOOKDI=1\n" +
                    "HOOKPIDVID=1\n" +
                    "HookName=0\n" +
                    "HookMode=0\n"
                );

                handlerInstance.Context.ReplaceLinesInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"), textChanges.ToArray());
            }

            if (File.Exists(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini")) && !handlerInstance.CurrentGameInfo.SymlinkGame && !handlerInstance.CurrentGameInfo.HardlinkGame && !handlerInstance.CurrentGameInfo.HardcopyGame)
            {
                handlerInstance.Log("x360ce.ini will be deleted upon ending session");
                handlerInstance.addedFiles.Add(Path.Combine(handlerInstance.instanceExeFolder, "x360ce.ini"));
            }

            handlerInstance.Log("x360ce setup complete");
        }

    }
}
