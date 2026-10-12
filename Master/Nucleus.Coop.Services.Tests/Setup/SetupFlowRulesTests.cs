using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Setup;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

public class SetupFlowRulesTests
{
    private static FlowInput Input(bool playersValid, bool options, params (bool required, bool selected)[] steps) => new FlowInput
    {
        PlayersValid = playersValid,
        HasVisibleOptions = options,
        CustomSteps = steps.Select((s, i) => new CustomStepState("Step " + i, s.required, s.selected)).ToList(),
    };

    [Fact]
    public void Without_options_play_is_possible_from_the_players_step()
    {
        SetupFlow flow = SetupFlowRules.Compute(Input(true, false), StepId.Players);

        Assert.Single(flow.Steps);
        Assert.Equal(SetupStepKind.Players, flow.Steps[0].Kind);
        Assert.True(flow.CanPlay);
        Assert.False(flow.CanGoNext);
        Assert.False(flow.CanGoBack);
    }

    [Fact]
    public void Play_needs_a_valid_players_step()
    {
        Assert.False(SetupFlowRules.Compute(Input(false, false), StepId.Players).CanPlay);
        Assert.False(SetupFlowRules.Compute(Input(false, true), StepId.Options).CanPlay);
    }

    [Fact]
    public void Options_step_comes_after_players_and_is_the_last_one()
    {
        FlowInput input = Input(true, true);

        SetupFlow onPlayers = SetupFlowRules.Compute(input, StepId.Players);
        Assert.Equal(new[] { SetupStepKind.Players, SetupStepKind.Options }, onPlayers.Steps.Select(s => s.Kind));
        Assert.True(onPlayers.CanGoNext);
        Assert.False(onPlayers.CanPlay);

        SetupFlow onOptions = SetupFlowRules.Compute(input, StepId.Options);
        Assert.Equal(1, onOptions.Current);
        Assert.True(onOptions.CanPlay);
        Assert.True(onOptions.CanGoBack);
    }

    [Fact]
    public void Next_is_refused_while_the_players_step_is_invalid()
    {
        Assert.False(SetupFlowRules.Compute(Input(false, true), StepId.Players).CanGoNext);
        Assert.Null(SetupFlowRules.Next(Input(false, true), StepId.Players));
    }

    [Fact]
    public void Every_required_custom_step_is_shown_not_only_the_first_one()
    {
        FlowInput input = Input(true, true, (true, false), (false, false), (true, false));
        List<StepId> steps = SetupFlowRules.VisibleSteps(input);

        Assert.Equal(new[] { "Players", "Options", "custom:0", "custom:2" }, steps.Select(s => s.ToString()));
    }

    [Fact]
    public void A_custom_step_without_selection_blocks_next_and_play()
    {
        FlowInput input = Input(true, false, (true, false), (true, true));

        Assert.False(SetupFlowRules.Compute(input, StepId.Custom(0)).CanGoNext);
        Assert.True(SetupFlowRules.Compute(Input(true, false, (true, true), (true, false)), StepId.Custom(0)).CanGoNext);
        Assert.False(SetupFlowRules.Compute(Input(true, false, (true, true), (true, false)), StepId.Custom(1)).CanPlay);
        Assert.True(SetupFlowRules.Compute(Input(true, false, (true, true), (true, true)), StepId.Custom(1)).CanPlay);
    }

    [Fact]
    public void Optional_custom_steps_are_skipped_so_play_moves_to_the_previous_step()
    {
        FlowInput input = Input(true, true, (false, false));

        Assert.True(SetupFlowRules.Compute(input, StepId.Options).CanPlay);
    }

    [Fact]
    public void A_step_that_is_no_longer_shown_resolves_to_the_previous_shown_step()
    {
        FlowInput input = Input(true, true, (false, false), (true, false));
        List<StepId> steps = SetupFlowRules.VisibleSteps(input);

        Assert.Equal(1, SetupFlowRules.IndexOf(steps, StepId.Custom(0)));//options
        Assert.Equal(2, SetupFlowRules.IndexOf(steps, StepId.Custom(1)));
        Assert.Equal(0, SetupFlowRules.IndexOf(SetupFlowRules.VisibleSteps(Input(true, false)), StepId.Options));
    }

    [Fact]
    public void Back_and_next_walk_the_shown_steps()
    {
        FlowInput input = Input(true, true, (true, true));

        Assert.Equal("Options", SetupFlowRules.Next(input, StepId.Players)!.Value.ToString());
        Assert.Equal("custom:0", SetupFlowRules.Next(input, StepId.Options)!.Value.ToString());
        Assert.Null(SetupFlowRules.Next(input, StepId.Custom(0)));
        Assert.Equal("Options", SetupFlowRules.Back(input, StepId.Custom(0))!.Value.ToString());
        Assert.Null(SetupFlowRules.Back(input, StepId.Players));
    }

    [Fact]
    public void A_matched_profile_goes_to_the_first_custom_step_without_a_card()
    {
        Assert.Equal("custom:1", SetupFlowRules.AfterProfileMatched(Input(true, true, (true, true), (true, false))).ToString());
        Assert.Equal("custom:1", SetupFlowRules.AfterProfileMatched(Input(true, true, (true, true), (true, true))).ToString());
        Assert.Equal("Options", SetupFlowRules.AfterProfileMatched(Input(true, true)).ToString());
        Assert.Equal("Players", SetupFlowRules.AfterProfileMatched(Input(true, false)).ToString());
    }

    [Fact]
    public void Step_titles_come_from_the_handler_for_custom_steps()
    {
        SetupFlow flow = SetupFlowRules.Compute(Input(true, true, (true, false)), StepId.Players);

        Assert.Equal(new[] { SetupFlowRules.PlayersTitle, SetupFlowRules.OptionsTitle, "Step 0" }, flow.Steps.Select(s => s.Title));
        Assert.Equal(0, flow.Steps[2].CustomStepIndex);
        Assert.Null(flow.Steps[0].CustomStepIndex);
    }
}
