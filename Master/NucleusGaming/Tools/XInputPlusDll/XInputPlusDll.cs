using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nucleus.Gaming.Tools.XInputPlusDll
{
    public static class XInputPlusDll
    {

        public static void SetupXInputPlusDll(PlayerInfo player, int i, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Setting up XInput Plus");
            string utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\XInputPlus");

            if (handlerInstance.CurrentGameInfo.XInputPlusOldDll)
            {
                utilFolder = Path.Combine(Globals.NucleusInstallRoot, "utils\\XInputPlus\\old");
            }

            if (setupDll)
            {
                foreach (string xinputDllName in handlerInstance.CurrentGameInfo.XInputPlusDll)
                {
                    string xinputDll = "xinput1_3.dl_";

                    FileUtil.FileCheck(Path.Combine(handlerInstance.instanceExeFolder, xinputDllName));
                    try
                    {
                        if (xinputDllName.ToLower().StartsWith("dinput."))
                        {
                            xinputDll = "Dinput.dl_";
                        }
                        else if (xinputDllName.ToLower().StartsWith("dinput8."))
                        {
                            xinputDll = "Dinput8.dl_";
                        }

                        handlerInstance.Log("Using " + xinputDll + " (" + handlerInstance.garch + ") as base and naming it: " + xinputDllName);

                        File.Copy(Path.Combine(utilFolder, handlerInstance.garch + "\\" + xinputDll), Path.Combine(handlerInstance.instanceExeFolder, xinputDllName), true);
                    }
                    catch (Exception ex)
                    {
                        handlerInstance.Log("ERROR - " + ex.Message);
                        handlerInstance.Log("Using alternative copy method for " + xinputDll);
                        CmdUtil.ExecuteCommand(utilFolder, out int exitCode, "copy \"" + Path.Combine(utilFolder, handlerInstance.garch + "\\" + xinputDll) + "\" \"" + Path.Combine(handlerInstance.instanceExeFolder, xinputDllName) + "\"");
                    }
                }
            }

            if (!handlerInstance.CurrentGameInfo.XInputPlusNoIni)
            {
                List<string> textChanges = new List<string>();

                if (player.IsController || (player.IsKeyboardPlayer && handlerInstance.CurrentGameInfo.PlayersPerInstance <= 1))
                {
                    if (setupDll)
                    {
                        handlerInstance.addedFiles.Add(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"));
                    }

                    handlerInstance.Log("Copying XInputPlus.ini");

                    File.Copy(Path.Combine(utilFolder, "XInputPlus.ini"), Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), true);

                    handlerInstance.Log("Making changes to the lines in XInputPlus.ini; FileVersion and Controller values");

                    handlerInstance.CurrentGameInfo.XInputPlusDll = Array.ConvertAll(handlerInstance.CurrentGameInfo.XInputPlusDll, x => x.ToLower());

                    if (handlerInstance.CurrentGameInfo.XInputPlusDll.ToList().Any(val => val.StartsWith("dinput") == true)) //(xinputDll.ToLower().StartsWith("dinput"))
                    {
                        handlerInstance.Log("A Dinput dll has been detected, also enabling X2Dinput in XInputPlus.ini");
                        textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "EnableX2Dinput=", SearchType.StartsWith) + "|EnableX2Dinput=True");
                    }

                    textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "FileVersion=", SearchType.StartsWith) + "|FileVersion=" + handlerInstance.garch);

                    if (player.IsController)
                    {
                        if (handlerInstance.CurrentGameInfo.PlayersPerInstance > 1 && handlerInstance.profile.DevicesList.Any(pl => pl.InstanceGuests.Count > 0 ))
                        {
                            #region New multiple player per instance with UI assignation support

                            //instance host
                            textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "Controller1" + "=", SearchType.StartsWith) + "|Controller1" + "=" + (player.GamepadId + 1));

                            if (player.InstanceGuests.Count > 0)
                            {
                                //guest
                                for (int x = 0; x < player.InstanceGuests.Count; x++)
                                {
                                    int guestControllerIndex = player.InstanceGuests[x].GamepadId + 1;

                                    textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "Controller" + (x + 2)  + "=", SearchType.StartsWith) + "|Controller" + (x + 2) + "=" + (guestControllerIndex));
                                }
                            }
                            #endregion
                        }
                        else if (handlerInstance.CurrentGameInfo.PlayersPerInstance > 1)//the old way
                        {
                            for (int x = 1; x <= handlerInstance.CurrentGameInfo.PlayersPerInstance; x++)
                            {
                                textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "Controller" + x + "=", SearchType.StartsWith) + "|Controller" + x + "=" + (x + handlerInstance.plyrIndex));
                            }

                            handlerInstance.plyrIndex += handlerInstance.CurrentGameInfo.PlayersPerInstance;
                        }
                        else
                        {
                            textChanges.Add(handlerInstance.context.FindLineNumberInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), "Controller1=", SearchType.StartsWith) + "|Controller1=" + (player.GamepadId + 1));
                        }
                    }
                    else
                    {
                        handlerInstance.Log("Skipping setting controller value for this instance, as this player is using keyboard");
                        handlerInstance.kbi = 0;
                    }

                    handlerInstance.context.ReplaceLinesInTextFile(Path.Combine(handlerInstance.instanceExeFolder, "XInputPlus.ini"), textChanges.ToArray());
                }
            }

            handlerInstance.Log("XInput Plus setup complete");
        }

        public static void CustomDllEnabled(PlayerInfo player, Rectangle playerBounds, int i, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (setupDll)
            {
                handlerInstance.Log(string.Format("Setting up Custom DLL, UseAlpha8CustomDll: {0}", handlerInstance.CurrentGameInfo.Hook.UseAlpha8CustomDll));

                byte[] xdata;

                if (handlerInstance.CurrentGameInfo.Hook.UseAlpha8CustomDll && !handlerInstance.gameIs64)
                {
                    xdata = Properties.Resources.xinput1_3;
                }
                else
                {
                    if (handlerInstance.CurrentGameInfo.Hook.UseAlpha8CustomDll)
                    {
                        handlerInstance.Log("Using Alpha 10 custom dll as there is no Alpha 8 x64 custom dll");
                    }

                    if (handlerInstance.gameIs64)
                    {
                        xdata = Properties.Resources.xinput1_3_a10_x64;
                    }
                    else
                    {
                        xdata = Properties.Resources.xinput1_3_a10;
                    }
                }

                if (handlerInstance.context.Hook.XInputNames == null)
                {
                    string ogFile = Path.Combine(handlerInstance.instanceExeFolder, "xinput1_3.dll");
                    FileUtil.FileCheck(ogFile);
                    handlerInstance.Log(string.Format("Writing custom dll xinput1_3.dll to {0}", handlerInstance.instanceExeFolder));

                    using (Stream str = File.OpenWrite(ogFile))
                    {
                        str.Write(xdata, 0, xdata.Length);
                    }
                }
                else
                {
                    string[] xinputs = handlerInstance.context.Hook.XInputNames;

                    for (int z = 0; z < xinputs.Length; z++)
                    {
                        string xinputName = xinputs[z];
                        string ogFile = Path.Combine(handlerInstance.instanceExeFolder, xinputName);

                        FileUtil.FileCheck(ogFile);
                        handlerInstance.Log(string.Format("Writing custom dll {0} to {1}", xinputName, handlerInstance.instanceExeFolder));
                        using (Stream str = File.OpenWrite(Path.Combine(handlerInstance.instanceExeFolder, xinputName)))
                        {
                            str.Write(xdata, 0, xdata.Length);
                        }
                    }
                }
            }

            handlerInstance.Log(string.Format("Writing ncoop.ini to {0} with Game.Hook values", handlerInstance.instanceExeFolder));
            string ncoopIni = Path.Combine(handlerInstance.instanceExeFolder, "ncoop.ini");

            using (Stream str = File.OpenWrite(ncoopIni))
            {
                byte[] ini = Properties.Resources.ncoop;
                str.Write(ini, 0, ini.Length);
            }

            FileUtil.FileCheck(Path.Combine(handlerInstance.instanceExeFolder, "ncoop.ini"));
            IniFile x360 = new IniFile(ncoopIni);

            x360.IniWriteValue("Options", "Log", "0");
            x360.IniWriteValue("Options", "FileLog", "0");
            x360.IniWriteValue("Options", "ForceFocus", handlerInstance.CurrentGameInfo.Hook.ForceFocus.ToString(CultureInfo.InvariantCulture));

            if (!handlerInstance.CurrentGameInfo.Hook.UseAlpha8CustomDll)
            {
                x360.IniWriteValue("Options", "Version", "2");
                x360.IniWriteValue("Options", "ForceFocusWindowRegex", handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                string windowTitle = handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName;
                if (handlerInstance.CurrentGameInfo.IdInWindowTitle || handlerInstance.CurrentGameInfo.FlawlessWidescreen?.Length > 0)
                {
                    windowTitle = handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName + "(" + i + ")";
                    if (!string.IsNullOrEmpty(handlerInstance.CurrentGameInfo.FlawlessWidescreen))
                    {
                        windowTitle = "Nucleus Instance " + (i + 1) + "(" + handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName + ")";
                    }
                }
                x360.IniWriteValue("Options", "ForceFocusWindowName", windowTitle.ToString(CultureInfo.InvariantCulture));
            }

            int wx;
            int wy;
            int rw;
            int rh;

            if (handlerInstance.context.Hook.WindowX > 0 && handlerInstance.context.Hook.WindowY > 0)
            {
                wx = handlerInstance.context.Hook.WindowX;
                wy = handlerInstance.context.Hook.WindowY;
                x360.IniWriteValue("Options", "WindowX", handlerInstance.context.Hook.WindowX.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "WindowY", handlerInstance.context.Hook.WindowY.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                wx = playerBounds.X;
                wy = playerBounds.Y;
                x360.IniWriteValue("Options", "WindowX", playerBounds.X.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "WindowY", playerBounds.Y.ToString(CultureInfo.InvariantCulture));
            }

            if (handlerInstance.context.Hook.ResWidth > 0 && handlerInstance.context.Hook.ResHeight > 0)
            {
                rw = handlerInstance.context.Hook.ResWidth;
                rh = handlerInstance.context.Hook.ResHeight;
                x360.IniWriteValue("Options", "ResWidth", handlerInstance.context.Hook.ResWidth.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ResHeight", handlerInstance.context.Hook.ResHeight.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                rw = handlerInstance.context.Width;
                rh = handlerInstance.context.Height;
                x360.IniWriteValue("Options", "ResWidth", handlerInstance.context.Width.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ResHeight", handlerInstance.context.Height.ToString(CultureInfo.InvariantCulture));
            }

            if (!handlerInstance.CurrentGameInfo.Hook.UseAlpha8CustomDll)
            {
                if (handlerInstance.context.Hook.FixResolution)
                {
                    handlerInstance.Log(string.Format("Custom DLL will be doing the resizing with values width:{0}, height:{1}", rw, rh));
                    handlerInstance.dllResize = true;
                }
                if (handlerInstance.context.Hook.FixPosition)
                {
                    handlerInstance.Log(string.Format("Custom DLL will be doing the repositioning with values x:{0}, y:{1}", wx, wy));
                    handlerInstance.dllRepos = true;
                }
                x360.IniWriteValue("Options", "FixResolution", handlerInstance.context.Hook.FixResolution.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "FixPosition", handlerInstance.context.Hook.FixPosition.ToString(CultureInfo.InvariantCulture));
                x360.IniWriteValue("Options", "ClipMouse", player.IsKeyboardPlayer.ToString(CultureInfo.InvariantCulture)); //context.Hook.ClipMouse
            }

            x360.IniWriteValue("Options", "RerouteInput", handlerInstance.context.Hook.XInputReroute.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "RerouteJoystickTemplate", JoystickDatabase.GetID(player.GamepadProductGuid.ToString()).ToString(CultureInfo.InvariantCulture));

            if (handlerInstance.context.Hook.EnableMKBInput || player.IsKeyboardPlayer)
            {
                handlerInstance.Log("Enabling MKB");
                x360.IniWriteValue("Options", "EnableMKBInput", "True".ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                x360.IniWriteValue("Options", "EnableMKBInput", "False".ToString(CultureInfo.InvariantCulture));
            }

            x360.IniWriteValue("Options", "IsKeyboardPlayer", player.IsKeyboardPlayer.ToString(CultureInfo.InvariantCulture));
            // windows events
            x360.IniWriteValue("Options", "BlockInputEvents", handlerInstance.context.Hook.BlockInputEvents.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "BlockMouseEvents", handlerInstance.context.Hook.BlockMouseEvents.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "BlockKeyboardEvents", handlerInstance.context.Hook.BlockKeyboardEvents.ToString(CultureInfo.InvariantCulture));
            // xinput
            x360.IniWriteValue("Options", "XInputEnabled", handlerInstance.context.Hook.XInputEnabled.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "XInputPlayerID", player.GamepadId.ToString(CultureInfo.InvariantCulture));
            // dinput
            x360.IniWriteValue("Options", "DInputEnabled", handlerInstance.context.Hook.DInputEnabled.ToString(CultureInfo.InvariantCulture));
            x360.IniWriteValue("Options", "DInputGuid", player.GamepadGuid.ToString().ToUpper());
            x360.IniWriteValue("Options", "DInputForceDisable", handlerInstance.context.Hook.DInputForceDisable.ToString());

            handlerInstance.Log("Custom DLL setup complete");
        }

    }
}
