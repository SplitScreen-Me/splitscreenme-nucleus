using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;

namespace Nucleus.Coop.Services.Notices;

/// <summary>
/// The <c>notices</c> service: messages for the user that wait until dismissed (see <see cref="INoticesService"/>), and
/// the crash report of the last failed launch, which the session shows once it has ended. Thread-safe.
/// </summary>
internal sealed class NoticeBoard : INoticesService
{
    /// <summary>Oldest notices are dropped beyond this.</summary>
    public const int MaxNotices = 20;

    private readonly object gate = new object();
    private readonly List<AppNotice> notices = new List<AppNotice>();
    private readonly IBridgeEvents events;
    private long lastId;
    private string? launchFailure;

    public NoticeBoard(IBridgeEvents events)
    {
        this.events = events;
    }

    public List<AppNotice> List()
    {
        lock (gate)
        {
            return notices.Select(Copy).ToList();
        }
    }

    public void Dismiss(DismissNoticeRequest request)
    {
        int count;

        lock (gate)
        {
            if (notices.RemoveAll(n => n.Id == request.Id) == 0)
            {
                return;
            }

            count = notices.Count;
        }

        events.Emit(new NoticesChanged { Count = count });
    }

    /// <summary>Adds a notice, unless the same one is already waiting (a handler reloaded twice). Any thread.</summary>
    public AppNotice Add(NoticeLevel level, NoticeSource source, string title, string message)
    {
        AppNotice notice;
        int count;

        lock (gate)
        {
            AppNotice? same = notices.FirstOrDefault(n => n.Level == level && n.Source == source && n.Title == title && n.Message == message);
            if (same != null)
            {
                return Copy(same);
            }

            notice = new AppNotice
            {
                Id = (++lastId).ToString(CultureInfo.InvariantCulture),
                Level = level,
                Source = source,
                Title = title,
                Message = message,
                At = DateTime.UtcNow,
            };
            notices.Add(notice);

            if (notices.Count > MaxNotices)
            {
                notices.RemoveRange(0, notices.Count - MaxNotices);
            }

            count = notices.Count;
        }

        string firstLine = message.Split('\n').FirstOrDefault()?.Trim() ?? "";
        ServiceLog.Warn($"notice ({source}, {level}): {title}: {firstLine}");
        events.Emit(new NoticesChanged { Count = count });
        return Copy(notice);
    }

    /// <summary>Keeps the crash report of a failed launch for the session snapshot. Any thread.</summary>
    public void SetLaunchFailure(string report)
    {
        lock (gate)
        {
            launchFailure = report;
        }
    }

    /// <summary>The crash report kept by <see cref="SetLaunchFailure"/>, once.</summary>
    public string? TakeLaunchFailure()
    {
        lock (gate)
        {
            string? report = launchFailure;
            launchFailure = null;
            return report;
        }
    }

    private static AppNotice Copy(AppNotice n) => new AppNotice { Id = n.Id, Level = n.Level, Source = n.Source, Title = n.Title, Message = n.Message, At = n.At };
}
