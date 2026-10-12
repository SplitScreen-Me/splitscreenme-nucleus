using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nucleus.Bridge.Contracts;

namespace Nucleus.Coop.Services.Settings;

/// <summary>
/// Validation of the global player nicknames and Steam IDs (Players tab of the legacy Settings form), done before
/// anything is saved. Pure.
/// </summary>
public static class PlayerIdentitiesRules
{
    /// <summary>Player slots.</summary>
    public const int MaxPlayers = 32;

    /// <summary>Nickname length limit of the legacy fields.</summary>
    public const int MaxNicknameLength = 9;

    private static readonly Regex SteamIdPattern = new Regex("^[0-9]{17}$", RegexOptions.CultureInvariant);

    /// <summary>Default nickname of a slot (1-based), as <c>PlayersIdentityCache</c> generates it.</summary>
    public static string DefaultNickname(int number) => "Player" + number;

    /// <summary>Default Steam ID of a slot (1-based), as <c>PlayersIdentityCache</c> generates it.</summary>
    public static string DefaultSteamId(int number) => (76561199023125438L + number - 1).ToString();

    /// <summary>
    /// Validates the submitted slots merged over the current ones (index 0 = player 1). Returns the errors (index =
    /// position in <paramref name="submitted"/>) and the 32 resulting identities with empty nicknames replaced by the
    /// default; empty Steam IDs stay empty (the default is used at launch).
    /// </summary>
    public static (List<ValidationError> Errors, List<PlayerIdentity> Result) Validate(IReadOnlyList<PlayerIdentity> submitted, IReadOnlyList<PlayerIdentity> current)
    {
        var errors = new List<ValidationError>();
        var result = Enumerable.Range(1, MaxPlayers).Select(n =>
        {
            PlayerIdentity? existing = current.FirstOrDefault(c => c.Number == n);
            return new PlayerIdentity
            {
                Number = n,
                Nickname = string.IsNullOrWhiteSpace(existing?.Nickname) ? DefaultNickname(n) : existing!.Nickname.Trim(),
                SteamId = existing?.SteamId?.Trim() ?? "",
            };
        }).ToList();

        var submittedIndex = new Dictionary<int, int>(); // player number -> index in the request
        for (int i = 0; i < submitted.Count; i++)
        {
            PlayerIdentity player = submitted[i];
            if (player == null || player.Number < 1 || player.Number > MaxPlayers)
            {
                errors.Add(new ValidationError { Field = "number", Index = i, Message = $"Player numbers go from 1 to {MaxPlayers}." });
                continue;
            }

            if (submittedIndex.ContainsKey(player.Number))
            {
                errors.Add(new ValidationError { Field = "number", Index = i, Message = $"Player {player.Number} is listed twice." });
                continue;
            }

            submittedIndex[player.Number] = i;
            PlayerIdentity slot = result[player.Number - 1];

            string nickname = (player.Nickname ?? "").Trim();
            string? nicknameError = CheckNickname(nickname);
            if (nicknameError != null)
            {
                errors.Add(new ValidationError { Field = "nickname", Index = i, Message = nicknameError });
            }
            else
            {
                slot.Nickname = nickname.Length == 0 ? DefaultNickname(player.Number) : nickname;
            }

            string steamId = (player.SteamId ?? "").Trim();
            if (steamId.Length > 0 && !SteamIdPattern.IsMatch(steamId))
            {
                errors.Add(new ValidationError { Field = "steamId", Index = i, Message = "A Steam ID is 17 digits, for example 76561199075562883." });
            }
            else
            {
                slot.SteamId = steamId;
            }
        }

        // Duplicates (case-insensitive nicknames: they become folder names; effective Steam IDs: empty means the default).
        ReportDuplicates(result, p => p.Nickname.ToUpperInvariant(), "nickname", "This nickname is already used by player {0}.", submittedIndex, errors);
        ReportDuplicates(result, p => p.SteamId.Length == 0 ? DefaultSteamId(p.Number) : p.SteamId, "steamId", "This Steam ID is already used by player {0}.", submittedIndex, errors);

        return (errors, result);
    }

    private static string? CheckNickname(string nickname)
    {
        if (nickname.Length > MaxNicknameLength)
        {
            return $"Use at most {MaxNicknameLength} characters.";
        }

        if (nickname.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return "Nicknames are used as folder names: avoid \\ / : * ? \" < > |.";
        }

        if (!SettingsValidator.FitsIniCodePage(nickname))
        {
            return "Some characters can't be saved in Settings.ini on this PC (it uses the system code page).";
        }

        return null;
    }

    private static void ReportDuplicates(List<PlayerIdentity> players, Func<PlayerIdentity, string> identity, string field, string message, Dictionary<int, int> submittedIndex, List<ValidationError> errors)
    {
        var firstByValue = new Dictionary<string, PlayerIdentity>();
        foreach (PlayerIdentity player in players)
        {
            string value = identity(player);
            if (!firstByValue.TryGetValue(value, out PlayerIdentity? first))
            {
                firstByValue[value] = player;
                continue;
            }

            // Report on a submitted slot of the pair; pre-existing duplicates between untouched slots are left alone.
            if (submittedIndex.TryGetValue(player.Number, out int index))
            {
                errors.Add(new ValidationError { Field = field, Index = index, Message = string.Format(message, first.Number) });
            }
            else if (submittedIndex.TryGetValue(first.Number, out int firstIndex))
            {
                errors.Add(new ValidationError { Field = field, Index = firstIndex, Message = string.Format(message, player.Number) });
            }
        }
    }
}
