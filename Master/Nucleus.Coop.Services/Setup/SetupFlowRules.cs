using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>State of one custom step, as seen by the flow rules.</summary>
    internal sealed class CustomStepState
    {
        public CustomStepState(string title, bool required, bool selected)
        {
            Title = title;
            Required = required;
            Selected = selected;
        }

        /// <summary>Title declared by the handler.</summary>
        public string Title { get; }

        /// <summary>Required after the handler's UpdateRequired callback. Optional steps are not shown.</summary>
        public bool Required { get; }

        /// <summary>An item is selected.</summary>
        public bool Selected { get; }
    }

    /// <summary>Identifies a step independently of which steps are shown (the list changes when a custom step becomes optional).</summary>
    internal readonly struct StepId
    {
        public StepId(SetupStepKind kind, int customIndex = -1)
        {
            Kind = kind;
            CustomIndex = kind == SetupStepKind.Custom ? customIndex : -1;
        }

        public SetupStepKind Kind { get; }

        public int CustomIndex { get; }

        public static StepId Players => new StepId(SetupStepKind.Players);

        public static StepId Options => new StepId(SetupStepKind.Options);

        public static StepId Custom(int index) => new StepId(SetupStepKind.Custom, index);

        public bool Equals(StepId other) => Kind == other.Kind && CustomIndex == other.CustomIndex;

        public override string ToString() => Kind == SetupStepKind.Custom ? $"custom:{CustomIndex}" : Kind.ToString();
    }

    /// <summary>Inputs of the flow rules.</summary>
    internal sealed class FlowInput
    {
        /// <summary>The players step is valid: at least one placed device, or every profile player (and guest) matched.</summary>
        public bool PlayersValid { get; set; }

        /// <summary>At least one handler option is shown on the options step (options driven by a custom step are hidden).</summary>
        public bool HasVisibleOptions { get; set; }

        /// <summary>Custom steps, in handler order.</summary>
        public IReadOnlyList<CustomStepState> CustomSteps { get; set; } = new List<CustomStepState>();
    }

    /// <summary>
    /// The setup step machine as a pure function. Replaces Core_Interface.GoToStep/StepCanPlay and the Play button state:
    /// players, then options (only when an option is visible), then every required custom step (all of them, not only
    /// the first one as in the WinForms UI). Play is possible on the last shown step once it is complete and the players
    /// step is still valid.
    /// </summary>
    internal static class SetupFlowRules
    {
        /// <summary>Title of the players step, in the language in use.</summary>
        public static string PlayersTitle => I18n.T("host.setup.steps.players");

        /// <summary>Title of the options step, in the language in use.</summary>
        public static string OptionsTitle => I18n.T("host.setup.steps.options");

        /// <summary>The steps that will actually be shown, in order.</summary>
        public static List<StepId> VisibleSteps(FlowInput input)
        {
            List<StepId> steps = new List<StepId> { StepId.Players };

            if (input.HasVisibleOptions)
            {
                steps.Add(StepId.Options);
            }

            for (int i = 0; i < input.CustomSteps.Count; i++)
            {
                if (input.CustomSteps[i].Required)
                {
                    steps.Add(StepId.Custom(i));
                }
            }

            return steps;
        }

        /// <summary>
        /// Position of <paramref name="current"/> in <paramref name="steps"/>. A step that is no longer shown resolves to
        /// the closest shown step before it.
        /// </summary>
        public static int IndexOf(List<StepId> steps, StepId current)
        {
            int index = steps.FindIndex(s => s.Equals(current));

            if (index >= 0)
            {
                return index;
            }

            int rank = Rank(current);
            int best = 0;

            for (int i = 0; i < steps.Count; i++)
            {
                if (Rank(steps[i]) < rank)
                {
                    best = i;
                }
            }

            return best;
        }

        private static int Rank(StepId step)
        {
            switch (step.Kind)
            {
                case SetupStepKind.Players: return 0;
                case SetupStepKind.Options: return 1;
                default: return 2 + step.CustomIndex;
            }
        }

        /// <summary>Whether the user finished <paramref name="step"/>.</summary>
        public static bool IsComplete(FlowInput input, StepId step)
        {
            switch (step.Kind)
            {
                case SetupStepKind.Players:
                    return input.PlayersValid;
                case SetupStepKind.Options:
                    return true;//defaults are pre-filled
                default:
                    return step.CustomIndex >= 0 && step.CustomIndex < input.CustomSteps.Count && input.CustomSteps[step.CustomIndex].Selected;
            }
        }

        /// <summary>The flow DTO for the current step.</summary>
        public static SetupFlow Compute(FlowInput input, StepId current)
        {
            List<StepId> steps = VisibleSteps(input);
            int index = IndexOf(steps, current);
            int last = steps.Count - 1;

            return new SetupFlow
            {
                Steps = steps.Select(s => new SetupStep
                {
                    Kind = s.Kind,
                    CustomStepIndex = s.Kind == SetupStepKind.Custom ? s.CustomIndex : (int?)null,
                    Title = TitleOf(input, s),
                }).ToList(),
                Current = index,
                CanGoBack = index > 0,
                CanGoNext = index < last && IsComplete(input, steps[index]),
                CanPlay = input.PlayersValid && index == last && IsComplete(input, steps[index]),
            };
        }

        private static string TitleOf(FlowInput input, StepId step)
        {
            switch (step.Kind)
            {
                case SetupStepKind.Players:
                    return PlayersTitle;
                case SetupStepKind.Options:
                    return OptionsTitle;
                default:
                    return input.CustomSteps[step.CustomIndex].Title ?? "";
            }
        }

        /// <summary>The step after <paramref name="current"/>, or null when it is the last one or not complete.</summary>
        public static StepId? Next(FlowInput input, StepId current)
        {
            List<StepId> steps = VisibleSteps(input);
            int index = IndexOf(steps, current);

            if (index >= steps.Count - 1 || !IsComplete(input, steps[index]))
            {
                return null;
            }

            return steps[index + 1];
        }

        /// <summary>The step before <paramref name="current"/>, or null on the first step.</summary>
        public static StepId? Back(FlowInput input, StepId current)
        {
            List<StepId> steps = VisibleSteps(input);
            int index = IndexOf(steps, current);

            return index > 0 ? steps[index - 1] : (StepId?)null;
        }

        /// <summary>
        /// Where to go once a loaded profile has matched every player: the first required custom step without a selection,
        /// else the last shown step (the options step is skipped when custom steps exist, as in the WinForms UI).
        /// </summary>
        public static StepId AfterProfileMatched(FlowInput input)
        {
            List<StepId> steps = VisibleSteps(input);

            foreach (StepId step in steps)
            {
                if (step.Kind == SetupStepKind.Custom && !IsComplete(input, step))
                {
                    return step;
                }
            }

            return steps[steps.Count - 1];
        }
    }
}
