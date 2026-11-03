using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Supporters;
using Nucleus.Coop.Services.Tests.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Supporters;

public class SupporterNamesTests
{
    [Fact]
    public void Reads_the_published_format_in_order()
    {
        string text = "<div>Mara Quill</div>\r\n<div>Lukáš Novák</div>\r\n<div>Алексей</div>\r\n";

        Assert.Equal(new[] { "Mara Quill", "Lukáš Novák", "Алексей" }, SupporterNames.Parse(text));
    }

    [Fact]
    public void Strips_tags_decodes_entities_and_collapses_spaces()
    {
        string text = "<div><b>Bob</b>   &amp; Alice</div>\n<div>  Tom&#39;s &lt;3 </div>\n<div>Zoë&nbsp;Q</div>";

        Assert.Equal(new[] { "Bob & Alice", "Tom's <3", "Zoë Q" }, SupporterNames.Parse(text));
    }

    [Fact]
    public void Entities_that_look_like_tags_stay_text()
    {
        Assert.Equal(new[] { "<script>alert(1)</script>" }, SupporterNames.Parse("<div>&lt;script&gt;alert(1)&lt;/script&gt;</div>"));
        Assert.Equal(new[] { "<3 Bob" }, SupporterNames.Parse("<div><3 Bob</div>"));
    }

    [Fact]
    public void Splits_names_written_on_one_line()
    {
        Assert.Equal(new[] { "One", "Two", "Three" }, SupporterNames.Parse("<div>One</div><div>Two</div><br>Three"));
    }

    [Fact]
    public void Skips_blank_lines_and_names_whose_characters_were_lost()
    {
        string text = "<div>? ?</div>\r\n\r\n<div>   </div>\n<div></div>\r<div>-</div>\n<div>Kim</div>\n";

        Assert.Equal(new[] { "Kim" }, SupporterNames.Parse(text));
    }

    [Fact]
    public void Keeps_symbols_and_emoji_names()
    {
        Assert.Equal(new[] { "♥", "🎮 Pad" }, SupporterNames.Parse("<div>♥</div>\n<div>🎮 Pad</div>"));
    }

    [Fact]
    public void Removes_invisible_and_reordering_characters()
    {
        string text = "<div>\u202EevilName\u202C</div>\n<div>Zero\u200BWidth</div>\n<div>\uFEFFBom</div>\n<div>Tab\tName</div>";

        Assert.Equal(new[] { "evilName", "ZeroWidth", "Bom", "Tab Name" }, SupporterNames.Parse(text));
    }

    [Fact]
    public void Keeps_duplicates_since_people_share_names()
    {
        Assert.Equal(new[] { "Alex", "Alex" }, SupporterNames.Parse("<div>Alex</div>\n<div>Alex</div>"));
    }

    [Fact]
    public void Cuts_very_long_names()
    {
        string name = SupporterNames.Clean(new string('a', 300))!;

        Assert.Equal(SupporterNames.MaxNameLength, name.Length);
        Assert.EndsWith("…", name);
    }

    [Fact]
    public void Nothing_in_nothing_out()
    {
        Assert.Empty(SupporterNames.Parse(null));
        Assert.Empty(SupporterNames.Parse(""));
        Assert.Empty(SupporterNames.Parse("<html><body></body></html>"));
    }
}

[Collection(GlobalLanguageCollection.Name)]
public sealed class SupportersServiceTests : IDisposable
{
    private const string Url = "https://lists.test/tiers.txt";
    private const string Music = "https://assets.nucleus.example/audio/patreon_credits.mp3?v=1";

    private readonly string root = Path.Combine(Path.GetTempPath(), "nc-supporters-" + Guid.NewGuid().ToString("N"));

    private string CacheFile => Path.Combine(root, "content", "patreon-supporters.txt");

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }
    }

    private SupportersService Service(FakeHttp http, string? music = Music) =>
        new SupportersService(Url, CacheFile, http, () => music) { Timeout = TimeSpan.FromMilliseconds(300) };

    [Fact]
    public async Task Downloads_the_list_and_saves_a_copy()
    {
        var http = FakeHttp.Text("<div>Ann</div>\r\n<div>Ben</div>\r\n");
        using SupportersService service = Service(http);

        SupportersList list = await service.Get(CancellationToken.None);

        Assert.Equal(new[] { "Ann", "Ben" }, list.Names);
        Assert.False(list.FromCache);
        Assert.Null(list.SavedAt);
        Assert.Equal(Music, list.MusicUrl);
        Assert.Equal(new[] { Url }, http.Requests);
        Assert.Equal("Ann\r\nBen\r\n", File.ReadAllText(CacheFile));
        Assert.False(File.Exists(CacheFile + ".tmp"));
    }

    [Fact]
    public async Task A_new_download_replaces_the_copy()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CacheFile)!);
        File.WriteAllText(CacheFile, "Old\r\n");
        using SupportersService service = Service(FakeHttp.Text("<div>New</div>"));

        SupportersList list = await service.Get(CancellationToken.None);

        Assert.Equal(new[] { "New" }, list.Names);
        Assert.Equal("New\r\n", File.ReadAllText(CacheFile));
    }

    [Theory]
    [InlineData("offline")]
    [InlineData("status")]
    [InlineData("empty")]
    [InlineData("timeout")]
    [InlineData("tooLarge")]
    public async Task Falls_back_to_the_saved_copy(string failure)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CacheFile)!);
        File.WriteAllText(CacheFile, "Ann\r\nBen\r\n");
        var saved = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(CacheFile, saved);
        using SupportersService service = Service(FakeHttp.Failing(failure));

        SupportersList list = await service.Get(CancellationToken.None);

        Assert.Equal(new[] { "Ann", "Ben" }, list.Names);
        Assert.True(list.FromCache);
        Assert.Equal(saved, list.SavedAt);
        Assert.Equal(DateTimeKind.Utc, list.SavedAt!.Value.Kind);
        Assert.Equal("Ann\r\nBen\r\n", File.ReadAllText(CacheFile)); // a failed download never touches the copy
    }

    [Fact]
    public async Task Unavailable_without_a_download_or_a_copy()
    {
        using SupportersService service = Service(FakeHttp.Failing("offline"));

        BridgeException e = await Assert.ThrowsAsync<BridgeException>(() => service.Get(CancellationToken.None));

        Assert.Equal(BridgeErrorKind.Unavailable, e.Kind);
        Assert.Equal("The supporters list can't be downloaded right now. Check your internet connection and try again.", e.Message);
        Assert.False(File.Exists(CacheFile));
    }

    [Fact]
    public async Task An_empty_copy_counts_as_no_copy()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CacheFile)!);
        File.WriteAllText(CacheFile, "\r\n  \r\n");
        using SupportersService service = Service(FakeHttp.Failing("status"));

        BridgeException e = await Assert.ThrowsAsync<BridgeException>(() => service.Get(CancellationToken.None));

        Assert.Equal(BridgeErrorKind.Unavailable, e.Kind);
    }

    [Fact]
    public async Task Cancelled_by_the_caller_is_not_a_fallback()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CacheFile)!);
        File.WriteAllText(CacheFile, "Ann\r\n");
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        using SupportersService service = Service(FakeHttp.Failing("timeout"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.Get(cancel.Token));
    }

    [Fact]
    public async Task No_music_url_when_the_file_is_missing()
    {
        using SupportersService service = Service(FakeHttp.Text("<div>Ann</div>"), music: null);

        Assert.Null((await service.Get(CancellationToken.None)).MusicUrl);
    }

    [Fact]
    public void Default_paths_are_runtime_data_and_a_shipped_asset()
    {
        Assert.EndsWith(@"\content\patreon-supporters.txt", Startup.NucleusPaths.SupportersCache);
        Assert.EndsWith(@"\gui\audio\patreon_credits.mp3", Startup.NucleusPaths.CreditsMusic);
        Assert.StartsWith("https://raw.githubusercontent.com/SplitScreen-Me/splitscreenme-patreon/", SupportersService.ListUrl);
    }

    private sealed class FakeHttp : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> answer;

        private FakeHttp(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> answer)
        {
            this.answer = answer;
        }

        public List<string> Requests { get; } = new List<string>();

        public static FakeHttp Text(string body) =>
            new FakeHttp((request, token) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(WithBom(body)),
            }));

        public static FakeHttp Failing(string how) => new FakeHttp(async (request, token) =>
        {
            switch (how)
            {
                case "offline":
                    throw new HttpRequestException("No such host is known.");
                case "status":
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("<div>Down</div>") };
                case "empty":
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html><body></body></html>") };
                case "tooLarge":
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Encoding.ASCII.GetBytes(new string('a', SupportersService.MaxDownloadBytes + 1))) };
                default:
                    await Task.Delay(Timeout.Infinite, token);
                    throw new InvalidOperationException("unreachable");
            }
        });

        // The raw GitHub file has no byte order mark, a hand-saved one may: both read the same.
        private static byte[] WithBom(string body)
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            byte[] text = Encoding.UTF8.GetBytes(body);
            var all = new byte[bom.Length + text.Length];
            Buffer.BlockCopy(bom, 0, all, 0, bom.Length);
            Buffer.BlockCopy(text, 0, all, bom.Length, text.Length);
            return all;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (Requests)
            {
                Requests.Add(request.RequestUri!.AbsoluteUri);
            }

            return answer(request, cancellationToken);
        }
    }
}

