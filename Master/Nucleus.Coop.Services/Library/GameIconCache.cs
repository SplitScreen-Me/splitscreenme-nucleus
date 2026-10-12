using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Game icons for the web UI, extracted from <c>GetGameIcon</c>: the custom icon of the game (<c>metaInfo IconPath</c>,
/// an image or an .exe) or the executable's icon, rendered to <c>gui\icons\cache\&lt;GUID&gt;.png</c> on a worker
/// thread. A sidecar <c>&lt;GUID&gt;.src</c> remembers the source so a changed custom icon is rendered again.
/// </summary>
public sealed class GameIconCache
{
    private const int MaxSize = 128;

    private readonly LibraryNotifier notifier;
    private readonly ConcurrentDictionary<string, string> fresh = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> queued = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim renderSlots = new SemaphoreSlim(2, 2);

    /// <summary>Creates the cache.</summary>
    public GameIconCache(LibraryNotifier notifier)
    {
        this.notifier = notifier;
    }

    /// <summary>
    /// Icon URL for a game. When the cached PNG is missing or stale it is rendered in the background (the previous
    /// PNG, or the default icon, is returned meanwhile) and <c>library.changed</c> follows.
    /// </summary>
    public string? GetUrl(string gameId, string? exePath, string? customIconPath)
    {
        string? source = Describe(exePath, customIconPath);
        if (source == null)
        {
            return DefaultIconUrl();
        }

        string png = PngPath(gameId);
        if (!(fresh.TryGetValue(gameId, out string? known) && known == source) && !IsFresh(gameId, source))
        {
            Queue(gameId, source);
        }

        return NucleusPaths.AssetUrl(png) ?? DefaultIconUrl();
    }

    /// <summary>Forgets the cached icon (custom icon changed) and renders it again.</summary>
    public void Refresh(string gameId, string? exePath, string? customIconPath)
    {
        fresh.TryRemove(gameId, out _);
        string? source = Describe(exePath, customIconPath);
        if (source != null)
        {
            Queue(gameId, source);
        }
    }

    /// <summary>Deletes the cached icon of a removed game.</summary>
    public void Delete(string gameId)
    {
        fresh.TryRemove(gameId, out _);
        TryDelete(PngPath(gameId));
        TryDelete(SourcePath(gameId));
    }

    /// <summary>URL of <c>gui\icons\default.png</c>.</summary>
    public static string? DefaultIconUrl() => NucleusPaths.AssetUrl(Path.Combine(NucleusPaths.Icons, "default.png"));

    /// <summary>Renders an icon source (<c>custom:&lt;path&gt;</c> or <c>exe:&lt;path&gt;</c>) to a bitmap of at most 128 px.</summary>
    public static Bitmap? Render(string source)
    {
        int colon = source.IndexOf(':');
        string kind = source.Substring(0, colon);
        string path = source.Substring(colon + 1);
        string extension = Path.GetExtension(path).ToLowerInvariant();

        if (kind == "exe" || extension == ".exe")
        {
            return ExtractExecutableIcon(path);
        }

        if (extension == ".ico")
        {
            using var icon = new Icon(path, 256, 256);
            using Bitmap bitmap = icon.ToBitmap();
            return Fit(bitmap);
        }

        // Read through a memory stream so the user's file is not kept locked.
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using var image = Image.FromStream(stream);
        return Fit(image);
    }

    internal static string? Describe(string? exePath, string? customIconPath)
    {
        if (!string.IsNullOrEmpty(customIconPath) && File.Exists(customIconPath))
        {
            return "custom:" + customIconPath;
        }

        return !string.IsNullOrEmpty(exePath) && File.Exists(exePath) ? "exe:" + exePath : null;
    }

    private static string PngPath(string gameId) => Path.Combine(NucleusPaths.IconCache, gameId + ".png");

    private static string SourcePath(string gameId) => Path.Combine(NucleusPaths.IconCache, gameId + ".src");

    private bool IsFresh(string gameId, string source)
    {
        try
        {
            string png = PngPath(gameId);
            string sidecar = SourcePath(gameId);
            if (!File.Exists(png) || !File.Exists(sidecar) || File.ReadAllText(sidecar) != source)
            {
                return false;
            }

            string sourceFile = source.Substring(source.IndexOf(':') + 1);
            if (File.GetLastWriteTimeUtc(png) < File.GetLastWriteTimeUtc(sourceFile))
            {
                return false;
            }

            fresh[gameId] = source;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void Queue(string gameId, string source)
    {
        if (!queued.TryAdd(gameId, 0))
        {
            return;
        }

        Task.Run(async () =>
        {
            await renderSlots.WaitAsync().ConfigureAwait(false);
            try
            {
                RenderToCache(gameId, source);
                fresh[gameId] = source;
                notifier.GamesChanged(gameId);
            }
            catch (Exception e)
            {
                ServiceLog.Warn($"icon of {gameId} could not be rendered from {source}", e);
            }
            finally
            {
                renderSlots.Release();
                queued.TryRemove(gameId, out _);
            }
        });
    }

    private static void RenderToCache(string gameId, string source)
    {
        Directory.CreateDirectory(NucleusPaths.IconCache);
        using Bitmap? bitmap = Render(source);
        if (bitmap == null)
        {
            throw new InvalidOperationException("no icon");
        }

        string png = PngPath(gameId);
        string temp = png + ".tmp";
        bitmap.Save(temp, ImageFormat.Png);
        if (File.Exists(png))
        {
            File.Replace(temp, png, null);
        }
        else
        {
            File.Move(temp, png);
        }

        File.WriteAllText(SourcePath(gameId), source);
    }

    private static Bitmap? ExtractExecutableIcon(string path)
    {
        // The largest icon of the executable (up to 128 px), then the shell's 32 px icon as the legacy app did.
        var handles = new IntPtr[1];
        var ids = new uint[1];
        try
        {
            uint count = PrivateExtractIcons(path, 0, MaxSize, MaxSize, handles, ids, 1, 0);
            if (count != 0 && count != uint.MaxValue && handles[0] != IntPtr.Zero)
            {
                using var icon = Icon.FromHandle(handles[0]);
                using Bitmap bitmap = icon.ToBitmap();
                return Fit(bitmap);
            }
        }
        finally
        {
            if (handles[0] != IntPtr.Zero)
            {
                DestroyIcon(handles[0]);
            }
        }

        using Icon? shellIcon = Shell32.GetIcon(path, false);
        if (shellIcon == null)
        {
            return null;
        }

        using Bitmap shellBitmap = shellIcon.ToBitmap();
        return Fit(shellBitmap);
    }

    private static Bitmap Fit(Image image)
    {
        double scale = Math.Min(1.0, Math.Min((double)MaxSize / image.Width, (double)MaxSize / image.Height));
        int width = Math.Max(1, (int)Math.Round(image.Width * scale));
        int height = Math.Max(1, (int)Math.Round(image.Height * scale));
        var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.Clear(Color.Transparent);
        g.DrawImage(image, 0, 0, width, height);
        return result;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception)
        {
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint PrivateExtractIcons(string szFileName, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, uint[] piconid, uint nIcons, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
