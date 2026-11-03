using Nucleus.Gaming.Coop;
using System;
using System.IO;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Nucleus.Gaming.Tools.SDL2Dll
{
    public static class SDL2Dll
    {
        /// <summary>
        /// The SDL dll Nucleus copies next to the instance executable for <c>Game.Hook.SDL2Enabled</c> or (Nucleus 2.5)
        /// <c>Game.Hook.SDL3Enabled</c>: <c>utils\SDL2|SDL3\x86|x64\SDL2|SDL3.dll</c>, a custom SDL build that reads the
        /// player's gamepad indexes from <c>SDL2.ini</c> / <c>SDL3.ini</c> in the same folder.
        /// </summary>
        public static string SourceDllPath(string installRoot, bool sdl2, string arch)
        {
            string version = sdl2 ? "SDL2" : "SDL3";
            return Path.Combine(installRoot, "utils", version, arch, version + ".dll");
        }

        public static void SetupSDLDll(PlayerInfo player, int i, bool setupDll)
        {
            var handlerInstance = GenericGameHandler.Instance;
            string gameGUID = handlerInstance.CurrentGameInfo.GUID;

            string gameContentPath = Path.Combine(GameManager.Instance.GetAppContentPath(), gameGUID);//game content root

            string dllVersion;

            if(handlerInstance.CurrentGameInfo.Hook.SDL2Enabled)
            {
                dllVersion = "SDL2";
            }
            else
            {
                dllVersion = "SDL3";
            }

            string dllsPath = Path.Combine(Globals.NucleusInstallRoot, $"utils\\{dllVersion}");
            string dllName = $"{dllVersion}.dll";

            handlerInstance.Log($"Setting up {dllVersion} for {player.Nickname} at index {i}...");

            if (setupDll)
            {
                //string controllerIndex;
                StringBuilder gpIndexes = new StringBuilder();

                if (player.IsSDL2 || player.IsSDL3)
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
                    File.WriteAllText(handlerInstance.instanceExeFolder + $"\\{dllVersion}.ini", gpIndexes.ToString());
                }  
                catch (Exception ex)
                {
                    handlerInstance.Log($"ERROR - Failed to write {dllVersion}.ini for {player.Nickname}(index {i}) at {handlerInstance.instanceExeFolder}" );
                    handlerInstance.Log("ERROR INFO - Message \n" + ex.Message);
                    return;
                }

                //utils\SDL3 is not shipped by every package (gitignored binaries): the game then keeps its own dll and every
                //instance sees every gamepad. Say so in the launch log instead of failing on the copy.
                string sourceDll = SourceDllPath(Globals.NucleusInstallRoot, dllVersion == "SDL2", handlerInstance.garch);
                if (!File.Exists(sourceDll))
                {
                    handlerInstance.Log($"ERROR - {dllName} not found ({sourceDll}): {dllVersion} gamepads can't be split between the instances, the game keeps its own {dllName}.");
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
