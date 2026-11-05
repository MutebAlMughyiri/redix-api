namespace Redix.Api.Entries;

/// <summary>A file an entry carries, and what it is to that entry.</summary>
public class EntryAttachment
{
    /// <summary>The entry.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The file.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>What the file is to the entry.</summary>
    public EntryAttachmentRole Role { get; init; }
}
