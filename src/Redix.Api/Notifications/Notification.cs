namespace Redix.Api.Notifications;

/// <summary>What happened that someone needs to know (UC-036, FR-20).</summary>
public enum NotificationKind : byte
{
    /// <summary>An entry was sent, and waits for a decision.</summary>
    EntrySubmitted,

    /// <summary>The author's entry was approved.</summary>
    EntryApproved,

    /// <summary>The author's entry was rejected.</summary>
    EntryRejected,

    /// <summary>The admin let the manager's project in.</summary>
    ProjectApproved,

    /// <summary>The admin refused the manager's project.</summary>
    ProjectRejected,
}

/// <summary>One thing one person was told. It carries a kind, not text, so the words come from the app in that person's language.</summary>
public class Notification
{
    /// <summary>The notification's id.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>Who it is for.</summary>
    public Guid UserId { get; init; }

    /// <summary>The project it is about.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The entry tapping it opens; empty when it is about the project itself.</summary>
    public Guid EntryId { get; init; }

    /// <summary>What happened.</summary>
    public NotificationKind Kind { get; init; }

    /// <summary>When.</summary>
    public DateTime At { get; init; }

    /// <summary>True once they have seen it.</summary>
    public bool IsRead { get; set; }
}
