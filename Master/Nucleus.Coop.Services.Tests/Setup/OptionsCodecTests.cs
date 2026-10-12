using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

public class OptionsCodecTests
{
    private static ExpandoObject Card(string name, string? details = null)
    {
        ExpandoObject card = new ExpandoObject();
        IDictionary<string, object> dict = card;
        dict["Name"] = name;

        if (details != null)
        {
            dict["Details"] = details;
        }

        dict["Extra"] = 42.0;
        return card;
    }

    [Fact]
    public void Kind_follows_the_options_control_rules()
    {
        Assert.Equal(OptionKind.Choice, OptionsCodec.KindOf(new GameOption("n", "d", "k", new[] { "No", "Yes" })));
        Assert.Equal(OptionKind.Text, OptionsCodec.KindOf(new GameOption("n", "d", "k", new object[0])));
        Assert.Equal(OptionKind.Choice, OptionsCodec.KindOf(new GameOption("n", "d", "k", DayOfWeek.Monday)));
        Assert.Equal(OptionKind.Unsupported, OptionsCodec.KindOf(new GameOption("n", "d", "k", true)));
        Assert.Equal(OptionKind.Unsupported, OptionsCodec.KindOf(new GameOption("n", "d", "k", 5.0)));
    }

    [Fact]
    public void A_list_option_starts_with_its_first_item()
    {
        GameOption option = new GameOption("n", "d", "k", new[] { "No", "Yes" });

        Assert.Equal("", option.Value);//GameOption keeps "" for lists
        Assert.Equal("No", OptionsCodec.DefaultValue(option));
        Assert.Equal(DayOfWeek.Friday, OptionsCodec.DefaultValue(new GameOption("n", "d", "k", DayOfWeek.Friday)));
    }

    [Fact]
    public void Resolve_finds_the_same_object_first()
    {
        ExpandoObject a = Card("A"), b = Card("B");
        GameOption option = new GameOption("n", "d", "k", new object[] { a, b });

        Assert.Equal(1, OptionsCodec.Resolve(option, b));
    }

    [Fact]
    public void Resolve_maps_profile_text_back_to_list_items()
    {
        Assert.Equal(1, OptionsCodec.Resolve(new GameOption("n", "d", "k", new[] { "No", "Yes" }), "Yes"));
        Assert.Equal(2, OptionsCodec.Resolve(new GameOption("n", "d", "k", new object[] { 1.0, 1.5, 2.0 }), "2"));
        Assert.Equal(1, OptionsCodec.Resolve(new GameOption("n", "d", "k", new object[] { 1.0, 1.5, 2.0 }), 1.5.ToString()));
        Assert.Null(OptionsCodec.Resolve(new GameOption("n", "d", "k", new[] { "No", "Yes" }), "Maybe"));
    }

    [Fact]
    public void Resolve_matches_enums_by_name()
    {
        GameOption option = new GameOption("n", "d", "k", DayOfWeek.Monday);

        Assert.Equal((int)DayOfWeek.Wednesday, OptionsCodec.Resolve(option, "Wednesday"));
        Assert.Equal((int)DayOfWeek.Sunday, OptionsCodec.Resolve(option, DayOfWeek.Sunday));
        Assert.Null(OptionsCodec.Resolve(option, "Someday"));
    }

    [Fact]
    public void Resolve_matches_a_saved_card_by_name_not_by_substring()
    {
        GameOption option = new GameOption("n", "d", "k", new object[] { Card("Mod"), Card("Mod 2") });
        string saved = JsonConvert.SerializeObject(Card("Mod 2"));//what a profile stores

        Assert.Equal(1, OptionsCodec.Resolve(option, saved));
        Assert.Equal(0, OptionsCodec.Resolve(option, JsonConvert.SerializeObject(Card("Mod"))));
    }

    [Fact]
    public void Normalize_stores_the_original_item_objects()
    {
        ExpandoObject card = Card("B");
        GameOption choice = new GameOption("n", "d", "k", new object[] { 1.0, 2.0 });
        GameOption step = new GameOption("n", "d", "s", new object[] { Card("A"), card }) { Hidden = true };

        Assert.IsType<double>(OptionsCodec.Normalize(choice, "2", out bool matched));
        Assert.True(matched);
        Assert.Same(card, OptionsCodec.Normalize(step, JsonConvert.SerializeObject(Card("B")), out matched));
        Assert.True(matched);
    }

    [Fact]
    public void Unknown_values_fall_back_to_defaults_and_custom_steps_stay_unselected()
    {
        GameOption choice = new GameOption("n", "d", "k", new[] { "No", "Yes" });
        GameOption step = new GameOption("n", "d", "s", new object[] { Card("A") }) { Hidden = true };

        Assert.Equal("No", OptionsCodec.Normalize(choice, "", out bool matched));
        Assert.False(matched);
        Assert.Equal("", OptionsCodec.Normalize(step, "", out matched));
        Assert.False(matched);
        Assert.Equal("", OptionsCodec.Normalize(step, JsonConvert.SerializeObject(Card("Z")), out matched));
    }

    [Fact]
    public void Custom_steps_only_select_cards()
    {
        GameOption step = new GameOption("n", "d", "s", new object[] { "plain text", Card("A") }) { Hidden = true };

        Assert.Null(OptionsCodec.ResolveStepItem(step, "plain text"));
        Assert.Equal(1, OptionsCodec.ResolveStepItem(step, "{\"Name\":\"A\"}"));
    }

    [Fact]
    public void Merge_adds_new_handler_options_and_reports_restored_cards()
    {
        GameOption keep = new GameOption("n", "d", "keep", new[] { "a", "b" });
        GameOption added = new GameOption("n", "d", "added", new[] { "x", "y" });
        GameOption text = new GameOption("n", "d", "text", new object[0]);
        GameOption step = new GameOption("n", "d", "step", new object[] { Card("A"), Card("B") }) { Hidden = true };

        Dictionary<string, object> values = new Dictionary<string, object>
        {
            ["keep"] = "b",
            ["text"] = "hello",
            ["step"] = "{\"Name\":\"B\",\"Extra\":42.0}",
            ["removed"] = "old",
        };

        HashSet<string> restored = OptionsCodec.Merge(new List<GameOption> { keep, added, text, step }, values);

        Assert.Equal("b", values["keep"]);
        Assert.Equal("x", values["added"]);
        Assert.Equal("hello", values["text"]);
        Assert.Equal("B", OptionsCodec.Label(values["step"]));
        Assert.Equal("old", values["removed"]);
        Assert.Equal(new[] { "step" }, restored);
    }

    [Fact]
    public void Labels_and_card_texts()
    {
        Assert.Equal("Mod", OptionsCodec.Label(Card("Mod", "details")));
        Assert.Equal("details", OptionsCodec.DictionaryText(Card("Mod", "details"), "Details"));
        Assert.Null(OptionsCodec.DictionaryText(Card("Mod"), "ImageUrl"));
        Assert.Equal("Yes", OptionsCodec.Label("Yes"));
    }
}
