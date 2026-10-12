using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nucleus.Coop.Services.Startup;

namespace Nucleus.Coop.Services.Library;

/// <summary>Cover and screenshot files of a game under <c>gui\</c> (lookup rules of <c>GameAssets.GetGameAssets</c>).</summary>
public static class GameAssetFiles
{
    private static readonly string[] ImageExtensions = { ".jpeg", ".jpg", ".png", ".bmp", ".gif" };

    /// <summary>Path the downloader writes the cover to.</summary>
    public static string CoverPath(string gameId) => Path.Combine(NucleusPaths.Covers, gameId + ".jpeg");

    /// <summary>Folder of the screenshots.</summary>
    public static string ScreenshotsFolder(string gameId) => Path.Combine(NucleusPaths.Screenshots, gameId);

    /// <summary>Path of screenshot <paramref name="index"/> as the downloader names it.</summary>
    public static string ScreenshotPath(string gameId, int index) => Path.Combine(ScreenshotsFolder(gameId), $"{index}_{gameId}.jpeg");

    /// <summary>The cover file: <c>&lt;GUID&gt;.jpeg</c>, else any image in <c>gui\covers</c> whose name contains the GUID.</summary>
    public static string? FindCover(string gameId)
    {
        string standard = CoverPath(gameId);
        if (File.Exists(standard))
        {
            return standard;
        }

        try
        {
            if (!Directory.Exists(NucleusPaths.Covers))
            {
                return null;
            }

            return Directory.EnumerateFiles(NucleusPaths.Covers)
                .FirstOrDefault(f => Path.GetFileName(f).IndexOf(gameId, StringComparison.OrdinalIgnoreCase) >= 0 && IsImage(f));
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Screenshot files, sorted by name.</summary>
    public static List<string> FindScreenshots(string gameId)
    {
        string folder = ScreenshotsFolder(gameId);
        try
        {
            return Directory.Exists(folder)
                ? Directory.EnumerateFiles(folder).Where(IsImage).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList()
                : new List<string>();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    /// <summary>Deletes cover and screenshots of a game (on removal). Returns false when a file is locked.</summary>
    public static bool Delete(string gameId)
    {
        bool ok = true;
        try
        {
            string? cover = FindCover(gameId);
            if (cover != null)
            {
                File.Delete(cover);
            }
        }
        catch (Exception e)
        {
            ok = false;
            ServiceLog.Warn($"cover of {gameId} could not be deleted", e);
        }

        try
        {
            string folder = ScreenshotsFolder(gameId);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch (Exception e)
        {
            ok = false;
            ServiceLog.Warn($"screenshots of {gameId} could not be deleted", e);
        }

        return ok;
    }

    private static bool IsImage(string path) => ImageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
}
