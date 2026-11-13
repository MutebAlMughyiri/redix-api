namespace Redix.Api.Notifications;

/// <summary>The entry tapping a notification opens (findings row 214). A notification has one at most, and having no row is being about the project itself.</summary>
public class NotificationEntry
{
    /// <summary>The notification.</summary>
    public Guid NotificationId { get; init; }

    /// <summary>The entry it opens.</summary>
    public Guid EntryId { get; init; }
}
