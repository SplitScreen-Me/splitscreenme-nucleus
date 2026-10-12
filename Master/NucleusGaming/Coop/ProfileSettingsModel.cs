using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// Settings of the loaded profile, or of the profile that will be saved at launch, edited as one object.
    /// The reading, validation and apply logic come from the WinForms ProfileSettings form (UpdateUiValues and
    /// CloseBtnPicture_Click), so the form and the web UI can share them. Values live in GameProfile statics.
    /// </summary>
    public sealed class ProfileSettingsModel
    {
        public static readonly string[] PriorityClasses = { "Normal", "AboveNormal", "High", "RealTime" };

        /// <summary>Number of per-player audio outputs (AudioInstance1..8).</summary>
        public const int AudioInstanceCount = 8;

        public bool IsNew;
        public string Title = "";
        public string Notes = "";
        public bool AutoPlay;
        public bool UseNicknames;
        public bool AutoDesktopScaling;
        public bool SplitDiv;
        public bool SplitDivHideDesktopOnly;
        public string SplitDivColor = "Black";
        public int CustomLayoutVer;
        public int CustomLayoutHor;
        public int CustomLayoutMax;
        public int WindowsSetupTimingMs;
        public int PauseBetweenInstanceLaunchSec;
        public string Network = "Automatic";
        public bool AudioCustom;
        public bool CutscenesKeepAspectRatio;
        public bool CutscenesMuteAudioOnly;
        public bool CutscenesUnfocus;

        /// <summary>"StopGPUINav" of the legacy gamepad-as-mouse feature (removed): no effect, kept so profile files round-trip.</summary>
        public bool StopUINav;
        public bool WindowsMerger;
        public bool LosslessHook;
        public string MergerResolution = "";

        /// <summary>Per-player settings, in player order.</summary>
        public List<ProfileSlotModel> Slots = new List<ProfileSlotModel>();

        /// <summary>"AudioInstance1".."AudioInstance8" to an audio endpoint id, or "Default".</summary>
        public Dictionary<string, string> AudioInstances = new Dictionary<string, string>();

        /// <summary>Reads the current values (what the form shows when it opens). Needs GameProfile.Game.</summary>
        public static ProfileSettingsModel FromCurrent()
        {
            ProfileSettingsModel model = new ProfileSettingsModel
            {
                IsNew = GameProfile.IsNewProfile,
                Title = GameProfile.Title ?? "",
                Notes = GameProfile.Notes ?? "",
                AutoPlay = GameProfile.AutoPlay,
                UseNicknames = GameProfile.UseNicknames,
                AutoDesktopScaling = GameProfile.AutoDesktopScaling,
                SplitDiv = GameProfile.UseSplitDiv,
                SplitDivHideDesktopOnly = GameProfile.HideDesktopOnly,
                SplitDivColor = GameProfile.SplitDivColor ?? "Black",
                CustomLayoutVer = GameProfile.CustomLayout_Ver,
                CustomLayoutHor = GameProfile.CustomLayout_Hor,
                CustomLayoutMax = GameProfile.CustomLayout_Max,
                WindowsSetupTimingMs = GameProfile.HWndInterval,
                PauseBetweenInstanceLaunchSec = GameProfile.PauseBetweenInstanceLaunch,
                Network = string.IsNullOrEmpty(GameProfile.Network) ? "Automatic" : GameProfile.Network,
                AudioCustom = GameProfile.AudioCustomSettings,
                CutscenesKeepAspectRatio = GameProfile.Cts_KeepAspectRatio,
                CutscenesMuteAudioOnly = GameProfile.Cts_MuteAudioOnly,
                CutscenesUnfocus = GameProfile.Cts_Unfocus,
                StopUINav = GameProfile.Stop_UINav,
                WindowsMerger = GameProfile.EnableWindowsMerger,
                LosslessHook = GameProfile.EnableLosslessHook,
                MergerResolution = string.IsNullOrEmpty(GameProfile.MergerResolution) ? PrimaryScreenResolution() : GameProfile.MergerResolution,
            };

            List<PlayerInfo> profilePlayers = GameProfile.ProfilePlayersList;
            string[] handlerSteamIds = GameProfile.Game?.UseHandlerSteamIds == true ? GameProfile.Game.PlayerSteamIDs : null;

            for (int i = 0; i < SlotCount(); i++)
            {
                ProfileSlotModel slot = new ProfileSlotModel();

                if (i < profilePlayers.Count)
                {
                    PlayerInfo player = profilePlayers[i];

                    slot.Nickname = player.Nickname ?? "";
                    slot.SteamId = player.SteamID.ToString() != "-1" ? player.SteamID.ToString() : "";
                    slot.IdealProcessor = player.IdealProcessor ?? "*";
                    slot.Affinity = player.Affinity ?? "";
                    slot.PriorityClass = player.PriorityClass ?? "Normal";
                }
                else//new profile: the slots that will be created at launch
                {
                    slot.Nickname = PlayersIdentityCache.GetNicknameAt(i) ?? "";
                    slot.SteamId = PlayersIdentityCache.GetSteamIdAt(i);
                }

                //UseHandlerSteamIds forces Game.PlayerSteamIDs
                if (handlerSteamIds != null && i < handlerSteamIds.Length)
                {
                    slot.SteamId = handlerSteamIds[i];
                    slot.SteamIdLocked = true;
                }

                model.Slots.Add(slot);
            }

            for (int i = 0; i < AudioInstanceCount; i++)
            {
                string key = "AudioInstance" + (i + 1);
                string value = null;

                if (GameProfile.AudioInstances.TryGetValue(key, out string profileValue) && !string.IsNullOrEmpty(profileValue))
                {
                    value = profileValue;
                }
                else if (GameProfile.IsNewProfile && App_Audio.Instances_AudioOutput.TryGetValue(key, out string appValue) && !string.IsNullOrEmpty(appValue))
                {
                    value = appValue;
                }

                model.AudioInstances[key] = value ?? "Default";
            }

            return model;
        }

        /// <summary>
        /// Number of per-player slots: the players of the loaded profile, or for a new profile the players already
        /// prepared, else the handler's maximum. At most Globals.NucleusMaxPlayers.
        /// </summary>
        public static int SlotCount()
        {
            int count = GameProfile.ProfilePlayersList.Count;

            if (count == 0 && GameProfile.IsNewProfile)
            {
                count = GameProfile.Game?.MaxPlayers ?? 0;
            }

            return Math.Min(count, Globals.NucleusMaxPlayers);
        }

        private static string PrimaryScreenResolution()
        {
            Screen primary = Screen.PrimaryScreen;
            return primary == null ? "" : $"{primary.Bounds.Width}X{primary.Bounds.Height}";
        }

        /// <summary>The invalid fields, with the same rules as the WinForms form. Field names are the web contract's (camelCase).</summary>
        public List<ProfileSettingsError> Validate()
        {
            List<ProfileSettingsError> errors = new List<ProfileSettingsError>();
            int cores = Environment.ProcessorCount;

            for (int i = 0; i < Slots.Count && i < Globals.NucleusMaxPlayers; i++)
            {
                ProfileSlotModel slot = Slots[i];

                if (string.IsNullOrWhiteSpace(slot.Nickname))
                {
                    errors.Add(new ProfileSettingsError("slots.nickname", i, "Nicknames can't be empty."));
                }

                string sid = slot.SteamId ?? "";

                if (!((Regex.IsMatch(sid, "^[0-9]+$") && sid.Length == 17) || sid.Length == 0 || sid == "-1"))
                {
                    errors.Add(new ProfileSettingsError("slots.steamId", i, "A Steam ID must be 17 digits, for example 76561199075562883."));
                }
                else if (sid != "" && Slots.Where((other, j) => j != i && (other.SteamId ?? "") == sid).Any())
                {
                    errors.Add(new ProfileSettingsError("slots.steamId", i, "Each player needs a different Steam ID."));
                }

                string ideal = string.IsNullOrEmpty(slot.IdealProcessor) ? "*" : slot.IdealProcessor;

                if (ideal != "*" && (!int.TryParse(ideal, out int idealValue) || idealValue < 0 || idealValue > cores))
                {
                    errors.Add(new ProfileSettingsError("slots.idealProcessor", i, $"This PC has {cores} cores: use * or a number up to {cores}."));
                }

                foreach (string value in (slot.Affinity ?? "").Split(','))
                {
                    if (value == "")
                    {
                        continue;
                    }

                    if (!int.TryParse(value.Trim(), out int core) || core < 0 || core > cores)
                    {
                        errors.Add(new ProfileSettingsError("slots.affinity", i, $"This PC has {cores} cores: use numbers up to {cores}, separated by commas."));
                        break;
                    }
                }

                string priority = string.IsNullOrEmpty(slot.PriorityClass) ? "Normal" : slot.PriorityClass;

                if (!PriorityClasses.Contains(priority))
                {
                    errors.Add(new ProfileSettingsError("slots.priorityClass", i, "The priority must be Normal, AboveNormal, High or RealTime."));
                }
            }

            if (CustomLayoutHor < 0 || CustomLayoutHor > 5)
            {
                errors.Add(new ProfileSettingsError("customLayoutHorizontalLines", null, "Use 0 to 5 horizontal lines."));
            }

            if (CustomLayoutVer < 0 || CustomLayoutVer > 5)
            {
                errors.Add(new ProfileSettingsError("customLayoutVerticalLines", null, "Use 0 to 5 vertical lines."));
            }

            if (CustomLayoutMax < 1)
            {
                errors.Add(new ProfileSettingsError("customLayoutMaxPlayers", null, "The custom layout needs at least 1 player."));
            }

            if (WindowsSetupTimingMs < 0)
            {
                errors.Add(new ProfileSettingsError("windowSetupDelayMs", null, "The delay can't be negative."));
            }

            if (PauseBetweenInstanceLaunchSec < 0)
            {
                errors.Add(new ProfileSettingsError("pauseBetweenInstancesSec", null, "The pause can't be negative."));
            }

            if (!string.IsNullOrEmpty(SplitDivColor) && !BackgroundColors.ColorsDictionnary.ContainsKey(SplitDivColor))
            {
                errors.Add(new ProfileSettingsError("splitDividerColor", null, "Unknown colour."));
            }

            if (!string.IsNullOrEmpty(MergerResolution) && !Regex.IsMatch(MergerResolution, "^[0-9]+X[0-9]+$"))
            {
                errors.Add(new ProfileSettingsError("mergerResolution", null, "The resolution must look like 1920X1080."));
            }

            return errors;
        }

        /// <summary>
        /// Validates, then applies the values to the GameProfile statics and the profile players. A saved profile is
        /// rewritten and reloaded (as the form does); a new profile keeps the values until it is saved at launch.
        /// Nothing changes when errors are returned.
        /// </summary>
        public List<ProfileSettingsError> Apply()
        {
            List<ProfileSettingsError> errors = Validate();

            if (errors.Count > 0)
            {
                return errors;
            }

            //the nickname/Steam ID history files live there
            if (!System.IO.Directory.Exists(Globals.GameProfilesFolder))
            {
                System.IO.Directory.CreateDirectory(Globals.GameProfilesFolder);
            }

            if (GameProfile.ProfilePlayersList.Count == 0)
            {
                //Create profile players for new game profile
                for (int i = 0; i < GameProfile.Game.MaxPlayers; i++)
                {
                    GameProfile.ProfilePlayersList.Add(new PlayerInfo());
                }
            }

            string[] handlerSteamIds = GameProfile.Game?.PlayerSteamIDs;

            for (int i = 0; i < GameProfile.ProfilePlayersList.Count && i < Slots.Count && i < Globals.NucleusMaxPlayers; i++)
            {
                PlayerInfo player = GameProfile.ProfilePlayersList[i];
                ProfileSlotModel slot = Slots[i];

                player.Nickname = slot.Nickname;
                PlayersIdentityCache.AddNicknameToCache(slot.Nickname);

                string sid = slot.SteamId ?? "";

                if (sid != "")
                {
                    player.SteamID = long.Parse(sid);

                    if (handlerSteamIds == null || handlerSteamIds.All(handlerSid => handlerSid != sid))
                    {
                        PlayersIdentityCache.AddSteamIdToCache(sid);
                    }
                }
                else
                {
                    player.SteamID = PlayersIdentityCache.DefaultSteamIds[i];
                }

                player.IdealProcessor = string.IsNullOrEmpty(slot.IdealProcessor) ? "*" : slot.IdealProcessor;
                player.Affinity = slot.Affinity ?? "";
                player.PriorityClass = string.IsNullOrEmpty(slot.PriorityClass) ? "Normal" : slot.PriorityClass;
            }

            PlayersIdentityCache.BackupNicknames();
            PlayersIdentityCache.BackupSteamIds();

            GameProfile.Title = Title ?? "";
            GameProfile.Notes = Notes ?? "";
            GameProfile.AutoPlay = AutoPlay;
            GameProfile.AudioDefaultSettings = !AudioCustom;
            GameProfile.AudioCustomSettings = AudioCustom;
            GameProfile.Network = string.IsNullOrEmpty(Network) ? "Automatic" : Network;
            GameProfile.CustomLayout_Ver = CustomLayoutVer;
            GameProfile.CustomLayout_Hor = CustomLayoutHor;
            GameProfile.CustomLayout_Max = CustomLayoutMax;
            GameProfile.UseNicknames = UseNicknames;
            GameProfile.UseSplitDiv = SplitDiv;
            GameProfile.HideDesktopOnly = SplitDivHideDesktopOnly;
            GameProfile.SplitDivColor = SplitDivColor;
            GameProfile.AutoDesktopScaling = AutoDesktopScaling;
            GameProfile.PauseBetweenInstanceLaunch = PauseBetweenInstanceLaunchSec;
            GameProfile.HWndInterval = WindowsSetupTimingMs;
            GameProfile.Cts_MuteAudioOnly = CutscenesMuteAudioOnly;
            GameProfile.Cts_KeepAspectRatio = CutscenesKeepAspectRatio;
            GameProfile.Cts_Unfocus = CutscenesUnfocus;
            GameProfile.EnableWindowsMerger = WindowsMerger;
            GameProfile.EnableLosslessHook = LosslessHook;
            GameProfile.MergerResolution = MergerResolution ?? "";
            GameProfile.Stop_UINav = StopUINav;

            GameProfile.AudioInstances.Clear();

            foreach (KeyValuePair<string, string> audio in AudioInstances)
            {
                if (!string.IsNullOrEmpty(audio.Value))
                {
                    GameProfile.AudioInstances.Add(audio.Key, audio.Value);
                }
            }

            ///Update profile[#].json file, then reload it
            if (!GameProfile.IsNewProfile)
            {
                GameProfile.UpdateGameProfile(GameProfile.Instance);
                GameProfile.Instance.LoadGameProfile(GameProfile.CurrentProfileId);
            }

            return errors;
        }
    }

    /// <summary>Per-player settings of a profile.</summary>
    public sealed class ProfileSlotModel
    {
        public string Nickname = "";
        public string SteamId = "";
        public bool SteamIdLocked;
        public string IdealProcessor = "*";
        public string Affinity = "";
        public string PriorityClass = "Normal";
    }

    /// <summary>An invalid profile setting.</summary>
    public sealed class ProfileSettingsError
    {
        public ProfileSettingsError(string field, int? index, string message)
        {
            Field = field;
            Index = index;
            Message = message;
        }

        /// <summary>Field name (camelCase, "slots.nickname" for a per-player field).</summary>
        public string Field { get; }

        /// <summary>Slot index for per-player fields.</summary>
        public int? Index { get; }

        public string Message { get; }
    }
}
