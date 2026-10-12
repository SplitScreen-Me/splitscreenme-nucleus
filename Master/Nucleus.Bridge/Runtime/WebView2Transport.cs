using System;
using System.Linq;
using Microsoft.Web.WebView2.Core;

namespace Nucleus.Bridge;

/// <summary>
/// Connects a <see cref="BridgeHost"/> to a WebView2: web messages from the UI origin go to the host, responses and
/// events go back with <see cref="CoreWebView2.PostWebMessageAsString"/>. Requests in flight are cancelled when the
/// page navigates or reloads.
/// </summary>
/// <remarks>
/// The transport also answers the reserved method <c>$/filePaths</c>: the UI posts it with
/// <c>chrome.webview.postMessageWithAdditionalObjects(message, files)</c> and receives the absolute paths of the
/// DOM <c>File</c> objects (drag and drop), which page scripts cannot read.
/// </remarks>
public sealed class WebView2Transport : IDisposable
{
    /// <summary>Reserved method answered by the transport: absolute paths of the files posted with the message.</summary>
    public const string FilePathsMethod = "$/filePaths";

    private readonly CoreWebView2 _webView;
    private readonly BridgeHost _host;
    private readonly string _origin;
    private bool _disposed;

    /// <summary>Starts relaying messages between <paramref name="webView"/> and <paramref name="host"/>.</summary>
    /// <param name="webView">The WebView2 core, used on the UI thread only.</param>
    /// <param name="host">
    /// The bridge host. It must have been created with the UI thread's synchronization context, so that events reach
    /// the WebView on the UI thread.
    /// </param>
    /// <param name="allowedOrigin">
    /// Origin of the UI, for example <c>https://ui.nucleus.example</c> (or the dev server URL). Messages from other
    /// origins are ignored, and nothing is posted while another origin is displayed.
    /// </param>
    public WebView2Transport(CoreWebView2 webView, BridgeHost host, string allowedOrigin)
    {
        if (host.SynchronizationContext == null)
        {
            throw new ArgumentException("Create the BridgeHost with the UI thread's SynchronizationContext: WebView2 must be called on the UI thread.", nameof(host));
        }

        _webView = webView ?? throw new ArgumentNullException(nameof(webView));
        _host = host;
        _origin = Origin.Of(allowedOrigin) ?? throw new ArgumentException($"'{allowedOrigin}' is not an absolute URL.", nameof(allowedOrigin));
        _webView.WebMessageReceived += OnWebMessageReceived;
        _webView.ContentLoading += OnContentLoading;
        _host.Outgoing += Post;
    }

    /// <summary>Stops relaying. Call it before the WebView is disposed.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _host.Outgoing -= Post;
        _webView.WebMessageReceived -= OnWebMessageReceived;
        _webView.ContentLoading -= OnContentLoading;
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (!Origin.Matches(e.Source, _origin))
        {
            _host.Log(BridgeLogLevel.Warning, $"ignored a message from {e.Source}");
            return;
        }

        string message;
        try
        {
            message = e.TryGetWebMessageAsString();
        }
        catch (ArgumentException)
        {
            _host.Log(BridgeLogLevel.Warning, "ignored a message that is not a string (post JSON strings only)");
            return;
        }

        var response = TryAnswerFilePaths(message, e) ?? await _host.HandleAsync(message);
        if (response != null)
        {
            Post(response);
        }
    }

    /// <summary>Answers <see cref="FilePathsMethod"/>, or returns null for any other message.</summary>
    private string? TryAnswerFilePaths(string message, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var files = e.AdditionalObjects;
        if (files == null || files.Count == 0)
        {
            return null;
        }

        var request = JsonRpcRequest.Parse(message, out _);
        if (request?.Method != FilePathsMethod || request.IsNotification)
        {
            return null;
        }

        var paths = files.OfType<CoreWebView2File>().Select(f => f.Path).ToArray();
        _host.Log(BridgeLogLevel.Debug, $"{FilePathsMethod} #{request.IdText}: {paths.Length} file(s)");
        return JsonRpcWriter.Result(request.Id, paths);
    }

    // ContentLoading is raised once the new document is committed, before its scripts run; same-document navigations
    // (routing) and navigations cancelled by the host do not raise it, unlike NavigationStarting.
    private void OnContentLoading(object? sender, CoreWebView2ContentLoadingEventArgs e) => _host.CancelAll();

    private void Post(string message)
    {
        if (_disposed || !Origin.Matches(_webView.Source, _origin))
        {
            return;
        }

        try
        {
            _webView.PostWebMessageAsString(message);
        }
        catch (InvalidOperationException e)
        {
            _host.Log(BridgeLogLevel.Debug, "message not posted: the WebView is closed (" + e.Message + ")");
        }
    }
}

/// <summary>Origin (<c>scheme://host[:port]</c>) comparison for <see cref="WebView2Transport"/>.</summary>
internal static class Origin
{
    /// <summary>The origin of an absolute URL, or null.</summary>
    public static string? Of(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.GetLeftPart(UriPartial.Authority) : null;

    public static bool Matches(string? url, string origin) =>
        string.Equals(Of(url), origin, StringComparison.OrdinalIgnoreCase);
}
