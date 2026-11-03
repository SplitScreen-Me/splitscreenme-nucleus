using System.Collections.Generic;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>A profile saved before the handler options changed (Nucleus 2.5: the profile settings follow the handler).</summary>
public sealed class ProfileOptionsMergeTests
{
    private static Dictionary<string, object> Handler(params (string Key, object Value)[] options)
    {
        var result = new Dictionary<string, object>();
        foreach ((string key, object value) in options)
        {
            result[key] = value;
        }

        return result;
    }

    private static List<KeyValuePair<string, string>> Saved(params (string Key, string Value)[] options)
    {
        var result = new List<KeyValuePair<string, string>>();
        foreach ((string key, string value) in options)
        {
            result.Add(new KeyValuePair<string, string>(key, value));
        }

        return result;
    }

    [Fact]
    public void Same_options_take_the_saved_values()
    {
        Dictionary<string, object> merged = GameProfile.MergeProfileOptions(Handler(("Resolution", "1080p"), ("Mode", "")), Saved(("Resolution", "720p"), ("Mode", "Coop")), out bool outdated);

        Assert.False(outdated);
        Assert.Equal("720p", merged["Resolution"]);
        Assert.Equal("Coop", merged["Mode"]);
    }

    [Fact]
    public void Option_added_to_the_handler_gets_its_default_and_marks_the_profile()
    {
        Dictionary<string, object> merged = GameProfile.MergeProfileOptions(Handler(("Resolution", "1080p"), ("NewOption", true)), Saved(("Resolution", "720p")), out bool outdated);

        Assert.True(outdated);
        Assert.Equal("720p", merged["Resolution"]);
        Assert.Equal(true, merged["NewOption"]);
    }

    [Fact]
    public void Option_removed_from_the_handler_is_dropped_and_marks_the_profile()
    {
        Dictionary<string, object> merged = GameProfile.MergeProfileOptions(Handler(("Resolution", "1080p")), Saved(("Resolution", "720p"), ("Gone", "x")), out bool outdated);

        Assert.True(outdated);
        Assert.False(merged.ContainsKey("Gone"));
        Assert.Equal("720p", merged["Resolution"]);
    }

    [Fact]
    public void Renamed_option_with_the_same_count_is_outdated()
    {
        Dictionary<string, object> merged = GameProfile.MergeProfileOptions(Handler(("A", 1), ("C", 3)), Saved(("A", "10"), ("B", "20")), out bool outdated);

        Assert.True(outdated);
        Assert.Equal("10", merged["A"]);
        Assert.Equal(3, merged["C"]);
        Assert.False(merged.ContainsKey("B"));
    }

    [Fact]
    public void No_saved_options()
    {
        Dictionary<string, object> merged = GameProfile.MergeProfileOptions(Handler(), null!, out bool outdated);

        Assert.False(outdated);
        Assert.Empty(merged);
    }
}
