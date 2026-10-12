using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Model;

namespace Nucleus.Bridge;

/// <summary>
/// Host side of the bridge: dispatches JSON-RPC requests from the UI to the registered services and sends events
/// back. It does not know the transport: pass incoming messages to <see cref="HandleAsync"/>, send its answer and
/// every <see cref="Outgoing"/> message to the UI (<see cref="WebView2Transport"/> does both).
/// </summary>
/// <example>
/// <code>
/// var host = new BridgeHost(SynchronizationContext.Current, entry => Log(entry.ToString()));
/// host.Register&lt;IWindowService&gt;(new WindowService(form));
/// var transport = new WebView2Transport(webView.CoreWebView2, host, "https://ui.nucleus.example");
/// host.Events.Emit(new WindowStateChanged { State = state });   // any thread
/// </code>
/// </example>
public sealed class BridgeHost
{
    /// <summary>Version of the wire protocol (envelope, error codes, cancellation), returned by <c>app.hello</c>.</summary>
    public const int ProtocolVersion = 1;

    private const string CancelMethod = "$/cancel";

    private readonly Dictionary<string, MethodInvoker> _methods;
    private readonly ConcurrentDictionary<Type, object> _services = new();
    private readonly ConcurrentDictionary<Type, Task<object>> _deferred = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _inFlight = new();
    private readonly Action<BridgeLogEntry>? _log;

    // Incremented by CancelAll. Requests remember the generation they started in: responses from an older one are
    // dropped, and their ids cannot collide with the ids of the new page (which restart at 1).
    private int _generation;

    /// <summary>Creates a host for a contract.</summary>
    /// <param name="synchronizationContext">
    /// Context that <see cref="Outgoing"/> is raised on for events (the UI thread with WebView2). Null raises it on the
    /// emitting thread, which suits tests.
    /// </param>
    /// <param name="log">Receives the bridge log: every call with its duration (Debug), failures (Warning, Error).</param>
    /// <param name="contract">The contract to serve. Default: <see cref="ContractModel.Default"/>, the Nucleus contract.</param>
    public BridgeHost(SynchronizationContext? synchronizationContext = null, Action<BridgeLogEntry>? log = null, ContractModel? contract = null)
    {
        Contract = contract ?? ContractModel.Default;
        SynchronizationContext = synchronizationContext;
        _log = log;
        _methods = Contract.Methods.ToDictionary(m => m.WireName, m => new MethodInvoker(m));
        Events = new BridgeEventEmitter(Contract, synchronizationContext, message => Outgoing?.Invoke(message), (level, message) => Log(level, message));
    }

    /// <summary>
    /// A message for the UI: an event, raised on the synchronization context given at construction.
    /// Responses are not raised here; <see cref="HandleAsync"/> returns them.
    /// </summary>
    public event Action<string>? Outgoing;

    /// <summary>The contract this host serves.</summary>
    public ContractModel Contract { get; }

    /// <summary>Hash of the contract, for <c>app.hello</c>. The UI refuses to run when it differs from its own.</summary>
    public string ContractHash => Contract.Hash;

    /// <summary>Sends events to the UI. Give it to the services that emit events.</summary>
    public IBridgeEvents Events { get; }

    internal SynchronizationContext? SynchronizationContext { get; }

    /// <summary>Registers the implementation of a service of the contract.</summary>
    /// <typeparam name="TService">A <see cref="BridgeServiceAttribute"/> interface of the contract.</typeparam>
    /// <param name="implementation">The implementation. Its methods are called on the thread that calls <see cref="HandleAsync"/>.</param>
    /// <exception cref="ArgumentException"><typeparamref name="TService"/> is not a service of the contract.</exception>
    /// <exception cref="InvalidOperationException">The service is already registered.</exception>
    public void Register<TService>(TService implementation)
        where TService : class
    {
        if (implementation == null)
        {
            throw new ArgumentNullException(nameof(implementation));
        }

        var service = Contract.Services.FirstOrDefault(s => s.Interface == typeof(TService))
            ?? throw new ArgumentException($"{typeof(TService).Name} is not a service of the contract: service interfaces carry [BridgeService] and live in the contract assembly.", nameof(TService));
        if (_deferred.ContainsKey(typeof(TService)) || !_services.TryAdd(typeof(TService), implementation))
        {
            throw new InvalidOperationException($"The '{service.Name}' service is already registered.");
        }
    }

    /// <summary>
    /// Registers a service whose implementation is still being created (for example while the library loads in the
    /// background). Calls to it wait for the task without blocking the calling thread, then run on that thread as
    /// usual; if the task fails, they fail with <see cref="BridgeErrorKind.Unavailable"/>.
    /// </summary>
    /// <typeparam name="TService">A <see cref="BridgeServiceAttribute"/> interface of the contract.</typeparam>
    /// <param name="implementation">Completes with the implementation.</param>
    /// <exception cref="ArgumentException"><typeparamref name="TService"/> is not a service of the contract.</exception>
    /// <exception cref="InvalidOperationException">The service is already registered.</exception>
    public void RegisterDeferred<TService>(Task<TService> implementation)
        where TService : class
    {
        if (implementation == null)
        {
            throw new ArgumentNullException(nameof(implementation));
        }

        var service = Contract.Services.FirstOrDefault(s => s.Interface == typeof(TService))
            ?? throw new ArgumentException($"{typeof(TService).Name} is not a service of the contract: service interfaces carry [BridgeService] and live in the contract assembly.", nameof(TService));
        var pending = implementation.ContinueWith(t => (object)t.Result, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        if (_services.ContainsKey(typeof(TService)) || !_deferred.TryAdd(typeof(TService), pending))
        {
            throw new InvalidOperationException($"The '{service.Name}' service is already registered.");
        }

        pending.ContinueWith(t =>
        {
            if (t.Status == TaskStatus.RanToCompletion)
            {
                _services.TryAdd(typeof(TService), t.Result);
            }
            else
            {
                Log(BridgeLogLevel.Error, $"the '{service.Name}' service couldn't start", t.Exception?.GetBaseException());
            }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    /// <summary>
    /// Handles one message from the UI and returns the response to send back, or null when there is none
    /// (notifications, responses to a page that has been reloaded since). Never throws.
    /// </summary>
    /// <param name="message">A JSON-RPC 2.0 request, or a <c>$/cancel</c> notification.</param>
    public async Task<string?> HandleAsync(string message)
    {
        try
        {
            var request = JsonRpcRequest.Parse(message, out var errorResponse);
            if (request == null)
            {
                Log(BridgeLogLevel.Warning, "rejected message: " + Truncate(message));
                return errorResponse;
            }

            if (request.IsNotification)
            {
                HandleNotification(request);
                return null;
            }

            return await DispatchAsync(request);
        }
        catch (Exception e)
        {
            Log(BridgeLogLevel.Error, "bridge failure while handling: " + Truncate(message), e);
            return JsonRpcWriter.Error(null, BridgeErrorKind.Internal, "Internal bridge error.", null);
        }
    }

    /// <summary>
    /// Cancels every request in flight and drops their responses. Call it when the page navigates or reloads;
    /// <see cref="WebView2Transport"/> does.
    /// </summary>
    public void CancelAll()
    {
        Interlocked.Increment(ref _generation);
        var pending = _inFlight.Values.ToList();
        foreach (var cancellation in pending)
        {
            TryCancel(cancellation);
        }

        if (pending.Count > 0)
        {
            Log(BridgeLogLevel.Info, $"page reloaded: cancelled {pending.Count} request(s) in flight");
        }
    }

    internal void Log(BridgeLogLevel level, string message, Exception? exception = null) =>
        _log?.Invoke(new BridgeLogEntry(level, message, exception));

    private async Task<string?> DispatchAsync(JsonRpcRequest request)
    {
        var generation = Volatile.Read(ref _generation);
        var key = generation + ":" + request.IdText;
        var label = $"{request.Method} #{request.IdText}";
        using var cancellation = new CancellationTokenSource();
        if (!_inFlight.TryAdd(key, cancellation))
        {
            Log(BridgeLogLevel.Warning, $"{label} rejected: a request with this id is in flight");
            return JsonRpcWriter.Error(request.Id, BridgeErrorKind.InvalidRequest, $"A request with id {request.IdText} is already in flight.", null);
        }

        var clock = Stopwatch.StartNew();
        string response;
        try
        {
            var result = await InvokeAsync(request, cancellation.Token);
            response = JsonRpcWriter.Result(request.Id, result);
            Log(BridgeLogLevel.Debug, $"{label} ok ({clock.ElapsedMilliseconds} ms)");
        }
        catch (Exception e)
        {
            response = ErrorResponse(request, label, e, cancellation.IsCancellationRequested, clock.ElapsedMilliseconds);
        }
        finally
        {
            _inFlight.TryRemove(key, out _);
        }

        if (generation != Volatile.Read(ref _generation))
        {
            Log(BridgeLogLevel.Debug, $"{label} response dropped: the page was reloaded");
            return null;
        }

        return response;
    }

    private async Task<object?> InvokeAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (!_methods.TryGetValue(request.Method, out var invoker))
        {
            throw new BridgeException(BridgeErrorKind.MethodNotFound, $"Unknown method '{request.Method}'.");
        }

        var serviceType = invoker.Model.Method.DeclaringType!;
        if (!_services.TryGetValue(serviceType, out var service))
        {
            if (!_deferred.TryGetValue(serviceType, out var pending))
            {
                throw new BridgeException(BridgeErrorKind.MethodNotFound, $"'{request.Method}' is in the contract, but no '{invoker.Model.ServiceName}' service is registered.");
            }

            // Resumes on the caller's context (the UI thread with WebView2): services keep running where they expect.
            service = await WaitForDeferred(pending, invoker.Model.ServiceName, cancellationToken);
        }

        return await invoker.InvokeAsync(service, request.Params, cancellationToken);
    }

    private static async Task<object> WaitForDeferred(Task<object> pending, string serviceName, CancellationToken cancellationToken)
    {
        if (!pending.IsCompleted)
        {
            var cancelled = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(pending, cancelled.Task) != pending)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        if (pending.Status != TaskStatus.RanToCompletion)
        {
            var reason = pending.Exception?.GetBaseException().Message ?? "it was cancelled";
            throw new BridgeException(BridgeErrorKind.Unavailable, $"The '{serviceName}' service couldn't start: {reason}");
        }

        return pending.Result;
    }

    private string ErrorResponse(JsonRpcRequest request, string label, Exception exception, bool cancelled, long elapsedMs)
    {
        BridgeErrorKind kind;
        string message;
        object? data = null;
        if (exception is OperationCanceledException && cancelled)
        {
            kind = BridgeErrorKind.Cancelled;
            message = "The request was cancelled.";
            Log(BridgeLogLevel.Debug, $"{label} cancelled ({elapsedMs} ms)");
        }
        else if (exception is BridgeException bridgeException)
        {
            kind = bridgeException.Kind;
            message = bridgeException.Message;
            data = bridgeException.ErrorData;
            Log(BridgeLogLevel.Warning, $"{label} failed: {KindName(kind)}: {message} ({elapsedMs} ms)");
        }
        else
        {
            kind = BridgeErrorKind.Internal;
            message = $"Unexpected error in {request.Method}. Details are in the Nucleus Co-op log.";
            Log(BridgeLogLevel.Error, $"{label} failed: {exception.GetType().Name}: {exception.Message} ({elapsedMs} ms)", exception);
        }

        try
        {
            return JsonRpcWriter.Error(request.Id, kind, message, data);
        }
        catch (JsonException e)
        {
            Log(BridgeLogLevel.Error, $"{label}: the error data could not be serialized", e);
            return JsonRpcWriter.Error(request.Id, kind, message, null);
        }
    }

    private void HandleNotification(JsonRpcRequest notification)
    {
        if (notification.Method != CancelMethod)
        {
            Log(BridgeLogLevel.Warning, $"ignored notification '{notification.Method}'");
            return;
        }

        var id = JsonRpcRequest.IdToText((notification.Params as JObject)?["id"]);
        if (_inFlight.TryGetValue(Volatile.Read(ref _generation) + ":" + id, out var cancellation))
        {
            Log(BridgeLogLevel.Debug, $"cancel #{id}");
            TryCancel(cancellation);
        }
    }

    private void TryCancel(CancellationTokenSource cancellation)
    {
        try
        {
            cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The request completed meanwhile.
        }
        catch (AggregateException e)
        {
            Log(BridgeLogLevel.Error, "a cancellation callback threw", e);
        }
    }

    private static string KindName(BridgeErrorKind kind) => BridgeJson.Naming.GetPropertyName(kind.ToString(), false);

    private static string Truncate(string text) => text.Length <= 200 ? text : text.Substring(0, 200) + "…";
}
