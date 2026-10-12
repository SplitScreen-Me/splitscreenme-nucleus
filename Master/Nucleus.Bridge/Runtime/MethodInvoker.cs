using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Model;

namespace Nucleus.Bridge;

/// <summary>Binds params and calls one contract method on a service implementation.</summary>
internal sealed class MethodInvoker
{
    private readonly int _parameterCount;
    private readonly PropertyInfo? _taskResult;

    public MethodInvoker(MethodModel model)
    {
        Model = model;
        _parameterCount = model.Method.GetParameters().Length;
        var returnType = model.Method.ReturnType;
        _taskResult = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)
            ? returnType.GetProperty(nameof(Task<object>.Result))
            : null;
    }

    public MethodModel Model { get; }

    /// <summary>Calls the method and awaits it. Exceptions thrown by the service propagate unwrapped.</summary>
    public async Task<object?> InvokeAsync(object service, JToken? @params, CancellationToken cancellationToken)
    {
        var arguments = new object?[_parameterCount];
        if (Model.ParamsType != null)
        {
            arguments[0] = Bind(@params, Model.ParamsType);
        }

        if (Model.AcceptsCancellation)
        {
            arguments[_parameterCount - 1] = cancellationToken;
        }

        object? returned;
        try
        {
            returned = Model.Method.Invoke(service, arguments);
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }

        if (returned is not Task task)
        {
            return returned;
        }

        await task;
        return _taskResult?.GetValue(task);
    }

    private object Bind(JToken? @params, Type type)
    {
        if (@params is not JObject request)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, $"'{Model.WireName}' expects params: a {type.Name} object.");
        }

        try
        {
            return request.ToObject(type, BridgeJson.Serializer)!;
        }
        catch (JsonException e)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, $"Invalid params for '{Model.WireName}': {e.Message}");
        }
    }
}
