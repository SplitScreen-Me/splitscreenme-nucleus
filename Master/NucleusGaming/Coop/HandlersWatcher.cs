using System;
using System.IO;
using System.Threading;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// Watches the handlers folder for <c>*.js</c> files created, changed, renamed or deleted (Nucleus 2.5 reloads handlers
    /// when their file is edited, added or removed). Events are coalesced: <see cref="Changed"/> is raised once, on a
    /// thread pool thread, after <see cref="QuietMs"/> without file events (editors write a file in several steps). It
    /// replaces the 2.5 polling timer (a scan of the folder every 3 s): nothing runs while no file changes.
    /// </summary>
    public sealed class HandlersWatcher : IDisposable
    {
        private readonly object sync = new object();
        private FileSystemWatcher watcher;
        private Timer debounce;
        private bool disposed;

        /// <summary>Delay without file events before <see cref="Changed"/> is raised.</summary>
        public int QuietMs { get; set; } = 600;

        /// <summary>Raised on a thread pool thread when handler files may have changed: call <see cref="GameManager.ScanHandlerFiles"/>.</summary>
        public event Action Changed;

        public HandlersWatcher(string folder)
        {
            Directory.CreateDirectory(folder);

            debounce = new Timer(_ => Raise(), null, Timeout.Infinite, Timeout.Infinite);

            watcher = new FileSystemWatcher(folder, "*.js")
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                InternalBufferSize = 64 * 1024,
            };

            watcher.Changed += OnFileEvent;
            watcher.Created += OnFileEvent;
            watcher.Deleted += OnFileEvent;
            watcher.Renamed += OnFileEvent;
            watcher.Error += (s, e) => Poke();//buffer overflow: some events were lost, scan anyway
            watcher.EnableRaisingEvents = true;
        }

        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            Poke();
        }

        /// <summary>Schedules <see cref="Changed"/> as if a file changed (after a session, to catch up).</summary>
        public void Poke()
        {
            lock (sync)
            {
                if (!disposed)
                {
                    debounce.Change(QuietMs, Timeout.Infinite);
                }
            }
        }

        private void Raise()
        {
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }
            }

            try
            {
                Changed?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.Log("Handlers watcher: " + e.Message);
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
            }

            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            debounce.Dispose();
        }
    }
}
