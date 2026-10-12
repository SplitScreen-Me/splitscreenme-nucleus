using System;
using System.IO;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Platform.PCSpecs;

namespace Nucleus.Coop.Services.Library;

/// <summary>Builds the library DTOs from GameManager objects. Call on the UI thread.</summary>
public sealed class GameDtoFactory
{
    private readonly GameIconCache icons;
    private readonly Func<string, int?> latestVersion;

    /// <summary>Creates the factory. <paramref name="latestVersion"/> gives the hub version found by update checks.</summary>
    public GameDtoFactory(GameIconCache icons, Func<string, int?> latestVersion)
    {
        this.icons = icons;
        this.latestVersion = latestVersion;
    }

    /// <summary>Input hooks of a handler.</summary>
    public static HandlerInputFlags Flags(GenericGameInfo game) => new HandlerInputFlags
    {
        XInputEnabled = game.Hook?.XInputEnabled ?? false,
        XInputReroute = game.Hook?.XInputReroute ?? false,
        DInputEnabled = game.Hook?.DInputEnabled ?? false,
        Sdl2Enabled = game.Hook?.SDL2Enabled ?? false,
        ProtoXinputHook = game.ProtoInput?.XinputHook ?? false,
        ProtoDinputDeviceHook = game.ProtoInput?.DinputDeviceHook ?? false,
        SupportsKeyboard = game.SupportsKeyboard,
        SupportsMultipleKeyboardsAndMice = game.SupportsMultipleKeyboardsAndMice,
    };

    /// <summary>The list row of a library game.</summary>
    public GameSummary Summary(UserGameInfo entry)
    {
        GenericGameInfo game = entry.Game;
        GameMetaInfo meta = game.MetaInfo;
        string? cover = GameAssetFiles.FindCover(game.GUID);
        string? hero = GameAssetFiles.FindScreenshots(game.GUID).FirstOrDefault();

        return new GameSummary
        {
            Id = game.GUID,
            Name = game.GameName ?? game.GUID,
            HandlerId = string.IsNullOrWhiteSpace(game.Hub?.Handler?.Id) ? null : game.Hub!.Handler.Id,
            HandlerVersion = game.Hub?.Handler != null && game.Hub.Handler.Version >= 0 ? game.Hub.Handler.Version : (int?)null,
            MaxPlayers = game.MaxPlayers,
            Inputs = GameInputRules.ToInputs(Flags(game)),
            Favorite = meta.Favorite,
            UpdateAvailable = game.UpdateAvailable && meta.CheckUpdate,
            LatestHandlerVersion = latestVersion(game.GUID),
            HeroUrl = hero == null ? null : NucleusPaths.AssetUrl(hero),
            IconUrl = icons.GetUrl(game.GUID, entry.ExePath, meta.IconPath),
            CoverUrl = cover == null ? null : NucleusPaths.AssetUrl(cover),
            PlaytimeSeconds = PlaytimeParser.ParseSeconds(meta.TotalPlayTimeRaw),
            LastPlayedAt = PlaytimeParser.ParseLastPlayed(meta.LastPlayedAtRaw),
            FirstLaunch = meta.FirstLaunch,
            ExecutableFound = !string.IsNullOrEmpty(entry.ExePath) && File.Exists(entry.ExePath),
        };
    }

    /// <summary>The game page of a library game.</summary>
    public GameDetails Details(UserGameInfo entry)
    {
        GenericGameInfo game = entry.Game;
        GameMetaInfo meta = game.MetaInfo;
        bool profilesEnabled = !App_Misc.DisableGameProfiles && !meta.DisableProfiles;
        string description = game.Description ?? "";

        return new GameDetails
        {
            Summary = Summary(entry),
            Notes = string.IsNullOrEmpty(description) ? null : description,
            ScreenshotUrls = GameAssetFiles.FindScreenshots(game.GUID).Select(NucleusPaths.AssetUrl).Where(u => u != null).Select(u => u!).ToList(),
            ExecutablePath = entry.ExePath ?? "",
            Architecture = Architecture(entry.ExePath),
            HandlerFile = game.JsFileName ?? "",
            Maintainer = string.IsNullOrWhiteSpace(game.Hub?.Maintainer?.Name) ? null : game.Hub!.Maintainer.Name,
            SteamId = string.IsNullOrWhiteSpace(game.SteamID) ? null : game.SteamID,
            ContentFolder = Path.Combine(GameManager.Instance.GetAppContentPath(), game.GUID),
            ProfilesEnabled = profilesEnabled,
            ProfileCount = CountProfiles(game.GUID),
            HasOptions = (game.Options?.Count ?? 0) > 0 || (game.CustomSteps?.Count ?? 0) > 0,
            ShowNotesFirst = meta.FirstLaunch && !App_Misc.DisableForcedNote && description.Length > 0,
        };
    }

    private static string Architecture(string? exePath)
    {
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            return "unknown";
        }

        try
        {
            bool? x64 = MachineSpecs.GetMachineArch(exePath);
            return x64 == true ? "x64" : x64 == false ? "x86" : "unknown";
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    private static int CountProfiles(string gameId)
    {
        try
        {
            string folder = Path.Combine(NucleusPaths.GameProfiles, gameId);
            return Directory.Exists(folder) ? Directory.GetFiles(folder, "Profile*.json").Length : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
