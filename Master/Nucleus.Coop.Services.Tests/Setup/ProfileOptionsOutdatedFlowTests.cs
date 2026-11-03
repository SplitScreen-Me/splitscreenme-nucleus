using System.Linq;
using Nucleus.Coop.Services.Setup;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

/// <summary>A profile saved with other handler options goes to the options step before playing (Nucleus 2.5).</summary>
public class ProfileOptionsOutdatedFlowTests
{
    private static FlowInput Input(bool options, bool outdated, params (bool required, bool selected)[] steps) => new FlowInput
    {
        PlayersValid = true,
        HasVisibleOptions = options,
        ProfileOptionsOutdated = outdated,
        CustomSteps = steps.Select((s, i) => new CustomStepState("Step " + i, s.required, s.selected)).ToList(),
    };

    [Fact]
    public void Outdated_options_go_to_the_options_step_even_with_custom_steps()
    {
        Assert.Equal("Options", SetupFlowRules.AfterProfileMatched(Input(true, true, (true, true))).ToString());
        Assert.Equal("custom:0", SetupFlowRules.AfterProfileMatched(Input(true, false, (true, true))).ToString());
    }

    [Fact]
    public void Without_visible_options_the_usual_target_is_kept()
    {
        Assert.Equal("custom:0", SetupFlowRules.AfterProfileMatched(Input(false, true, (true, false))).ToString());
        Assert.Equal("Players", SetupFlowRules.AfterProfileMatched(Input(false, true)).ToString());
    }
}
