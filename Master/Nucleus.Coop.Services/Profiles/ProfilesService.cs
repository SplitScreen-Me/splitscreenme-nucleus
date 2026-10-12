using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Tools.MonitorsDpiScaling;
using Nucleus.Gaming.UI;
using ProfileSettings = Nucleus.Bridge.Contracts.ProfileSettings;

namespace Nucleus.Coop.Services.Profiles
{
    /// <summary>
    /// Saved setups of the open game. Replaces the WinForms ProfilesList (list, load, unload, delete with renumbering)
    /// and ProfileSettings (through <see cref="ProfileSettingsModel"/>, shared with the form). UI thread.
    /// </summary>
    internal sealed class ProfilesService : IProfilesService
    {
        private static readonly Guid SingleKeyboardGuid = new Guid("10000000-1000-1000-1000-100000000000");

        private readonly SetupSession session;

        public ProfilesService(SetupSession session)
        {
            this.session = session;
        }

        private static bool ProfilesDisabled => App_Misc.DisableGameProfiles || GameProfile.Game?.MetaInfo.DisableProfiles == true;

        public List<ProfileSummary> List()
        {
            session.EnsureOpen();

            if (ProfilesDisabled)
            {
                return new List<ProfileSummary>();
            }

            return GameProfile.ReadProfiles().Select(p => Summary(p.Key, p.Value)).ToList();
        }

        /// <summary>A profile file as listed (what ProfilesList and its preview showed).</summary>
        internal static ProfileSummary Summary(int id, JObject? json)
        {
            ProfileSummary summary = new ProfileSummary { Id = id };

            if (json == null)
            {
                summary.Notes = "This profile can't be read.";
                return summary;
            }

            summary.Title = (string?)json["Title"] ?? "";
            summary.Notes = (string?)json["Notes"] ?? "";
            summary.AutoPlay = (bool?)json["AutoPlay"]?["Enabled"] ?? false;

            if (json["Data"] is JArray players)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    JToken data = players[i];
                    DeviceKind kind = TemplateKind(data);

                    string guests = data["Guests"]?["GUIDS"]?.ToString() ?? "";

                    summary.PlayerList.Add(new ProfilePlayer
                    {
                        Number = ((int?)data["PlayerID"] ?? i) + 1,
                        Nickname = (string?)data["Nickname"],
                        Device = kind,
                        ScreenIndex = (int?)data["ScreenIndex"] ?? 0,
                        Guests = guests.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length,
                    });

                    if (kind == DeviceKind.XInput || kind == DeviceKind.DInput || kind == DeviceKind.Sdl)
                    {
                        summary.Gamepads++;
                    }
                    else
                    {
                        summary.KeyboardMice++;
                    }
                }

                summary.PlayerList = summary.PlayerList.OrderBy(p => p.Number).ToList();
                summary.Players = players.Count;
            }

            return summary;
        }

        private static DeviceKind TemplateKind(JToken data)
        {
            int hidCount = data["HIDDeviceID"] is JArray hids ? hids.Count(h => !string.IsNullOrEmpty(h.ToString())) : 0;

            return DeviceIds.TemplateKind(
                (bool?)data["IsXInput"] == true,
                (bool?)data["IsDInput"] == true,
                (bool?)data["IsSDL2"] == true,
                Guid.TryParse((string?)data["GamepadGuid"], out Guid guid) && guid == SingleKeyboardGuid,
                (bool?)data["IsRawMouse"] == true,
                hidCount);
        }

        public SetupSnapshot Load(ProfileRequest request)
        {
            session.EnsureEditable();

            if (ProfilesDisabled)
            {
                throw SetupSession.Conflict("profilesDisabled", "Profiles are turned off for this game.");
            }

            if (!GameProfile.ReadProfiles().Any(p => p.Key == request.ProfileId))
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "Profile not found.", new { profileId = request.ProfileId });
            }

            //The error box of LoadGameProfile becomes the error of this call.
            bool loaded = session.CaptureErrors(() => GameProfile.Instance.LoadGameProfile(request.ProfileId), out List<string> errors);

            session.AfterProfileReset(null);

            if (!loaded)
            {
                throw SetupSession.Conflict("profileInvalid", errors.FirstOrDefault() ?? "The profile can't be loaded.");
            }

            //Devices are matched to the profile players by the pump as they are detected or used; with AutoPlay the
            //session starts by itself once every player is matched.
            return session.CommitCommand();
        }

        public SetupSnapshot Unload()
        {
            session.EnsureEditable();

            GameProfile.Instance.Reset();
            session.AfterProfileReset(null);

            return session.CommitCommand();
        }

        public void Delete(ProfileRequest request)
        {
            session.EnsureEditable();

            if (!GameProfile.DeleteProfile(request.ProfileId))
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "Profile not found.", new { profileId = request.ProfileId });
            }

            //DeleteProfile renumbered the files and reset the setup, as the profiles list did.
            session.AfterProfileReset(null);
            session.ProfilesChanged();
            session.RequestRefresh();
        }

        public ProfileSettings GetSettings()
        {
            session.EnsureOpen();

            ProfileSettingsModel model = ProfileSettingsModel.FromCurrent();
            session.Audio.Refresh();

            ProfileSettings settings = new ProfileSettings
            {
                IsNew = model.IsNew,
                Title = model.Title,
                Notes = model.Notes,
                AutoPlay = model.AutoPlay,
                UseNicknames = model.UseNicknames,
                AutoDesktopScaling = model.AutoDesktopScaling,
                SplitDivider = model.SplitDiv,
                SplitDividerHideDesktopOnly = model.SplitDivHideDesktopOnly,
                SplitDividerColor = model.SplitDivColor,
                CustomLayoutHorizontalLines = model.CustomLayoutHor,
                CustomLayoutVerticalLines = model.CustomLayoutVer,
                CustomLayoutMaxPlayers = model.CustomLayoutMax,
                WindowSetupDelayMs = model.WindowsSetupTimingMs,
                PauseBetweenInstancesSec = model.PauseBetweenInstanceLaunchSec,
                Network = model.Network,
                CustomAudio = model.AudioCustom,
                CutscenesKeepAspectRatio = model.CutscenesKeepAspectRatio,
                CutscenesMuteAudioOnly = model.CutscenesMuteAudioOnly,
                CutscenesUnfocus = model.CutscenesUnfocus,
                WindowsMerger = model.WindowsMerger,
                LosslessHook = model.LosslessHook,
                MergerResolution = model.MergerResolution,
                NetworkChoices = NetworkChoices(),
                AudioChoices = session.Audio.Current.ToList(),
                MergerResolutionChoices = MergerResolutionChoices(),
                //the names ProfileSettingsModel validates against (the form's colour list)
                SplitDividerColorChoices = BackgroundColors.ColorsDictionnary.Keys.Select(name => new SettingChoice { Value = name, Label = name }).ToList(),
            };

            for (int i = 0; i < model.Slots.Count; i++)
            {
                ProfileSlotModel slot = model.Slots[i];

                settings.Slots.Add(new ProfileSlot
                {
                    Nickname = slot.Nickname,
                    SteamId = slot.SteamId,
                    SteamIdLocked = slot.SteamIdLocked,
                    IdealProcessor = slot.IdealProcessor,
                    Affinity = slot.Affinity,
                    PriorityClass = slot.PriorityClass,
                    AudioDevice = i < ProfileSettingsModel.AudioInstanceCount && model.AudioInstances.TryGetValue("AudioInstance" + (i + 1), out string audio) ? audio : AudioOutputs.Default,
                });
            }

            return settings;
        }

        public List<ValidationError> SaveSettings(ProfileSettings settings)
        {
            session.EnsureEditable();

            ProfileSettingsModel current = ProfileSettingsModel.FromCurrent();

            ProfileSettingsModel model = new ProfileSettingsModel
            {
                IsNew = current.IsNew,
                Title = settings.Title ?? "",
                Notes = settings.Notes ?? "",
                AutoPlay = settings.AutoPlay,
                UseNicknames = settings.UseNicknames,
                AutoDesktopScaling = settings.AutoDesktopScaling,
                SplitDiv = settings.SplitDivider,
                SplitDivHideDesktopOnly = settings.SplitDividerHideDesktopOnly,
                SplitDivColor = string.IsNullOrEmpty(settings.SplitDividerColor) ? "Black" : settings.SplitDividerColor,
                CustomLayoutHor = settings.CustomLayoutHorizontalLines,
                CustomLayoutVer = settings.CustomLayoutVerticalLines,
                CustomLayoutMax = settings.CustomLayoutMaxPlayers,
                WindowsSetupTimingMs = settings.WindowSetupDelayMs,
                PauseBetweenInstanceLaunchSec = settings.PauseBetweenInstancesSec,
                Network = string.IsNullOrEmpty(settings.Network) ? "Automatic" : settings.Network,
                AudioCustom = settings.CustomAudio,
                CutscenesKeepAspectRatio = settings.CutscenesKeepAspectRatio,
                CutscenesMuteAudioOnly = settings.CutscenesMuteAudioOnly,
                CutscenesUnfocus = settings.CutscenesUnfocus,
                StopUINav = current.StopUINav,//legacy gamepad-as-mouse flag: kept as the profile file has it
                WindowsMerger = settings.WindowsMerger,
                LosslessHook = settings.LosslessHook,
                MergerResolution = settings.MergerResolution ?? "",
            };

            List<ValidationError> errors = new List<ValidationError>();
            List<ProfileSlot> slots = settings.Slots ?? new List<ProfileSlot>();

            for (int i = 0; i < current.Slots.Count; i++)
            {
                ProfileSlotModel locked = current.Slots[i];
                ProfileSlot? slot = i < slots.Count ? slots[i] : null;

                model.Slots.Add(new ProfileSlotModel
                {
                    Nickname = slot?.Nickname ?? locked.Nickname,
                    SteamId = locked.SteamIdLocked ? locked.SteamId : (slot?.SteamId ?? locked.SteamId),//the handler imposes its Steam IDs
                    SteamIdLocked = locked.SteamIdLocked,
                    IdealProcessor = slot?.IdealProcessor ?? locked.IdealProcessor,
                    Affinity = slot?.Affinity ?? locked.Affinity,
                    PriorityClass = slot?.PriorityClass ?? locked.PriorityClass,
                });
            }

            //Audio outputs: per slot (AudioInstance1..8). An unknown output is refused (the form only offered listed ones).
            if (slots.Take(ProfileSettingsModel.AudioInstanceCount).Any(s => !string.IsNullOrEmpty(s.AudioDevice) && !session.Audio.IsKnown(s.AudioDevice)))
            {
                session.Audio.Refresh();//an output plugged since the list was read
            }

            for (int i = 0; i < ProfileSettingsModel.AudioInstanceCount; i++)
            {
                string key = "AudioInstance" + (i + 1);
                string value = i < slots.Count && !string.IsNullOrEmpty(slots[i].AudioDevice) ? slots[i].AudioDevice
                               : current.AudioInstances.TryGetValue(key, out string kept) ? kept : AudioOutputs.Default;

                if (i < slots.Count && !session.Audio.IsKnown(value))
                {
                    errors.Add(new ValidationError { Field = "slots.audioDevice", Index = i, Message = "This audio output is not available." });
                }

                model.AudioInstances[key] = value;
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            bool wasNew = model.IsNew;
            List<ProfileSettingsError> modelErrors = session.CaptureErrors(() => model.Apply(), out _);

            errors.AddRange(modelErrors.Select(e => new ValidationError { Field = e.Field, Index = e.Index, Message = e.Message }));

            if (errors.Count == 0)
            {
                if (!wasNew)
                {
                    //Apply rewrote Profile[n].json and reloaded it (devices are matched again).
                    session.AfterProfileReset(null);
                    session.ProfilesChanged();
                }

                session.RequestRefresh();
            }

            return errors;
        }

        /// <summary>"Automatic" then the up IPv4 network interfaces (ProfileSettings.RefreshCmbNetwork).</summary>
        private static List<SettingChoice> NetworkChoices()
        {
            List<SettingChoice> choices = new List<SettingChoice> { new SettingChoice { Value = "Automatic", Label = "Automatic" } };

            try
            {
                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (item.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    if (item.GetIPProperties().UnicastAddresses.Any(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork) &&
                        !choices.Any(c => c.Value == item.Name))
                    {
                        choices.Add(new SettingChoice { Value = item.Name, Label = item.Name });
                    }
                }
            }
            catch (Exception)
            {
            }

            return choices;
        }

        /// <summary>Display resolutions of every monitor, up to its size (ProfileSettings.GetAllScreensResolutions).</summary>
        private static List<SettingChoice> MergerResolutionChoices()
        {
            List<string> resolutions = new List<string>();

            try
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    MonitorsDpiScaling.DEVMODE mode = new MonitorsDpiScaling.DEVMODE();

                    for (int i = 0; MonitorsDpiScaling.EnumDisplaySettings(screen.DeviceName, i, ref mode); i++)
                    {
                        string res = $"{mode.dmPelsWidth}X{mode.dmPelsHeight}";

                        if (mode.dmPelsWidth <= screen.Bounds.Width && mode.dmPelsHeight <= screen.Bounds.Height && !resolutions.Contains(res))
                        {
                            resolutions.Add(res);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return resolutions.Select(r => new SettingChoice { Value = r, Label = r.Replace("X", " × ") }).ToList();
        }
    }
}
