using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Bridge.Model;

namespace Nucleus.Bridge;

/// <summary><see cref="IBridgeEvents"/> of a <see cref="BridgeHost"/>.</summary>
internal sealed class BridgeEventEmitter : IBridgeEvents
{
    private readonly Dictionary<Type, EventModel> _events;
    private readonly SynchronizationContext? _context;
    private readonly Action<string> _send;
    private readonly Action<BridgeLogLevel, string> _log;

    public BridgeEventEmitter(ContractModel contract, SynchronizationContext? context, Action<string> send, Action<BridgeLogLevel, string> log)
    {
        _events = contract.Events.ToDictionary(e => e.PayloadType);
        _context = context;
        _send = send;
        _log = log;
    }

    public void Emit<TEvent>(TEvent payload)
        where TEvent : class
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (!_events.TryGetValue(payload.GetType(), out var evt))
        {
            throw new ArgumentException($"{payload.GetType().Name} is not an event of the contract: mark it [BridgeEvent(\"service.name\")].", nameof(payload));
        }

        // Serialize now, on the caller's thread: the event carries the state at emit time.
        var message = JsonRpcWriter.Notification(evt.Name, payload);
        _log(BridgeLogLevel.Debug, "event " + evt.Name);
        if (_context == null)
        {
            _send(message);
        }
        else
        {
            _context.Post(state => _send((string)state!), message);
        }
    }
}
