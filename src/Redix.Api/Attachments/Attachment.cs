using System.Security.Cryptography;
using Microsoft.AspNetCore.StaticFiles;

namespace Redix.Api.Attachments;

/// <summary>A file kept for one project (FR-09): a picture or a PDF for an entry, a contract or a receipt, and any file for a document (UC-064). Its file lives outside the database, and it is never deleted (NFR-11). It owns which files can be kept.</summary>
public class Attachment : IProjectRow
{
    /// <summary>A picture must be smaller than this many bytes (NFR-06): 1 MB.</summary>
    public const int ImageLimitBytes = 1_000_000;

    /// <summary>A file kept as any kind (a document) must be at most this many bytes: 25 MB.</summary>
    public const int AnyFileLimitBytes = 25_000_000;

    /// <summary>The longest content type: a type and a subtype of 127 letters each (RFC 6838).</summary>
    public const int ContentTypeMaxLength = 255;

    /// <summary>The longest name a file keeps.</summary>
    public const int FileNameMaxLength = 255;

    private const string PdfType = "application/pdf";

    private static readonly FileExtensionContentTypeProvider TypesByExtension = new();

    private static readonly (byte[] Signature, string ContentType, string Extension)[] Kept =
    [
        ([0xFF, 0xD8, 0xFF], "image/jpeg", ".jpg"),
        ([0x89, 0x50, 0x4E, 0x47], "image/png", ".png"),
        ([0x25, 0x50, 0x44, 0x46], PdfType, ".pdf"),
    ];

    /// <summary>The attachment's id, time-ordered.</summary>
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    /// <summary>The project it belongs to: only people with access to that project read it (BR-33).</summary>
    public Guid ProjectId { get; private init; }

    /// <summary>Where its file is, under the folder the API keeps files in. Nothing a person typed is part of it.</summary>
    public string StoragePath { get; private init; } = "";

    /// <summary>The file's name, as it was sent.</summary>
    public string FileName { get; private init; } = "";

    /// <summary>What the file is: a picture or a PDF read from its first bytes rather than from what the sender says; any other file named by its extension, so the phone knows which app opens it.</summary>
    public string ContentType { get; private init; } = "";

    /// <summary>The file's size.</summary>
    public int SizeBytes { get; private init; }

    /// <summary>The file's SHA-256 in lowercase hexadecimal: what shows a kept file is unchanged (AC-N10), and that two files are the same.</summary>
    public string Sha256 { get; private init; } = "";

    /// <summary>The user who sent it.</summary>
    public Guid UploadedByUserId { get; private init; }

    /// <summary>When it was sent.</summary>
    public DateTime UploadedAt { get; private init; }

    /// <summary>Why <paramref name="content"/> cannot be kept as one of <paramref name="kinds"/>: any file over 25 MB; otherwise not a picture or a PDF, or a picture of 1 MB or more (NFR-06). Empty when it can.</summary>
    public static string RefusalOf(byte[] content, FileKinds kinds)
    {
        if (kinds == FileKinds.Any)
        {
            return content.Length > AnyFileLimitBytes ? "FileTooLarge" : "";
        }
        var contentType = KindOf(content).ContentType;
        if (contentType.Length == 0)
        {
            return "InvalidFileType";
        }
        return contentType != PdfType && content.Length >= ImageLimitBytes ? "ImageTooLarge" : "";
    }

    /// <summary>The attachment of <paramref name="content"/>, which passed <see cref="RefusalOf"/>, sent as <paramref name="fileName"/> by <paramref name="userId"/> at <paramref name="at"/>.</summary>
    public static Attachment Of(Guid projectId, byte[] content, string fileName, Guid userId, DateTime at)
    {
        var id = Guid.CreateVersion7();
        var name = Path.GetFileName(fileName.Trim());
        var kind = KindOf(content);
        return new Attachment
        {
            Id = id,
            ProjectId = projectId,
            StoragePath = $"{projectId}/{id}{kind.Extension}",
            FileName = name.Length > FileNameMaxLength ? name[^FileNameMaxLength..] : name,
            ContentType = kind.ContentType.Length > 0 ? kind.ContentType : TypeNamedBy(name),
            SizeBytes = content.Length,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(content)),
            UploadedByUserId = userId,
            UploadedAt = at,
        };
    }

    private static string TypeNamedBy(string fileName) => TypesByExtension.TryGetContentType(fileName, out var contentType) ? contentType : "application/octet-stream";

    private static (string ContentType, string Extension) KindOf(byte[] content)
    {
        foreach (var kind in Kept)
        {
            if (content.AsSpan().StartsWith(kind.Signature))
            {
                return (kind.ContentType, kind.Extension);
            }
        }
        return ("", "");
    }
}
