using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Saves <c>content\userprofile.json</c> (the library). Replaces <c>GameManager.SaveUserProfile</c> for the services:
/// that one silently drops a save requested while another is running, and <c>RemoveGame</c> bypassed it. Here the
/// snapshot is taken on the UI thread and written on a worker, always the latest one, atomically.
/// </summary>
public sealed class UserProfileStore
{
    private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
    private string? pendingJson;
    private string? pendingPath;

    /// <summary>Sorts the library by game name (as the legacy save) and saves it. Call on the UI thread.</summary>
    public Task SaveAsync()
    {
        GameManager manager = GameManager.Instance;
        lock (manager.User.Games)
        {
            manager.User.Games = manager.User.Games.OrderBy(g => g?.Game?.GameName).ToList();
        }

        string json = JsonConvert.SerializeObject(manager.User, Formatting.Indented);
        Interlocked.Exchange(ref pendingPath, manager.GetUserProfilePath());
        Interlocked.Exchange(ref pendingJson, json);
        return Task.Run(WritePendingAsync);
    }

    private async Task WritePendingAsync()
    {
        await writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            string? json = Interlocked.Exchange(ref pendingJson, null);
            string? path = Volatile.Read(ref pendingPath);
            if (json == null || path == null)
            {
                return; // a later call already wrote the newest snapshot
            }

            WriteAtomically(path, json);
        }
        catch (Exception e)
        {
            ServiceLog.Error("userprofile.json could not be saved", e);
        }
        finally
        {
            writeLock.Release();
        }
    }

    internal static void WriteAtomically(string path, string content)
    {
        string temp = path + ".tmp";
        File.WriteAllText(temp, content);
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Replace(temp, path, null);
                }
                else
                {
                    File.Move(temp, path);
                }

                return;
            }
            catch (IOException) when (attempt < 5)
            {
                Thread.Sleep(100); // the legacy saver may hold the file for a moment
            }
        }
    }
}
