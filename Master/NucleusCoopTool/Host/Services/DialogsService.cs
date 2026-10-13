using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Host.Services
{
    /// <summary>Native file and folder pickers, modal to the host window.</summary>
    /// <remarks>
    /// Bridge calls arrive inside the WebView2 WebMessageReceived handler. A modal dialog must not run inside a WebView2
    /// event handler, so each picker is posted to the UI message loop and the call completes when the user answers.
    /// </remarks>
    internal sealed class DialogsService : IDialogsService
    {
        private readonly HostForm owner;

        public DialogsService(HostForm owner)
        {
            this.owner = owner;
        }

        public Task<string> PickFile(PickFileRequest request)
        {
            request = request ?? new PickFileRequest();
            return ShowLater(() =>
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Title = string.IsNullOrWhiteSpace(request.Title) ? I18n.TPlain("host.app.dialogs.selectFile") : request.Title;
                    dialog.Filter = BuildFilter(request.Filters);
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;
                    dialog.Multiselect = false;
                    dialog.RestoreDirectory = true;
                    dialog.DereferenceLinks = true;

                    string initial = ExistingDirectory(request.InitialDirectory);
                    if (initial != null)
                    {
                        dialog.InitialDirectory = initial;
                    }

                    return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
                }
            });
        }

        public Task<string> PickFolder(PickFolderRequest request)
        {
            request = request ?? new PickFolderRequest();
            return ShowLater(() =>
            {
                string title = string.IsNullOrWhiteSpace(request.Title) ? I18n.TPlain("host.app.dialogs.selectFolder") : request.Title;
                string initial = ExistingDirectory(request.InitialDirectory);

                try
                {
                    return FolderPicker.Show(owner.Handle, title, initial);
                }
                catch (Exception ex)
                {
                    HostLog.Warning($"The modern folder picker failed, using FolderBrowserDialog: {ex.Message}");
                }

                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = title;
                    dialog.ShowNewFolderButton = true;
                    if (initial != null)
                    {
                        dialog.SelectedPath = initial;
                    }

                    return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedPath : null;
                }
            });
        }

        private Task<string> ShowLater(Func<string> show)
        {
            TaskCompletionSource<string> completion = new TaskCompletionSource<string>();

            if (owner.IsDisposed || !owner.IsHandleCreated)
            {
                completion.SetResult(null);
                return completion.Task;
            }

            owner.BeginInvoke(new Action(() =>
            {
                try
                {
                    completion.SetResult(owner.IsDisposed ? null : show());
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }));

            return completion.Task;
        }

        /// <summary>"Game executable (*.exe)|*.exe|All files (*.*)|*.*" from the contract filters.</summary>
        internal static string BuildFilter(IEnumerable<FileFilter> filters)
        {
            StringBuilder builder = new StringBuilder();

            foreach (FileFilter filter in filters ?? Enumerable.Empty<FileFilter>())
            {
                if (filter == null)
                {
                    continue;
                }

                List<string> patterns = (filter.Extensions ?? new List<string>())
                    .Select(e => (e ?? "").Trim().TrimStart('*').TrimStart('.'))
                    .Where(e => e.Length > 0 && e.IndexOfAny(new[] { '|', ';', '\\', '/' }) < 0)
                    .Select(e => "*." + e)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (patterns.Count == 0)
                {
                    patterns.Add("*.*");
                }

                string pattern = string.Join(";", patterns);
                string name = string.IsNullOrWhiteSpace(filter.Name) ? pattern : filter.Name.Replace("|", " ").Trim();

                if (builder.Length > 0)
                {
                    builder.Append('|');
                }

                builder.Append(name).Append(" (").Append(pattern).Append(")|").Append(pattern);
            }

            return builder.Length > 0 ? builder.ToString() : I18n.TPlain("host.app.dialogs.allFiles").Replace("|", " ") + " (*.*)|*.*";
        }

        private static string ExistingDirectory(string path)
        {
            string full = ExternalLauncher.NormalizeLocalPath(path);
            return full != null && Directory.Exists(full) ? full : null;
        }
    }
}
