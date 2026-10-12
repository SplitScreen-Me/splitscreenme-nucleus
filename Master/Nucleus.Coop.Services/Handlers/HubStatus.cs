using System;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming.Coop.Generic;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// Whether hub.splitscreen.me answers. Backed by the legacy static <c>Hub.Connected</c> (which the update checks of
/// Nucleus.Gaming read and which the MainForm used to set); refreshed by a HEAD request at startup and, when it was
/// offline, at most every 30 s when a feature needs the hub.
/// </summary>
public sealed class HubStatus
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(30);
    private readonly HubClient client;
    private readonly IBridgeEvents? events;
    private readonly object gate = new object();
    private DateTime lastProbeUtc = DateTime.MinValue;

    /// <summary>Creates the status. Changes are sent as <c>app.hubStatusChanged</c> through <paramref name="events"/>.</summary>
    public HubStatus(HubClient client, IBridgeEvents? events = null)
    {
        this.client = client;
        this.events = events;
    }

    /// <summary>Last known state.</summary>
    public bool IsOnline => Hub.Connected;

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
            if (Hub.Connected == online)
            {
                return;
            }

            Hub.Connected = online;
        }

        ServiceLog.Info("hub.splitscreen.me is " + (online ? "online" : "offline"));
        try
        {
            events?.Emit(new HubStatusChanged { Online = online });
        }
        catch (Exception e)
        {
            ServiceLog.Warn("app.hubStatusChanged could not be sent", e);
        }
    }
}
