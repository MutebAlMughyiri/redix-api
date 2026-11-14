namespace Redix.Api.Documents;

/// <summary>A document kept for a project (UC-064): any file from the attachment store, under a name, and in a group when the manager gives one.</summary>
public class Document : IProjectRow
{
    /// <summary>The longest name.</summary>
    public const int NameMaxLength = 200;

    /// <summary>The longest group.</summary>
    public const int GroupMaxLength = 100;

    /// <summary>The document's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>Its file, in the attachment store.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>What it is, as the manager named it.</summary>
    public required string Name { get; init; }

    /// <summary>The group it is listed under, as the manager typed it; empty when it is in none. It only groups the list and means nothing else.</summary>
    public string Group { get; init; } = "";

    /// <summary>Who kept it.</summary>
    public Guid UploadedByUserId { get; init; }

    /// <summary>When.</summary>
    public DateTime UploadedAt { get; init; }
}
