using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Generic.Step;
using SharpDX.XInput;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// The game setup on the bridge. Replaces the WinForms setup screen, Core_Interface's step machine and the options
    /// controls. Calls arrive on the UI thread (WebView2), where every setup state change happens.
    /// </summary>
    internal sealed class SetupService : ISetupService
    {
        private readonly SetupSession session;

        public SetupService(SetupSession session)
        {
            this.session = session;
        }

        // ---------------------------------------------------------------- lifecycle and navigation

        public SetupSnapshot Open(GameRequest request)
        {
            if (session.SessionActive)
            {
                throw SetupSession.Conflict("sessionRunning", "A session is running.");
            }

            GameManager? manager = GameManager.Instance;

            if (manager?.User?.Games == null)
            {
                throw new BridgeException(BridgeErrorKind.Unavailable, "The game library is not loaded yet.");
            }

            UserGameInfo? game = manager.User.Games.FirstOrDefault(g => string.Equals(g.GameGuid, request.GameId, StringComparison.OrdinalIgnoreCase) && g.Game != null);

            if (game == null)
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "Game not found.", new { gameId = request.GameId });
            }

            session.Open(game);
            return session.CommitCommand();
        }

        public void Close()
        {
            if (!session.IsOpen)
            {
                return;
            }

            if (session.SessionActive)
            {
                throw SetupSession.Conflict("sessionRunning", "A session is running.");
            }

            session.Close();
        }

        public SetupSnapshot? Get() => session.Commit();

        public SetupSnapshot Next()
        {
            session.EnsureEditable();
            session.RefreshRequired();

            StepId? next = SetupFlowRules.Next(session.FlowInput(), session.CurrentStep);

            if (next == null)
            {
                throw SetupSession.Conflict("cannotGoNext", "This step isn't complete, or it is the last one.");
            }

            session.CurrentStep = next.Value;
            session.RefreshRequired();
            return session.CommitCommand();
        }

        public SetupSnapshot Back()
        {
            session.EnsureEditable();
            session.RefreshRequired();

            StepId? previous = SetupFlowRules.Back(session.FlowInput(), session.CurrentStep);

            if (previous == null)
            {
                throw SetupSession.Conflict("firstStep", "This is the first step.");
            }

            GameProfile.Ready = false;//as GotoPrevStep: a loaded profile stops auto-advancing
            session.CurrentStep = previous.Value;
            return session.CommitCommand();
        }

        // ---------------------------------------------------------------- screens and players

        public SetupSnapshot SetLayout(SetLayoutRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.SetLayout(request.ScreenIndex, SnapshotMath.ToScreenType(request.Layout)));
            return session.CommitCommand();
        }

        public SetupSnapshot Join(DeviceRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.Join(Device(request.DeviceId)));
            return session.CommitCommand();
        }

        public SetupSnapshot AssignToSlot(AssignToSlotRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.AssignToSlot(Device(request.DeviceId), request.ScreenIndex, request.SlotIndex));
            return session.CommitCommand();
        }

        public SetupSnapshot AssignAt(AssignAtRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.AssignAt(Device(request.DeviceId), request.ScreenIndex, request.X, request.Y));
            return session.CommitCommand();
        }

        public SetupSnapshot Unassign(DeviceRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.Unassign(Device(request.DeviceId)));
            return session.CommitCommand();
        }

        public SetupSnapshot AddGuest(AddGuestRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.AddGuest(Player(request.PlayerId), Device(request.DeviceId)));
            return session.CommitCommand();
        }

        public SetupSnapshot RemoveGuest(DeviceRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.RemoveGuest(Device(request.DeviceId)));
            return session.CommitCommand();
        }

        public SetupSnapshot ToggleExpand(PlayerRequest request)
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.ToggleExpand(Player(request.PlayerId)));
            return session.CommitCommand();
        }

        public SetupSnapshot Resize(ResizePlayerRequest request)
        {
            session.EnsureEditable();

            BoundsFunctions.ResizeEdge edge;

            switch (request.Edge)
            {
                case Edge.Left: edge = BoundsFunctions.ResizeEdge.Left; break;
                case Edge.Right: edge = BoundsFunctions.ResizeEdge.Right; break;
                case Edge.Top: edge = BoundsFunctions.ResizeEdge.Top; break;
                default: edge = BoundsFunctions.ResizeEdge.Bottom; break;
            }

            if (request.Steps == 0)
            {
                throw new BridgeException(BridgeErrorKind.InvalidParams, "Steps can't be 0.");
            }

            Refuse(BoundsFunctions.Resize(Player(request.PlayerId), edge, Math.Max(-100, Math.Min(100, request.Steps))));
            return session.CommitCommand();
        }

        public SetupSnapshot AutoFill()
        {
            session.EnsureEditable();
            Refuse(BoundsFunctions.AutoFill());
            return session.CommitCommand();
        }

        public SetupSnapshot ClearAll()
        {
            session.EnsureEditable();
            session.ResetSetup();
            return session.CommitCommand();
        }

        // ---------------------------------------------------------------- options and custom steps

        public SetupSnapshot SetOptionChoice(SetOptionChoiceRequest request)
        {
            session.EnsureEditable();

            GameOption option = VisibleOption(request.Key);

            if (OptionsCodec.KindOf(option) != OptionKind.Choice)
            {
                throw SetupSession.Conflict("notAChoice", "This option is not a list of values.");
            }

            IList choices = OptionsCodec.Choices(option);

            if (request.Index < 0 || request.Index >= choices.Count)
            {
                throw new BridgeException(BridgeErrorKind.InvalidParams, "No value at this index.", new { index = request.Index });
            }

            //the original list item (or enum value), as PlayerOptionsControl stored it
            GameProfile.Instance.Options[option.Key] = choices[request.Index];
            session.RefreshRequired();
            return session.CommitCommand();
        }

        public SetupSnapshot SetOptionText(SetOptionTextRequest request)
        {
            session.EnsureEditable();

            GameOption option = VisibleOption(request.Key);

            if (OptionsCodec.KindOf(option) != OptionKind.Text)
            {
                throw SetupSession.Conflict("notAText", "This option is not a text.");
            }

            GameProfile.Instance.Options[option.Key] = request.Text ?? "";
            session.RefreshRequired();
            return session.CommitCommand();
        }

        public SetupSnapshot SelectStepItem(SelectStepItemRequest request)
        {
            session.EnsureEditable();

            List<CustomStep> steps = GameProfile.Game.CustomSteps;

            if (request.StepIndex < 0 || request.StepIndex >= steps.Count || steps[request.StepIndex].Option?.List == null)
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "Custom step not found.", new { stepIndex = request.StepIndex });
            }

            GameOption option = steps[request.StepIndex].Option;

            if (request.ItemIndex < 0 || request.ItemIndex >= option.List.Count || !(option.List[request.ItemIndex] is IDictionary<string, object>))
            {
                throw new BridgeException(BridgeErrorKind.InvalidParams, "No item at this index.", new { itemIndex = request.ItemIndex });
            }

            //the whole card object (ExpandoObject with the handler's fields), as JSUserInputControl stored it
            GameProfile.Instance.Options[option.Key] = option.List[request.ItemIndex];
            session.ClearAutoSelected(option.Key);
            session.RefreshRequired();

            //as the WinForms custom step (CanPlayUpdated(true, true)): picking on the current step moves to the next one
            StepId current = session.CurrentStep;

            if (current.Kind == SetupStepKind.Custom && current.CustomIndex == request.StepIndex)
            {
                StepId? next = SetupFlowRules.Next(session.FlowInput(), current);

                if (next != null)
                {
                    session.CurrentStep = next.Value;
                }
            }

            return session.CommitCommand();
        }

        // ---------------------------------------------------------------- identity, audio, settings

        public SetupSnapshot SetPlayerIdentity(SetPlayerIdentityRequest request)
        {
            session.EnsureEditable();

            PlayerInfo player = Player(request.PlayerId);
            int index = SetupSnapshotBuilder.PlayerPrimaries().IndexOf(player);

            if (index < 0 || index >= Globals.NucleusMaxPlayers)
            {
                throw SetupSession.Conflict("noIdentitySlot", "This player has no identity slot.");
            }

            string? nickname = request.Nickname?.Trim();
            string? steamId = request.SteamId?.Trim();

            if (nickname != null && nickname.Length == 0)
            {
                throw SetupSession.Conflict("invalidNickname", "Nicknames can't be empty.");
            }

            if (steamId != null)
            {
                GenericGameInfo gen = GameProfile.Game;

                if (gen.UseHandlerSteamIds && gen.PlayerSteamIDs != null && index < gen.PlayerSteamIDs.Length)
                {
                    throw SetupSession.Conflict("steamIdLocked", "This handler sets the Steam IDs.");
                }

                if (!(steamId.Length == 0 || steamId == "-1" || (steamId.Length == 17 && Regex.IsMatch(steamId, "^[0-9]+$"))))
                {
                    throw SetupSession.Conflict("invalidSteamId", "A Steam ID must be 17 digits, for example 76561199075562883.");
                }

                if (steamId.Length == 17 && PlayersIdentityCache.PlayersSteamId.Where((sid, i) => i != index && sid == steamId).Any())
                {
                    throw SetupSession.Conflict("steamIdInUse", "Another player slot uses this Steam ID.");
                }
            }

            //The global player slots of the old Players settings ([ControllerMapping] / [SteamIDs] in settings.ini);
            //the nickname/Steam ID history files live in "game profiles".
            System.IO.Directory.CreateDirectory(Globals.GameProfilesFolder);

            if (nickname != null)
            {
                PlayersIdentityCache.SetNicknameAt(index, nickname);
                PlayersIdentityCache.BackupNicknames();
            }

            if (steamId != null)
            {
                PlayersIdentityCache.SetSteamIdAt(index, steamId.Length == 0 ? null : steamId);
                PlayersIdentityCache.BackupSteamIds();
            }

            //A loaded (or prepared) profile overrides the global slots for its players: keep it in line.
            if (index < GameProfile.ProfilePlayersList.Count)
            {
                PlayerInfo template = GameProfile.ProfilePlayersList[index];

                if (nickname != null)
                {
                    template.Nickname = nickname;
                }

                if (steamId != null)
                {
                    template.SteamID = long.TryParse(PlayersIdentityCache.GetSteamIdAt(index), out long sid) ? sid : template.SteamID;
                }
            }

            return session.CommitCommand();//the pump refreshes the identities (UpdateProfilePlayerIdentity)
        }

        public SetupSnapshot SetPlayerAudio(SetPlayerAudioRequest request)
        {
            session.EnsureEditable();

            PlayerInfo player = Player(request.PlayerId);
            int index = SetupSnapshotBuilder.PlayerPrimaries().IndexOf(player);

            if (index < 0 || index >= ProfileSettingsModel.AudioInstanceCount)
            {
                throw SetupSession.Conflict("noAudioSlot", $"Per-player audio is available for the first {ProfileSettingsModel.AudioInstanceCount} players.");
            }

            string deviceId = string.IsNullOrEmpty(request.AudioDeviceId) ? AudioOutputs.Default : request.AudioDeviceId;

            if (!session.Audio.IsKnown(deviceId))
            {
                session.Audio.Refresh();

                if (!session.Audio.IsKnown(deviceId))
                {
                    throw new BridgeException(BridgeErrorKind.NotFound, "Audio output not found.", new { audioDeviceId = deviceId });
                }
            }

            GameProfile.AudioCustomSettings = true;
            GameProfile.AudioDefaultSettings = false;
            GameProfile.AudioInstances["AudioInstance" + (index + 1)] = deviceId;

            return session.CommitCommand();
        }

        public async Task<SetupSnapshot> RunCheckAction(CheckActionRequest request)
        {
            session.EnsureEditable();

            SetupCheck? check = SetupChecks.Compute(session).FirstOrDefault(c => c.Id == request.CheckId);

            if (check == null)
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "Check not found.", new { checkId = request.CheckId });
            }

            if (check.ActionLabel == null)
            {
                throw SetupSession.Conflict("noAction", "This check has no fix.");
            }

            switch (check.Id)
            {
                case SetupChecks.GameFound:
                    //The file picker belongs to the UI: it opens it and calls library.setExecutable.
                    throw SetupSession.Conflict("needsPicker", "Choose the game executable.");

                case SetupChecks.Admin:
                    if (!SetupChecks.RestartAsAdministrator())
                    {
                        throw SetupSession.Conflict("cancelled", "Restart as administrator was cancelled.");
                    }

                    return session.CommitCommand();

                case SetupChecks.HandlerUpToDate:
                    return await UpdateHandler();

                default:
                    throw SetupSession.Conflict("noAction", "This check has no fix.");
            }
        }

        private async Task<SetupSnapshot> UpdateHandler()
        {
            if (session.HandlerUpdater == null)
            {
                throw SetupSession.Conflict("noAction", "Handler updates are not available.");
            }

            string gameId = session.Game!.GameGuid;

            if (!session.BusyChecks.Add(SetupChecks.HandlerUpToDate))
            {
                throw SetupSession.Conflict("busy", "The handler is already being updated.");
            }

            session.RequestRefresh();

            try
            {
                InstallResult result = await session.HandlerUpdater.UpdateAsync(gameId, CancellationToken.None);
            }
            finally
            {
                session.BusyChecks.Remove(SetupChecks.HandlerUpToDate);
            }

            //The handler script was replaced: open the setup again on the new one (still on the UI thread).
            if (session.IsOpen && !session.SessionActive && session.Game?.GameGuid == gameId)
            {
                UserGameInfo game = session.Game;

                if (GameManager.Instance.Games.TryGetValue(game.GameGuid, out GenericGameInfo updated) && updated != null)
                {
                    game.InitializeDefault(updated, game.ExePath);
                }

                session.Open(game);
            }

            return session.CommitCommand();
        }

        public SetupSnapshot SetUseApiIndex(SetEnabledRequest request)
        {
            session.EnsureEditable();

            GenericGameInfo gen = GameProfile.Game;
            gen.MetaInfo.UseApiIndex = request.Enabled;

            if (request.Enabled)
            {
                gen.MetaInfo.ProfileAssignGamepadByButonPress = false;
            }

            bool wasLoaded = GameProfile.Loaded;
            int profileId = GameProfile.CurrentProfileId;

            //Device objects are rebuilt (DevicesFunctions drops their assignments too).
            GameProfile.UseXinputIndex = request.Enabled && !gen.UseDevReorder && !gen.CreateSingleDeviceFile;

            if (wasLoaded)
            {
                GameProfile.Instance.LoadGameProfile(profileId);
                session.AfterProfileReset(null);
            }

            session.CurrentStep = StepId.Players;
            return session.CommitCommand();
        }

        public SetupSnapshot SetSaveProfileOnLaunch(SetEnabledRequest request)
        {
            session.EnsureEditable();
            GameProfile.Game.MetaInfo.SaveProfile = request.Enabled;
            return session.CommitCommand();
        }

        public SetupSnapshot RefreshDevices()
        {
            session.EnsureEditable();

            GenericGameInfo gen = GameProfile.Game;

            //Rescanning replaces device objects: assigned DirectInput pads come back unassigned, like a hot-plug.
            session.SuppressDeviceLost(TimeSpan.FromSeconds(3));

            if (gen.Hook.DInputEnabled || gen.Hook.XInputReroute || gen.ProtoInput.DinputDeviceHook || gen.Hook.XInputEnabled || gen.Hook.SDL2Enabled)
            {
                DevicesFunctions.RefreshDInput(SynchronizationContext.Current ?? new SynchronizationContext());
            }

            if (gen.Hook.SDL2Enabled)
            {
                DevicesFunctions.RequestSdlRefresh();//on the SDL helper thread: the 1 s pause doesn't block the UI
            }

            if (gen.SupportsMultipleKeyboardsAndMice)
            {
                foreach (PlayerInfo raw in GameProfile.Instance.DevicesList.Where(d => (d.IsRawKeyboard || d.IsRawMouse) && d.ScreenIndex != -1).ToList())
                {
                    BoundsFunctions.Unassign(raw);
                }

                GameProfile.Instance.RefreshKeyboardAndMouse();
            }

            DevicesFunctions.UpdateDevices();
            session.Audio.Refresh();

            return session.CommitCommand();
        }

        public void Rumble(DeviceRequest request)
        {
            session.EnsureOpen();

            PlayerInfo device = Device(request.DeviceId);

            switch (device.InputType)
            {
                case InputType.XInput when device.XInputJoystick != null && !device.IsFake:
                    {
                        OpenXinputController controller = device.XInputJoystick;
                        controller.SetVibration(new Vibration { LeftMotorSpeed = 65535, RightMotorSpeed = 65535 });

                        Timer? stop = null;
                        stop = new Timer(_ =>
                        {
                            try
                            {
                                controller.SetVibration(new Vibration());
                            }
                            catch (Exception)
                            {
                            }

                            stop?.Dispose();
                        }, null, 300, Timeout.Infinite);
                        break;
                    }

                case InputType.SDL2 when !device.IsFake:
                    SDL.SDL2.SDL_GameControllerRumble(device.SDL2Joystick, 65535, 65535, 300);
                    break;

                default:
                    throw SetupSession.Conflict("rumbleNotSupported", "This device can't vibrate.");
            }
        }

        // ---------------------------------------------------------------- helpers

        private static PlayerInfo Device(string id)
        {
            return SetupSnapshotBuilder.FindDevice(id) ?? throw new BridgeException(BridgeErrorKind.NotFound, "Device not found.", new { deviceId = id });
        }

        /// <summary>The primary device of a player (a player id is the id of its first device; a partner's id is accepted too).</summary>
        private static PlayerInfo Player(string id)
        {
            PlayerInfo? device = SetupSnapshotBuilder.FindDevice(id);
            PlayerInfo? primary = device == null ? null : SetupSnapshotBuilder.PlayerOf(device);

            return primary ?? throw new BridgeException(BridgeErrorKind.NotFound, "Player not found.", new { playerId = id });
        }

        private static GameOption VisibleOption(string key)
        {
            return GameProfile.Game.Options.FirstOrDefault(o => o.Key == key && !o.Hidden)
                   ?? throw new BridgeException(BridgeErrorKind.NotFound, "Option not found.", new { key });
        }

        private static readonly Dictionary<string, string> RefusalMessages = new Dictionary<string, string>
        {
            [BoundsFunctions.Refusal.NoSetup] = "No game setup is open.",
            [BoundsFunctions.Refusal.InvalidScreen] = "There is no monitor with this index.",
            [BoundsFunctions.Refusal.InvalidSlot] = "There is no slot with this index.",
            [BoundsFunctions.Refusal.ManualLayout] = "The manual layout has no slots: place the player at a point.",
            [BoundsFunctions.Refusal.ProfileLocked] = "The loaded profile decides where players go.",
            [BoundsFunctions.Refusal.DeviceNotRevealed] = "Press a button on this device first.",
            [BoundsFunctions.Refusal.SlotOccupied] = "This place is taken.",
            [BoundsFunctions.Refusal.NotAssigned] = "This device is not placed.",
            [BoundsFunctions.Refusal.NotAGamepad] = "Guests must be gamepads, and so must their host.",
            [BoundsFunctions.Refusal.GuestsNotSupported] = "This game doesn't support several players per instance.",
            [BoundsFunctions.Refusal.GuestLimitReached] = "This instance can't take more guests.",
            [BoundsFunctions.Refusal.AlreadyGuest] = "This gamepad is already a guest.",
            [BoundsFunctions.Refusal.NotAGuest] = "This gamepad is not a guest.",
            [BoundsFunctions.Refusal.CannotExpand] = "This player can't grow or shrink here.",
            [BoundsFunctions.Refusal.CannotResize] = "This player can't be resized that way.",
            [BoundsFunctions.Refusal.NoFreeSlot] = "No free slot.",
            [BoundsFunctions.Refusal.NothingToPlace] = "No revealed device is waiting to be placed.",
            ["full"] = "Every slot is taken and no monitor can be split further.",
        };

        /// <summary>Turns a refusal reason of BoundsFunctions into a conflict error.</summary>
        private static void Refuse(string? reason)
        {
            if (reason != null)
            {
                throw SetupSession.Conflict(reason, RefusalMessages.TryGetValue(reason, out string message) ? message : reason);
            }
        }
    }
}
