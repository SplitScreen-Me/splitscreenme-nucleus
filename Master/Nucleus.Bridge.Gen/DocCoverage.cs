using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Model;

namespace Nucleus.Bridge.Gen;

/// <summary>Finds the parts of a contract that have no XML documentation (the generator warns about them).</summary>
public static class DocCoverage
{
    /// <summary>Names of the undocumented services, methods, types, properties and enum members.</summary>
    public static IEnumerable<string> Undocumented(ContractModel model)
    {
        foreach (var service in model.Services)
        {
            if (service.Doc == null)
            {
                yield return $"service {service.Name} ({service.Interface.Name})";
            }

            foreach (var method in service.Methods.Where(m => m.Doc == null))
            {
                yield return $"method {method.WireName}";
            }
        }

        foreach (var type in model.Types)
        {
            if (type.Doc == null)
            {
                yield return $"type {type.Name}";
            }

            var members = type switch
            {
                ObjectTypeModel dto => dto.Properties.Where(p => p.Doc == null).Select(p => p.Name),
                EnumTypeModel enumType => enumType.Members.Where(m => m.Doc == null).Select(m => m.Value),
                _ => Enumerable.Empty<string>(),
            };
            foreach (var member in members)
            {
                yield return $"member {type.Name}.{member}";
            }
        }
    }
}
