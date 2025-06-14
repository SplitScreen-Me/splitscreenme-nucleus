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
    public class GameMetaInfo
    {
        private readonly string nucleusEnvironment = $@"{Globals.UserEnvironmentRoot}\NucleusCoop";
        private readonly string metaInfoJson = "metaInfo.json";

        private ulong intervale = 20000;//milliseconds
        private bool stopped;

        private GenericGameInfo gen;
        private string gameGuid;

        private string lastPlayedAt;
        public string LastPlayedAt => GetLastPlayed();

        public string LastPlayedAtFull => GetLastPlayedFull();

        private string totalPlayTime;
        public string TotalPlayTime => FormatPlayTime();

        private string iconPath;
        public string IconPath
        {
            get => iconPath;
            set
            {
                iconPath = value;
                SaveGameMetaInfo();
            }
        }

        private bool saveProfile;
        public bool SaveProfile
        {
            get => saveProfile;
            set
            {
                saveProfile = value;
                SaveGameMetaInfo();
            }
        }

        private bool disableProfiles;
        public bool DisableProfiles
        {
            get => disableProfiles;
            set
            {
                disableProfiles = value;
                SaveGameMetaInfo();
            }
        }

        private bool keepSymLink;
        public bool KeepSymLink
        {
            get => keepSymLink;
            set
            {
                keepSymLink = value;
                SaveGameMetaInfo();
            }
        }

        private bool favorite = false;
        public bool Favorite
        {
            get => favorite;
            set
            {
                favorite = value;
                SaveGameMetaInfo();
            }
        }

        private bool firstLaunch;
        public bool FirstLaunch
        {
            get => firstLaunch;
            set
            {
                firstLaunch = value;
                SaveGameMetaInfo();
            }
        }

        private bool checkUpdate;
        public bool CheckUpdate
        {
            get => checkUpdate;
            set
            {
                checkUpdate = value;
                SaveGameMetaInfo();
            }
        }

        private bool useApiIndex;
        public bool UseApiIndex
        {
            get => useApiIndex;
            set
            {
                useApiIndex = value;

                if(useApiIndex)
                {
                    useApiIndexForGuests = true;
                }

                SaveGameMetaInfo();
            }
        }

        private bool useApiIndexForGuests;
        public bool UseApiIndexForGuests
        {
            get => useApiIndexForGuests;
            set
            {
                useApiIndexForGuests = value;
                if (gen.PlayersPerInstance > 0)
                {
                    SaveGameMetaInfo();
                }
            }
        }

        private string steamLanguage;
        public string SteamLanguage
        {
            get => steamLanguage;
            set
            {
                steamLanguage = value;
                SaveGameMetaInfo();
            }
        }

        public void LoadGameMetaInfo(GenericGameInfo genericGameInfo)
        {
            try
            {
                gen = genericGameInfo;

                gameGuid = genericGameInfo.GUID;

                string path = $"{nucleusEnvironment}\\{metaInfoJson}";

                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);

                    JObject JMetaInfo = (JObject)JsonConvert.DeserializeObject(jsonString);

                    bool hasMetaInfo = JMetaInfo[gameGuid] != null;

                    if (hasMetaInfo)
                    {
                        totalPlayTime = (string)JMetaInfo[gameGuid]["TotalPlayTime"] ?? null;
                        lastPlayedAt = (string)JMetaInfo[gameGuid]["LastPlayedAt"] ?? null;
                        iconPath = (string)JMetaInfo[gameGuid]["IconPath"] ?? null;
                        saveProfile = JMetaInfo[gameGuid]["SaveProfile"] == null || (bool)JMetaInfo[gameGuid]["SaveProfile"];
                        disableProfiles = JMetaInfo[gameGuid]["DisableProfiles"] != null && (bool)JMetaInfo[gameGuid]["DisableProfiles"];
                        keepSymLink = JMetaInfo[gameGuid]["KeepSymLink"] != null && (bool)JMetaInfo[gameGuid]["KeepSymLink"];
                        favorite = JMetaInfo[gameGuid]["Favorite"] != null && (bool)JMetaInfo[gameGuid]["Favorite"];
                        firstLaunch = JMetaInfo[gameGuid]["FirstLaunch"] == null || (bool)JMetaInfo[gameGuid]["FirstLaunch"];
                        checkUpdate = JMetaInfo[gameGuid]["CheckUpdate"] == null || (bool)JMetaInfo[gameGuid]["CheckUpdate"];
                        useApiIndex = JMetaInfo[gameGuid]["UseApiIndex"] == null ? App_Misc.UseXinputIndex : (bool)JMetaInfo[gameGuid]["UseApiIndex"];                       
                        useApiIndexForGuests = JMetaInfo[gameGuid]["UseApiIndexForGuests"] == null ? useApiIndex : (bool)JMetaInfo[gameGuid]["UseApiIndexForGuests"];                                            
                        steamLanguage = (string)JMetaInfo[gameGuid]["SteamLanguage"] ?? "App Setting";
                        return;
                    }

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
                    steamLanguage = "App Setting";
                }

                SaveGameMetaInfo();
            }
            catch 
            {

            }
        }

        public void SaveGameMetaInfo()
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
                        JMetaInfo[gameGuid]["TotalPlayTime"] = totalPlayTime;
                        JMetaInfo[gameGuid]["LastPlayedAt"] = lastPlayedAt;
                        JMetaInfo[gameGuid]["IconPath"] = iconPath;
                        JMetaInfo[gameGuid]["SaveProfile"] = saveProfile;
                        JMetaInfo[gameGuid]["DisableProfiles"] = disableProfiles;
                        JMetaInfo[gameGuid]["KeepSymLink"] = keepSymLink;
                        JMetaInfo[gameGuid]["Favorite"] = favorite;
                        JMetaInfo[gameGuid]["FirstLaunch"] = firstLaunch;
                        JMetaInfo[gameGuid]["CheckUpdate"] = checkUpdate;
                        JMetaInfo[gameGuid]["UseApiIndex"] = useApiIndex;
                        JMetaInfo[gameGuid]["UseApiIndexForGuests"] = useApiIndexForGuests;
                        JMetaInfo[gameGuid]["SteamLanguage"] = steamLanguage;
                    }
                    else
                    {
                        //new game metaInfo
                        JProperty gameMeta = new JProperty(gameGuid, new JObject(new JProperty("TotalPlayTime", totalPlayTime),
                                                            new JProperty("LastPlayedAt", lastPlayedAt),
                                                            new JProperty("IconPath", iconPath),
                                                            new JProperty("SaveProfile", saveProfile),
                                                            new JProperty("DisableProfiles", disableProfiles),
                                                            new JProperty("KeepSymLink", keepSymLink),
                                                            new JProperty("Favorite", favorite),
                                                            new JProperty("FirstLaunch", firstLaunch),
                                                            new JProperty("CheckUpdate", checkUpdate),
                                                            new JProperty("UseApiIndex", useApiIndex),
                                                            new JProperty("UseApiIndexForGuests", useApiIndexForGuests),
                                                            new JProperty("SteamLanguage", steamLanguage)
                                                            ));
                        JMetaInfo.Add(gameMeta);
                    }
                }
                else
                {
                    JObject gameMeta = new JObject(new JProperty("TotalPlayTime", totalPlayTime),
                                                   new JProperty("LastPlayedAt", lastPlayedAt),
                                                   new JProperty("IconPath", iconPath),
                                                   new JProperty("SaveProfile", saveProfile),
                                                   new JProperty("DisableProfiles", disableProfiles),
                                                   new JProperty("KeepSymLink", keepSymLink),
                                                   new JProperty("Favorite", favorite),
                                                   new JProperty("FirstLaunch", firstLaunch),
                                                   new JProperty("CheckUpdate", checkUpdate),
                                                   new JProperty("UseApiIndex", useApiIndex),
                                                    new JProperty("UseApiIndexForGuests", useApiIndexForGuests),
                                                   new JProperty("SteamLanguage", steamLanguage)
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
            }
            catch
            {

            }
        }

        private string FormatPlayTime()
        {
            if (totalPlayTime == null)
            {
                return "...";// 00h:00m:00s";
            }

            ulong totalSeconds = ulong.Parse(totalPlayTime);

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
            if (lastPlayedAt == null)
            {
                return "...";
            }

            return lastPlayedAt.Split(' ')[0];//display the date only
        }

        private string GetLastPlayedFull()
        {
            if (lastPlayedAt == null)
            {
                return "...";
            }

            return lastPlayedAt;//returns the full datetime
        }


        public void StopGameplayTimerThread()
        {
            stopped = true;
        }

        public void SaveGameplayTime()
        {
            FirstLaunch = false;
            lastPlayedAt = DateTime.Now.ToString();

            while (!stopped)
            {
                Thread.Sleep((int)intervale);

                if (totalPlayTime == null)
                {
                    totalPlayTime = (intervale / 1000).ToString();//seconds
                }
                else
                {
                    totalPlayTime = (intervale / 1000 + ulong.Parse(totalPlayTime)).ToString();//seconds
                }

                SaveGameMetaInfo();
            }

            stopped = false;
        }

        public void StartGameplayTimerThread()
        {
            Thread gameplayTimerThread = new Thread(delegate ()
            {
                SaveGameplayTime();
                System.Windows.Threading.Dispatcher.Run();
            });

            gameplayTimerThread.SetApartmentState(ApartmentState.STA);
            gameplayTimerThread.Start();
        }
    }
}
