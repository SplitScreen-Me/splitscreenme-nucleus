﻿using Nucleus.Gaming.Coop;
using System;
using System.IO;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Nucleus.Gaming.Tools.SDL2Dll
{
    public static class SDL2Dll
    {
        public static void SetupSDL2Dll(PlayerInfo player, int i, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string gameGUID = handlerInstance.CurrentGameInfo.GUID;

            string gameContentPath = Path.Combine(GameManager.Instance.GetAppContentPath(), gameGUID);//game content root

            string dllsPath = Path.Combine(Globals.NucleusInstallRoot, "utils\\SDL2");
            string dllName = "SDL2.dll";

            handlerInstance.Log($"Setting up SDL2 for {player.Nickname} at index {i}...");

            if (setupDll)
            {
                //string controllerIndex;
                StringBuilder gpIndexes = new StringBuilder();

                if (player.IsSDL2)
                {
                    gpIndexes.Append((player.GamepadId).ToString());

                    foreach(PlayerInfo guest in player.InstanceGuests)
                    {
                        gpIndexes.Append(" ");
                        gpIndexes.Append(guest.GamepadId);
                    }
                }
                else
                {
                    gpIndexes.Append("-1");
                }

                try
                {
                    File.WriteAllText(handlerInstance.instanceExeFolder + "\\SDL2.ini", gpIndexes.ToString());
                }  
                catch (Exception ex)
                {
                    handlerInstance.Log($"ERROR - Failed to write SDL2.ini for {player.Nickname}(index {i}) at {handlerInstance.instanceExeFolder}" );
                    handlerInstance.Log("ERROR INFO - Message \n" + ex.Message);
                    return;
                }


                if (handlerInstance.CurrentGameInfo.SDLPaths?.Length > 0)
                {
                    foreach (string dllPath in handlerInstance.CurrentGameInfo.SDLPaths)
                    {
                        string destInstance = $"{gameContentPath}\\Instance{i}\\{dllPath}";
                        try
                        {                 
                            File.Copy(Path.Combine(dllsPath, handlerInstance.garch + "\\" + dllName), Path.Combine(destInstance, dllName), true);
                        }
                        catch (Exception ex)
                        {
                            handlerInstance.Log("ERROR - " + ex.Message);
                            handlerInstance.Log("Using alternative copy method for " + dllName);
                            CmdUtil.ExecuteCommand(dllsPath, out int exitCode, "copy \"" + Path.Combine(dllsPath, handlerInstance.garch + "\\" + dllName) + "\" \"" + Path.Combine(destInstance, dllName) + "\"");
                        }
                    }
                }
                else
                {
                    try
                    {
                        File.Copy(Path.Combine(dllsPath, handlerInstance.garch + "\\" + dllName), Path.Combine(handlerInstance.instanceExeFolder, dllName), true);
                    }
                    catch (Exception ex)
                    {
                        handlerInstance.Log("ERROR - " + ex.Message);
                        handlerInstance.Log("Using alternative copy method for " + dllName);
                        CmdUtil.ExecuteCommand(dllsPath, out int exitCode, "copy \"" + Path.Combine(dllsPath, handlerInstance.garch + "\\" + dllName) + "\" \"" + Path.Combine(handlerInstance.instanceExeFolder, dllName) + "\"");
                    }
                }
            }

        }
    
    }
}
