using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Manages games information, so we can know what games are supported 
    /// and how to support it
    /// </summary>
    /// 

    public static class AddGameResultString
    {
        public static string WrongExeSelected = "No handler match found for this executable.";
        public static string MissExecutableContext = "Missing content in game directory:";
        public static string GameAddedSuccessfully = " added successfully.";
        public static string GameAlreadyAdded = "This game is already in your game list.";
    }

    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        private string jsonFilePath = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\userGameDirectories.json";
        private List<string> gamesSearcheDirectories = new List<string>();
        public List<string> GamesSearcheDirectories => gamesSearcheDirectories;

        private List<BackupFile> backupFiles;
        private bool isSaving;

        private RawInputProcessor rawInputProcessor;
        public Action OnGenericGameInfoListUpdate;
        private System.Windows.Forms.Timer filesSuprVisorTimer;
        //private InputInterceptor inputInterceptor;

        /// object instance so we can thread-safe save the user profile
        private readonly object saving = new object();
        private readonly object refreshing = new object();

        public bool IsSaving => isSaving;

        /// <summary>
        /// A dictionary containing GameInfos. The key is the game's guid
        /// </summary>
        private List<HandlerInfo> handlerInfos;
        public List<HandlerInfo> HandlerInfos => handlerInfos;
        public List<string> BlackListedFiles = new List<string>();

        private UserProfile user;
        public UserProfile User
        {
            get => user;
            set => user = value;
        }

        public GameManager()
        {
            Instance = this;
            Directory.CreateDirectory(GetAppContentPath());
            Directory.CreateDirectory(GetJsScriptsPath());

            //TODO: update isRunningSplitScreen
            Debug.WriteLine("Registering raw input");
            rawInputProcessor = new RawInputProcessor(() => LockInputRuntime.IsLocked);//TODO: needs more robust method
                                                                                       //Action<IntPtr> rawInputAction = rawInputProcessor.Process;
                                                                                       //GameManager.mainForm.GetType().GetProperty("RawInputAction").SetValue(GameManager.mainForm, rawInputAction, new object[] { });
                                                                                       //IntPtr rawInputHwnd = GameManager.mainFormHandle;
            RawInputManager.RegisterRawInput(rawInputProcessor);

            Initialize();
            LoadUserGameSearchPaths();
            LoadUser();
            CleanGamesAssets();
        }

        private void LoadUserGameSearchPaths()
        {
            if (File.Exists(jsonFilePath))
            {
                string pathString = File.ReadAllText(jsonFilePath);

                if (!string.IsNullOrEmpty(pathString))
                {
                    JObject pathJObject = (JObject)JsonConvert.DeserializeObject(pathString);

                    if (pathJObject == null)
                    {
                        SaveSearchPathsJson();
                        return;
                    }

                    for (int i = 0; i < pathJObject["Path"].Count(); i++)
                    {
                        string dirPath = pathJObject["Path"][i].ToString();

                        if (!Directory.Exists(dirPath))
                        {
                            continue;
                        }

                        gamesSearcheDirectories.Add(dirPath);
                        //Console.WriteLine(dirPath);
                    }
                }
            }
            else
            {
                SaveSearchPathsJson();
            }
        }

        private void SaveSearchPathsJson()
        {
            lock (saving)
            {
                if (!File.Exists(jsonFilePath))
                {
                    if (!Directory.Exists($@"{Globals.UserEnvironmentRoot}\NucleusCoop"))
                    {
                        Directory.CreateDirectory($@"{Globals.UserEnvironmentRoot}\NucleusCoop");
                    }
                }

                gamesSearcheDirectories.Sort(StringComparer.OrdinalIgnoreCase);

                JObject pathJObject = new JObject(new JProperty("Path", gamesSearcheDirectories));

                using (FileStream stream = new FileStream(jsonFilePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(pathJObject, Formatting.Indented);
                        writer.Write(json);
                        stream.Flush();
                    }
                }
            }
        }

        public void AddGamesSearchDirectory(string path)//pass a string array to avoid repeated calls
        {
            if (!gamesSearcheDirectories.Any(p => p == path))
            {
                gamesSearcheDirectories.Add(path);

                SaveSearchPathsJson();
            }
            else
            {
                Console.WriteLine("Json object already contains: " + path);
            }
        }

        public void RemoveGamesSearchDirectory(string path)//pass a string array to avoid repeated calls
        {
            if (gamesSearcheDirectories.Any(p => p == path))
            {
                gamesSearcheDirectories.Remove(path);
                SaveSearchPathsJson();
            }
            else
            {
                Console.WriteLine("Could not find path in json object: " + path);
            }
        }

        private void StartJsFilesSupervisor()
        {
            filesSuprVisorTimer?.Dispose();
            filesSuprVisorTimer = new System.Windows.Forms.Timer();
            filesSuprVisorTimer.Interval = (3000); //millisecond                   
            filesSuprVisorTimer.Tick += FileSupervisorTimer_Tick;
            filesSuprVisorTimer.Start();
        }

        private void FileSupervisorTimer_Tick(object Object, EventArgs eventArgs)
        {
            if (GenericGameHandler.IsRunning)
            {
                return;
            }

            RefreshHandlersData();
        }

        /// <summary>
        /// Tests if there's any game with the named exe
        /// </summary>
        /// <param name="gameExe"></param>
        /// <returns></returns>
        public bool AnyGame(string gameExe)
        {
            return user.Games.Any(c => c.HandlerInfo.GenericGameInfo.ExecutableName.ToLower() == gameExe);
        }
     
        /// <summary>
        /// Returns all the possible games with the exe path
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        /// 
        public (List<HandlerInfo>, string) GetGames(string exePath)
        {
            string lower = exePath.ToLower();
            string fileName = Path.GetFileName(exePath).ToLower();
            string dir = Path.GetDirectoryName(exePath);

            List<HandlerInfo> possibilities = new List<HandlerInfo>();

            foreach (UserGameInfo usergameInfo in user.Games)
            {
                GenericGameInfo genericGameInfo = usergameInfo?.HandlerInfo?.GenericGameInfo;

                if (genericGameInfo != null)
                {
                    if (genericGameInfo.ExecutableNames != null)
                    {
                        foreach (string exeName in genericGameInfo.ExecutableNames)
                        {
                            if (Path.Combine(dir, exeName).ToLower() == exePath.ToLower())
                            {
                                return (possibilities, AddGameResultString.GameAlreadyAdded);
                            }
                        }
                    }
                    else if (usergameInfo.ExePath == exePath)
                    {
                        return (possibilities, AddGameResultString.GameAlreadyAdded);
                    }
                }
            }

            //Will return only the last result because that the one that will be loaded by the GameManager by default anyway
            //!!!! May change that it use creation time.
            for (int i = 0; i < handlerInfos.Count; i++)
            {
                HandlerInfo handlerInfo = handlerInfos[i];

                if (!handlerInfo.IsValid)
                {
                    continue;
                }

                GenericGameInfo genericGameInfo = handlerInfo.GenericGameInfo;

                if (user.Games.Any(ug => ug.GameGuid == handlerInfo.GUID))
                {
                    continue;
                }

                if (genericGameInfo.ExecutableNames != null)
                {
                    foreach (string exeName in genericGameInfo.ExecutableNames)
                    {
                        if (exeName.ToLower() == fileName)
                        {
                            if (!possibilities.Any(p => p.GenericGameInfo == genericGameInfo))
                            {
                                //ExecutableContext check will do the rest
                                possibilities.Add(handlerInfo);
                            }
                        }
                    }
                }
                else if (genericGameInfo.ExecutableName.ToLower() == fileName)
                {
                    //ExecutaleContext check will do the rest
                    possibilities.Add(handlerInfo);
                }
            }

            if (possibilities.Count == 0)
            {
                return (possibilities, AddGameResultString.WrongExeSelected);
            }

            List<HandlerInfo> games = new List<HandlerInfo>();
            Dictionary<string,string> missingContextValues = new Dictionary<string, string>();

            int duplicatData = 0;

            foreach (HandlerInfo handlerInfo in possibilities)
            {
                // check if the Context matches
                string[] contextValuesArray = handlerInfo.GenericGameInfo.ExecutableContext;
                bool notAdd = false;

                if (contextValuesArray != null)
                {
                    for (int j = 0; j < contextValuesArray.Length; j++)
                    {
                        string contextFile = Path.Combine(dir, contextValuesArray[j]);

                        if (contextValuesArray[j].StartsWith("!"))//negate if the context file value starts with "!", if the file exists the handler can't be added;
                        {
                            contextFile = Path.Combine(dir, contextValuesArray[j].TrimStart('!'));

                            if (File.Exists(contextFile) || Directory.Exists(contextFile))
                            {
                                notAdd = true;
                                break;
                            }
                        }
                        else if (!File.Exists(contextFile) && !Directory.Exists(contextFile))
                        {
                            //for (int f = 0; f < contextValuesArray.Length; f++)
                            //{
                            missingContextValues.Add(handlerInfo.FileFullPath ,Path.Combine(dir, contextFile /*contextValuesArray[f]*/));
                            //}

                            notAdd = true;
                            duplicatData++;
                            break;
                        }
                    }
                }

                if (notAdd)
                {
                    continue;
                }

                games.Add(handlerInfo);
            }

            string reason = AddGameResultString.GameAddedSuccessfully;

            if (games.Count == 0)
            {
                reason = AddGameResultString.MissExecutableContext + " ";
                foreach (KeyValuePair<string, string> contextValue in missingContextValues)
                {
                    reason += "\nHandler: " + contextValue.Key +  "\n";
                    reason += "Value: " + contextValue.Value + "\n";
                }

                if(duplicatData > 1)
                {
                    reason += "\n\nNote: There are handlers sharing \"ExecutableContext\" values.";
                }
            }

            //A workaround so more than one exe can be selected to add a game to the app.
            for (int i = 0; i < games.Count; i++)
            {
                HandlerInfo handlerInfo = games[i];
                if (handlerInfo?.GenericGameInfo.ExecutableNames != null)
                {
                    foreach (var exe in handlerInfo.GenericGameInfo.ExecutableNames)
                    {
                        handlerInfo.GenericGameInfo.ExecutableName = Path.GetFileName(exePath);
                    }
                }
            }

            return (games, reason);
        }

        public (List<HandlerInfo>, string) GetGameByJsFilePath(string filePath,string exePath)
        {
           HandlerInfo found =  Instance.HandlerInfos.Find(hi => hi.IsValid && hi.FileFullPath == filePath);

            if(found != null)
            {
                if (found.GenericGameInfo.ExecutableNames != null)
                {
                    //foreach (var exe in found.GenericGameInfo.ExecutableNames)
                    //{
                        found.GenericGameInfo.ExecutableName = Path.GetFileName(exePath);
                    //}
                }

                return (new List<HandlerInfo> { found }, AddGameResultString.GameAddedSuccessfully);
            }
            else
            {
                return (new List<HandlerInfo>(), AddGameResultString.WrongExeSelected);
            }
           
        }
        /// <summary>
        /// Tries adding a game to the collection with the provided IGameInfo
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public UserGameInfo TryAddGame(string exePath, HandlerInfo handlerInfo)
        {
            LogManager.Log("Found game: {0}, full path: {1}", handlerInfo.GameName, exePath);
            UserGameInfo uinfo = new UserGameInfo();

            uinfo.InitializeDefault(handlerInfo, exePath);
            handlerInfo.GenericGameInfo.MetaInfo.FirstLaunch = true;

            Instance.User.Games.Add(uinfo);
            Instance.SaveUserProfile();

            return uinfo;
        }

        public void WaitSave()
        {
            while (IsSaving)
            {
            }
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

        public UserGameInfo AddGame(HandlerInfo hanlderInfo, string exePath)
        {
            UserGameInfo gInfo = new UserGameInfo();
            gInfo.InitializeDefault(hanlderInfo, exePath);
            user.Games.Add(gInfo);

            SaveUserProfile();

            return gInfo;
        }

        public void BeginBackup(GenericGameInfo game)
        {
            string appData = GetAppContentPath();
            string gamePath = Path.Combine(appData, game.GUID);
            Directory.CreateDirectory(gamePath);

            backupFiles = new List<BackupFile>();
        }

        public IGameHandler MakeHandler(UserGameInfo userGameInfo)
        {
            return (IGameHandler)Activator.CreateInstance(userGameInfo.HandlerInfo.GenericGameInfo.HandlerType);
        }

        public string GempTempFolder(GenericGameInfo game)
        {
            string appData = GetAppContentPath();
            return Path.Combine(appData, game.GUID);
        }

        public BackupFile BackupFile(GenericGameInfo game, string path)
        {
            string appData = GetAppContentPath();
            string gamePath = Path.Combine(appData, game.GUID);
            string destination = Path.Combine(gamePath, Path.GetFileName(path));

            if (!File.Exists(path))
            {
                if (File.Exists(destination))
                {
                    // we fucked up and the backup exists? maybe, so restore
                    File.Copy(destination, path);
                }
            }
            else
            {
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }

                File.Copy(path, destination);
            }

            BackupFile bkp = new BackupFile(path, destination);
            backupFiles.Add(bkp);

            return bkp;
        }

        public void ExecuteBackup(GenericGameInfo game)
        {
            // we didnt backup anything
            if (backupFiles == null)
            {
                return;
            }

            string appData = GetAppContentPath();
            string gamePath = Path.Combine(appData, game.GUID);

            for (int i = 0; i < backupFiles.Count; i++)
            {
                BackupFile bkp = backupFiles[i];
                if (File.Exists(bkp.BackupPath))
                {
                    File.Delete(bkp.Source);
                    File.Move(bkp.BackupPath, bkp.Source);
                }
            }
        }

        public void ReorderUserProfile()
        {
            lock (user.Games)
            {
                user.Games = user.Games.OrderBy(g => g.HandlerInfo?.GameName).ToList();
            }
        }

        public void SaveUserProfile()
        {
            lock (user.Games)
            {
                user.Games = user.Games.OrderBy(g => g.HandlerInfo?.GameName).ToList();
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

                            if (user.Games == null || user.Games.Count == 0)
                            {
                                // json doesn't save empty lists, and user didn't add any game
                                user.InitializeDefault();
                            }
                            else
                            {
                                for (int i = 0; i < user.Games.Count; i++)
                                {
                                    UserGameInfo userGameInfo = user.Games[i];

                                    HandlerInfo handlerInfo = null;
                                    
                                    if(!string.IsNullOrEmpty(userGameInfo.FileFullPath))
                                    {
                                        //Wip Nucleus
                                        handlerInfo = handlerInfos.Find(hi => hi.IsValid && hi.FileFullPath == userGameInfo.FileFullPath)/*.LastOrDefault()*/;
                                    }
                                    
                                    if(handlerInfo == null)
                                    {
                                        //priority to hub handlers
                                        handlerInfo = handlerInfos.Where(hi => hi.IsValid && !string.IsNullOrEmpty(hi.GenericGameInfo.Hub.Handler.Id) && hi.GUID == userGameInfo.GameGuid).LastOrDefault();
                                    }

                                    if(handlerInfo == null)
                                    {
                                        //last resort
                                        handlerInfo = handlerInfos.Where(hi => hi.IsValid && hi.GUID == userGameInfo.GameGuid).LastOrDefault();
                                    }

                                    //the js file exists but has error,we keep the game in the profile so it can be can automatically updated once the file fixed.
                                    if (handlerInfo == null && File.Exists(userGameInfo.FileFullPath) && File.Exists(userGameInfo.ExePath))
                                    {
                                        continue;
                                    }

                                    if (handlerInfo == null || !File.Exists(userGameInfo.ExePath))
                                    {
                                        user.Games.Remove(userGameInfo);
                                        continue;
                                    }
                                   
                                    //A workaround so more than one exe can be selected to add a game to the app.                                 
                                    if (handlerInfo.GenericGameInfo.ExecutableNames != null)
                                    {
                                        var fileName = Path.GetFileName(userGameInfo.ExePath);
                                        handlerInfo.GenericGameInfo.ExecutableName = fileName;
                                    }

                                    userGameInfo.HandlerInfo = handlerInfo;
                                }
                            }

                            user.Games = user.Games.OrderBy(g => g.HandlerInfo?.GameName).ToList();
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

            StartJsFilesSupervisor();
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
            string handlerDir = GetJsScriptsPath();

            DirectoryInfo handlerDirInfo = new DirectoryInfo(handlerDir);
            FileInfo[] files = handlerDirInfo.GetFiles("*.js");
            handlerInfos = new List<HandlerInfo>();

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fileInfo = files[i];

                if(fileInfo.Name == "MasterHandler.js")
                {
                    continue;
                }

                GenericGameInfo genericGameInfo = null;

                try
                {
                    string ext = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string handlerDependanciesDir = Path.Combine(fileInfo.Directory.FullName, ext);
                    using (Stream str = fileInfo.OpenRead())
                    {
                        genericGameInfo = new GenericGameInfo(fileInfo, handlerDependanciesDir, str, true);
                    }
                    {
                        //Regex pattern = new Regex("[\\/:*?\"<>|]");

                        //if (!string.IsNullOrEmpty(info.GameName))
                        //{
                        //    string properFileNameNoExtension = pattern.Replace(info.GameName, "");

                        //    if (ext.ToLower() != properFileNameNoExtension.ToLower())
                        //    {
                        //        string expectedFileName = properFileNameNoExtension + ".js";

                        //        if (!File.Exists(Path.Combine(handlerDir, expectedFileName)))
                        //        {
                        //            string newFileFullName = Path.Combine(handlerDir, expectedFileName);
                        //            string newDepDirName = Path.Combine(handlerDir, properFileNameNoExtension);

                        //            File.Move(f.FullName, Path.Combine(handlerDir, newFileFullName));

                        //            if(Directory.Exists(handlerDependanciesDir))
                        //            {
                        //                if(!Directory.Exists(newDepDirName))
                        //                {
                        //                    Directory.Move(handlerDependanciesDir, newDepDirName);
                        //                }
                        //            }

                        //            f = new FileInfo(newFileFullName);
                        //            ext = Path.GetFileNameWithoutExtension(f.Name);
                        //            handlerDependanciesDir = Path.Combine(f.Directory.FullName, ext);

                        //            using (Stream str = f.OpenRead())
                        //            {
                        //                info = new GenericGameInfo(f, handlerDependanciesDir, str, true);
                        //            }
                        //        }
                        //    }
                        //}

                        //var t = pattern.Replace(info.GameName, "");

                        //if ( t.ToLower() != Path.GetFileNameWithoutExtension(f.Name).ToLower())
                        //{
                        //    continue;
                        //}
                    }
                }
                catch (ArgumentNullException)
                {
                    //Issue with content of script, ignore this as error prompt is already displayed
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.InnerException + ": " + ex.Message, "Error with handler " + fileInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                HandlerInfo handlerInfo = new HandlerInfo(fileInfo, genericGameInfo);
                handlerInfos.Add(handlerInfo);
            }
        }

        private void RefreshHandlersData()
        {
            if (GenericGameHandler.IsRunning)
            {
                return;
            }

            lock (refreshing)
            {
                //check for existing handlerInfo changes or deletion (based on .js files)
                for (int i = 0; i < handlerInfos.Count; i++)
                {
                    HandlerInfo handlerInfo = handlerInfos[i];
                    FileInfo fileInfo = new FileInfo(handlerInfo.FileFullPath);

                    //the file has been deleted since startup so we remove the handlerInfo and eventually the usergameinfo that uses it.
                    if (!File.Exists(handlerInfo.FileFullPath))
                    {
                        UserGameInfo installedUserGameInfo = user.Games.Find(ugi => ugi.FileFullPath == handlerInfo.FileFullPath);

                        if (installedUserGameInfo != null)
                        {
                            installedUserGameInfo.HandlerInfo = null;//trigger UserGameInfo.OnRefreshGenericGameInfo action                      
                            RemoveUserGameInfo(installedUserGameInfo);
                        }

                        handlerInfos.RemoveAt(i);

                        if (i - 1 >= 0)
                        {
                            i--;
                            continue;
                        }
                    }

                    //No changes continue.
                    if (handlerInfo?.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
                    {
                        continue;
                    }

                    //the file has been edited

                    //check if it's a valid one
                    var newGenericGameInfo = CheckGenericGameInfoIntegrity(fileInfo);

                    //it's a valid one so remove it from blacklist eventually
                    if (newGenericGameInfo is GenericGameInfo)
                    {
                        if (BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                        {
                            BlackListedFiles.Remove(fileInfo.FullName);
                        }
                    }
                    else//an exception has been returned
                    {
                        //the file is invalid so we add it to the blacklist and show an error only once
                        if (!BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                        {
                            BlackListedFiles.Add(fileInfo.FullName);
                            UserGameInfo existingUserGameInfo = user.Games.Find(ug => ug?.HandlerInfo?.FileName == fileInfo.Name);

                            Exception exception = (Exception)newGenericGameInfo;

                            if (existingUserGameInfo != null)
                            {
                                string details = exception.Message;

                                if (details.Contains(":"))
                                {
                                    string[] lineSplit = exception.Message.Split(':');
                                    string splited = lineSplit?[0];
                                    string[] getNum = splited.Split(' ');

                                    bool lineIsNum = int.TryParse(getNum?[1], out int numLine);

                                    

                                    if (lineIsNum)
                                    {
                                        int num = int.Parse(getNum?[1]);
                                        details = $"Error at line {num / 2} {lineSplit?[1]}";
                                    }
                                }

                                string error = $"There is an error in {fileInfo.Name}.\n" +
                                $"\nThe handler has not been refreshed due to an error in the latest changes." +
                                $"\n\n{details}";

                                NucleusMessageBox.Show("Error in handler", error, false);
                            }
                        }

                        continue;
                    }

                    //check if the file is of an added game
                    UserGameInfo userGameInfo = user.Games.Find(ugi => ugi.FileFullPath == fileInfo.FullName);

                    GenericGameInfo _newGenericGameInfo = (GenericGameInfo)newGenericGameInfo;

                    if (userGameInfo != null)
                    {
                        //it's of an added game so we update the UserGameInfo HandlerInfo and replace HandlerInfo in the handlerInfos list
                        if (_newGenericGameInfo.ExecutableNames != null)
                        {
                            var fileName = Path.GetFileName(userGameInfo.ExePath);
                            _newGenericGameInfo.ExecutableName = fileName;
                        }

                        userGameInfo.HandlerInfo = AddOrUpdateHandlerInfo(_newGenericGameInfo);
                    }
                    else
                    {
                        //not of an added game so just update it in the handlerInfos list
                        AddOrUpdateHandlerInfo(_newGenericGameInfo);
                    }
                }

                DirectoryInfo handlersDir = new DirectoryInfo(GetJsScriptsPath());
                FileInfo[] files = handlersDir.GetFiles("*.js");

                //New file(s) added in handlers dir
                if (files.Length - 1 > handlerInfos.Count)//-1 because of masterhandler.js
                {
                    //get the file which are not of any handlerinfo yet
                    var newFiles = files.ToList().Where(f => !handlerInfos.Any(agi => agi.FileFullPath == f.FullName));

                    foreach (FileInfo fileInfo in newFiles)
                    {
                        if (fileInfo.Name == "MasterHandler.js")
                        {
                            continue;
                        }

                        var newGenericGameInfo = CheckGenericGameInfoIntegrity(fileInfo);

                        //it's a valid one so remove it from blacklist eventually
                        if (newGenericGameInfo is GenericGameInfo)
                        {
                            if (BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                            {
                                BlackListedFiles.Remove(fileInfo.FullName);
                            }
                        }
                        else//it's an exception
                        {
                            //it's an exception so we add it to the blacklist and show an error only once
                            if (!BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                            {
                                BlackListedFiles.Add(fileInfo.FullName);
                                Exception exception = (Exception)newGenericGameInfo;

                                string details = exception.Message;

                                if (details.Contains(":"))
                                {
                                    string[] lineSplit = exception.Message.Split(':');
                                    string splited = lineSplit?[0];
                                    string[] getNum = splited.Split(' ');

                                    bool lineIsNum = int.TryParse(getNum?[1], out int numLine);

                                    if (lineIsNum)
                                    {
                                        int num = int.Parse(getNum?[1]);
                                        details = $"Error at line {num / 2} {lineSplit?[1]}";
                                    }
                                }

                                string error = $"There is an error in {fileInfo.Name}.\n" +
                                $"{details}";

                                NucleusMessageBox.Show("Error in handler", error, false);
                            }

                            continue;
                        }

                        AddOrUpdateHandlerInfo((GenericGameInfo)newGenericGameInfo);
                    }
                }
            }
        }

        public object CheckGenericGameInfoIntegrity(FileInfo fileInfo)
        {
            try
            {
                using (Stream str = fileInfo.OpenRead())
                {
                    string ext = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string pathBlock = Path.Combine(fileInfo.Directory.FullName, ext);

                    GenericGameInfo genericGameInfo = new GenericGameInfo(fileInfo, pathBlock, str, false);
                    if (genericGameInfo.GameName == null)
                    {
                       return new Exception("\"Game.GameName\" can't be null");

                    }
                    else if (genericGameInfo.GUID == null)
                    {
                        return new Exception("\"Game.GUID\" can't be null");
                    }

                    return genericGameInfo;
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public void UpdateUserGameInfo(UserGameInfo userGameInfo, GenericGameInfo genericGameInfo, bool updating)
        {
            if (updating)//if called after updating the handler from the hub
            {
                userGameInfo.UpdateAvailable = false;
            }

            if (genericGameInfo.ExecutableNames != null)
            {
                foreach (var exe in genericGameInfo.ExecutableNames)
                {
                    genericGameInfo.ExecutableName = Path.GetFileName(userGameInfo.ExePath);
                }
            }

            userGameInfo.HandlerInfo = AddOrUpdateHandlerInfo(genericGameInfo);
        }

        public void RemoveUserGameInfo(UserGameInfo userGameInfo)
        {
            UserGameInfo toRemove = user.Games.Find(ug => ug.GameGuid == userGameInfo.GameGuid); 
            user.Games.Remove(toRemove);
            SaveUserProfile();
        }

        private HandlerInfo AddOrUpdateHandlerInfo(GenericGameInfo genericGameInfo)
        {
            var lastMatch = handlerInfos.Select((ggi, index) => new { ggi, index }).LastOrDefault(x => x.ggi.FileFullPath == genericGameInfo.FileFullPath);

            HandlerInfo handlerInfo = new HandlerInfo(genericGameInfo.JsFileInfo, genericGameInfo);

            if (lastMatch != null)
            {
                handlerInfos[lastMatch.index] = handlerInfo;

                Globals.MainOSD?.Show(3000, $"*@{OSDHighlightColors.Regular}|The@* *@{OSDHighlightColors.Info}|{handlerInfo.GameName}@* *@{OSDHighlightColors.Regular}| handler has been updated successfully@*");
            }
            else
            {
                handlerInfos.Add(handlerInfo);
            }

            return handlerInfo;
        }

        //return true if there is an existing UserGameInfo for this game else make it available for install
        public bool AddNewGenericGameInfo(string handlerName, bool updating)
        {
            lock (refreshing)
            {
                string jsfolder = GetJsScriptsPath();
                DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);

                FileInfo fileInfo = new FileInfo(Path.Combine(jsFolder.FullName, handlerName + ".js"));

                try
                {
                    GenericGameInfo genericGameInfo = null;
                    
                    using (Stream str = fileInfo.OpenRead())
                    {
                        string ext = Path.GetFileNameWithoutExtension(fileInfo.Name);
                        string pathBlock = Path.Combine(fileInfo.Directory.FullName, ext);
                        genericGameInfo = new GenericGameInfo(fileInfo, pathBlock, str, true);
                    }

                    if (genericGameInfo.GameName == null || genericGameInfo.GUID == null)
                    {
                        if (!BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                        {
                            BlackListedFiles.Add(fileInfo.FullName);
                        }

                        return true;//So the search function don't get triggered from DownloadPrompt and HubWebView etc
                    }
                    else if (BlackListedFiles.Any(blf => blf == fileInfo.FullName))
                    {
                        BlackListedFiles.Remove(fileInfo.FullName);
                    }

                    LogManager.Log("Found game info: " + genericGameInfo.GameName);
             
                    UserGameInfo userGameInfo = user.Games.Find(ug => ug.GameGuid == genericGameInfo.GUID);

                    if (userGameInfo != null)//the game is already in the Userprofile, so we just update its GenericGameInfo
                    {
                        UpdateUserGameInfo(userGameInfo, genericGameInfo, updating);
                        return true;
                    }

                    AddOrUpdateHandlerInfo(genericGameInfo);
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return false;
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.InnerException + ": " + ex.Message, "Error with handler " + fileInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void CleanGamesAssets()
        {
            try
            {
                var validGuids = new HashSet<string>(
                    user.Games.Select(g => g.GameGuid),
                    StringComparer.OrdinalIgnoreCase
                );

                string screenshotsPath = Path.Combine(Application.StartupPath, @"gui\screenshots");
                string coversPath = Path.Combine(Application.StartupPath, @"gui\covers");

                foreach (var dir in Directory.GetDirectories(screenshotsPath))
                {
                    if (!validGuids.Contains(Path.GetFileName(dir)))
                        Directory.Delete(dir, true);
                }

                foreach (var file in Directory.GetFiles(coversPath))
                {
                    if (!validGuids.Contains(Path.GetFileNameWithoutExtension(file)))
                        File.Delete(file);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        #endregion
    }
}
