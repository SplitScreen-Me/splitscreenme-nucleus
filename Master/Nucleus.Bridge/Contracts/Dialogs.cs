using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>Native file and folder pickers, owned by the host window.</summary>
    [BridgeService("dialogs")]
    public interface IDialogsService
    {
        /// <summary>Asks the user to pick a file. Returns null when the user cancels.</summary>
        Task<string?> PickFile(PickFileRequest request);

        /// <summary>Asks the user to pick a folder. Returns null when the user cancels.</summary>
        Task<string?> PickFolder(PickFolderRequest request);
    }

    /// <summary>A file type filter, for example "Game executable" with extensions ["exe"].</summary>
    public sealed class FileFilter
    {
        /// <summary>Label shown in the picker.</summary>
        public string Name { get; set; } = "";

        /// <summary>Extensions without the dot.</summary>
        public List<string> Extensions { get; set; } = new List<string>();
    }

    /// <summary>Parameters of <see cref="IDialogsService.PickFile"/>.</summary>
    public sealed class PickFileRequest
    {
        /// <summary>Picker title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Allowed file types. Empty means any file.</summary>
        public List<FileFilter> Filters { get; set; } = new List<FileFilter>();

        /// <summary>Folder shown first.</summary>
        public string? InitialDirectory { get; set; }
    }

    /// <summary>Parameters of <see cref="IDialogsService.PickFolder"/>.</summary>
    public sealed class PickFolderRequest
    {
        /// <summary>Picker title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Folder shown first.</summary>
        public string? InitialDirectory { get; set; }
    }
}
