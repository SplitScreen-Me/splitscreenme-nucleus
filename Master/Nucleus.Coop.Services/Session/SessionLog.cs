using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming;

namespace Nucleus.Coop.Services.Session
{
    /// <summary>
    /// The launch log: every GenericGameHandler.Log line (through its LogHook) while a session runs, kept in a ring
    /// buffer of <see cref="Capacity"/> lines and pushed to the UI as <c>session.log</c> at most 5 times per second.
    /// Thread-safe: the handler logs from the launch, tick and hotkey threads.
    /// </summary>
    internal sealed class SessionLog
    {
        public const int Capacity = 500;

        private static readonly Regex PlayerPattern = new Regex(@"\b(?:player|instance)\s*#?\s*(\d{1,2})\b", RegexOptions.IgnoreCase);
        private static readonly Regex ErrorPattern = new Regex(@"\b(error|exception|failed|fail|crash(ed)?)\b", RegexOptions.IgnoreCase);
        private static readonly Regex WarningPattern = new Regex(@"\b(warning|could not|couldn't|cannot|can't|not found|missing|retry(ing)?|timed? ?out)\b", RegexOptions.IgnoreCase);
        private static readonly Regex SuccessPattern = new Regex(@"\b(success(fully)?|done|completed?|finished|ready|launched)\b", RegexOptions.IgnoreCase);

        private readonly object sync = new object();
        private readonly LinkedList<SessionLogEntry> buffer = new LinkedList<SessionLogEntry>();
        private readonly List<SessionLogEntry> pending = new List<SessionLogEntry>();
        private readonly IBridgeEvents events;
        private readonly Timer flushTimer;
        private long seq;
        private bool flushScheduled;

        public SessionLog(IBridgeEvents events)
        {
            this.events = events;
            flushTimer = new Timer(_ => Flush(), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>Starts capturing (Play).</summary>
        public void Start() => GenericGameHandler.LogHook = Append;

        /// <summary>Stops capturing (session end); the lines stay readable until the next Play.</summary>
        public void Stop()
        {
            if (GenericGameHandler.LogHook == (Action<string>)Append)
            {
                GenericGameHandler.LogHook = null;
            }

            Flush();
        }

        public void Clear()
        {
            lock (sync)
            {
                buffer.Clear();
                pending.Clear();
            }
        }

        public List<SessionLogEntry> Entries()
        {
            lock (sync)
            {
                return buffer.ToList();
            }
        }

        /// <summary>One log line (any thread).</summary>
        public void Append(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            SessionLogEntry entry = Parse(text.Trim());

            lock (sync)
            {
                entry.Seq = ++seq;
                buffer.AddLast(entry);

                while (buffer.Count > Capacity)
                {
                    buffer.RemoveFirst();
                }

                pending.Add(entry);

                if (!flushScheduled)
                {
                    flushScheduled = true;
                    flushTimer.Change(200, Timeout.Infinite);//at most 5 events per second
                }
            }
        }

        private void Flush()
        {
            List<SessionLogEntry> batch;

            lock (sync)
            {
                flushScheduled = false;

                if (pending.Count == 0)
                {
                    return;
                }

                batch = pending.ToList();
                pending.Clear();
            }

            events.Emit(new SessionLogAppended { Entries = batch });
        }

        /// <summary>Level and player of a line, guessed from its words (the handler log has no levels).</summary>
        internal static SessionLogEntry Parse(string text)
        {
            SessionLogLevel level = ErrorPattern.IsMatch(text) ? SessionLogLevel.Error
                                  : WarningPattern.IsMatch(text) ? SessionLogLevel.Warning
                                  : SuccessPattern.IsMatch(text) ? SessionLogLevel.Success
                                  : SessionLogLevel.Info;

            Match player = PlayerPattern.Match(text);

            return new SessionLogEntry
            {
                At = DateTime.UtcNow,
                Level = level,
                PlayerNumber = player.Success && int.TryParse(player.Groups[1].Value, out int number) && number > 0 ? number : (int?)null,
                Text = text,
            };
        }
    }
}
