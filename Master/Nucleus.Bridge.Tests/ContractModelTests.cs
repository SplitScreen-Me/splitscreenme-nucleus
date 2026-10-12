using System;
using System.IO;
using System.Linq;
using Nucleus.Bridge.Model;
using Nucleus.Bridge.Tests.TestContract;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class ContractModelTests
{
    internal static readonly XmlDocs Docs = XmlDocs.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nucleus.Bridge.Tests.xml"));

    private readonly ContractModel _model = ContractModel.Build(ShelfContract.Types, Docs);

    [Fact]
    public void The_nucleus_contract_is_valid()
    {
        var model = ContractModel.Default;

        Assert.Contains(model.Services, s => s.Name == "app");
        Assert.All(model.Methods, m => Assert.Matches(@"^[a-z][A-Za-z0-9]*\.[a-z][A-Za-z0-9]*$", m.WireName));
    }

    [Fact]
    public void Describes_services_and_methods_with_wire_names()
    {
        var shelf = Assert.Single(_model.Services, s => s.Name == "shelf");
        Assert.Equal(new[] { "clock", "shelf" }, _model.Services.Select(s => s.Name));
        Assert.Equal(new[] { "echo", "fail", "find", "labels", "list", "nameOf", "rename", "wait" }, shelf.Methods.Select(m => m.Name));

        var wait = shelf.Methods.Single(m => m.Name == "wait");
        Assert.Equal("shelf.wait", wait.WireName);
        Assert.True(wait.AcceptsCancellation);
        Assert.Null(wait.Params);
        Assert.Null(wait.Result);
    }

    [Theory]
    [InlineData("list", null, "list<ShelfItem>")]
    [InlineData("find", "FindItemRequest", "ShelfItem?")]
    [InlineData("nameOf", "FindItemRequest", "string?")]
    [InlineData("rename", "RenameRequest", null)]
    [InlineData("labels", null, "map<string>")]
    public void Maps_params_and_results(string method, string? expectedParams, string? expectedResult)
    {
        var model = _model.Methods.Single(m => m.Name == method);

        Assert.Equal(expectedParams, model.Params?.ToString());
        Assert.Equal(expectedResult, model.Result?.ToString());
    }

    [Theory]
    [InlineData("text", "string")]
    [InlineData("maybeText", "string?")]
    [InlineData("count", "number")]
    [InlineData("maybeCount", "number?")]
    [InlineData("big", "number")]
    [InlineData("price", "number?")]
    [InlineData("flag", "boolean")]
    [InlineData("id", "string")]
    [InlineData("at", "date")]
    [InlineData("maybeAt", "date?")]
    [InlineData("offset", "date")]
    [InlineData("color", "Color")]
    [InlineData("maybeColor", "Color?")]
    [InlineData("tags", "list<string>")]
    [InlineData("sparseTags", "list<string?>")]
    [InlineData("names", "list<string>")]
    [InlineData("items", "list<ShelfItem>")]
    [InlineData("grid", "list<list<number>>")]
    [InlineData("labels", "map<string>")]
    [InlineData("itemsById", "map<ShelfItem?>")]
    [InlineData("maybeList", "list<number>?")]
    [InlineData("palette", "list<Color>")]
    [InlineData("samples", "list<number?>")]
    [InlineData("main", "ShelfItem")]
    [InlineData("spare", "ShelfItem?")]
    public void Reads_nullability_and_maps_types(string property, string expected)
    {
        var shapes = (ObjectTypeModel)_model.Types.Single(t => t.Name == "Shapes");

        Assert.Equal(expected, shapes.Properties.Single(p => p.Name == property).Type.ToString());
    }

    [Fact]
    public void Keeps_declaration_order_of_properties()
    {
        var item = (ObjectTypeModel)_model.Types.Single(t => t.Name == "ShelfItem");

        Assert.Equal(new[] { "id", "name", "color" }, item.Properties.Select(p => p.Name));
    }

    [Fact]
    public void Discovers_types_transitively_and_sorts_them()
    {
        Assert.Equal(
            new[] { "Color", "FindItemRequest", "RenameRequest", "Shapes", "ShelfChanged", "ShelfItem", "ShelfSeen" },
            _model.Types.Select(t => t.Name));
        var color = (EnumTypeModel)_model.Types.Single(t => t.Name == "Color");
        Assert.Equal(new[] { "red", "skyBlue" }, color.Members.Select(m => m.Value));
    }

    [Fact]
    public void Describes_events_and_their_invalidations()
    {
        var changed = Assert.Single(_model.Events, e => e.Name == "shelf.changed");

        Assert.Equal(new[] { "shelf.changed", "shelf.seen" }, _model.Events.Select(e => e.Name));
        Assert.Equal("ShelfChanged", changed.PayloadName);
        Assert.Equal(new[] { "shelf.list", "shelf.find" }, changed.Invalidates);
    }

    [Fact]
    public void Renders_xml_docs_with_wire_names()
    {
        var shapes = _model.Types.Single(t => t.Name == "Shapes");
        var spare = ((ObjectTypeModel)shapes).Properties.Single(p => p.Name == "spare");

        Assert.Equal("Items on a shelf. See `ShelfChanged`.", _model.Services.Single(s => s.Name == "shelf").Doc);
        Assert.Equal("The item at `FindItemRequest.index`, or null.", _model.Methods.Single(m => m.Name == "find").Doc);
        Assert.Equal("Every supported shape.\n\nSecond paragraph with `code`.", shapes.Doc);
        Assert.Equal("Optional nested DTO, see `ShelfItem.name` and `\"skyBlue\"`.", spare.Doc);
        Assert.Equal("Items changed. Refetch `shelf.list`.", _model.Events.Single(e => e.Name == "shelf.changed").Doc);
    }

    [Fact]
    public void Hash_is_stable_and_ignores_docs()
    {
        var again = ContractModel.Build(ShelfContract.Types);

        Assert.Matches("^[0-9a-f]{16}$", _model.Hash);
        Assert.Equal(_model.Hash, again.Hash);
        Assert.Equal(_model.ToJson(), again.ToJson());
        Assert.NotEqual(_model.ToJson(includeDocs: true), again.ToJson(includeDocs: true));
    }

    [Fact]
    public void Hash_changes_with_the_wire_shape()
    {
        var smaller = ContractModel.Build(ShelfContract.Types.Where(t => t != typeof(ShelfSeen)));

        Assert.NotEqual(_model.Hash, smaller.Hash);
    }

    [Theory]
    [InlineData(typeof(InvalidContracts.IOverloads), "overloads are not allowed")]
    [InlineData(typeof(InvalidContracts.IGenericDto), "Box<String> is not supported")]
    [InlineData(typeof(InvalidContracts.IUnsupportedTypes), "Unsupported.Anything: Object is not supported")]
    [InlineData(typeof(InvalidContracts.IUnsupportedTypes), "Unsupported.Duration: TimeSpan is not supported")]
    [InlineData(typeof(InvalidContracts.IUnsupportedTypes), "Unsupported.Bytes: Byte[] is not supported")]
    [InlineData(typeof(InvalidContracts.IUnsupportedTypes), "dictionary keys must be strings")]
    [InlineData(typeof(InvalidContracts.ITwoParams), "at most one parameter")]
    [InlineData(typeof(InvalidContracts.IPrimitiveParam), "must be a request DTO class")]
    [InlineData(typeof(InvalidContracts.INullableParam), "cannot be nullable")]
    [InlineData(typeof(InvalidContracts.IBadServiceName), "service name 'Bad-Name'")]
    [InlineData(typeof(InvalidContracts.BadEventName), "event name 'changed'")]
    [InlineData(typeof(InvalidContracts.UnknownInvalidation), "Invalidates names 'shelf.nope'")]
    [InlineData(typeof(InvalidContracts.IFlagsEnum), "[Flags] enums are not supported")]
    [InlineData(typeof(InvalidContracts.IHasProperty), "methods only")]
    [InlineData(typeof(InvalidContracts.IObliviousDto), "Oblivious.Name: the nullability of String is unknown")]
    public void Reports_convention_errors(Type contract, string expected)
    {
        var error = Assert.Throws<ContractModelException>(() => ContractModel.Build(new[] { contract }));

        Assert.Contains(error.Errors, e => e.Contains(expected));
        Assert.Contains(expected, error.Message);
    }

    [Fact]
    public void Reports_every_error_at_once()
    {
        var error = Assert.Throws<ContractModelException>(() => ContractModel.Build(new[] { typeof(InvalidContracts.IUnsupportedTypes) }));

        Assert.Equal(4, error.Errors.Count);
    }
}
