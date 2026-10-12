using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Generic.Step;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// Maps the setup state (GameProfile instance and statics, BoundsFunctions.Screens, the handler) to the
    /// <see cref="SetupSnapshot"/> contract. Coordinates are desktop pixels (MonitorBounds); the virtual-canvas
    /// EditBounds/UIBounds kept for profiles are not sent. UI thread.
    /// </summary>
    internal static class SetupSnapshotBuilder
    {
        private static readonly Guid SingleKeyboardGuid = new Guid("10000000-1000-1000-1000-100000000000");

        /// <summary>Id of a device (its DeviceKey, set where detected; a fallback is computed for older objects).</summary>
        public static string KeyOf(PlayerInfo device)
        {
            if (string.IsNullOrEmpty(device.DeviceKey))
            {
                device.DeviceKey = DeviceIds.FallbackKey(device.InputType, device.GamepadId, device.GamepadGuid, device.HIDDeviceID?.FirstOrDefault());
            }

            return device.DeviceKey;
        }

        /// <summary>Makes the ids of the detected devices unique (stored back so they stay stable).</summary>
        public static void EnsureUniqueKeys(List<PlayerInfo> devices)
        {
            List<string> keys = devices.Select(KeyOf).ToList();
            List<string> unique = DeviceIds.MakeUnique(keys);

            for (int i = 0; i < devices.Count; i++)
            {
                if (unique[i] != keys[i])
                {
                    devices[i].DeviceKey = unique[i];
                }
            }
        }

        /// <summary>The device with this id among the detected devices, or null.</summary>
        public static PlayerInfo? FindDevice(string id)
        {
            List<PlayerInfo>? devices = GameProfile.Instance?.DevicesList;

            if (devices == null)
            {
                return null;
            }

            EnsureUniqueKeys(devices);
            return devices.FirstOrDefault(d => d.DeviceKey == id);
        }

        /// <summary>The primary device of every player, in launch order: GameProfile.AssignedDevices, then partners left alone in DevicesToMerge (CleanClone launches them too).</summary>
        public static List<PlayerInfo> PlayerPrimaries()
        {
            List<PlayerInfo> primaries = GameProfile.AssignedDevices.ToList();

            foreach (PlayerInfo merged in GameProfile.DevicesToMerge)
            {
                if (merged.ScreenIndex != -1 && !primaries.Any(p => p.ScreenIndex == merged.ScreenIndex && p.MonitorBounds == merged.MonitorBounds) && !primaries.Contains(merged))
                {
                    primaries.Add(merged);
                }
            }

            return primaries;
        }

        /// <summary>The partner devices of a player (raw keyboard/mouse in DevicesToMerge with the same area).</summary>
        public static List<PlayerInfo> PartnersOf(PlayerInfo primary)
        {
            return GameProfile.DevicesToMerge.Where(d => d != primary && d.ScreenIndex == primary.ScreenIndex && d.MonitorBounds == primary.MonitorBounds && d.ScreenIndex != -1).Distinct().ToList();
        }

        /// <summary>The primary device of the player a device belongs to (itself, its raw partner, or its guest host), or null.</summary>
        public static PlayerInfo? PlayerOf(PlayerInfo device)
        {
            List<PlayerInfo> primaries = PlayerPrimaries();

            if (primaries.Contains(device))
            {
                return device;
            }

            return primaries.FirstOrDefault(p => PartnersOf(p).Contains(device));
        }

        public static SetupSnapshot Build(SetupSession session)
        {
            UserGameInfo game = session.Game!;
            GenericGameInfo gen = GameProfile.Game;
            GameProfile profile = GameProfile.Instance;
            List<PlayerInfo> devices = profile.DevicesList;
            bool loaded = GameProfile.Loaded;

            EnsureUniqueKeys(devices);

            SetupSnapshot snapshot = new SetupSnapshot
            {
                GameId = game.GameGuid ?? "",
                GameName = gen.GameName ?? "",
                TotalBounds = SnapshotMath.ToRect(BoundsFunctions.TotalBounds),
                UseApiIndex = gen.MetaInfo.UseApiIndex,
                SaveProfileOnLaunch = gen.MetaInfo.SaveProfile,
                AudioOutputs = session.Audio.Current.ToList(),
                CustomAudio = GameProfile.AudioCustomSettings,
                Capabilities = new SetupCapabilities
                {
                    MaxPlayers = gen.MaxPlayers,
                    Gamepads = gen.Hook.DInputEnabled || gen.Hook.XInputEnabled || gen.Hook.XInputReroute || gen.Hook.SDL2Enabled ||
                               gen.ProtoInput.XinputHook || gen.ProtoInput.DinputDeviceHook,
                    Keyboard = gen.SupportsKeyboard,
                    MultipleKeyboardsAndMice = gen.SupportsMultipleKeyboardsAndMice,
                    PlayersPerInstance = Math.Max(1, gen.PlayersPerInstance),
                },
            };

            // players, in launch order
            List<PlayerInfo> primaries = PlayerPrimaries();
            Dictionary<PlayerInfo, string> playerOfDevice = new Dictionary<PlayerInfo, string>();
            Dictionary<PlayerInfo, string> hostOfGuest = new Dictionary<PlayerInfo, string>();
            UserScreen[] screens = BoundsFunctions.Screens ?? new UserScreen[0];

            for (int i = 0; i < primaries.Count; i++)
            {
                PlayerInfo primary = primaries[i];
                string id = KeyOf(primary);
                UserScreen? screen = primary.ScreenIndex >= 0 && primary.ScreenIndex < screens.Length ? screens[primary.ScreenIndex] : null;

                SetupPlayer player = new SetupPlayer
                {
                    Id = id,
                    Number = i + 1,
                    ScreenIndex = primary.ScreenIndex,
                    Bounds = SnapshotMath.ToRect(primary.MonitorBounds),
                    Nickname = string.IsNullOrEmpty(primary.Nickname) ? null : primary.Nickname,
                    SteamId = primary.SteamID > 0 ? primary.SteamID.ToString() : null,
                    MaxGuests = primary.IsController ? (loaded ? primary.CurrentMaxGuests : Math.Max(0, gen.PlayersPerInstance - 1)) : 0,
                    CanExpand = screen != null && SnapshotMath.CanExpand(screen.Type, loaded),
                    CanResize = screen != null && SnapshotMath.CanResize(screen.SubScreensBounds.Count, loaded),
                    AudioDeviceId = GameProfile.AudioCustomSettings ? AudioOf(i) : null,
                };

                player.DeviceIds.Add(id);
                playerOfDevice[primary] = id;

                foreach (PlayerInfo partner in PartnersOf(primary))
                {
                    player.DeviceIds.Add(KeyOf(partner));
                    playerOfDevice[partner] = id;
                }

                foreach (PlayerInfo guest in primary.InstanceGuests)
                {
                    player.GuestDeviceIds.Add(KeyOf(guest));
                    hostOfGuest[guest] = id;
                }

                snapshot.Players.Add(player);
            }

            // devices
            bool buttonPressMode = gen.MetaInfo.ProfileAssignGamepadByButonPress;

            foreach (PlayerInfo device in devices)
            {
                string? hid = device.HIDDeviceID?.FirstOrDefault();

                snapshot.Devices.Add(new SetupDevice
                {
                    Id = KeyOf(device),
                    Kind = DeviceIds.KindOf(device.InputType),
                    Name = DeviceIds.Name(device.InputType, device.GamepadId, device.GamepadName, hid),
                    ApiIndex = device.GamepadId,
                    Revealed = device.IsInputUsed,
                    NeedsButtonPress = !device.IsInputUsed && device.IsController,
                    IsVirtual = DeviceIds.IsVirtual(hid),
                    PlayerId = playerOfDevice.TryGetValue(device, out string playerId) ? playerId : null,
                    GuestOfPlayerId = hostOfGuest.TryGetValue(device, out string hostId) ? hostId : null,
                });
            }

            // screens and slots
            List<Rectangle> playerAreas = primaries.Select(p => p.MonitorBounds).ToList();
            Rectangle primaryBounds = Screen.PrimaryScreen?.Bounds ?? Rectangle.Empty;

            for (int s = 0; s < screens.Length; s++)
            {
                UserScreen screen = screens[s];
                (int rows, int columns) = SnapshotMath.Grid(screen.Type, GameProfile.CustomLayout_Hor, GameProfile.CustomLayout_Ver, screen.SubScreensBounds.Count);

                SetupScreen dto = new SetupScreen
                {
                    Index = s,
                    DisplayIndex = screen.DisplayIndex,
                    Bounds = SnapshotMath.ToRect(screen.MonitorBounds),
                    IsPrimary = screen.MonitorBounds == primaryBounds,
                    Layout = SnapshotMath.ToLayout(screen.Type),
                    Rows = rows,
                    Columns = columns,
                    LayoutLocked = loaded,
                };

                if (screen.Type != UserScreenType.Manual)
                {
                    int slotIndex = 0;

                    foreach (KeyValuePair<Rectangle, RectangleF> slot in screen.SubScreensBounds)
                    {
                        int occupant = SnapshotMath.OccupantOf(slot.Key, playerAreas);

                        dto.Slots.Add(new SetupSlot
                        {
                            Index = slotIndex++,
                            Bounds = SnapshotMath.ToRect(slot.Key),
                            PlayerId = occupant >= 0 ? snapshot.Players[occupant].Id : null,
                            Expected = occupant < 0 && loaded && GameProfile.GhostBounds.Any(g => g.Item1.IntersectsWith(slot.Key)),
                        });
                    }
                }

                snapshot.Screens.Add(dto);
            }

            // options and custom steps
            Dictionary<string, object> values = profile.Options;

            foreach (GameOption option in gen.Options.Where(o => !o.Hidden))
            {
                OptionKind kind = OptionsCodec.KindOf(option);
                values.TryGetValue(option.Key, out object value);

                SetupOption dto = new SetupOption
                {
                    Key = option.Key,
                    Name = option.Name ?? option.Key,
                    Description = option.Description ?? "",
                    Kind = kind,
                };

                if (kind == OptionKind.Choice)
                {
                    IList choices = OptionsCodec.Choices(option);

                    for (int i = 0; i < choices.Count; i++)
                    {
                        dto.Choices.Add(new OptionChoice { Index = i, Label = OptionsCodec.Label(choices[i]) });
                    }

                    dto.SelectedIndex = OptionsCodec.Resolve(option, value);
                }
                else if (kind == OptionKind.Text)
                {
                    dto.Text = value?.ToString() ?? "";
                }
                else
                {
                    dto.Text = value?.ToString();
                }

                snapshot.Options.Add(dto);
            }

            List<CustomStep> steps = gen.CustomSteps;
            string handlerFolder = Path.GetFileNameWithoutExtension(gen.JsFileName ?? "");

            for (int i = 0; i < steps.Count; i++)
            {
                CustomStep step = steps[i];
                GameOption? option = step.Option;
                object? value = null;

                if (option != null)
                {
                    values.TryGetValue(option.Key, out value);
                }

                SetupCustomStep dto = new SetupCustomStep
                {
                    Index = i,
                    Title = step.Title ?? "",
                    OptionKey = option?.Key ?? "",
                    Required = session.IsStepRequired(i),
                    SelectedIndex = option == null ? null : OptionsCodec.ResolveStepItem(option, value),
                    AutoSelected = option != null && session.AutoSelectedSteps.Contains(option.Key),
                };

                IList? items = option?.List;

                for (int j = 0; items != null && j < items.Count; j++)
                {
                    if (!(items[j] is IDictionary<string, object>))
                    {
                        continue;//only cards (JS objects) are shown, as JSUserInputControl did
                    }

                    string? image = OptionsCodec.DictionaryText(items[j], "ImageUrl");

                    dto.Items.Add(new OptionChoice
                    {
                        Index = j,
                        Label = OptionsCodec.Label(items[j]),
                        Details = OptionsCodec.DictionaryText(items[j], "Details"),
                        ImageUrl = image == null ? null : HandlerImageUrl(handlerFolder, image),
                    });
                }

                snapshot.CustomSteps.Add(dto);
            }

            snapshot.Profile = ProfileMatchOf(loaded);
            snapshot.Checks = SetupChecks.Compute(session);
            snapshot.Flow = session.ComputeFlow(snapshot.Checks);
            snapshot.Hint = HintOf(snapshot, session);

            return snapshot;
        }

        /// <summary>Audio output of player <paramref name="index"/> (AudioInstance{index+1}), "Default" when unset.</summary>
        public static string AudioOf(int index)
        {
            return GameProfile.AudioInstances.TryGetValue("AudioInstance" + (index + 1), out string id) && !string.IsNullOrEmpty(id) ? id : AudioOutputs.Default;
        }

        /// <summary>URL of a custom step picture: &lt;handlers&gt;\&lt;handler js name&gt;\&lt;url lowercased&gt;, as ContentManager.LoadImage reads it.</summary>
        public static string HandlerImageUrl(string handlerFolder, string relativeUrl)
        {
            string path = string.Join("/", relativeUrl.ToLowerInvariant().Replace('\\', '/').Split('/').Where(p => p.Length > 0).Select(Uri.EscapeDataString));
            return NucleusPaths.HandlersBaseUrl + Uri.EscapeDataString(handlerFolder) + "/" + path;
        }

        private static ProfileMatch ProfileMatchOf(bool loaded)
        {
            ProfileMatch match = new ProfileMatch
            {
                Loaded = loaded,
                ProfileId = loaded ? GameProfile.CurrentProfileId : (int?)null,
                Assigned = loaded ? GameProfile.TotalAssignedPlayers : 0,
                Total = GameProfile.TotalProfilePlayers,
                AutoPlay = GameProfile.AutoPlay,
            };

            if (!loaded)
            {
                return match;
            }

            if (GameProfile.TotalAssignedPlayers < GameProfile.TotalProfilePlayers && GameProfile.TotalAssignedPlayers >= 0 &&
                GameProfile.TotalAssignedPlayers < GameProfile.ProfilePlayersList.Count)
            {
                match.AwaitingPlayer = GameProfile.TotalAssignedPlayers + 1;
                match.AwaitingKind = TemplateKind(GameProfile.ProfilePlayersList[GameProfile.TotalAssignedPlayers]);
            }
            else
            {
                int waiting = GameProfile.AssignedDevices.FindIndex(p => p.WaitGuests);

                if (waiting >= 0)
                {
                    match.AwaitingPlayer = waiting + 1;
                    match.AwaitingKind = DeviceIds.KindOf(GameProfile.AssignedDevices[waiting].InputType);
                }
            }

            match.Ready = SetupSession.PlayersValid();
            return match;
        }

        /// <summary>Kind of device a profile player expects.</summary>
        private static DeviceKind TemplateKind(PlayerInfo template)
        {
            return DeviceIds.TemplateKind(template.IsXInput, template.IsDInput, template.IsSDL2, template.GamepadGuid == SingleKeyboardGuid,
                template.IsRawMouse, template.HIDDeviceID?.Count(h => !string.IsNullOrEmpty(h)) ?? 0);
        }

        /// <summary>How long the "gamepad not identified" hint stays after the press that failed.</summary>
        internal static readonly TimeSpan UnidentifiedHintDuration = TimeSpan.FromSeconds(10);

        /// <summary>A hidden gamepad whose last button press couldn't identify it (no unique DirectInput match), recently.</summary>
        internal static PlayerInfo? UnidentifiedPad(IEnumerable<PlayerInfo>? devices, DateTime nowUtc)
        {
            return devices?.FirstOrDefault(d => d.IsController && !d.IsInputUsed && !d.IsFake &&
                                                d.IdentificationFailedUtc != DateTime.MinValue &&
                                                nowUtc - d.IdentificationFailedUtc < UnidentifiedHintDuration);
        }

        private static SetupHint? HintOf(SetupSnapshot snapshot, SetupSession session)
        {
            SetupHint Hint(string code, string text, params string[] args) => new SetupHint { Code = code, Text = text, Args = args.ToList() };

            SetupStep current = snapshot.Flow.Steps[snapshot.Flow.Current];

            if (current.Kind == SetupStepKind.Custom)
            {
                SetupCustomStep step = snapshot.CustomSteps[current.CustomStepIndex ?? 0];
                return step.SelectedIndex == null ? Hint("pickStepItem", $"Pick one: {step.Title}", step.Title) : null;
            }

            if (current.Kind != SetupStepKind.Players)
            {
                return null;
            }

            if (snapshot.Profile.Loaded && !snapshot.Profile.Ready && snapshot.Profile.AwaitingPlayer.HasValue)
            {
                string number = snapshot.Profile.AwaitingPlayer.Value.ToString();
                return Hint("profileWaitingForPlayer", $"Player {number}: press a button or a key on the device to use.", number);
            }

            if (snapshot.Devices.Count(d => !d.IsVirtual) == 0)
            {
                return Hint("noDevices", "Plug in a controller.");
            }

            PlayerInfo? unidentified = UnidentifiedPad(GameProfile.Instance?.DevicesList, DateTime.UtcNow);

            if (unidentified != null)
            {
                GenericGameInfo? gen = GameProfile.Game;
                bool slotModeWorks = gen != null && !gen.UseDevReorder && !gen.CreateSingleDeviceFile;

                return Hint("padNotIdentified", slotModeWorks
                    ? "A gamepad was pressed, but Nucleus couldn't tell which device it is. Press A on that gamepad alone. If it keeps failing, close tools that duplicate gamepads (DS4Windows without HidHide, Steam Input) or turn on \"Identify gamepads by slot\"."
                    : "A gamepad was pressed, but Nucleus couldn't tell which device it is. Press A on that gamepad alone. If it keeps failing, close tools that duplicate gamepads (DS4Windows without HidHide, Steam Input).",
                    KeyOf(unidentified));
            }

            if (snapshot.Players.Count == 0)
            {
                bool anyRevealed = snapshot.Devices.Any(d => d.Revealed && !d.IsVirtual);
                return anyRevealed
                    ? Hint("dragPlayersToScreens", "Drag a device to a screen to add a player.")
                    : Hint("pressButtonToIdentify", "Press a button on a controller (or use a keyboard) to show it.");
            }

            return null;
        }
    }
}
