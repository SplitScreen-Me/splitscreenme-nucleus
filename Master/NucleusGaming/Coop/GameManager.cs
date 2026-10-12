using Microsoft.Win32;
using Newtonsoft.Json;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Manages games information, so we can know what games are supported 
    /// and how to support it
    /// </summary>
    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        private bool isSaving;

        private RawInputProcessor rawInputProcessor;
        private static RawInputProcessor sharedRawInputProcessor;

        /// object instance so we can thread-safe save the user profile
        private object saving = new object();

        public bool IsSaving => isSaving;

        /// <summary>
        /// A dictionary containing GameInfos. The key is the game's guid
        /// </summary>
        private Dictionary<string, GenericGameInfo> games;
        public Dictionary<string, GenericGameInfo> Games => games;

        private Dictionary<string, GenericGameInfo> gameInfos;
        public Dictionary<string, GenericGameInfo> GameInfos => gameInfos;

        private UserProfile user;
        public UserProfile User
        {
            get => user;
            set => user = value;
        }

        public GameManager()
        {
            Instance = this;
            games = new Dictionary<string, GenericGameInfo>();
            gameInfos = new Dictionary<string, GenericGameInfo>();

            string appData = GetAppContentPath();
            Directory.CreateDirectory(appData);

            string gameJs = GetJsScriptsPath();
            Directory.CreateDirectory(gameJs);

            //Subscribe to raw input
            //TODO: update isRunningSplitScreen
            //Raw input is registered once per process: a new GameManager (handlers refresh) reuses the window and thread
            //instead of starting a second one that steals the registration.
            if (sharedRawInputProcessor == null)
            {
                Debug.WriteLine("Registering raw input");
                sharedRawInputProcessor = new RawInputProcessor(() => LockInputRuntime.IsLocked);//TODO: needs more robust method
                                                                                    //Action<IntPtr> rawInputAction = rawInputProcessor.Process;
                                                                                    //GameManager.mainForm.GetType().GetProperty("RawInputAction").SetValue(GameManager.mainForm, rawInputAction, new object[] { });
                                                                                    //IntPtr rawInputHwnd = GameManager.mainFormHandle;

                RawInputManager.RegisterRawInput(sharedRawInputProcessor);
            }

            rawInputProcessor = sharedRawInputProcessor;

            Initialize();
            LoadUser();
        }

        /// <summary>
        /// Tries to find the game
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public GenericGameInfo GetGame(string exePath)
        {
            string lower = exePath.ToLower();
            string fileName = Path.GetFileName(exePath).ToLower();
            string dir = Path.GetDirectoryName(exePath);

            List<GenericGameInfo> possibilities = new List<GenericGameInfo>();

            //a workaround so we can choose different game executables.
            foreach (var gen in Games.Values)
            {
                if (gen.ExecutableNames != null)
                {
                    if (gen.ExecutableNames.Any(exe => exe.ToLower() == fileName))
                    {
                        possibilities.Add(gen);
                    }
                }
            }

            if (possibilities.Count() == 0)
            {
                var getPossiblities = Games.Values.Where(c => c.ExecutableName?.ToLower() == fileName).ToList();

                if (getPossiblities != null)
                {
                    possibilities = getPossiblities;
                }
            }

            foreach (GenericGameInfo game in possibilities)
            {
                // check if the Context matches
                string[] context = game.ExecutableContext;
                bool notAdd = false;
                if (context != null)
                {
                    for (int j = 0; j < context.Length; j++)
                    {
                        string con = Path.Combine(dir, context[j]);
                        if (!File.Exists(con) &&
                            !Directory.Exists(con))
                        {
                            notAdd = true;
                            break;
                        }
                    }
                }

                if (notAdd)
                {
                    continue;
                }

                // search for the same exe on the user profile
                return game;
            }

            return null;
        }

        /// <summary>
        /// Returns all the possible games with the exe path
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public List<GenericGameInfo> GetGames(string exePath)
        {
            string lower = exePath.ToLower();
            string fileName = Path.GetFileName(exePath).ToLower();
            string dir = Path.GetDirectoryName(exePath);

            List<GenericGameInfo> possibilities = new List<GenericGameInfo>();

            //a workaround so we can choose different game executables.
            foreach (var gen in Games.Values)
            {
                if(gen.ExecutableNames != null)
                {
                    if(gen.ExecutableNames.Any(exe => exe.ToLower() == fileName))
                    {
                        possibilities.Add(gen);
                    }
                }
            }

            if(possibilities.Count() == 0)
            {
                var getPossiblities = Games.Values.Where(c => c.ExecutableName?.ToLower() == fileName).ToList();

                if(getPossiblities != null)
                {
                    possibilities = getPossiblities;
                }             
            }

            List<GenericGameInfo> games = new List<GenericGameInfo>();

            foreach (GenericGameInfo game in possibilities)
            {
                // check if the Context matches
                string[] context = game.ExecutableContext;
                bool notAdd = false;
                if (context != null)
                {
                    for (int j = 0; j < context.Length; j++)
                    {
                        string con = Path.Combine(dir, context[j]);
                        if (!File.Exists(con) &&
                            !Directory.Exists(con))
                        {
                            notAdd = true;
                            break;
                        }
                    }
                }

                if (notAdd)
                {
                    continue;
                }

                // search for the same exe on the user profile
                games.Add(game);
            }

            return games;
        }

        #region Initialize

        public string GetAppContentPath()
        {
#if ALPHA
            return Path.Combine(Globals.NucleusInstallRoot, "content");
#else
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Nucleus Coop");
#endif
        }

        public string GetJsScriptsPath()
        {
            return Path.Combine(Globals.NucleusInstallRoot, "handlers");
        }

        public string GetUtilsPath()
        {
            return Path.Combine(Globals.NucleusInstallRoot, "utils");
        }

        public string GetUserProfilePath()
        {
            return Path.Combine(GetAppContentPath(), "userprofile.json");
        }

        public UserGameInfo AddGame(GenericGameInfo game, string exePath)
        {
            UserGameInfo gInfo = new UserGameInfo();
            gInfo.InitializeDefault(game, exePath);
            user.Games.Add(gInfo);

            SaveUserProfile();

            return gInfo;
        }

        public IGameHandler MakeHandler(GenericGameInfo game)
        {
            return (IGameHandler)Activator.CreateInstance(game.HandlerType);
        }

        public string GempTempFolder(GenericGameInfo game)
        {
            string appData = GetAppContentPath();
            return Path.Combine(appData, game.GUID);
        }

        public void SaveUserProfile()
        {
            lock (user.Games)
            {
                user.Games = user.Games.OrderBy(g => g?.Game?.GameName).ToList();
            }

            string userProfile = GetUserProfilePath();
            AsyncSaveUser(userProfile);
        }

        private void LoadUser()
        {
            string userProfile = GetUserProfilePath();

            if (File.Exists(userProfile))
            {
                try
                {
                    using (FileStream stream = new FileStream(userProfile, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string json = reader.ReadToEnd();

                            user = JsonConvert.DeserializeObject<UserProfile>(json);

                            if (user.Games == null)
                            {
                                // json doesn't save empty lists, and user didn't add any game
                                user.InitializeDefault();
                            }
                            else
                            {
                                // delete invalid games
                                for (int i = 0; i < user.Games.Count; i++)
                                {                                
                                    if (user.Games[i].Game == null || !File.Exists(user.Games[i].ExePath))//no .js file?(deleted manually) no game executable?
                                    {
                                        user.Games.RemoveAt(i);
                                        if (i > 0) 
                                        {
                                            i--;
                                        } 
                                    }

                                    if (user.Games[i].Game != null)
                                    {
                                        bool exist = File.Exists(user.Games[i].ExePath);

                                        if (!exist)
                                        {
                                            CleanGamesAssets(user.Games[i].GameGuid);
                                            user.Games.RemoveAt(i);
                                            if (i > 0) { i--; }
                                        }

                                        //A workaround so more than one exe can be selected to add a game to the app.
                                        //set it here so it's not null because a few functions requires it early. 
                                        if (user.Games[i].Game.ExecutableNames != null)
                                        {
                                            var fileName = new FileInfo(user.Games[i].ExePath).Name;
                                            user.Games[i].Game.ExecutableName = fileName;
                                        }

                                        // Handler update checks are run by the services (HandlerUpdateChecker), not here.
                                    }
                                }
                            }

                            CleanGamesAssets(null);
                            user.Games = user.Games.OrderBy(g => g?.Game?.GameName).ToList();
                        }

                        SaveUser(userProfile);
                    }
                }
                catch
                {
                    MakeDefaultUserFile();
                }
            }
            else
            {
                MakeDefaultUserFile();
            }
        }

        private void MakeDefaultUserFile()
        {
            user = new UserProfile();
            user.InitializeDefault();

            string userProfile = GetUserProfilePath();
            string split = Path.GetDirectoryName(userProfile);
            if (!Directory.Exists(split))
            {
                Directory.CreateDirectory(split);
            }

            SaveUser(userProfile);
        }

        private void AsyncSaveUser(string path)
        {
            if (!IsSaving)
            {
                isSaving = true;
                //LogManager.Log("> Saving user profile....");
                ThreadPool.QueueUserWorkItem(SaveUser, path);
            }
        }

        private void SaveUser(object p)
        {
            lock (saving)
            {
                try
                {
                    isSaving = true;
                    string path = (string)p;
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            string json = JsonConvert.SerializeObject(user, Formatting.Indented);
                            writer.Write(json);
                            stream.Flush();
                        }
                    }
                    //LogManager.Log("Saved user profile");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                isSaving = false;
            }
        }

        public void Initialize()
        {
            // Search for Javascript games-infos
            string jsfolder = GetJsScriptsPath();
            DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);
            FileInfo[] files = jsFolder.GetFiles("*.js");

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo f = files[i];

                try
                {
                    using (Stream str = f.OpenRead())
                    {
                        string ext = Path.GetFileNameWithoutExtension(f.Name);
                        string pathBlock = Path.Combine(f.Directory.FullName, ext);

                        GenericGameInfo info = new GenericGameInfo(f.Name, pathBlock, str, new bool[] { false, false });

                        //LogManager.Log("Found game info: " + info.GameName);
                        if (games.Any(c => c.Value.GUID == info.GUID))
                        {
                            games.Remove(info.GUID);
                        }

                        games.Add(info.GUID, info);

                        if (gameInfos.Any(c => c.Value.GUID == info.GUID))
                        {
                            gameInfos.Remove(info.GUID);
                        }
                       
                        gameInfos.Add(info.GUID, info);
                    }
                }
                catch (ArgumentNullException)
                {
                    continue; // Issue with content of script, ignore this as error prompt is already displayed
                }
                catch (ArgumentException ex)
                {
                    UserPrompts.Report(PromptIcon.Error, "Error with handler " + f.Name, ex.InnerException + ": " + ex.Message, PromptOrigin.Handler);
                    continue;
                }
            }          
        }

        private void CleanGamesAssets(string gameGuid)
        {           
            try//Try just in case a file is opened in explorer
            {
                if (gameGuid != null)
                {
                    if (File.Exists(Path.Combine(Globals.NucleusInstallRoot, $"gui\\covers\\{gameGuid}.jpeg")))
                    {
                        File.Delete(Path.Combine(Globals.NucleusInstallRoot, $"gui\\covers\\{gameGuid}.jpeg"));
                    }

                    if (Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, $"gui\\screenshots\\{gameGuid}")))
                    {
                        Directory.Delete(Path.Combine(Globals.NucleusInstallRoot, $"gui\\screenshots\\{gameGuid}"), true);
                    }

                    if (Directory.Exists(Path.Combine(Globals.NucleusInstallRoot, $"gui\\descriptions")))
                    {
                        Directory.Delete(Path.Combine(Globals.NucleusInstallRoot, $"gui\\descriptions"), true);
                    }

                    return;
                }

                //Delete games assets for manually removed handlers (no available game guid to use)
                //We search for existing screenshot directories and cover file and compare with the actual user games.
                //If dirs or files with no corresponding user game are found, we delete them.

                List<string> scDirToDelete = Directory.GetDirectories(Path.Combine(Globals.NucleusInstallRoot, $"gui\\screenshots")).
                    Where(sc => user.Games.All(v => !sc.Contains(v.GameGuid))).ToList();

                foreach (var scDirectory in scDirToDelete)
                {
                    if (Directory.Exists(scDirectory))
                    {
                        Directory.Delete(scDirectory, true);
                        //Console.WriteLine(scDirectory);
                    }
                }

                List<string> coversToDelete = Directory.GetFiles(Path.Combine(Globals.NucleusInstallRoot, $"gui\\covers")).
                   Where(sc => user.Games.All(v => !sc.Contains(v.GameGuid))).ToList();

                foreach (var coverFile in coversToDelete)
                {
                    if (File.Exists(coverFile))
                    {
                        File.Delete(coverFile);
                        //Console.WriteLine(coverFile);
                    }
                }
            }
            catch {}
        }

        public string AutoSearchGameInstallPath(GenericGameInfo genericGameInfo)
        {
            string value64 = string.Empty;
            RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

            localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + genericGameInfo.SteamID);

            if (localKey != null)
            {
                value64 = localKey.GetValue("InstallLocation").ToString();
                localKey.Close();
            }

            return (genericGameInfo.BinariesFolder != "" && genericGameInfo.BinariesFolder != null) ? value64 + $@"\{genericGameInfo.BinariesFolder}" : value64;
        }

        public GenericGameInfo AddScript(string handlerName, bool[] checkUpdate)
        {
            string jsfolder = GetJsScriptsPath();
            DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);

            FileInfo f = new FileInfo(Path.Combine(jsFolder.FullName, handlerName + ".js"));

            try
            {
                GenericGameInfo info = null;

                using (Stream str = f.OpenRead())
                {
                    string ext = Path.GetFileNameWithoutExtension(f.Name);
                    string pathBlock = Path.Combine(f.Directory.FullName, ext);

                    info = new GenericGameInfo(f.Name, pathBlock, str, checkUpdate);

                    LogManager.Log("Found game info: " + info.GameName);
                    if (games.Any(c => c.Value.GUID == info.GUID))
                    {
                        games.Remove(info.GUID);
                    }

                    games.Add(info.GUID, info);

                    if (gameInfos.Any(c => c.Value.GUID == info.GUID))
                    {
                        gameInfos.Remove(info.GUID);
                    }

                    //A workaround so more than one exe can be selected to add a game to the app.
                    //re-set it here because the GenericGameInfo is fresh and it's not set anymore. 
                    if (info.ExecutableNames != null)
                    {
                        foreach (var exe in info.ExecutableNames)
                        {
                            var match = User.Games.Where(g => g.ExePath.Split('\\').Last() == exe).FirstOrDefault();
                            if (match != null)
                            {
                                var fileName = new FileInfo(exe).Name;
                                info.ExecutableName = fileName;
                            }
                        }
                    }

                    gameInfos.Add(info.GUID, info);
                   
                    UserGameInfo remove = user.Games.Where(ug => ug.GameGuid == info.GUID).FirstOrDefault();

                    //in case of handler update the game info can be reloaded easily
                    if (remove != null)
                    {
                        user.Games.Remove(remove);

                        UserGameInfo updatedInfo = new UserGameInfo();
                        updatedInfo.InitializeDefault(info, remove.ExePath);

                        user.Games.Add(updatedInfo);
                    }
                }

                user.Games = user.Games.OrderBy(g => g.Game.GameName).ToList();

                return info;
            }
            catch (ArgumentNullException)
            {
                return null;
                //continue; // Issue with content of script, ignore this as error prompt is already displayed
            }
            catch (ArgumentException ex)
            {
                UserPrompts.Report(PromptIcon.Error, "Error with handler " + f.Name, ex.InnerException + ": " + ex.Message, PromptOrigin.Handler);
                //continue;
                return null;
            }
        }

        #endregion
    }
}
