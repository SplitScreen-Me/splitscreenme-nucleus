using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>The hub could not be reached, timed out or answered with an error.</summary>
public sealed class HubUnavailableException : Exception
{
    /// <summary>Creates the exception.</summary>
    public HubUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

/// <summary>A handler record of the hub API (<c>GET /api/v1/handler/{id}</c>), the fields the app uses.</summary>
public sealed class HubHandlerInfo
{
    /// <summary>Hub handler id (<c>_id</c>).</summary>
    public string Id { get; set; } = "";

    /// <summary>Game name.</summary>
    public string GameName { get; set; } = "";

    /// <summary>IGDB image id of the cover, may be empty.</summary>
    public string GameCover { get; set; } = "";

    /// <summary>Current version number.</summary>
    public int CurrentVersion { get; set; } = -1;

    /// <summary>Id of the current package in the hub storage.</summary>
    public string CurrentPackage { get; set; } = "";
}

/// <summary>
/// HTTP access to hub.splitscreen.me and the IGDB image CDN for the services. Unlike the legacy helpers it keeps the
/// default TLS certificate validation (set per request, the process-wide accept-all callback set elsewhere is not
/// used), and every call has a timeout.
/// </summary>
public sealed class HubClient : IDisposable
{
    /// <summary>Hub site root (connectivity check).</summary>
    public const string SiteUrl = "https://hub.splitscreen.me/";

    /// <summary>Hub API root.</summary>
    public const string ApiBase = "https://hub.splitscreen.me/api/v1/";

    private static readonly Regex HubIdPattern = new Regex("^[A-Za-z0-9]{1,64}$", RegexOptions.CultureInvariant);

    private readonly HttpClient http;

    /// <summary>Creates the client. <paramref name="handler"/> is for tests.</summary>
    public HubClient(HttpMessageHandler? handler = null)
    {
        EnsureTls12();

        if (handler == null)
        {
            handler = new WebRequestHandler
            {
                // Per-request callback: overrides ServicePointManager.ServerCertificateValidationCallback (accept-all,
                // set by legacy code) and keeps the default validation result.
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) => errors == SslPolicyErrors.None,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
        }

        http = new HttpClient(handler, disposeHandler: true) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("NucleusCoop/" + Nucleus.Gaming.Globals.Version);
    }

    /// <summary>Timeout of API calls.</summary>
    public TimeSpan ApiTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Timeout of image downloads.</summary>
    public TimeSpan ImageTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>A package download fails when no data arrives for this long.</summary>
    public TimeSpan DownloadIdleTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Whether a string looks like a hub id (also keeps it safe to put in a URL).</summary>
    public static bool IsValidHubId(string? id) => id != null && HubIdPattern.IsMatch(id);

    /// <summary>Package URL of a handler version. <paramref name="countDownload"/> adds <c>?download=true</c>, which the hub counts as a download.</summary>
    public static string PackageUrl(HubHandlerInfo handler, bool countDownload) =>
        $"https://hub.splitscreen.me/cdn/storage/packages/{Uri.EscapeDataString(handler.CurrentPackage)}/original/handler-{Uri.EscapeDataString(handler.Id)}-v{handler.CurrentVersion}.nc"
        + (countDownload ? "?download=true" : "");

    /// <summary>Cover URL of an IGDB image id (size used by the legacy app).</summary>
    public static string CoverUrl(string gameCover) => $"https://images.igdb.com/igdb/image/upload/t_cover_big/{Uri.EscapeDataString(gameCover)}.jpg";

    /// <summary>
    /// Turns an IGDB screenshot URL of the hub (<c>//images.igdb.com/.../t_thumb/x.jpg</c>) into an absolute https URL
    /// of another size (<c>t_screenshot_big</c>, <c>t_original</c>…).
    /// </summary>
    public static string ScreenshotUrl(string hubUrl, string size)
    {
        string url = hubUrl.Replace("t_thumb", size);
        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            url = "https:" + url;
        }
        else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url.Substring("http://".Length);
        }

        return url;
    }

    /// <summary>Parses the hub answer of <c>/handler/{id}</c>: <c>{"Handlers":[{...}]}</c>. Null when empty.</summary>
    public static HubHandlerInfo? ParseHandler(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var root = JToken.Parse(json) as JObject;
        if (root?["Handlers"] is not JArray handlers || handlers.Count == 0 || handlers[0] is not JObject h)
        {
            return null;
        }

        return new HubHandlerInfo
        {
            Id = (string?)h["_id"] ?? "",
            GameName = (string?)h["gameName"] ?? "",
            GameCover = (string?)h["gameCover"] ?? "",
            CurrentVersion = int.TryParse(h["currentVersion"]?.ToString(), out int version) ? version : -1,
            CurrentPackage = (string?)h["currentPackage"] ?? "",
        };
    }

    /// <summary>Parses the hub answer of <c>/screenshots/{id}</c>: the raw <c>url</c> of each screenshot, in order.</summary>
    public static List<string> ParseScreenshots(string json)
    {
        var urls = new List<string>();
        if (string.IsNullOrWhiteSpace(json))
        {
            return urls;
        }

        if (JToken.Parse(json) is JObject root && root["screenshots"] is JArray array)
        {
            foreach (JToken item in array)
            {
                string? url = (string?)item["url"];
                if (!string.IsNullOrEmpty(url))
                {
                    urls.Add(url!);
                }
            }
        }

        return urls;
    }

    /// <summary>Handler record from the hub, or null when the hub does not know the id.</summary>
    public async Task<HubHandlerInfo?> GetHandlerAsync(string handlerId, CancellationToken cancellationToken)
    {
        if (!IsValidHubId(handlerId))
        {
            return null;
        }

        string? json = await GetStringAsync(ApiBase + "handler/" + handlerId, ApiTimeout, cancellationToken).ConfigureAwait(false);
        try
        {
            return json == null ? null : ParseHandler(json);
        }
        catch (Exception e) when (e is Newtonsoft.Json.JsonException)
        {
            throw new HubUnavailableException("The hub sent an unexpected answer.", e);
        }
    }

    /// <summary>Raw screenshot URLs (IGDB, <c>t_thumb</c>, protocol-relative) of a hub handler's game.</summary>
    public async Task<List<string>> GetScreenshotUrlsAsync(string handlerId, CancellationToken cancellationToken)
    {
        if (!IsValidHubId(handlerId))
        {
            return new List<string>();
        }

        string? json = await GetStringAsync(ApiBase + "screenshots/" + handlerId, ApiTimeout, cancellationToken).ConfigureAwait(false);
        try
        {
            return json == null ? new List<string>() : ParseScreenshots(json);
        }
        catch (Exception e) when (e is Newtonsoft.Json.JsonException)
        {
            throw new HubUnavailableException("The hub sent an unexpected answer.", e);
        }
    }

    /// <summary>Whether the hub answers (HEAD on the site root, like the startup check).</summary>
    public async Task<bool> PingAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(6));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, SiteUrl);
            using HttpResponseMessage response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            return (int)response.StatusCode < 500;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>GET as text. Null on 404, <see cref="HubUnavailableException"/> on other failures.</summary>
    public async Task<string?> GetStringAsync(string url, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);
        try
        {
            using HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseContentRead, linked.Token).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HubUnavailableException($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch (Exception e) when (IsNetworkFailure(e, cancellationToken))
        {
            throw Unavailable(e, linked.IsCancellationRequested);
        }
    }

    /// <summary>GET as bytes (images). <see cref="HubUnavailableException"/> on failure, 404 included.</summary>
    public async Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(ImageTimeout);
        try
        {
            using HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseContentRead, linked.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HubUnavailableException($"The server answered {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
        catch (Exception e) when (IsNetworkFailure(e, cancellationToken))
        {
            throw Unavailable(e, linked.IsCancellationRequested);
        }
    }

    /// <summary>
    /// Downloads a file. Reports the percentage when the size is known (null otherwise). The partial file is deleted
    /// on failure or cancellation.
    /// </summary>
    public async Task DownloadToFileAsync(string url, string destination, IProgress<int?>? progress, CancellationToken cancellationToken)
    {
        using var idle = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        idle.CancelAfter(DownloadIdleTimeout);
        try
        {
            using HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, idle.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new HubUnavailableException($"The download failed: the server answered {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            long? total = response.Content.Headers.ContentLength;
            using Stream source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            var buffer = new byte[81920];
            long done = 0;
            int lastPercent = -1;
            progress?.Report(total > 0 ? 0 : (int?)null);
            while (true)
            {
                idle.CancelAfter(DownloadIdleTimeout);
                int read = await source.ReadAsync(buffer, 0, buffer.Length, idle.Token).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                await target.WriteAsync(buffer, 0, read, idle.Token).ConfigureAwait(false);
                done += read;
                if (total > 0)
                {
                    int percent = (int)Math.Min(100, done * 100 / total.Value);
                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        progress?.Report(percent);
                    }
                }
            }
        }
        catch (Exception e)
        {
            TryDelete(destination);
            if (IsNetworkFailure(e, cancellationToken))
            {
                throw Unavailable(e, idle.IsCancellationRequested);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose() => http.Dispose();

    private static void EnsureTls12()
    {
        // SystemDefault (0) lets Windows pick, which includes TLS 1.2 on supported systems; otherwise add TLS 1.2.
        SecurityProtocolType current = ServicePointManager.SecurityProtocol;
        if (current != SecurityProtocolType.SystemDefault && (current & SecurityProtocolType.Tls12) == 0)
        {
            ServicePointManager.SecurityProtocol = current | SecurityProtocolType.Tls12;
        }
    }

    private static bool IsNetworkFailure(Exception e, CancellationToken callerToken)
    {
        if (callerToken.IsCancellationRequested)
        {
            return false; // the caller cancelled: let OperationCanceledException through
        }

        return e is HttpRequestException || e is WebException || e is OperationCanceledException || e is IOException;
    }

    private static HubUnavailableException Unavailable(Exception e, bool timedOut) =>
        e as HubUnavailableException
        ?? new HubUnavailableException(timedOut || e is OperationCanceledException ? "The server did not answer in time." : "The server could not be reached: " + (e.InnerException?.Message ?? e.Message), e);

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
}
