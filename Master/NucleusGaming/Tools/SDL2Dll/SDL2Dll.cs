using Nucleus.Gaming.Coop;
using System;
using System.IO;

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
                string controllerIndex;

                if (player.IsSDL2)
                {
                    controllerIndex = (player.GamepadId).ToString();
                }
                else
                {
                    controllerIndex = (-1).ToString();
                }

                try
                {
                    File.WriteAllText(handlerInstance.instanceExeFolder + "\\SDL2.ini", controllerIndex);
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
