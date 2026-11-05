namespace Redix.Api.Projects;

/// <summary>An engineering blueprint kept for a project or one of its units (UC-064): a PDF or a picture from the attachment store, under a title.</summary>
public class Blueprint
{
    /// <summary>The longest title.</summary>
    public const int TitleMaxLength = 200;

    /// <summary>The blueprint's id, time-ordered.</summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>The project.</summary>
    public Guid ProjectId { get; init; }

    /// <summary>The unit it is for; empty when it is for the whole project.</summary>
    public Guid UnitId { get; init; }

    /// <summary>Its file, in the attachment store.</summary>
    public Guid AttachmentId { get; init; }

    /// <summary>What it shows, as the manager named it.</summary>
    public required string Title { get; init; }

    /// <summary>Who kept it.</summary>
    public Guid UploadedByUserId { get; init; }

    /// <summary>When.</summary>
    public DateTime UploadedAt { get; init; }
}
