using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nucleus.Bridge.Model;

/// <summary>
/// The XML documentation file the compiler writes next to an assembly (<c>GenerateDocumentationFile</c>).
/// <see cref="ContractModel.Build(System.Reflection.Assembly, XmlDocs)"/> uses it to fill the <c>Doc</c> properties,
/// rendered as Markdown: <c>&lt;see cref&gt;</c> becomes the wire name in backticks (<c>`library.list`</c>,
/// <c>`GameSummary.name`</c>), <c>&lt;c&gt;</c> becomes inline code, <c>&lt;para&gt;</c> a new paragraph.
/// </summary>
public sealed class XmlDocs
{
    private static readonly Regex Whitespace = new(@"\s+");
    private static readonly Regex ExtraBlankLines = new(@"\n{3,}");

    private readonly Dictionary<string, XElement> _members;

    private XmlDocs(Dictionary<string, XElement> members) => _members = members;

    /// <summary>Reads an XML documentation file.</summary>
    /// <param name="path">Path of the <c>.xml</c> file, usually next to the assembly.</param>
    public static XmlDocs Load(string path)
    {
        var members = new Dictionary<string, XElement>(StringComparer.Ordinal);
        foreach (var member in XDocument.Load(path).Descendants("member"))
        {
            var id = (string?)member.Attribute("name");
            if (id != null)
            {
                // Overloads are not allowed in the contract, so methods are found without their parameter list.
                members[StripParameters(id)] = member;
            }
        }

        return new XmlDocs(members);
    }

    /// <summary>Fills the <c>Doc</c> properties of every node of the model.</summary>
    internal void Apply(ContractModel model)
    {
        var symbols = Symbols(model);
        string? Render(string id) => _members.TryGetValue(id, out var member) ? RenderMember(member, symbols) : null;

        foreach (var service in model.Services)
        {
            service.Doc = Render("T:" + DocName(service.Interface));
            foreach (var method in service.Methods)
            {
                method.Doc = Render("M:" + DocName(service.Interface) + "." + method.Method.Name);
            }
        }

        foreach (var type in model.Types)
        {
            type.Doc = Render("T:" + DocName(type.ClrType));
            switch (type)
            {
                case ObjectTypeModel dto:
                    foreach (var property in dto.Properties)
                    {
                        property.Doc = Render(MemberId(property));
                    }

                    break;
                case EnumTypeModel enumType:
                    foreach (var member in enumType.Members)
                    {
                        member.Doc = Render("F:" + DocName(type.ClrType) + "." + member.Field.Name);
                    }

                    break;
            }
        }

        foreach (var evt in model.Events)
        {
            evt.Doc = Render("T:" + DocName(evt.PayloadType));
        }
    }

    /// <summary>Documentation ids of the contract, mapped to the names the UI sees.</summary>
    private static Dictionary<string, string> Symbols(ContractModel model)
    {
        var symbols = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var service in model.Services)
        {
            symbols["T:" + DocName(service.Interface)] = service.Name;
            foreach (var method in service.Methods)
            {
                symbols["M:" + DocName(service.Interface) + "." + method.Method.Name] = method.WireName;
            }
        }

        foreach (var type in model.Types)
        {
            symbols["T:" + DocName(type.ClrType)] = type.Name;
            if (type is ObjectTypeModel dto)
            {
                foreach (var property in dto.Properties)
                {
                    symbols[MemberId(property)] = type.Name + "." + property.Name;
                }
            }
            else if (type is EnumTypeModel enumType)
            {
                foreach (var member in enumType.Members)
                {
                    symbols["F:" + DocName(type.ClrType) + "." + member.Field.Name] = $"\"{member.Value}\"";
                }
            }
        }

        return symbols;
    }

    private static string? RenderMember(XElement member, Dictionary<string, string> symbols)
    {
        var blocks = new[] { member.Element("summary"), member.Element("remarks") }
            .Where(e => e != null)
            .Select(e =>
            {
                var text = new StringBuilder();
                AppendNodes(e!, text, symbols);
                return Normalize(text.ToString());
            })
            .Where(text => text.Length > 0)
            .ToList();
        return blocks.Count == 0 ? null : string.Join("\n\n", blocks);
    }

    private static void AppendNodes(XElement parent, StringBuilder text, Dictionary<string, string> symbols)
    {
        foreach (var node in parent.Nodes())
        {
            if (node is XText literal)
            {
                text.Append(Whitespace.Replace(literal.Value, " "));
            }
            else if (node is XElement element)
            {
                AppendElement(element, text, symbols);
            }
        }
    }

    private static void AppendElement(XElement element, StringBuilder text, Dictionary<string, string> symbols)
    {
        switch (element.Name.LocalName)
        {
            case "see":
                var cref = (string?)element.Attribute("cref");
                var href = (string?)element.Attribute("href");
                var label = Whitespace.Replace(element.Value, " ").Trim();
                if (cref != null)
                {
                    text.Append('`').Append(label.Length > 0 ? label : Resolve(cref, symbols)).Append('`');
                }
                else if (href != null)
                {
                    text.Append(label.Length > 0 ? $"[{label}]({href})" : href);
                }
                else
                {
                    text.Append('`').Append((string?)element.Attribute("langword")).Append('`');
                }

                break;
            case "c":
                text.Append('`').Append(element.Value.Trim()).Append('`');
                break;
            case "paramref":
            case "typeparamref":
                text.Append('`').Append((string?)element.Attribute("name")).Append('`');
                break;
            case "para":
                text.Append("\n\n");
                AppendNodes(element, text, symbols);
                text.Append("\n\n");
                break;
            case "br":
                text.Append('\n');
                break;
            case "item":
                text.Append("\n- ");
                AppendNodes(element, text, symbols);
                break;
            case "seealso":
                break;
            default:
                AppendNodes(element, text, symbols);
                break;
        }
    }

    /// <summary>Wire name of a documentation id, or its last segment when it is outside the contract.</summary>
    private static string Resolve(string cref, Dictionary<string, string> symbols)
    {
        var id = StripParameters(cref);
        if (symbols.TryGetValue(id, out var name))
        {
            return name;
        }

        var qualified = id.Substring(id.IndexOf(':') + 1);
        return qualified.Substring(qualified.LastIndexOf('.') + 1);
    }

    private static string Normalize(string text)
    {
        var lines = text.Split('\n').Select(line => line.Trim());
        return ExtraBlankLines.Replace(string.Join("\n", lines), "\n\n").Trim();
    }

    private static string MemberId(PropertyModel property) =>
        (property.Member is System.Reflection.FieldInfo ? "F:" : "P:") + DocName(property.Member.DeclaringType!) + "." + property.Member.Name;

    private static string DocName(Type type) => type.FullName!.Replace('+', '.');

    private static string StripParameters(string id)
    {
        var paren = id.IndexOf('(');
        return paren < 0 ? id : id.Substring(0, paren);
    }
}
