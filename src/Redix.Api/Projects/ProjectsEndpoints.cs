using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Redix.Api.History;
using Redix.Api.Auth;
using Redix.Api.Entries;
using Redix.Api.Partners;
using Redix.Api.Structure;

namespace Redix.Api.Projects;

/// <summary>Lists each user's projects (UC-008, FR-36), registers one for someone signed in (UC-066), renames them, adds, renames and deletes units and closes them (UC-006, FR-04, BR-38), and keeps each user's default (UC-010, FR-37).</summary>
public static class ProjectsEndpoints
{
    /// <summary>Maps <c>GET /projects</c>, <c>POST /projects</c>, and under <c>POST /projects/{projectId}</c> the rename, <c>/units</c>, <c>/units/{unitId}</c>, <c>/units/{unitId}/delete</c>, <c>/close</c> and <c>/default</c>, and <c>POST /projects/default/remove</c>.</summary>
    public static void MapProjects(this WebApplication app)
    {
        app.MapGet("/projects", List);
        app.MapPost("/projects", Create).RequireAuthorization(SessionPolicies.Working);
        app.MapPut("/projects/{projectId:guid}", Rename).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPut("/projects/{projectId:guid}/description", Describe).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/units", AddUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPut("/projects/{projectId:guid}/units/{unitId:guid}", RenameUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapDelete("/projects/{projectId:guid}/units/{unitId:guid}", DeleteUnit).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/close", Close).RequireAuthorization(nameof(Permission.ManageProject));
        app.MapPost("/projects/{projectId:guid}/default", SetDefault).RequireAuthorization(nameof(Permission.ReadProject));
        app.MapDelete("/projects/default", RemoveDefault).RequireAuthorization(SessionPolicies.Working);
    }

    /// <summary>
    /// The projects the caller may read, and those they registered that are not approved, active ones first and then the newest (AC-36.1), each with what they may do
    /// in it (AC-36.2). A finished project stays listed, with its reports (AC-04.1).
    /// </summary>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the projects live in.</param>
    public static async Task<List<ProjectView>> List(ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var access = await UserAccess.OfAsync(userId, db);
        var theirs = (await db.ProjectPermissions.Where(held => held.UserId == userId).Select(held => held.ProjectId).Distinct().ToListAsync()).ToHashSet();
        var roleNames = await ProjectRoleName.OfUserAsync(userId, db);
        var projects = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder))
            .OrderBy(project => project.Status).ThenByDescending(project => project.StartDate)
            .ToListAsync();
        return projects.Where(project => access.In(project.Id).Contains(Permission.ReadProject) || (!project.IsApproved && theirs.Contains(project.Id)))
            .Select(project => ProjectView.Of(project, access.In(project.Id), roleNames)).ToList();
    }

    /// <summary>
    /// Registers a project with its units, all or nothing, with the caller as its first project manager, waiting for the admin (BR-29, BR-34). An empty name, or one
    /// longer than <see cref="Project.NameMaxLength"/>, for the project or any unit, answers <c>invalidName</c> and saves nothing; a unit's name repeated, <c>unitNameTaken</c> (BR-37). An earlier project to copy the structure from must be
    /// one the caller reads (BR-33), or it answers <c>unknownSource</c>.
    /// </summary>
    /// <param name="request">The project's name, start date and units.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project is added to.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Create(CreateProjectRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var (project, outcome) = Project.New(request.Name, request.StartDate, request.Units, request.Description);
        if (outcome != ProjectChangeOutcome.Done)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, outcome.ToString());
        }
        var (partners, refusal) = PartnersEndpoints.DeclaredWith(project.Id, request.Partners);
        if (refusal.Length > 0)
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, refusal);
        }

        var userId = AccessTokens.UserIdOf(principal);
        if (request.StructureSourceId != Guid.Empty && !(await UserAccess.OfAsync(userId, db)).In(request.StructureSourceId).Contains(Permission.ReadProject))
        {
            return Refusal.Of(StatusCodes.Status400BadRequest, "UnknownSource");
        }

        project.StructureSourceId = request.StructureSourceId;
        AddWithManager(project, userId, db, time.GetUtcNow().UtcDateTime);
        db.Partners.AddRange(partners);
        await db.SaveChangesAsync();
        return Results.Created($"/projects/{project.Id}", await ViewOfAsync(project, userId, db));
    }

    /// <summary>
    /// Adds a registered <paramref name="project"/> with <paramref name="managerId"/> holding every permission of a project in it, of the two capital ticks seeing all (redix-api #106), and their
    /// history rows; the caller saves them together.
    /// </summary>
    public static void AddWithManager(Project project, Guid managerId, RedixDbContext db, DateTime now)
    {
        db.Projects.Add(project);
        db.AddHistory(project, HistoryAction.RegisterProject, managerId, now);
        foreach (var permission in ProjectPermission.OfWhoeverStarts)
        {
            var held = new ProjectPermission { ProjectId = project.Id, UserId = managerId, Permission = permission };
            db.ProjectPermissions.Add(held);
            db.AddHistory(held, HistoryAction.AssignProject, managerId, now);
        }
    }

    /// <summary>Adds a unit at the end of the project's list. A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>; a name another unit of the project holds, compared trimmed, <c>unitNameTaken</c> (BR-37).</summary>
    /// <param name="projectId">The project the unit belongs to.</param>
    /// <param name="request">The unit's name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> AddUnit(Guid projectId, AddUnitRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, HistoryAction.Create, project => project.AddUnit(request.Name.Trim()));

    /// <summary>Renames a project while it is open (BR-38). A finished project answers <c>projectFinished</c>; an unacceptable name, <c>invalidName</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Rename(Guid projectId, NameRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, HistoryAction.Update, project => project.Rename(request.Name.Trim()));

    /// <summary>Keeps the project's description while it is open, in the History table as a rename is (redix-api 93). Over <see cref="Project.DescriptionMaxLength"/> characters it answers <c>invalidDescription</c>, and once finished <c>projectFinished</c>.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="request">Its description; empty for none.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> Describe(Guid projectId, DescriptionRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, HistoryAction.Update, project => project.Describe(request.Description.Trim()));

    /// <summary>Renames a unit while its project is open (BR-38). It answers as <see cref="AddUnit"/> does, and one outside the project 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="unitId">The unit.</param>
    /// <param name="request">Its new name.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> RenameUnit(Guid projectId, Guid unitId, NameRequest request, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, HistoryAction.Update, project => project.RenameUnit(unitId, request.Name.Trim()));

    /// <summary>Deletes a unit while its project is open (BR-38). A finished project answers <c>projectFinished</c>; one outside the project 404.</summary>
    /// <param name="projectId">The project.</param>
    /// <param name="unitId">The unit.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static Task<IResult> DeleteUnit(Guid projectId, Guid unitId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time) =>
        ChangeAsync(projectId, principal, db, time, HistoryAction.Delete, project => project.RemoveUnit(unitId));

    /// <summary>
    /// Closes a finished project: from then on it is read-only, and its reports stay available (AC-04.1). A finished project answers <c>projectFinished</c>; one with entries waiting for approval, <c>pendingEntries</c> (BR-35).
    /// A draft waits for nobody, so it holds nothing open.
    /// </summary>
    /// <param name="projectId">The project to close.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the project lives in.</param>
    /// <param name="time">The current time, for the history row.</param>
    public static async Task<IResult> Close(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time)
    {
        var hasEntriesWaiting = await db.Entries.AnyAsync(entry => entry.ProjectId == projectId && entry.Status == EntryStatus.PendingApproval);
        return await ChangeAsync(projectId, principal, db, time, HistoryAction.CloseProject, project => project.Close(hasEntriesWaiting));
    }

    /// <summary>
    /// Makes a project the caller's default, replacing any other (FR-37). From the next sign-in it opens directly, skipping the list (AC-37.1): a default always opens directly
    /// (Muteb, 8 Nov 2026).
    /// </summary>
    /// <param name="projectId">The project, which the caller must be able to read.</param>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the choice lives in.</param>
    public static async Task<DefaultProjectView> SetDefault(Guid projectId, ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        var chosen = await db.UserDefaultProjects.SingleOrDefaultAsync(row => row.UserId == userId);
        if (chosen is null)
        {
            db.UserDefaultProjects.Add(new UserDefaultProject { UserId = userId, ProjectId = projectId });
        }
        else
        {
            chosen.ProjectId = projectId;
        }
        await db.SaveChangesAsync();
        return new DefaultProjectView(projectId.ToString());
    }

    /// <summary>Clears the caller's default, so sign-in shows the list.</summary>
    /// <param name="principal">The caller's session.</param>
    /// <param name="db">The database the choice lives in.</param>
    public static async Task<DefaultProjectView> RemoveDefault(ClaimsPrincipal principal, RedixDbContext db)
    {
        var userId = AccessTokens.UserIdOf(principal);
        await db.UserDefaultProjects.Where(row => row.UserId == userId).ExecuteDeleteAsync();
        return new DefaultProjectView("");
    }

    private static async Task<IResult> ChangeAsync(Guid projectId, ClaimsPrincipal principal, RedixDbContext db, TimeProvider time, HistoryAction action, Func<Project, ProjectChangeOutcome> change)
    {
        var project = await db.Projects.Include(project => project.Units.OrderBy(unit => unit.SortOrder)).SingleOrDefaultAsync(project => project.Id == projectId);
        if (project is null)
        {
            return Results.NotFound();
        }

        var outcome = change(project);
        if (outcome == ProjectChangeOutcome.UnknownUnit)
        {
            return Results.NotFound();
        }
        if (outcome != ProjectChangeOutcome.Done)
        {
            var status = outcome is ProjectChangeOutcome.InvalidName or ProjectChangeOutcome.InvalidDescription ? StatusCodes.Status400BadRequest : StatusCodes.Status409Conflict;
            return Refusal.Of(status, outcome.ToString());
        }
        var userId = AccessTokens.UserIdOf(principal);
        db.AddHistoryOfChanges(action, userId, time.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync();
        return Results.Ok(await ViewOfAsync(project, userId, db));
    }

    /// <summary>The view of <paramref name="project"/> for <paramref name="userId"/>.</summary>
    public static async Task<ProjectView> ViewOfAsync(Project project, Guid userId, RedixDbContext db)
    {
        var access = await UserAccess.OfAsync(userId, db);
        var roleNames = await ProjectRoleName.OfUserAsync(userId, db);
        return ProjectView.Of(project, access.In(project.Id), roleNames);
    }
}

/// <summary>What the app sends to create a project.</summary>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day the project started.</param>
/// <param name="Units">The names of the project's units, in order; may be empty.</param>
public record CreateProjectRequest(string Name, DateOnly StartDate, List<string> Units)
{
    /// <summary>The earlier project to copy the structure from when it is approved; left out for the default template.</summary>
    public Guid StructureSourceId { get; init; }

    /// <summary>What the project is; left out for none.</summary>
    public string Description { get; init; } = "";

    /// <summary>The names of its partners, in order, each a table of its رأس المال once approved (redix-api #109); left out for none.</summary>
    public List<string> Partners { get; init; } = [];
}

/// <summary>What the app sends to keep a project's description.</summary>
/// <param name="Description">The description; empty for none.</param>
public record DescriptionRequest(string Description);

/// <summary>The caller's default project, as a session says it.</summary>
/// <param name="DefaultProjectId">The project sign-in opens directly; empty when none.</param>
public record DefaultProjectView(string DefaultProjectId);

/// <summary>What the app sends to add a unit.</summary>
/// <param name="Name">The unit's name.</param>
public record AddUnitRequest(string Name);

/// <summary>A project as the app shows it.</summary>
/// <param name="Id">The project's id.</param>
/// <param name="Name">The project's name.</param>
/// <param name="StartDate">The day the project started.</param>
/// <param name="Status"><c>pendingApproval</c>, <c>rejected</c>, <c>active</c>, or <c>finished</c> once read-only.</param>
/// <param name="RejectionReason">Why the admin refused it; empty otherwise.</param>
/// <param name="Description">What the project is; empty for none.</param>
/// <param name="Units">The project's units, in order.</param>
/// <param name="Permissions">What the caller may do in this project; the app shows what they allow.</param>
/// <param name="RoleName">What the caller is called in this project, shown under its name (Muteb, 9 Nov 2026); empty when nothing was typed.</param>
public record ProjectView(Guid Id, string Name, DateOnly StartDate, ProjectStatus Status, string RejectionReason, string Description, List<UnitView> Units, List<Permission> Permissions, string RoleName)
{
    /// <summary>The view of <paramref name="project"/>, with its units in order, and the caller's <paramref name="permissions"/> in it and role name among their <paramref name="roleNames"/>.</summary>
    public static ProjectView Of(Project project, List<Permission> permissions, Dictionary<Guid, string> roleNames) =>
        new(project.Id, project.Name, project.StartDate, project.Status, project.RejectionReason, project.Description, project.Units.Select(UnitView.Of).ToList(), permissions, roleNames.GetValueOrDefault(project.Id, ""));
}

/// <summary>A unit as the app shows it.</summary>
/// <param name="Id">The unit's id.</param>
/// <param name="Name">The unit's name.</param>
public record UnitView(Guid Id, string Name)
{
    /// <summary>The view of <paramref name="unit"/>.</summary>
    public static UnitView Of(Unit unit) => new(unit.Id, unit.Name);
}
