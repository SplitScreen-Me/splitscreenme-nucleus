using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// "Before you launch" checks: game executable, handler update, the requirements of CheckGameRequirements
    /// (OneDrive Documents, game location for other-user launches, administrator rights, Nucleus location), devices and
    /// players. Blocking checks make <see cref="SetupFlow.CanPlay"/> false. Opening a setup no longer shows the old
    /// requirement message boxes: the same conditions are reported here.
    /// </summary>
    internal static class SetupChecks
    {
        public const string GameFound = "gameFound";
        public const string HandlerUpToDate = "handlerUpToDate";
        public const string OneDrive = "oneDrive";
        public const string GameLocation = "gameLocation";
        public const string Admin = "admin";
        public const string NucleusLocation = "nucleusLocation";
        public const string Devices = "devices";
        public const string Players = "players";
        public const string InputLock = "inputLock";

        private static readonly Lazy<bool> IsAdministrator = new Lazy<bool>(() =>
        {
            try
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        });

        //Program.ForcedBadPath of Nucleus.Coop.exe (the start checks found Nucleus in an unsupported folder).
        private static readonly Lazy<bool> ForcedBadPath = new Lazy<bool>(() =>
        {
            try
            {
                FieldInfo? field = Assembly.GetEntryAssembly()?.GetType("Nucleus.Coop.Program")?.GetField("ForcedBadPath", BindingFlags.Public | BindingFlags.Static);
                return field != null && (bool)field.GetValue(null);
            }
            catch (Exception)
            {
                return false;
            }
        });

        public static List<SetupCheck> Compute(SetupSession session)
        {
            List<SetupCheck> checks = new List<SetupCheck>();
            UserGameInfo? game = session.Game;
            GenericGameInfo? gen = GameProfile.Game;

            if (game == null || gen == null)
            {
                return checks;
            }

            string exePath = game.ExePath ?? "";
            checks.Add(File.Exists(exePath)
                ? Check(GameFound, CheckState.Ok, "Game found", exePath)
                : Check(GameFound, CheckState.Blocking, "Game not found", exePath, "Locate"));

            if (session.BusyChecks.Contains(HandlerUpToDate))
            {
                checks.Add(Check(HandlerUpToDate, CheckState.Busy, "Updating the handler"));
            }
            else
            {
                bool update = session.HandlerUpdater?.IsUpdateAvailable(gen.GUID) ?? gen.UpdateAvailable;
                int? latest = session.HandlerUpdater?.LatestVersion(gen.GUID);

                checks.Add(update
                    ? Check(HandlerUpToDate, CheckState.Warning, "Handler update available", latest.HasValue ? $"Version {latest}" : null, session.HandlerUpdater != null ? "Update" : null)
                    : Check(HandlerUpToDate, CheckState.Ok, "Handler up to date"));
            }

            bool documents = StartsWithDocuments(gen.UserProfileSavePath) || StartsWithDocuments(gen.UserProfileConfigPath) ||
                             StartsWithDocuments(gen.DocumentsSavePath) || StartsWithDocuments(gen.DocumentsConfigPath);

            if (Globals.IsOneDriveEnabled && gen.UseNucleusEnvironment && documents)
            {
                checks.Add(Check(OneDrive, CheckState.Blocking, "Documents folder in OneDrive", $"This handler can't run when the Documents folder is in OneDrive ({Globals.UserDocumentsRoot})."));
            }

            bool otherUsers = gen.LaunchAsDifferentUsers || gen.LaunchAsDifferentUsersAlt;

            if (otherUsers && (exePath.StartsWith(@"C:\Users\", StringComparison.OrdinalIgnoreCase) || exePath.StartsWith(@"C:\Windows\", StringComparison.OrdinalIgnoreCase)))
            {
                checks.Add(Check(GameLocation, CheckState.Blocking, "Unsupported game location",
                    "This handler doesn't support games installed in C:\\Users (Documents, Desktop, Downloads…) or C:\\Windows."));
            }

            bool needsAdmin = gen.RequiresAdmin || otherUsers || gen.ChangeIPPerInstanceAlt;

            if (needsAdmin)
            {
                checks.Add(IsAdministrator.Value
                    ? Check(Admin, CheckState.Ok, "Administrator rights")
                    : Check(Admin, CheckState.Blocking, "Administrator rights needed", "This handler requires Nucleus Co-op to run as administrator.", "Restart as administrator"));
            }

            if ((otherUsers || gen.ChangeIPPerInstanceAlt) && ForcedBadPath.Value && IsAdministrator.Value)
            {
                checks.Add(Check(NucleusLocation, CheckState.Blocking, "Unsupported Nucleus Co-op location",
                    "Install Nucleus Co-op outside game folders, C:\\Users and protected folders, for example C:\\Nucleus."));
            }

            List<PlayerInfo> devices = GameProfile.Instance?.DevicesList ?? new List<PlayerInfo>();
            int physical = devices.Count(d => !DeviceIds.IsVirtual(d.HIDDeviceID?.FirstOrDefault()));

            checks.Add(physical == 0
                ? Check(Devices, CheckState.Warning, "No input device", "Plug in a controller.")
                : Check(Devices, CheckState.Ok, physical == 1 ? "1 input device" : $"{physical} input devices"));

            if (SetupSession.PlayersValid())
            {
                int players = GameProfile.AssignedDevices.Count;
                checks.Add(Check(Players, CheckState.Ok, players == 1 ? "1 player" : $"{players} players"));
            }
            else
            {
                checks.Add(GameProfile.Loaded
                    ? Check(Players, CheckState.Blocking, "Waiting for the profile's players", $"{GameProfile.TotalAssignedPlayers} of {GameProfile.TotalProfilePlayers} matched.")
                    : Check(Players, CheckState.Blocking, "No player placed", "Place at least one player on a screen."));
            }

            //Replaces the message box GenericGameHandler.Play showed at every launch (WebUserPrompts skips it).
            if (!App_Misc.IgnoreInputLockReminder)
            {
                checks.Add(InputLockReminder());
            }

            //blocking first, then warnings, information, busy and fine checks (stable within a state)
            return checks.Select((c, i) => (c, i)).OrderBy(x => Rank(x.c.State)).ThenBy(x => x.i).Select(x => x.c).ToList();
        }

        /// <summary>The "lock inputs" reminder, until the user turns it off ([Misc] IgnoreInputLockReminder).</summary>
        internal static SetupCheck InputLockReminder()
        {
            string key = string.IsNullOrWhiteSpace(App_Hotkeys.LockInputs) ? "End" : App_Hotkeys.LockInputs;
            return Check(InputLock, CheckState.Info, $"Press {key} to lock inputs",
                $"Some handlers need the keyboard and mouse locked to the game windows. Press {key} again when you finish playing.", "Don't show again");
        }

        private static int Rank(CheckState state)
        {
            switch (state)
            {
                case CheckState.Blocking: return 0;
                case CheckState.Warning: return 1;
                case CheckState.Busy: return 2;
                case CheckState.Info: return 3;
                default: return 4;
            }
        }

        private static bool StartsWithDocuments(string? path) => path != null && path.StartsWith("Documents", StringComparison.Ordinal);

        private static SetupCheck Check(string id, CheckState state, string title, string? detail = null, string? action = null)
        {
            return new SetupCheck { Id = id, State = state, Title = title, Detail = detail, ActionLabel = action };
        }

        /// <summary>
        /// Starts Nucleus Co-op again as administrator (UAC prompt) and closes this instance, as CheckGameRequirements did.
        /// Returns false when the user refused the prompt.
        /// </summary>
        public static bool RestartAsAdministrator()
        {
            try
            {
                string exe = Assembly.GetEntryAssembly()?.Location ?? Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true, Verb = "runas" });
            }
            catch (Exception e)
            {
                ServiceLog.Info("setup: restart as administrator cancelled: " + e.Message);
                return false;
            }

            //The UAC prompt gave the new instance enough time; the single-instance check sees this one gone.
            Process.GetCurrentProcess().Kill();
            return true;
        }
    }
}
