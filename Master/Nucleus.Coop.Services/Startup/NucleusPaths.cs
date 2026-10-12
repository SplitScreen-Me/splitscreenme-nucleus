using System;
using System.IO;
using System.Linq;

namespace Nucleus.Coop.Services.Startup;

/// <summary>
/// Install folders used by the library, handlers and settings services, and the URLs the web UI loads local images
/// from (WebView2 virtual hosts set up by the host window).
/// </summary>
public static class NucleusPaths
{
    /// <summary>Virtual host mapped to <c>&lt;install&gt;\gui\</c>.</summary>
    public const string AssetsBaseUrl = "https://assets.nucleus.example/";

    /// <summary>Virtual host mapped to <c>&lt;install&gt;\handlers\</c>.</summary>
    public const string HandlersBaseUrl = "https://handlers.nucleus.example/";

    private static string? installRootOverride;

    /// <summary>Folder of NucleusCoop.exe: <see cref="Nucleus.Gaming.InstallPaths.Root"/> (also <c>Globals.NucleusInstallRoot</c>).</summary>
    public static string InstallRoot => installRootOverride ?? Nucleus.Gaming.InstallPaths.Root;

    /// <summary><c>gui\</c>: covers, screenshots, icons, gamepad button images, shortcut reminder images.</summary>
    public static string Gui => Path.Combine(InstallRoot, "gui");

    /// <summary><c>gui\covers\</c>, one <c>&lt;GUID&gt;.jpeg</c> per game.</summary>
    public static string Covers => Path.Combine(Gui, "covers");

    /// <summary><c>gui\screenshots\&lt;GUID&gt;\</c>.</summary>
    public static string Screenshots => Path.Combine(Gui, "screenshots");

    /// <summary><c>gui\icons\</c>.</summary>
    public static string Icons => Path.Combine(Gui, "icons");

    /// <summary><c>gui\icons\cache\</c>: game icons rendered to PNG for the web UI.</summary>
    public static string IconCache => Path.Combine(Icons, "cache");

    /// <summary><c>gui\shortcuts\</c>: images shown by the in-game shortcuts reminder.</summary>
    public static string Shortcuts => Path.Combine(Gui, "shortcuts");

    /// <summary><c>handlers\</c> (same as <c>GameManager.GetJsScriptsPath()</c>).</summary>
    public static string Handlers => Path.Combine(InstallRoot, "handlers");

    /// <summary><c>game profiles\</c>.</summary>
    public static string GameProfiles => Path.Combine(InstallRoot, "game profiles");

    /// <summary><c>gui\gamepads\</c>: gamepad button images of the shortcuts reminder, one folder per gamepad type.</summary>
    public static string GamepadImages => Path.Combine(Gui, "gamepads");

    /// <summary>Tests only: makes every path relative to another folder.</summary>
    internal static void OverrideInstallRoot(string? root) => installRootOverride = root?.TrimEnd('\\');

    /// <summary>
    /// URL of a file under <c>gui\</c>, with a <c>?v=</c> cache-busting query (last write time), or null when the
    /// file does not exist or is outside <c>gui\</c>.
    /// </summary>
    public static string? AssetUrl(string fullPath) => FileUrl(fullPath, Gui, AssetsBaseUrl);

    /// <summary>Same as <see cref="AssetUrl"/> for files under <c>handlers\</c>.</summary>
    public static string? HandlerFileUrl(string fullPath) => FileUrl(fullPath, Handlers, HandlersBaseUrl);

    internal static string? FileUrl(string fullPath, string root, string baseUrl)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        string full;
        try
        {
            full = Path.GetFullPath(fullPath);
        }
        catch (Exception)
        {
            return null;
        }

        string rootFull = Path.GetFullPath(root).TrimEnd('\\') + "\\";
        if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
        {
            return null;
        }

        string relative = full.Substring(rootFull.Length);
        string escaped = string.Join("/", relative.Split('\\').Select(Uri.EscapeDataString));
        long version = File.GetLastWriteTimeUtc(full).Ticks;
        return baseUrl + escaped + "?v=" + version;
    }
}
