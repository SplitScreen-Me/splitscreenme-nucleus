using Nucleus.Gaming.Coop;
using System.IO;

namespace Nucleus.Gaming.Util
{
    public static class ExecutableUtil
    {
        public static void ChangeExeName(UserGameInfo userGame, string instanceExeFolder, int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            string newExe = Path.GetFileNameWithoutExtension(userGame.HandlerInfo.GenericGameInfo.ExecutableName) + " - Player " + (i + 1) + ".exe";

            if (File.Exists(Path.Combine(instanceExeFolder, userGame.HandlerInfo.GenericGameInfo.ExecutableName)))
            {
                if (File.Exists(Path.Combine(instanceExeFolder, newExe)))
                {
                    File.Delete(Path.Combine(instanceExeFolder, newExe));
                }

                File.Copy(Path.Combine(instanceExeFolder, userGame.HandlerInfo.GenericGameInfo.ExecutableName), Path.Combine(instanceExeFolder, newExe));
                handlerInstance.Log("Changed game executable from " + handlerInstance.CurrentGameInfo.ExecutableName + " to " + newExe);
            }

            if (File.Exists(Path.Combine(instanceExeFolder, newExe)))
            {
                handlerInstance.exePath = Path.Combine(instanceExeFolder, newExe);
                handlerInstance.Log("Using " + newExe + " as the game executable");

                if (!handlerInstance.CurrentGameInfo.SymlinkGame && !handlerInstance.CurrentGameInfo.HardlinkGame && !handlerInstance.CurrentGameInfo.HardcopyGame)
                {
                    handlerInstance.Log($"{newExe} will be deleted upon ending session");
                    handlerInstance.addedFiles.Add(Path.Combine(instanceExeFolder, newExe));
                }
                else
                {
                    if (File.Exists(Path.Combine(instanceExeFolder, userGame.HandlerInfo.GenericGameInfo.ExecutableName)))
                    {
                        File.Delete(Path.Combine(instanceExeFolder, userGame.HandlerInfo.GenericGameInfo.ExecutableName));
                    }
                }
            }
        }
    }
}
