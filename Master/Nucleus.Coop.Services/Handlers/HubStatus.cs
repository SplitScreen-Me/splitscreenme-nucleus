using System;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming.Coop.Generic;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// Whether the hub (<see cref="HubClient.SiteUrl"/>) answers. Backed by the legacy static <c>Hub.Connected</c> (which
/// the update checks of Nucleus.Gaming read and which the MainForm used to set); refreshed by <c>GET /api/v3/health</c>
/// at startup and, when it was offline, at most every 30 s when a feature needs the hub. With
/// <see cref="StartMonitoring"/> it also follows the connection by itself (Nucleus 2.5: the UI goes offline and online
/// automatically): every 30 s while offline, every 5 min while online, never during a session. The UI asks for a check
/// with <see cref="CheckNowAsync"/> (<c>handlers.checkHub</c>): its "Retry now" button, and its own hub requests that
/// contradict the known state.
/// </summary>
public sealed class HubStatus : IDisposable
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan OnlineCheckInterval = TimeSpan.FromMinutes(5);
    private Timer? monitor;
    private Func<bool>? paused;
    private readonly HubClient client;
    private readonly IBridgeEvents? events;
    private readonly object gate = new object();
    private readonly object probeGate = new object();
    private Task<bool>? probe;// the check running, shared by every caller
    private DateTime lastProbeUtc = DateTime.MinValue;

    /// <summary>Creates the status. Changes are sent as <c>app.hubStatusChanged</c> through <paramref name="events"/>.</summary>
    public HubStatus(HubClient client, IBridgeEvents? events = null)
    {
        this.client = client;
        this.events = events;
    }

    /// <summary>Last known state.</summary>
    public bool IsOnline => Hub.Connected;

    /// <summary>
    /// Raised (on the thread that noticed it) after the hub went online (true) or offline (false), once
    /// <c>app.hubStatusChanged</c> was sent.
    /// </summary>
    public event Action<bool>? Changed;

    /// <summary>Probes the hub now and records the result.</summary>
    public async Task<bool> ProbeAsync(CancellationToken cancellationToken)
    {
        lastProbeUtc = DateTime.UtcNow;
        bool online = await client.PingAsync(cancellationToken).ConfigureAwait(false);
        SetConnected(online);
        return online;
    }

    /// <summary>True when online; when offline, probes again if the last probe is old enough.</summary>
    public async Task<bool> EnsureOnlineAsync(CancellationToken cancellationToken)
    {
        if (Hub.Connected)
        {
            return true;
        }

        if (DateTime.UtcNow - lastProbeUtc < RetryInterval)
        {
            return false;
        }

        return await ProbeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// A check the UI asks for (<c>handlers.checkHub</c>). <paramref name="force"/> (the user pressed "Retry now"): probes
    /// now, or waits for the probe already running. Otherwise (a hub request of the UI disagreed with the known state):
    /// probes only when <see cref="RequestedCheckDue"/> and no session runs, else answers the known state at once.
    /// Returns whether the hub answers.
    /// </summary>
    public Task<bool> CheckNowAsync(bool force)
    {
        if (!force && (paused?.Invoke() == true || !RequestedCheckDue(lastProbeUtc, DateTime.UtcNow)))
        {
            return Task.FromResult(Hub.Connected);
        }

        return SharedProbeAsync();
    }

    /// <summary>Whether a check the UI asks for without forcing it is due (pure): the last check is 30 s old or more.</summary>
    internal static bool RequestedCheckDue(DateTime lastProbeUtc, DateTime nowUtc) => nowUtc - lastProbeUtc >= RetryInterval;

    /// <summary>
    /// Probes the hub periodically from now on (offline: every 30 s; online: every 5 min, a recent real call counts),
    /// skipping while <paramref name="pause"/> returns true (a game session). Changes go out as <c>app.hubStatusChanged</c>.
    /// </summary>
    public void StartMonitoring(Func<bool>? pause = null)
    {
        paused = pause;
        monitor ??= new Timer(_ => _ = MonitorTickAsync(), null, RetryInterval, RetryInterval);
    }

    /// <summary>Whether a periodic check is due (pure: last probe time, state, now).</summary>
    internal static bool ProbeDue(bool online, DateTime lastProbeUtc, DateTime nowUtc) =>
        nowUtc - lastProbeUtc >= (online ? OnlineCheckInterval : RetryInterval);

    private Task MonitorTickAsync()
    {
        if (paused?.Invoke() == true || !ProbeDue(Hub.Connected, lastProbeUtc, DateTime.UtcNow))
        {
            return Task.CompletedTask;
        }

        return SharedProbeAsync();
    }

    /// <summary>The probe running, or a new one: callers that come meanwhile share it (one request at a time).</summary>
    private Task<bool> SharedProbeAsync()
    {
        lock (probeGate)
        {
            return probe ??= RunProbeAsync();
        }
    }

    private async Task<bool> RunProbeAsync()
    {
        try
        {
            await Task.Yield();// so the probe is stored before it can complete
            return await ProbeAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ServiceLog.Warn("hub check failed", e);
            return Hub.Connected;
        }
        finally
        {
            lock (probeGate)
            {
                probe = null;
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        monitor?.Dispose();
        monitor = null;
    }

    /// <summary>Records the outcome of a real hub call (a successful call means online).</summary>
    public void Report(bool reachable)
    {
        lastProbeUtc = DateTime.UtcNow;
        SetConnected(reachable);
    }

    private void SetConnected(bool online)
    {
        lock (gate)
        {
            //The first check is always announced: before it, "offline" only meant "not checked yet".
            bool first = !Hub.Checked;
            Hub.Checked = true;

            if (!first && Hub.Connected == online)
            {
                return;
            }

            Hub.Connected = online;
        }

        ServiceLog.Info(client.DisplayName + " is " + (online ? "online" : "offline"));
        try
        {
            events?.Emit(new HubStatusChanged { Online = online });
        }
        catch (Exception e)
        {
            ServiceLog.Warn("app.hubStatusChanged could not be sent", e);
        }

        try
        {
            Changed?.Invoke(online);
        }
        catch (Exception e)
        {
            ServiceLog.Warn("hub status listener failed", e);
        }
    }
}
