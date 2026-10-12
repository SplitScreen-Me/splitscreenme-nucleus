using System.Linq;
using Nucleus.Coop.Services.Settings;
using Nucleus.Gaming.App.Settings;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Settings;

/// <summary>
/// The legacy "gamepad control of the Nucleus window" (mouse cursor on the left stick) is gone: with the web UI it
/// clicked at the cursor while the page's own gamepad navigation acted on the same press. Only the gamepad type of
/// <c>[XUINav]</c> is still read, and reading it never fails.
/// </summary>
public class LegacyGamepadNavigationTests
{
    [Fact]
    public void The_settings_no_longer_offer_the_mouse_cursor_mode()
    {
        string[] removed = { "gamepad.uiNavigation", "gamepad.leftClick", "gamepad.rightClick", "gamepad.dragDrop", "gamepad.deadzone", "gamepad.lockUiControl", "gamepad.openOsk" };
        var catalog = SettingsCatalog.Create();

        Assert.DoesNotContain(catalog, d => removed.Contains(d.Field.Key));
        Assert.DoesNotContain(SettingsCatalog.GamepadReminderRows, r => removed.Contains(r.Key));
        Assert.Contains(catalog, d => d.Field.Key == "gamepad.type");
    }

    [Theory]
    [InlineData("Xbox", "Xbox")]
    [InlineData("Playstation", "Playstation")]
    [InlineData(" playstation ", "Playstation")]
    [InlineData("", "Xbox")]//missing key: GetPrivateProfileString returns ""
    [InlineData(null, "Xbox")]
    [InlineData("Switch", "Xbox")]//hand-edited
    public void The_gamepad_type_falls_back_to_xbox(string? stored, string expected)
    {
        Assert.Equal(expected, App_GamePadNavigation.NormalizeType(stored!));
    }
}
