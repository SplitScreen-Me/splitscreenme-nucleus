using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Handlers;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Downloads a game's cover and up to 5 screenshots from IGDB through the hub, extracted from
/// <c>AssetsDownloader.DownloadCovers/DownloadScreenshots</c>: same files (<c>gui\covers\&lt;GUID&gt;.jpeg</c>,
/// <c>gui\screenshots\&lt;GUID&gt;\&lt;i&gt;_&lt;GUID&gt;.jpeg</c>), files that already exist are kept, without UI
/// calls, with cancellation and atomic writes.
/// </summary>
public sealed class ArtworkDownloader
{
    /// <summary>Screenshots kept per game (legacy value).</summary>
    public const int MaxScreenshots = 5;

    private readonly HubClient hub;

    /// <summary>Creates the downloader.</summary>
    public ArtworkDownloader(HubClient hub)
    {
        this.hub = hub;
    }

    /// <summary>Downloads what is missing. Returns whether a file was written.</summary>
    public async Task<bool> DownloadAsync(string gameId, string handlerId, CancellationToken cancellationToken)
    {
        bool wrote = false;

        HubHandlerInfo? handler = await hub.GetHandlerAsync(handlerId, cancellationToken).ConfigureAwait(false);
        if (handler != null && !string.IsNullOrEmpty(handler.GameCover) && GameAssetFiles.FindCover(gameId) == null)
        {
            byte[] cover = await hub.GetBytesAsync(HubClient.CoverUrl(handler.GameCover), cancellationToken).ConfigureAwait(false);
            Directory.CreateDirectory(Path.GetDirectoryName(GameAssetFiles.CoverPath(gameId))!);
            SaveAsJpeg(cover, GameAssetFiles.CoverPath(gameId));
            wrote = true;
        }

        List<string> screenshots = await hub.GetScreenshotUrlsAsync(handlerId, cancellationToken).ConfigureAwait(false);
        for (int i = 0; i < Math.Min(MaxScreenshots, screenshots.Count); i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string target = GameAssetFiles.ScreenshotPath(gameId, i);
            if (File.Exists(target))
            {
                continue;
            }

            byte[] image = await hub.GetBytesAsync(HubClient.ScreenshotUrl(screenshots[i], "t_original"), cancellationToken).ConfigureAwait(false);
            Directory.CreateDirectory(GameAssetFiles.ScreenshotsFolder(gameId));
            SaveAsJpeg(image, target);
            wrote = true;
        }

        return wrote;
    }

    /// <summary>Writes image bytes as a JPEG file (as is when they already are a JPEG), through a temporary file.</summary>
    public static void SaveAsJpeg(byte[] bytes, string path)
    {
        string temp = path + ".tmp";
        using (var input = new MemoryStream(bytes))
        using (Image image = Image.FromStream(input)) // throws ArgumentException when it is not an image
        {
            if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid)
            {
                File.WriteAllBytes(temp, bytes);
            }
            else
            {
                image.Save(temp, ImageFormat.Jpeg);
            }
        }

        if (File.Exists(path))
        {
            File.Replace(temp, path, null);
        }
        else
        {
            File.Move(temp, path);
        }
    }
}
