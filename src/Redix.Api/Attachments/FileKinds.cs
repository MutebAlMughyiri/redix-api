namespace Redix.Api.Attachments;

/// <summary>Which files an attachment may be.</summary>
public enum FileKinds
{
    /// <summary>A picture under 1 MB or a PDF: what an entry or a receipt keeps (FR-09, NFR-06).</summary>
    PicturesAndPdfs,

    /// <summary>Any file of at most 25 MB: a document, the project's (UC-064) or a contract's (findings row 138).</summary>
    Any,
}
