namespace Redix.Api.Attachments;

/// <summary>Which files an attachment may be.</summary>
public enum FileKinds
{
    /// <summary>A picture under 1 MB or a PDF: what an entry, a contract or a receipt keeps (FR-09, NFR-06).</summary>
    PicturesAndPdfs,

    /// <summary>Any file of at most 25 MB: a project's document (UC-064).</summary>
    Any,
}
