using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Library;
using Nucleus.Gaming.Coop.Generic;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Handlers;

/// <summary>
/// The hub status the UI follows (Nucleus 2.5: offline and online by itself): the first check always announced, "Retry
/// now" (a forced <c>handlers.checkHub</c>), UI reports without probe storms, and the update flags hidden offline.
/// <c>Hub.Connected</c> and <c>Hub.Checked</c> are process-wide: these tests run alone.
/// </summary>
[Collection(HubStatusCollection.Name)]
public class HubStatusTests : IDisposable
{
    public HubStatusTests()
    {
        Hub.Connected = false;
        Hub.Checked = false;
    }

    public void Dispose()
    {
        Hub.Connected = false;
        Hub.Checked = false;
    }

    private sealed class FakeEvents : IBridgeEvents
    {
        public List<object> Emitted { get; } = new List<object>();

        public void Emit<TEvent>(TEvent payload) where TEvent : class
        {
            lock (Emitted)
            {
                Emitted.Add(payload);
            }
        }

        public bool[] HubChanges()
        {
            lock (Emitted)
            {
                return Emitted.OfType<HubStatusChanged>().Select(e => e.Online).ToArray();
            }
        }
    }

    /// <summary>Answers <c>/health</c> with <see cref="Status"/>, after <see cref="Gate"/> opens; counts the requests.</summary>
    private sealed class HealthServer : HttpMessageHandler
    {
        private int requests;

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public TaskCompletionSource<bool> Gate { get; set; } = OpenGate();

        public int Requests => Volatile.Read(ref requests);

        public static TaskCompletionSource<bool> OpenGate()
        {
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            gate.SetResult(true);
            return gate;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref requests);
            await Gate.Task.ConfigureAwait(false);
            return new HttpResponseMessage(Status) { Content = new StringContent("{}") };
        }
    }

    private static (HubStatus status, HealthServer server, FakeEvents events) Create()
    {
        var server = new HealthServer();
        var events = new FakeEvents();
        return (new HubStatus(new HubClient("http://localhost:3000", server), events), server, events);
    }

    [Fact]
    public async Task The_first_check_is_announced_even_when_the_hub_is_offline()
    {
        var (status, server, events) = Create();
        server.Status = HttpStatusCode.ServiceUnavailable;
        var changes = new List<bool>();
        status.Changed += changes.Add;

        Assert.False(Hub.Checked);
        Assert.False(await status.CheckNowAsync(force: true));

        Assert.True(Hub.Checked, "the UI can tell offline from not checked yet (AppInfo.HubChecked)");
        Assert.Equal(new[] { false }, events.HubChanges());
        Assert.Equal(new[] { false }, changes);

        Assert.False(await status.CheckNowAsync(force: true));
        Assert.Equal(new[] { false }, events.HubChanges());//no change, no event
    }

    [Fact]
    public async Task Retry_now_probes_at_once_and_announces_the_hub_coming_back()
    {
        var (status, server, events) = Create();
        server.Status = HttpStatusCode.BadGateway;
        await status.CheckNowAsync(force: true);

        server.Status = HttpStatusCode.OK;
        Assert.True(await status.CheckNowAsync(force: true));

        Assert.Equal(2, server.Requests);
        Assert.Equal(new[] { false, true }, events.HubChanges());
        Assert.True(status.IsOnline);
    }

    [Fact]
    public async Task Reports_of_the_UI_do_not_probe_more_than_every_30_seconds()
    {
        var (status, server, _) = Create();
        server.Status = HttpStatusCode.ServiceUnavailable;
        await status.CheckNowAsync(force: true);

        //The catalogue of the WebView loads while the host says offline: the last check is recent, nothing is sent.
        server.Status = HttpStatusCode.OK;
        Assert.False(await status.CheckNowAsync(force: false));
        Assert.False(await status.CheckNowAsync(force: false));
        Assert.Equal(1, server.Requests);

        DateTime now = DateTime.UtcNow;
        Assert.False(HubStatus.RequestedCheckDue(now.AddSeconds(-29), now));
        Assert.True(HubStatus.RequestedCheckDue(now.AddSeconds(-30), now));
        Assert.True(HubStatus.RequestedCheckDue(DateTime.MinValue, now));
    }

    [Fact]
    public async Task Reports_of_the_UI_wait_while_a_session_runs()
    {
        var (status, server, _) = Create();
        bool session = true;
        status.StartMonitoring(() => session);

        Assert.False(await status.CheckNowAsync(force: false));
        Assert.Equal(0, server.Requests);

        Assert.True(await status.CheckNowAsync(force: true), "the user's own Retry still checks");
        Assert.Equal(1, server.Requests);
        status.Dispose();
    }

    [Fact]
    public async Task Checks_asked_meanwhile_share_the_one_running()
    {
        var (status, server, _) = Create();
        server.Gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<bool> first = status.CheckNowAsync(force: true);
        Task<bool> second = status.CheckNowAsync(force: true);
        Task<bool> report = status.CheckNowAsync(force: false);//never checked: due
        for (int i = 0; i < 100 && server.Requests == 0; i++)
        {
            await Task.Delay(10);
        }

        server.Gate.SetResult(true);
        bool[] results = await Task.WhenAll(first, second, report);

        Assert.Equal(new[] { true, true, true }, results);
        Assert.Equal(1, server.Requests);
    }

    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(true, true, false, false)]//offline: the flag of the last check is hidden (it can't be acted on)
    [InlineData(true, false, true, false)]//update checks off for the game
    [InlineData(false, true, true, false)]
    public void Update_flags_show_only_while_the_hub_is_online(bool updateAvailable, bool checkUpdate, bool online, bool shown)
    {
        Assert.Equal(shown, GameDtoFactory.ShowsUpdate(updateAvailable, checkUpdate, online));
    }

    [Fact]
    public void Hub_status_changes_refetch_the_library()
    {
        BridgeEventAttribute attribute = (BridgeEventAttribute)Attribute.GetCustomAttribute(typeof(HubStatusChanged), typeof(BridgeEventAttribute))!;

        Assert.Contains("app.getInfo", attribute.Invalidates);
        Assert.Contains("library.list", attribute.Invalidates);
        Assert.Contains("library.get", attribute.Invalidates);
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class HubStatusCollection
{
    public const string Name = "Hub status (process-wide Hub.Connected)";
}
