using Nucleus.Gaming.Coop;
using System.IO;

namespace Nucleus.Gaming.Forms
{
    public static class CustomPromptRuntime
    {
        public static string[] customValue;

        public static void CustomUserGeneralPrompts(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.Context.CustomUserGeneralValues == null || handlerInstance.Context.CustomUserGeneralValues?.Length < 1)
            {
                handlerInstance.Context.CustomUserGeneralValues = new string[handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length];
            }
            if (customValue == null || customValue?.Length < 1)
            {
                customValue = new string[handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length];
            }

            for (int c = 0; c < handlerInstance.Context.CustomUserGeneralValues.Length; c++)
            {
                handlerInstance.Context.CustomUserGeneralValues[c] = null;
            }

            string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.JsFileName) + "\\custom_gen_values.txt");

            int counter = 0;
            if (handlerInstance.CurrentGameInfo.SaveCustomUserGeneralValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserGeneralValues || player.PlayerID > 0)
            {
                handlerInstance.Log("Handler uses custom general values");
                if (File.Exists(valueFile))
                {
                    handlerInstance.Log("custom_gen_values.txt already exists for this handler, setting values accordingly");

                    string line;

                    StreamReader file = new StreamReader(valueFile);
                    while ((line = file.ReadLine()) != null)
                    {
                        handlerInstance.Log(string.Format("Custom value {0}: {1}", counter, line));
                        customValue[counter] = line;
                        handlerInstance.Context.CustomUserGeneralValues[counter] = line;
                        counter++;
                    }

                    file.Close();

                    if (counter != handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length)
                    {
                        handlerInstance.Log("Number of lines in file do not match number of prompts. Overwriting file");
                    }
                }
                else if (player.PlayerID == 0)
                {
                    handlerInstance.Log("custom_gen_values.txt does not exist. Creating new file at " + valueFile);
                }
            }
            else if (File.Exists(valueFile) && player.PlayerID == 0 && !handlerInstance.CurrentGameInfo.SaveCustomUserGeneralValues && !handlerInstance.CurrentGameInfo.SaveAndEditCustomUserGeneralValues)
            {
                handlerInstance.Log("Deleting value file");
                File.Delete(valueFile);
            }

            if (player.PlayerID == 0 && (!File.Exists(valueFile) || !handlerInstance.CurrentGameInfo.SaveCustomUserGeneralValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserGeneralValues || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length)))
            {

                if (player.PlayerID == 0 && ((File.Exists(valueFile) && !handlerInstance.CurrentGameInfo.SaveAndEditCustomUserGeneralValues && !handlerInstance.CurrentGameInfo.SaveCustomUserGeneralValues) || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length)))
                {
                    handlerInstance.Log("Deleting value file");
                    File.Delete(valueFile);
                }

                if (!Directory.Exists(Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.JsFileName))))
                {
                    Directory.CreateDirectory(Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.JsFileName)));
                }

                bool containsValue = false;
                for (int d = 0; d < handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length; d++)
                {
                    handlerInstance.Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts.Length, handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts[d]));
                    string prevAnswer = "";
                    if (d < customValue.Length && File.Exists(valueFile))
                    {
                        prevAnswer = customValue[d];
                    }
                    Forms.CustomPrompt prompt = new Forms.CustomPrompt(handlerInstance.CurrentGameInfo.CustomUserGeneralPrompts[d], prevAnswer, d);
                    prompt.ShowDialog();
                    if (customValue[d]?.Length > 0)
                    {
                        handlerInstance.Context.CustomUserGeneralValues[d] = customValue[d];
                        handlerInstance.Log("User entered: " + customValue[d]);
                        if (!containsValue)
                        {
                            containsValue = true;
                        }
                    }
                    else
                    {
                        handlerInstance.Log("User did not enter a value for this prompt");
                        handlerInstance.Context.CustomUserGeneralValues[d] = null;
                    }
                }

                if (containsValue)
                {
                    using (StreamWriter outputFile = new StreamWriter(valueFile))
                    {
                        foreach (string line in customValue)
                        {
                            outputFile.WriteLine(line);
                        }
                    }
                }
            }

        }

        public static void CustomUserPlayerPrompts(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.Context.CustomUserPlayerValues == null || handlerInstance.Context.CustomUserPlayerValues?.Length < 1)
            {
                handlerInstance.Context.CustomUserPlayerValues = new string[handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length];
            }
            if (customValue == null || customValue?.Length < 1)
            {
                customValue = new string[handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length];
            }

            string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.JsFileName) + "\\" + player.Nickname + "\\custom_plyr_values.txt");

            int counter = 0;
            if (handlerInstance.CurrentGameInfo.SaveCustomUserPlayerValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserPlayerValues)
            {
                handlerInstance.Log("Handler uses custom player values");
                if (File.Exists(valueFile))
                {
                    handlerInstance.Log("custom_plyr_values.txt already exists for this player, setting values accordingly");

                    string line;

                    StreamReader file = new StreamReader(valueFile);
                    while ((line = file.ReadLine()) != null)
                    {
                        handlerInstance.Log(string.Format("Custom value {0}: {1}", counter, line));
                        customValue[counter] = line;
                        handlerInstance.Context.CustomUserPlayerValues[counter] = line;
                        counter++;
                    }

                    file.Close();

                    if (counter != handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length)
                    {
                        handlerInstance.Log("Number of lines in file do not match number of prompts. Overwriting file");
                    }
                }
                else
                {
                    handlerInstance.Log("custom_plyr_values.txt does not exist for player " + player.Nickname + ". Creating new file at " + valueFile);
                }
            }

            if (!File.Exists(valueFile) || !handlerInstance.CurrentGameInfo.SaveCustomUserPlayerValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserPlayerValues || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length))
            {

                if ((File.Exists(valueFile) && !handlerInstance.CurrentGameInfo.SaveAndEditCustomUserPlayerValues && !handlerInstance.CurrentGameInfo.SaveCustomUserPlayerValues) || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length))
                {
                    handlerInstance.Log("Deleting value file");
                    File.Delete(valueFile);
                }

                if (!Directory.Exists(Path.GetDirectoryName(valueFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(valueFile));
                }

                for (int d = 0; d < handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length; d++)
                {
                    handlerInstance.Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts.Length, handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts[d]));
                    string prevAnswer = "";
                    if (d < customValue.Length && File.Exists(valueFile))
                    {
                        prevAnswer = customValue[d];
                    }
                    Forms.CustomPrompt prompt = new Forms.CustomPrompt(handlerInstance.CurrentGameInfo.CustomUserPlayerPrompts[d], prevAnswer, d);
                    prompt.ShowDialog();
                    handlerInstance.Context.CustomUserPlayerValues[d] = customValue[d];
                    handlerInstance.Log("User entered: " + customValue[d]);
                }

                using (StreamWriter outputFile = new StreamWriter(valueFile))
                {
                    foreach (string line in customValue)
                    {
                        outputFile.WriteLine(line);
                    }
                }
            }
        }

        public static void CustomUserInstancePrompts(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.Context.CustomUserInstanceValues == null || handlerInstance.Context.CustomUserInstanceValues?.Length < 1)
            {
                handlerInstance.Context.CustomUserInstanceValues = new string[handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length];
            }
            if (customValue == null || customValue?.Length < 1)
            {
                customValue = new string[handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length];
            }

            string valueFile = Path.Combine(GameManager.Instance.GetJsScriptsPath(), Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.JsFileName) + "\\instance " + player.PlayerID + "\\custom_inst_values.txt");

            int counter = 0;
            if (handlerInstance.CurrentGameInfo.SaveCustomUserInstanceValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserInstanceValues)
            {
                handlerInstance.Log("Handler uses custom instance values");
                if (File.Exists(valueFile))
                {
                    handlerInstance.Log("custom_inst_values.txt already exists for this player, setting values accordingly");

                    string line;

                    StreamReader file = new StreamReader(valueFile);
                    while ((line = file.ReadLine()) != null)
                    {
                        handlerInstance.Log(string.Format("Custom value {0}: {1}", counter, line));
                        customValue[counter] = line;
                        handlerInstance.Context.CustomUserInstanceValues[counter] = line;
                        counter++;
                    }

                    file.Close();

                    if (counter != handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length)
                    {
                        handlerInstance.Log("Number of lines in file do not match number of prompts. Overwriting file");
                    }
                }
                else
                {
                    handlerInstance.Log("custom_inst_values.txt does not exist for player " + player.Nickname + ". Creating new file at " + valueFile);
                }
            }

            if (!File.Exists(valueFile) || !handlerInstance.CurrentGameInfo.SaveCustomUserInstanceValues || handlerInstance.CurrentGameInfo.SaveAndEditCustomUserInstanceValues || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length))
            {
                if ((File.Exists(valueFile) && !handlerInstance.CurrentGameInfo.SaveAndEditCustomUserInstanceValues && !handlerInstance.CurrentGameInfo.SaveCustomUserInstanceValues) || (File.Exists(valueFile) && counter != handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length))
                {
                    handlerInstance.Log("Deleting value file");
                    File.Delete(valueFile);
                }

                if (!Directory.Exists(Path.GetDirectoryName(valueFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(valueFile));
                }

                for (int d = 0; d < handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length; d++)
                {
                    handlerInstance.Log(string.Format("Prompt {0}/{1}: {2}", (d + 1), handlerInstance.CurrentGameInfo.CustomUserInstancePrompts.Length, handlerInstance.CurrentGameInfo.CustomUserInstancePrompts[d]));
                    string prevAnswer = "";
                    if (d < customValue.Length && File.Exists(valueFile))
                    {
                        prevAnswer = customValue[d];
                    }
                    Forms.CustomPrompt prompt = new Forms.CustomPrompt(handlerInstance.CurrentGameInfo.CustomUserInstancePrompts[d], prevAnswer, d);
                    prompt.ShowDialog();
                    handlerInstance.Context.CustomUserInstanceValues[d] = customValue[d];
                    handlerInstance.Log("User entered: " + customValue[d]);
                }

                using (StreamWriter outputFile = new StreamWriter(valueFile))
                {
                    foreach (string line in customValue)
                    {
                        outputFile.WriteLine(line);
                    }
                }
            }
        }
    }
}
