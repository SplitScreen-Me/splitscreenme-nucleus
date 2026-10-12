using Nucleus.Gaming.Coop.Generic;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    /// <summary>The default <see cref="IUserPromptHandler"/>: the engine's native windows, shown on the calling thread.</summary>
    internal sealed class NativeUserPrompts : IUserPromptHandler
    {
        public PromptResult Ask(PromptRequest request)
        {
            switch (request.Kind)
            {
                case PromptKind.Notice:
                    NucleusMessageBox.Show(request.Title ?? "", request.Message, request.FormatMessage);
                    return new PromptResult { Accepted = true };

                case PromptKind.YesNo:
                    DialogResult answer = MessageBox.Show(request.Message, request.Title ?? "", MessageBoxButtons.YesNo, ToIcon(request.Icon));
                    return new PromptResult { Accepted = answer == DialogResult.Yes };

                case PromptKind.WaitForOk:
                    new Prompt(request.Message).ShowDialog();
                    return new PromptResult { Accepted = true };

                case PromptKind.PickLauncher:
                    Prompt launcherPrompt = new Prompt(request.Message, true, request.FileName);
                    launcherPrompt.ShowDialog();
                    return new PromptResult { Accepted = launcherPrompt.SelectedPath != null, Path = launcherPrompt.SelectedPath };

                case PromptKind.TextInput:
                    CustomPrompt textPrompt = new CustomPrompt(request.Message, request.DefaultText ?? "");
                    textPrompt.ShowDialog();
                    return new PromptResult { Accepted = textPrompt.Answer != null, Text = textPrompt.Answer };

                case PromptKind.PickProcess:
                    ProcessPicker picker = new ProcessPicker(request.Choices);
                    picker.ShowDialog();
                    return new PromptResult { Accepted = picker.SelectedChoice != null, Choice = picker.SelectedChoice };

                default:
                    MessageBox.Show(request.Message, request.Title ?? "", MessageBoxButtons.OK, ToIcon(request.Icon), MessageBoxDefaultButton.Button1,
                        request.DesktopOnly ? MessageBoxOptions.DefaultDesktopOnly : 0);
                    return new PromptResult { Accepted = true };
            }
        }

        private static MessageBoxIcon ToIcon(PromptIcon icon)
        {
            switch (icon)
            {
                case PromptIcon.Info: return MessageBoxIcon.Information;
                case PromptIcon.Warning: return MessageBoxIcon.Warning;
                case PromptIcon.Error: return MessageBoxIcon.Error;
                case PromptIcon.Question: return MessageBoxIcon.Question;
                default: return MessageBoxIcon.None;
            }
        }
    }
}
