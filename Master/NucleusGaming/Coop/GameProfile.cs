using IWshRuntimeLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    public class GameProfile
    {
        private List<UserScreen> screens;
        public List<UserScreen> Screens => screens;

        public Dictionary<string, object> Options => options;
        private Dictionary<string, object> options;
        /// <summary>
        /// Return a list of all players(connected devices)
        /// </summary>       
        public List<PlayerInfo> DevicesList => GetDevicesList();
        private List<PlayerInfo> deviceList;

        public static GameProfile Instance;

        public static GenericGameInfo Game;

        public static int TotalAssignedPlayers;//K&m player will count as one only if 2 devices(keyboard & mouse) do share the same bounds.

        private static int totalProfilePlayers;//How many players the loaded profile counts.
        public static int TotalProfilePlayers => totalProfilePlayers;

        private static int profilesCount;//Used to check if we need to create a new profile 
        public static int ProfilesCount => profilesCount;

        private static int profileToSave;
        public static int CurrentProfileId => profileToSave;

        private static string modeText;// = "New Profile";
        public static string ModeText => modeText;
        public static bool IsNewProfile => modeText == "New Profile";

        public static UserGameInfo GameInfo;

        public static List<string> profilesPathList = new List<string>();

        public static IDictionary<string, string> AudioInstances = new Dictionary<string, string>();

        private static bool showError = false;

        public static bool Loaded => totalProfilePlayers > 0;

        private static int hWndInterval;
        public static int HWndInterval
        {
            get => hWndInterval;
            set => hWndInterval = value;
        }

        private static bool useSplitDiv;
        public static bool UseSplitDiv
        {
            get => useSplitDiv;
            set => useSplitDiv = value;
        }

        private static bool hideDesktopOnly;
        public static bool HideDesktopOnly
        {
            get => hideDesktopOnly;
            set => hideDesktopOnly = value;
        }
        private static int customLayout_Ver;
        public static int CustomLayout_Ver
        {
            get => customLayout_Ver;
            set => customLayout_Ver = value;
        }

        private static int customLayout_Hor;
        public static int CustomLayout_Hor
        {
            get => customLayout_Hor;
            set => customLayout_Hor = value;
        }

        private static int customLayout_Max;
        public static int CustomLayout_Max
        {
            get => customLayout_Max;
            set => customLayout_Max = value;
        }

        private static bool autoDesktopScaling;
        public static bool AutoDesktopScaling
        {
            get => autoDesktopScaling;
            set => autoDesktopScaling = value;
        }

        private static bool enableWindowsMerger;
        public static bool EnableWindowsMerger
        {
            get => enableWindowsMerger;
            set => enableWindowsMerger = value;
        }

        private static bool enableLosslessHook;
        public static bool EnableLosslessHook
        {
            get => enableLosslessHook;
            set => enableLosslessHook = value;
        }

        private static bool autoPlay;
        public static bool AutoPlay
        {
            get => autoPlay;
            set => autoPlay = value;
        }

        private static string splitDivColor = "Black";
        public static string SplitDivColor
        {
            get => splitDivColor;
            set => splitDivColor = value;
        }

        private static string network = "Automatic";
        public static string Network
        {
            get => network;
            set => network = value;
        }

        private static string notes;
        public static string Notes
        {
            get => notes;
            set => notes = value;
        }

        private static string title;
        public static string Title
        {
            get => title;
            set => title = value;
        }

        private static string mergerResolution;
        public static string MergerResolution
        {
            get => mergerResolution;
            set => mergerResolution = value;
        }

        private static int pauseBetweenInstanceLaunch;
        public static int PauseBetweenInstanceLaunch
        {
            get => pauseBetweenInstanceLaunch;
            set => pauseBetweenInstanceLaunch = value;
        }

        private static bool useNicknames;
        public static bool UseNicknames
        {
            get => useNicknames;
            set => useNicknames = value;
        }

        private static bool audioDefaultSettings;
        public static bool AudioDefaultSettings
        {
            get => audioDefaultSettings;
            set => audioDefaultSettings = value;
        }

        private static bool audioCustomSettings;
        public static bool AudioCustomSettings
        {
            get => audioCustomSettings;
            set => audioCustomSettings = value;
        }

        private static bool cts_MuteAudioOnly;
        public static bool Cts_MuteAudioOnly
        {
            get => cts_MuteAudioOnly;
            set => cts_MuteAudioOnly = value;
        }

        private static bool cts_KeepAspectRatio;
        public static bool Cts_KeepAspectRatio
        {
            get => cts_KeepAspectRatio;
            set => cts_KeepAspectRatio = value;
        }

        private static bool cts_Unfocus;
        public static bool Cts_Unfocus
        {
            get => cts_Unfocus;
            set => cts_Unfocus = value;
        }

        private static bool stop_UINav;
        public static bool Stop_UINav
        {
            get => stop_UINav;
            set => stop_UINav = value;
        }

        private static int gamepadCount;
        public static int GamepadCount => gamepadCount;

        private static int keyboardCount;
        public static int KeyboardCount => keyboardCount;

        private static bool saved;
        public static bool Saved => saved;

        public static bool Ready;

        private static bool useXinputIndex;
        public static bool UseXinputIndex
        {
            get => useXinputIndex;
            set => useXinputIndex = value;
        }

        //Avoid "Autoplay" to be applied right after setting the option in profile settings
        private static bool updating;
        public static bool Updating
        {
            get => updating;
            set => updating = value;
        }

        public static List<RectangleF> AllScreens = new List<RectangleF>();
        public static List<PlayerInfo> ProfilePlayersList = new List<PlayerInfo>();
        public static List<PlayerInfo> AssignedDevices = new List<PlayerInfo>();
        public static List<PlayerInfo> DevicesToMerge = new List<PlayerInfo>();
        public static List<(Rectangle, RectangleF)> GhostBounds = new List<(Rectangle, RectangleF)>();

        private List<PlayerInfo> GetDevicesList()
        {
            lock (deviceList)
            {
                return deviceList;
            }
        }

        private void ListGameProfiles()
        {
            profilesPathList.Clear();
            profilesCount = 0;

            string path = GetGameProfilesPath();

            if (path != null)
            {
                profilesPathList = Directory.EnumerateFiles(path).OrderBy(s => s.Length).ToList();
                profilesCount = profilesPathList.Count();
            }
        }

        public virtual void Reset()
        {
            bool profileDisabled = App_Misc.DisableGameProfiles;

            SetupScreenControl.Instance?.CanPlayUpdated(false, false);

            ProfilePlayersList.Clear();
            AllScreens.Clear();
            GhostBounds.Clear();
            totalProfilePlayers = 0;
            AudioInstances.Clear();

            showError = false;
            autoPlay = false;

            UpdateSharedSettings();

            notes = string.Empty;
            title = string.Empty;

            hWndInterval = 0;
            pauseBetweenInstanceLaunch = 0;

            profileToSave = 0;

            gamepadCount = 0;
            keyboardCount = 0;

            modeText = "New Profile";

            Ready = false;
            saved = false;

            if (!profileDisabled)
            {
                TotalAssignedPlayers = 0;

                if(SetupScreenControl.Instance != null)
                {
                    BoundsFunctions.RefreshScreens();
                }
              
                if (deviceList != null)//Switching profile
                {
                    foreach (PlayerInfo player in DevicesList)
                    {
                        player.MonitorBounds = Rectangle.Empty;
                        player.EditBounds = player.SourceEditBounds;
                        player.ScreenIndex = -1;
                        player.PlayerID = -1;
                        player.SteamID = 0;
                        player.Nickname = null;
                    }

                    RefreshKeyboardAndMouse();
                }

                options = new Dictionary<string, object>();

                foreach (GameOption opt in Game.Options)
                {
                    options.Add(opt.Key, opt.Value);
                }

                ProfilesList.Instance?.Update_Unload();

                AssignedDevices.Clear();
                DevicesToMerge.Clear();

                if (!GameInfo.Game.MetaInfo.DisableProfiles)
                {
                    ListGameProfiles();
                }
            }

            if (SetupScreenControl.Instance != null)
            {
                if (BoundsFunctions.Screens != null)
                {
                    screens = BoundsFunctions.Screens.ToList();
                }
            }
            
        }

        public virtual void InitializeDefault(UserGameInfo userGameInfo)
        {
            Instance = this;

            GameInfo = userGameInfo;
            Game = userGameInfo.Game;

            Reset();

            RawInputProcessor.CurrentProfile = this;

            if (deviceList == null)
            {
                deviceList = new List<PlayerInfo>();
            }

            AssignedDevices.Clear();
            DevicesToMerge.Clear();

            TotalAssignedPlayers = 0;

            if (screens == null)
            {
                screens = new List<UserScreen>();
            }

            if (options == null)
            {
                options = new Dictionary<string, object>();

                foreach (GameOption opt in Game.Options)
                {
                    options.Add(opt.Key, opt.Value);
                }
            }
        }

        public static void UpdateSharedSettings()
        {
            autoDesktopScaling = App_Misc.AutoDesktopScaling;
            useNicknames = App_Misc.UseNicksInGame;
            useXinputIndex = App_Misc.UseXinputIndex;
            network = App_Misc.Network;

            audioCustomSettings = int.Parse(App_Audio.Custom) == 1;
            audioDefaultSettings = audioCustomSettings == false;
            

            useSplitDiv = App_Layouts.SplitDiv;
            splitDivColor = App_Layouts.SplitDivColor;
            hideDesktopOnly = App_Layouts.HideOnly;
            customLayout_Ver = App_Layouts.VerticalLines;
            customLayout_Hor = App_Layouts.HorizontalLines;
            customLayout_Max = App_Layouts.MaxPlayers;
            cts_MuteAudioOnly = App_Layouts.Cts_MuteAudioOnly;
            cts_KeepAspectRatio = App_Layouts.Cts_KeepAspectRatio;
            cts_Unfocus = App_Layouts.Cts_Unfocus;

            enableWindowsMerger = App_Layouts.WindowsMerger;
            enableLosslessHook = App_Layouts.LosslessHook;
            mergerResolution = App_Layouts.WindowsMergerRes;

            foreach(var output in App_Audio.Instances_AudioOutput)
            {
                AudioInstances.Add(output);
            }
        }

        public virtual bool LoadGameProfile(int _profileToLoad)
        {
            try
            {
                string path = $"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}\\Profile[{_profileToLoad}].json";

                string jsonString = System.IO.File.ReadAllText(path);

                JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);

                Reset();

                profileToSave = _profileToLoad;///to keep after reset()

                if (Jprofile == null)
                {
                    Ready = false;
                    return false;
                }

                JToken Joptions = Jprofile["Options"] as JToken;

                options = new Dictionary<string, object>();

                foreach (JProperty Jopt in Joptions)
                {
                    options.Add((string)Jopt.Name, Jopt.Value.ToString());
                }

                if (hWndInterval == 0)
                {
                    HWndInterval = (int)Jprofile["WindowsSetupTiming"]["Time"];
                }

                if (pauseBetweenInstanceLaunch == 0)
                {
                    PauseBetweenInstanceLaunch = (int)Jprofile["PauseBetweenInstanceLaunch"]["Time"];
                }

                UseSplitDiv = (bool)Jprofile["UseSplitDiv"]["Enabled"];

                if (Jprofile["UseSplitDiv"]["HideOnly"] != null)
                {
                    HideDesktopOnly = (bool)Jprofile["UseSplitDiv"]["HideOnly"];
                }

                SplitDivColor = (string)Jprofile["UseSplitDiv"]["Color"];
                AutoDesktopScaling = (bool)Jprofile["AutoDesktopScaling"]["Enabled"];
                UseNicknames = (bool)Jprofile["UseNicknames"]["Use"];
                Cts_KeepAspectRatio = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_KeepAspectRatio"];
                Cts_MuteAudioOnly = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_MuteAudioOnly"];
                Cts_Unfocus = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_Unfocus"];

                Stop_UINav = Jprofile["StopGPUINav"] != null ? (bool)Jprofile["StopGPUINav"]["Enabled"] : false;

                EnableWindowsMerger = Jprofile["EnableWindowsMerger"] != null ? (bool)Jprofile["EnableWindowsMerger"]["Enabled"] : false;
                EnableLosslessHook = Jprofile["EnableLosslessHook"] != null ? (bool)Jprofile["EnableLosslessHook"]["Enabled"] : false;
                MergerResolution = Jprofile["MergerResolution"] != null ? (string)Jprofile["MergerResolution"] : "";

                UseXinputIndex = (bool)Jprofile["Use XInput Index"];
                Network = (string)Jprofile["Network"]["Type"];
                CustomLayout_Ver = (int)Jprofile["CustomLayout"]["Ver"];
                CustomLayout_Hor = (int)Jprofile["CustomLayout"]["Hor"];
                CustomLayout_Max = (int)Jprofile["CustomLayout"]["Max"];
                AutoPlay = (bool)Jprofile["AutoPlay"]["Enabled"];
                Notes = (string)Jprofile["Notes"];
                Title = (string)Jprofile["Title"];

                JToken JplayersInfos = Jprofile["Data"] as JToken;

                for (int i = 0; i < JplayersInfos.Count(); i++)
                {
                    PlayerInfo player = new PlayerInfo();

                    player.PlayerID = (int)JplayersInfos[i]["PlayerID"];
                    player.Nickname = (string)JplayersInfos[i]["Nickname"];
                    player.SteamID = (long)JplayersInfos[i]["SteamID"];
                    player.GamepadGuid = (Guid)JplayersInfos[i]["GamepadGuid"];
                    player.OwnerType = (int)JplayersInfos[i]["Owner"]["Type"];
                    player.DisplayIndex = (int)JplayersInfos[i]["Owner"]["DisplayIndex"];

                    player.OwnerDisplay = new Rectangle(
                                                   (int)JplayersInfos[i]["Owner"]["Display"]["X"],
                                                   (int)JplayersInfos[i]["Owner"]["Display"]["Y"],
                                                   (int)JplayersInfos[i]["Owner"]["Display"]["Width"],
                                                   (int)JplayersInfos[i]["Owner"]["Display"]["Height"]);

                    player.OwnerUIBounds = new RectangleF(
                                                   (float)JplayersInfos[i]["Owner"]["UiBounds"]["X"],
                                                   (float)JplayersInfos[i]["Owner"]["UiBounds"]["Y"],
                                                   (float)JplayersInfos[i]["Owner"]["UiBounds"]["Width"],
                                                   (float)JplayersInfos[i]["Owner"]["UiBounds"]["Height"]);


                    player.MonitorBounds = new Rectangle(
                                                   (int)JplayersInfos[i]["MonitorBounds"]["X"],
                                                   (int)JplayersInfos[i]["MonitorBounds"]["Y"],
                                                   (int)JplayersInfos[i]["MonitorBounds"]["Width"],
                                                   (int)JplayersInfos[i]["MonitorBounds"]["Height"]);

                    player.EditBounds = new RectangleF(
                                                  (float)JplayersInfos[i]["EditBounds"]["X"],
                                                  (float)JplayersInfos[i]["EditBounds"]["Y"],
                                                  (float)JplayersInfos[i]["EditBounds"]["Width"],
                                                  (float)JplayersInfos[i]["EditBounds"]["Height"]);


                    player.IsDInput = (bool)JplayersInfos[i]["IsDInput"];
                    player.IsXInput = (bool)JplayersInfos[i]["IsXInput"];
                    player.IsSDL2 = JplayersInfos[i]["IsSDL2"] == null  ? false : (bool)JplayersInfos[i]["IsSDL2"];

                    player.IsKeyboardPlayer = (bool)JplayersInfos[i]["IsKeyboardPlayer"];
                    player.IsRawMouse = (bool)JplayersInfos[i]["IsRawMouse"];

                    string[] hidIds = new string[] { "", "" };
                    for (int h = 0; h < JplayersInfos[i]["HIDDeviceID"].Count(); h++)
                    {
                        hidIds.SetValue(JplayersInfos[i]["HIDDeviceID"][h].ToString(), h);
                    }

                    player.HIDDeviceID = hidIds;

                    player.ScreenPriority = (int)JplayersInfos[i]["ScreenPriority"];
                    player.ScreenIndex = (int)JplayersInfos[i]["ScreenIndex"];

                    player.IdealProcessor = (string)JplayersInfos[i]["Processor"]["IdealProcessor"];
                    player.Affinity = (string)JplayersInfos[i]["Processor"]["ProcessorAffinity"];
                    player.PriorityClass = (string)JplayersInfos[i]["Processor"]["ProcessorPriorityClass"];

                    if (player.IsXInput || player.IsDInput || player.IsSDL2)
                    {
                        gamepadCount++;
                    }
                    else
                    {
                        keyboardCount++;
                    }

                    ProfilePlayersList.Add(player);
                }

                JToken JaudioSettings = Jprofile["AudioSettings"] as JToken;

                foreach (JProperty JaudioSetting in JaudioSettings)
                {
                    if (JaudioSetting.Name.Contains("Custom"))
                    {
                        AudioCustomSettings = (bool)JaudioSetting.Value;
                    }
                    else
                    {
                        AudioDefaultSettings = (bool)JaudioSetting.Value;
                    }
                }

                AudioInstances.Clear();

                JToken JAudioInstances = Jprofile["AudioInstances"] as JToken;

                foreach (JProperty JaudioDevice in JAudioInstances)
                {
                    AudioInstances.Add((string)JaudioDevice.Name, (string)JaudioDevice.Value);
                }

                JToken JAllscreens = Jprofile["AllScreens"] as JToken; 

                for (int s = 0; s < JAllscreens.Count(); s++)
                {
                    AllScreens.Add(new RectangleF((float)JAllscreens[s]["X"], (float)JAllscreens[s]["Y"], (float)JAllscreens[s]["Width"], (float)JAllscreens[s]["Height"]));
                }

                totalProfilePlayers = JplayersInfos.Count();

                modeText = $"Profile n°{profileToSave}";

                Ready = true;

                GetGhostBounds();

                Globals.MainOSD.Show(1000, Title != "" ? $"Handler Profile \"{Title}\" Loaded" : $"Handler Profile N°{_profileToLoad} Loaded");

                return true;

            }
            catch (Exception ex)
            {
                Reset();
                NucleusMessageBox.Show("", "The profile can't be loaded.\n\n" +
                    "If the error persist delete the profile and create a new one.\n\n" +
                    "[Error]\n\n" + ex.Message, false);

                return false;
            }
        }

        public static void CreateShortcut(string gameGUID, string shortcutId, string profileId, string description)
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();

            string shortcutTitle = $@"\Profile_{shortcutId}_{gameGUID}.lnk";
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + shortcutTitle;
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = description != "" ? description : $"{gameGUID} Nucleus shortcut.";
            shortcut.TargetPath = Application.StartupPath + "\\Profiles Launcher.exe";
            shortcut.WorkingDirectory = Application.StartupPath;
            shortcut.Arguments = $"\"{gameGUID}\" \"{profileId}\"";
            UserGameInfo currentGameInfo = GameManager.Instance.User.Games.Where(c => c.GameGuid == gameGUID).FirstOrDefault();
            shortcut.IconLocation = currentGameInfo.ExePath;
            shortcut.Save();
        }

        private static string GetGameProfilesPath()
        {
            string path = $"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}";
            if (!Directory.Exists(path))
            {
                return null;
            }

            return path;
        }

        public static void UpdateGameProfile(GameProfile profile)
        {
            string path = $"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}\\Profile[{profileToSave}].json";

            JObject options = new JObject();
            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                if (opt.Value.GetType() == typeof(System.Dynamic.ExpandoObject))//Only used for options with pictures so far
                {
                    JObject values = new JObject();
                    System.Dynamic.ExpandoObject _vals = (System.Dynamic.ExpandoObject)opt.Value;

                    foreach (var t in _vals)
                    {
                        values.Add(new JProperty(t.Key, t.Value));
                    }

                    options.Add(new JProperty(opt.Key.ToString(), values));
                }
                else
                {
                    options.Add(new JProperty(opt.Key.ToString(), opt.Value.ToString()));
                }
            }

            JObject JHWndInterval = new JObject();

            if (hWndInterval > 0)
            {
                JHWndInterval.Add(new JProperty("Time", hWndInterval.ToString()));
            }
            else
            {
                JHWndInterval.Add(new JProperty("Time", "0"));
            }

            JObject JPauseBetweenInstanceLaunch = new JObject();

            if (pauseBetweenInstanceLaunch > 0)
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", pauseBetweenInstanceLaunch.ToString()));
            }
            else
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", "0"));
            }

            JObject JCustomLayout = new JObject(new JProperty("Ver", customLayout_Ver),
                                                new JProperty("Hor", customLayout_Hor),
                                                new JProperty("Max", CustomLayout_Max));

            JObject JUseSplitDiv = new JObject(new JProperty("Enabled", useSplitDiv),
                                               new JProperty("HideOnly", hideDesktopOnly),
                                               new JProperty("Color", splitDivColor));

            JObject JAutoDesktopScaling = new JObject(new JProperty("Enabled", autoDesktopScaling));
            JObject JUseNicknames = new JObject(new JProperty("Use", useNicknames));
            JObject JNetwork = new JObject(new JProperty("Type", network));
            JObject JAutoPlay = new JObject(new JProperty("Enabled", autoPlay));
            JObject JCts_Settings = new JObject(new JProperty("Cutscenes_KeepAspectRatio", cts_KeepAspectRatio),
                                                new JProperty("Cutscenes_MuteAudioOnly", cts_MuteAudioOnly),
                                                new JProperty("Cutscenes_Unfocus", cts_Unfocus));


            JObject JStop_UINav = new JObject(new JProperty("Enabled", stop_UINav));


            JObject JEnableMerger = new JObject(new JProperty("Enabled", enableWindowsMerger));
            JObject JEnableLosslessHook = new JObject(new JProperty("Enabled", enableLosslessHook));

            JObject JAudioInstances = new JObject();

            foreach (KeyValuePair<string, string> JaudioDevice in AudioInstances)
            {
                JAudioInstances.Add(new JProperty(JaudioDevice.Key, JaudioDevice.Value));
            }

            JObject JAudioSettings = new JObject(new JProperty("CustomSettings", audioCustomSettings), new JProperty("DefaultSettings", audioDefaultSettings));

            List<JObject> playersInfos = new List<JObject>();//Players object

            int gamepadCount = 0;
            int keyboardCount = 0;

            for (int i = 0; i < ProfilePlayersList.Count(); i++)//build per player object
            {
                PlayerInfo player = ProfilePlayersList[i];

                if (player.IsXInput || player.IsDInput || player.IsSDL2)
                {
                    gamepadCount++;
                }
                else
                {
                    keyboardCount++;
                }

                JObject JOwner = new JObject(
                                      new JProperty("Type", player.OwnerType),

                                       new JProperty("UiBounds", new JObject(
                                                               new JProperty("X", player.OwnerUIBounds.X),
                                                               new JProperty("Y", player.OwnerUIBounds.Y),
                                                               new JProperty("Width", player.OwnerUIBounds.Width),
                                                               new JProperty("Height", player.OwnerUIBounds.Height))),

                                      new JProperty("DisplayIndex", player.DisplayIndex),
                                      new JProperty("Display", new JObject(
                                                               new JProperty("X", player.OwnerDisplay.X),
                                                               new JProperty("Y", player.OwnerDisplay.Y),
                                                               new JProperty("Width", player.OwnerDisplay.Width),
                                                               new JProperty("Height", player.OwnerDisplay.Height))));

                JObject JMonitorBounds = new JObject(
                                             new JProperty("X", player.MonitorBounds.X),
                                             new JProperty("Y", player.MonitorBounds.Y),
                                             new JProperty("Width", player.MonitorBounds.Width),
                                             new JProperty("Height", player.MonitorBounds.Height));

                JObject JEditBounds = new JObject(
                                          new JProperty("X", player.EditBounds.X),
                                          new JProperty("Y", player.EditBounds.Y),
                                          new JProperty("Width", player.EditBounds.Width),
                                          new JProperty("Height", player.EditBounds.Height));

                JObject JProcessor = new JObject
                {
                    new JProperty("IdealProcessor", player.IdealProcessor),
                    new JProperty("ProcessorAffinity", player.Affinity),
                    new JProperty("ProcessorPriorityClass", player.PriorityClass)
                };

                JObject JPData = new JObject(//build all individual player datas object
                                 new JProperty("PlayerID", player.PlayerID),
                                 new JProperty("Nickname", player.Nickname),
                                 new JProperty("SteamID", player.SteamID),
                                 new JProperty("GamepadGuid", player.GamepadGuid),
                                 new JProperty("IsDInput", player.IsDInput),
                                 new JProperty("IsXInput", player.IsXInput),
                                 new JProperty("IsSDL2", player.IsSDL2),
                                 new JProperty("Processor", JProcessor),
                                 new JProperty("IsKeyboardPlayer", player.IsKeyboardPlayer),
                                 new JProperty("IsRawMouse", player.IsRawMouse),
                                 new JProperty("HIDDeviceID", player.HIDDeviceID),
                                 new JProperty("ScreenPriority", player.ScreenPriority),
                                 new JProperty("ScreenIndex", player.ScreenIndex),
                                 new JProperty("EditBounds", JEditBounds),
                                 new JProperty("MonitorBounds", JMonitorBounds),
                                 new JProperty("Owner", JOwner)
                    );

                playersInfos.Add(JPData);
            }

            List<JObject> JScreens = new List<JObject>();

            for (int s = 0; s < AllScreens.Count(); s++)
            {
                JObject JScreen = new JObject(new JProperty("X", AllScreens[s].X),
                                              new JProperty("Y", AllScreens[s].Y),
                                              new JProperty("Width", AllScreens[s].Width),
                                              new JProperty("Height", AllScreens[s].Height)
                                              );

                JScreens.Add(JScreen);
            }

            JObject profileJson = new JObject//shared settings object
            (
               new JProperty("Title", Title),
               new JProperty("Notes", Notes),
               new JProperty("Player(s)", ProfilePlayersList.Count),
               new JProperty("Controller(s)", gamepadCount),
               new JProperty("Use XInput Index", useXinputIndex),
               new JProperty("K&M", keyboardCount),
               new JProperty("AutoPlay", JAutoPlay),
               new JProperty("Data", playersInfos),
               new JProperty("Options", options),
               new JProperty("UseNicknames", JUseNicknames),
               new JProperty("AutoDesktopScaling", JAutoDesktopScaling),
               new JProperty("UseSplitDiv", JUseSplitDiv),
               new JProperty("CustomLayout", JCustomLayout),
               new JProperty("WindowsSetupTiming", JHWndInterval),
               new JProperty("PauseBetweenInstanceLaunch", JPauseBetweenInstanceLaunch),
               new JProperty("Network", JNetwork),
               new JProperty("AudioSettings", JAudioSettings),
               new JProperty("AudioInstances", JAudioInstances),
               new JProperty("CutscenesModeSettings", JCts_Settings),
               new JProperty("StopGPUINav", JStop_UINav),
               new JProperty("EnableWindowsMerger", JEnableMerger),
               new JProperty("MergerResolution", mergerResolution),
               new JProperty("EnableLosslessHook", JEnableLosslessHook),
               new JProperty("AllScreens", JScreens)

            );

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(profileJson, Formatting.Indented);
                    writer.Write(json);
                    stream.Flush();
                }
            }

            modeText = $"Profile n°{profileToSave}";

            updating = true;

            Globals.MainOSD.Show(1500, Title != "" ? $"Handler Profile \"{Title}\" Updated" : $"Handler Profile N°{profileToSave} Updated");
        }

        public static void SaveGameProfile(GameProfile profile)
        {
            string path;
            bool profileDisabled = App_Misc.DisableGameProfiles;

            if (profileDisabled || GameInfo.Game.MetaInfo.DisableProfiles || !GameInfo.Game.MetaInfo.SaveProfile)
            {
                saved = true;
                Globals.MainOSD.Show(1600, $"Setup Finished");
                return;
            }

            if (profile.DevicesList.Count != TotalProfilePlayers || !Loaded || profilesCount == 0)
            {
                profilesCount++;//increase to set new profile name
                path = $"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}\\Profile[{profilesCount}].json";
            }
            else
            {
                path = $"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}\\Profile[{profileToSave}].json";
            }

            if (!Directory.Exists(Path.Combine($"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}")))
            {
                Directory.CreateDirectory($"{Globals.GameProfilesFolder}\\{GameInfo.GameGuid}");
            }

            JObject options = new JObject();

            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                if (opt.Value.GetType() == typeof(System.Dynamic.ExpandoObject))//Only used for options with pictures so far
                {
                    JObject values = new JObject();
                    System.Dynamic.ExpandoObject _vals = (System.Dynamic.ExpandoObject)opt.Value;

                    foreach (var t in _vals)
                    {
                        values.Add(new JProperty(t.Key, t.Value));
                    }

                    options.Add(new JProperty(opt.Key.ToString(), values));
                }
                else
                {
                    options.Add(new JProperty(opt.Key.ToString(), opt.Value.ToString()));
                }
            }

            JObject JHWndInterval = new JObject();

            if (hWndInterval > 0)
            {
                JHWndInterval.Add(new JProperty("Time", hWndInterval.ToString()));
            }
            else
            {
                JHWndInterval.Add(new JProperty("Time", "0"));
            }

            JObject JPauseBetweenInstanceLaunch = new JObject();

            if (pauseBetweenInstanceLaunch > 0)
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", pauseBetweenInstanceLaunch.ToString()));
            }
            else
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", "0"));
            }

            JObject JCustomLayout = new JObject(new JProperty("Ver", customLayout_Ver),
                                                new JProperty("Hor", customLayout_Hor),
                                                new JProperty("Max", CustomLayout_Max));

            JObject JUseSplitDiv = new JObject(new JProperty("Enabled", useSplitDiv),
                                               new JProperty("HideOnly", hideDesktopOnly),
                                               new JProperty("Color", splitDivColor));

            JObject JAutoDesktopScaling = new JObject(new JProperty("Enabled", autoDesktopScaling));
            JObject JUseNicknames = new JObject(new JProperty("Use", useNicknames));
            JObject JNetwork = new JObject(new JProperty("Type", network));
            JObject JAutoPlay = new JObject(new JProperty("Enabled", autoPlay));

            JObject JAudioInstances = new JObject();

            JObject JCts_Settings = new JObject(new JProperty("Cutscenes_KeepAspectRatio", cts_KeepAspectRatio),
                                                new JProperty("Cutscenes_MuteAudioOnly", cts_MuteAudioOnly),
                                                new JProperty("Cutscenes_Unfocus", cts_Unfocus));

            JObject JStop_UINav = new JObject(new JProperty("Enabled", stop_UINav));

            JObject JEnableMerger = new JObject(new JProperty("Enabled", EnableWindowsMerger));
            JObject JEnableLosslessHook = new JObject(new JProperty("Enabled", EnableLosslessHook));

            foreach (KeyValuePair<string, string> JaudioDevice in AudioInstances)
            {
                JAudioInstances.Add(new JProperty(JaudioDevice.Key, JaudioDevice.Value));
            }

            JObject JAudioSettings = new JObject(new JProperty("CustomSettings", audioCustomSettings), new JProperty("DefaultSettings", audioDefaultSettings));

            List<PlayerInfo> players = (List<PlayerInfo>)profile.DevicesList.OrderBy(c => c.PlayerID).ToList();//need to do this because sometimes it's reversed
            List<JObject> playersInfos = new List<JObject>();//Players object

            int gamepadCount = 0;
            int keyboardCount = 0;

            for (int i = 0; i < players.Count(); i++)//build per players object
            {
                if (players[i].IsXInput || players[i].IsDInput || players[i].IsSDL2)
                {
                    gamepadCount++;
                }
                else
                {
                    keyboardCount++;
                }

                JObject JOwner = new JObject(
                                   new JProperty("Type", players[i].Owner.Type),

                                   new JProperty("UiBounds", new JObject(
                                                           new JProperty("X", players[i].Owner.UIBounds.X),
                                                           new JProperty("Y", players[i].Owner.UIBounds.Y),
                                                           new JProperty("Width", players[i].Owner.UIBounds.Width),
                                                           new JProperty("Height", players[i].Owner.UIBounds.Height))),
                                   new JProperty("DisplayIndex", players[i].Owner.DisplayIndex),
                                   new JProperty("Display", new JObject(
                                                           new JProperty("X", players[i].Owner.MonitorBounds.X),
                                                           new JProperty("Y", players[i].Owner.MonitorBounds.Y),
                                                           new JProperty("Width", players[i].Owner.MonitorBounds.Width),
                                                           new JProperty("Height", players[i].Owner.MonitorBounds.Height))));


                JObject JMonitorBounds = new JObject(
                                         new JProperty("X", players[i].DefaultMonitorBounds.X),
                                         new JProperty("Y", players[i].DefaultMonitorBounds.Y),
                                         new JProperty("Width", players[i].DefaultMonitorBounds.Width),
                                         new JProperty("Height", players[i].DefaultMonitorBounds.Height));

                JObject JEditBounds = new JObject(
                                      new JProperty("X", players[i].EditBounds.X),
                                      new JProperty("Y", players[i].EditBounds.Y),
                                      new JProperty("Width", players[i].EditBounds.Width),
                                      new JProperty("Height", players[i].EditBounds.Height));

                JObject JProcessor = new JObject
                {
                    new JProperty("IdealProcessor", players[i].IdealProcessor),
                    new JProperty("ProcessorAffinity", players[i].Affinity),
                    new JProperty("ProcessorPriorityClass", players[i].PriorityClass)
                };

                JObject JPData = new JObject(//build all individual player datas object
                                 new JProperty("PlayerID", players[i].PlayerID),
                                 new JProperty("Nickname", players[i].Nickname),
                                 new JProperty("SteamID", players[i].SteamID),
                                 new JProperty("GamepadGuid", players[i].GamepadGuid),
                                 new JProperty("IsDInput", players[i].IsDInput),
                                 new JProperty("IsXInput", players[i].IsXInput),
                                 new JProperty("IsSDL2", players[i].IsSDL2),
                                 new JProperty("Processor", JProcessor),
                                 new JProperty("IsKeyboardPlayer", players[i].IsKeyboardPlayer),
                                 new JProperty("IsRawMouse", players[i].IsRawMouse),
                                 new JProperty("HIDDeviceID", players[i].HIDDeviceID),
                                 new JProperty("ScreenPriority", players[i].ScreenPriority),
                                 new JProperty("ScreenIndex", players[i].ScreenIndex),
                                 new JProperty("EditBounds", JEditBounds),
                                 new JProperty("MonitorBounds", JMonitorBounds),
                                 new JProperty("Owner", JOwner)
                                 );

                playersInfos.Add(JPData);
            }

            List<JObject> JScreens = new List<JObject>();

            var screens = BoundsFunctions.Screens;

            for (int s = 0; s < BoundsFunctions.Screens.Count(); s++)
            {
                UserScreen screen = screens[s];


                JObject JScreen = new JObject(new JProperty("X", screen.UIBounds.X),
                                              new JProperty("Y", screen.UIBounds.Y),
                                              new JProperty("Width", screen.UIBounds.Width),
                                              new JProperty("Height", screen.UIBounds.Height)
                                             );

                JScreens.Add(JScreen);
            }

            JObject profileJson = new JObject//shared settings object
            (
               new JProperty("Title", Title),
               new JProperty("Notes", Notes),
               new JProperty("Player(s)", profile.DevicesList.Count),
               new JProperty("Controller(s)", gamepadCount),
               new JProperty("K&M", keyboardCount),
               new JProperty("Use XInput Index", useXinputIndex),
               new JProperty("AutoPlay", JAutoPlay),
               new JProperty("Data", playersInfos),
               new JProperty("Options", options),
               new JProperty("UseNicknames", JUseNicknames),
               new JProperty("AutoDesktopScaling", JAutoDesktopScaling),
               new JProperty("UseSplitDiv", JUseSplitDiv),
               new JProperty("CustomLayout", JCustomLayout),
               new JProperty("WindowsSetupTiming", JHWndInterval),
               new JProperty("PauseBetweenInstanceLaunch", JPauseBetweenInstanceLaunch),
               new JProperty("Network", JNetwork),
               new JProperty("AudioSettings", JAudioSettings),
               new JProperty("AudioInstances", JAudioInstances),
               new JProperty("CutscenesModeSettings", JCts_Settings),
               new JProperty("StopGPUINav", JStop_UINav),
               new JProperty("EnableWindowsMerger", JEnableMerger),
               new JProperty("MergerResolution", mergerResolution),
               new JProperty("EnableLosslessHook", JEnableLosslessHook),
               new JProperty("AllScreens", JScreens)

            );

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(profileJson, Formatting.Indented);
                    writer.Write(json);
                    stream.Flush();
                }
            }

            saved = true;

            LogManager.Log("Handler Profile Saved");

            Globals.MainOSD.Show(1600, $"Handler Profile Saved. Setup Finished");
        }

        public static void FindProfilePlayers(PlayerInfo player)
        {
            if (AssignedDevices.Contains(player))
            {
                return;
            }

            if (showError)
            {
                Instance.Reset();
                return;
            }

            if (Loaded)
            {
                if (TotalAssignedPlayers < 0 && !showError)
                {
                    showError = true;
                    NucleusMessageBox.Show("error", "Oops!\nSomething went wrong, profile has been unloaded.", false);
                    return;
                }
            }

            if (TotalAssignedPlayers < TotalProfilePlayers)
            {
                GroupKeyboardAndMouse();

                PlayerInfo profilePlayer = FindProfilePlayerInput(player);

                if (profilePlayer == null)
                {
                    return;
                }

                //if the screen is not present look for an other one
                var scr = FindScreenOrAlternative(profilePlayer);

                //if the profile requires more screens than available
                if (ProfilePlayersList.Any(pp => pp.ScreenIndex != profilePlayer.ScreenIndex) && scr.Item2 != profilePlayer.ScreenIndex)
                {
                    Instance.Reset();
                    Globals.MainOSD.Show(2000, $"Not Enough Active Screens");
                    scr.Item1.Type = UserScreenType.FullScreen;
                    return;
                }

                if (Instance.DevicesList.All(lpp => lpp.MonitorBounds != TranslateBounds(profilePlayer, scr.Item1).Item1) &&
                    ProfilePlayersList.FindIndex(pp => pp == profilePlayer) == AssignedDevices.Count)//avoid to add player in the same bounds                                                                                                                           // ProfilePlayersList.FindIndex(pp => pp == profilePlayer) == AssignedDevices.Count)//make sure to insert player like saved in the game profile
                {
                    SetProfilePlayerDatas(player, profilePlayer, scr.Item1, scr.Item2);
                    AssignedDevices.Add(player);

                    scr.Item1.PlayerOnScreen++;
                    TotalAssignedPlayers++;

                    SetupScreenControl.Instance?.Invalidate();

                    if (TotalAssignedPlayers == TotalProfilePlayers && Ready && AutoPlay && !Updating)
                    {
                        SetupScreenControl.Instance?.CanPlayUpdated(true, true);
                        Globals.PlayButton.PerformClick();
                        Ready = false;
                        return;
                    }
                }
            }

            if (TotalAssignedPlayers == TotalProfilePlayers)
            {
                if (Ready && (!AutoPlay || Updating))
                {
                    SetupScreenControl.Instance?.CanPlayUpdated(true, true);
                    Updating = false;
                    return;
                }
            }
        }

        private void GetGhostBounds()
        {
            foreach (PlayerInfo pp in ProfilePlayersList)
            {
                var scr = FindScreenOrAlternative(pp);
                var ghostBounds = TranslateBounds(pp, scr.Item1);

                GhostBounds.Add(ghostBounds);
            }
        }

        private static void SetProfilePlayerDatas(PlayerInfo player, PlayerInfo profilePlayer, UserScreen screen, int screenIndex)
        {
            player.Owner = screen;
            player.Owner.Type = screen.Type;

            player.ScreenPriority = screen.priority;
            player.DisplayIndex = screen.DisplayIndex;

            var translatedBounds = TranslateBounds(profilePlayer, screen);

            player.EditBounds = translatedBounds.Item2;

            player.MonitorBounds = translatedBounds.Item1;

            player.ScreenIndex = screenIndex;
            player.Nickname = profilePlayer.Nickname;
            player.PlayerID = profilePlayer.PlayerID;
            player.SteamID = profilePlayer.SteamID;
            player.IsInputUsed = true;
        }

        private static PlayerInfo FindProfilePlayerInput(PlayerInfo player)
        {
            PlayerInfo profilePlayer = null;

            var screens = Instance.screens;

            bool skipGuid = false;

            //DInput && XInput (follow gamepad api indexes)
            if (player.IsController && useXinputIndex)
            {
                foreach (PlayerInfo pp in ProfilePlayersList)
                {
                    if (AssignedDevices.All(lp => lp.PlayerID != pp.PlayerID) && (pp.IsDInput || pp.IsXInput || pp.IsSDL2))
                    {
                        profilePlayer = pp;
                        skipGuid = true;
                        break;
                    }
                }
            }

            //DInput && XInput using GamepadGuid(do not follow gamepad api indexes)
            if (player.IsController && !skipGuid && !useXinputIndex)
            {
                profilePlayer = ProfilePlayersList.Where(pl => (pl.IsDInput || pl.IsXInput || pl.IsSDL2) && (pl.GamepadGuid == player.GamepadGuid)).FirstOrDefault();
            }

            //single k&m 
            if (player.GamepadGuid.ToString() == "10000000-1000-1000-1000-100000000000")
            {
                profilePlayer = ProfilePlayersList.Where(pl => pl.GamepadGuid.ToString() == "10000000-1000-1000-1000-100000000000").FirstOrDefault();
            }

            bool isKm = player.IsRawKeyboard && player.IsRawMouse;

            //Merged raw keyboard and raw mouse player
            if (isKm)
            {
                profilePlayer = ProfilePlayersList.Where(pl => isKm &&
                (pl.HIDDeviceID.Contains(player.HIDDeviceID[0]) && pl.HIDDeviceID.Contains(player.HIDDeviceID[1]))).FirstOrDefault();
            }

            for (int i = 0; i < screens.Count(); i++)
            {
                UserScreen anyScr = screens[i];

                if (ProfilePlayersList.Any(pp => pp.ScreenIndex == i))
                {
                    anyScr.Type = (UserScreenType)ProfilePlayersList.Where(pp => pp.ScreenIndex == i).FirstOrDefault().OwnerType;
                }
            }

            return profilePlayer;
        }

        private static void GroupKeyboardAndMouse()
        {
            for (int i = 0; i < ProfilePlayersList.Count(); i++)
            {
                List<PlayerInfo> groupedPlayers = new List<PlayerInfo>();

                PlayerInfo kbPlayer = ProfilePlayersList[i];

                for (int pl = 0; pl < Instance.DevicesList.Count; pl++)
                {
                    PlayerInfo p = Instance.DevicesList[pl];

                    if (p.IsRawKeyboard || p.IsRawMouse)
                    {
                        if (kbPlayer.HIDDeviceID.Any(hid => hid == p.HIDDeviceID[0]))
                        {
                            groupedPlayers.Add(p);
                        }
                    }

                    if (groupedPlayers.Count == 2)
                    {
                        var firstInGroup = groupedPlayers.First();
                        var secondInGroup = groupedPlayers.Last();

                        firstInGroup.IsRawKeyboard = groupedPlayers.Count(x => x.IsRawKeyboard) > 0;
                        firstInGroup.IsRawMouse = groupedPlayers.Count(x => x.IsRawMouse) > 0;

                        if (firstInGroup.IsRawKeyboard) firstInGroup.RawKeyboardDeviceHandle = groupedPlayers.First(x => x.RawKeyboardDeviceHandle != (IntPtr)(-1)).RawKeyboardDeviceHandle;
                        if (firstInGroup.IsRawMouse) firstInGroup.RawMouseDeviceHandle = groupedPlayers.First(x => x.RawMouseDeviceHandle != (IntPtr)(-1)).RawMouseDeviceHandle;

                        firstInGroup.HIDDeviceID = new string[2] { firstInGroup.HIDDeviceID[0], secondInGroup.HIDDeviceID[0] };

                        Instance.DevicesList.Remove(secondInGroup);
                        p = firstInGroup;
                        p.IsInputUsed = true;///needed else device is invisible and can't be moved

                        break;
                    }
                }
            }
        }

        private void RefreshKeyboardAndMouse()
        {
            DevicesList.RemoveAll(p => p.IsRawMouse || p.IsRawKeyboard);

            if (Game.SupportsMultipleKeyboardsAndMice)///Raw mice/keyboards
            {
                DevicesList.AddRange(RawInputManager.GetDeviceInputInfos());
            }

            for (int i = 0; i < DevicesList.Count(); i++)
            {
                DevicesList[i].EditBounds = BoundsFunctions.GetDefaultBounds(i);
            }
        }

        internal static (UserScreen, int) FindScreenOrAlternative(PlayerInfo profilePlayer)
        {
            UserScreen screen = BoundsFunctions.Screens.ElementAtOrDefault(profilePlayer.ScreenIndex);
            int ogIndex = profilePlayer.ScreenIndex;

            while (screen == null)
            {
                --ogIndex;
                screen = BoundsFunctions.Screens.ElementAtOrDefault(ogIndex);
            }

            screen.Type = (UserScreenType)profilePlayer.OwnerType;

            //BoundsFunctions.GetScreenDivisionBounds(screen);
            return (screen, ogIndex);
        }

        internal static (Rectangle, RectangleF) TranslateBounds(PlayerInfo profilePlayer, UserScreen screen)
        {
            RectangleF ogScrUiBounds = AllScreens[profilePlayer.ScreenIndex];
            RectangleF ogEditBounds = profilePlayer.EditBounds;

            Vector2 ogScruiLoc = new Vector2(ogScrUiBounds.X, ogScrUiBounds.Y);//original screen ui location
            Vector2 ogpEb = new Vector2(ogEditBounds.X, ogEditBounds.Y);//original on ui screen player location(editbounds)
            Vector2 ogOnUIScrLoc = Vector2.Subtract(ogpEb, ogScruiLoc);//relative og ui player loc on og player ui screen

            float ratioEW = (float)ogScrUiBounds.Width / (float)screen.UIBounds.Width;
            float ratioEH = (float)ogScrUiBounds.Height / (float)screen.UIBounds.Height;

            RectangleF translatedEditBounds = new RectangleF(screen.UIBounds.X + (ogOnUIScrLoc.X / ratioEW), screen.UIBounds.Y + (ogOnUIScrLoc.Y / ratioEH), ogEditBounds.Width / ratioEW, ogEditBounds.Height / ratioEH);

            ///## Re-calcul & scale player monitor bounds if needed ##///
            Rectangle ogScr = profilePlayer.OwnerDisplay;
            Rectangle ogMb = profilePlayer.MonitorBounds;

            Vector2 ogscr = new Vector2(ogScr.X, ogScr.Y);//original player screen location
            Vector2 ogPMb = new Vector2(ogMb.X, ogMb.Y);//original on screen player location(monitorBounds)
            Vector2 VogOnScrLoc = Vector2.Subtract(ogPMb, ogscr);//relative og player loc on og player screen

            float ratioMW = (float)ogScr.Width / (float)screen.MonitorBounds.Width;
            float ratioMH = (float)ogScr.Height / (float)screen.MonitorBounds.Height;

            Rectangle translatedMonitorBounds = new Rectangle(screen.MonitorBounds.X + (Convert.ToInt32(VogOnScrLoc.X / ratioMW)), screen.MonitorBounds.Y + (Convert.ToInt32(VogOnScrLoc.Y / ratioMH)), Convert.ToInt32(ogMb.Width / ratioMW), Convert.ToInt32(ogMb.Height / ratioMH));

            return (translatedMonitorBounds, translatedEditBounds);
        }

        public static GameProfile CleanClone(GameProfile profile)
        {
            AssignedDevices.AddRange(DevicesToMerge);

            GameProfile nprof = new GameProfile
            {
                deviceList = AssignedDevices,
                screens = profile.screens,
            };

            //Clear profile screens subBounds
            nprof.screens.AsParallel().ForAll(i => i.SubScreensBounds.Clear());

            foreach (PlayerInfo player in nprof.DevicesList)
            {
                player.DefaultMonitorBounds = player.MonitorBounds;

                if (UseSplitDiv && !HideDesktopOnly)
                {
                    player.MonitorBounds = new Rectangle(player.MonitorBounds.X + 1, player.MonitorBounds.Y + 1, player.MonitorBounds.Width - 2, player.MonitorBounds.Height - 2);
                }

                //Insert only players bounds in screens sub bounds
                foreach (UserScreen screen in nprof.screens)
                {
                    if (screen.Index == player.ScreenIndex)
                    {
                        if (!screen.SubScreensBounds.Keys.Contains(player.MonitorBounds))
                        {
                            screen.SubScreensBounds.Add(player.MonitorBounds, player.EditBounds);
                        }
                    }
                }
            }

            //Remove any screens not containing players based on the above cleanup 
            nprof.Screens.RemoveAll(s => s.SubScreensBounds.Count() == 0);

            Dictionary<string, object> noptions = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                noptions.Add(opt.Key, opt.Value);
            }

            nprof.options = noptions;

            return nprof;
        }

        public static void UpdateProfilePlayerIdentity(PlayerInfo player)
        {
            PlayerInfo secondInBounds = DevicesToMerge.Where(pl => pl.EditBounds == player.EditBounds && pl != player).FirstOrDefault();

            int playerIndex = AssignedDevices.FindIndex(pl => pl == player);

            bool getIdFromProfile = ProfilePlayersList.Count() > 0 && ProfilePlayersList.Count() > playerIndex && !GameInfo.Game.MetaInfo.DisableProfiles;

            player.Nickname = getIdFromProfile ? ProfilePlayersList[playerIndex].Nickname :
                             PlayersIdentityCache.GetNicknameAt(playerIndex);


            string steamID = string.Empty;

            if (getIdFromProfile)//get profile value
            {
                if (Game.UseHandlerSteamIds)//get custom handler value anyway if Game.UseHandlerSteamIds 
                {                
                    steamID = Game.PlayerSteamIDs[playerIndex];
                }
                else
                {
                    steamID = ProfilePlayersList[playerIndex].SteamID.ToString();
                }
            }
            else if(Game.UseHandlerSteamIds)//get custom handler value if Game.UseHandlerSteamIds
            {
                steamID = Game.PlayerSteamIDs[playerIndex];
            }
            else if (PlayersIdentityCache.GetSteamIdAt(playerIndex) != "")//get custom user (ini) value
            {
                steamID = PlayersIdentityCache.GetSteamIdAt(playerIndex);
            }

            //Console.WriteLine(steamID + " " + player.Nickname + "\n");

            player.SteamID = long.Parse(steamID);

            if (secondInBounds != null)
            {
                secondInBounds.Nickname = player.Nickname;
                secondInBounds.SteamID = player.SteamID;
            }
        }

        public static void GenMissingIdFromPlayerSteamIDs()
        {
            string lastIdOfArray = Game.PlayerSteamIDs.Last();
            List<string> newArray = Game.PlayerSteamIDs.ToList();

            while (newArray.Count() < Globals.NucleusMaxPlayers)
            {
                long convToLong;

                if (newArray.Count == 0)
                {
                    convToLong = long.Parse(lastIdOfArray);
                }
                else
                {
                    convToLong = long.Parse(newArray.Last());
                }

                convToLong++;

                newArray.Add(convToLong.ToString());
            }

            Game.PlayerSteamIDs = newArray.ToArray();
        }

    }
}
