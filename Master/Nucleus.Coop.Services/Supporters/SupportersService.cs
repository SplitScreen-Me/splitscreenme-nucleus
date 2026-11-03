using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Supporters;

/// <summary>
/// <c>supporters.get</c>: downloads the Patreon supporters list published on GitHub, saves it to
/// <see cref="NucleusPaths.SupportersCache"/>, and answers with the saved copy when the download fails (offline, timeout,
/// an empty or unreadable list). The legacy credits window (<c>Patrons.cs</c>) worked the same way.
/// </summary>
public sealed class SupportersService : ISupportersService, IDisposable
{
    /// <summary>The published list: one <c>&lt;div&gt;Name&lt;/div&gt;</c> per line.</summary>
    public const string ListUrl = "https://raw.githubusercontent.com/SplitScreen-Me/splitscreenme-patreon/refs/heads/main/tiers.txt";

    /// <summary>A larger answer is not a names list (about 12 KB in 2026).</summary>
    public const int MaxDownloadBytes = 1024 * 1024;

    private readonly HttpClient http;
    private readonly string listUrl;
    private readonly string cacheFile;
    private readonly Func<string?> musicUrl;

    /// <summary>
    /// Creates the service. Defaults: <see cref="ListUrl"/>, <see cref="NucleusPaths.SupportersCache"/>, the system HTTP
    /// stack with certificate validation, and the music of <see cref="NucleusPaths.CreditsMusic"/>. Tests pass the others.
    /// </summary>
    public SupportersService(string? listUrl = null, string? cacheFile = null, HttpMessageHandler? handler = null, Func<string?>? musicUrl = null)
    {
        this.listUrl = listUrl ?? ListUrl;
        this.cacheFile = cacheFile ?? NucleusPaths.SupportersCache;
        this.musicUrl = musicUrl ?? (() => NucleusPaths.AssetUrl(NucleusPaths.CreditsMusic));

        if (handler == null)
        {
            HubClient.EnsureTls12();
            handler = new WebRequestHandler
            {
                // Per request: the process-wide accept-all callback set by legacy code does not apply here.
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
        }

        http = new HttpClient(handler, disposeHandler: true) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("NucleusCoop/" + Globals.Version);
    }

    /// <summary>The download gives up after this long and the saved copy is used.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <inheritdoc />
    public async Task<SupportersList> Get(CancellationToken cancellationToken)
    {
        List<string>? names = await DownloadAsync(cancellationToken).ConfigureAwait(false);
        if (names != null)
        {
            Save(names);
            return new SupportersList { Names = names, MusicUrl = musicUrl() };
        }

        if (Load() is { } saved)
        {
            return new SupportersList { Names = saved.Names, FromCache = true, SavedAt = saved.SavedAt, MusicUrl = musicUrl() };
        }

        throw new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.supporters.errors.unavailable"));
    }

    /// <summary>The names of the published list, or null when it can't be read (logged). Cancellation by the caller throws.</summary>
    private async Task<List<string>?> DownloadAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);
        try
        {
            using HttpResponseMessage response = await http.GetAsync(listUrl, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                ServiceLog.Warn($"Supporters list: HTTP {(int)response.StatusCode} from {listUrl}");
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxDownloadBytes)
            {
                ServiceLog.Warn($"Supporters list: {response.Content.Headers.ContentLength} bytes is too large for a names list");
                return null;
            }

            string text;
            using (Stream body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                text = await ReadTextAsync(body, timeout.Token).ConfigureAwait(false) ?? "";
                if (text.Length == 0)
                {
                    ServiceLog.Warn("Supporters list: empty or too large");
                    return null;
                }
            }

            List<string> names = SupporterNames.Parse(text);
            if (names.Count == 0)
            {
                ServiceLog.Warn("Supporters list: no names in the answer");
                return null;
            }

            return names;
        }
        catch (Exception e) when (!cancellationToken.IsCancellationRequested && e is HttpRequestException or OperationCanceledException or IOException or WebException)
        {
            ServiceLog.Warn(timeout.IsCancellationRequested ? $"Supporters list: no answer in {Timeout.TotalSeconds:0} s" : "Supporters list: download failed", e);
            return null;
        }
    }

    /// <summary>The body as UTF-8 text (a byte order mark is skipped), or null when it is larger than <see cref="MaxDownloadBytes"/>.</summary>
    private static async Task<string?> ReadTextAsync(Stream body, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await body.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > MaxDownloadBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        byte[] bytes = buffer.ToArray();
        int start = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        return new UTF8Encoding(false, false).GetString(bytes, start, bytes.Length - start);
    }

    /// <summary>Saves the names, one per line, replacing the previous copy in one step. A failure is only logged.</summary>
    private void Save(List<string> names)
    {
        string temp = cacheFile + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
            File.WriteAllText(temp, string.Join("\r\n", names) + "\r\n", new UTF8Encoding(false));
            if (File.Exists(cacheFile))
            {
                File.Replace(temp, cacheFile, null);
            }
            else
            {
                File.Move(temp, cacheFile);
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ServiceLog.Warn("Supporters list: could not save " + cacheFile, e);
            TryDelete(temp);
        }
    }

    /// <summary>The saved copy, or null when there is none (or nothing readable in it).</summary>
    private Saved? Load()
    {
        try
        {
            if (!File.Exists(cacheFile))
            {
                return null;
            }

            List<string> names = SupporterNames.Parse(File.ReadAllText(cacheFile, Encoding.UTF8));
            return names.Count == 0 ? null : new Saved(names, File.GetLastWriteTimeUtc(cacheFile));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ServiceLog.Warn("Supporters list: could not read " + cacheFile, e);
            return null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    /// <inheritdoc />
    public void Dispose() => http.Dispose();

    private sealed class Saved
    {
        public Saved(List<string> names, DateTime savedAt)
        {
            Names = names;
            SavedAt = savedAt;
        }

        public List<string> Names { get; }

        public DateTime SavedAt { get; }
    }
}
