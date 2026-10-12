using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;

namespace Nucleus.Coop.Services.Session
{
    /// <summary>
    /// The summary of a finished session (<see cref="SessionSnapshot.LastSession"/>): the players come from the setup
    /// snapshot taken at Play (it names the devices; the launch players don't), the errors from the session log. Pure.
    /// </summary>
    internal static class SessionSummaryBuilder
    {
        /// <summary>Most error lines kept in a summary.</summary>
        public const int MaxErrors = 20;

        /// <summary>The placed players of a setup snapshot, in player order, with their devices and guests.</summary>
        public static List<SessionSummaryPlayer> Players(SetupSnapshot snapshot)
        {
            Dictionary<string, SetupDevice> devices = new Dictionary<string, SetupDevice>();

            foreach (SetupDevice device in snapshot.Devices)
            {
                if (!devices.ContainsKey(device.Id))
                {
                    devices.Add(device.Id, device);
                }
            }

            SessionSummaryDevice? Device(string id, bool guest) =>
                devices.TryGetValue(id, out SetupDevice device)
                    ? new SessionSummaryDevice { Kind = device.Kind, Name = device.Name, Guest = guest }
                    : null;

            return snapshot.Players
                .OrderBy(p => p.Number)
                .Select(p => new SessionSummaryPlayer
                {
                    Number = p.Number,
                    Nickname = string.IsNullOrWhiteSpace(p.Nickname) ? null : p.Nickname,
                    ScreenIndex = p.ScreenIndex,
                    Devices = p.DeviceIds.Select(id => Device(id, false))
                        .Concat(p.GuestDeviceIds.Select(id => Device(id, true)))
                        .Where(d => d != null)
                        .Select(d => d!)
                        .ToList(),
                })
                .ToList();
        }

        /// <summary>The summary of a session that just ended.</summary>
        public static SessionSummary Build(
            string gameId,
            string gameName,
            DateTime startedAt,
            DateTime endedAt,
            bool launched,
            IEnumerable<SessionSummaryPlayer> players,
            string? error,
            IEnumerable<SessionLogEntry> log)
        {
            List<SessionLogEntry> errors = log.Where(e => e.Level == SessionLogLevel.Error).ToList();

            return new SessionSummary
            {
                GameId = gameId,
                GameName = gameName,
                StartedAt = startedAt,
                EndedAt = endedAt < startedAt ? startedAt : endedAt,
                Launched = launched,
                Players = players.ToList(),
                Error = string.IsNullOrWhiteSpace(error) ? null : error,
                Errors = errors.Take(MaxErrors).ToList(),
                ErrorCount = errors.Count,
            };
        }
    }
}
