using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming;
using Nucleus.Gaming.Localization;

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

/// <summary>A downloaded package is not the file the hub published: its SHA-256 differs from the one of the API.</summary>
public sealed class PackageChecksumException : Exception
{
    /// <summary>Creates the exception.</summary>
    public PackageChecksumException(string expected, string actual)
        : base(I18n.T("host.hub.errors.checksumMismatch", new { actual, expected }))
    {
        Expected = expected;
        Actual = actual;
    }

    /// <summary>SHA-256 published by the hub (hex).</summary>
    public string Expected { get; }

    /// <summary>SHA-256 of the downloaded file (hex).</summary>
    public string Actual { get; }
}

/// <summary>The current package of a handler: <c>currentVersion</c> of <c>GET /api/v3/handlers/{id}</c>.</summary>
public sealed class HubPackageInfo
{
    /// <summary>Version number.</summary>
    public int Version { get; set; } = -1;

    /// <summary>SHA-256 of the <c>.nc</c> file, lower-case hex; empty when the hub sent none (or a malformed one).</summary>
    public string Sha256 { get; set; } = "";

    /// <summary>File size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>File name (<c>handler-&lt;id&gt;-v&lt;version&gt;.nc</c>).</summary>
    public string FileName { get; set; } = "";
}

/// <summary>A handler of the hub API v3 (<c>GET /api/v3/handlers/{id}</c>), the fields the services use.</summary>
public sealed class HubHandlerInfo
{
    /// <summary>Hub handler id (the legacy 17-character id, stored by installed handlers).</summary>
    public string Id { get; set; } = "";

    /// <summary>Handler title.</summary>
    public string Title { get; set; } = "";

    /// <summary>Game name.</summary>
    public string GameName { get; set; } = "";

    /// <summary>IGDB image id of the cover, may be empty.</summary>
    public string CoverImageId { get; set; } = "";

    /// <summary>Current version number (<c>version</c>), -1 when missing.</summary>
    public int Version { get; set; } = -1;

    /// <summary>Web page of the handler on the site.</summary>
    public string Url { get; set; } = "";

    /// <summary>The current package, null when the answer has none.</summary>
    public HubPackageInfo? Package { get; set; }

    /// <summary>IGDB image ids of the game screenshots, in the hub's order.</summary>
    public List<string> ScreenshotIds { get; set; } = new List<string>();
}

/// <summary>
/// HTTP access to the hub API v3 (<see cref="HubSite"/>) and the IGDB image CDN for the services. Unlike the legacy
/// helpers it keeps the default TLS certificate validation (set per request, the process-wide accept-all callback set
/// elsewhere is not used), and every call has a timeout.
/// </summary>
public sealed class HubClient : IDisposable
{
    /// <summary>Root of the IGDB images: <c>&lt;root&gt;&lt;size&gt;/&lt;imageId&gt;.jpg</c>.</summary>
    public const string IgdbImageRoot = "https://images.igdb.com/igdb/image/upload/";

    private static readonly Regex HubIdPattern = new Regex("^[A-Za-z0-9]{1,64}$", RegexOptions.CultureInvariant);
    private static readonly Regex ImageIdPattern = new Regex("^[A-Za-z0-9_-]{1,64}$", RegexOptions.CultureInvariant);
    private static readonly Regex Sha256Pattern = new Regex("^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant);

    private readonly HttpClient http;

    /// <summary>
    /// Creates the client for a site root (<see cref="HubSite.Url"/> when null). <paramref name="handler"/> is for tests.
    /// </summary>
    public HubClient(string? siteUrl = null, HttpMessageHandler? handler = null)
    {
        SiteUrl = siteUrl == null ? HubSite.Url : HubSite.Normalize(siteUrl) ?? throw new ArgumentException("Not an absolute http(s) URL.", nameof(siteUrl));
        ApiBase = HubSite.ApiUrl(SiteUrl);

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
        http.DefaultRequestHeaders.UserAgent.ParseAdd("NucleusCoop/" + Globals.Version);
    }

    /// <summary>Site root, without a trailing slash (<c>https://v3.splitscreen.me</c>).</summary>
    public string SiteUrl { get; }

    /// <summary>API root, with a trailing slash (<c>https://v3.splitscreen.me/api/v3/</c>).</summary>
    public string ApiBase { get; }

    /// <summary>Host (and port) of the site, for messages: "v3.splitscreen.me can't be reached".</summary>
    public string DisplayName => new Uri(SiteUrl).Authority;

    /// <summary>Health check URL (<c>GET /health</c>, the app ping).</summary>
    public string HealthUrl => ApiBase + "health";

    /// <summary>Timeout of API calls.</summary>
    public TimeSpan ApiTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Timeout of image downloads.</summary>
    public TimeSpan ImageTimeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>A package download fails when no data arrives for this long.</summary>
    public TimeSpan DownloadIdleTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Whether a string looks like a hub id (also keeps it safe to put in a URL).</summary>
    public static bool IsValidHubId(string? id) => id != null && HubIdPattern.IsMatch(id);

    /// <summary>Handler detail URL (<c>GET /handlers/{id}</c>).</summary>
    public string HandlerUrl(string handlerId) => ApiBase + "handlers/" + Uri.EscapeDataString(handlerId);

    /// <summary>
    /// Download URL of a handler version (<c>GET /handlers/{id}/download?version=n</c>). The hub counts a download once per
    /// address, handler, version and day, unless <paramref name="countDownload"/> is false (<c>count=false</c>: tests,
    /// previews).
    /// </summary>
    public string PackageUrl(string handlerId, int version, bool countDownload) =>
        HandlerUrl(handlerId) + "/download?version=" + version.ToString(System.Globalization.CultureInfo.InvariantCulture)
        + (countDownload ? "" : "&count=false");

    /// <summary>URL of an IGDB image at a size (<c>t_cover_big</c>, <c>t_screenshot_big</c>, <c>t_original</c>…).</summary>
    public static string IgdbImageUrl(string imageId, string size) => $"{IgdbImageRoot}{size}/{Uri.EscapeDataString(imageId)}.jpg";

    /// <summary>Cover URL of an IGDB image id (size used by the legacy app).</summary>
    public static string CoverUrl(string coverImageId) => IgdbImageUrl(coverImageId, "t_cover_big");

    /// <summary>A SHA-256 in lower-case hex, or null when <paramref name="value"/> is not 64 hex digits.</summary>
    public static string? NormalizeSha256(string? value) =>
        value != null && Sha256Pattern.IsMatch(value) ? value.ToLowerInvariant() : null;

    /// <summary>Lower-case hex of a hash.</summary>
    public static string ToHex(byte[] hash)
    {
        var text = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            text.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return text.ToString();
    }

    /// <summary>Parses a handler detail of the API v3 (<c>HandlerDetail</c>). Null when it is not a handler.</summary>
    public static HubHandlerInfo? ParseHandler(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || JToken.Parse(json) is not JObject h)
        {
            return null;
        }

        string id = Text(h["id"]);
        if (id.Length == 0)
        {
            return null;
        }

        var info = new HubHandlerInfo
        {
            Id = id,
            Title = Text(h["title"]),
            GameName = Text(h["gameName"]),
            CoverImageId = Text(h["coverImageId"]),
            Version = Int(h["version"]),
            Url = Text(h["url"]),
        };

        if (h["currentVersion"] is JObject package)
        {
            info.Package = new HubPackageInfo
            {
                Version = Int(package["version"]),
                Sha256 = NormalizeSha256(Text(package["sha256"])) ?? "",
                SizeBytes = long.TryParse(package["sizeBytes"]?.ToString(), out long size) ? size : 0,
                FileName = Text(package["fileName"]),
            };
        }

        if (h["screenshots"] is JArray screenshots)
        {
            foreach (JToken item in screenshots)
            {
                string imageId = item is JObject shot ? Text(shot["imageId"]) : "";
                if (ImageIdPattern.IsMatch(imageId))
                {
                    info.ScreenshotIds.Add(imageId);
                }
            }
        }

        return info;
    }

    /// <summary>Handler from the hub, or null when the hub does not list the id (404).</summary>
    public async Task<HubHandlerInfo?> GetHandlerAsync(string handlerId, CancellationToken cancellationToken)
    {
        if (!IsValidHubId(handlerId))
        {
            return null;
        }

        string? json = await GetStringAsync(HandlerUrl(handlerId), ApiTimeout, cancellationToken).ConfigureAwait(false);
        try
        {
            return json == null ? null : ParseHandler(json);
        }
        catch (Exception e) when (e is Newtonsoft.Json.JsonException)
        {
            throw new HubUnavailableException(I18n.T("host.hub.errors.unexpectedAnswer"), e);
        }
    }

    /// <summary>Whether the hub answers: <c>GET /api/v3/health</c> is 200 (503 when its database is down).</summary>
    public async Task<bool> PingAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(6));
        try
        {
            using HttpResponseMessage response = await http.GetAsync(HealthUrl, HttpCompletionOption.ResponseContentRead, timeout.Token).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
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
                throw new HubUnavailableException(FailureMessage(download: false, response));
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
                throw new HubUnavailableException(FailureMessage(download: false, response));
            }

            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
        catch (Exception e) when (IsNetworkFailure(e, cancellationToken))
        {
            throw Unavailable(e, linked.IsCancellationRequested);
        }
    }

    /// <summary>
    /// Downloads a package and checks its SHA-256 against <paramref name="expectedSha256"/> (hex, from the API). Reports
    /// the percentage when the size is known (null otherwise). The file is deleted on failure, cancellation or a checksum
    /// mismatch (<see cref="PackageChecksumException"/>).
    /// </summary>
    public async Task DownloadToFileAsync(string url, string destination, string expectedSha256, IProgress<int?>? progress, CancellationToken cancellationToken)
    {
        string expected = NormalizeSha256(expectedSha256) ?? throw new ArgumentException("Not a SHA-256 in hex.", nameof(expectedSha256));

        using var idle = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        idle.CancelAfter(DownloadIdleTimeout);
        try
        {
            string actual;
            using (HttpResponseMessage response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, idle.Token).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HubUnavailableException(FailureMessage(download: true, response));
                }

                long? total = response.Content.Headers.ContentLength;
                using Stream source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var target = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                using var sha = SHA256.Create();

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

                    sha.TransformBlock(buffer, 0, read, null, 0);
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

                sha.TransformFinalBlock(buffer, 0, 0);
                actual = ToHex(sha.Hash);
            }

            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new PackageChecksumException(expected, actual);
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

    private static string Text(JToken? token) => token != null && token.Type == JTokenType.String ? (string)token! : "";

    private static int Int(JToken? token) =>
        token != null && token.Type == JTokenType.Integer && int.TryParse(token.ToString(), out int value) ? value : -1;

    /// <summary>Message of an HTTP failure (of a package download or another request); a 429 says when to try again (<c>Retry-After</c>).</summary>
    private static string FailureMessage(bool download, HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 429)
        {
            TimeSpan? wait = response.Headers.RetryAfter?.Delta;
            if (wait.HasValue)
            {
                int count = Math.Max(1, (int)Math.Ceiling(wait.Value.TotalSeconds));
                return download ? I18n.T("host.hub.errors.downloadBusyRetryIn", new { count }) : I18n.T("host.hub.errors.busyRetryIn", new { count });
            }

            return download ? I18n.T("host.hub.errors.downloadBusy") : I18n.T("host.hub.errors.busy");
        }

        // The status code as a string: digits printed as they are.
        var status = new { status = ((int)response.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture), reason = response.ReasonPhrase ?? "" };
        return download ? I18n.T("host.hub.errors.downloadHttpStatus", status) : I18n.T("host.hub.errors.httpStatus", status);
    }

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
        ?? new HubUnavailableException(timedOut || e is OperationCanceledException
            ? I18n.T("host.hub.errors.timeout")
            : I18n.T("host.hub.errors.unreachable", new { error = e.InnerException?.Message ?? e.Message }), e);

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
