using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Tests.Engine;
using Nucleus.Gaming;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

/// <summary>
/// A handler with <c>Game.Hook.SDL3Enabled</c> (Nucleus 2.5) gets the same lobby as an SDL2 one: SDL pads (devices and
/// kinds are covered by DeviceIdsTests and SetupEngineTests), both gamepad icons, SDL started for its setup.
/// </summary>
public class Sdl3HandlerTests
{
    private static GenericGameInfo Handler(string hooks) =>
        EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("sdl3"), "SDL3 game", "game.exe", hooks));

    [Fact]
    public void An_SDL3_handler_reads_as_an_SDL_handler()
    {
        GenericGameInfo game = Handler("Game.Hook.SDL3Enabled = true;\nGame.Hook.XInputEnabled = false;\nGame.Hook.DInputEnabled = false;");

        Assert.True(game.Hook.SDL3Enabled);
        Assert.True(GameDtoFactory.Flags(game).Sdl2Enabled, "the library flags of SDL handlers");
        GameInputs inputs = GameInputRules.ToInputs(GameDtoFactory.Flags(game));
        Assert.True(inputs.XInput && inputs.DInput, "both gamepad icons, as SDL2 handlers");
        Assert.True(GameInputRules.SupportsGamepads(GameDtoFactory.Flags(game)));
        Assert.True(DevicesFunctions.UsesSdl(game), "SDL starts for its setup");
    }

    [Fact]
    public void A_handler_without_SDL_does_not_start_it()
    {
        GenericGameInfo game = Handler("Game.Hook.XInputEnabled = true;");

        Assert.False(DevicesFunctions.UsesSdl(game));
        Assert.False(GameDtoFactory.Flags(game).Sdl2Enabled);
    }

    [Fact]
    public void The_OneDrive_warning_links_the_v3_documentation()
    {
        //The check of the lobby and the startup warning (Nucleus 2.5 linked the FAQ).
        Assert.Equal(HubSite.Url + "/doc/troubleshooting/handler-does-not-work#documents-in-onedrive", HubSite.DocUrl(HubSite.OneDriveHelpPage));
        Assert.Equal(HubSite.Url + "/doc/faq", HubSite.DocUrl("/faq"));
    }
}
