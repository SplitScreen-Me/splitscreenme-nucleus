using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nucleus.Gaming.Coop.Generic
{
    /// <summary>
    /// The launch path's waits and its timing log (see v3-notes\refactor-study\launch-speed-investigation.md).
    /// <para>Fast launch off (legacy): <see cref="Sleep(int, string, int, Func{bool}, int, int, int)"/> is exactly
    /// <c>Thread.Sleep(ms)</c>; only the timing is added. Fast launch on: a wait of a tier the level allows returns as soon
    /// as its readiness condition holds (after its minimum and settle times), and never later than the legacy duration.</para>
    /// <para>Timing goes to the handler log (debug-log.txt and the session log) as it happens, and one block per launch is
    /// appended to <c>content\launch-timing.log</c> when the session is running (or ends). Thread-safe.</para>
    /// </summary>
    public static class LaunchWait
    {
        /// <summary>Nucleus's own redundant or pollable sleeps.</summary>
        public const int Tier1 = 1;
        /// <summary>Handler pauses and window setup cadence, ended early on a readiness condition.</summary>
        public const int Tier2 = 2;
        /// <summary>A wait fast launch never shortens (timing only).</summary>
        public const int Never = int.MaxValue;
        public const int MaxLevel = 2;

        /// <summary>Fast launch: shortest PauseBetweenStarts, whatever the readiness.</summary>
        public const int StartsFloorMs = 5000;
        /// <summary>Fast launch: time an instance must have been ready before the next one starts.</summary>
        public const int StartsSettleMs = 3000;
        /// <summary>Fast launch: interval between two window setup steps (legacy: the Windows Setup Timing, 10 s).</summary>
        public const int WindowStepMs = 1000;

        public const string TimingFileName = "launch-timing.log";
        private const long TimingFileMaxBytes = 1024 * 1024;
        private const int UnfinishedLayoutFlushMs = 120000;

        private static readonly object Sync = new object();
        private static readonly Stopwatch Clock = new Stopwatch();
        private static readonly List<string> Lines = new List<string>();
        private static readonly List<WaitRecord> Waits = new List<WaitRecord>();
        private static readonly List<KeyValuePair<string, double>> Stages = new List<KeyValuePair<string, double>>();

        private static bool fast;
        private static int level;
        private static string title = "";
        private static DateTime startedAt;
        private static Action<string> logSink;
        private static string timingFile;
        private static Func<bool> abort;
        private static bool written = true;
        private static double runningAt = -1;
        private static double layoutFinalAt = -1;
        private static Timer flushTimer;

        private struct WaitRecord
        {
            public string Label;
            public int PlannedMs;
            public int ActualMs;
            public string Exit;
        }

        /// <summary>Fast launch is on for this session.</summary>
        public static bool Fast { get { lock (Sync) { return fast; } } }

        /// <summary>Fast launch level of this session (1 = tier 1 only, 2 = tiers 1 and 2).</summary>
        public static int Level { get { lock (Sync) { return level; } } }

        /// <summary>Fast launch is on and its level includes <paramref name="tier"/>.</summary>
        public static bool FastAt(int tier)
        {
            lock (Sync)
            {
                return fast && level >= tier;
            }
        }

        /// <summary>Seconds since <see cref="Begin"/>.</summary>
        public static double Elapsed => Clock.Elapsed.TotalSeconds;

        /// <summary>
        /// Starts a launch: snapshot of the mode, clock, empty records. <paramref name="sink"/> receives each timing line
        /// (null: none), <paramref name="timingFilePath"/> receives the block (null: no file), <paramref name="abortCheck"/>
        /// lets a fast wait stop when the launch is cancelled (legacy waits ignore it, as before).
        /// </summary>
        public static void Begin(bool fastLaunch, int fastLevel, string launchTitle, Action<string> sink, string timingFilePath, Func<bool> abortCheck)
        {
            lock (Sync)
            {
                flushTimer?.Dispose();
                flushTimer = null;
                fast = fastLaunch;
                level = fastLaunch ? Math.Max(1, Math.Min(MaxLevel, fastLevel)) : 0;
                title = launchTitle ?? "";
                logSink = sink;
                timingFile = timingFilePath;
                abort = abortCheck;
                Lines.Clear();
                Waits.Clear();
                Stages.Clear();
                written = false;
                runningAt = -1;
                layoutFinalAt = -1;
                startedAt = DateTime.Now;
                Clock.Restart();
            }

            Emit($"launch start: {ModeText()}");
            Mark("session.start");
        }

        /// <summary>A fixed wait: <c>Thread.Sleep(ms)</c> in both modes, timed.</summary>
        public static void Sleep(int ms, string label)
        {
            Sleep(ms, label, Never, null);
        }

        /// <summary>
        /// Waits <paramref name="ms"/> like <c>Thread.Sleep</c>, or, when fast launch includes <paramref name="tier"/>,
        /// until <paramref name="ready"/> holds: it returns once ready has been seen, <paramref name="settleMs"/> have passed
        /// since it was first seen and at least <paramref name="minMs"/> have passed in total; never after <paramref name="ms"/>.
        /// <paramref name="ready"/> is checked every <paramref name="pollMs"/>; an exception counts as not ready.
        /// </summary>
        public static void Sleep(int ms, string label, int tier, Func<bool> ready, int minMs = 0, int settleMs = 0, int pollMs = 100)
        {
            if (ms < 0)
            {
                ms = 0;
            }

            Stopwatch watch = Stopwatch.StartNew();
            string exit;

            if (!FastAt(tier) || ready == null)
            {
                Thread.Sleep(ms);
                exit = FastAt(1) ? "kept" : "legacy";
            }
            else
            {
                exit = PollUntilReady(ms, ready, minMs, settleMs, Math.Max(1, pollMs), watch);
            }

            Record(label, ms, (int)watch.ElapsedMilliseconds, exit);
        }

        private static string PollUntilReady(int ms, Func<bool> ready, int minMs, int settleMs, int pollMs, Stopwatch watch)
        {
            long readyAt = -1;

            for (; ; )
            {
                long elapsed = watch.ElapsedMilliseconds;

                if (elapsed >= ms)
                {
                    return readyAt >= 0 ? "ready+cap" : "timeout";
                }

                if (IsAborted())
                {
                    return "abort";
                }

                if (readyAt < 0 && Check(ready))
                {
                    readyAt = watch.ElapsedMilliseconds;
                }

                elapsed = watch.ElapsedMilliseconds;

                long due = -1;
                if (readyAt >= 0)
                {
                    due = Math.Max(minMs, readyAt + settleMs);
                    if (elapsed >= due)
                    {
                        return "ready";
                    }
                }

                long remaining = ms - elapsed;
                long nap = readyAt >= 0 ? Math.Min(due - elapsed, remaining) : Math.Min(pollMs, remaining);
                if (nap > 0)
                {
                    Thread.Sleep((int)Math.Min(nap, int.MaxValue));
                }
            }
        }

        /// <summary>Polls <paramref name="condition"/> every <paramref name="pollMs"/> until it holds or <paramref name="maxMs"/> pass (both modes). Returns the condition.</summary>
        public static bool Until(Func<bool> condition, int maxMs, string label, int pollMs = 50)
        {
            Stopwatch watch = Stopwatch.StartNew();
            bool result;

            for (; ; )
            {
                result = Check(condition);
                if (result || watch.ElapsedMilliseconds >= maxMs)
                {
                    break;
                }

                Thread.Sleep((int)Math.Max(1, Math.Min(pollMs, maxMs - watch.ElapsedMilliseconds)));
            }

            Record(label, maxMs, (int)watch.ElapsedMilliseconds, result ? "ready" : "timeout");
            return result;
        }

        /// <summary>Records a stage (timestamp since <see cref="Begin"/>).</summary>
        public static void Mark(string stage)
        {
            double t;
            lock (Sync)
            {
                t = Clock.Elapsed.TotalSeconds;
                Stages.Add(new KeyValuePair<string, double>(stage, t));
            }

            Emit($"{Stamp(t)} stage {stage}");
        }

        /// <summary>The launch is done (Play returned): the session is running. Writes the block unless the window layout is still pending.</summary>
        public static void Running(bool layoutPending)
        {
            Mark("session.running");

            PendingBlock pending = null;
            lock (Sync)
            {
                runningAt = Clock.Elapsed.TotalSeconds;
                if (!layoutPending || layoutFinalAt >= 0)
                {
                    pending = TakeBlockLocked("session running");
                }
                else
                {
                    flushTimer?.Dispose();
                    flushTimer = new Timer(_ => Finish("window layout not final after " + (UnfinishedLayoutFlushMs / 1000) + " s"), null, UnfinishedLayoutFlushMs, Timeout.Infinite);
                }
            }

            pending?.Write();
        }

        /// <summary>All game windows reached their final layout (update tick).</summary>
        public static void LayoutFinal()
        {
            lock (Sync)
            {
                if (written || layoutFinalAt >= 0)
                {
                    return;
                }
            }

            Mark("layout.final");

            PendingBlock pending = null;
            lock (Sync)
            {
                layoutFinalAt = Clock.Elapsed.TotalSeconds;
                if (runningAt >= 0)
                {
                    pending = TakeBlockLocked("session running, windows placed");
                }
            }

            pending?.Write();
        }

        /// <summary>Writes the block now if it was not written yet (session ended, launch failed, timeout).</summary>
        public static void Finish(string reason)
        {
            PendingBlock pending;
            lock (Sync)
            {
                pending = TakeBlockLocked(reason);
            }

            pending?.Write();
        }

        /// <summary>The summary of the current launch (also the end of the file block).</summary>
        public static string Summary()
        {
            lock (Sync)
            {
                return BuildSummaryLocked();
            }
        }

        /// <summary>The recorded waits as (label, planned ms, actual ms, exit), for tests and diagnostics.</summary>
        public static List<Tuple<string, int, int, string>> RecordedWaits()
        {
            lock (Sync)
            {
                return Waits.Select(w => Tuple.Create(w.Label, w.PlannedMs, w.ActualMs, w.Exit)).ToList();
            }
        }

        /// <summary>Reads <c>[Misc] FastLaunch</c> / <c>FastLaunchLevel</c> (for StartGame.exe and Nucleus.Inject). A missing or bad key is off.</summary>
        public static bool IniFastAt(IniFile ini, int tier)
        {
            try
            {
                if (ini == null || !bool.TryParse(ini.IniReadValue("Misc", "FastLaunch"), out bool on) || !on)
                {
                    return false;
                }

                return ParseLevel(ini.IniReadValue("Misc", "FastLaunchLevel")) >= tier;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Same as <see cref="IniFastAt"/> with the install's Settings.ini (helper processes: StartGame.exe, Nucleus.Inject).</summary>
        public static bool InstallIniFastAt(int tier)
        {
            try
            {
                return IniFastAt(new IniFile(Path.Combine(InstallPaths.Root, "Settings.ini")), tier);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>The fast launch level from its ini text: 1 or 2, default (missing or invalid) 2.</summary>
        public static int ParseLevel(string text)
        {
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) && value >= 1)
            {
                return Math.Min(MaxLevel, value);
            }

            return MaxLevel;
        }

        private static bool Check(Func<bool> condition)
        {
            try
            {
                return condition();
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAborted()
        {
            Func<bool> check;
            lock (Sync)
            {
                check = abort;
            }

            return check != null && Check(check);
        }

        private static void Record(string label, int plannedMs, int actualMs, string exit)
        {
            double t;
            lock (Sync)
            {
                t = Clock.Elapsed.TotalSeconds;
                Waits.Add(new WaitRecord { Label = label, PlannedMs = plannedMs, ActualMs = actualMs, Exit = exit });
            }

            Emit(string.Format(CultureInfo.InvariantCulture, "{0} wait  {1} planned={2}ms actual={3}ms ({4})", Stamp(t), label, plannedMs, actualMs, exit));
        }

        private static void Emit(string text)
        {
            Action<string> sink;
            lock (Sync)
            {
                if (!written)
                {
                    Lines.Add(text);
                }

                sink = logSink;
            }

            try
            {
                sink?.Invoke("TIMING " + text);
            }
            catch
            {
            }
        }

        private static string Stamp(double seconds) => string.Format(CultureInfo.InvariantCulture, "+{0,8:0.000}s", seconds);

        private static string ModeText()
        {
            lock (Sync)
            {
                return fast ? $"mode=fast (level {level})" : "mode=legacy (fast launch off)";
            }
        }

        // Builds the block once (under the lock); the caller writes it outside the lock.
        private static PendingBlock TakeBlockLocked(string reason)
        {
            if (written)
            {
                return null;
            }

            written = true;
            flushTimer?.Dispose();
            flushTimer = null;

            string summary = BuildSummaryLocked();
            var block = new StringBuilder();
            block.AppendLine(string.Format(CultureInfo.InvariantCulture, "===== {0:yyyy-MM-dd HH:mm:ss.fff} | {1} | {2} | written: {3} =====",
                startedAt, title, fast ? $"fast launch level {level}" : "legacy (fast launch off)", reason));
            foreach (string line in Lines)
            {
                block.AppendLine(line);
            }

            block.Append(summary);
            block.AppendLine();

            return new PendingBlock { Summary = summary, Block = block.ToString(), Sink = logSink, FilePath = timingFile };
        }

        private sealed class PendingBlock
        {
            public string Summary;
            public string Block;
            public Action<string> Sink;
            public string FilePath;

            public void Write()
            {
                try
                {
                    foreach (string line in Summary.Split('\n'))
                    {
                        string text = line.TrimEnd('\r');
                        if (text.Length > 0)
                        {
                            Sink?.Invoke("TIMING " + text);
                        }
                    }
                }
                catch
                {
                }

                if (string.IsNullOrEmpty(FilePath))
                {
                    return;
                }

                try
                {
                    string dir = Path.GetDirectoryName(FilePath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var info = new FileInfo(FilePath);
                    if (info.Exists && info.Length > TimingFileMaxBytes)
                    {
                        File.Copy(FilePath, Path.ChangeExtension(FilePath, ".old.log"), true);
                        File.Delete(FilePath);
                    }

                    File.AppendAllText(FilePath, Block, new UTF8Encoding(false));
                }
                catch
                {
                    // Timing must never break a launch.
                }
            }
        }

        private static string BuildSummaryLocked()
        {
            var sb = new StringBuilder();
            CultureInfo ci = CultureInfo.InvariantCulture;
            sb.AppendLine("----- summary -----");
            sb.AppendLine(fast ? $"Mode: fast launch level {level}" : "Mode: legacy (fast launch off)");

            double running = StageTime("session.running");
            double layout = StageTime("layout.final");
            sb.AppendLine(running >= 0
                ? string.Format(ci, "Play to session running: {0:0.000} s", running)
                : string.Format(ci, "Play to session running: not reached ({0:0.000} s recorded)", Clock.Elapsed.TotalSeconds));
            sb.AppendLine(layout >= 0
                ? string.Format(ci, "Play to all windows placed: {0:0.000} s", layout)
                : "Play to all windows placed: not reached when this block was written");

            var setups = Stages.Where(s => IsInstanceStage(s.Key, ".setup")).ToList();
            if (setups.Count > 0)
            {
                sb.AppendLine("Per instance (setup -> ready = next instance may start):");
                for (int k = 0; k < setups.Count; k++)
                {
                    string player = setups[k].Key.Substring(0, setups[k].Key.IndexOf('.'));
                    double start = setups[k].Value;
                    double ready = StageTime(player + ".ready");
                    double launched = StageTime(player + ".launched");
                    double grabbed = StageTime(player + ".grabbed");
                    double placed = StageTime(player + ".finished");
                    double end = ready >= 0 ? ready : (k + 1 < setups.Count ? setups[k + 1].Value : -1);
                    sb.AppendLine(string.Format(ci, "  {0}: {1} -> {2}{3}{4}{5}{6}",
                        player.ToUpperInvariant(),
                        Fmt(start),
                        end >= 0 ? Fmt(end) : "?",
                        end >= 0 ? string.Format(ci, "  ({0:0.000} s)", end - start) : "",
                        launched >= 0 ? string.Format(ci, ", launched +{0:0.000} s", launched - start) : "",
                        grabbed >= 0 ? string.Format(ci, ", process +{0:0.000} s", grabbed - start) : "",
                        placed >= 0 ? string.Format(ci, ", window placed at {0}", Fmt(placed)) : ""));
                }
            }

            int planned = Waits.Sum(w => w.PlannedMs);
            int actual = Waits.Sum(w => w.ActualMs);
            sb.AppendLine(string.Format(ci, "Waits: {0} recorded, planned {1:0.0} s, actual {2:0.0} s", Waits.Count, planned / 1000.0, actual / 1000.0));

            var top = Waits
                .GroupBy(w => GenericLabel(w.Label))
                .Select(g => new { Label = g.Key, Count = g.Count(), Planned = g.Sum(w => w.PlannedMs), Actual = g.Sum(w => w.ActualMs) })
                .OrderByDescending(g => g.Actual)
                .ThenByDescending(g => g.Planned)
                .Take(10)
                .ToList();
            if (top.Count > 0)
            {
                sb.AppendLine("Top waits (actual):");
                foreach (var g in top)
                {
                    sb.AppendLine(string.Format(ci, "  {0,3} x {1,-36} planned {2,8:0.0} s  actual {3,8:0.0} s", g.Count, g.Label, g.Planned / 1000.0, g.Actual / 1000.0));
                }
            }

            return sb.ToString();
        }

        private static double StageTime(string stage)
        {
            foreach (var s in Stages)
            {
                if (s.Key == stage)
                {
                    return s.Value;
                }
            }

            return -1;
        }

        private static bool IsInstanceStage(string stage, string suffix)
        {
            return stage.Length > 1 && stage[0] == 'p' && char.IsDigit(stage[1]) && stage.EndsWith(suffix, StringComparison.Ordinal);
        }

        // "p3.PauseBetweenStarts" -> "p*.PauseBetweenStarts"
        private static string GenericLabel(string label)
        {
            int dot = label.IndexOf('.');
            if (dot > 1 && label[0] == 'p' && label.Substring(1, dot - 1).All(char.IsDigit))
            {
                return "p*" + label.Substring(dot);
            }

            return label;
        }

        private static string Fmt(double seconds) => string.Format(CultureInfo.InvariantCulture, "{0:0.000} s", seconds);
    }
}
