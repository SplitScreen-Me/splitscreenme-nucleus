namespace Nucleus.Bridge;

/// <summary>
/// Sends events to the UI. Services receive this interface (not the whole <see cref="BridgeHost"/>), which keeps
/// them easy to test with a fake.
/// </summary>
public interface IBridgeEvents
{
    /// <summary>
    /// Sends <paramref name="payload"/> to the UI as the event declared by its <see cref="BridgeEventAttribute"/>.
    /// Thread-safe: callable from any thread. The payload is serialized immediately, so later changes to the object
    /// are not sent; delivery happens on the host's synchronization context, in emit order.
    /// </summary>
    /// <typeparam name="TEvent">An event class of the contract.</typeparam>
    /// <param name="payload">The event.</param>
    /// <exception cref="System.ArgumentException">The type is not an event of the contract.</exception>
    void Emit<TEvent>(TEvent payload)
        where TEvent : class;
}
