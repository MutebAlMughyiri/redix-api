namespace Redix.Api.Entries;

/// <summary>A file an entry carries, and what it is to that entry.</summary>
public class EntryAttachment : IProjectRow
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The file.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>What the file is to the entry.</summary>
    public EntryAttachmentRole Role { get; init; }

    /// <summary>
    /// <paramref name="files"/>, the oldest first. An entry's own file is its oldest: the one Add entry shows and its edit replaces, and the one Approvals opens; a file shared onto it
    /// later comes after (redix-app #115).
    /// </summary>
    public static IQueryable<EntryAttachment> OldestFirst(IQueryable<EntryAttachment> files, RedixDbContext db) =>
        from file in files
        join attachment in db.Attachments on file.AttachmentId equals attachment.Id
        orderby attachment.UploadedAt
        select file;
}
