using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Redix.Api.Attachments;
using Redix.Api.Audit;
using Redix.Api.Auth;
using Redix.Api.Projects;

namespace Redix.Api.Documents;

/// <summary>A project's documents (UC-064): the manager keeps any file under a name, in a group when he gives one, and everyone who reads the project opens them through the attachment store.</summary>
public static class DocumentsEndpoints
{
    /// <summary>Maps the list, for anyone who reads the project, and the changes, for whoever holds <see cref="Permission.ManageProject"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapDocuments(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/documents", List).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.ManageProject);
        changes.MapPost("/documents", Add).DisableAntiforgery();
        changes.MapPost("/documents/{documentId:guid}/delete", Remove);
    }

    /// <summary>The project's documents: those in no group first, then each group in order, newest first within each.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static Task<List<DocumentView>> List(Guid projectId, RedixDbContext db) => (
        from document in db.Documents
        join attachment in db.Attachments on document.AttachmentId equals attachment.Id
        where document.ProjectId == projectId
        orderby document.Group, document.UploadedAt descending
        select new DocumentView(document.Id, document.Name, document.Group, attachment.Id, attachment.FileName, attachment.ContentType, attachment.SizeBytes, document.UploadedAt)).ToListAsync();

    /// <summary>
    /// Keeps <paramref name="file"/>, of any kind, as a document of the project under its name and group. A name empty or longer than 200 letters answers <c>invalidName</c>; a group longer than
    /// 100 letters <c>invalidGroup</c>; a file over 25 MB <c>fileTooLarge</c>; a file already kept as a document <c>documentAlreadyKept</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="file">The document's file.</param>
    /// <param name="fields">Its name and group.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="options">The folder files are kept under.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Add(Guid projectId, IFormFile file, [FromForm] DocumentFields fields, ClaimsPrincipal principal, RedixDbContext db, IOptions<AttachmentOptions> options, TimeProvider time)
    {
        var name = fields.Name.Trim();
        var group = fields.Group.Trim();
        if (name.Length is 0 or > Document.NameMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidName");
        }

        if (group.Length > Document.GroupMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidGroup");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var (refusal, attachment, _) = await AttachmentsEndpoints.KeepAsync(projectId, file, FileKinds.Any, userId, db, options.Value, time);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        if (await db.Documents.AnyAsync(row => row.ProjectId == projectId && row.AttachmentId == attachment.Id))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "DocumentAlreadyKept");
        }

        var at = time.GetUtcNow().UtcDateTime;
        var document = new Document { ProjectId = projectId, AttachmentId = attachment.Id, Name = name, Group = group, UploadedByUserId = userId, UploadedAt = at };
        db.Documents.Add(document);
        db.Audit(document, AuditAction.Create, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok(new DocumentIdView(document.Id));
    }

    /// <summary>Takes a document off the project's list. Its file stays in the attachment store, which never deletes (NFR-11). One outside the project is not found.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="documentId">The document.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Remove(Guid projectId, Guid documentId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var document = await db.Documents.SingleOrDefaultAsync(row => row.Id == documentId && row.ProjectId == projectId);
        if (document is null)
        {
            return Results.NotFound();
        }

        db.Audit(document, AuditAction.Delete, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        db.Documents.Remove(document);
        await db.SaveChangesAsync();
        return Results.Ok(new DocumentIdView(document.Id));
    }
}

/// <summary>A document's name and group, sent beside its file.</summary>
public class DocumentFields
{
    /// <summary>What it is.</summary>
    public string Name { get; init; } = "";

    /// <summary>The group it is listed under; empty for none.</summary>
    public string Group { get; init; } = "";
}

/// <summary>A document as the app lists it.</summary>
/// <param name="Id">The document.</param>
/// <param name="Name">What it is.</param>
/// <param name="Group">The group it is listed under; empty for none.</param>
/// <param name="AttachmentId">Its file.</param>
/// <param name="FileName">The file's name.</param>
/// <param name="ContentType">What the file is.</param>
/// <param name="SizeBytes">The file's size.</param>
/// <param name="UploadedAt">When it was kept.</param>
public record DocumentView(Guid Id, string Name, string Group, Guid AttachmentId, string FileName, string ContentType, int SizeBytes, DateTime UploadedAt);

/// <summary>The document a change was made to.</summary>
/// <param name="Id">The document.</param>
public record DocumentIdView(Guid Id);
