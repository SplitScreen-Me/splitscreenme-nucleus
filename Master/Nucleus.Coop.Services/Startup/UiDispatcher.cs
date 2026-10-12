using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Coop.Services.Startup;

/// <summary>
/// Runs code on the UI thread. Library state (GameManager collections, UserGameInfo objects) is only read and
/// changed there; background work hands its results back through this dispatcher.
/// </summary>
public sealed class UiDispatcher
{
    private readonly SynchronizationContext? context;
    private readonly int uiThreadId;

    /// <summary>Must be created on the UI thread. A null context (tests) runs everything inline.</summary>
    public UiDispatcher(SynchronizationContext? context)
    {
        this.context = context;
        uiThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>Whether the caller is on the UI thread (always true without a context).</summary>
    public bool IsUiThread => context == null || Thread.CurrentThread.ManagedThreadId == uiThreadId;

    /// <summary>Queues an action on the UI thread (inline when already there or without a context).</summary>
    public void Post(Action action)
    {
        if (IsUiThread)
        {
            action();
            return;
        }

        context!.Post(_ => action(), null);
    }

    /// <summary>Runs an action on the UI thread and completes when it has run.</summary>
    public Task InvokeAsync(Action action) => InvokeAsync(() =>
    {
        action();
        return true;
    });

    /// <summary>Runs a function on the UI thread and returns its result.</summary>
    public Task<T> InvokeAsync<T>(Func<T> function)
    {
        if (IsUiThread)
        {
            try
            {
                return Task.FromResult(function());
            }
            catch (Exception e)
            {
                return Task.FromException<T>(e);
            }
        }

        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        context!.Post(_ =>
        {
            try
            {
                completion.SetResult(function());
            }
            catch (Exception e)
            {
                completion.SetException(e);
            }
        }, null);
        return completion.Task;
    }
}
