using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.Generic;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Session;

/// <summary>LaunchWait is static (one launch at a time): its tests share one collection and run one by one.</summary>
[Collection("LaunchWait")]
public class LaunchWaitTests
{
    // Thread.Sleep and the poll granularity: allow for timer resolution and a busy test machine.
    private const int Tolerance = 25;
    private const int Slack = 700;

    private static long Measure(Action action)
    {
        Stopwatch watch = Stopwatch.StartNew();
        action();
        return watch.ElapsedMilliseconds;
    }

    private static List<string> BeginCapture(bool fast, int level, Func<bool>? abort = null, string? timingFile = null)
    {
        var lines = new List<string>();
        LaunchWait.Begin(fast, level, "test", line => { lock (lines) { lines.Add(line); } }, timingFile, abort);
        return lines;
    }

    [Fact]
    public void Fast_returns_as_soon_as_the_condition_holds()
    {
        BeginCapture(fast: true, level: 2);

        long elapsed = Measure(() => LaunchWait.Sleep(3000, "p1.ready", LaunchWait.Tier1, () => true));

        Assert.True(elapsed < Slack, $"returned after {elapsed} ms");
        Assert.Equal("ready", LaunchWait.RecordedWaits().Last().Item4);
    }

    [Fact]
    public void Fast_returns_when_a_condition_becomes_true()
    {
        BeginCapture(fast: true, level: 2);
        Stopwatch since = Stopwatch.StartNew();

        long elapsed = Measure(() => LaunchWait.Sleep(3000, "p1.later", LaunchWait.Tier2, () => since.ElapsedMilliseconds >= 300, pollMs: 20));

        Assert.True(elapsed >= 300 - Tolerance, $"returned after {elapsed} ms");
        Assert.True(elapsed < 300 + Slack, $"returned after {elapsed} ms");
    }

    [Fact]
    public void Fast_never_waits_longer_than_the_maximum()
    {
        BeginCapture(fast: true, level: 2);

        long elapsed = Measure(() => LaunchWait.Sleep(400, "p1.never", LaunchWait.Tier2, () => false, pollMs: 50));

        Assert.True(elapsed >= 400 - Tolerance, $"returned after {elapsed} ms");
        Assert.True(elapsed < 400 + Slack, $"returned after {elapsed} ms");
        Assert.Equal("timeout", LaunchWait.RecordedWaits().Last().Item4);
    }

    [Fact]
    public void Fast_keeps_the_minimum_and_the_settle_time()
    {
        BeginCapture(fast: true, level: 2);

        long withMin = Measure(() => LaunchWait.Sleep(3000, "p1.min", LaunchWait.Tier2, () => true, minMs: 300));
        long withSettle = Measure(() => LaunchWait.Sleep(3000, "p1.settle", LaunchWait.Tier2, () => true, settleMs: 300));
        long cappedSettle = Measure(() => LaunchWait.Sleep(250, "p1.cap", LaunchWait.Tier2, () => true, settleMs: 5000));

        Assert.InRange(withMin, 300 - Tolerance, 300 + Slack);
        Assert.InRange(withSettle, 300 - Tolerance, 300 + Slack);
        Assert.InRange(cappedSettle, 250 - Tolerance, 250 + Slack);
        Assert.Equal("ready+cap", LaunchWait.RecordedWaits().Last().Item4);
    }

    [Fact]
    public void Legacy_waits_the_full_time_even_when_ready()
    {
        BeginCapture(fast: false, level: 2);

        long elapsed = Measure(() => LaunchWait.Sleep(400, "p1.legacy", LaunchWait.Tier1, () => true));

        Assert.True(elapsed >= 400 - Tolerance, $"returned after {elapsed} ms");
        Assert.Equal("legacy", LaunchWait.RecordedWaits().Last().Item4);
        Assert.False(LaunchWait.Fast);
        Assert.False(LaunchWait.FastAt(LaunchWait.Tier1));
    }

    [Fact]
    public void A_tier_above_the_level_waits_the_full_time()
    {
        BeginCapture(fast: true, level: 1);

        long tier2 = Measure(() => LaunchWait.Sleep(400, "p1.tier2", LaunchWait.Tier2, () => true));
        long tier1 = Measure(() => LaunchWait.Sleep(3000, "p1.tier1", LaunchWait.Tier1, () => true));

        Assert.True(tier2 >= 400 - Tolerance, $"tier 2 returned after {tier2} ms");
        Assert.True(tier1 < Slack, $"tier 1 returned after {tier1} ms");
        Assert.True(LaunchWait.FastAt(LaunchWait.Tier1));
        Assert.False(LaunchWait.FastAt(LaunchWait.Tier2));
    }

    [Fact]
    public void Fixed_waits_and_never_tier_are_not_shortened()
    {
        BeginCapture(fast: true, level: 2);

        long fixedWait = Measure(() => LaunchWait.Sleep(300, "p1.fixed"));
        long never = Measure(() => LaunchWait.Sleep(300, "p1.never", LaunchWait.Never, () => true));

        Assert.True(fixedWait >= 300 - Tolerance, $"fixed returned after {fixedWait} ms");
        Assert.True(never >= 300 - Tolerance, $"never returned after {never} ms");
    }

    [Fact]
    public void A_throwing_condition_counts_as_not_ready()
    {
        BeginCapture(fast: true, level: 2);

        long elapsed = Measure(() => LaunchWait.Sleep(300, "p1.throws", LaunchWait.Tier1, () => throw new InvalidOperationException("gone")));

        Assert.True(elapsed >= 300 - Tolerance, $"returned after {elapsed} ms");
    }

    [Fact]
    public void Fast_stops_when_the_launch_is_aborted()
    {
        BeginCapture(fast: true, level: 2, abort: () => true);

        long elapsed = Measure(() => LaunchWait.Sleep(3000, "p1.abort", LaunchWait.Tier2, () => false));

        Assert.True(elapsed < Slack, $"returned after {elapsed} ms");
        Assert.Equal("abort", LaunchWait.RecordedWaits().Last().Item4);
    }

    [Fact]
    public void Waits_and_stages_are_logged_and_written_once_to_the_timing_file()
    {
        string dir = Path.Combine(Path.GetTempPath(), "nucleus-launchwait-" + Guid.NewGuid().ToString("N"));
        string file = Path.Combine(dir, LaunchWait.TimingFileName);
        try
        {
            List<string> lines = BeginCapture(fast: true, level: 2, timingFile: file);

            LaunchWait.Mark("p1.setup");
            LaunchWait.Sleep(2000, "p1.PauseBetweenStarts", LaunchWait.Tier2, () => true);
            LaunchWait.Mark("p1.ready");
            LaunchWait.Mark("p2.setup");
            LaunchWait.Sleep(2000, "p2.PauseBetweenStarts", LaunchWait.Tier2, () => true);
            LaunchWait.Mark("p2.ready");
            LaunchWait.Running(layoutPending: false);
            LaunchWait.Finish("session ended");// already written: no second block

            var wait = LaunchWait.RecordedWaits().First();
            Assert.Equal("p1.PauseBetweenStarts", wait.Item1);
            Assert.Equal(2000, wait.Item2);
            Assert.True(wait.Item3 < 2000);

            string text = File.ReadAllText(file);
            Assert.Equal(1, CountOf(text, "===== "));
            Assert.Contains("fast launch level 2", text);
            Assert.Contains("wait  p1.PauseBetweenStarts planned=2000ms", text);
            Assert.Contains("stage p2.ready", text);
            Assert.Contains("Play to session running:", text);
            Assert.Contains("P1:", text);
            Assert.Contains("P2:", text);
            Assert.Contains("p*.PauseBetweenStarts", text);

            lock (lines)
            {
                Assert.All(lines, l => Assert.StartsWith("TIMING ", l));
                Assert.Contains(lines, l => l.Contains("stage session.running"));
                Assert.Contains(lines, l => l.Contains("----- summary -----"));
            }
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    [Fact]
    public void A_pending_layout_delays_the_block_until_the_windows_are_placed()
    {
        string dir = Path.Combine(Path.GetTempPath(), "nucleus-launchwait-" + Guid.NewGuid().ToString("N"));
        string file = Path.Combine(dir, LaunchWait.TimingFileName);
        try
        {
            BeginCapture(fast: false, level: 2, timingFile: file);

            LaunchWait.Running(layoutPending: true);
            Assert.False(File.Exists(file));

            LaunchWait.LayoutFinal();
            string text = File.ReadAllText(file);
            Assert.Contains("legacy (fast launch off)", text);
            Assert.Contains("Play to all windows placed:", text);
            Assert.DoesNotContain("not reached when", text);
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData("", 2)]
    [InlineData("1", 1)]
    [InlineData("2", 2)]
    [InlineData("7", 2)]
    [InlineData("0", 2)]
    [InlineData("fast", 2)]
    public void Level_parsing_is_tolerant(string? text, int expected)
    {
        Assert.Equal(expected, LaunchWait.ParseLevel(text));
    }

    [Fact]
    public void Ini_reading_is_off_unless_the_key_says_true()
    {
        string dir = Path.Combine(Path.GetTempPath(), "nucleus-launchwait-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "Settings.ini");
        try
        {
            File.WriteAllText(path, "[Misc]\r\nDebugLog=True\r\n");
            var ini = new IniFile(path);
            Assert.False(LaunchWait.IniFastAt(ini, LaunchWait.Tier1));
            Assert.False(LaunchWait.IniFastAt(null, LaunchWait.Tier1));

            File.WriteAllText(path, "[Misc]\r\nFastLaunch=True\r\n");
            Assert.True(LaunchWait.IniFastAt(ini, LaunchWait.Tier1));
            Assert.True(LaunchWait.IniFastAt(ini, LaunchWait.Tier2));

            File.WriteAllText(path, "[Misc]\r\nFastLaunch=True\r\nFastLaunchLevel=1\r\n");
            Assert.True(LaunchWait.IniFastAt(ini, LaunchWait.Tier1));
            Assert.False(LaunchWait.IniFastAt(ini, LaunchWait.Tier2));

            File.WriteAllText(path, "[Misc]\r\nFastLaunch=yes\r\n");
            Assert.False(LaunchWait.IniFastAt(ini, LaunchWait.Tier1));
        }
        finally
        {
            try { Directory.Delete(dir, true); } catch { }
        }
    }

    private static int CountOf(string text, string part)
    {
        int count = 0;
        for (int at = text.IndexOf(part, StringComparison.Ordinal); at >= 0; at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
