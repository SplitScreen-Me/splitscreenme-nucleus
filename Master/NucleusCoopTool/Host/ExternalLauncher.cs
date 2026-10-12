using System;
using System.Diagnostics;
using System.IO;

namespace Nucleus.Coop.Host
{
    /// <summary>Opens URLs in the default browser and paths in File Explorer, with validation.</summary>
    internal static class ExternalLauncher
    {
        /// <summary>Parses an absolute http or https URL. Returns null for anything else.</summary>
        public static Uri ParseWebUrl(string url)
        {
            Uri uri;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri))
            {
                return null;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps ? uri : null;
        }

        /// <summary>Opens an http(s) URL in the default browser. Returns false when the URL is rejected or the shell fails.</summary>
        public static bool TryOpenUrl(string url)
        {
            Uri uri = ParseWebUrl(url);
            if (uri == null)
            {
                HostLog.Warning($"Refused to open a non-http(s) URL: {url}");
                return false;
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not open {uri.AbsoluteUri} in the default browser: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Normalizes an absolute local path. Returns null when the path is relative, malformed or contains quotes.
        /// </summary>
        public static string NormalizeLocalPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.IndexOf('"') >= 0 || !Path.IsPathRooted(path))
            {
                return null;
            }

            try
            {
                return Path.GetFullPath(path.Trim());
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Opens a folder, or selects a file, in File Explorer. Never executes the file.</summary>
        public static void OpenInExplorer(string fullPath)
        {
            string arguments = Directory.Exists(fullPath)
                ? "\"" + fullPath + "\""
                : "/select,\"" + fullPath + "\"";

            Process.Start(new ProcessStartInfo("explorer.exe", arguments) { UseShellExecute = true });
        }
    }
}
