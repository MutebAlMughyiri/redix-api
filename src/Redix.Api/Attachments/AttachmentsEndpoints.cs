using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Attachments;

/// <summary>Keeps a project's pictures and PDFs, and serves them only to people with access to that project (UC-021, FR-09). There is no delete (NFR-11).</summary>
public static class AttachmentsEndpoints
{
    /// <summary>Maps the route that reads a file, for anyone who reads the project, and the one that keeps it, for whoever holds <see cref="Permission.AttachFiles"/>.</summary>
    public static void MapAttachments(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/attachments/{attachmentId:guid}", Read).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapProjectChanges(Permission.AttachFiles).MapPost("/attachments", Upload).DisableAntiforgery();
    }

    /// <summary>The file of an attachment of the project; one of another project is not found (BR-33).</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="attachmentId">The attachment.</param>
    /// <param name="db">The database that knows where it is.</param>
    /// <param name="options">The folder files are kept under.</param>
    public static async Task<IResult> Read(Guid projectId, Guid attachmentId, RedixDbContext db, IOptions<AttachmentOptions> options)
    {
        var attachment = await db.Attachments.SingleOrDefaultAsync(row => row.Id == attachmentId && row.ProjectId == projectId);
        return attachment is null ? Results.NotFound() : Results.File(options.Value.PathOf(attachment), attachment.ContentType, attachment.FileName);
    }

    /// <summary>Keeps a file, and answers it, for an entry or a contract to name. The same file from the same person answers the one already kept, so a retried upload keeps it once (UC-029). A file that is not a picture or a PDF answers <c>invalidFileType</c>; a picture of 1 MB or more, <c>imageTooLarge</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="file">The file.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database that records it.</param>
    /// <param name="options">The folder files are kept under.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Upload(Guid projectId, IFormFile file, ClaimsPrincipal principal, RedixDbContext db, IOptions<AttachmentOptions> options, TimeProvider time)
    {
        var (refusal, attachment, isNew) = await KeepAsync(projectId, file, AccessTokens.UserIdOf(principal), db, options.Value, time);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }
        await db.SaveChangesAsync();
        return isNew ? Results.Created($"/projects/{projectId}/attachments/{attachment.Id}", AttachmentView.Of(attachment)) : Results.Ok(AttachmentView.Of(attachment));
    }

    /// <summary>
    /// Keeps <paramref name="file"/> for the project, its file written and its record added with its audit, saved with the change; the same file from the same person answers the one already kept.
    /// Answers why it cannot be kept, as <see cref="Attachment.RefusalOf"/> does, with no attachment; otherwise the attachment and whether it is new.
    /// </summary>
    public static async Task<(string Refusal, Attachment Attachment, bool IsNew)> KeepAsync(Guid projectId, IFormFile file, Guid userId, RedixDbContext db, AttachmentOptions options, TimeProvider time)
    {
        using var read = new MemoryStream();
        await file.CopyToAsync(read);
        var content = read.ToArray();
        var refusal = Attachment.RefusalOf(content);
        if (refusal.Length > 0)
        {
            return (refusal, Attachment.Of(projectId, [], "", userId, default), false);
        }

        var at = time.GetUtcNow().UtcDateTime;
        var attachment = Attachment.Of(projectId, content, file.FileName, userId, at);
        var sent = await db.Attachments.FirstOrDefaultAsync(row => row.ProjectId == projectId && row.UploadedByUserId == userId && row.Sha256 == attachment.Sha256);
        if (sent is not null)
        {
            return ("", sent, false);
        }

        var path = options.PathOf(attachment);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content);
        db.Attachments.Add(attachment);
        db.Audit(attachment, AuditAction.Attach, userId, at);
        return ("", attachment, true);
    }
}

/// <summary>An attachment as the app shows it.</summary>
/// <param name="Id">The attachment's id.</param>
/// <param name="FileName">The file's name.</param>
/// <param name="ContentType">What the file is.</param>
/// <param name="SizeBytes">The file's size.</param>
public record AttachmentView(Guid Id, string FileName, string ContentType, int SizeBytes)
{
    /// <summary>The view of <paramref name="attachment"/>.</summary>
    public static AttachmentView Of(Attachment attachment) => new(attachment.Id, attachment.FileName, attachment.ContentType, attachment.SizeBytes);
}
