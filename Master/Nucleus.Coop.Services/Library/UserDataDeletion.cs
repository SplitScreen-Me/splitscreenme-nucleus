using System;
using System.IO;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;

namespace Nucleus.Coop.Services.Library;

/// <summary>How a file or folder is removed.</summary>
internal enum DeleteMode
{
    /// <summary>To the recycle bin, so the user can restore it.</summary>
    RecycleBin,

    /// <summary>For good.</summary>
    Permanent,
}

/// <summary>
/// Nucleus 2.5 sends what the user deletes (saves, configs, backups, the instances folder, game profiles, replaced
/// handler scripts) to the recycle bin instead of deleting it for good. Cleanups Nucleus runs by itself (content folder
/// when a setup opens or a session ends, download work folders) still delete for good.
/// </summary>
internal static class UserDataDeletion
{
    /// <summary>Mode of a game menu deletion: every user-facing delete item recycles.</summary>
    public static DeleteMode ForMenuItem(string itemId)
    {
        switch (itemId)
        {
            case GameMenuBuilder.Ids.DeleteContentFolder:
            case GameMenuBuilder.Ids.DeleteUserConfig:
            case GameMenuBuilder.Ids.DeleteUserSave:
            case GameMenuBuilder.Ids.DeleteDocumentsConfig:
            case GameMenuBuilder.Ids.DeleteDocumentsSave:
            case GameMenuBuilder.Ids.DeleteBackup:
                return DeleteMode.RecycleBin;
            default:
                return DeleteMode.Permanent;
        }
    }

    /// <summary>
    /// What actually happens: a recycle bin deletion on a drive without a recycle bin (network share, some removable
    /// drives) deletes for good, as the user already confirmed the deletion.
    /// </summary>
    public static DeleteMode Effective(DeleteMode wanted, bool driveHasRecycleBin) =>
        wanted == DeleteMode.RecycleBin && driveHasRecycleBin ? DeleteMode.RecycleBin : DeleteMode.Permanent;

    /// <summary>Removes a folder or a file (worker thread). Throws IOException / UnauthorizedAccessException when it stays.</summary>
    public static void Delete(string path, DeleteMode mode)
    {
        bool isFolder = Directory.Exists(path);

        if (!isFolder && !File.Exists(path))
        {
            return;
        }

        if (Effective(mode, FileUtil.HasRecycleBin(path)) == DeleteMode.RecycleBin)
        {
            if (FileUtil.SendToRecycleBin(path))
            {
                ServiceLog.Info($"moved to the recycle bin: {path}");
                return;
            }

            if (Directory.Exists(path) || File.Exists(path))
            {
                throw new IOException($"{path} could not be moved to the recycle bin (files in use?).");
            }

            return;
        }

        if (isFolder)
        {
            Directory.Delete(path, true);
        }
        else
        {
            File.Delete(path);
        }

        ServiceLog.Info($"deleted: {path}");
    }

    /// <summary><see cref="Delete"/> on a worker.</summary>
    public static Task DeleteAsync(string path, DeleteMode mode) => Task.Run(() => Delete(path, mode));
}
