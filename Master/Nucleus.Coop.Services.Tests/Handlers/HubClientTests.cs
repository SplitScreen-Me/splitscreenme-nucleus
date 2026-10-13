using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Gaming;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Handlers;

public class HubSiteTests
{
    [Fact]
    public void Default_site_is_the_v3_site() => Assert.Equal("https://v3.splitscreen.me", HubSite.DefaultUrl);

    [Theory]
    [InlineData("https://v3.splitscreen.me", "https://v3.splitscreen.me")]
    [InlineData("  https://v3.splitscreen.me/  ", "https://v3.splitscreen.me")]
    [InlineData("http://localhost:3000", "http://localhost:3000")]
    [InlineData("http://localhost:3000/api/v3/", "http://localhost:3000")]
    [InlineData("HTTPS://Example.COM:443/site/?x=1#y", "https://example.com/site")]
    [InlineData("https://example.com/site/api/v3", "https://example.com/site")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    [InlineData("v3.splitscreen.me", null)]
    [InlineData("ftp://example.com", null)]
    [InlineData("file:///C:/site", null)]
    [InlineData("https://user:secret@example.com", null)]
    public void Setting_values(string? value, string? expected) => Assert.Equal(expected, HubSite.Normalize(value!));

    [Fact]
    public void Api_root() => Assert.Equal("http://localhost:3000/api/v3/", HubSite.ApiUrl("http://localhost:3000"));
}

public class HubClientUrlTests
{
    [Fact]
    public void Default_site_urls()
    {
        using var hub = new HubClient(HubSite.DefaultUrl);

        Assert.Equal("https://v3.splitscreen.me", hub.SiteUrl);
        Assert.Equal("https://v3.splitscreen.me/api/v3/", hub.ApiBase);
        Assert.Equal("v3.splitscreen.me", hub.DisplayName);
        Assert.Equal("https://v3.splitscreen.me/api/v3/health", hub.HealthUrl);
        Assert.Equal("https://v3.splitscreen.me/api/v3/handlers/fDmTaAoYKJmiWi3s3", hub.HandlerUrl("fDmTaAoYKJmiWi3s3"));
    }

    [Fact]
    public void Package_urls_count_downloads_unless_asked_not_to()
    {
        using var hub = new HubClient(HubSite.DefaultUrl);

        Assert.Equal("https://v3.splitscreen.me/api/v3/handlers/fDmTaAoYKJmiWi3s3/download?version=6", hub.PackageUrl("fDmTaAoYKJmiWi3s3", 6, countDownload: true));
        Assert.Equal("https://v3.splitscreen.me/api/v3/handlers/fDmTaAoYKJmiWi3s3/download?version=6&count=false", hub.PackageUrl("fDmTaAoYKJmiWi3s3", 6, countDownload: false));
    }

    [Fact]
    public void Custom_site_urls()
    {
        using var hub = new HubClient("http://localhost:3000/");

        Assert.Equal("http://localhost:3000", hub.SiteUrl);
        Assert.Equal("localhost:3000", hub.DisplayName);
        Assert.Equal("http://localhost:3000/api/v3/handlers/abc/download?version=12&count=false", hub.PackageUrl("abc", 12, countDownload: false));
        Assert.Throws<ArgumentException>(() => new HubClient("not a url"));
    }

    [Fact]
    public void Igdb_images()
    {
        Assert.Equal("https://images.igdb.com/igdb/image/upload/t_cover_big/co7n02.jpg", HubClient.CoverUrl("co7n02"));
        Assert.Equal("https://images.igdb.com/igdb/image/upload/t_original/scaewu.jpg", HubClient.IgdbImageUrl("scaewu", "t_original"));
    }

    [Theory]
    [InlineData("fcaL3FKqLTH62jLoM", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("../api", false)]
    [InlineData("abc?x=1", false)]
    public void Hub_ids(string? id, bool valid) => Assert.Equal(valid, HubClient.IsValidHubId(id));
}

public class HubClientParsingTests
{
    private static string Fixture(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Handlers", "Fixtures", name));

    [Fact]
    public void Handler_detail()
    {
        HubHandlerInfo info = HubClient.ParseHandler(Fixture("hub-handler.json"))!;

        Assert.Equal("fDmTaAoYKJmiWi3s3", info.Id);
        Assert.Equal("Palworld", info.Title);
        Assert.Equal("Palworld", info.GameName);
        Assert.Equal("co7n02", info.CoverImageId);
        Assert.Equal(6, info.Version);
        Assert.Equal("https://v3.splitscreen.me/hub/palworld", info.Url);
        Assert.Equal(new[] { "scaewu", "sctlbq" }, info.ScreenshotIds.ToArray());
        Assert.NotNull(info.Package);
        Assert.Equal(6, info.Package!.Version);
        Assert.Equal("18fdd9eb1491175929de57eaa06635eb4d70c175caf23b1e6ea4055c2d700a29", info.Package.Sha256);
        Assert.Equal(3567, info.Package.SizeBytes);
        Assert.Equal("handler-fDmTaAoYKJmiWi3s3-v6.nc", info.Package.FileName);
    }

    [Fact]
    public void Odd_answers()
    {
        Assert.Null(HubClient.ParseHandler("{}"));
        Assert.Null(HubClient.ParseHandler("[]"));
        Assert.Null(HubClient.ParseHandler(""));
        Assert.Null(HubClient.ParseHandler("{\"error\":{\"code\":\"not_found\",\"message\":\"Not found\"}}"));

        HubHandlerInfo bare = HubClient.ParseHandler("{\"id\":\"abc\",\"version\":\"7\",\"coverImageId\":null,\"screenshots\":[{\"imageId\":\"../x\"},{\"imageId\":\"ok_1\"},5]}")!;
        Assert.Equal(-1, bare.Version); // a string is not a version
        Assert.Equal("", bare.CoverImageId);
        Assert.Null(bare.Package);
        Assert.Equal(new[] { "ok_1" }, bare.ScreenshotIds.ToArray());

        HubHandlerInfo badSha = HubClient.ParseHandler("{\"id\":\"abc\",\"version\":2,\"currentVersion\":{\"version\":2,\"sha256\":\"not-a-hash\"}}")!;
        Assert.Equal("", badSha.Package!.Sha256);

        HubHandlerInfo upper = HubClient.ParseHandler("{\"id\":\"abc\",\"version\":2,\"currentVersion\":{\"version\":2,\"sha256\":\"18FDD9EB1491175929DE57EAA06635EB4D70C175CAF23B1E6EA4055C2D700A29\"}}")!;
        Assert.Equal("18fdd9eb1491175929de57eaa06635eb4d70c175caf23b1e6ea4055c2d700a29", upper.Package!.Sha256);
    }
}

public sealed class HubClientHttpTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "nc-hub-tests-" + Guid.NewGuid().ToString("N"));

    public HubClientHttpTests()
    {
        Directory.CreateDirectory(root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(root, true);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public async Task Handler_detail_404_is_null()
    {
        var fake = new FakeHttp(_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{\"error\":{\"code\":\"not_found\"}}") });
        using var hub = new HubClient("https://hub.test", fake);

        Assert.Null(await hub.GetHandlerAsync("abcDEF123", CancellationToken.None));
        Assert.Equal("https://hub.test/api/v3/handlers/abcDEF123", fake.Requests.Single());
        Assert.Null(await hub.GetHandlerAsync("../etc", CancellationToken.None)); // never sent
        Assert.Single(fake.Requests);
    }

    [Fact]
    public async Task Rate_limited_answers_say_when_to_retry()
    {
        var fake = new FakeHttp(_ =>
        {
            var response = new HttpResponseMessage((HttpStatusCode)429) { Content = new StringContent("{}") };
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(42));
            return response;
        });
        using var hub = new HubClient("https://hub.test", fake);

        var e = await Assert.ThrowsAsync<HubUnavailableException>(() => hub.GetHandlerAsync("abc", CancellationToken.None));
        Assert.Contains("try again in 42 s", e.Message);
    }

    [Fact]
    public async Task Ping_reads_the_health_endpoint()
    {
        var status = HttpStatusCode.OK;
        var fake = new FakeHttp(_ => new HttpResponseMessage(status) { Content = new StringContent("{\"status\":\"ok\"}") });
        using var hub = new HubClient("https://hub.test", fake);

        Assert.True(await hub.PingAsync(CancellationToken.None));
        status = HttpStatusCode.ServiceUnavailable;
        Assert.False(await hub.PingAsync(CancellationToken.None));
        Assert.All(fake.Requests, url => Assert.Equal("https://hub.test/api/v3/health", url));
    }

    [Fact]
    public async Task Download_keeps_a_package_that_matches_its_sha256()
    {
        byte[] package = Encoding.UTF8.GetBytes("PK fake package bytes");
        var fake = new FakeHttp(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(package) });
        using var hub = new HubClient("https://hub.test", fake);
        string target = Path.Combine(root, "ok.nc");
        var percents = new List<int?>();

        await hub.DownloadToFileAsync(hub.PackageUrl("abc", 3, countDownload: false), target, Sha256Hex(package).ToUpperInvariant(), new SyncProgress(percents.Add), CancellationToken.None);

        Assert.Equal(package, File.ReadAllBytes(target));
        Assert.Equal(100, percents.Last());
        Assert.Equal("https://hub.test/api/v3/handlers/abc/download?version=3&count=false", fake.Requests.Single());
    }

    [Fact]
    public async Task Download_refuses_a_package_that_does_not_match_its_sha256()
    {
        byte[] package = Encoding.UTF8.GetBytes("PK tampered package");
        var fake = new FakeHttp(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(package) });
        using var hub = new HubClient("https://hub.test", fake);
        string target = Path.Combine(root, "bad.nc");
        string expected = Sha256Hex(Encoding.UTF8.GetBytes("PK the published package"));

        var e = await Assert.ThrowsAsync<PackageChecksumException>(() => hub.DownloadToFileAsync("https://hub.test/x", target, expected, null, CancellationToken.None));

        Assert.Equal(expected, e.Expected);
        Assert.Equal(Sha256Hex(package), e.Actual);
        Assert.Contains("not installed", e.Message);
        Assert.False(File.Exists(target));
    }

    [Fact]
    public async Task Download_needs_a_sha256_and_reports_http_failures()
    {
        var fake = new FakeHttp(_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{}") });
        using var hub = new HubClient("https://hub.test", fake);
        string target = Path.Combine(root, "missing.nc");

        await Assert.ThrowsAsync<ArgumentException>(() => hub.DownloadToFileAsync("https://hub.test/x", target, "", null, CancellationToken.None));
        Assert.Empty(fake.Requests);

        var e = await Assert.ThrowsAsync<HubUnavailableException>(() => hub.DownloadToFileAsync("https://hub.test/x", target, new string('a', 64), null, CancellationToken.None));
        Assert.StartsWith("The download failed: the server answered 404", e.Message);
        Assert.False(File.Exists(target));
    }

    private static string Sha256Hex(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return HubClient.ToHex(sha.ComputeHash(bytes));
    }

    private sealed class FakeHttp : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> answer;

        public FakeHttp(Func<HttpRequestMessage, HttpResponseMessage> answer)
        {
            this.answer = answer;
        }

        public List<string> Requests { get; } = new List<string>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (Requests)
            {
                Requests.Add(request.RequestUri!.AbsoluteUri);
            }

            return Task.FromResult(answer(request));
        }
    }

    private sealed class SyncProgress : IProgress<int?>
    {
        private readonly Action<int?> report;

        public SyncProgress(Action<int?> report)
        {
            this.report = report;
        }

        public void Report(int? value) => report(value);
    }
}
