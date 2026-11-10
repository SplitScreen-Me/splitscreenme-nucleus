using Jint.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop.InputManagement.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    public class GameMetaInfo
    {
        private readonly string nucleusEnvironment = $@"{Globals.UserEnvironmentRoot}\NucleusCoop";
#if DEBUG
        private readonly string metaInfoJson = "metaInfo_DEBUG.json";
#else
        private readonly string metaInfoJson = "metaInfo.json";
#endif

        private const int SaveDebounceMilliseconds = 400;
        private static readonly object FileLock = new object();

        private readonly object sync = new object();
        private readonly System.Threading.Timer saveTimer;

        private readonly ulong intervale = 20000;
        private volatile bool stopped;

        private GenericGameInfo gen;
        private string gameGuid;

        private string lastPlayedAt;
        private string totalPlayTime;
        private string iconPath;
        private bool saveProfile;
        private bool disableProfiles;
        private bool keepSymLink;
        private bool favorite;
        private bool firstLaunch;
        private bool checkUpdate;
        private bool useApiIndex;
        private bool useApiIndexForGuests;
        private bool profileAssignGamepadByButonPress;
        private string steamLanguage;
        private bool tryRestartOnGameCrash;

        public GameMetaInfo()
        {
            saveTimer = new System.Threading.Timer(
                _ => SaveGameMetaInfoNow(),
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        public string LastPlayedAt => GetLastPlayed();
        public string LastPlayedAtFull => GetLastPlayedFull();
        public string TotalPlayTime => FormatPlayTime();

        public string IconPath
        {
            get { lock (sync) return iconPath; }
            set { lock (sync) iconPath = value; SaveGameMetaInfo(); }
        }

        public bool SaveProfile
        {
            get { lock (sync) return saveProfile; }
            set { lock (sync) saveProfile = value; SaveGameMetaInfo(); }
        }

        public bool DisableProfiles
        {
            get { lock (sync) return disableProfiles; }
            set { lock (sync) disableProfiles = value; SaveGameMetaInfo(); }
        }

        public bool KeepSymLink
        {
            get { lock (sync) return keepSymLink; }
            set { lock (sync) keepSymLink = value; SaveGameMetaInfo(); }
        }

        public bool Favorite
        {
            get { lock (sync) return favorite; }
            set { lock (sync) favorite = value; SaveGameMetaInfo(); }
        }

        public bool FirstLaunch
        {
            get { lock (sync) return firstLaunch; }
            set { lock (sync) firstLaunch = value; SaveGameMetaInfo(); }
        }

        public bool CheckUpdate
        {
            get { lock (sync) return checkUpdate; }
            set { lock (sync) checkUpdate = value; SaveGameMetaInfo(); }
        }

        public bool UseApiIndex
        {
            get { lock (sync) return useApiIndex; }
            set
            {
                lock (sync)
                {
                    useApiIndex = value;
                    useApiIndexForGuests = value;
                }
                SaveGameMetaInfo();
            }
        }

        public bool UseApiIndexForGuests
        {
            get { lock (sync) return useApiIndexForGuests; }
            set
            {
                bool shouldSave;
                lock (sync)
                {
                    useApiIndexForGuests = value;
                    shouldSave = gen != null && gen.PlayersPerInstance > 0;
                }

                if (shouldSave)
                    SaveGameMetaInfo();
            }
        }

        public bool TryRestartOnGameCrash
        {
            get { lock (sync) return tryRestartOnGameCrash; }
            set { lock (sync) tryRestartOnGameCrash = value; SaveGameMetaInfo(); }
        }

        public bool ProfileAssignGamepadByButonPress
        {
            get { lock (sync) return profileAssignGamepadByButonPress; }
            set { lock (sync) profileAssignGamepadByButonPress = value; SaveGameMetaInfo(); }
        }

        public string SteamLanguage
        {
            get { lock (sync) return steamLanguage; }
            set { lock (sync) steamLanguage = value; SaveGameMetaInfo(); }
        }

        public void LoadGameMetaInfo(GenericGameInfo genericGameInfo)
        {
            if (genericGameInfo == null)
                throw new ArgumentNullException(nameof(genericGameInfo));

            bool needsInitialSave = false;
            string path = GetMetaInfoPath();

            try
            {
                lock (sync)
                {
                    gen = genericGameInfo;
                    gameGuid = genericGameInfo.GUID;

                    JObject root = null;

                    lock (FileLock)
                    {
                        if (File.Exists(path))
                        {
                            string json = File.ReadAllText(path);
                            root = JObject.Parse(json);
                        }
                    }

                    JObject entry = root?[gameGuid] as JObject;
                    if (entry != null)
                    {
                        totalPlayTime = (string)entry["TotalPlayTime"];
                        lastPlayedAt = (string)entry["LastPlayedAt"];
                        iconPath = (string)entry["IconPath"];
                        saveProfile = entry["SaveProfile"] == null || (bool)entry["SaveProfile"];
                        disableProfiles = entry["DisableProfiles"] != null && (bool)entry["DisableProfiles"];
                        keepSymLink = entry["KeepSymLink"] != null && (bool)entry["KeepSymLink"];
                        favorite = entry["Favorite"] != null && (bool)entry["Favorite"];
                        firstLaunch = entry["FirstLaunch"] == null || (bool)entry["FirstLaunch"];
                        checkUpdate = entry["CheckUpdate"] == null || (bool)entry["CheckUpdate"];
                        useApiIndex = entry["UseApiIndex"] == null
                            ? App_Misc.UseXinputIndex
                            : (bool)entry["UseApiIndex"];
                        useApiIndexForGuests = entry["UseApiIndexForGuests"] == null
                            ? useApiIndex
                            : (bool)entry["UseApiIndexForGuests"];
                        profileAssignGamepadByButonPress = entry["ProfileAssignGamepadByButonPress"] == null
                            ? App_Misc.ProfileAssignGamepadByButonPress
                            : (bool)entry["ProfileAssignGamepadByButonPress"];
                        steamLanguage = (string)entry["SteamLanguage"] ?? "App Setting";
                        tryRestartOnGameCrash = entry["TryRestartOnGameCrash"] != null &&
                        (bool)entry["TryRestartOnGameCrash"];
                    }
                    else
                    {
                        SetDefaults();
                        needsInitialSave = true;
                    }
                }

                if (needsInitialSave)
                    SaveGameMetaInfo();
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Failed to load game metadata: {ex}");
            }
        }

        public void SaveGameMetaInfo()
        {
            lock (sync)
            {
                if (string.IsNullOrEmpty(gameGuid))
                    return;

                saveTimer.Change(SaveDebounceMilliseconds, Timeout.Infinite);
            }
        }

        public void FlushGameMetaInfo()
        {
            saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            SaveGameMetaInfoNow();
        }

        private void SaveGameMetaInfoNow()
        {
            string path = GetMetaInfoPath();

            try
            {
                lock (sync)
                {
                    if (string.IsNullOrEmpty(gameGuid))
                        return;

                    Directory.CreateDirectory(nucleusEnvironment);

                    lock (FileLock)
                    {
                        JObject root;

                        if (File.Exists(path))
                        {
                            try
                            {
                                root = JObject.Parse(File.ReadAllText(path));
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteLine($"Metadata JSON is invalid: {ex}");
                                return;
                            }
                        }
                        else
                        {
                            root = new JObject();
                        }

                        root[gameGuid] = CreateGameEntry();

                        WriteJsonAtomically(path, JsonConvert.SerializeObject(root, Formatting.Indented));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Failed to save game metadata: {ex}");
            }
        }

        private JObject CreateGameEntry()
        {
            return new JObject
            {
                ["TotalPlayTime"] = totalPlayTime == null ? JValue.CreateNull() : new JValue(totalPlayTime),
                ["LastPlayedAt"] = lastPlayedAt == null ? JValue.CreateNull() : new JValue(lastPlayedAt),
                ["IconPath"] = iconPath == null ? JValue.CreateNull() : new JValue(iconPath),
                ["SaveProfile"] = saveProfile,
                ["DisableProfiles"] = disableProfiles,
                ["KeepSymLink"] = keepSymLink,
                ["Favorite"] = favorite,
                ["FirstLaunch"] = firstLaunch,
                ["CheckUpdate"] = checkUpdate,
                ["UseApiIndex"] = useApiIndex,
                ["UseApiIndexForGuests"] = useApiIndexForGuests,
                ["ProfileAssignGamepadByButonPress"] = profileAssignGamepadByButonPress,
                ["SteamLanguage"] = steamLanguage == null ? JValue.CreateNull() : new JValue(steamLanguage),
                ["TryRestartOnGameCrash"] = tryRestartOnGameCrash,
            };
        }

        private void WriteJsonAtomically(string path, string json)
        {
            string tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

                using (var stream = new FileStream(
                    tempPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(path))
                {
                    File.Replace(tempPath, path, null);
                }
                else
                {
                    File.Move(tempPath, path);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    { }
                }
            }
        }

        private string GetMetaInfoPath()
        {
            return Path.Combine(nucleusEnvironment, metaInfoJson);
        }

        private void SetDefaults()
        {
            totalPlayTime = null;
            lastPlayedAt = null;
            iconPath = null;
            saveProfile = true;
            disableProfiles = false;
            keepSymLink = false;
            favorite = false;
            firstLaunch = true;
            checkUpdate = true;
            useApiIndex = App_Misc.UseXinputIndex;
            useApiIndexForGuests = useApiIndex;
            profileAssignGamepadByButonPress = App_Misc.ProfileAssignGamepadByButonPress;
            steamLanguage = "App Setting";
            tryRestartOnGameCrash = false;
        }

        private string FormatPlayTime()
        {
            lock (sync)
            {
                if (totalPlayTime == null)
                    return "...";

                if (!ulong.TryParse(totalPlayTime, out ulong totalSeconds))
                    return "...";

                ulong seconds = totalSeconds % 60;
                ulong minutes = (totalSeconds / 60) % 60;
                ulong hours = totalSeconds / 3600;

                string formatHours = hours >= 10 ? "" : "0";
                string formatMinutes = minutes >= 10 ? "" : "0";
                string formatSeconds = seconds >= 10 ? "" : "0";

                return $"{formatHours}{hours}h:{formatMinutes}{minutes}m:{formatSeconds}{seconds}s";
            }
        }

        private string GetLastPlayed()
        {
            lock (sync)
            {
                return lastPlayedAt == null ? "..." : lastPlayedAt.Split(' ')[0];
            }
        }

        private string GetLastPlayedFull()
        {
            lock (sync)
            {
                return lastPlayedAt ?? "...";
            }
        }

        public void StopGameplayTimerThread()
        {
            stopped = true;
        }

        public void SaveGameplayTime()
        {
            lock (sync)
            {
                firstLaunch = false;
                lastPlayedAt = DateTime.Now.ToString();
            }
            SaveGameMetaInfo();

            while (!stopped)
            {
                Thread.Sleep((int)intervale);

                if (stopped)
                    break;

                lock (sync)
                {
                    ulong currentSeconds = 0;
                    if (totalPlayTime != null)
                        ulong.TryParse(totalPlayTime, out currentSeconds);

                    totalPlayTime = (currentSeconds + intervale / 1000).ToString();
                }

                SaveGameMetaInfo();
            }

            stopped = false;
            FlushGameMetaInfo();
        }

        public void StartGameplayTimerThread()
        {
            stopped = false;

            Thread gameplayTimerThread = new Thread(delegate ()
            {
                SaveGameplayTime();
                System.Windows.Threading.Dispatcher.Run();
            })
            {
                IsBackground = true
            };

            gameplayTimerThread.SetApartmentState(ApartmentState.STA);
            gameplayTimerThread.Start();
        }
    }
}
