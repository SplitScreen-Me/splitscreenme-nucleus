using System;
using System.Threading;
using System.Threading.Tasks;

public static class UIThread
{
    private static SynchronizationContext _context;
    public static SynchronizationContext SynchronizationContext => _context;
    /// <summary>
    /// Must be called ONCE from the UI thread
    /// </summary>
    public static void Initialize()
    {
        _context = SynchronizationContext.Current
            ?? throw new InvalidOperationException("No UI SynchronizationContext found. Call Initialize() from the UI thread.");
    }

    /// <summary>
    /// Post a parameterless action to the UI thread
    /// </summary>
    public static void Post(Action action)
    {
        if (_context == null)
            throw new InvalidOperationException("UIThread not initialized.");

        _context.Post(_ => action(), null);
    }

    /// <summary>
    /// Post an action with a strongly-typed parameter
    /// </summary>
    public static void Post<T>(Action<T> action, T arg)
    {
        if (_context == null)
            throw new InvalidOperationException("UIThread not initialized.");

        _context.Post(_ => action(arg), null);
    }

    /// <summary>
    /// Post a function with a return value (fire-and-forget)
    /// </summary>
    public static void Post<T>(Func<T> func, Action<T> onResult)
    {
        if (_context == null)
            throw new InvalidOperationException("UIThread not initialized.");

        _context.Post(_ =>
        {
            var result = func();
            onResult?.Invoke(result);
        }, null);
    }


    public static Task PostAsync(Action action)
    {
        var tcs = new TaskCompletionSource<object>();

        Post(() =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public static SynchronizationContext GetThreadContext()
    {
        SynchronizationContext context = SynchronizationContext.Current;
        return context;      
    }

}
