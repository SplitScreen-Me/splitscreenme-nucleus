using Microsoft.Win32;
using Newtonsoft.Json;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
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
using Nucleus.Gaming.Localization;

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

        private HandlerFileIndex handlerIndex;

        /// <summary>Snapshot of every handler file as last read (Nucleus 2.5 <c>HandlerInfos</c>), valid or not.</summary>
        public List<HandlerInfo> HandlerInfos => handlerIndex.Snapshot();

        /// <summary>
        /// Raised by <see cref="ApplyHandlerScan"/> (on its caller's thread) when handler files were added, changed, fixed,
        /// broken or deleted.
        /// </summary>
        public event Action<HandlerChanges> HandlersChanged;

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
            handlerIndex = new HandlerFileIndex(GetJsScriptsPath(), games, gameInfos);

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
        /// Handlers declaring this executable (<c>Game.ExecutableNames</c>, else <c>Game.ExecutableName</c>), split into
        /// those whose <c>Game.ExecutableContext</c> matches the executable's folder and those whose context does not, with
        /// what is missing or in the way (negated entries, Nucleus 2.5).
        /// </summary>
        public ExecutableMatch MatchExecutable(string exePath) => ExecutableMatch.Evaluate(Games.Values, exePath);

        /// <summary>
        /// Tries to find the game
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public GenericGameInfo GetGame(string exePath)
        {
            return MatchExecutable(exePath).Matches.FirstOrDefault();
        }

        /// <summary>
        /// Returns all the possible games with the exe path
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public List<GenericGameInfo> GetGames(string exePath)
        {
            return MatchExecutable(exePath).Matches.ToList();
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
                                List<UserGameInfo> kept = new List<UserGameInfo>();

                                foreach (UserGameInfo entry in user.Games)
                                {
                                    if (entry == null)
                                    {
                                        continue;
                                    }

                                    bool exeExists = !string.IsNullOrEmpty(entry.ExePath) && File.Exists(entry.ExePath);

                                    if (entry.Game == null)
                                    {
                                        //Nucleus 2.5: the script exists but has an error. Keep the game so it comes back once the
                                        //script is fixed (ApplyHandlerScan binds it again).
                                        if (exeExists && handlerIndex.IsBroken(entry.FileFullPath))
                                        {
                                            kept.Add(entry);
                                        }

                                        continue;//no .js file?(deleted manually)
                                    }

                                    if (!exeExists)//no game executable?
                                    {
                                        CleanGamesAssets(entry.GameGuid);
                                        continue;
                                    }

                                    entry.Rebind(entry.Game);//records the script path (FileFullPath) for older entries

                                    //A workaround so more than one exe can be selected to add a game to the app.
                                    //set it here so it's not null because a few functions requires it early.
                                    if (entry.Game.ExecutableNames != null)
                                    {
                                        entry.Game.ExecutableName = Path.GetFileName(entry.ExePath);
                                    }

                                    // Handler update checks are run by the services (HandlerUpdateChecker), not here.
                                    kept.Add(entry);
                                }

                                user.Games = kept;
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

                if (HandlerFileIndex.IsIgnoredScript(f.Name))
                {
                    continue;
                }

                GenericGameInfo info = null;
                string error = null;

                try
                {
                    info = GenericGameInfo.Load(f, new bool[] { false, false });
                    error = HandlerInfo.Describe(info);
                }
                catch (ArgumentException ex)
                {
                    error = ex.InnerException + ": " + ex.Message;
                    UserPrompts.Report(PromptIcon.Error, I18n.T("engine.handlers.errorTitle", new { file = f.Name }), error, PromptOrigin.Handler);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                //As before: a script that failed late (GUID and name already set) is still listed, its error was reported.
                bool usable = info != null && !string.IsNullOrEmpty(info.GUID);

                handlerIndex.Record(new HandlerInfo(f.FullName, f.Name, f.LastWriteTimeUtc, usable ? info : null, error));

                if (!usable)
                {
                    continue; // Issue with content of script, ignore this as error prompt is already displayed
                }

                //LogManager.Log("Found game info: " + info.GameName);
                games[info.GUID] = info;
                gameInfos[info.GUID] = info;
            }
        }

        #region Handlers hot reload (Nucleus 2.5)

        /// <summary>
        /// Lists the handlers folder and parses every new or changed script (any thread, it doesn't touch the library).
        /// Pass the result to <see cref="ApplyHandlerScan"/> on the thread that owns the library. With
        /// <paramref name="all"/>, every script is parsed again (the library "reload" action).
        /// </summary>
        public HandlerScan ScanHandlerFiles(bool all = false) => handlerIndex.Scan(all);

        /// <summary>
        /// Swaps a scan in (<see cref="HandlerFileIndex.Apply"/>): new and fixed handlers join <see cref="Games"/>, changed
        /// ones replace their previous parse, broken edits keep the previous version, deleted files leave (another file with
        /// the same GUID takes over), library entries are bound to the new parse. Call it on the thread that owns the
        /// library, not during a session. Raises <see cref="HandlersChanged"/>.
        /// </summary>
        public HandlerChanges ApplyHandlerScan(HandlerScan scan)
        {
            HandlerChanges changes = handlerIndex.Apply(scan, user?.Games);

            if (changes.Any)
            {
                try
                {
                    HandlersChanged?.Invoke(changes);
                }
                catch (Exception e)
                {
                    LogManager.Log("HandlersChanged subscriber failed: " + e);
                }
            }

            return changes;
        }

        #endregion

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

        /// <summary>
        /// Parses <c>handlers\&lt;handlerName&gt;.js</c> again and puts it in <see cref="Games"/> (install, update, play).
        /// Returns null when the script can't be used (a script error is reported as a handler notice): the previous parse is
        /// not launched instead.
        /// </summary>
        public GenericGameInfo AddScript(string handlerName, bool[] checkUpdate)
        {
            string jsfolder = GetJsScriptsPath();
            DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);

            FileInfo f = new FileInfo(Path.Combine(jsFolder.FullName, handlerName + ".js"));

            try
            {
                GenericGameInfo info = GenericGameInfo.Load(f, checkUpdate);

                if (string.IsNullOrEmpty(info.GUID) || info.ScriptError != null)
                {
                    handlerIndex.Record(new HandlerInfo(f.FullName, f.Name, f.LastWriteTimeUtc, null, HandlerInfo.Describe(info)));
                    return null;
                }

                handlerIndex.Record(new HandlerInfo(f.FullName, f.Name, info.LastWriteTimeUtc, info, null));

                LogManager.Log("Found game info: " + info.GameName);
                games[info.GUID] = info;

                //A workaround so more than one exe can be selected to add a game to the app.
                //re-set it here because the GenericGameInfo is fresh and it's not set anymore.
                if (info.ExecutableNames != null)
                {
                    foreach (var exe in info.ExecutableNames)
                    {
                        var match = User.Games.Where(g => g.ExePath != null && string.Equals(Path.GetFileName(g.ExePath), exe, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (match != null)
                        {
                            info.ExecutableName = Path.GetFileName(exe);
                        }
                    }
                }

                gameInfos[info.GUID] = info;

                UserGameInfo remove = user.Games.Where(ug => ug.GameGuid == info.GUID).FirstOrDefault();

                //in case of handler update the game info can be reloaded easily
                if (remove != null)
                {
                    user.Games.Remove(remove);

                    UserGameInfo updatedInfo = new UserGameInfo();
                    updatedInfo.InitializeDefault(info, remove.ExePath);

                    user.Games.Add(updatedInfo);
                }

                user.Games = user.Games.OrderBy(g => g.Game?.GameName).ToList();

                return info;
            }
            catch (ArgumentNullException)
            {
                return null;
                //continue; // Issue with content of script, ignore this as error prompt is already displayed
            }
            catch (ArgumentException ex)
            {
                UserPrompts.Report(PromptIcon.Error, I18n.T("engine.handlers.errorTitle", new { file = f.Name }), ex.InnerException + ": " + ex.Message, PromptOrigin.Handler);
                //continue;
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        #endregion
    }
}
