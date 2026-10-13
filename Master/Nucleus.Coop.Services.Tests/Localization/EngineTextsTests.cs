using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

/// <summary>Engine texts (launch overlay, OSD, prompts) as the built-in English renders them.</summary>
public class EngineTextsTests
{
    private static LanguageFolder English() => new(("en.json", I18n.BuiltInEnglish()));

    [Fact]
    public void Launch_cards_name_the_device_and_count_the_guests()
    {
        using LanguageFolder folder = English();
        Translator en = folder.Translator();
        string keyboard = en.T("engine.overlay.device.keyboardMouse");

        Assert.Equal("Keyboard & mouse", keyboard);
        Assert.Equal("Keyboard & mouse + 1 more", en.T("engine.overlay.device.withGuests", new { device = keyboard, count = 1 }));
        Assert.Equal("Controller + 3 more", en.T("engine.overlay.device.withGuests", new { device = en.T("engine.overlay.device.controller"), count = 3 }));
        Assert.Equal("P12", en.T("engine.overlay.badge", new { number = 12 }));
        Assert.Equal("Player 2", en.T("engine.overlay.playerFallback", new { number = 2 }));
    }

    [Fact]
    public void Launch_texts_keep_their_values_whole()
    {
        using LanguageFolder folder = English();
        Translator en = folder.Translator();

        Assert.Equal("Starting Portal 2 instance for Alice as Player #2",
            en.T("engine.osd.playerStarting", new { game = "Portal 2", nickname = "Alice", number = 2 }));
        Assert.Equal("Press OK when ready to launch instance 3.", en.T("engine.launch.waitForOk.launchInstance", new { number = 3 }));
        Assert.Equal("IdInWindowTitle: Could not find main window handle for game (pid:12345)",
            en.T("engine.launch.windowNotFound.idInWindowTitle", new { process = "game", pid = "12345" }));
        Assert.Equal("Get ready [Link]{https://example.com} File path: C:\\Games\\a.exe",
            en.T("engine.launch.waitForOk.handlerTextWithPath", new { text = "Get ready [Link]{https://example.com}", path = @"C:\Games\a.exe" }));
        Assert.Equal("Unlock Inputs First (Press End Key)", en.T("engine.osd.unlockInputsFirstKey", new { key = "End" }));
    }

    [Fact]
    public void Counts_pick_their_plural_form()
    {
        using LanguageFolder folder = English();
        Translator en = folder.Translator();

        Assert.Equal("This PC has 1 core: use * or a number up to 1.", en.T("engine.profileSettings.errors.idealProcessor", new { count = 1 }));
        Assert.Equal("This PC has 16 cores: use numbers up to 16, separated by commas.", en.T("engine.profileSettings.errors.affinity", new { count = 16 }));
        Assert.Contains("Limit is 256 characters and the file path is 1,024 characters long.",
            en.T("engine.gameFiles.pathTooLong.message", new { path = @"C:\x", limit = 256, count = 1024 }));
    }

    [Fact]
    public void The_crash_report_keeps_its_sections()
    {
        using LanguageFolder folder = English();
        Translator en = folder.Translator();

        string report = en.T("engine.crashReport.debug", new { version = "3.0.0", type = "IOException", message = "Locked", help = "", stackTrace = "at X" });

        Assert.StartsWith("Nucleus version: 3.0.0\n\nNucleus failed setting up the handler.", report);
        Assert.Contains("[Type]\n\nIOException\n\n[Message]\n\nLocked\n", report);
        Assert.EndsWith("[Stacktrace]\n\nat X", report);
        Assert.Empty(folder.Log);
    }
}
