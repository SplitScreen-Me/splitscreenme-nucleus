using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Model;

/// <summary>Builds a <see cref="ContractModel"/> by reflection and checks the contract conventions.</summary>
internal sealed class ContractModelBuilder
{
    private static readonly Regex ServiceName = new("^[a-z][A-Za-z0-9]*$");
    private static readonly Regex EventName = new(@"^[a-z][A-Za-z0-9]*(\.[a-z][A-Za-z0-9]*)+$");

    private readonly List<string> _errors = new();
    private readonly TypeMapper _types;

    public ContractModelBuilder() => _types = new TypeMapper(_errors);

    public ContractModel Build(IEnumerable<Type> candidates, XmlDocs? docs)
    {
        var all = candidates.Distinct().ToList();
        var services = all
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<BridgeServiceAttribute>(false)))
            .Where(s => s.Attribute != null)
            .Select(s => BuildService(s.Type, s.Attribute!.Name))
            .OrderBy(s => s.Name, StringComparer.Ordinal)
            .ToList();
        var events = all
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<BridgeEventAttribute>(false)))
            .Where(e => e.Attribute != null)
            .Select(e => BuildEvent(e.Type, e.Attribute!))
            .OrderBy(e => e.Name, StringComparer.Ordinal)
            .ToList();

        RequireUnique("service", services.Select(s => s.Name));
        RequireUnique("event", events.Select(e => e.Name));
        CheckInvalidations(events, services);
        var types = _types.BuildTypes();

        if (_errors.Count > 0)
        {
            throw new ContractModelException(_errors);
        }

        var model = new ContractModel(services, types, events);
        docs?.Apply(model);
        return model;
    }

    private ServiceModel BuildService(Type type, string name)
    {
        if (!type.IsInterface || type.IsGenericType)
        {
            _errors.Add($"{type.Name}: [BridgeService] goes on non-generic interfaces.");
        }

        if (type.GetInterfaces().Length > 0)
        {
            _errors.Add($"{type.Name}: service interfaces cannot inherit other interfaces; declare every method on the service.");
        }

        if (type.GetProperties().Length > 0 || type.GetEvents().Length > 0)
        {
            _errors.Add($"{type.Name}: services contain methods only (no properties or events).");
        }

        if (!ServiceName.IsMatch(name))
        {
            _errors.Add($"{type.Name}: service name '{name}' must be camelCase letters and digits, for example 'library'.");
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName)
            .ToList();
        foreach (var overload in methods.GroupBy(m => m.Name).Where(g => g.Count() > 1))
        {
            _errors.Add($"{type.Name}.{overload.Key}: overloads are not allowed; give each method its own name.");
        }

        var models = methods
            .Select(m => BuildMethod(name, type, m))
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();
        return new ServiceModel(name, type, models);
    }

    private MethodModel BuildMethod(string serviceName, Type service, MethodInfo method)
    {
        var location = $"{service.Name}.{method.Name}";
        if (method.IsGenericMethodDefinition)
        {
            _errors.Add($"{location}: generic methods are not allowed.");
        }

        var parameters = method.GetParameters();
        var acceptsCancellation = parameters.Length > 0 && parameters[parameters.Length - 1].ParameterType == typeof(CancellationToken);
        var requestParameters = acceptsCancellation ? parameters.Take(parameters.Length - 1).ToArray() : parameters;

        Type? paramsType = null;
        TypeRef? paramsRef = null;
        if (requestParameters.Length > 1)
        {
            _errors.Add($"{location}: a method takes at most one parameter, a request DTO (optionally followed by a CancellationToken).");
        }
        else if (requestParameters.Length == 1)
        {
            var parameter = requestParameters[0];
            paramsType = parameter.ParameterType;
            var parameterLocation = $"{location} (parameter '{parameter.Name}')";
            if (parameter.ParameterType.IsByRef || !TypeMapper.IsObjectType(parameter.ParameterType))
            {
                _errors.Add($"{parameterLocation}: the parameter must be a request DTO class, for example {method.Name}Request.");
            }
            else
            {
                paramsRef = _types.Map(Nullability.Of(parameter), parameterLocation);
                if (paramsRef.Nullable)
                {
                    _errors.Add($"{parameterLocation}: the request cannot be nullable; the UI always sends it.");
                }
            }
        }

        var result = MapResult(method, location);
        var name = BridgeJson.Naming.GetPropertyName(method.Name, false);
        return new MethodModel(serviceName, name, method, paramsType, paramsRef, acceptsCancellation, result);
    }

    private TypeRef? MapResult(MethodInfo method, string location)
    {
        var type = method.ReturnType;
        if (type == typeof(void) || type == typeof(Task))
        {
            return null;
        }

        var node = Nullability.Of(method.ReturnParameter);
        var resultLocation = $"{location} (result)";
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)
            ? _types.Map(node.Children[0], resultLocation)
            : _types.Map(node, resultLocation);
    }

    private EventModel BuildEvent(Type type, BridgeEventAttribute attribute)
    {
        if (!EventName.IsMatch(attribute.Name))
        {
            _errors.Add($"{type.Name}: event name '{attribute.Name}' must look like 'service.camelCaseName', for example 'library.changed'.");
        }

        if (!TypeMapper.IsObjectType(type))
        {
            _errors.Add($"{type.Name}: [BridgeEvent] goes on DTO classes.");
        }
        else
        {
            _types.Map(NullabilityNode.NotNull(type), type.Name);
        }

        return new EventModel(attribute.Name, type, attribute.Invalidates ?? Array.Empty<string>());
    }

    private void CheckInvalidations(IEnumerable<EventModel> events, IEnumerable<ServiceModel> services)
    {
        var methods = new HashSet<string>(services.SelectMany(s => s.Methods).Select(m => m.WireName), StringComparer.Ordinal);
        foreach (var evt in events)
        {
            foreach (var unknown in evt.Invalidates.Where(m => !methods.Contains(m)))
            {
                _errors.Add($"{evt.PayloadName}: Invalidates names '{unknown}', which is not a method of the contract.");
            }
        }
    }

    private void RequireUnique(string kind, IEnumerable<string> names)
    {
        foreach (var duplicate in names.GroupBy(n => n).Where(g => g.Count() > 1))
        {
            _errors.Add($"Two {kind}s are named '{duplicate.Key}'.");
        }
    }
}
