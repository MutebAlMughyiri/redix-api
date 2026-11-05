using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.Audit;
using Redix.Api.Auth;

namespace Redix.Api.Projects;

/// <summary>Engineering blueprints (UC-064): the manager keeps them for the project or a unit, and everyone who reads the project downloads them through the attachment store.</summary>
public static class BlueprintsEndpoints
{
    /// <summary>Maps the list, for anyone who reads the project, and the changes, for whoever holds <see cref="Permission.ManageProject"/>.</summary>
    /// <param name="app">The app.</param>
    public static void MapBlueprints(this WebApplication app)
    {
        app.MapGet("/projects/{projectId:guid}/blueprints", List).RequireAuthorization(nameof(Permission.ReadProject));
        var changes = app.MapProjectChanges(Permission.ManageProject);
        changes.MapPost("/blueprints", Add);
        changes.MapPost("/blueprints/{blueprintId:guid}/delete", Remove);
    }

    /// <summary>The project's blueprints: the whole project's first, then each unit's in the units' order, newest first within each.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="db">The database they live in.</param>
    public static async Task<List<BlueprintView>> List(Guid projectId, RedixDbContext db)
    {
        var units = await db.Set<Unit>().Where(unit => unit.ProjectId == projectId).ToDictionaryAsync(unit => unit.Id);
        var rows = await (
            from blueprint in db.Blueprints
            join attachment in db.Attachments on blueprint.AttachmentId equals attachment.Id
            where blueprint.ProjectId == projectId
            select new { blueprint, attachment }).ToListAsync();
        return rows
            .OrderBy(row => row.blueprint.UnitId == Guid.Empty ? -1 : units[row.blueprint.UnitId].SortOrder)
            .ThenByDescending(row => row.blueprint.UploadedAt)
            .Select(row => new BlueprintView(row.blueprint.Id, row.blueprint.Title, row.blueprint.UnitId, row.blueprint.UnitId == Guid.Empty ? "" : units[row.blueprint.UnitId].Name, row.attachment.Id, row.attachment.FileName, row.attachment.ContentType, row.attachment.SizeBytes, row.blueprint.UploadedAt))
            .ToList();
    }

    /// <summary>
    /// Keeps a file already in the project's attachment store as a blueprint of the project, or of one of its own units. A title empty or longer than 200 letters answers <c>invalidTitle</c>; a file not
    /// in the project <c>unknownAttachment</c>; a unit not one of the project's own <c>unknownUnit</c>; a file already kept as a blueprint <c>blueprintAlreadyKept</c>.
    /// </summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">The file, the unit and the title.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Add(Guid projectId, BlueprintRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var title = request.Title.Trim();
        if (title.Length is 0 or > Blueprint.TitleMaxLength)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "InvalidTitle");
        }
        if (!await db.Attachments.AnyAsync(row => row.Id == request.AttachmentId && row.ProjectId == projectId))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownAttachment");
        }
        if (request.UnitId != Guid.Empty && !await db.Set<Unit>().AnyAsync(unit => unit.Id == request.UnitId && unit.ProjectId == projectId && !unit.IsShared))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownUnit");
        }
        if (await db.Blueprints.AnyAsync(row => row.ProjectId == projectId && row.AttachmentId == request.AttachmentId))
        {
            return Refusal.Of(StatusCodes.Status409Conflict, "BlueprintAlreadyKept");
        }

        var userId = AccessTokens.UserIdOf(principal);
        var at = time.GetUtcNow().UtcDateTime;
        var blueprint = new Blueprint { ProjectId = projectId, UnitId = request.UnitId, AttachmentId = request.AttachmentId, Title = title, UploadedByUserId = userId, UploadedAt = at };
        db.Blueprints.Add(blueprint);
        db.Audit(blueprint, AuditAction.Create, userId, at);
        await db.SaveChangesAsync();
        return Results.Ok(new BlueprintIdView(blueprint.Id));
    }

    /// <summary>Takes a blueprint off the project's list. Its file stays in the attachment store, which never deletes (NFR-11). One outside the project is not found.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="blueprintId">The blueprint.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database they live in.</param>
    /// <param name="time">The current time.</param>
    public static async Task<IResult> Remove(Guid projectId, Guid blueprintId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var blueprint = await db.Blueprints.SingleOrDefaultAsync(row => row.Id == blueprintId && row.ProjectId == projectId);
        if (blueprint is null)
        {
            return Results.NotFound();
        }
        db.Audit(blueprint, AuditAction.Delete, AccessTokens.UserIdOf(principal), time.GetUtcNow().UtcDateTime);
        db.Blueprints.Remove(blueprint);
        await db.SaveChangesAsync();
        return Results.Ok(new BlueprintIdView(blueprint.Id));
    }
}

/// <summary>A blueprint to keep.</summary>
/// <param name="AttachmentId">Its file, already in the project's attachment store.</param>
/// <param name="Title">What it shows.</param>
public record BlueprintRequest(Guid AttachmentId, string Title)
{
    /// <summary>The unit it is for; left out for the whole project.</summary>
    public Guid UnitId { get; init; }
}

/// <summary>A blueprint as the app lists it.</summary>
/// <param name="Id">The blueprint.</param>
/// <param name="Title">What it shows.</param>
/// <param name="UnitId">The unit it is for; empty for the whole project.</param>
/// <param name="UnitName">That unit's name; empty for the whole project.</param>
/// <param name="AttachmentId">Its file.</param>
/// <param name="FileName">The file's name.</param>
/// <param name="ContentType">What the file is.</param>
/// <param name="SizeBytes">The file's size.</param>
/// <param name="UploadedAt">When it was kept.</param>
public record BlueprintView(Guid Id, string Title, Guid UnitId, string UnitName, Guid AttachmentId, string FileName, string ContentType, int SizeBytes, DateTime UploadedAt);

/// <summary>The blueprint a change was made to.</summary>
/// <param name="Id">The blueprint.</param>
public record BlueprintIdView(Guid Id);
