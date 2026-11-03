using Jint.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.App.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    //need a lock or saveasync whaterver make it thread safe and avoid spamming
    public class GameMetaInfo
    {
        /// <summary>Folder of metaInfo.json, null for the default %USERPROFILE%\NucleusCoop (another folder: tests, or a host keeping user data elsewhere). Read when a GameMetaInfo is created.</summary>
        public static string StorageFolder { get; set; }

        private readonly string nucleusEnvironment = StorageFolder ?? $@"{Globals.UserEnvironmentRoot}\NucleusCoop";
#if DEBUG
 private readonly string metaInfoJson = "metaInfo_DEBUG.json";
#else
 private readonly string metaInfoJson = "metaInfo.json";
#endif
        private ulong intervale = 20000;//milliseconds
        private bool stopped;

        // metaInfo.json is shared by every game and rewritten whole by each setter, from the UI thread, the
        // gameplay timer thread and background tasks: serialize every read-modify-write.
        private static readonly object metaInfoFileLock = new object();

        /// <summary>Raised (on the thread that saved) after metaInfo.json was written for a game. Argument: game GUID.</summary>
        public static event Action<string> Saved;

        private GenericGameInfo gen;
        private string gameGuid;

        /// <summary>
        /// The values of one game. Every GameMetaInfo of a game shares it: a handler is parsed again (play, update, hot
        /// reload) while older GenericGameInfo objects of the same game are still in use (library entry, setup, session
        /// play-time thread), and each save writes the whole entry. With separate copies a stale one wrote its old values
        /// back over a newer change (KeepSymLink switched on, then lost and the instances folder deleted).
        /// </summary>
        private sealed class MetaState
        {
            public string lastPlayedAt;
            public string totalPlayTime;
            public string iconPath;
            public bool saveProfile;
            public bool disableProfiles;
            public bool keepSymLink;
            public bool favorite;
            public bool firstLaunch;
            public bool checkUpdate;
            public bool useApiIndex;
            public bool useApiIndexForGuests;
            public bool profileAssignGamepadByButonPress;
            public string steamLanguage;
        }

        /// <summary>Shared values per game GUID (guarded by metaInfoFileLock).</summary>
        private static readonly Dictionary<string, MetaState> states = new Dictionary<string, MetaState>(StringComparer.OrdinalIgnoreCase);

        private MetaState state = new MetaState();

        /// <summary>Forgets the shared values (tests, or after metaInfo.json was replaced): the next load reads the file.</summary>
        public static void ForgetLoadedValues()
        {
            lock (metaInfoFileLock)
            {
                states.Clear();
            }
        }

        public string LastPlayedAt => GetLastPlayed();

        public string LastPlayedAtFull => GetLastPlayedFull();

        /// <summary>Last session start as stored (DateTime.Now.ToString() of the culture at that time), or null.</summary>
        public string LastPlayedAtRaw => state.lastPlayedAt;

        public string TotalPlayTime => FormatPlayTime();

        /// <summary>Total play time in seconds as stored, or null when never played.</summary>
        public string TotalPlayTimeRaw => state.totalPlayTime;

        public string IconPath
        {
            get => state.iconPath;
            set
            {
                state.iconPath = value;
                SaveGameMetaInfo();
            }
        }

        public bool SaveProfile
        {
            get => state.saveProfile;
            set
            {
                state.saveProfile = value;         
                SaveGameMetaInfo();
            }
        }

        public bool DisableProfiles
        {
            get => state.disableProfiles;
            set
            {
                state.disableProfiles = value;
                SaveGameMetaInfo();
            }
        }

        public bool KeepSymLink
        {
            get => state.keepSymLink;
            set
            {
                state.keepSymLink = value;
                SaveGameMetaInfo();
            }
        }

        public bool Favorite
        {
            get => state.favorite;
            set
            {
                state.favorite = value;
                SaveGameMetaInfo();
            }
        }

        public bool FirstLaunch
        {
            get => state.firstLaunch;
            set
            {
                state.firstLaunch = value;
                SaveGameMetaInfo();
            }
        }

        public bool CheckUpdate
        {
            get => state.checkUpdate;
            set
            {
                state.checkUpdate = value;
                SaveGameMetaInfo();
            }
        }

        public bool UseApiIndex
        {
            get => state.useApiIndex;
            set
            {
                state.useApiIndex = value;
                state.useApiIndexForGuests = value;

                SaveGameMetaInfo();
            }
        }

        public bool UseApiIndexForGuests
        {
            get => state.useApiIndexForGuests;
            set
            {
                state.useApiIndexForGuests = value;
                if (gen.PlayersPerInstance > 0)
                {
                    SaveGameMetaInfo();
                }
            }
        }

        public bool ProfileAssignGamepadByButonPress
        {
            get => state.profileAssignGamepadByButonPress;
            set
            {
                state.profileAssignGamepadByButonPress = value;
                SaveGameMetaInfo();
            }
        }

        public string SteamLanguage
        {
            get => state.steamLanguage;
            set
            {
                state.steamLanguage = value;
                SaveGameMetaInfo();
            }
        }

        public void LoadGameMetaInfo(GenericGameInfo genericGameInfo)
        {
            try
            {
                gen = genericGameInfo;

                gameGuid = genericGameInfo.GUID;

                //Another GenericGameInfo of this game already loaded the values: share them (see MetaState).
                if (gameGuid != null)
                {
                    lock (metaInfoFileLock)
                    {
                        if (states.TryGetValue(gameGuid, out MetaState shared))
                        {
                            state = shared;
                            return;
                        }

                        states[gameGuid] = state;
                    }
                }

                string path = $"{nucleusEnvironment}\\{metaInfoJson}";

                if (File.Exists(path))
                {
                    string jsonString;
                    lock (metaInfoFileLock)
                    {
                        jsonString = File.ReadAllText(path);
                    }

                    JObject JMetaInfo = (JObject)JsonConvert.DeserializeObject(jsonString);

                    bool hasMetaInfo = JMetaInfo[gameGuid] != null;

                    if (hasMetaInfo)
                    {
                        state.totalPlayTime = (string)JMetaInfo[gameGuid]["TotalPlayTime"] ?? null;
                        state.lastPlayedAt = (string)JMetaInfo[gameGuid]["LastPlayedAt"] ?? null;
                        state.iconPath = (string)JMetaInfo[gameGuid]["IconPath"] ?? null;
                        state.saveProfile = JMetaInfo[gameGuid]["SaveProfile"] == null || (bool)JMetaInfo[gameGuid]["SaveProfile"];
                        state.disableProfiles = JMetaInfo[gameGuid]["DisableProfiles"] != null && (bool)JMetaInfo[gameGuid]["DisableProfiles"];
                        state.keepSymLink = JMetaInfo[gameGuid]["KeepSymLink"] != null && (bool)JMetaInfo[gameGuid]["KeepSymLink"];
                        state.favorite = JMetaInfo[gameGuid]["Favorite"] != null && (bool)JMetaInfo[gameGuid]["Favorite"];
                        state.firstLaunch = JMetaInfo[gameGuid]["FirstLaunch"] == null || (bool)JMetaInfo[gameGuid]["FirstLaunch"];
                        state.checkUpdate = JMetaInfo[gameGuid]["CheckUpdate"] == null || (bool)JMetaInfo[gameGuid]["CheckUpdate"];
                        state.useApiIndex = JMetaInfo[gameGuid]["UseApiIndex"] == null ? App_Misc.UseXinputIndex : (bool)JMetaInfo[gameGuid]["UseApiIndex"];                       
                        state.useApiIndexForGuests = JMetaInfo[gameGuid]["UseApiIndexForGuests"] == null ? state.useApiIndex : (bool)JMetaInfo[gameGuid]["UseApiIndexForGuests"];
                        state.profileAssignGamepadByButonPress = JMetaInfo[gameGuid]["ProfileAssignGamepadByButonPress"] == null ? App_Misc.ProfileAssignGamepadByButonPress : (bool)JMetaInfo[gameGuid]["ProfileAssignGamepadByButonPress"];
                        state.steamLanguage = (string)JMetaInfo[gameGuid]["SteamLanguage"] ?? "App Setting";
                        return;
                    }
                }

                // New game, or first run without metaInfo.json: start from the defaults (the first run used to
                // save the C# field defaults, e.g. CheckUpdate=false, for the first handler loaded).
                state.totalPlayTime = null;
                state.lastPlayedAt = null;
                state.iconPath = null;
                state.saveProfile = true;
                state.disableProfiles = false;
                state.keepSymLink = false;
                state.favorite = false;
                state.firstLaunch = true;
                state.checkUpdate = true;
                state.useApiIndex = App_Misc.UseXinputIndex;
                state.useApiIndexForGuests = state.useApiIndex;
                state.profileAssignGamepadByButonPress = App_Misc.ProfileAssignGamepadByButonPress;
                state.steamLanguage = "App Setting";

                SaveGameMetaInfo();
            }
            catch 
            {

            }
        }

        public void SaveGameMetaInfo()
        {
            bool saved = false;

            lock (metaInfoFileLock)
            {
                saved = WriteGameMetaInfo();
            }

            if (saved)
            {
                try
                {
                    Saved?.Invoke(gameGuid);
                }
                catch
                {
                }
            }
        }

        private bool WriteGameMetaInfo()
        {
            try
            {
                string path = $"{nucleusEnvironment}\\{metaInfoJson}";

                if (!Directory.Exists(nucleusEnvironment))
                {
                    Directory.CreateDirectory(nucleusEnvironment);
                }

                JObject JMetaInfo;

                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);

                    JMetaInfo = (JObject)JsonConvert.DeserializeObject(jsonString);

                    bool hasMetaInfo = JMetaInfo[gameGuid] != null;

                    if (hasMetaInfo)
                    {
                        JMetaInfo[gameGuid]["TotalPlayTime"] = state.totalPlayTime;
                        JMetaInfo[gameGuid]["LastPlayedAt"] = state.lastPlayedAt;
                        JMetaInfo[gameGuid]["IconPath"] = state.iconPath;
                        JMetaInfo[gameGuid]["SaveProfile"] = state.saveProfile;
                        JMetaInfo[gameGuid]["DisableProfiles"] = state.disableProfiles;
                        JMetaInfo[gameGuid]["KeepSymLink"] = state.keepSymLink;
                        JMetaInfo[gameGuid]["Favorite"] = state.favorite;
                        JMetaInfo[gameGuid]["FirstLaunch"] = state.firstLaunch;
                        JMetaInfo[gameGuid]["CheckUpdate"] = state.checkUpdate;
                        JMetaInfo[gameGuid]["UseApiIndex"] = state.useApiIndex;
                        JMetaInfo[gameGuid]["UseApiIndexForGuests"] = state.useApiIndexForGuests;
                        JMetaInfo[gameGuid]["ProfileAssignGamepadByButonPress"] = state.profileAssignGamepadByButonPress;
                        JMetaInfo[gameGuid]["SteamLanguage"] = state.steamLanguage;
                    }
                    else
                    {
                        //new game metaInfo
                        JProperty gameMeta = new JProperty(gameGuid, new JObject(new JProperty("TotalPlayTime", state.totalPlayTime),
                                                            new JProperty("LastPlayedAt", state.lastPlayedAt),
                                                            new JProperty("IconPath", state.iconPath),
                                                            new JProperty("SaveProfile", state.saveProfile),
                                                            new JProperty("DisableProfiles", state.disableProfiles),
                                                            new JProperty("KeepSymLink", state.keepSymLink),
                                                            new JProperty("Favorite", state.favorite),
                                                            new JProperty("FirstLaunch", state.firstLaunch),
                                                            new JProperty("CheckUpdate", state.checkUpdate),
                                                            new JProperty("UseApiIndex", state.useApiIndex),
                                                            new JProperty("UseApiIndexForGuests", state.useApiIndexForGuests),
                                                            new JProperty("ProfileAssignGamepadByButonPress", state.profileAssignGamepadByButonPress),
                                                            new JProperty("SteamLanguage", state.steamLanguage)
                                                            ));
                        JMetaInfo.Add(gameMeta);
                    }
                }
                else
                {
                    JObject gameMeta = new JObject(new JProperty("TotalPlayTime", state.totalPlayTime),
                                                   new JProperty("LastPlayedAt", state.lastPlayedAt),
                                                   new JProperty("IconPath", state.iconPath),
                                                   new JProperty("SaveProfile", state.saveProfile),
                                                   new JProperty("DisableProfiles", state.disableProfiles),
                                                   new JProperty("KeepSymLink", state.keepSymLink),
                                                   new JProperty("Favorite", state.favorite),
                                                   new JProperty("FirstLaunch", state.firstLaunch),
                                                   new JProperty("CheckUpdate", state.checkUpdate),
                                                   new JProperty("UseApiIndex", state.useApiIndex),
                                                   new JProperty("UseApiIndexForGuests", state.useApiIndexForGuests),
                                                   new JProperty("ProfileAssignGamepadByButonPress", state.profileAssignGamepadByButonPress),
                                                   new JProperty("SteamLanguage", state.steamLanguage)
                                                   );

                    JMetaInfo = new JObject(new JProperty(gameGuid, gameMeta));
                }

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(JMetaInfo, Formatting.Indented);
                        writer.Write(json);
                        stream.Flush();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string FormatPlayTime()
        {
            if (state.totalPlayTime == null || !ulong.TryParse(state.totalPlayTime, out ulong totalSeconds))
            {
                return "...";// 00h:00m:00s";
            }

            ulong seconds = totalSeconds % 60;
            ulong minutes = (totalSeconds / 60) % 60;
            ulong hours = totalSeconds / 3600;

            string formatHours = hours >= 10 ? "" : "0";
            string formatMinutes = minutes >= 10 ? "" : "0";
            string formatSecondes = seconds >= 10 ? "" : "0";

            return $"{formatHours}{hours}h:{formatMinutes}{minutes}m:{formatSecondes}{seconds}s";
        }

        private string GetLastPlayed()
        {
            if (state.lastPlayedAt == null)
            {
                return "...";
            }

            return state.lastPlayedAt.Split(' ')[0];//display the date only
        }

        private string GetLastPlayedFull()
        {
            if (state.lastPlayedAt == null)
            {
                return "...";
            }

            return state.lastPlayedAt;//returns the full datetime
        }


        public void StopGameplayTimerThread()
        {
            stopped = true;
        }

        public void SaveGameplayTime()
        {
            FirstLaunch = false;
            state.lastPlayedAt = DateTime.Now.ToString();

            while (!stopped)
            {
                Thread.Sleep((int)intervale);

                if (state.totalPlayTime == null || !ulong.TryParse(state.totalPlayTime, out ulong previousSeconds))
                {
                    state.totalPlayTime = (intervale / 1000).ToString();//seconds
                }
                else
                {
                    state.totalPlayTime = (intervale / 1000 + previousSeconds).ToString();//seconds
                }

                SaveGameMetaInfo();
            }

            stopped = false;
        }

        public void StartGameplayTimerThread()
        {
            //Ends with the session (it used to run a WPF dispatcher afterwards, which kept one thread per session alive).
            Thread gameplayTimerThread = new Thread(delegate ()
            {
                SaveGameplayTime();
            });

            gameplayTimerThread.IsBackground = true;
            gameplayTimerThread.Name = "Nucleus play time";
            gameplayTimerThread.SetApartmentState(ApartmentState.STA);
            gameplayTimerThread.Start();
        }
    }
}
