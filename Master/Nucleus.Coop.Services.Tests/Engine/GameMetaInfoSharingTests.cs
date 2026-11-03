using Nucleus.Gaming;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>
/// KeepSymLink per-game option (Nucleus 2.5 fix "content deleted on game selection"): every parse of a handler shares the
/// game's metaInfo values, so a stale GenericGameInfo can't write an old value back.
/// </summary>
public sealed class GameMetaInfoSharingTests
{
    [Fact]
    public void Parses_of_the_same_game_share_their_meta_info_values()
    {
        string guid = EngineTestHandlers.NewGuid("meta");
        GenericGameInfo library = EngineTestHandlers.Parse(EngineTestHandlers.Script(guid, "Meta", "meta.exe"));
        GenericGameInfo session = EngineTestHandlers.Parse(EngineTestHandlers.Script(guid, "Meta", "meta.exe"));

        Assert.False(session.MetaInfo.KeepSymLink);

        library.MetaInfo.KeepSymLink = true;

        Assert.True(session.MetaInfo.KeepSymLink);

        //The older object saving another value (play time thread, favourite...) keeps the toggle.
        session.MetaInfo.Favorite = true;
        Assert.True(library.MetaInfo.KeepSymLink);
        Assert.True(library.MetaInfo.Favorite);

        GenericGameInfo later = EngineTestHandlers.Parse(EngineTestHandlers.Script(guid, "Meta", "meta.exe"));
        Assert.True(later.MetaInfo.KeepSymLink);
    }

    [Fact]
    public void Other_games_keep_their_own_values()
    {
        GenericGameInfo first = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("meta-a"), "A", "a.exe"));
        GenericGameInfo second = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("meta-b"), "B", "b.exe"));

        first.MetaInfo.KeepSymLink = true;

        Assert.False(second.MetaInfo.KeepSymLink);
    }

    [Fact]
    public void A_broken_script_does_not_load_meta_info()
    {
        GenericGameInfo broken = EngineTestHandlers.Parse("Game.GUID = \"" + EngineTestHandlers.NewGuid("meta-broken") + "\"; (");

        Assert.NotNull(broken.ScriptError);
    }
}
