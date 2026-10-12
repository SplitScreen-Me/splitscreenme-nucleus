using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Sends <c>library.changed</c> (replaces the legacy per-row 1.5 s polling). Changes are coalesced for a short delay
/// so a burst (a toggle that saves metaInfo.json twice, a download of 30 covers…) becomes one event. Thread-safe.
/// </summary>
public sealed class LibraryNotifier : IDisposable
{
    private readonly IBridgeEvents events;
    private readonly TimeSpan delay;
    private readonly object gate = new object();
    private readonly HashSet<string> pending = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Timer timer;
    private bool everything;
    private bool scheduled;

    /// <summary>Creates the notifier.</summary>
    public LibraryNotifier(IBridgeEvents events, TimeSpan? delay = null)
    {
        this.events = events;
        this.delay = delay ?? TimeSpan.FromMilliseconds(250);
        timer = new Timer(_ => Flush(), null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>Some games changed.</summary>
    public void GamesChanged(params string[] gameIds) => GamesChanged((IEnumerable<string>)gameIds);

    /// <summary>Some games changed.</summary>
    public void GamesChanged(IEnumerable<string> gameIds)
    {
        lock (gate)
        {
            foreach (string id in gameIds.Where(i => !string.IsNullOrEmpty(i)))
            {
                pending.Add(id);
            }

            Schedule();
        }
    }

    /// <summary>Anything may have changed (reload, settings that affect every game).</summary>
    public void Everything()
    {
        lock (gate)
        {
            everything = true;
            Schedule();
        }
    }

    /// <summary>Sends the pending event now.</summary>
    public void Flush()
    {
        List<string> ids;
        lock (gate)
        {
            scheduled = false;
            if (!everything && pending.Count == 0)
            {
                return;
            }

            ids = everything ? new List<string>() : pending.ToList();
            everything = false;
            pending.Clear();
        }

        try
        {
            events.Emit(new LibraryChanged { GameIds = ids });
        }
        catch (Exception e)
        {
            ServiceLog.Warn("library.changed could not be sent", e);
        }
    }

    /// <inheritdoc />
    public void Dispose() => timer.Dispose();

    private void Schedule()
    {
        if (!scheduled)
        {
            scheduled = true;
            timer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }
}
