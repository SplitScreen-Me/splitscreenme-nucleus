using System.Linq;
using Nucleus.Bridge.Model;
using Xunit;

namespace Nucleus.Bridge.Tests;

/// <summary>The contract pieces that replaced native message boxes: notices, session crash details, setting confirmations.</summary>
public class NoticesContractTests
{
    private static readonly ContractModel Model = ContractModel.Default;

    private static string? PropertyType(string type, string property) =>
        Model.Types.OfType<ObjectTypeModel>().Single(t => t.Name == type).Properties.SingleOrDefault(p => p.Name == property)?.Type.ToString();

    [Fact]
    public void The_notices_service_lists_and_dismisses()
    {
        ServiceModel notices = Assert.Single(Model.Services, s => s.Name == "notices");
        Assert.Equal(new[] { "dismiss", "list" }, notices.Methods.Select(m => m.Name).OrderBy(n => n));

        MethodModel list = notices.Methods.Single(m => m.Name == "list");
        Assert.Null(list.Params);
        Assert.Equal("list<AppNotice>", list.Result?.ToString());
        Assert.Equal("DismissNoticeRequest", notices.Methods.Single(m => m.Name == "dismiss").Params?.ToString());
    }

    [Fact]
    public void Notices_changed_refetches_the_list()
    {
        EventModel changed = Assert.Single(Model.Events, e => e.Name == "notices.changed");
        Assert.Equal(new[] { "notices.list" }, changed.Invalidates);
    }

    [Theory]
    [InlineData("AppNotice", "id", "string")]
    [InlineData("AppNotice", "level", "NoticeLevel")]
    [InlineData("AppNotice", "source", "NoticeSource")]
    [InlineData("AppNotice", "title", "string")]
    [InlineData("AppNotice", "message", "string")]
    [InlineData("AppNotice", "at", "date")]
    [InlineData("SessionSnapshot", "errorDetails", "string?")]
    [InlineData("SettingField", "confirm", "SettingConfirmation?")]
    [InlineData("SettingConfirmation", "values", "list<string>")]
    [InlineData("SettingConfirmation", "confirmLabel", "string")]
    public void New_properties_have_the_expected_wire_types(string type, string property, string expected)
    {
        Assert.Equal(expected, PropertyType(type, property));
    }

    [Fact]
    public void Notice_levels_are_info_warning_error()
    {
        EnumTypeModel level = Model.Types.OfType<EnumTypeModel>().Single(t => t.Name == "NoticeLevel");
        Assert.Equal(new[] { "info", "warning", "error" }, level.Members.Select(m => m.Value));
    }
}
